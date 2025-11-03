Imports System.Data.Odbc
Imports System.Data.SqlClient
Imports System.IO
Imports System.IO.Ports
Imports System.Management
Imports System.Reflection
Imports System.Text

Public Class FormAttendanceScanner
    Private port As SerialPort
    Private ReadOnly _logger As ILogger = LoggerFactory.Instance
    Public deviceID As Integer = 0
    Public student_id As String = ""

    Private WithEvents displayTimer As New Timer()
    Private announcementIndex As Integer = 0
    Private announcementCards As New List(Of AnnouncementCard)

    Private Sub FormAttendanceScanner_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _logger.LogInfo("FormAttendanceScanner loading...")
        
        ' Initialize port
        port = New SerialPort()
        
        ' Connect to COM port with Silicon Labs detection
        ConnectToSiliconLabsPort()

        displayTimer.Interval = 10000 ' 10 seconds
        displayTimer.Start()

        ' Load announcements from database
        LoadAnnouncements()

        ' Initially show the first announcement
        ShowNextAnnouncement()

        Timer1.Enabled = True
        Timer2.Enabled = True
        
        _logger.LogInfo("FormAttendanceScanner loaded successfully")
    End Sub
    Private Sub LoadAnnouncements()
        Dim cmd As Odbc.OdbcCommand
        Dim da As New Odbc.OdbcDataAdapter
        Dim dt As New DataTable
        Try
            connectDB()
            cmd = New Odbc.OdbcCommand("SELECT pictureHeader, header, dayInfo, timeInfo, description, lookFor FROM announcements WHERE isActive = 1", con)
            da.SelectCommand = cmd
            da.Fill(dt)

            If dt.Rows.Count > 0 Then
                Dim reader As Odbc.OdbcDataReader = cmd.ExecuteReader()

                While reader.Read()
                    Dim announcement As New AnnouncementCard()

                    Dim header As String = reader("header")
                    Dim dayInfo As Date = reader("dayInfo")
                    Dim timeInfo As String = reader("timeInfo")
                    Dim description As String = reader("description")
                    Dim lookFor As String = reader("lookFor")

                    Try
                        Dim profileImg As Byte()
                        profileImg = reader("pictureHeader")
                        Dim ms As New MemoryStream(profileImg)
                        announcement.AnnouncementImage = Image.FromStream(ms)
                    Catch ex As Exception

                    End Try

                    ' Create New AnnouncementCard instance
                    announcement.Header = header
                    announcement.DateInfo = "Date: " + dayInfo.ToString("MMMM dd, yyyy")
                    announcement.DayInfo = "Day: " + dayInfo.DayOfWeek.ToString()
                    announcement.TimeInfo = "Time: " + timeInfo
                    announcement.Description = description
                    announcement.LookFor = "For further information please look for Mr/Ms. " + lookFor
                    ' announcement.AnnouncementImage = Image

                    announcement.Dock = DockStyle.Fill
                    announcementCards.Add(announcement)
                End While

                reader.Close()
            End If
        Catch ex As Exception
            MessageBox.Show("Error loading announcements: " & ex.Message)
        Finally
            con.Close()
        End Try
    End Sub

    Private Sub displayTimer_Tick(sender As Object, e As EventArgs) Handles displayTimer.Tick
        ShowNextAnnouncement()
    End Sub
    Private Sub ShowNextAnnouncement()
        ' Check if there are announcements to display
        If announcementIndex < announcementCards.Count Then
            ' Remove previous announcement card, if any
            panelThisAnnouncement.Controls.Clear()

            ' Add new announcement card to the panel
            Dim currentAnnouncement As AnnouncementCard = announcementCards(announcementIndex)
            panelThisAnnouncement.Controls.Add(currentAnnouncement)
            currentAnnouncement.Dock = DockStyle.Fill

            ' Increment index for next announcement
            announcementIndex += 1
        Else
            ' Reset index to loop back to the beginning
            announcementIndex = 0

            ' Clear panel to prepare for the next cycle
            panelThisAnnouncement.Controls.Clear()

            ' Add the first announcement card again
            Dim currentAnnouncement As AnnouncementCard = announcementCards(announcementIndex)
            panelThisAnnouncement.Controls.Add(currentAnnouncement)
            currentAnnouncement.Dock = DockStyle.Fill

            ' Increment index for next announcement
            announcementIndex += 1
        End If
    End Sub
    Private Sub ConnectToSiliconLabsPort()
        Try
            _logger.LogInfo("Searching for Silicon Labs RFID receiver...")
            
            ' Define the desired baud rate
            Dim baudRate As Integer = 115200

            ' Retrieve the available COM ports
            Dim availablePorts As String() = SerialPort.GetPortNames()
            _logger.LogInfo($"Found {availablePorts.Length} available COM ports: {String.Join(", ", availablePorts)}")

            If availablePorts.Length = 0 Then
                _logger.LogWarning("No COM ports detected on system")
                MessageBox.Show("No COM ports available. Please connect the RFID receiver.", "No Ports", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' Get detailed port information using WMI to detect Silicon Labs devices
            Dim portDetails As New Dictionary(Of String, String)
            Dim siliconLabsPorts As New List(Of String)
            
            Try
                Dim searcher As New ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%(COM%'")
                For Each obj As ManagementObject In searcher.Get()
                    Dim caption As String = obj("Caption")?.ToString()
                    If Not String.IsNullOrEmpty(caption) Then
                        Dim match = System.Text.RegularExpressions.Regex.Match(caption, "\(COM(\d+)\)")
                        If match.Success Then
                            Dim portName As String = "COM" & match.Groups(1).Value
                            portDetails(portName) = caption
                            
                            ' Check if it's a Silicon Labs device
                            If caption.ToLower().Contains("silicon labs") OrElse caption.ToLower().Contains("cp210") Then
                                siliconLabsPorts.Add(portName)
                                _logger.LogInfo($"✓ Detected Silicon Labs port: {portName} - {caption}")
                            End If
                        End If
                    End If
                Next
            Catch ex As Exception
                _logger.LogWarning($"Could not query WMI for port details: {ex.Message}")
            End Try

            ' Try Silicon Labs ports first
            If siliconLabsPorts.Count > 0 Then
                For Each comPort As String In siliconLabsPorts
                    If TryConnectToPort(comPort, baudRate) Then
                        Return ' Successfully connected
                    End If
                Next
            End If

            ' If no Silicon Labs port worked, try all available ports
            _logger.LogInfo("No Silicon Labs port found or connection failed, trying all available ports...")
            For Each comPort As String In availablePorts
                If Not siliconLabsPorts.Contains(comPort) Then ' Skip already tried ports
                    If TryConnectToPort(comPort, baudRate) Then
                        Return ' Successfully connected
                    End If
                End If
            Next

            ' No valid port found
            _logger.LogError("Failed to connect to any COM port")
            MessageBox.Show("Could not connect to RFID receiver. Please check the connection and try again.", "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
            
        Catch ex As Exception
            _logger.LogError($"Error in ConnectToSiliconLabsPort: {ex.Message}")
            MessageBox.Show($"Error connecting to COM port: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function TryConnectToPort(comPort As String, baudRate As Integer) As Boolean
        Try
            _logger.LogInfo($"Attempting to connect to {comPort} at {baudRate} baud...")
            
            ' Create a new SerialPort instance
            Dim tempPort As New SerialPort(comPort, baudRate)
            tempPort.Open()
            
            ' Connection successful
            port = tempPort
            _logger.LogInfo($"✓ Successfully connected to {comPort}")
            
            ' Start reading data
            Task.Run(AddressOf ReadDataAsync)
            _logger.LogInfo("Started async data reading task for Attendance Scanner")
            
            Return True
            
        Catch ex As Exception
            _logger.LogDebug($"Failed to connect to {comPort}: {ex.Message}")
            Return False
        End Try
    End Function
    Public Sub ReadDataAsync()
        Try
            _logger.LogInfo("ReadDataAsync started for Attendance Scanner - waiting for RFID scans...")
            _logger.LogInfo($"Port status - IsOpen: {port.IsOpen}, PortName: {port.PortName}, BaudRate: {port.BaudRate}")

            While True
                ' Check if port is still open
                If port Is Nothing OrElse Not port.IsOpen Then
                    _logger.LogDebug("Port closed, exiting ReadDataAsync loop")
                    Exit While
                End If

                Dim buffer As New StringBuilder()
                
                ' Wait until there is data to read
                While port.BytesToRead = 0
                    ' Check if port is still open during wait
                    If port Is Nothing OrElse Not port.IsOpen Then
                        _logger.LogDebug("Port closed while waiting for data")
                        Exit While
                    End If
                    Application.DoEvents()
                    System.Threading.Thread.Sleep(10) ' Small delay to prevent CPU spinning
                End While
                
                ' Double-check port is still open before reading
                If port Is Nothing OrElse Not port.IsOpen Then
                    Exit While
                End If

                _logger.LogDebug($"Data detected on {port.PortName}! Bytes to read: {port.BytesToRead}")

                ' Read data byte by byte
                Do
                    Dim data As Integer = port.ReadByte()
                    
                    If data = 10 Then ' Newline - end of transmission
                        _logger.LogDebug("Newline character detected, ending read")
                        Exit Do
                    ElseIf data = 13 Then ' Carriage return - skip
                        _logger.LogDebug("Carriage return detected, skipping")
                    Else
                        buffer.Append(Convert.ToChar(data))
                    End If
                Loop

                Dim tagData As String = buffer.ToString().Trim()
                _logger.LogInfo($"RFID tag scanned (raw): '{tagData}' (length: {tagData.Length} chars)")

                If Not String.IsNullOrEmpty(tagData) Then
                    ' Parse device ID if present
                    If tagData.Length > 8 AndAlso Char.IsDigit(tagData(0)) Then
                        deviceID = Integer.Parse(tagData.Substring(0, 1))
                        tagData = tagData.Substring(1)
                        _logger.LogInfo($"✓ Parsed - Device ID: {deviceID}, Tag ID: {tagData}")
                    Else
                        deviceID = 0
                        _logger.LogInfo($"✓ No device ID prefix, using full tag: {tagData}")
                    End If

                    Invoke(Sub() txtTagID.Text = tagData)
                    _logger.LogInfo($"✓ Tag ID set in textbox: {tagData}")
                Else
                    _logger.LogWarning("Received empty data, ignoring...")
                End If
            End While
            
        Catch ex As IO.IOException When ex.Message.Contains("port is closed")
            ' Expected when form is closing - don't log as error
            _logger.LogDebug("Port closed during read operation (expected on form close)")
        Catch ex As Exception
            _logger.LogError($"Error in ReadDataAsync: {ex.Message}")
            _logger.LogError($"Stack trace: {ex.StackTrace}")
        End Try
    End Sub
    '123CBA029
    '133C85A29
    '1E3B66729
    '1E355E328

    Private Sub AddFacultyCard(ByVal tagID As String)
        Dim cmd As System.Data.Odbc.OdbcCommand
        Dim da As New System.Data.Odbc.OdbcDataAdapter
        Dim dt As New DataTable
        
        _logger.LogInfo($"Processing faculty card for tag: {tagID}, Device ID: {deviceID}")

        Try
            connectDB()

            ' Check if tag belongs to a teacher
            cmd = New System.Data.Odbc.OdbcCommand("
                            SELECT tagID, 
                            UCase(CONCAT(lastname, ', ', LEFT(firstname, 1), '.', ' ', LEFT(middlename, 1), '.')) AS teacherName, 
                            profileImg, employeeID, teacherID   
                            FROM teacherinformation WHERE tagID=?", con)
            cmd.Parameters.AddWithValue("@", tagID)
            da.SelectCommand = cmd
            da.Fill(dt)

            If dt.Rows.Count > 0 Then
                _logger.LogInfo($"✓ Faculty found: {dt.Rows(0)("teacherName")}, Employee ID: {dt.Rows(0)("employeeID")}")
                
                ' Create a new card for the teacher
                Dim facultyCard As New StudentCard()

                facultyCard.StudentName = dt.Rows(0)("teacherName").ToString()
                facultyCard.LRN = dt.Rows(0)("employeeID").ToString()
                facultyCard.DateTimeInfo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt")

                ' Handle profile image
                If Not IsDBNull(dt.Rows(0)("profileImg")) Then
                    Dim imgData As Byte() = CType(dt.Rows(0)("profileImg"), Byte())
                    Using ms As New System.IO.MemoryStream(imgData)
                        facultyCard.pbPicture.Image = Image.FromStream(ms)
                    End Using
                Else
                    ' Default to a placeholder image if none exists
                    facultyCard.pbPicture.Image = My.Resources.default_image
                End If

                ' Add the card to the grid panel
                AddToGridPanel(facultyCard)

                ' Check if the teacher is already checked ina
                cmd = New System.Data.Odbc.OdbcCommand("SELECT ar.tag_id FROM attendance_record ar 
                                            WHERE ar.tag_id=? AND departureTime IS NULL AND depState = 0", con)
                cmd.Parameters.AddWithValue("@", tagID)
                Dim myreader As OdbcDataReader = cmd.ExecuteReader()

                If myreader.HasRows Then
                    ' Update the departure time
                    _logger.LogInfo($"Faculty already checked in, recording Time Out for tag: {tagID}")
                    cmd = New System.Data.Odbc.OdbcCommand("UPDATE attendance_record SET departureTime=?, depStatus='Successful', depState = 1 WHERE tag_id=? AND arrivalTime IS NOT NULL AND depState = 0", con)
                    Dim departureTime As DateTime = DateTime.Now
                    cmd.Parameters.AddWithValue("@", departureTime)
                    cmd.Parameters.AddWithValue("@", tagID)

                    facultyCard.Status = "Time Out"
                Else
                    ' Insert new attendance record for teacher
                    _logger.LogInfo($"Recording Time In for tag: {tagID}")
                    cmd = New System.Data.Odbc.OdbcCommand("INSERT INTO attendance_record(tag_id, teacherID, logDate, arrivalTime, arrStatus) VALUES(?,?,?,?,?)", con)
                    Dim logDate As Date = Date.Today
                    Dim arrivalTime As DateTime = DateTime.Now
                    Dim arrStatus As String = "Successful"
                    With cmd.Parameters
                        .AddWithValue("@", Trim(tagID))
                        .AddWithValue("@", dt.Rows(0)("teacherID"))
                        .AddWithValue("@", logDate)
                        .AddWithValue("@", arrivalTime)
                        .AddWithValue("@", arrStatus)
                    End With

                    facultyCard.Status = "Time In"
                End If
                cmd.ExecuteNonQuery()
                _logger.LogInfo($"✓ Attendance recorded successfully - Status: {facultyCard.Status}")
                txtTagID.Clear()
            Else
                _logger.LogWarning($"No faculty found with tag ID: {tagID}")
                txtTagID.Clear()
            End If
        Catch ex As Exception
            _logger.LogError($"Error in AddFacultyCard: {ex.Message}")
            MessageBox.Show($"Error processing attendance: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            con.Close()
            GC.Collect()
        End Try
    End Sub

    Private Sub AddToGridPanel(ByVal facultyCard As StudentCard)
        pnlFacultyCard.SuspendLayout()
        pnlFacultyCard.Controls.Add(facultyCard)

        ' Set card size for two columns
        Dim cardWidth As Integer = (pnlFacultyCard.Width \ 2) - 15 ' ~445 pixels
        Dim cardHeight As Integer = pnlFacultyCard.Height \ 4 - 10 ' Adjust as needed
        facultyCard.Size = New Size(cardWidth, cardHeight)
        facultyCard.Margin = New Padding(5)

        ' Ensure FlowLayoutPanel properties
        pnlFacultyCard.FlowDirection = FlowDirection.LeftToRight
        pnlFacultyCard.WrapContents = True

        ' Timer to remove the card
        Dim removalTimer As New Timer()
        removalTimer.Interval = 2000
        AddHandler removalTimer.Tick,
        Sub(sender As Object, e As EventArgs)
            removalTimer.Stop()
            pnlFacultyCard.Controls.Remove(facultyCard)
            facultyCard.Dispose()
            removalTimer.Dispose()
            pnlFacultyCard.ResumeLayout()
        End Sub
        removalTimer.Start()

        pnlFacultyCard.ResumeLayout()
    End Sub

    'Private Sub AddToGridPanel(ByVal facultyCard As StudentCard)
    '    pnlFacultyCard.SuspendLayout()

    '    ' Ensure FlowLayoutPanel is top-to-bottom
    '    pnlFacultyCard.FlowDirection = FlowDirection.TopDown

    '    pnlFacultyCard.WrapContents = False
    '    pnlFacultyCard.AutoScroll = True

    '    ' Set card size to fit the panel's width with some padding
    '    Dim cardWidth As Integer = pnlFacultyCard.ClientSize.Width - 20 ' Adjust for padding and scrollbar
    '    Dim cardHeight As Integer = 200 ' Or any fixed height you prefer

    '    facultyCard.Size = New Size(cardWidth, cardHeight)
    '    facultyCard.Margin = New Padding(5)

    '    pnlFacultyCard.Controls.Add(facultyCard)

    '    ' Timer to remove the card
    '    Dim removalTimer As New Timer()
    '    removalTimer.Interval = 2000
    '    AddHandler removalTimer.Tick,
    '    Sub(sender As Object, e As EventArgs)
    '        removalTimer.Stop()
    '        pnlFacultyCard.Controls.Remove(facultyCard)
    '        facultyCard.Dispose()
    '        removalTimer.Dispose()
    '        pnlFacultyCard.ResumeLayout()
    '    End Sub
    '    removalTimer.Start()

    '    pnlFacultyCard.ResumeLayout()
    'End Sub



    Private Sub tmrHideCard_Tick(sender As Object, e As EventArgs) Handles tmrHideCard.Tick
        Dim panel As Panel = CType(tmrHideCard.Tag, Panel)
        If panel IsNot Nothing Then
            panel.Controls.Clear() ' Clear the card from the panel
        End If
        tmrHideCard.Stop() ' Stop the timer
        tmrHideCard.Enabled = False ' Disable the timer
    End Sub

    Private Sub FormAttendanceScanner_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        _logger.LogInfo("FormAttendanceScanner closing...")
        
        Try
            ' Stop timers
            If displayTimer IsNot Nothing Then
                displayTimer.Stop()
                displayTimer.Dispose()
            End If
            
            Timer1.Enabled = False
            Timer2.Enabled = False
            
            ' Close COM port
            If port IsNot Nothing AndAlso port.IsOpen Then
                port.Close()
                _logger.LogInfo($"Closed COM port: {port.PortName}")
            End If
        Catch ex As Exception
            _logger.LogError($"Error closing FormAttendanceScanner: {ex.Message}")
        End Try
        
        _logger.LogInfo("FormAttendanceScanner closed")
        LoginForm.Show()
    End Sub

    Private Sub txtTagID_TextChanged(sender As Object, e As EventArgs) Handles txtTagID.TextChanged
        If txtTagID.TextLength > 7 Then
            AddFacultyCard(txtTagID.Text)
        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Label4.Text = Date.Now.ToString("MMMM-dd-yyyy   hh:mm:ss tt")
    End Sub

    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        Label6.Text = TimeOfDay.ToString(" hh:mm:ss tt    ")
        Label7.Text = Date.Now.ToString("MMMM-dd-yyyy    ")

    End Sub
End Class