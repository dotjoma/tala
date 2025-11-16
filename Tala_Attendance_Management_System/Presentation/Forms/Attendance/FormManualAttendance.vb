Imports System.Data.Odbc

Public Class FormManualAttendance
    Private ReadOnly _logger As ILogger = LoggerFactory.Instance
    Private ReadOnly _auditLogger As AuditLogger = AuditLogger.Instance

    Private Sub FormManualAttendance_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            _logger.LogInfo("FormManualAttendance - Form loaded")

            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.StartPosition = FormStartPosition.CenterParent

            dtpDate.Value = DateTime.Today
            dtpDate.MaxDate = DateTime.Today
            dtpTimeIn.Format = DateTimePickerFormat.Time
            dtpTimeIn.ShowUpDown = True
            dtpTimeOut.Format = DateTimePickerFormat.Time
            dtpTimeOut.ShowUpDown = True

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
        ' Prevent double-clicking or multiple saves
        If btnSave.Enabled = False Then
            Return
        End If

        Try
            ' Disable save button to prevent multiple submissions
            btnSave.Enabled = False
            btnCancel.Enabled = False

            If cboTeacher.SelectedIndex = -1 Then
                btnSave.Enabled = True
                btnCancel.Enabled = True
                MessageBox.Show("Please select a teacher.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                cboTeacher.Focus()
                Return
            End If

            If Not chkTimeIn.Checked AndAlso Not chkTimeOut.Checked Then
                btnSave.Enabled = True
                btnCancel.Enabled = True
                MessageBox.Show("Please check at least Time-In or Time-Out.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' Validate reason field
            If String.IsNullOrWhiteSpace(txtReason.Text) Then
                btnSave.Enabled = True
                btnCancel.Enabled = True
                MessageBox.Show("Please provide a reason for this manual attendance entry.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtReason.Focus()
                Return
            End If

            Dim teacherID As Integer = Convert.ToInt32(cboTeacher.SelectedValue)
            Dim teacherName As String = cboTeacher.Text

            ' Get tagID directly from database using teacherID to ensure accuracy
            connectDB()
            Dim getTagCmd As New OdbcCommand("SELECT tagID FROM teacherinformation WHERE teacherID = ?", con)
            getTagCmd.Parameters.AddWithValue("?", teacherID)
            Dim tagID As String = ""
            Dim tagReader = getTagCmd.ExecuteReader()
            If tagReader.Read() Then
                If Not IsDBNull(tagReader("tagID")) Then
                    tagID = tagReader("tagID").ToString().Trim()
                End If
            End If
            tagReader.Close()
            con.Close()

            ' Validate tagID
            If String.IsNullOrWhiteSpace(tagID) OrElse tagID = "--" Then
                btnSave.Enabled = True
                btnCancel.Enabled = True
                MessageBox.Show($"Teacher '{teacherName}' does not have a valid RFID tag ID. Please assign a tag ID to this teacher first.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                _logger.LogWarning($"FormManualAttendance - Teacher '{teacherName}' (ID: {teacherID}) has no valid tagID")
                Return
            End If

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

            Dim daysDifference As Integer = DateTime.Now.Date.Subtract(logDate).Days
            If daysDifference > 30 Then
                btnSave.Enabled = True
                btnCancel.Enabled = True
                MessageBox.Show("Cannot enter attendance records older than 30 days.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim currentDateTime As DateTime = DateTime.Now
            If timeIn.HasValue AndAlso timeIn.Value > currentDateTime Then
                btnSave.Enabled = True
                btnCancel.Enabled = True
                MessageBox.Show("Time-In cannot be in the future.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                dtpTimeIn.Focus()
                Return
            End If

            If timeOut.HasValue AndAlso timeOut.Value > currentDateTime Then
                btnSave.Enabled = True
                btnCancel.Enabled = True
                MessageBox.Show("Time-Out cannot be in the future.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                dtpTimeOut.Focus()
                Return
            End If

            If timeIn.HasValue AndAlso timeOut.HasValue Then
                Dim timeDifference As TimeSpan = timeOut.Value.Subtract(timeIn.Value)

                If timeDifference.TotalMinutes < 1 Then
                    btnSave.Enabled = True
                    btnCancel.Enabled = True
                    MessageBox.Show("Time-Out must be at least 1 minute after Time-In.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return
                End If

                If timeDifference.TotalHours > 24 Then
                    btnSave.Enabled = True
                    btnCancel.Enabled = True
                    MessageBox.Show("Time duration cannot exceed 24 hours. Please check your time entries.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return
                End If
            End If

            Dim message As String = $"Add manual attendance record?{vbCrLf}{vbCrLf}" &
                                   $"Teacher: {teacherName}{vbCrLf}" &
                                   $"Date: {logDate:MMMM dd, yyyy}{vbCrLf}" &
                                   $"Time-In: {If(timeIn.HasValue, timeIn.Value.ToString("hh:mm tt"), "Not Set")}{vbCrLf}" &
                                   $"Time-Out: {If(timeOut.HasValue, timeOut.Value.ToString("hh:mm tt"), "Not Set")}"

            If MessageBox.Show(message, "Confirm Manual Entry", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.No Then
                Return
            End If

            connectDB()

            _logger.LogInfo($"FormManualAttendance - Processing manual attendance for teacher '{teacherName}' (ID: {teacherID}, tagID: '{tagID}') on {logDate:yyyy-MM-dd}")

            ' Check for existing records using both tag_id and teacherID for better accuracy
            Dim checkQuery As String = "SELECT attendanceID, arrivalTime, departureTime, tag_id, teacherID " &
                                      "FROM attendance_record " &
                                      "WHERE teacherID = ? AND DATE(logDate) = ? " &
                                      "ORDER BY attendanceID DESC"
            Dim existingRecordId As Integer = 0
            Dim hasTimeIn As Boolean = False
            Dim hasTimeOut As Boolean = False
            Dim existingRecords As New List(Of Integer)

            Using checkCmd As New OdbcCommand(checkQuery, con)
                checkCmd.Parameters.AddWithValue("?", teacherID)
                checkCmd.Parameters.AddWithValue("?", logDate.ToString("yyyy-MM-dd"))

                _logger.LogDebug($"Checking for existing records - teacherID: {teacherID}, tagID: '{tagID}', date: {logDate:yyyy-MM-dd}")

                Using reader = checkCmd.ExecuteReader()
                    While reader.Read()
                        Dim recordId As Integer = Convert.ToInt32(reader("attendanceID"))
                        existingRecords.Add(recordId)
                        Dim recordTagID As String = If(IsDBNull(reader("tag_id")), "", reader("tag_id").ToString().Trim())

                        _logger.LogDebug($"Found existing record - ID: {recordId}, tagID: '{recordTagID}', teacherID: {reader("teacherID")}")

                        ' Use the most recent record that matches (first one in DESC order)
                        If existingRecordId = 0 Then
                            existingRecordId = recordId
                            hasTimeIn = Not IsDBNull(reader("arrivalTime"))
                            hasTimeOut = Not IsDBNull(reader("departureTime"))
                            _logger.LogDebug($"Using record ID {existingRecordId} - hasTimeIn: {hasTimeIn}, hasTimeOut: {hasTimeOut}")
                        End If
                    End While

                    If existingRecords.Count > 1 Then
                        _logger.LogWarning($"Found {existingRecords.Count} existing records for teacherID {teacherID} on {logDate:yyyy-MM-dd}. This may indicate duplicate records.")
                    End If

                    If existingRecordId = 0 Then
                        _logger.LogDebug("No existing record found")
                    End If
                End Using
            End Using

            Dim cmd As OdbcCommand
            Dim action As String = ""

            Dim manualReason As String = txtReason.Text.Trim()
            Dim manualInputRemark As String = $"Manual Input: {manualReason}"
            Dim currentUser As String = MainForm.currentUsername

            ' Check for incomplete records from previous days (no time-out) if adding time-in
            If chkTimeIn.Checked AndAlso existingRecordId = 0 Then
                Dim checkIncompleteCmd As New OdbcCommand("SELECT attendanceID, logDate, remarks FROM attendance_record WHERE teacherID = ? AND departureTime IS NULL AND DATE(logDate) < ?", con)
                checkIncompleteCmd.Parameters.AddWithValue("?", teacherID)
                checkIncompleteCmd.Parameters.AddWithValue("?", logDate.ToString("yyyy-MM-dd"))
                Dim incompleteReader = checkIncompleteCmd.ExecuteReader()
                
                Dim incompleteRecords As New List(Of Tuple(Of Integer, Date, String))
                While incompleteReader.Read()
                    Dim incompleteID As Integer = incompleteReader("attendanceID")
                    Dim incompleteDate As Date = incompleteReader("logDate")
                    Dim existingRemarks As String = If(IsDBNull(incompleteReader("remarks")), "", incompleteReader("remarks").ToString())
                    incompleteRecords.Add(New Tuple(Of Integer, Date, String)(incompleteID, incompleteDate, existingRemarks))
                End While
                incompleteReader.Close()
                
                ' Mark incomplete records
                For Each incomplete In incompleteRecords
                    If Not incomplete.Item3.Contains("Incomplete") Then
                        Dim newRemarks As String = If(String.IsNullOrWhiteSpace(incomplete.Item3), "Incomplete - No time-out", incomplete.Item3 & "; Incomplete - No time-out")
                        
                        Dim updateIncompleteCmd As New OdbcCommand("UPDATE attendance_record SET remarks = ? WHERE attendanceID = ?", con)
                        updateIncompleteCmd.Parameters.AddWithValue("?", newRemarks)
                        updateIncompleteCmd.Parameters.AddWithValue("?", incomplete.Item1)
                        updateIncompleteCmd.ExecuteNonQuery()
                        
                        _logger.LogInfo($"Marked incomplete record ID {incomplete.Item1} from {incomplete.Item2:yyyy-MM-dd} as 'Incomplete - No time-out'")
                    End If
                Next
            End If
            
            ' Get teacher shift times (start/end) for status and remarks calculation
            Dim shiftStartTime As Object = Nothing
            Dim shiftEndTime As Object = Nothing
            Dim getShiftCmd As New OdbcCommand("SELECT shift_start_time, shift_end_time FROM teacherinformation WHERE teacherID = ?", con)
            getShiftCmd.Parameters.AddWithValue("?", teacherID)
            Dim shiftReader = getShiftCmd.ExecuteReader()
            If shiftReader.Read() Then
                If Not IsDBNull(shiftReader("shift_start_time")) Then
                    shiftStartTime = shiftReader("shift_start_time")
                End If
                If Not IsDBNull(shiftReader("shift_end_time")) Then
                    shiftEndTime = shiftReader("shift_end_time")
                End If
            End If
            shiftReader.Close()

            ' Compute arrival and departure status (zero grace)
            Dim arrStatusValue As String = Nothing
            Dim timeInRemarks As String = ""
            If timeIn.HasValue Then
                If shiftStartTime IsNot Nothing Then
                    Dim s As TimeSpan
                    If TypeOf shiftStartTime Is TimeSpan Then
                        s = DirectCast(shiftStartTime, TimeSpan)
                    Else
                        TimeSpan.TryParse(shiftStartTime.ToString(), s)
                    End If
                    arrStatusValue = If(timeIn.Value.TimeOfDay <= s, "On Time", "Late")
                    ' Calculate time-in remarks (Late X minutes)
                    timeInRemarks = CalculateTimeInRemarks(timeIn.Value, shiftStartTime)
                Else
                    arrStatusValue = "On Time"
                End If
            End If

            Dim depStatusValue As String = Nothing
            If timeOut.HasValue Then
                If shiftEndTime IsNot Nothing Then
                    Dim ek As TimeSpan
                    If TypeOf shiftEndTime Is TimeSpan Then
                        ek = DirectCast(shiftEndTime, TimeSpan)
                    Else
                        TimeSpan.TryParse(shiftEndTime.ToString(), ek)
                    End If
                    depStatusValue = If(timeOut.Value.TimeOfDay <= ek, "On Time", "Late")
                Else
                    depStatusValue = "On Time"
                End If
            End If

            If existingRecordId > 0 Then
                If hasTimeIn AndAlso Not hasTimeOut AndAlso chkTimeOut.Checked AndAlso Not chkTimeIn.Checked Then
                    ' Get existing remarks
                    Dim getRemarksCmd As New OdbcCommand("SELECT remarks FROM attendance_record WHERE attendanceID = ?", con)
                    getRemarksCmd.Parameters.AddWithValue("?", existingRecordId)
                    Dim existingRemarks As String = ""
                    Dim remarksReader = getRemarksCmd.ExecuteReader()
                    If remarksReader.Read() AndAlso Not IsDBNull(remarksReader("remarks")) Then
                        existingRemarks = remarksReader("remarks").ToString()
                    End If
                    remarksReader.Close()

                    ' Calculate Under time/Over time if time-out is provided
                    Dim finalRemarks As String = manualInputRemark
                    If timeOut.HasValue Then
                        Dim timeRemarks As String = CalculateTimeRemarks(timeOut.Value, shiftEndTime, "")
                        If Not String.IsNullOrWhiteSpace(timeRemarks) Then
                            finalRemarks = $"{manualInputRemark}; {timeRemarks}"
                        End If

                        ' Append to existing remarks if any (but not if it already has Manual Input)
                        If Not String.IsNullOrWhiteSpace(existingRemarks) AndAlso Not existingRemarks.Contains("Manual Input") Then
                            finalRemarks = $"{existingRemarks}; {finalRemarks}"
                        End If
                    Else
                        ' Append to existing remarks if any
                        If Not String.IsNullOrWhiteSpace(existingRemarks) Then
                            finalRemarks = $"{existingRemarks}; {manualInputRemark}"
                        End If
                    End If

                    Dim updateQuery As String = "UPDATE attendance_record SET departureTime = ?, depStatus = ?, remarks = ?, manual_input_by = ?, manual_input_reason = ? WHERE attendanceID = ?"
                    cmd = New OdbcCommand(updateQuery, con)
                    cmd.Parameters.AddWithValue("?", timeOut.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                    cmd.Parameters.AddWithValue("?", If(String.IsNullOrEmpty(depStatusValue), "On Time", depStatusValue))
                    cmd.Parameters.AddWithValue("?", finalRemarks)
                    cmd.Parameters.AddWithValue("?", currentUser)
                    cmd.Parameters.AddWithValue("?", manualReason)
                    cmd.Parameters.AddWithValue("?", existingRecordId)
                    action = "updated"

                    _logger.LogInfo($"Updating record ID {existingRecordId} for teacher '{teacherName}' (tagID: {tagID}) with time-out: {timeOut.Value:HH:mm:ss}")
                ElseIf hasTimeIn AndAlso hasTimeOut Then
                    ' A complete record already exists - warn user and prevent duplicate
                    Dim existingRecordQuery As String = "SELECT attendanceID, arrivalTime, departureTime FROM attendance_record WHERE attendanceID = ?"
                    Dim existingCmd As New OdbcCommand(existingRecordQuery, con)
                    existingCmd.Parameters.AddWithValue("?", existingRecordId)
                    Dim existingReader = existingCmd.ExecuteReader()
                    Dim existingTimeIn As String = "N/A"
                    Dim existingTimeOut As String = "N/A"
                    If existingReader.Read() Then
                        If Not IsDBNull(existingReader("arrivalTime")) Then
                            existingTimeIn = Convert.ToDateTime(existingReader("arrivalTime")).ToString("hh:mm tt")
                        End If
                        If Not IsDBNull(existingReader("departureTime")) Then
                            existingTimeOut = Convert.ToDateTime(existingReader("departureTime")).ToString("hh:mm tt")
                        End If
                    End If
                    existingReader.Close()
                    existingCmd.Dispose()

                    Dim warningMessage As String = $"A complete attendance record already exists for '{teacherName}' on {logDate:MMMM dd, yyyy}.{vbCrLf}{vbCrLf}" &
                                                  $"Existing Record:{vbCrLf}" &
                                                  $"Time-In: {existingTimeIn}{vbCrLf}" &
                                                  $"Time-Out: {existingTimeOut}{vbCrLf}{vbCrLf}" &
                                                  $"Do you want to create an additional record? (This may create a duplicate)"

                    If MessageBox.Show(warningMessage, "Record Already Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.No Then
                        btnSave.Enabled = True
                        btnCancel.Enabled = True
                        con.Close()
                        Return
                    End If

                    ' User confirmed - create new record
                    If timeIn.HasValue AndAlso timeOut.HasValue Then
                        ' Calculate Under time/Over time
                        Dim timeRemarks As String = CalculateTimeRemarks(timeOut.Value, shiftEndTime, "")
                        Dim finalRemarks As String = manualInputRemark
                        If Not String.IsNullOrWhiteSpace(timeRemarks) Then
                            finalRemarks = $"{manualInputRemark}; {timeRemarks}"
                        End If

                        Dim insertQuery As String = "INSERT INTO attendance_record (tag_id, teacherID, logDate, arrivalTime, departureTime, arrStatus, depStatus, remarks, manual_input_by, manual_input_reason) " &
                                                   "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)"
                        cmd = New OdbcCommand(insertQuery, con)
                        cmd.Parameters.AddWithValue("?", tagID)
                        cmd.Parameters.AddWithValue("?", teacherID)
                        cmd.Parameters.AddWithValue("?", logDate.ToString("yyyy-MM-dd"))
                        cmd.Parameters.AddWithValue("?", timeIn.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                        cmd.Parameters.AddWithValue("?", timeOut.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                        cmd.Parameters.AddWithValue("?", If(String.IsNullOrEmpty(arrStatusValue), "On Time", arrStatusValue))
                        cmd.Parameters.AddWithValue("?", If(String.IsNullOrEmpty(depStatusValue), "On Time", depStatusValue))
                        cmd.Parameters.AddWithValue("?", finalRemarks)
                        cmd.Parameters.AddWithValue("?", currentUser)
                        cmd.Parameters.AddWithValue("?", manualReason)
                        action = "created"

                        _logger.LogWarning($"Creating additional record for teacher '{teacherName}' (ID: {teacherID}) on {logDate:yyyy-MM-dd} - Complete record already exists (ID: {existingRecordId})")
                    ElseIf timeIn.HasValue Then
                        ' Combine manual input remark with time-in remark (Late)
                        Dim finalRemarks As String = manualInputRemark
                        If Not String.IsNullOrWhiteSpace(timeInRemarks) Then
                            finalRemarks = $"{manualInputRemark}; {timeInRemarks}"
                        End If
                        
                        Dim insertQuery As String = "INSERT INTO attendance_record (tag_id, teacherID, logDate, arrivalTime, arrStatus, remarks, manual_input_by, manual_input_reason) " &
                                                   "VALUES (?, ?, ?, ?, ?, ?, ?, ?)"
                        cmd = New OdbcCommand(insertQuery, con)
                        cmd.Parameters.AddWithValue("?", tagID)
                        cmd.Parameters.AddWithValue("?", teacherID)
                        cmd.Parameters.AddWithValue("?", logDate.ToString("yyyy-MM-dd"))
                        cmd.Parameters.AddWithValue("?", timeIn.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                        cmd.Parameters.AddWithValue("?", If(String.IsNullOrEmpty(arrStatusValue), "On Time", arrStatusValue))
                        cmd.Parameters.AddWithValue("?", finalRemarks)
                        cmd.Parameters.AddWithValue("?", currentUser)
                        cmd.Parameters.AddWithValue("?", manualReason)
                        action = "created"

                        _logger.LogInfo("Creating new time-in record")
                    Else
                        btnSave.Enabled = True
                        btnCancel.Enabled = True
                        MessageBox.Show("Cannot add only time-out when a complete record already exists for this date.", "Invalid Operation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        con.Close()
                        Return
                    End If
                Else
                    btnSave.Enabled = True
                    btnCancel.Enabled = True
                    MessageBox.Show("A record already exists for this teacher on this date. Please review the existing record.", "Record Exists", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    con.Close()
                    Return
                End If
            Else
                ' No existing record found - verify one more time before inserting to prevent race conditions
                Dim doubleCheckQuery As String = "SELECT COUNT(*) AS recordCount FROM attendance_record WHERE teacherID = ? AND DATE(logDate) = ?"
                Dim doubleCheckCmd As New OdbcCommand(doubleCheckQuery, con)
                doubleCheckCmd.Parameters.AddWithValue("?", teacherID)
                doubleCheckCmd.Parameters.AddWithValue("?", logDate.ToString("yyyy-MM-dd"))
                Dim recordCount As Integer = Convert.ToInt32(doubleCheckCmd.ExecuteScalar())
                doubleCheckCmd.Dispose()

                If recordCount > 0 Then
                    btnSave.Enabled = True
                    btnCancel.Enabled = True
                    _logger.LogWarning($"FormManualAttendance - Record was created between check and insert! Found {recordCount} record(s) for teacherID {teacherID} on {logDate:yyyy-MM-dd}")
                    MessageBox.Show($"An attendance record was found for '{teacherName}' on {logDate:MMMM dd, yyyy}. Please refresh and try again.", "Record Already Exists", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    con.Close()
                    Return
                End If

                If timeIn.HasValue AndAlso timeOut.HasValue Then
                    ' Calculate time-in and time-out remarks
                    Dim timeOutRemarks As String = CalculateTimeRemarks(timeOut.Value, shiftEndTime, "")
                    Dim finalRemarks As String = manualInputRemark
                    
                    ' Combine time-in remarks (Late) and time-out remarks (Under/Over time)
                    If Not String.IsNullOrWhiteSpace(timeInRemarks) Then
                        finalRemarks = $"{manualInputRemark}; {timeInRemarks}"
                    End If
                    If Not String.IsNullOrWhiteSpace(timeOutRemarks) Then
                        If finalRemarks.Contains(";") Then
                            finalRemarks = $"{finalRemarks}; {timeOutRemarks}"
                        Else
                            finalRemarks = $"{finalRemarks}; {timeOutRemarks}"
                        End If
                    End If

                    Dim insertQuery As String = "INSERT INTO attendance_record (tag_id, teacherID, logDate, arrivalTime, departureTime, arrStatus, depStatus, remarks, manual_input_by, manual_input_reason) " &
                                               "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)"
                    cmd = New OdbcCommand(insertQuery, con)
                    cmd.Parameters.AddWithValue("?", tagID)
                    cmd.Parameters.AddWithValue("?", teacherID)
                    cmd.Parameters.AddWithValue("?", logDate.ToString("yyyy-MM-dd"))
                    cmd.Parameters.AddWithValue("?", timeIn.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                    cmd.Parameters.AddWithValue("?", timeOut.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                    cmd.Parameters.AddWithValue("?", If(String.IsNullOrEmpty(arrStatusValue), "On Time", arrStatusValue))
                    cmd.Parameters.AddWithValue("?", If(String.IsNullOrEmpty(depStatusValue), "On Time", depStatusValue))
                    cmd.Parameters.AddWithValue("?", finalRemarks)
                    cmd.Parameters.AddWithValue("?", currentUser)
                    cmd.Parameters.AddWithValue("?", manualReason)
                    action = "created"

                    _logger.LogInfo($"FormManualAttendance - Creating new complete record for teacher '{teacherName}' (ID: {teacherID}, tagID: '{tagID}') on {logDate:yyyy-MM-dd}")
                ElseIf timeIn.HasValue Then
                    Dim insertQuery As String = "INSERT INTO attendance_record (tag_id, teacherID, logDate, arrivalTime, arrStatus, remarks, manual_input_by, manual_input_reason) " &
                                               "VALUES (?, ?, ?, ?, ?, ?, ?, ?)"
                    cmd = New OdbcCommand(insertQuery, con)
                    cmd.Parameters.AddWithValue("?", tagID)
                    cmd.Parameters.AddWithValue("?", teacherID)
                    cmd.Parameters.AddWithValue("?", logDate.ToString("yyyy-MM-dd"))
                    cmd.Parameters.AddWithValue("?", timeIn.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                    cmd.Parameters.AddWithValue("?", If(String.IsNullOrEmpty(arrStatusValue), "On Time", arrStatusValue))
                    cmd.Parameters.AddWithValue("?", manualInputRemark)
                    cmd.Parameters.AddWithValue("?", currentUser)
                    cmd.Parameters.AddWithValue("?", manualReason)
                    action = "created"

                    _logger.LogInfo($"FormManualAttendance - Creating new time-in only record for teacher '{teacherName}' (ID: {teacherID}, tagID: '{tagID}') on {logDate:yyyy-MM-dd}")
                Else
                    btnSave.Enabled = True
                    btnCancel.Enabled = True
                    MessageBox.Show("Cannot add only time-out without an existing time-in record.", "Invalid Operation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    con.Close()
                    Return
                End If

                _logger.LogInfo("No existing record found, creating new record")
            End If

            Dim rowsAffected As Integer = cmd.ExecuteNonQuery()
            cmd.Dispose()

            If rowsAffected = 0 Then
                btnSave.Enabled = True
                btnCancel.Enabled = True
                _logger.LogError($"FormManualAttendance - Failed to {action} attendance record. No rows affected.")
                MessageBox.Show("Failed to save attendance record. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                con.Close()
                Return
            End If

            ' Additional validation: Check if duplicate was created (should not happen, but verify)
            If action = "created" Then
                ' Wait a moment for database to commit
                System.Threading.Thread.Sleep(100)

                ' Check how many records exist now for this teacher on this date
                Dim verifyQuery As String = "SELECT COUNT(*) AS recordCount FROM attendance_record WHERE teacherID = ? AND DATE(logDate) = ?"
                Dim verifyCmd As New OdbcCommand(verifyQuery, con)
                verifyCmd.Parameters.AddWithValue("?", teacherID)
                verifyCmd.Parameters.AddWithValue("?", logDate.ToString("yyyy-MM-dd"))
                Dim recordCount As Integer = Convert.ToInt32(verifyCmd.ExecuteScalar())
                verifyCmd.Dispose()

                _logger.LogInfo($"FormManualAttendance - After {action}, found {recordCount} total record(s) for teacher '{teacherName}' (ID: {teacherID}) on {logDate:yyyy-MM-dd}")

                If recordCount > (existingRecords.Count + 1) Then
                    _logger.LogWarning($"FormManualAttendance - WARNING: Expected {existingRecords.Count + 1} record(s), but found {recordCount}. Possible duplicate detected!")
                    MessageBox.Show($"Warning: Multiple attendance records found for this teacher on this date. Please check the attendance list.", "Duplicate Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                End If
            End If

            Dim auditAction As String = If(action = "updated", "Update", "Create")
            If action = "updated" Then
                _auditLogger.LogUpdate(MainForm.currentUsername, "Attendance",
                    $"Manual time-out added - Teacher: '{teacherName}', Date: {logDate:yyyy-MM-dd}, Time-Out: {timeOut.Value.ToString("HH:mm")}")
            Else
                _auditLogger.LogCreate(MainForm.currentUsername, "Attendance",
                    $"Manual attendance entry - Teacher: '{teacherName}', Date: {logDate:yyyy-MM-dd}, Time-In: {If(timeIn.HasValue, timeIn.Value.ToString("HH:mm"), "N/A")}, Time-Out: {If(timeOut.HasValue, timeOut.Value.ToString("HH:mm"), "N/A")}")
            End If

            _logger.LogInfo($"FormManualAttendance - Manual attendance {action} for teacher '{teacherName}' (ID: {teacherID}, tagID: '{tagID}') on {logDate:yyyy-MM-dd} - Rows affected: {rowsAffected}")

            Dim successMessage As String = If(action = "updated", "Time-out added successfully!", "Manual attendance record created successfully!")
            MessageBox.Show(successMessage, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)

            Me.DialogResult = DialogResult.OK
            Me.Close()

        Catch ex As Exception
            btnSave.Enabled = True
            btnCancel.Enabled = True
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

    Private Function FormatTimeSpan(totalMinutes As Integer) As String
        If totalMinutes < 60 Then
            ' Less than 1 hour - show minutes only
            Return $"{totalMinutes} min{If(totalMinutes > 1, "s", "")}"
        Else
            Dim hours As Integer = totalMinutes \ 60
            Dim minutes As Integer = totalMinutes Mod 60
            
            If minutes = 0 Then
                ' Exact hours
                Return $"{hours} hour{If(hours > 1, "s", "")}"
            Else
                ' Hours and minutes
                Return $"{hours} h {minutes} min{If(minutes > 1, "s", "")}"
            End If
        End If
    End Function

    Private Function CalculateTimeInRemarks(arrivalTime As DateTime, shiftStartTime As Object) As String
        Try
            ' If no shift start time is set, no remark
            If shiftStartTime Is Nothing OrElse IsDBNull(shiftStartTime) Then
                _logger.LogDebug("No shift start time set, no time-in remark")
                Return ""
            End If
            
            ' Parse shift start time
            Dim shiftStart As TimeSpan
            If TypeOf shiftStartTime Is TimeSpan Then
                shiftStart = CType(shiftStartTime, TimeSpan)
            ElseIf TypeOf shiftStartTime Is DateTime Then
                shiftStart = CType(shiftStartTime, DateTime).TimeOfDay
            Else
                TimeSpan.TryParse(shiftStartTime.ToString(), shiftStart)
            End If
            
            ' Get the time portion of arrival
            Dim arrivalTimeOnly As TimeSpan = arrivalTime.TimeOfDay
            
            ' Calculate the difference in minutes
            Dim timeDifference As Integer = CInt((arrivalTimeOnly - shiftStart).TotalMinutes)
            
            Dim timeRemarks As String = ""
            
            If timeDifference > 0 Then
                ' Arrived late
                Dim minutesLate As Integer = timeDifference
                Dim formattedTime As String = FormatTimeSpan(minutesLate)
                timeRemarks = $"Late ({formattedTime})"
                _logger.LogInfo($"Late arrival detected: {minutesLate} minutes late (Arrival: {arrivalTimeOnly}, Shift Start: {shiftStart})")
            Else
                ' On time or early
                _logger.LogInfo($"On time arrival (Arrival: {arrivalTimeOnly}, Shift Start: {shiftStart})")
            End If
            
            Return timeRemarks
            
        Catch ex As Exception
            _logger.LogError($"Error calculating time-in remarks: {ex.Message}")
            Return ""
        End Try
    End Function

    Private Function CalculateTimeRemarks(departureTime As DateTime, shiftEndTime As Object, existingRemarks As String) As String
        Try
            ' If no shift end time is set, return existing remarks
            If shiftEndTime Is Nothing OrElse IsDBNull(shiftEndTime) Then
                _logger.LogDebug("No shift end time set, returning existing remarks")
                Return existingRemarks
            End If
            
            ' Parse shift end time
            Dim shiftEnd As TimeSpan
            If TypeOf shiftEndTime Is TimeSpan Then
                shiftEnd = CType(shiftEndTime, TimeSpan)
            ElseIf TypeOf shiftEndTime Is DateTime Then
                shiftEnd = CType(shiftEndTime, DateTime).TimeOfDay
            Else
                TimeSpan.TryParse(shiftEndTime.ToString(), shiftEnd)
            End If
            
            ' Get the time portion of departure
            Dim departureTimeOnly As TimeSpan = departureTime.TimeOfDay
            
            ' Calculate the difference in minutes
            Dim timeDifference As Integer = CInt((departureTimeOnly - shiftEnd).TotalMinutes)
            
            Dim timeRemarks As String = ""
            
            If timeDifference < 0 Then
                ' Left early - Under time
                Dim minutesEarly As Integer = Math.Abs(timeDifference)
                Dim formattedTime As String = FormatTimeSpan(minutesEarly)
                timeRemarks = $"Under time ({formattedTime})"
                _logger.LogInfo($"Under time detected: {minutesEarly} minutes early (Departure: {departureTimeOnly}, Shift End: {shiftEnd})")
            ElseIf timeDifference > 0 Then
                ' Stayed late - Over time
                Dim minutesLate As Integer = timeDifference
                Dim formattedTime As String = FormatTimeSpan(minutesLate)
                timeRemarks = $"Over time ({formattedTime})"
                _logger.LogInfo($"Over time detected: {minutesLate} minutes late (Departure: {departureTimeOnly}, Shift End: {shiftEnd})")
            Else
                ' Exactly on time
                _logger.LogInfo($"On time departure (Departure: {departureTimeOnly}, Shift End: {shiftEnd})")
            End If
            
            ' Combine with existing remarks if any
            If Not String.IsNullOrWhiteSpace(existingRemarks) AndAlso Not String.IsNullOrWhiteSpace(timeRemarks) Then
                Return existingRemarks & "; " & timeRemarks
            ElseIf Not String.IsNullOrWhiteSpace(timeRemarks) Then
                Return timeRemarks
            Else
                Return existingRemarks
            End If
            
        Catch ex As Exception
            _logger.LogError($"Error calculating time remarks: {ex.Message}")
            Return existingRemarks
        End Try
    End Function

    Private Sub chkTimeIn_CheckedChanged(sender As Object, e As EventArgs) Handles chkTimeIn.CheckedChanged
        dtpTimeIn.Enabled = chkTimeIn.Checked
    End Sub

    Private Sub chkTimeOut_CheckedChanged(sender As Object, e As EventArgs) Handles chkTimeOut.CheckedChanged
        dtpTimeOut.Enabled = chkTimeOut.Checked
    End Sub
End Class
