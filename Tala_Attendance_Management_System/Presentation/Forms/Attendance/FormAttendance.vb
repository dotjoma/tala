Imports System.Data.Odbc

Public Class FormAttendace
    Private ReadOnly _logger As ILogger = LoggerFactory.Instance
    Public strFilter As String = ""
    Private selectedAttendanceId As Integer = 0
    Public Property UserRole As String = ""
    
    ' Shared instance reference for real-time updates
    Public Shared CurrentInstance As FormAttendace = Nothing

    Public Sub DefaultSettings()
        dgvAttendance.CurrentCell = Nothing

        ' Setup DataGridView
        dgvAttendance.AutoGenerateColumns = False
        dgvAttendance.AllowUserToAddRows = False
        dgvAttendance.ReadOnly = True
        dgvAttendance.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        ' Use styles defined in Designer; avoid overriding at runtime

        LoadAttendanceData()

        cbFilter.SelectedIndex = 0
    End Sub
    
    Private Sub LoadAttendanceData()
        Try
            Dim dateFrom As String = dtpDateFrom.Value.ToString("yyyy-MM-dd")
            Dim dateTo As String = dtpDateTo.Value.ToString("yyyy-MM-dd")
            
            Dim query As String = "SELECT ar.attendanceID, CONCAT(firstname, ' ', lastname) AS NAME, 
                     DATE_FORMAT(logDate, '%M %d, %Y') AS logDate, 
                     DATE_FORMAT(arrivalTime, '%h:%i:%s %p') AS arrivalTime, arrStatus, 
                     DATE_FORMAT(departureTime, '%h:%i:%s %p') AS departureTime, depStatus 
                     FROM attendance_record ar 
                     JOIN teacherinformation ti ON ar.tag_id = ti.tagID 
                     WHERE logDate BETWEEN '" & dateFrom & "' AND '" & dateTo & "'"
            
            ' Add search filter if provided
            If txtSearch.Text.Trim().Length > 0 Then
                query &= " AND (ti.firstname LIKE '%" & txtSearch.Text.Trim() & "%' OR ti.lastname LIKE '%" & txtSearch.Text.Trim() & "%')"
            End If
            
            query &= " ORDER BY logDate DESC, arrivalTime DESC"
            
            loadDGV(query, dgvAttendance)
            
            _logger.LogInfo($"Loaded {dgvAttendance.Rows.Count} attendance records from {dateFrom} to {dateTo}")

            ' Log raw data for each record for debugging (one line per record)
            If TypeOf dgvAttendance.DataSource Is DataTable Then
                Dim dt As DataTable = CType(dgvAttendance.DataSource, DataTable)
                For i As Integer = 0 To dt.Rows.Count - 1
                    Dim row = dt.Rows(i)
                    _logger.LogDebug($"Record #{i + 1}: {{ID={If(IsDBNull(row("attendanceID")), "NULL", row("attendanceID").ToString())}, NAME={If(IsDBNull(row("NAME")), "NULL", row("NAME").ToString())}, logDate={If(IsDBNull(row("logDate")), "NULL", row("logDate").ToString())}, arrivalTime={If(IsDBNull(row("arrivalTime")), "NULL", row("arrivalTime").ToString())}, arrStatus={If(IsDBNull(row("arrStatus")), "NULL", row("arrStatus").ToString())}, departureTime={If(IsDBNull(row("departureTime")), "NULL", row("departureTime").ToString())}, depStatus={If(IsDBNull(row("depStatus")), "NULL", row("depStatus").ToString())}}}")
                Next
            End If

        Catch ex As Exception
            _logger.LogError($"Error loading attendance data: {ex.Message}")
            Dim msgForm As New Form() With {.TopMost = True}
            MessageBox.Show(msgForm, "Error loading attendance data: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub FormAttendance_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Set current instance for real-time updates
        CurrentInstance = Me
        
        ' Temporarily remove event handlers to prevent multiple LoadAttendanceData calls
        RemoveHandler dtpDateFrom.ValueChanged, AddressOf dtpDateFrom_ValueChanged
        RemoveHandler dtpDateTo.ValueChanged, AddressOf dtpDateTo_ValueChanged

        ' Set date constraints - prevent future dates
        dtpDateFrom.MaxDate = DateTime.Today
        dtpDateTo.MaxDate = DateTime.Today

        ' Set reasonable minimum date (e.g., 5 years back)
        dtpDateFrom.MinDate = DateTime.Today.AddYears(-5)
        dtpDateTo.MinDate = DateTime.Today.AddYears(-5)

        ' Set default date range to today
        dtpDateFrom.Value = DateTime.Today
        dtpDateTo.Value = DateTime.Today

        ' Re-attach event handlers
        AddHandler dtpDateFrom.ValueChanged, AddressOf dtpDateFrom_ValueChanged
        AddHandler dtpDateTo.ValueChanged, AddressOf dtpDateTo_ValueChanged

        DefaultSettings()
        ApplyRoleBasedAccess()

        ' Use styles from Designer for row colors

        _logger.LogInfo("FormAttendance loaded")
    End Sub
    
    Private Sub FormAttendance_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        ' Clear instance reference when form closes
        If CurrentInstance Is Me Then
            CurrentInstance = Nothing
        End If
    End Sub
    
    ' Public method to refresh data from external sources (like RFID scanner)
    Public Sub RefreshAttendanceData()
        If InvokeRequired Then
            Invoke(New Action(AddressOf RefreshAttendanceData))
        Else
            LoadAttendanceData()
            _logger.LogInfo("Attendance data refreshed from external trigger")
        End If
    End Sub

    Private Sub ApplyRoleBasedAccess()
        Try
            ' Try multiple sources for user role
            Dim userRole As String = ""

            ' 1. Check if role was set directly on this form
            If Not String.IsNullOrEmpty(Me.UserRole) Then
                userRole = Me.UserRole.ToLower()
                ' 2. Check MainForm.currentUserRole
            ElseIf Not String.IsNullOrEmpty(MainForm.currentUserRole) Then
                userRole = MainForm.currentUserRole.ToLower()
                ' 3. Try to parse from MainForm.lblUser label text
            ElseIf MainForm.lblUser IsNot Nothing AndAlso Not String.IsNullOrEmpty(MainForm.lblUser.Text) Then
                Dim labelText As String = MainForm.lblUser.Text
                If labelText.Contains("(Admin)") Then
                    userRole = "admin"
                ElseIf labelText.Contains("(HR)") Then
                    userRole = "hr"
                ElseIf labelText.Contains("(Attendance)") Then
                    userRole = "attendance"
                End If
            End If

            ' Edit button disabled - manual editing of records not allowed
            btnEdit.Visible = False
            btnEdit.Enabled = False

            ' Manual input button enabled for admin and hr roles only
            Dim canManualInput As Boolean = (userRole = "admin" OrElse userRole = "hr")
            btnManualInput.Visible = canManualInput
            btnManualInput.Enabled = canManualInput

            _logger.LogInfo($"Role-based access applied - User role: '{userRole}', Edit button disabled, Manual input {If(canManualInput, "enabled", "disabled")}")
        Catch ex As Exception
            _logger.LogError($"Error applying role-based access: {ex.Message}")
            ' Default to hiding edit button if there's an error
            btnEdit.Visible = False
            btnEdit.Enabled = False
        End Try
    End Sub

    Private Sub txtSearch_TextChanged(sender As Object, e As EventArgs) Handles txtSearch.TextChanged
        UpdateDataBasedOnFilterAndSearch()
    End Sub

    Private Sub dgvAttendance_DataBindingComplete(sender As Object, e As DataGridViewBindingCompleteEventArgs) Handles dgvAttendance.DataBindingComplete
        dgvAttendance.CurrentCell = Nothing
    End Sub

    Private Sub cbFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbFilter.SelectedIndexChanged
        UpdateDataBasedOnFilterAndSearch()
    End Sub
    Private Sub UpdateDataBasedOnFilterAndSearch()
        Dim strFilter As String = cbFilter.SelectedItem.ToString()

        Try
            Select Case strFilter
                Case "All"
                    LoadAllRecords()
                Case "Teachers"
                    LoadTeachersRecords()
                Case "Students"
                    LoadStudentsRecords()
                Case Else
                    DefaultSettings()
            End Select
        Catch ex As Exception
            Dim msgForm As New Form() With {.TopMost = True}
            MessageBox.Show(msgForm, ex.Message.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub SearchAllRecords()
        Dim cmd As Odbc.OdbcCommand
        Dim da As New Odbc.OdbcDataAdapter
        Dim dt As New DataTable

        Dim sql As String = ""

        ' Construct the SQL query with parameters
        sql = "SELECT ar.attendanceID, CONCAT(firstname, ' ', lastname) AS Name, DATE_FORMAT(logDate, '%M %d, %Y') AS logDate, " &
           "DATE_FORMAT(arrivalTime, '%h:%i:%s %p') AS arrivalTime, arrStatus, " &
           "DATE_FORMAT(departureTime, '%h:%i:%s %p') AS departureTime, depStatus  " &
           "FROM attendance_record ar JOIN teacherinformation ti ON ar.tag_id = ti.tagID " &
           "WHERE logDate = CURDATE() AND (ti.lastname LIKE ? OR ti.firstname LIKE ? OR DATE_FORMAT(ar.logDate, '%Y-%m-%d') LIKE ?)"

        Try
            connectDB()
            cmd = New Odbc.OdbcCommand(sql, con)

            cmd.Parameters.AddWithValue("@lastname", "%" & txtSearch.Text.Trim() & "%")
            cmd.Parameters.AddWithValue("@firstname", "%" & txtSearch.Text.Trim() & "%")
            cmd.Parameters.AddWithValue("@logDate", "%" & txtSearch.Text.Trim() & "%")

            da.SelectCommand = cmd
            da.Fill(dt)
            dgvAttendance.DataSource = dt
            dgvAttendance.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
        Catch ex As Exception
            Dim msgForm As New Form() With {.TopMost = True}
            MessageBox.Show(msgForm, ex.Message.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            con.Close()
            GC.Collect()
        End Try
    End Sub

    Private Sub LoadAllRecords()
        If txtSearch.TextLength > 0 Then
            SearchAllRecords()
        Else
            'loadDGV("SELECT CONCAT(firstname, ' ', lastname) AS Name, 
            '         DATE_FORMAT(logDate, '%M %d, %Y') AS logDate, 
            '         DATE_FORMAT(arrivalTime, '%h:%i:%s %p') AS arrivalTime, 
            '         arrStatus, DATE_FORMAT(departureTime, '%h:%i:%s %p') AS departureTime, 
            '         depStatus,'Student' AS TYPE, 
            '         CONCAT(cr.location, ' ', cr.classroom_name) AS classroom, 
            '         s.subject_name 
            '         FROM attendance_record ar 
            '         JOIN studentrecords sr ON ar.tag_id = sr.tagID 
            '         JOIN classrooms cr ON ar.classroom_id=cr.classroom_id 
            '         JOIN subjects s ON ar.subject_id=s.subject_id 
            '         WHERE logDate = CURDATE() 
            '         UNION ALL 
            '         SELECT CONCAT(firstname, ' ', lastname) AS NAME, 
            '         DATE_FORMAT(logDate, '%M %d, %Y') AS logDate, 
            '         DATE_FORMAT(arrivalTime, '%h:%i:%s %p') AS arrivalTime, 
            '         arrStatus, DATE_FORMAT(departureTime, '%h:%i:%s %p') AS departureTime, 
            '         depStatus,'Teacher' AS TYPE, 
            '         CONCAT(cr.location, ' ', cr.classroom_name) AS classroom, 
            '         s.subject_name 
            '         FROM attendance_record ar 
            '         JOIN teacherinformation ti ON ar.tag_id = ti.tagID 
            '         JOIN classrooms cr ON ar.classroom_id=cr.classroom_id 
            '         JOIN subjects s ON ar.subject_id=s.subject_id 
            '         WHERE logDate = CURDATE()", dgvAttendance)

            loadDGV("SELECT ar.attendanceID, CONCAT(firstname, ' ', lastname) AS NAME, 
                     DATE_FORMAT(logDate, '%M %d, %Y') AS logDate, 
                     DATE_FORMAT(arrivalTime, '%h:%i:%s %p') AS arrivalTime, 
                     arrStatus, DATE_FORMAT(departureTime, '%h:%i:%s %p') AS departureTime, 
                     depStatus 
                     FROM attendance_record ar 
                     JOIN teacherinformation ti ON ar.tag_id = ti.tagID 
                     WHERE logDate = CURDATE()", dgvAttendance)
        End If
    End Sub

    Private Sub LoadTeachersRecords()
        If txtSearch.TextLength > 0 Then
            loadDGV("SELECT ar.attendanceID, CONCAT(firstname, ' ', lastname) AS Name, 
                     DATE_FORMAT(logDate, '%M %d, %Y') AS logDate, 
                     DATE_FORMAT(arrivalTime, '%h:%i:%s %p') AS arrivalTime, 
                     arrStatus, DATE_FORMAT(departureTime, '%h:%i:%s %p') AS departureTime, 
                     depStatus 
                     FROM attendance_record ar 
                     JOIN teacherinformation ti ON ar.tag_id = ti.tagID  
                     WHERE logDate = CURDATE()", dgvAttendance, "lastname", "firstname", "logDate", txtSearch.Text.Trim())
        Else
            loadDGV("SELECT ar.attendanceID, CONCAT(firstname, ' ', lastname) AS Name, 
                     DATE_FORMAT(logDate, '%M %d, %Y') AS logDate, 
                     DATE_FORMAT(arrivalTime, '%h:%i:%s %p') AS arrivalTime, 
                     arrStatus, DATE_FORMAT(departureTime, '%h:%i:%s %p') AS departureTime, 
                     depStatus 
                     FROM attendance_record ar 
                     JOIN teacherinformation ti ON ar.tag_id = ti.tagID  
                     WHERE logDate = CURDATE()", dgvAttendance)
        End If
    End Sub

    Private Sub LoadStudentsRecords()
        If txtSearch.TextLength > 0 Then
            loadDGV("SELECT CONCAT(firstname, ' ', lastname) AS Name, 
                     DATE_FORMAT(logDate, '%M %d, %Y') AS logDate, 
                     DATE_FORMAT(arrivalTime, '%h:%i:%s %p') AS arrivalTime, 
                     arrStatus, 
                     DATE_FORMAT(departureTime, '%h:%i:%s %p') AS departureTime, 
                     depStatus,'Student' AS TYPE,
                     CONCAT(cr.location, ' ', cr.classroom_name) AS classroom, 
                     s.subject_name 
                     FROM attendance_record ar 
                     JOIN studentrecords sr ON ar.tag_id = sr.tagID 
                     JOIN classrooms cr ON ar.classroom_id=cr.classroom_id 
                     JOIN subjects s ON ar.subject_id=s.subject_id 
                     WHERE logDate = CURDATE()", dgvAttendance, "lastname", "firstname", "logDate", txtSearch.Text.Trim())
        Else
            loadDGV("SELECT CONCAT(firstname, ' ', lastname) AS Name, 
                     DATE_FORMAT(logDate, '%M %d, %Y') AS logDate, 
                     DATE_FORMAT(arrivalTime, '%h:%i:%s %p') AS arrivalTime, 
                     arrStatus, DATE_FORMAT(departureTime, '%h:%i:%s %p') AS departureTime, 
                     depStatus,'Student' AS TYPE,
                     CONCAT(cr.location, ' ', cr.classroom_name) AS classroom, 
                     s.subject_name 
                     FROM attendance_record ar 
                     JOIN studentrecords sr ON ar.tag_id = sr.tagID 
                     JOIN classrooms cr ON ar.classroom_id=cr.classroom_id 
                     JOIN subjects s ON ar.subject_id=s.subject_id 
                     WHERE logDate = CURDATE()", dgvAttendance)
        End If
    End Sub

    Private Sub dtpDateFrom_ValueChanged(sender As Object, e As EventArgs) Handles dtpDateFrom.ValueChanged
        ValidateDateRange()
    End Sub

    Private Sub dtpDateTo_ValueChanged(sender As Object, e As EventArgs) Handles dtpDateTo.ValueChanged
        ValidateDateRange()
    End Sub

    Private Sub ValidateDateRange()
        Dim needsCorrection As Boolean = False

        ' Check for future dates (shouldn't happen with MaxDate set, but double-check)
        If dtpDateFrom.Value > DateTime.Today Then
            _logger.LogWarning($"Future date detected in Date From: {dtpDateFrom.Value:yyyy-MM-dd}")
            RemoveHandler dtpDateFrom.ValueChanged, AddressOf dtpDateFrom_ValueChanged
            dtpDateFrom.Value = DateTime.Today
            AddHandler dtpDateFrom.ValueChanged, AddressOf dtpDateFrom_ValueChanged
            needsCorrection = True
        End If

        If dtpDateTo.Value > DateTime.Today Then
            _logger.LogWarning($"Future date detected in Date To: {dtpDateTo.Value:yyyy-MM-dd}")
            RemoveHandler dtpDateTo.ValueChanged, AddressOf dtpDateTo_ValueChanged
            dtpDateTo.Value = DateTime.Today
            AddHandler dtpDateTo.ValueChanged, AddressOf dtpDateTo_ValueChanged
            needsCorrection = True
        End If

        ' Check if date range is valid
        If dtpDateFrom.Value > dtpDateTo.Value Then
            ' Date From is after Date To - auto-correct
            _logger.LogWarning($"Invalid date range detected: From {dtpDateFrom.Value:yyyy-MM-dd} > To {dtpDateTo.Value:yyyy-MM-dd}")

            ' Auto-correct: Set Date To to match Date From
            RemoveHandler dtpDateTo.ValueChanged, AddressOf dtpDateTo_ValueChanged
            dtpDateTo.Value = dtpDateFrom.Value
            AddHandler dtpDateTo.ValueChanged, AddressOf dtpDateTo_ValueChanged

            needsCorrection = True
            _logger.LogInfo($"Auto-corrected date range: Both dates set to {dtpDateFrom.Value:yyyy-MM-dd}")
        End If

        ' Visual feedback if correction was needed
        If needsCorrection Then
            dtpDateFrom.BackColor = Color.LightYellow
            dtpDateTo.BackColor = Color.LightYellow

            ' Reset colors after a short delay
            Dim timer As New Timer With {.Interval = 1000}
            AddHandler timer.Tick, Sub()
                                       dtpDateFrom.BackColor = SystemColors.Window
                                       dtpDateTo.BackColor = SystemColors.Window
                                       timer.Stop()
                                       timer.Dispose()
                                   End Sub
            timer.Start()
        Else
            ' Valid range - ensure normal colors
            dtpDateFrom.BackColor = SystemColors.Window
            dtpDateTo.BackColor = SystemColors.Window
        End If

        ' Warn if date range is too large (more than 1 year)
        Dim daysDiff As Integer = (dtpDateTo.Value - dtpDateFrom.Value).Days
        If daysDiff > 365 Then
            _logger.LogWarning($"Large date range selected: {daysDiff} days")
        End If

        ' Load data with validated range
        LoadAttendanceData()
    End Sub

    Private Sub dgvAttendance_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvAttendance.CellClick
        Try
            _logger.LogDebug($"CellClick event fired - RowIndex: {e.RowIndex}, ColumnIndex: {e.ColumnIndex}")

            If e.RowIndex >= 0 Then
                _logger.LogDebug($"Valid row clicked. Total columns: {dgvAttendance.Columns.Count}")

                ' Check if attendanceID column exists
                Dim hasAttendanceIdColumn As Boolean = dgvAttendance.Columns.Contains("attendanceID")
                _logger.LogDebug($"DataGridView has 'attendanceID' column: {hasAttendanceIdColumn}")

                If hasAttendanceIdColumn AndAlso dgvAttendance.Rows(e.RowIndex).Cells("attendanceID").Value IsNot Nothing Then
                    Dim cellValue = dgvAttendance.Rows(e.RowIndex).Cells("attendanceID").Value
                    _logger.LogDebug($"attendanceID cell value type: {cellValue.GetType().Name}, value: '{cellValue}'")
                    selectedAttendanceId = Convert.ToInt32(cellValue)
                    _logger.LogInfo($"✓ Selected attendance ID from column: {selectedAttendanceId}")
                Else
                    ' Column doesn't exist or value is null - try DataTable
                    If hasAttendanceIdColumn Then
                        _logger.LogWarning("attendanceID cell value is null, trying DataTable fallback")
                    End If

                    _logger.LogDebug($"DataSource type: {If(dgvAttendance.DataSource IsNot Nothing, dgvAttendance.DataSource.GetType().Name, "null")}")

                    If TypeOf dgvAttendance.DataSource Is DataTable Then
                        Dim dt As DataTable = CType(dgvAttendance.DataSource, DataTable)
                        _logger.LogDebug($"DataTable has {dt.Columns.Count} columns, {dt.Rows.Count} rows")
                        _logger.LogDebug($"DataTable columns: {String.Join(", ", dt.Columns.Cast(Of DataColumn)().Select(Function(c) c.ColumnName))}")

                        If dt.Columns.Contains("attendanceID") Then
                            _logger.LogDebug($"DataTable contains 'attendanceID' column")
                            If e.RowIndex < dt.Rows.Count Then
                                Dim dtValue = dt.Rows(e.RowIndex)("attendanceID")
                                _logger.LogDebug($"DataTable attendanceID value type: {dtValue.GetType().Name}, value: '{dtValue}'")
                                selectedAttendanceId = Convert.ToInt32(dtValue)
                                _logger.LogInfo($"✓ Selected attendance ID from DataTable: {selectedAttendanceId}")
                            Else
                                _logger.LogWarning($"Row index {e.RowIndex} is out of range for DataTable with {dt.Rows.Count} rows")
                            End If
                        Else
                            _logger.LogWarning("DataTable does not contain 'attendanceID' column")
                        End If
                    Else
                        _logger.LogWarning("DataSource is not a DataTable")
                    End If
                End If
            Else
                _logger.LogDebug($"Invalid row index: {e.RowIndex}")
            End If
        Catch ex As Exception
            _logger.LogError($"Error selecting attendance record: {ex.Message}")
            _logger.LogError($"Stack trace: {ex.StackTrace}")
        End Try
    End Sub

    Private Sub btnEdit_Click(sender As Object, e As EventArgs) Handles btnEdit.Click
        _logger.LogDebug($"Edit button clicked. Current selectedAttendanceId: {selectedAttendanceId}")

        If selectedAttendanceId = 0 Then
            _logger.LogWarning("No attendance record selected")
            Dim msgForm As New Form() With {.TopMost = True}
            MessageBox.Show(msgForm, "Please select an attendance record to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Try
            _logger.LogInfo($"Opening edit dialog for attendance ID: {selectedAttendanceId}")

            ' Connect to database first
            connectDB()

            ' Get current values
            Dim cmd As New OdbcCommand("SELECT arrivalTime, departureTime, CONCAT(ti.firstname, ' ', ti.lastname) AS teacher_name, logDate 
                                        FROM attendance_record ar 
                                        JOIN teacherinformation ti ON ar.tag_id = ti.tagID 
                                        WHERE ar.attendanceID = ?", con)
            cmd.Parameters.AddWithValue("?", selectedAttendanceId)

            Dim reader = cmd.ExecuteReader()

            If reader.Read() Then
                Dim teacherName As String = reader("teacher_name").ToString()
                Dim logDate As Date = Convert.ToDateTime(reader("logDate"))
                Dim currentTimeIn As Object = reader("arrivalTime")
                Dim currentTimeOut As Object = reader("departureTime")

                _logger.LogDebug($"Retrieved from DB - TimeIn: {If(IsDBNull(currentTimeIn), "NULL", currentTimeIn.ToString())}, TimeOut: {If(IsDBNull(currentTimeOut), "NULL", currentTimeOut.ToString())}")

                reader.Close()

                ' Show edit dialog
                Using editForm As New FormEditAttendance()
                    editForm.AttendanceId = selectedAttendanceId
                    editForm.TeacherName = teacherName
                    editForm.AttendanceDate = logDate

                    ' Handle NULL datetime values - use nullable DateTime or current time as default
                    If Not IsDBNull(currentTimeIn) Then
                        editForm.TimeIn = Convert.ToDateTime(currentTimeIn)
                        _logger.LogDebug($"Setting TimeIn to: {editForm.TimeIn}")
                    Else
                        editForm.TimeIn = Nothing
                        _logger.LogDebug("TimeIn is NULL, setting to Nothing")
                    End If

                    If Not IsDBNull(currentTimeOut) Then
                        editForm.TimeOut = Convert.ToDateTime(currentTimeOut)
                        _logger.LogDebug($"Setting TimeOut to: {editForm.TimeOut}")
                    Else
                        editForm.TimeOut = Nothing
                        _logger.LogDebug("TimeOut is NULL, setting to Nothing")
                    End If

                    editForm.TopMost = True

                    If editForm.ShowDialog() = DialogResult.OK Then
                        LoadAttendanceData()
                        _logger.LogInfo($"Attendance record {selectedAttendanceId} updated successfully")
                    End If
                End Using
            Else
                reader.Close()
                Dim msgForm As New Form() With {.TopMost = True}
                MessageBox.Show(msgForm, "Attendance record not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If

        Catch ex As Exception
            _logger.LogError($"Error editing attendance record: {ex.Message}")
            Dim msgForm As New Form() With {.TopMost = True}
            MessageBox.Show(msgForm, "Error editing attendance record: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

    Private Sub btnManualInput_Click(sender As Object, e As EventArgs) Handles btnManualInput.Click
        Try
            _logger.LogInfo("Manual Input button clicked - Opening FormManualAttendance")

            Using manualForm As New FormManualAttendance()
                manualForm.TopMost = True
                If manualForm.ShowDialog() = DialogResult.OK Then
                    LoadAttendanceData()
                    _logger.LogInfo("Manual attendance input completed successfully")
                End If
            End Using

        Catch ex As Exception
            _logger.LogError($"Error opening manual attendance form: {ex.Message}")
            Dim msgForm As New Form() With {.TopMost = True}
            MessageBox.Show(msgForm, "Error opening manual input form: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnExport_Click(sender As Object, e As EventArgs) Handles btnExport.Click
        Try
            If dgvAttendance.Rows.Count = 0 Then
                Dim msgForm As New Form() With {.TopMost = True}
                MessageBox.Show(msgForm, "No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim saveDialog As New SaveFileDialog()
            saveDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv"
            saveDialog.FilterIndex = 1
            saveDialog.FileName = $"TeacherAttendance_{dtpDateFrom.Value:yyyyMMdd}_to_{dtpDateTo.Value:yyyyMMdd}"

            ' Make dialog topmost
            Dim helper As New NativeWindow()
            helper.AssignHandle(Me.Handle)

            If saveDialog.ShowDialog(helper) = DialogResult.OK Then
                If saveDialog.FilterIndex = 1 Then
                    ' Export to Excel with auto-sized columns
                    ExportToExcel(saveDialog.FileName)
                Else
                    ' Export to CSV
                    ExportToCSV(saveDialog.FileName)
                End If

                Dim msgForm As New Form() With {.TopMost = True}
                MessageBox.Show(msgForm, "Report exported successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
                _logger.LogInfo($"Attendance report exported to: {saveDialog.FileName}")
            End If

        Catch ex As Exception
            _logger.LogError($"Error exporting report: {ex.Message}")
            Dim msgForm As New Form() With {.TopMost = True}
            MessageBox.Show(msgForm, "Error exporting report: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ExportToExcel(filePath As String)
        Try
            ' Try using Excel Interop if available
            Dim excelApp As Object = Nothing
            Dim workbook As Object = Nothing
            Dim worksheet As Object = Nothing

            Try
                excelApp = CreateObject("Excel.Application")
                workbook = excelApp.Workbooks.Add()
                worksheet = workbook.Worksheets(1)

                ' Write headers
                Dim col As Integer = 1
                For Each column As DataGridViewColumn In dgvAttendance.Columns
                    If column.Visible AndAlso column.Name <> "attendanceID" Then
                        worksheet.Cells(1, col).Value = column.HeaderText
                        worksheet.Cells(1, col).Font.Bold = True
                        worksheet.Cells(1, col).Interior.Color = RGB(70, 130, 180) ' SteelBlue
                        worksheet.Cells(1, col).Font.Color = RGB(255, 255, 255) ' White
                        col += 1
                    End If
                Next

                ' Write data
                Dim row As Integer = 2
                For Each dgvRow As DataGridViewRow In dgvAttendance.Rows
                    If dgvRow.IsNewRow Then Continue For

                    col = 1
                    For Each column As DataGridViewColumn In dgvAttendance.Columns
                        If column.Visible AndAlso column.Name <> "attendanceID" Then
                            Dim cellValue = dgvRow.Cells(column.Index).Value
                            worksheet.Cells(row, col).Value = If(cellValue Is Nothing OrElse IsDBNull(cellValue), "", cellValue.ToString())
                            col += 1
                        End If
                    Next
                    row += 1
                Next

                ' Auto-fit all columns
                worksheet.Columns.AutoFit()

                ' Add borders
                Dim lastCol As Integer = col - 1
                Dim lastRow As Integer = row - 1
                Dim range = worksheet.Range(worksheet.Cells(1, 1), worksheet.Cells(lastRow, lastCol))
                range.Borders.LineStyle = 1 ' xlContinuous

                ' Save and close
                workbook.SaveAs(filePath)
                workbook.Close(False)
                excelApp.Quit()

                _logger.LogInfo($"Exported to Excel with auto-sized columns: {filePath}")

            Finally
                ' Clean up COM objects
                If worksheet IsNot Nothing Then System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet)
                If workbook IsNot Nothing Then System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook)
                If excelApp IsNot Nothing Then System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp)
            End Try

        Catch ex As Exception
            _logger.LogWarning($"Excel Interop not available, falling back to CSV: {ex.Message}")
            ' Fallback to CSV if Excel is not installed
            ExportToCSV(filePath.Replace(".xlsx", ".csv"))
        End Try
    End Sub

    Private Sub ExportToCSV(filePath As String)
        ' Use UTF8 with BOM for better Excel compatibility
        Using writer As New System.IO.StreamWriter(filePath, False, New System.Text.UTF8Encoding(True))
            ' Define minimum column widths for better Excel display
            Dim columnWidths As New Dictionary(Of String, Integer) From {
                {"logdate", 20},
                {"Name", 30},
                {"arrivalTime", 15},
                {"arrStatus", 12},
                {"departuretime", 15},
                {"depStatus", 12}
            }

            ' Write headers with padding
            Dim headers As New List(Of String)
            Dim visibleColumns As New List(Of DataGridViewColumn)

            For Each column As DataGridViewColumn In dgvAttendance.Columns
                If column.Visible AndAlso column.Name <> "attendanceID" Then
                    visibleColumns.Add(column)
                    Dim headerText As String = column.HeaderText
                    ' Pad header to minimum width to hint Excel about column size
                    Dim minWidth As Integer = If(columnWidths.ContainsKey(column.DataPropertyName), columnWidths(column.DataPropertyName), 15)
                    headerText = headerText.PadRight(Math.Max(headerText.Length, minWidth))
                    headers.Add(EscapeCsvValue(headerText))
                End If
            Next
            writer.WriteLine(String.Join(",", headers))

            ' Write data with padding
            For Each row As DataGridViewRow In dgvAttendance.Rows
                If row.IsNewRow Then Continue For ' Skip the new row placeholder

                Dim values As New List(Of String)
                For Each column In visibleColumns
                    Dim cellValue = row.Cells(column.Index).Value
                    Dim value As String = If(cellValue Is Nothing OrElse IsDBNull(cellValue), "", cellValue.ToString().Trim())

                    ' Pad value to match header width for consistent column sizing
                    Dim minWidth As Integer = If(columnWidths.ContainsKey(column.DataPropertyName), columnWidths(column.DataPropertyName), 15)
                    value = value.PadRight(Math.Max(value.Length, minWidth))

                    values.Add(EscapeCsvValue(value))
                Next
                writer.WriteLine(String.Join(",", values))
            Next
        End Using

        _logger.LogInfo($"Exported {dgvAttendance.Rows.Count} records to CSV: {filePath}")
    End Sub

    Private Function EscapeCsvValue(value As String) As String
        If String.IsNullOrEmpty(value) Then
            Return ""
        End If

        ' If value contains comma, quote, or newline, wrap in quotes and escape quotes
        If value.Contains(",") OrElse value.Contains("""") OrElse value.Contains(vbCr) OrElse value.Contains(vbLf) Then
            Return """" & value.Replace("""", """""") & """"
        End If

        Return value
    End Function

    Private Sub btnPrint_Click(sender As Object, e As EventArgs) Handles btnPrint.Click
        Try
            If dgvAttendance.Rows.Count = 0 Then
                Dim msgForm As New Form() With {.TopMost = True}
                MessageBox.Show(msgForm, "No data to generate report.", "Generate Report", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            _logger.LogInfo("Generating Teacher Attendance report")

            ' Create report form
            Dim reportForm As New Form()
            reportForm.Text = "Teacher Attendance Report"
            reportForm.Size = New Size(1024, 768)
            reportForm.StartPosition = FormStartPosition.CenterScreen
            reportForm.WindowState = FormWindowState.Maximized
            reportForm.TopMost = True

            ' Create ReportViewer
            Dim reportViewer As New Microsoft.Reporting.WinForms.ReportViewer()
            reportViewer.Dock = DockStyle.Fill
            reportForm.Controls.Add(reportViewer)

            ' Get data for report
            Dim dateFrom As String = dtpDateFrom.Value.ToString("yyyy-MM-dd")
            Dim dateTo As String = dtpDateTo.Value.ToString("yyyy-MM-dd")

            Dim query As String = "SELECT ar.attendanceID, CONCAT(firstname, ' ', lastname) AS NAME, 
                     DATE_FORMAT(logDate, '%M %d, %Y') AS logDate, 
                     DATE_FORMAT(arrivalTime, '%h:%i:%s %p') AS arrivalTime, arrStatus, 
                     DATE_FORMAT(departureTime, '%h:%i:%s %p') AS departureTime, depStatus 
                     FROM attendance_record ar 
                     JOIN teacherinformation ti ON ar.tag_id = ti.tagID 
                     WHERE logDate BETWEEN '" & dateFrom & "' AND '" & dateTo & "'"

            ' Add search filter if provided
            If txtSearch.Text.Trim().Length > 0 Then
                query &= " AND (ti.firstname LIKE '%" & txtSearch.Text.Trim() & "%' OR ti.lastname LIKE '%" & txtSearch.Text.Trim() & "%')"
            End If

            query &= " ORDER BY logDate DESC, arrivalTime DESC"

            Dim dt As DataTable = Nothing
            connectDB()
            Dim cmd As New OdbcCommand(query, con)
            Dim da As New OdbcDataAdapter(cmd)
            dt = New DataTable()
            da.Fill(dt)
            con.Close()

            ' Set up report
            reportViewer.LocalReport.ReportPath = "ReportTeacherAttendance.rdlc"
            reportViewer.LocalReport.DataSources.Clear()

            Dim rds As New Microsoft.Reporting.WinForms.ReportDataSource("DataSetTeacherAttendance", dt)
            reportViewer.LocalReport.DataSources.Add(rds)

            ' Set report parameters
            Dim reportParams As New List(Of Microsoft.Reporting.WinForms.ReportParameter)
            reportParams.Add(New Microsoft.Reporting.WinForms.ReportParameter("DateFrom", dtpDateFrom.Value.ToString("MMMM dd, yyyy")))
            reportParams.Add(New Microsoft.Reporting.WinForms.ReportParameter("DateTo", dtpDateTo.Value.ToString("MMMM dd, yyyy")))
            reportParams.Add(New Microsoft.Reporting.WinForms.ReportParameter("TotalRecords", dt.Rows.Count.ToString()))
            reportViewer.LocalReport.SetParameters(reportParams)

            reportViewer.RefreshReport()
            reportForm.ShowDialog()

            _logger.LogInfo($"Generated attendance report with {dt.Rows.Count} records")

        Catch ex As Exception
            _logger.LogError($"Error generating report: {ex.Message}")
            Dim msgForm As New Form() With {.TopMost = True}
            MessageBox.Show(msgForm, "Error generating report: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class