Imports System.Data.Odbc
Imports System.IO
Imports System.IO.Ports
Imports System.Management
Imports System.Text

Public Class RFIDScanMonitor
    Private ReadOnly _logger As ILogger = LoggerFactory.Instance
    Private ReadOnly _comPortManager As ComPortManager = ComPortManager.Instance
    Private deviceID As Integer = 0
    Private WithEvents resetTimer As New Timer()
    Private WithEvents clockTimer As New Timer()
    Private WithEvents connectionMonitorTimer As New Timer()
    Private Const FORM_NAME As String = "RFIDScanMonitor"

    ' Track last scan time for each RFID tag to prevent duplicate scans
    Private lastScanTimes As New Dictionary(Of String, DateTime)
    Private ReadOnly scanCooldownSeconds As Integer = 60 ' 1 minute cooldown

    Private Sub RFIDScanMonitor_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _logger.LogInfo("RFIDScanMonitor loading...")

        ' Apply blue tint to school background image
        ApplyBlueTintToSchoolImage()

        ' Make picRFIDIcon truly transparent by setting its parent to pbSchool
        picRFIDIcon.Parent = pbSchool
        picRFIDIcon.BackColor = Color.Transparent

        ' Also make lblWaitingMessage transparent
        lblWaitingMessage.Parent = pbSchool
        lblWaitingMessage.BackColor = Color.Transparent

        ' Setup reset timer (to show RFID icon again after showing card)
        resetTimer.Interval = 3000 ' 3 seconds
        resetTimer.Enabled = False

        ' Setup clock timer
        clockTimer.Interval = 1000 ' 1 second
        clockTimer.Enabled = True
        UpdateClock()

        ' Setup connection monitor timer (check every 5 seconds)
        connectionMonitorTimer.Interval = 5000 ' 5 seconds
        connectionMonitorTimer.Enabled = True

        ' Subscribe to COM Port Manager events
        AddHandler _comPortManager.PortAccessRequested, AddressOf OnPortAccessRequested
        AddHandler _comPortManager.PortAccessGranted, AddressOf OnPortAccessGranted
        AddHandler _comPortManager.PortReleased, AddressOf OnPortReleased
        AddHandler _comPortManager.PortDisconnected, AddressOf OnPortDisconnected
        AddHandler _comPortManager.DataReceived, AddressOf OnDataReceived

        ' Request access to COM port
        If _comPortManager.RequestAccess(FORM_NAME, autoConnect:=True) Then
            ShowWaitingState()
        Else
            UpdateWaitingMessageForError("FAILED TO CONNECT")
        End If

        _logger.LogInfo("RFIDScanMonitor loaded successfully")
    End Sub

    ''' <summary>
    ''' Called when another form requests access to the COM port
    ''' </summary>
    Private Sub OnPortAccessRequested(currentOwner As String, requester As String)
        If currentOwner = FORM_NAME Then
            _logger.LogInfo($"Port access requested by [{requester}], temporarily releasing...")
            UpdateWaitingMessageForError("PORT IN USE BY ANOTHER FORM")
            
            ' Stop connection monitor during temporary release
            If connectionMonitorTimer IsNot Nothing Then
                connectionMonitorTimer.Stop()
            End If
        End If
    End Sub

    ''' <summary>
    ''' Called when port access is granted
    ''' </summary>
    Private Sub OnPortAccessGranted(owner As String, portName As String)
        If owner = FORM_NAME Then
            _logger.LogInfo($"✓ Port access granted: {portName}")
            Me.Invoke(Sub() ShowWaitingState())
        End If
    End Sub

    ''' <summary>
    ''' Called when port is released by another form
    ''' </summary>
    Private Sub OnPortReleased(owner As String)
        ' If we're not the current owner, try to reclaim access
        If owner <> FORM_NAME AndAlso _comPortManager.CurrentOwner <> FORM_NAME Then
            _logger.LogInfo($"Port released by [{owner}], reclaiming access...")
            
            ' Restart connection monitor
            If connectionMonitorTimer IsNot Nothing Then
                connectionMonitorTimer.Start()
            End If
            
            ' Request access again
            If _comPortManager.RequestAccess(FORM_NAME, autoConnect:=False) Then
                Me.Invoke(Sub() ShowWaitingState())
            End If
        End If
    End Sub

    ''' <summary>
    ''' Called when port is disconnected
    ''' </summary>
    Private Sub OnPortDisconnected(portName As String)
        _logger.LogWarning($"Port {portName} disconnected")
        Me.Invoke(Sub() UpdateWaitingMessageForError("RECEIVER DISCONNECTED"))
    End Sub

    ''' <summary>
    ''' Called when RFID data is received from COM port
    ''' </summary>
    Private Sub OnDataReceived(tagData As String)
        _logger.LogInfo($"OnDataReceived called with tag: '{tagData}', CurrentOwner: '{_comPortManager.CurrentOwner}', FORM_NAME: '{FORM_NAME}'")
        
        ' Only process if we're the current owner
        If _comPortManager.CurrentOwner = FORM_NAME Then
            _logger.LogInfo("We are the owner, invoking ProcessRFIDTag...")
            Me.Invoke(Sub() ProcessRFIDTag(tagData))
        Else
            _logger.LogWarning($"Not the owner, skipping. Owner is: '{_comPortManager.CurrentOwner}'")
        End If
    End Sub

    Private Sub ShowWaitingState()
        ' Hide attendance card panel
        pnlCardContainer.Visible = False
        pnlCardContainer.Controls.Clear()

        ' Show RFID waiting image
        picRFIDIcon.Visible = True
        lblWaitingMessage.Visible = True
        lblWaitingMessage.Text = "TAP YOUR CARD"
        lblWaitingMessage.ForeColor = Color.Black

        _logger.LogDebug("Showing waiting state - ready for RFID tap")
    End Sub

    Private Sub UpdateWaitingMessageForError(message As String)
        ' Update waiting message to show error
        lblWaitingMessage.Visible = True
        lblWaitingMessage.Text = message
        lblWaitingMessage.ForeColor = Color.FromArgb(231, 76, 60) ' Red color for errors

        _logger.LogInfo($"Waiting message updated to error state: {message}")
    End Sub

    Private Sub ProcessRFIDTag(tagID As String)
        Try
            _logger.LogInfo($"========== Processing RFID tag: {tagID} ==========")

            ' Check if this tag was scanned recently (within cooldown period)
            If lastScanTimes.ContainsKey(tagID) Then
                Dim timeSinceLastScan As TimeSpan = DateTime.Now - lastScanTimes(tagID)
                If timeSinceLastScan.TotalSeconds < scanCooldownSeconds Then
                    Dim remainingSeconds As Integer = CInt(scanCooldownSeconds - timeSinceLastScan.TotalSeconds)
                    _logger.LogInfo($"Tag {tagID} scanned too soon. Please wait {remainingSeconds} seconds.")
                    ShowErrorMessage($"PLEASE WAIT {remainingSeconds} SECONDS")
                    Return
                End If
            End If

            ' Update last scan time for this tag
            lastScanTimes(tagID) = DateTime.Now

            ' Show scanning animation
            ShowScanningAnimation()

            ' Query database for teacher information (same query as FormAttendanceScanner)
            connectDB()
            _logger.LogInfo("Database connected")

            Dim cmd As New OdbcCommand("SELECT tagID, UCase(CONCAT(lastname, ', ', LEFT(firstname, 1), '.', IF(middlename IS NOT NULL AND middlename != '', CONCAT(' ', LEFT(middlename, 1), '.'), ''))) AS teacherName, profileImg, employeeID, teacherID FROM teacherinformation WHERE tagID = ?", con)
            cmd.Parameters.AddWithValue("?", tagID)
            Dim reader = cmd.ExecuteReader()

            If reader.Read() Then
                Dim teacherName As String = reader("teacherName").ToString()
                Dim employeeID As String = reader("employeeID").ToString()
                Dim teacherID As Integer = reader("teacherID")
                Dim profileImg As Byte() = Nothing

                _logger.LogInfo($"✓ Teacher found: {teacherName}, Employee ID: {employeeID}")

                ' Get profile image if available
                If Not IsDBNull(reader("profileImg")) Then
                    profileImg = CType(reader("profileImg"), Byte())
                    _logger.LogInfo($"Profile image size: {profileImg.Length} bytes")
                Else
                    _logger.LogInfo("No profile image in database")
                End If

                reader.Close()

                ' Record attendance
                _logger.LogInfo("Recording attendance...")
                Dim attendanceType As String = RecordAttendance(tagID, teacherID)
                _logger.LogInfo($"Attendance type: {attendanceType}")

                ' Refresh FormAttendance if it's open
                If FormAttendace.CurrentInstance IsNot Nothing Then
                    FormAttendace.CurrentInstance.RefreshAttendanceData()
                    _logger.LogInfo("Triggered real-time refresh of FormAttendance")
                End If

                ' Show attendance card
                _logger.LogInfo("Calling ShowAttendanceCard...")
                ShowAttendanceCard(teacherName, employeeID, attendanceType, profileImg)

                ' Start timer to reset to waiting state
                resetTimer.Stop()
                resetTimer.Start()
                _logger.LogInfo("Reset timer started (3 seconds)")
            Else
                reader.Close()
                _logger.LogWarning($"❌ RFID tag not found in database: {tagID}")
                ShowErrorMessage("RFID not registered")
            End If

            con.Close()
            _logger.LogInfo("========== RFID processing complete ==========")
        Catch ex As Exception
            _logger.LogError($"❌ Error processing RFID: {ex.Message}")
            _logger.LogError($"Stack trace: {ex.StackTrace}")
            Try
                If con IsNot Nothing AndAlso con.State = ConnectionState.Open Then
                    con.Close()
                End If
            Catch
            End Try
        End Try
    End Sub

    Private Function RecordAttendance(tagID As String, teacherID As Integer) As String
        ' Check if already has time in today, if yes then time out, else time in
        Dim cmd As New OdbcCommand("SELECT ar.tag_id FROM attendance_record ar WHERE ar.tag_id = ? AND departureTime IS NULL AND depState = 0", con)
        cmd.Parameters.AddWithValue("?", tagID)
        Dim myreader As OdbcDataReader = cmd.ExecuteReader()

        If myreader.HasRows Then
            myreader.Close()
            ' Update the departure time
            _logger.LogInfo($"Teacher already checked in, recording Time Out for tag: {tagID}")
            cmd = New OdbcCommand("UPDATE attendance_record SET departureTime = ?, depStatus = 'Successful', depState = 1 WHERE tag_id = ? AND arrivalTime IS NOT NULL AND depState = 0", con)
            Dim departureTime As DateTime = DateTime.Now
            cmd.Parameters.AddWithValue("?", departureTime)
            cmd.Parameters.AddWithValue("?", tagID)
            cmd.ExecuteNonQuery()
            Return "Time Out"
        Else
            myreader.Close()
            ' Insert new attendance record for teacher
            _logger.LogInfo($"Recording Time In for tag: {tagID}")
            cmd = New OdbcCommand("INSERT INTO attendance_record(tag_id, teacherID, logDate, arrivalTime, arrStatus) VALUES(?, ?, ?, ?, ?)", con)
            Dim logDate As Date = Date.Today
            Dim arrivalTime As DateTime = DateTime.Now
            Dim arrStatus As String = "Successful"
            With cmd.Parameters
                .AddWithValue("?", Trim(tagID))
                .AddWithValue("?", teacherID)
                .AddWithValue("?", logDate)
                .AddWithValue("?", arrivalTime)
                .AddWithValue("?", arrStatus)
            End With
            cmd.ExecuteNonQuery()
            Return "Time In"
        End If
    End Function

    Private Sub ShowAttendanceCard(fullName As String, position As String, attendanceType As String, profileImg As Byte())
        Try
            _logger.LogInfo($"ShowAttendanceCard called - Name: {fullName}, Status: {attendanceType}")

            ' Hide RFID waiting image
            picRFIDIcon.Visible = False
            lblWaitingMessage.Visible = False

            ' Clear previous cards
            pnlCardContainer.Controls.Clear()

            ' Determine card color based on status
            Dim statusColor As Color
            Dim statusText As String
            If attendanceType = "Time In" Then
                statusColor = Color.FromArgb(39, 174, 96) ' Professional Green
                statusText = "TIME IN"
            ElseIf attendanceType = "Time Out" Then
                statusColor = Color.FromArgb(192, 57, 43) ' Professional Red
                statusText = "TIME OUT"
            Else
                statusColor = Color.FromArgb(243, 156, 18) ' Professional Orange
                statusText = "COMPLETED"
            End If

            ' Create main card panel - Professional ID Card Style
            Dim cardPanel As New Panel()
            cardPanel.Size = New Size(950, 420)
            cardPanel.Location = New Point((pnlCardContainer.Width - 950) \ 2, (pnlCardContainer.Height - 420) \ 2)
            cardPanel.BackColor = Color.White
            cardPanel.BorderStyle = BorderStyle.FixedSingle

            ' Left side - Photo section with colored background
            Dim photoPanel As New Panel()
            photoPanel.Size = New Size(300, 420)
            photoPanel.Location = New Point(0, 0)
            photoPanel.BackColor = Color.FromArgb(240, 240, 240)
            cardPanel.Controls.Add(photoPanel)

            ' Profile picture - Stretched to fill
            Dim picProfile As New PictureBox()
            picProfile.Size = New Size(270, 270)
            picProfile.Location = New Point(27, 80)
            picProfile.SizeMode = PictureBoxSizeMode.StretchImage
            picProfile.BorderStyle = BorderStyle.FixedSingle
            picProfile.BackColor = Color.White

            ' Load profile image
            If profileImg IsNot Nothing AndAlso profileImg.Length > 0 Then
                Try
                    Using ms As New System.IO.MemoryStream(profileImg)
                        picProfile.Image = Image.FromStream(ms)
                    End Using
                    _logger.LogInfo("Profile image loaded successfully")
                Catch ex As Exception
                    _logger.LogWarning($"Could not load profile image: {ex.Message}")
                    picProfile.BackColor = Color.FromArgb(200, 200, 200)
                End Try
            Else
                picProfile.BackColor = Color.FromArgb(200, 200, 200)
            End If
            photoPanel.Controls.Add(picProfile)

            ' School logo/text at top of photo panel
            Dim lblSchool As New Label()
            lblSchool.Text = "TALA HIGH SCHOOL"
            lblSchool.Font = New Font("Segoe UI", 11, FontStyle.Bold)
            lblSchool.ForeColor = Color.FromArgb(52, 73, 94)
            lblSchool.Size = New Size(280, 50)
            lblSchool.Location = New Point(10, 20)
            lblSchool.TextAlign = ContentAlignment.MiddleCenter
            photoPanel.Controls.Add(lblSchool)

            ' Employee ID at bottom of photo
            Dim lblEmpID As New Label()
            lblEmpID.Text = $"ID: {position}"
            lblEmpID.Font = New Font("Segoe UI", 16, FontStyle.Bold)
            lblEmpID.ForeColor = Color.FromArgb(52, 73, 94)
            lblEmpID.Size = New Size(280, 50)
            lblEmpID.Location = New Point(10, 360)
            lblEmpID.TextAlign = ContentAlignment.MiddleCenter
            photoPanel.Controls.Add(lblEmpID)

            ' Right side - Information section
            Dim infoPanel As New Panel()
            infoPanel.Size = New Size(650, 420)
            infoPanel.Location = New Point(300, 0)
            infoPanel.BackColor = Color.White
            cardPanel.Controls.Add(infoPanel)

            ' Status bar at top
            Dim statusBar As New Panel()
            statusBar.Size = New Size(650, 80)
            statusBar.Location = New Point(0, 0)
            statusBar.BackColor = statusColor
            infoPanel.Controls.Add(statusBar)

            ' Status label
            Dim lblStatus As New Label()
            lblStatus.Text = statusText
            lblStatus.Font = New Font("Segoe UI", 38, FontStyle.Bold)
            lblStatus.ForeColor = Color.White
            lblStatus.Size = New Size(650, 80)
            lblStatus.TextAlign = ContentAlignment.MiddleCenter
            statusBar.Controls.Add(lblStatus)

            ' Teacher name - Large and prominent
            Dim lblName As New Label()
            lblName.Text = fullName
            lblName.Font = New Font("Segoe UI", 34, FontStyle.Bold)
            lblName.ForeColor = Color.FromArgb(44, 62, 80)
            lblName.Size = New Size(620, 90)
            lblName.Location = New Point(15, 100)
            lblName.TextAlign = ContentAlignment.MiddleLeft
            lblName.AutoEllipsis = True
            infoPanel.Controls.Add(lblName)

            ' Separator line
            Dim separator As New Panel()
            separator.Size = New Size(620, 3)
            separator.Location = New Point(15, 195)
            separator.BackColor = statusColor
            infoPanel.Controls.Add(separator)

            ' Date label
            Dim lblDateLabel As New Label()
            lblDateLabel.Text = "DATE:"
            lblDateLabel.Font = New Font("Segoe UI", 16, FontStyle.Bold)
            lblDateLabel.ForeColor = Color.FromArgb(127, 140, 141)
            lblDateLabel.Size = New Size(100, 40)
            lblDateLabel.Location = New Point(15, 220)
            lblDateLabel.TextAlign = ContentAlignment.MiddleLeft
            infoPanel.Controls.Add(lblDateLabel)

            ' Date value
            Dim lblDate As New Label()
            lblDate.Text = DateTime.Now.ToString("MMMM dd, yyyy")
            lblDate.Font = New Font("Segoe UI", 22, FontStyle.Regular)
            lblDate.ForeColor = Color.FromArgb(52, 73, 94)
            lblDate.Size = New Size(500, 40)
            lblDate.Location = New Point(120, 220)
            lblDate.TextAlign = ContentAlignment.MiddleLeft
            infoPanel.Controls.Add(lblDate)

            ' Time label
            Dim lblTimeLabel As New Label()
            lblTimeLabel.Text = "TIME:"
            lblTimeLabel.Font = New Font("Segoe UI", 16, FontStyle.Bold)
            lblTimeLabel.ForeColor = Color.FromArgb(127, 140, 141)
            lblTimeLabel.Size = New Size(100, 40)
            lblTimeLabel.Location = New Point(15, 275)
            lblTimeLabel.TextAlign = ContentAlignment.MiddleLeft
            infoPanel.Controls.Add(lblTimeLabel)

            ' Time value
            Dim lblTime As New Label()
            lblTime.Text = DateTime.Now.ToString("hh:mm:ss tt")
            lblTime.Font = New Font("Segoe UI", 28, FontStyle.Bold)
            lblTime.ForeColor = statusColor
            lblTime.Size = New Size(500, 50)
            lblTime.Location = New Point(120, 270)
            lblTime.TextAlign = ContentAlignment.MiddleLeft
            infoPanel.Controls.Add(lblTime)

            ' Day of week
            Dim lblDay As New Label()
            lblDay.Text = DateTime.Now.ToString("dddd")
            lblDay.Font = New Font("Segoe UI", 18, FontStyle.Regular)
            lblDay.ForeColor = Color.FromArgb(149, 165, 166)
            lblDay.Size = New Size(620, 40)
            lblDay.Location = New Point(15, 340)
            lblDay.TextAlign = ContentAlignment.MiddleLeft
            infoPanel.Controls.Add(lblDay)

            ' Add card to container
            pnlCardContainer.Controls.Add(cardPanel)
            pnlCardContainer.Visible = True

            _logger.LogInfo($"✓ Professional attendance card displayed: {fullName} - {attendanceType}")
        Catch ex As Exception
            _logger.LogError($"Error in ShowAttendanceCard: {ex.Message}")
            _logger.LogError($"Stack trace: {ex.StackTrace}")
        End Try
    End Sub

    Private Sub ShowErrorMessage(message As String)
        picRFIDIcon.Visible = False
        lblWaitingMessage.Visible = True
        lblWaitingMessage.Text = message
        lblWaitingMessage.ForeColor = Color.Red

        resetTimer.Stop()
        resetTimer.Start()
    End Sub

    Private Sub resetTimer_Tick(sender As Object, e As EventArgs) Handles resetTimer.Tick
        resetTimer.Stop()
        ShowWaitingState()
        lblWaitingMessage.ForeColor = Color.Black
    End Sub

    Private Sub clockTimer_Tick(sender As Object, e As EventArgs) Handles clockTimer.Tick
        UpdateClock()
    End Sub

    Private Sub UpdateClock()
        lblClock.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy - hh:mm:ss tt")
    End Sub

    Private Sub ShowScanningAnimation()
        ' Show scanning message
        lblWaitingMessage.Text = "Scanning..."
        lblWaitingMessage.ForeColor = Color.FromArgb(46, 204, 113) ' Green

        _logger.LogInfo("Showing scanning animation")
    End Sub

    Private Sub connectionMonitorTimer_Tick(sender As Object, e As EventArgs) Handles connectionMonitorTimer.Tick
        Try
            ' Check if we're the current owner and if connection is still active
            If _comPortManager.CurrentOwner = FORM_NAME Then
                If Not _comPortManager.IsConnected Then
                    _logger.LogWarning("Connection lost - attempting to reconnect...")
                    UpdateWaitingMessageForError("RECEIVER DISCONNECTED")
                    
                    ' Try to reconnect
                    If _comPortManager.ConnectToSiliconLabsPort() Then
                        _logger.LogInfo("✓ Reconnected successfully!")
                        ShowWaitingState()
                    End If
                End If
            Else
                ' We're not the owner, check if we can reclaim
                If String.IsNullOrEmpty(_comPortManager.CurrentOwner) Then
                    _logger.LogDebug("No current owner, attempting to reclaim access...")
                    _comPortManager.RequestAccess(FORM_NAME, autoConnect:=True)
                End If
            End If
        Catch ex As Exception
            _logger.LogError($"Error in connection monitor: {ex.Message}")
        End Try
    End Sub

    Private Sub RFIDScanMonitor_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            ' Prevent closing from logout or other programmatic closes - only allow manual close
            If e.CloseReason = CloseReason.ApplicationExitCall OrElse e.CloseReason = CloseReason.FormOwnerClosing Then
                ' Cancel the close - RFID Monitor should stay open
                e.Cancel = True
                _logger.LogInfo("RFIDScanMonitor close cancelled - monitor should remain open")
                Return
            End If
            
            ' If user manually closes via X button, allow it without confirmation
            If e.CloseReason = CloseReason.UserClosing Then
                _logger.LogInfo("User manually closing RFIDScanMonitor")
            End If
            
            ' Unsubscribe from events
            RemoveHandler _comPortManager.PortAccessRequested, AddressOf OnPortAccessRequested
            RemoveHandler _comPortManager.PortAccessGranted, AddressOf OnPortAccessGranted
            RemoveHandler _comPortManager.PortReleased, AddressOf OnPortReleased
            RemoveHandler _comPortManager.PortDisconnected, AddressOf OnPortDisconnected
            RemoveHandler _comPortManager.DataReceived, AddressOf OnDataReceived
            
            ' Release COM port access
            _comPortManager.ReleaseAccess(FORM_NAME)
            
            ' Stop timers
            If clockTimer IsNot Nothing Then
                clockTimer.Stop()
                clockTimer.Dispose()
            End If

            If resetTimer IsNot Nothing Then
                resetTimer.Stop()
                resetTimer.Dispose()
            End If

            If connectionMonitorTimer IsNot Nothing Then
                connectionMonitorTimer.Stop()
                connectionMonitorTimer.Dispose()
            End If
            
            _logger.LogInfo("RFIDScanMonitor closed successfully")
        Catch ex As Exception
            _logger.LogError($"Error closing RFIDScanMonitor: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' Applies transparency to the school background image
    ''' </summary>
    Private Sub ApplyBlueTintToSchoolImage()
        Try
            If pbSchool.Image Is Nothing Then
                _logger.LogWarning("pbSchool has no image to apply transparency")
                Return
            End If

            Dim imageOpacity As Single = 0.2F
            Dim blueTintAlpha As Integer = 0
            Dim backgroundColor As Color = Color.White

            ' original image
            Dim originalImage As Image = pbSchool.Image
            Dim tintedImage As New Bitmap(originalImage.Width, originalImage.Height)

            Using g As Graphics = Graphics.FromImage(tintedImage)
                g.Clear(backgroundColor)
                Dim colorMatrix As New System.Drawing.Imaging.ColorMatrix()
                colorMatrix.Matrix33 = imageOpacity
                Dim imageAttributes As New System.Drawing.Imaging.ImageAttributes()
                imageAttributes.SetColorMatrix(colorMatrix, System.Drawing.Imaging.ColorMatrixFlag.Default, System.Drawing.Imaging.ColorAdjustType.Bitmap)
                g.DrawImage(originalImage, New Rectangle(0, 0, originalImage.Width, originalImage.Height), 0, 0, originalImage.Width, originalImage.Height, GraphicsUnit.Pixel, imageAttributes)
                Using blueTint As New SolidBrush(Color.FromArgb(blueTintAlpha, 100, 150, 200))
                    g.FillRectangle(blueTint, 0, 0, tintedImage.Width, tintedImage.Height)
                End Using
            End Using
            pbSchool.Image = tintedImage

            _logger.LogInfo($"Image transparency ({imageOpacity}) and blue tint ({blueTintAlpha}) applied")
        Catch ex As Exception
            _logger.LogError($"Error applying transparency to school image: {ex.Message}")
        End Try
    End Sub

End Class
