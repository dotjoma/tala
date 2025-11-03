Imports System.Data.Odbc

Public Class FormEditAttendance
    Private ReadOnly _logger As ILogger = LoggerFactory.Instance
    
    Public Property AttendanceId As Integer
    Public Property TeacherName As String
    Public Property AttendanceDate As Date
    Public Property TimeIn As DateTime?
    Public Property TimeOut As DateTime?
    
    Private Sub FormEditAttendance_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Set form title and labels
        Me.Text = "Edit Attendance Record"
        lblTeacherName.Text = $"Teacher: {TeacherName}"
        lblDate.Text = $"Date: {AttendanceDate:MMMM dd, yyyy}"
        
        ' Set time pickers
        ' Ensure ShowCheckBox is enabled
        dtpTimeIn.ShowCheckBox = True
        dtpTimeOut.ShowCheckBox = True
        
        If TimeIn.HasValue Then
            dtpTimeIn.Value = TimeIn.Value
            dtpTimeIn.Checked = True
        Else
            dtpTimeIn.Value = DateTime.Now ' Set a valid default value first
            dtpTimeIn.Checked = False ' Then uncheck it
        End If
        
        If TimeOut.HasValue Then
            dtpTimeOut.Value = TimeOut.Value
            dtpTimeOut.Checked = True
            _logger.LogDebug($"TimeOut checkbox: CHECKED, value: {TimeOut.Value}")
        Else
            dtpTimeOut.Value = DateTime.Now ' Set a valid default value first
            dtpTimeOut.Checked = False ' Then uncheck it
            _logger.LogDebug("TimeOut checkbox: UNCHECKED (no time out yet)")
        End If
        
        _logger.LogInfo($"FormEditAttendance loaded for attendance ID: {AttendanceId} - TimeIn checked: {dtpTimeIn.Checked}, TimeOut checked: {dtpTimeOut.Checked}")
    End Sub
    
    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            ' Validate that at least one time is set
            If Not dtpTimeIn.Checked AndAlso Not dtpTimeOut.Checked Then
                MessageBox.Show("Please set at least Time In or Time Out.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            
            ' Confirm the edit
            Dim message As String = $"Are you sure you want to update this attendance record?{vbCrLf}{vbCrLf}" &
                                   $"Teacher: {TeacherName}{vbCrLf}" &
                                   $"Date: {AttendanceDate:MMMM dd, yyyy}{vbCrLf}" &
                                   $"Time In: {If(dtpTimeIn.Checked, dtpTimeIn.Value.ToString("hh:mm tt"), "Not Set")}{vbCrLf}" &
                                   $"Time Out: {If(dtpTimeOut.Checked, dtpTimeOut.Value.ToString("hh:mm tt"), "Not Set")}"
            
            If MessageBox.Show(message, "Confirm Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.No Then
                Return
            End If
            
            ' Update the database
            connectDB()
            
            Dim query As String = "UPDATE attendance_record SET "
            Dim params As New List(Of Object)
            Dim updates As New List(Of String)
            
            ' Build the SET clause dynamically
            If dtpTimeIn.Checked Then
                updates.Add("arrivalTime = ?")
                ' Combine the attendance date with the selected time
                Dim fullDateTime As DateTime = New DateTime(AttendanceDate.Year, AttendanceDate.Month, AttendanceDate.Day,
                                                           dtpTimeIn.Value.Hour, dtpTimeIn.Value.Minute, dtpTimeIn.Value.Second)
                params.Add(fullDateTime.ToString("yyyy-MM-dd HH:mm:ss"))
                _logger.LogDebug($"Setting arrivalTime to: {fullDateTime:yyyy-MM-dd HH:mm:ss}")
            Else
                updates.Add("arrivalTime = NULL")
                _logger.LogDebug("Setting arrivalTime to NULL")
            End If
            
            If dtpTimeOut.Checked Then
                updates.Add("departureTime = ?")
                ' Combine the attendance date with the selected time
                Dim fullDateTime As DateTime = New DateTime(AttendanceDate.Year, AttendanceDate.Month, AttendanceDate.Day,
                                                           dtpTimeOut.Value.Hour, dtpTimeOut.Value.Minute, dtpTimeOut.Value.Second)
                params.Add(fullDateTime.ToString("yyyy-MM-dd HH:mm:ss"))
                _logger.LogDebug($"Setting departureTime to: {fullDateTime:yyyy-MM-dd HH:mm:ss}")
            Else
                updates.Add("departureTime = NULL")
                _logger.LogDebug("Setting departureTime to NULL")
            End If
            
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
                ' Log the audit trail
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
    
    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub
End Class
