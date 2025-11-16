Imports System.Data.Odbc

Public Class FormEditAttendance
    Private ReadOnly _logger As ILogger = LoggerFactory.Instance
    
    Public Property AttendanceId As Integer
    Public Property TeacherName As String
    Public Property AttendanceDate As Date
    Public Property TimeIn As DateTime?
    Public Property TimeOut As DateTime?
    Public Property CurrentRemarks As String = ""

    Private Sub FormEditAttendance_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Edit Attendance Record"
        lblTeacherName.Text = $"Teacher: {TeacherName}"
        lblDate.Text = $"Date: {AttendanceDate:MMMM dd, yyyy}"

        dtpTimeIn.ShowCheckBox = True
        dtpTimeOut.ShowCheckBox = True

        If TimeIn.HasValue Then
            dtpTimeIn.Value = TimeIn.Value
            dtpTimeIn.Checked = True
        Else
            dtpTimeIn.Value = DateTime.Now
            dtpTimeIn.Checked = False
        End If

        If TimeOut.HasValue Then
            dtpTimeOut.Value = TimeOut.Value
            dtpTimeOut.Checked = True
            _logger.LogDebug($"TimeOut checkbox: CHECKED, value: {TimeOut.Value}")
        Else
            dtpTimeOut.Value = DateTime.Now
            dtpTimeOut.Checked = False
            _logger.LogDebug("TimeOut checkbox: UNCHECKED (no time out yet)")
        End If
        
        ' Display existing remarks (read-only)
        If Not String.IsNullOrEmpty(CurrentRemarks) Then
            txtExistingRemarks.Text = CurrentRemarks
        Else
            txtExistingRemarks.Text = "(No remarks)"
        End If

        _logger.LogInfo($"FormEditAttendance loaded for attendance ID: {AttendanceId} - TimeIn checked: {dtpTimeIn.Checked}, TimeOut checked: {dtpTimeOut.Checked}")
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            If Not dtpTimeIn.Checked AndAlso Not dtpTimeOut.Checked Then
                MessageBox.Show("Please set at least Time In or Time Out.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim message As String = $"Are you sure you want to update this attendance record?{vbCrLf}{vbCrLf}" &
                                   $"Teacher: {TeacherName}{vbCrLf}" &
                                   $"Date: {AttendanceDate:MMMM dd, yyyy}{vbCrLf}" &
                                   $"Time In: {If(dtpTimeIn.Checked, dtpTimeIn.Value.ToString("hh:mm tt"), "Not Set")}{vbCrLf}" &
                                   $"Time Out: {If(dtpTimeOut.Checked, dtpTimeOut.Value.ToString("hh:mm tt"), "Not Set")}"

            If MessageBox.Show(message, "Confirm Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.No Then
                Return
            End If

            connectDB()
            
            ' Get teacher ID for incomplete record checking
            Dim teacherID As Integer = 0
            Dim getTeacherIDCmd As New OdbcCommand("SELECT teacherID FROM attendance_record WHERE attendanceID = ?", con)
            getTeacherIDCmd.Parameters.AddWithValue("?", AttendanceId)
            Dim teacherIDReader = getTeacherIDCmd.ExecuteReader()
            If teacherIDReader.Read() Then
                teacherID = Convert.ToInt32(teacherIDReader("teacherID"))
            End If
            teacherIDReader.Close()
            
            ' Check for incomplete records from previous days (no time-out) if adding time-in
            If dtpTimeIn.Checked AndAlso teacherID > 0 Then
                Dim checkIncompleteCmd As New OdbcCommand("SELECT attendanceID, logDate, remarks FROM attendance_record WHERE teacherID = ? AND departureTime IS NULL AND DATE(logDate) < ? AND attendanceID != ?", con)
                checkIncompleteCmd.Parameters.AddWithValue("?", teacherID)
                checkIncompleteCmd.Parameters.AddWithValue("?", AttendanceDate.ToString("yyyy-MM-dd"))
                checkIncompleteCmd.Parameters.AddWithValue("?", AttendanceId)
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

            Dim query As String = "UPDATE attendance_record SET "
            Dim params As New List(Of Object)
            Dim updates As New List(Of String)

            If dtpTimeIn.Checked Then
                updates.Add("arrivalTime = ?")
                Dim fullDateTime As DateTime = New DateTime(AttendanceDate.Year, AttendanceDate.Month, AttendanceDate.Day,
                                                           dtpTimeIn.Value.Hour, dtpTimeIn.Value.Minute, dtpTimeIn.Value.Second)
                params.Add(fullDateTime.ToString("yyyy-MM-dd HH:mm:ss"))
                _logger.LogDebug($"Setting arrivalTime to: {fullDateTime:yyyy-MM-dd HH:mm:ss}")
            Else
                updates.Add("arrivalTime = NULL")
                _logger.LogDebug("Setting arrivalTime to NULL")
            End If

            Dim departureTime As DateTime? = Nothing
            If dtpTimeOut.Checked Then
                updates.Add("departureTime = ?")
                Dim fullDateTime As DateTime = New DateTime(AttendanceDate.Year, AttendanceDate.Month, AttendanceDate.Day,
                                                           dtpTimeOut.Value.Hour, dtpTimeOut.Value.Minute, dtpTimeOut.Value.Second)
                departureTime = fullDateTime
                params.Add(fullDateTime.ToString("yyyy-MM-dd HH:mm:ss"))
                _logger.LogDebug($"Setting departureTime to: {fullDateTime:yyyy-MM-dd HH:mm:ss}")
            Else
                updates.Add("departureTime = NULL")
                _logger.LogDebug("Setting departureTime to NULL")
            End If

            ' Get teacher's shift end time for Under time/Over time calculation
            Dim shiftEndTime As Object = Nothing
            Dim getTeacherCmd As New OdbcCommand("SELECT ti.teacherID, ti.shift_end_time FROM attendance_record ar JOIN teacherinformation ti ON ar.teacherID = ti.teacherID WHERE ar.attendanceID = ?", con)
            getTeacherCmd.Parameters.AddWithValue("?", AttendanceId)
            Dim teacherReader = getTeacherCmd.ExecuteReader()
            If teacherReader.Read() Then
                teacherID = Convert.ToInt32(teacherReader("teacherID"))
                If Not IsDBNull(teacherReader("shift_end_time")) Then
                    shiftEndTime = teacherReader("shift_end_time")
                End If
            End If
            teacherReader.Close()
            
            ' Build remarks: Calculate Under/Over time, then add the latest action (Edit) as the final part
            Dim editReason As String = txtRemarks.Text.Trim()
            Dim finalRemarks As String = ""
            
            ' Calculate Under/Over time if time-out is provided (existing action parts are ignored in calculation)
            If departureTime.HasValue AndAlso shiftEndTime IsNot Nothing Then
                Dim timeRemarks As String = CalculateTimeRemarks(departureTime.Value, shiftEndTime, CurrentRemarks)
                finalRemarks = timeRemarks
            Else
                ' No time-out or shift time - keep only non-action custom remarks
                finalRemarks = CalculateTimeRemarks(DateTime.MinValue, Nothing, CurrentRemarks)
            End If
            
            ' Append the latest action (Edit) last
            If Not String.IsNullOrEmpty(editReason) Then
                Dim editRemark As String = $"Edit: {editReason}"
                If Not String.IsNullOrEmpty(finalRemarks) Then
                    finalRemarks = $"{finalRemarks}; {editRemark}"
                Else
                    finalRemarks = editRemark
                End If
            End If
            
            ' Always update remarks
            updates.Add("remarks = ?")
            params.Add(If(String.IsNullOrEmpty(finalRemarks), DBNull.Value, finalRemarks))
            
            ' Track who edited and when
            updates.Add("edited_by = ?")
            updates.Add("edited_at = NOW()")
            params.Add(MainForm.currentUsername)

            query &= String.Join(", ", updates)
            query &= " WHERE attendanceID = ?"
            params.Add(AttendanceId)

            _logger.LogDebug($"Update query: {query}")

            Dim cmd As New OdbcCommand(query, con)
            For Each param In params
                cmd.Parameters.AddWithValue("?", param)
            Next

            Dim rowsAffected As Integer = cmd.ExecuteNonQuery()

            If rowsAffected > 0 Then
                Dim userName As String = MainForm.currentUsername
                AuditLogger.Instance.LogUpdate(userName, "Attendance",
                    $"Updated attendance record ID {AttendanceId} for {TeacherName} on {AttendanceDate:yyyy-MM-dd}")

                _logger.LogInfo($"Attendance record {AttendanceId} updated successfully by {userName}")
                MessageBox.Show("Attendance record updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Me.DialogResult = DialogResult.OK
                Me.Close()
            Else
                MessageBox.Show("Failed to update attendance record.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If

        Catch ex As Exception
            _logger.LogError($"Error updating attendance record: {ex.Message}")
            MessageBox.Show("Error updating attendance record: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If con.State = ConnectionState.Open Then con.Close()
        End Try
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

    Private Function CalculateTimeRemarks(departureTime As DateTime, shiftEndTime As Object, existingRemarks As String) As String
        Try
            ' If departure time is MinValue, just return cleaned existing remarks (no time-out calculation)
            If departureTime = DateTime.MinValue Then
                ' Remove any previous Under/Over time remarks, keep only custom remarks
                If String.IsNullOrWhiteSpace(existingRemarks) Then
                    Return ""
                End If
                
                Dim parts As String() = existingRemarks.Split(New String() {"; "}, StringSplitOptions.RemoveEmptyEntries)
                Dim cleanedParts As New List(Of String)
                
                For Each part As String In parts
                    ' Keep only parts that are NOT Under time, Over time, or Edit remarks
                    If Not part.StartsWith("Under time") AndAlso Not part.StartsWith("Over time") AndAlso Not part.StartsWith("Edit:") Then
                        cleanedParts.Add(part)
                    End If
                Next
                
                Return String.Join("; ", cleanedParts)
            End If
            
            ' If no shift end time is set, return cleaned existing remarks
            If shiftEndTime Is Nothing OrElse IsDBNull(shiftEndTime) Then
                _logger.LogDebug("No shift end time set, returning cleaned existing remarks")
                ' Remove previous Under/Over time remarks
                If String.IsNullOrWhiteSpace(existingRemarks) Then
                    Return ""
                End If
                
                Dim parts As String() = existingRemarks.Split(New String() {"; "}, StringSplitOptions.RemoveEmptyEntries)
                Dim cleanedParts As New List(Of String)
                
                For Each part As String In parts
                    If Not part.StartsWith("Under time") AndAlso Not part.StartsWith("Over time") AndAlso Not part.StartsWith("Edit:") Then
                        cleanedParts.Add(part)
                    End If
                Next
                
                Return String.Join("; ", cleanedParts)
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
            
            ' Clean existing remarks - remove old Under/Over time and Edit remarks
            Dim cleanedExisting As String = ""
            If Not String.IsNullOrWhiteSpace(existingRemarks) Then
                Dim parts As String() = existingRemarks.Split(New String() {"; "}, StringSplitOptions.RemoveEmptyEntries)
                Dim cleanedParts As New List(Of String)
                
                For Each part As String In parts
                    If Not part.StartsWith("Under time") AndAlso Not part.StartsWith("Over time") AndAlso Not part.StartsWith("Edit:") Then
                        cleanedParts.Add(part)
                    End If
                Next
                
                If cleanedParts.Count > 0 Then
                    cleanedExisting = String.Join("; ", cleanedParts)
                End If
            End If
            
            ' Combine cleaned existing with new time remarks
            If Not String.IsNullOrWhiteSpace(cleanedExisting) AndAlso Not String.IsNullOrWhiteSpace(timeRemarks) Then
                Return cleanedExisting & "; " & timeRemarks
            ElseIf Not String.IsNullOrWhiteSpace(timeRemarks) Then
                Return timeRemarks
            Else
                Return cleanedExisting
            End If
            
        Catch ex As Exception
            _logger.LogError($"Error calculating time remarks: {ex.Message}")
            Return existingRemarks
        End Try
    End Function

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub
End Class
