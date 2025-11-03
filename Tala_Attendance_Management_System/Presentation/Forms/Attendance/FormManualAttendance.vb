Imports System.Data.Odbc

Public Class FormManualAttendance
    Private ReadOnly _logger As ILogger = LoggerFactory.Instance
    Private ReadOnly _auditLogger As AuditLogger = AuditLogger.Instance

    Private Sub FormManualAttendance_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            _logger.LogInfo("FormManualAttendance - Form loaded")

            ' Set form properties
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.StartPosition = FormStartPosition.CenterParent

            ' Configure DateTimePickers
            dtpDate.Value = DateTime.Today
            dtpDate.MaxDate = DateTime.Today
            dtpTimeIn.Format = DateTimePickerFormat.Time
            dtpTimeIn.ShowUpDown = True
            dtpTimeOut.Format = DateTimePickerFormat.Time
            dtpTimeOut.ShowUpDown = True

            ' Load teachers
            LoadTeachers()

        Catch ex As Exception
            _logger.LogError($"FormManualAttendance - Error in Form_Load: {ex.Message}")
            MessageBox.Show("Error loading form. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LoadTeachers()
        Try
            _logger.LogInfo("FormManualAttendance - Loading teachers")

            connectDB()
            Dim query As String = "SELECT teacherID, CONCAT(firstname, ' ', lastname) AS teacher_name, tagID 
                                   FROM teacherinformation 
                                   WHERE isActive = 1 
                                   ORDER BY firstname, lastname"

            Dim dt As New DataTable()
            Using cmd As New OdbcCommand(query, con)
                Using adapter As New OdbcDataAdapter(cmd)
                    adapter.Fill(dt)
                End Using
            End Using

            cboTeacher.DataSource = dt
            cboTeacher.DisplayMember = "teacher_name"
            cboTeacher.ValueMember = "teacherID"
            cboTeacher.SelectedIndex = -1

            _logger.LogInfo($"FormManualAttendance - {dt.Rows.Count} teachers loaded")

        Catch ex As Exception
            _logger.LogError($"FormManualAttendance - Error loading teachers: {ex.Message}")
            MessageBox.Show("Error loading teachers. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            ' Validate inputs
            If cboTeacher.SelectedIndex = -1 Then
                MessageBox.Show("Please select a teacher.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                cboTeacher.Focus()
                Return
            End If

            If Not chkTimeIn.Checked AndAlso Not chkTimeOut.Checked Then
                MessageBox.Show("Please check at least Time-In or Time-Out.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' Get teacher info
            Dim teacherID As Integer = Convert.ToInt32(cboTeacher.SelectedValue)
            Dim teacherName As String = cboTeacher.Text
            Dim tagID As String = CType(cboTeacher.DataSource, DataTable).Rows(cboTeacher.SelectedIndex)("tagID").ToString()

            ' Prepare date and times
            Dim logDate As Date = dtpDate.Value.Date
            Dim timeIn As DateTime? = Nothing
            Dim timeOut As DateTime? = Nothing

            If chkTimeIn.Checked Then
                timeIn = New DateTime(logDate.Year, logDate.Month, logDate.Day,
                                     dtpTimeIn.Value.Hour, dtpTimeIn.Value.Minute, dtpTimeIn.Value.Second)
            End If

            If chkTimeOut.Checked Then
                timeOut = New DateTime(logDate.Year, logDate.Month, logDate.Day,
                                      dtpTimeOut.Value.Hour, dtpTimeOut.Value.Minute, dtpTimeOut.Value.Second)
            End If

            ' Validation #13: Past date limit (max 30 days back)
            Dim daysDifference As Integer = DateTime.Now.Date.Subtract(logDate).Days
            If daysDifference > 30 Then
                MessageBox.Show("Cannot enter attendance records older than 30 days.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' Validation #1: Future time prevention
            Dim currentDateTime As DateTime = DateTime.Now
            If timeIn.HasValue AndAlso timeIn.Value > currentDateTime Then
                MessageBox.Show("Time-In cannot be in the future.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                dtpTimeIn.Focus()
                Return
            End If

            If timeOut.HasValue AndAlso timeOut.Value > currentDateTime Then
                MessageBox.Show("Time-Out cannot be in the future.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                dtpTimeOut.Focus()
                Return
            End If

            ' Validate time logic (minimum 1 minute interval)
            If timeIn.HasValue AndAlso timeOut.HasValue Then
                Dim timeDifference As TimeSpan = timeOut.Value.Subtract(timeIn.Value)
                
                If timeDifference.TotalMinutes < 1 Then
                    MessageBox.Show("Time-Out must be at least 1 minute after Time-In.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return
                End If

                ' Validation #3: Maximum duration (24 hours)
                If timeDifference.TotalHours > 24 Then
                    MessageBox.Show("Time duration cannot exceed 24 hours. Please check your time entries.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return
                End If
            End If

            ' Confirm action
            Dim message As String = $"Add manual attendance record?{vbCrLf}{vbCrLf}" &
                                   $"Teacher: {teacherName}{vbCrLf}" &
                                   $"Date: {logDate:MMMM dd, yyyy}{vbCrLf}" &
                                   $"Time-In: {If(timeIn.HasValue, timeIn.Value.ToString("hh:mm tt"), "Not Set")}{vbCrLf}" &
                                   $"Time-Out: {If(timeOut.HasValue, timeOut.Value.ToString("hh:mm tt"), "Not Set")}"

            If MessageBox.Show(message, "Confirm Manual Entry", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.No Then
                Return
            End If

            ' Smart logic: Check if record exists and handle accordingly
            connectDB()
            
            ' Check for existing record with only time-in (no time-out)
            ' Order by attendanceID DESC to get the most recent record first
            Dim checkQuery As String = "SELECT attendanceID, arrivalTime, departureTime FROM attendance_record WHERE tag_id = ? AND DATE(logDate) = ? ORDER BY attendanceID DESC LIMIT 1"
            Dim existingRecordId As Integer = 0
            Dim hasTimeIn As Boolean = False
            Dim hasTimeOut As Boolean = False

            Using checkCmd As New OdbcCommand(checkQuery, con)
                checkCmd.Parameters.AddWithValue("?", tagID)
                checkCmd.Parameters.AddWithValue("?", logDate.ToString("yyyy-MM-dd"))
                
                _logger.LogDebug($"Checking for existing record - tagID: {tagID}, date: {logDate:yyyy-MM-dd}")
                
                Using reader = checkCmd.ExecuteReader()
                    If reader.Read() Then
                        existingRecordId = Convert.ToInt32(reader("attendanceID"))
                        hasTimeIn = Not IsDBNull(reader("arrivalTime"))
                        hasTimeOut = Not IsDBNull(reader("departureTime"))
                        
                        _logger.LogDebug($"Found existing record - ID: {existingRecordId}, hasTimeIn: {hasTimeIn}, hasTimeOut: {hasTimeOut}")
                    Else
                        _logger.LogDebug("No existing record found")
                    End If
                End Using
            End Using

            Dim cmd As OdbcCommand
            Dim action As String = ""

            ' Smart decision logic
            If existingRecordId > 0 Then
                ' Record exists for this teacher on this date
                If hasTimeIn AndAlso Not hasTimeOut AndAlso chkTimeOut.Checked AndAlso Not chkTimeIn.Checked Then
                    ' Existing record has only time-in, user is adding time-out only -> UPDATE
                    Dim updateQuery As String = "UPDATE attendance_record SET departureTime = ?, depStatus = ? WHERE attendanceID = ?"
                    cmd = New OdbcCommand(updateQuery, con)
                    cmd.Parameters.AddWithValue("?", timeOut.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                    cmd.Parameters.AddWithValue("?", "Manual Entry")
                    cmd.Parameters.AddWithValue("?", existingRecordId)
                    action = "updated"
                    
                    _logger.LogInfo($"Updating record ID {existingRecordId} for teacher '{teacherName}' (tagID: {tagID}) with time-out: {timeOut.Value:HH:mm:ss}")
                ElseIf hasTimeIn AndAlso hasTimeOut Then
                    ' Record already complete -> INSERT new record
                    If timeIn.HasValue AndAlso timeOut.HasValue Then
                        Dim insertQuery As String = "INSERT INTO attendance_record (tag_id, teacherID, logDate, arrivalTime, departureTime, arrStatus, depStatus) " &
                                                   "VALUES (?, ?, ?, ?, ?, ?, ?)"
                        cmd = New OdbcCommand(insertQuery, con)
                        cmd.Parameters.AddWithValue("?", tagID)
                        cmd.Parameters.AddWithValue("?", teacherID)
                        cmd.Parameters.AddWithValue("?", logDate.ToString("yyyy-MM-dd"))
                        cmd.Parameters.AddWithValue("?", timeIn.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                        cmd.Parameters.AddWithValue("?", timeOut.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                        cmd.Parameters.AddWithValue("?", "Manual Entry")
                        cmd.Parameters.AddWithValue("?", "Manual Entry")
                        action = "created"
                        
                        _logger.LogInfo("Existing record is complete, creating new record")
                    ElseIf timeIn.HasValue Then
                        Dim insertQuery As String = "INSERT INTO attendance_record (tag_id, teacherID, logDate, arrivalTime, arrStatus) " &
                                                   "VALUES (?, ?, ?, ?, ?)"
                        cmd = New OdbcCommand(insertQuery, con)
                        cmd.Parameters.AddWithValue("?", tagID)
                        cmd.Parameters.AddWithValue("?", teacherID)
                        cmd.Parameters.AddWithValue("?", logDate.ToString("yyyy-MM-dd"))
                        cmd.Parameters.AddWithValue("?", timeIn.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                        cmd.Parameters.AddWithValue("?", "Manual Entry")
                        action = "created"
                        
                        _logger.LogInfo("Creating new time-in record")
                    Else
                        MessageBox.Show("Cannot add only time-out when a complete record already exists for this date.", "Invalid Operation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        con.Close()
                        Return
                    End If
                Else
                    ' Other scenarios - show warning
                    MessageBox.Show("A record already exists for this teacher on this date. Please review the existing record.", "Record Exists", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    con.Close()
                    Return
                End If
            Else
                ' No existing record -> INSERT new record
                If timeIn.HasValue AndAlso timeOut.HasValue Then
                    Dim insertQuery As String = "INSERT INTO attendance_record (tag_id, teacherID, logDate, arrivalTime, departureTime, arrStatus, depStatus) " &
                                               "VALUES (?, ?, ?, ?, ?, ?, ?)"
                    cmd = New OdbcCommand(insertQuery, con)
                    cmd.Parameters.AddWithValue("?", tagID)
                    cmd.Parameters.AddWithValue("?", teacherID)
                    cmd.Parameters.AddWithValue("?", logDate.ToString("yyyy-MM-dd"))
                    cmd.Parameters.AddWithValue("?", timeIn.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                    cmd.Parameters.AddWithValue("?", timeOut.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                    cmd.Parameters.AddWithValue("?", "Manual Entry")
                    cmd.Parameters.AddWithValue("?", "Manual Entry")
                    action = "created"
                ElseIf timeIn.HasValue Then
                    Dim insertQuery As String = "INSERT INTO attendance_record (tag_id, teacherID, logDate, arrivalTime, arrStatus) " &
                                               "VALUES (?, ?, ?, ?, ?)"
                    cmd = New OdbcCommand(insertQuery, con)
                    cmd.Parameters.AddWithValue("?", tagID)
                    cmd.Parameters.AddWithValue("?", teacherID)
                    cmd.Parameters.AddWithValue("?", logDate.ToString("yyyy-MM-dd"))
                    cmd.Parameters.AddWithValue("?", timeIn.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                    cmd.Parameters.AddWithValue("?", "Manual Entry")
                    action = "created"
                Else
                    ' Only time-out without existing time-in
                    MessageBox.Show("Cannot add only time-out without an existing time-in record.", "Invalid Operation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    con.Close()
                    Return
                End If
                
                _logger.LogInfo("No existing record, creating new record")
            End If

            cmd.ExecuteNonQuery()
            cmd.Dispose()

            ' Log audit trail
            Dim auditAction As String = If(action = "updated", "Update", "Create")
            If action = "updated" Then
                _auditLogger.LogUpdate(MainForm.currentUsername, "Attendance",
                    $"Manual time-out added - Teacher: '{teacherName}', Date: {logDate:yyyy-MM-dd}, Time-Out: {timeOut.Value.ToString("HH:mm")}")
            Else
                _auditLogger.LogCreate(MainForm.currentUsername, "Attendance",
                    $"Manual attendance entry - Teacher: '{teacherName}', Date: {logDate:yyyy-MM-dd}, Time-In: {If(timeIn.HasValue, timeIn.Value.ToString("HH:mm"), "N/A")}, Time-Out: {If(timeOut.HasValue, timeOut.Value.ToString("HH:mm"), "N/A")}")
            End If

            _logger.LogInfo($"FormManualAttendance - Manual attendance {action} for teacher '{teacherName}' on {logDate:yyyy-MM-dd}")

            Dim successMessage As String = If(action = "updated", "Time-out added successfully!", "Manual attendance record created successfully!")
            MessageBox.Show(successMessage, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)

            Me.DialogResult = DialogResult.OK
            Me.Close()

        Catch ex As Exception
            _logger.LogError($"FormManualAttendance - Error saving manual attendance: {ex.Message}")
            MessageBox.Show("Error saving manual attendance record: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub chkTimeIn_CheckedChanged(sender As Object, e As EventArgs) Handles chkTimeIn.CheckedChanged
        dtpTimeIn.Enabled = chkTimeIn.Checked
    End Sub

    Private Sub chkTimeOut_CheckedChanged(sender As Object, e As EventArgs) Handles chkTimeOut.CheckedChanged
        dtpTimeOut.Enabled = chkTimeOut.Checked
    End Sub
End Class
