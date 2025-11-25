Imports System.Data.Odbc

Public Class FormAttendace
    Private ReadOnly _logger As ILogger = LoggerFactory.Instance
    Public strFilter As String = ""
    Private selectedAttendanceId As Integer = 0
    Public Property UserRole As String = ""
    
    Public Shared CurrentInstance As FormAttendace = Nothing

    Public Sub DefaultSettings()
        dgvAttendance.CurrentCell = Nothing

        dgvAttendance.AutoGenerateColumns = False
        dgvAttendance.AllowUserToAddRows = False
        dgvAttendance.ReadOnly = True
        dgvAttendance.SelectionMode = DataGridViewSelectionMode.FullRowSelect

        ' Load department filter
        LoadDepartmentFilter()

        ' Load status filter
        LoadStatusFilter()

        LoadAttendanceData()
    End Sub

    Private Sub LoadDepartmentFilter()
        Try
            _logger.LogInfo("FormAttendance - Loading departments for filtering")

            Dim dt As New DataTable()
            dt.Columns.Add("department_id", GetType(String))
            dt.Columns.Add("department_display", GetType(String))

            Dim allRow As DataRow = dt.NewRow()
            allRow("department_id") = "ALL"
            allRow("department_display") = "All Departments"
            dt.Rows.Add(allRow)

            ' Load departments from database
            connectDB()
            Dim query As String = "SELECT department_id, department_code, department_name FROM departments WHERE is_active = 1 ORDER BY department_name"
            Dim cmd As New OdbcCommand(query, con)
            Dim reader As OdbcDataReader = cmd.ExecuteReader()

            While reader.Read()
                Dim row As DataRow = dt.NewRow()
                row("department_id") = reader("department_id").ToString()
                row("department_display") = $"{reader("department_code")} - {reader("department_name")}"
                dt.Rows.Add(row)
            End While
            reader.Close()
            con.Close()

            _logger.LogInfo($"FormAttendance - {dt.Rows.Count - 1} departments loaded for filtering")

            cboDepartmentFilter.DataSource = dt
            cboDepartmentFilter.ValueMember = "department_id"
            cboDepartmentFilter.DisplayMember = "department_display"
            cboDepartmentFilter.SelectedIndex = 0

        Catch ex As Exception
            _logger.LogError($"FormAttendance - Error loading department filter: {ex.Message}")
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

    Private Sub LoadStatusFilter()
        Try
            ' Combined Status Filter
            cboStatus.Items.Clear()
            cboStatus.Items.AddRange({"All Status", "Late - On time", "Late - Over time", "Late - Early out", "On time - On time", "On time - Over time", "On time - Early out"})
            cboStatus.SelectedIndex = 0

            _logger.LogInfo("FormAttendance - Status filters initialized")
        Catch ex As Exception
            _logger.LogError($"FormAttendance - Error loading status filter: {ex.Message}")
        End Try
    End Sub

    Private Sub LoadAttendanceData()
        Try
            Dim dateFrom As String = dtpDateFrom.Value.ToString("yyyy-MM-dd")
            Dim dateTo As String = dtpDateTo.Value.ToString("yyyy-MM-dd")

            ' Check if status filter is applied
            Dim hasStatusFilter As Boolean = False
            Dim selectedStatus As String = ""
            If cboStatus IsNot Nothing AndAlso cboStatus.SelectedItem IsNot Nothing Then
                selectedStatus = cboStatus.SelectedItem.ToString()
                hasStatusFilter = (selectedStatus <> "All Status")
            End If

            Dim query As String = ""
            
            If hasStatusFilter Then
                ' Use subquery when status filter is applied
                query = "SELECT * FROM (
                         SELECT ar.attendanceID, 
                         CONCAT(ti.firstname, ' ', ti.lastname) AS NAME, 
                         COALESCE(d.department_name, 'N/A') AS department, 
                         DATE_FORMAT(ar.logDate, '%M %d, %Y') AS logDate, 
                         ti.shift_start_time,
                         ti.shift_end_time,
                         DATE_FORMAT(ar.arrivalTime, '%h:%i:%s %p') AS arrivalTime, 
                         DATE_FORMAT(ar.departureTime, '%h:%i:%s %p') AS departureTime, 
                         " & GetCombinedStatusSQL("ar.arrivalTime", "ar.departureTime", "ti.shift_start_time", "ti.shift_end_time", "") & " AS STATUS,
                         COALESCE(ar.remarks, '') AS remarks
                         FROM attendance_record ar 
                         JOIN teacherinformation ti ON ar.teacherID = ti.teacherID 
                         LEFT JOIN departments d ON ti.department_id = d.department_id
                         WHERE ar.logDate BETWEEN '" & dateFrom & "' AND '" & dateTo & "'"
                
                ' Add department filter
                If cboDepartmentFilter IsNot Nothing AndAlso cboDepartmentFilter.SelectedValue IsNot Nothing Then
                    Dim selectedDept As String = cboDepartmentFilter.SelectedValue.ToString()
                    If selectedDept <> "ALL" AndAlso IsNumeric(selectedDept) Then
                        query &= " AND ti.department_id = " & selectedDept
                    End If
                End If
                
                ' Add search filter
                If txtSearch.Text.Trim().Length > 0 Then
                    query &= " AND (ti.firstname LIKE '%" & txtSearch.Text.Trim() & "%' OR ti.lastname LIKE '%" & txtSearch.Text.Trim() & "%')"
                End If
                
                query &= ") AS filtered_data WHERE STATUS = '" & selectedStatus & "' ORDER BY logDate DESC, arrivalTime DESC"
            Else
                ' No status filter - use simple query
                query = "SELECT ar.attendanceID, 
                         CONCAT(ti.firstname, ' ', ti.lastname) AS NAME, 
                         COALESCE(d.department_name, 'N/A') AS department, 
                         DATE_FORMAT(ar.logDate, '%M %d, %Y') AS logDate, 
                         ti.shift_start_time,
                         ti.shift_end_time,
                         DATE_FORMAT(ar.arrivalTime, '%h:%i:%s %p') AS arrivalTime, 
                         DATE_FORMAT(ar.departureTime, '%h:%i:%s %p') AS departureTime, 
                         " & GetCombinedStatusSQL("ar.arrivalTime", "ar.departureTime", "ti.shift_start_time", "ti.shift_end_time", "") & " AS STATUS,
                         COALESCE(ar.remarks, '') AS remarks
                         FROM attendance_record ar 
                         JOIN teacherinformation ti ON ar.teacherID = ti.teacherID 
                         LEFT JOIN departments d ON ti.department_id = d.department_id
                         WHERE ar.logDate BETWEEN '" & dateFrom & "' AND '" & dateTo & "'"
                
                ' Add department filter
                If cboDepartmentFilter IsNot Nothing AndAlso cboDepartmentFilter.SelectedValue IsNot Nothing Then
                    Dim selectedDept As String = cboDepartmentFilter.SelectedValue.ToString()
                    If selectedDept <> "ALL" AndAlso IsNumeric(selectedDept) Then
                        query &= " AND ti.department_id = " & selectedDept
                    End If
                End If
                
                ' Add search filter
                If txtSearch.Text.Trim().Length > 0 Then
                    query &= " AND (ti.firstname LIKE '%" & txtSearch.Text.Trim() & "%' OR ti.lastname LIKE '%" & txtSearch.Text.Trim() & "%')"
                End If
                
                query &= " ORDER BY ar.logDate DESC, ar.arrivalTime DESC"
            End If

            _logger.LogDebug($"SQL Query: {query}")
            loadDGV(query, dgvAttendance)

            _logger.LogInfo($"Loaded {dgvAttendance.Rows.Count} attendance records from {dateFrom} to {dateTo}")

            If TypeOf dgvAttendance.DataSource Is DataTable Then
                Dim dt As DataTable = CType(dgvAttendance.DataSource, DataTable)
                For i As Integer = 0 To dt.Rows.Count - 1
                    Dim row = dt.Rows(i)
                    _logger.LogDebug($"Record #{i + 1}: {{ID={If(IsDBNull(row("attendanceID")), "NULL", row("attendanceID").ToString())}, NAME={If(IsDBNull(row("NAME")), "NULL", row("NAME").ToString())}, logDate={If(IsDBNull(row("logDate")), "NULL", row("logDate").ToString())}, arrivalTime={If(IsDBNull(row("arrivalTime")), "NULL", row("arrivalTime").ToString())}, departureTime={If(IsDBNull(row("departureTime")), "NULL", row("departureTime").ToString())}, STATUS={If(IsDBNull(row("STATUS")), "NULL", row("STATUS").ToString())}}}")
                Next
            End If

        Catch ex As Exception
            _logger.LogError($"Error loading attendance data: {ex.Message}")
            Dim msgForm As New Form() With {.TopMost = True}
            MessageBox.Show(msgForm, "Error loading attendance data: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub FormAttendance_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CurrentInstance = Me

        RemoveHandler dtpDateFrom.ValueChanged, AddressOf dtpDateFrom_ValueChanged
        RemoveHandler dtpDateTo.ValueChanged, AddressOf dtpDateTo_ValueChanged

        dtpDateFrom.MaxDate = DateTime.Today
        dtpDateTo.MaxDate = DateTime.Today
        dtpDateFrom.MinDate = DateTime.Today.AddYears(-5)
        dtpDateTo.MinDate = DateTime.Today.AddYears(-5)
        dtpDateFrom.Value = DateTime.Today
        dtpDateTo.Value = DateTime.Today
        AddHandler dtpDateFrom.ValueChanged, AddressOf dtpDateFrom_ValueChanged
        AddHandler dtpDateTo.ValueChanged, AddressOf dtpDateTo_ValueChanged

        ' Add filter event handlers
        AddHandler cboDepartmentFilter.SelectedIndexChanged, AddressOf FilterChanged
        AddHandler cboStatus.SelectedIndexChanged, AddressOf FilterChanged

        DefaultSettings()
        ApplyRoleBasedAccess()

        _logger.LogInfo("FormAttendance loaded")
    End Sub

    Private Sub FilterChanged(sender As Object, e As EventArgs)
        LoadAttendanceData()
    End Sub

    Private Function GetCombinedStatusSQL(arrivalColumn As String, departureColumn As String, shiftStartColumn As String, shiftEndColumn As String, statusColumn As String) As String
        ' Combined status: Shows time in status only, then combines with time out status after time out
        ' Time out status: On time (exactly at shift end), Over time (after shift end), Early out (before shift end)
        Return $"CASE 
            WHEN {arrivalColumn} IS NULL THEN 'Absent'
            WHEN {shiftStartColumn} IS NULL THEN 'No shift'
            WHEN {departureColumn} IS NULL AND TIME({arrivalColumn}) <= {shiftStartColumn} THEN 'On time'
            WHEN {departureColumn} IS NULL AND TIME({arrivalColumn}) > {shiftStartColumn} THEN 'Late'
            WHEN {shiftEndColumn} IS NULL AND TIME({arrivalColumn}) <= {shiftStartColumn} THEN 'On time'
            WHEN {shiftEndColumn} IS NULL AND TIME({arrivalColumn}) > {shiftStartColumn} THEN 'Late'
            WHEN TIME({arrivalColumn}) <= {shiftStartColumn} AND TIME({departureColumn}) = {shiftEndColumn} THEN 'On time - On time'
            WHEN TIME({arrivalColumn}) <= {shiftStartColumn} AND TIME({departureColumn}) > {shiftEndColumn} THEN 'On time - Over time'
            WHEN TIME({arrivalColumn}) <= {shiftStartColumn} AND TIME({departureColumn}) < {shiftEndColumn} THEN 'On time - Early out'
            WHEN TIME({arrivalColumn}) > {shiftStartColumn} AND TIME({departureColumn}) = {shiftEndColumn} THEN 'Late - On time'
            WHEN TIME({arrivalColumn}) > {shiftStartColumn} AND TIME({departureColumn}) > {shiftEndColumn} THEN 'Late - Over time'
            WHEN TIME({arrivalColumn}) > {shiftStartColumn} AND TIME({departureColumn}) < {shiftEndColumn} THEN 'Late - Early out'
            ELSE 'Unknown'
        END"
    End Function

    Private Sub FormAttendance_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If CurrentInstance Is Me Then
            CurrentInstance = Nothing
        End If
    End Sub

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
            Dim userRole As String = ""

            If Not String.IsNullOrEmpty(Me.UserRole) Then
                userRole = Me.UserRole.ToLower()
            ElseIf Not String.IsNullOrEmpty(MainForm.currentUserRole) Then
                userRole = MainForm.currentUserRole.ToLower()
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

            ' Enable Edit button for admin and HR
            'Dim canEdit As Boolean = (userRole = "admin" OrElse userRole = "hr")
            'btnEdit.Visible = canEdit
            'btnEdit.Enabled = canEdit

            'Dim canManualInput As Boolean = (userRole = "admin" OrElse userRole = "hr")
            'btnManualInput.Visible = canManualInput
            'btnManualInput.Enabled = canManualInput

            '_logger.LogInfo($"Role-based access applied - User role: '{userRole}', Edit button {If(canEdit, "enabled", "disabled")}, Manual input {If(canManualInput, "enabled", "disabled")}")
        Catch ex As Exception
            _logger.LogError($"Error applying role-based access: {ex.Message}")
            btnEdit.Visible = False
            btnEdit.Enabled = False
        End Try
    End Sub

    Private Sub txtSearch_TextChanged(sender As Object, e As EventArgs) Handles txtSearch.TextChanged
        UpdateDataBasedOnFilterAndSearch()
    End Sub

    Private Sub dgvAttendance_DataBindingComplete(sender As Object, e As DataGridViewBindingCompleteEventArgs) Handles dgvAttendance.DataBindingComplete
        Try
            dgvAttendance.CurrentCell = Nothing
            dgvAttendance.ClearSelection()
            selectedAttendanceId = 0
            
            ' Populate Expected Time In/Out columns
            PopulateExpectedTimeColumns()
            
            _logger.LogInfo("Cleared selection after data binding; selectedAttendanceId reset to 0")
        Catch ex As Exception
            _logger.LogError($"Error in DataBindingComplete: {ex.Message}")
        End Try
    End Sub
    
    Private Sub PopulateExpectedTimeColumns()
        Try
            If TypeOf dgvAttendance.DataSource Is DataTable Then
                Dim dt As DataTable = CType(dgvAttendance.DataSource, DataTable)
                
                ' Check if shift time columns exist in the DataTable
                If Not dt.Columns.Contains("shift_start_time") OrElse Not dt.Columns.Contains("shift_end_time") Then
                    _logger.LogWarning("shift_start_time or shift_end_time columns not found in DataTable")
                    Return
                End If
                
                For i As Integer = 0 To dgvAttendance.Rows.Count - 1
                    Dim row As DataRow = dt.Rows(i)
                    
                    ' Get shift times from DataTable
                    Dim shiftStartTime As Object = row("shift_start_time")
                    Dim shiftEndTime As Object = row("shift_end_time")
                    
                    ' Format and populate Expected Time In
                    If Not IsDBNull(shiftStartTime) AndAlso shiftStartTime IsNot Nothing Then
                        Dim startTime As TimeSpan = CType(shiftStartTime, TimeSpan)
                        dgvAttendance.Rows(i).Cells("colExpectedTimeIn").Value = FormatTimeSpanTo12Hour(startTime)
                    Else
                        dgvAttendance.Rows(i).Cells("colExpectedTimeIn").Value = "Not Set"
                    End If
                    
                    ' Format and populate Expected Time Out
                    If Not IsDBNull(shiftEndTime) AndAlso shiftEndTime IsNot Nothing Then
                        Dim endTime As TimeSpan = CType(shiftEndTime, TimeSpan)
                        dgvAttendance.Rows(i).Cells("colExpectedTimeOut").Value = FormatTimeSpanTo12Hour(endTime)
                    Else
                        dgvAttendance.Rows(i).Cells("colExpectedTimeOut").Value = "Not Set"
                    End If
                Next
                
                _logger.LogInfo($"Populated expected time columns for {dgvAttendance.Rows.Count} rows")
            End If
        Catch ex As Exception
            _logger.LogError($"Error populating expected time columns: {ex.Message}")
        End Try
    End Sub
    
    Private Function FormatTimeSpanTo12Hour(timeValue As TimeSpan) As String
        Try
            Dim hours As Integer = timeValue.Hours
            Dim minutes As Integer = timeValue.Minutes
            Dim period As String = If(hours >= 12, "PM", "AM")
            
            ' Convert to 12-hour format
            If hours = 0 Then
                hours = 12
            ElseIf hours > 12 Then
                hours = hours - 12
            End If
            
            Return $"{hours:D2}:{minutes:D2}:{timeValue.Seconds:D2} {period}"
        Catch ex As Exception
            _logger.LogError($"Error formatting time: {ex.Message}")
            Return "Invalid Time"
        End Try
    End Function

    Private Sub cbFilter_SelectedIndexChanged(sender As Object, e As EventArgs)
        UpdateDataBasedOnFilterAndSearch()
    End Sub
    Private Sub UpdateDataBasedOnFilterAndSearch()
        Dim strFilter As String = "Teachers"

        Try
            Select Case strFilter
                Case "All"
                    LoadAllRecords()
                Case "Teachers"
                    LoadTeachersRecords()
                Case "Students"
                    LoadStudentsRecords()
                Case Else
                    LoadTeachersRecords()
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
        sql = "SELECT ar.attendanceID, CONCAT(ti.firstname, ' ', ti.lastname) AS Name, COALESCE(d.department_name, 'N/A') AS department, DATE_FORMAT(ar.logDate, '%M %d, %Y') AS logDate, " &
           "DATE_FORMAT(ar.arrivalTime, '%h:%i:%s %p') AS arrivalTime, " &
           "DATE_FORMAT(ar.departureTime, '%h:%i:%s %p') AS departureTime, " &
           GetCombinedStatusSQL("ar.arrivalTime", "ar.departureTime", "ti.shift_start_time", "ti.shift_end_time", "") & " AS STATUS, " &
           "COALESCE(ar.remarks, '') AS remarks " &
           "FROM attendance_record ar JOIN teacherinformation ti ON ar.teacherID = ti.teacherID " &
           "LEFT JOIN departments d ON ti.department_id = d.department_id " &
           "WHERE DATE(ar.logDate) = CURDATE() AND (ti.lastname LIKE ? OR ti.firstname LIKE ? OR DATE_FORMAT(ar.logDate, '%Y-%m-%d') LIKE ?)"

        Try
            connectDB()
            cmd = New Odbc.OdbcCommand(sql, con)

            cmd.Parameters.AddWithValue("@lastname", "%" & txtSearch.Text.Trim() & "%")
            cmd.Parameters.AddWithValue("@firstname", "%" & txtSearch.Text.Trim() & "%")
            cmd.Parameters.AddWithValue("@logDate", "%" & txtSearch.Text.Trim() & "%")

            ' Apply department filter if set
            If cboDepartmentFilter IsNot Nothing AndAlso cboDepartmentFilter.SelectedValue IsNot Nothing Then
                Dim selectedDept As String = cboDepartmentFilter.SelectedValue.ToString()
                If selectedDept <> "ALL" AndAlso IsNumeric(selectedDept) Then
                    sql &= " AND ti.department_id = " & selectedDept
                    cmd.CommandText = sql
                End If
            End If

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
        Dim dateFrom As String = dtpDateFrom.Value.ToString("yyyy-MM-dd")
        Dim dateTo As String = dtpDateTo.Value.ToString("yyyy-MM-dd")

        Dim query As String = "SELECT ar.attendanceID, 
                     CONCAT(ti.firstname, ' ', ti.lastname) AS NAME, 
                     COALESCE(d.department_name, 'N/A') AS department, 
                     DATE_FORMAT(ar.logDate, '%M %d, %Y') AS logDate, 
                     ti.shift_start_time,
                     ti.shift_end_time,
                     DATE_FORMAT(ar.arrivalTime, '%h:%i:%s %p') AS arrivalTime, 
                     DATE_FORMAT(ar.departureTime, '%h:%i:%s %p') AS departureTime, 
                     " & GetCombinedStatusSQL("ar.arrivalTime", "ar.departureTime", "ti.shift_start_time", "ti.shift_end_time", "") & " AS STATUS,
                     COALESCE(ar.remarks, '') AS remarks
                     FROM attendance_record ar 
                     JOIN teacherinformation ti ON ar.teacherID = ti.teacherID 
                     LEFT JOIN departments d ON ti.department_id = d.department_id
                     WHERE ar.logDate BETWEEN '" & dateFrom & "' AND '" & dateTo & "'"

        If cboDepartmentFilter IsNot Nothing AndAlso cboDepartmentFilter.SelectedValue IsNot Nothing Then
            Dim selectedDept As String = cboDepartmentFilter.SelectedValue.ToString()
            If selectedDept <> "ALL" AndAlso IsNumeric(selectedDept) Then
                query &= " AND ti.department_id = " & selectedDept
            End If
        End If

        If txtSearch.TextLength > 0 Then
            query &= " AND (ti.firstname LIKE '%" & txtSearch.Text.Trim() & "%' OR ti.lastname LIKE '%" & txtSearch.Text.Trim() & "%')"
        End If

        query &= " ORDER BY ar.logDate DESC, ar.arrivalTime DESC"

        loadDGV(query, dgvAttendance)
    End Sub

    Private Sub LoadTeachersRecords()
        Dim dateFrom As String = dtpDateFrom.Value.ToString("yyyy-MM-dd")
        Dim dateTo As String = dtpDateTo.Value.ToString("yyyy-MM-dd")

        Dim query As String = "SELECT ar.attendanceID, 
                     CONCAT(ti.firstname, ' ', ti.lastname) AS Name, 
                     COALESCE(d.department_name, 'N/A') AS department, 
                     DATE_FORMAT(ar.logDate, '%M %d, %Y') AS logDate, 
                     ti.shift_start_time,
                     ti.shift_end_time,
                     DATE_FORMAT(ar.arrivalTime, '%h:%i:%s %p') AS arrivalTime, 
                     DATE_FORMAT(ar.departureTime, '%h:%i:%s %p') AS departureTime, 
                     " & GetCombinedStatusSQL("ar.arrivalTime", "ar.departureTime", "ti.shift_start_time", "ti.shift_end_time", "") & " AS STATUS,
                     COALESCE(ar.remarks, '') AS remarks
                     FROM attendance_record ar 
                     JOIN teacherinformation ti ON ar.teacherID = ti.teacherID  
                     LEFT JOIN departments d ON ti.department_id = d.department_id
                     WHERE ar.logDate BETWEEN '" & dateFrom & "' AND '" & dateTo & "'"

        If cboDepartmentFilter IsNot Nothing AndAlso cboDepartmentFilter.SelectedValue IsNot Nothing Then
            Dim selectedDept As String = cboDepartmentFilter.SelectedValue.ToString()
            If selectedDept <> "ALL" AndAlso IsNumeric(selectedDept) Then
                query &= " AND ti.department_id = " & selectedDept
            End If
        End If

        If txtSearch.TextLength > 0 Then
            query &= " AND (ti.firstname LIKE '%" & txtSearch.Text.Trim() & "%' OR ti.lastname LIKE '%" & txtSearch.Text.Trim() & "%')"
        End If

        query &= " ORDER BY ar.logDate DESC, ar.arrivalTime DESC"

        loadDGV(query, dgvAttendance)
    End Sub

    Private Sub LoadStudentsRecords()
        Dim dateFrom As String = dtpDateFrom.Value.ToString("yyyy-MM-dd")
        Dim dateTo As String = dtpDateTo.Value.ToString("yyyy-MM-dd")

        Dim query As String = "SELECT CONCAT(firstname, ' ', lastname) AS Name, 
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
                     WHERE logDate BETWEEN '" & dateFrom & "' AND '" & dateTo & "'"

        If cboDepartmentFilter IsNot Nothing AndAlso cboDepartmentFilter.SelectedValue IsNot Nothing Then
            Dim selectedDept As String = cboDepartmentFilter.SelectedValue.ToString()
            If selectedDept <> "ALL" AndAlso IsNumeric(selectedDept) Then
                query &= " AND ti.department_id = " & selectedDept
            End If
        End If

        If txtSearch.TextLength > 0 Then
            query &= " AND (sr.firstname LIKE '%" & txtSearch.Text.Trim() & "%' OR sr.lastname LIKE '%" & txtSearch.Text.Trim() & "%')"
        End If

        query &= " ORDER BY logDate DESC, arrivalTime DESC"

        loadDGV(query, dgvAttendance)
    End Sub

    Private Sub dtpDateFrom_ValueChanged(sender As Object, e As EventArgs) Handles dtpDateFrom.ValueChanged
        ValidateDateRange()
    End Sub

    Private Sub dtpDateTo_ValueChanged(sender As Object, e As EventArgs) Handles dtpDateTo.ValueChanged
        ValidateDateRange()
    End Sub

    Private Sub ValidateDateRange()
        Dim needsCorrection As Boolean = False

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
            _logger.LogWarning($"Invalid date range detected: From {dtpDateFrom.Value:yyyy-MM-dd} > To {dtpDateTo.Value:yyyy-MM-dd}")

            RemoveHandler dtpDateTo.ValueChanged, AddressOf dtpDateTo_ValueChanged
            dtpDateTo.Value = dtpDateFrom.Value
            AddHandler dtpDateTo.ValueChanged, AddressOf dtpDateTo_ValueChanged

            needsCorrection = True
            _logger.LogInfo($"Auto-corrected date range: Both dates set to {dtpDateFrom.Value:yyyy-MM-dd}")
        End If

        If needsCorrection Then
            dtpDateFrom.BackColor = Color.LightYellow
            dtpDateTo.BackColor = Color.LightYellow

            Dim timer As New Timer With {.Interval = 1000}
            AddHandler timer.Tick, Sub()
                                       dtpDateFrom.BackColor = SystemColors.Window
                                       dtpDateTo.BackColor = SystemColors.Window
                                       timer.Stop()
                                       timer.Dispose()
                                   End Sub
            timer.Start()
        Else
            dtpDateFrom.BackColor = SystemColors.Window
            dtpDateTo.BackColor = SystemColors.Window
        End If

        Dim daysDiff As Integer = (dtpDateTo.Value - dtpDateFrom.Value).Days
        If daysDiff > 365 Then
            _logger.LogWarning($"Large date range selected: {daysDiff} days")
        End If

        UpdateDataBasedOnFilterAndSearch()
    End Sub

    Private Sub dgvAttendance_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvAttendance.CellClick
        Try
            _logger.LogDebug($"CellClick event fired - RowIndex: {e.RowIndex}, ColumnIndex: {e.ColumnIndex}")

            If e.RowIndex >= 0 Then
                _logger.LogDebug($"Valid row clicked. Total columns: {dgvAttendance.Columns.Count}")

                Dim hasAttendanceIdColumn As Boolean = dgvAttendance.Columns.Contains("attendanceID")
                _logger.LogDebug($"DataGridView has 'attendanceID' column: {hasAttendanceIdColumn}")

                If hasAttendanceIdColumn AndAlso dgvAttendance.Rows(e.RowIndex).Cells("attendanceID").Value IsNot Nothing Then
                    Dim cellValue = dgvAttendance.Rows(e.RowIndex).Cells("attendanceID").Value
                    _logger.LogDebug($"attendanceID cell value type: {cellValue.GetType().Name}, value: '{cellValue}'")
                    selectedAttendanceId = Convert.ToInt32(cellValue)
                    _logger.LogInfo($"✓ Selected attendance ID from column: {selectedAttendanceId}")
                Else
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

    Private Sub dgvAttendance_SelectionChanged(sender As Object, e As EventArgs) Handles dgvAttendance.SelectionChanged
        UpdateSelectedAttendance()
    End Sub

    Private Sub UpdateSelectedAttendance()
        Try
            If dgvAttendance.SelectedRows IsNot Nothing AndAlso dgvAttendance.SelectedRows.Count > 0 Then
                Dim row = dgvAttendance.SelectedRows(0)
                Dim id = TryGetAttendanceIdFromRow(row)
                If id.HasValue AndAlso id.Value > 0 Then
                    selectedAttendanceId = id.Value
                    _logger.LogInfo($"✓ Selected attendance ID from selection: {selectedAttendanceId}")
                End If
            Else
                selectedAttendanceId = 0
            End If
        Catch ex As Exception
            _logger.LogWarning($"Error updating selected attendance: {ex.Message}")
        End Try
    End Sub

    Private Function TryGetAttendanceIdFromRow(row As DataGridViewRow) As Integer?
        If row Is Nothing Then Return Nothing
        Try
            ' 1) Try by column name "attendanceID"
            If dgvAttendance.Columns.Contains("attendanceID") Then
                Dim v = row.Cells("attendanceID").Value
                If v IsNot Nothing AndAlso Not IsDBNull(v) Then Return Convert.ToInt32(v)
            End If

            ' 2) Try by DataPropertyName == "attendanceID"
            For Each col As DataGridViewColumn In dgvAttendance.Columns
                If String.Equals(col.DataPropertyName, "attendanceID", StringComparison.OrdinalIgnoreCase) Then
                    Dim v = row.Cells(col.Index).Value
                    If v IsNot Nothing AndAlso Not IsDBNull(v) Then Return Convert.ToInt32(v)
                End If
            Next

            ' 3) Try from DataBoundItem
            If TypeOf row.DataBoundItem Is DataRowView Then
                Dim drv = DirectCast(row.DataBoundItem, DataRowView)
                If drv.DataView.Table.Columns.Contains("attendanceID") Then
                    Dim v = drv("attendanceID")
                    If v IsNot Nothing AndAlso Not IsDBNull(v) Then Return Convert.ToInt32(v)
                End If
            End If

            Return Nothing
        Catch
            Return Nothing
        End Try
    End Function

    Private Sub btnEdit_Click(sender As Object, e As EventArgs) Handles btnEdit.Click
        _logger.LogDebug($"Edit button clicked. Current selectedAttendanceId: {selectedAttendanceId}")

        If dgvAttendance.SelectedRows Is Nothing OrElse dgvAttendance.SelectedRows.Count = 0 Then
            _logger.LogWarning("No attendance row selected in grid")
            Dim msgForm As New Form() With {.TopMost = True}
            MessageBox.Show(msgForm, "Please select an attendance record to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        If selectedAttendanceId = 0 Then
            _logger.LogWarning("No attendance record selected")
            Dim msgForm As New Form() With {.TopMost = True}
            MessageBox.Show(msgForm, "Please select an attendance record to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Try
            _logger.LogInfo($"Opening edit dialog for attendance ID: {selectedAttendanceId}")

            connectDB()

            Dim cmd As New OdbcCommand("SELECT ar.arrivalTime, ar.departureTime, ar.remarks, 
                                        CONCAT(ti.firstname, ' ', ti.lastname) AS teacher_name, ar.logDate 
                                        FROM attendance_record ar 
                                        JOIN teacherinformation ti ON ar.teacherID = ti.teacherID 
                                        WHERE ar.attendanceID = ?", con)
            cmd.Parameters.AddWithValue("?", selectedAttendanceId)

            Dim reader = cmd.ExecuteReader()

            If reader.Read() Then
                Dim teacherName As String = reader("teacher_name").ToString()
                Dim logDate As Date = Convert.ToDateTime(reader("logDate"))
                Dim currentTimeIn As Object = reader("arrivalTime")
                Dim currentTimeOut As Object = reader("departureTime")
                Dim currentRemarks As String = If(IsDBNull(reader("remarks")), "", reader("remarks").ToString())

                _logger.LogDebug($"Retrieved from DB - TimeIn: {If(IsDBNull(currentTimeIn), "NULL", currentTimeIn.ToString())}, TimeOut: {If(IsDBNull(currentTimeOut), "NULL", currentTimeOut.ToString())}")

                reader.Close()

                Using editForm As New FormEditAttendance()
                    editForm.AttendanceId = selectedAttendanceId
                    editForm.TeacherName = teacherName
                    editForm.AttendanceDate = logDate
                    editForm.CurrentRemarks = currentRemarks

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

            Dim helper As New NativeWindow()
            helper.AssignHandle(Me.Handle)

            If saveDialog.ShowDialog(helper) = DialogResult.OK Then
                If saveDialog.FilterIndex = 1 Then
                    ExportToExcel(saveDialog.FileName)
                Else
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
            Dim excelApp As Object = Nothing
            Dim workbook As Object = Nothing
            Dim worksheet As Object = Nothing

            Try
                excelApp = CreateObject("Excel.Application")
                workbook = excelApp.Workbooks.Add()
                worksheet = workbook.Worksheets(1)

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

                worksheet.Columns.AutoFit()

                Dim lastCol As Integer = col - 1
                Dim lastRow As Integer = row - 1
                Dim range = worksheet.Range(worksheet.Cells(1, 1), worksheet.Cells(lastRow, lastCol))
                range.Borders.LineStyle = 1

                workbook.SaveAs(filePath)
                workbook.Close(False)
                excelApp.Quit()

                _logger.LogInfo($"Exported to Excel with auto-sized columns: {filePath}")

            Finally
                If worksheet IsNot Nothing Then System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet)
                If workbook IsNot Nothing Then System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook)
                If excelApp IsNot Nothing Then System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp)
            End Try

        Catch ex As Exception
            _logger.LogWarning($"Excel Interop not available, falling back to CSV: {ex.Message}")
            ExportToCSV(filePath.Replace(".xlsx", ".csv"))
        End Try
    End Sub

    Private Sub ExportToCSV(filePath As String)
        Using writer As New System.IO.StreamWriter(filePath, False, New System.Text.UTF8Encoding(True))
            Dim columnWidths As New Dictionary(Of String, Integer) From {
                {"logdate", 20},
                {"Name", 30},
                {"arrivalTime", 15},
                {"arrStatus", 12},
                {"departuretime", 15},
                {"depStatus", 12}
            }

            Dim headers As New List(Of String)
            Dim visibleColumns As New List(Of DataGridViewColumn)

            For Each column As DataGridViewColumn In dgvAttendance.Columns
                If column.Visible AndAlso column.Name <> "attendanceID" Then
                    visibleColumns.Add(column)
                    Dim headerText As String = column.HeaderText
                    Dim minWidth As Integer = If(columnWidths.ContainsKey(column.DataPropertyName), columnWidths(column.DataPropertyName), 15)
                    headerText = headerText.PadRight(Math.Max(headerText.Length, minWidth))
                    headers.Add(EscapeCsvValue(headerText))
                End If
            Next
            writer.WriteLine(String.Join(",", headers))

            For Each row As DataGridViewRow In dgvAttendance.Rows
                If row.IsNewRow Then Continue For

                Dim values As New List(Of String)
                For Each column In visibleColumns
                    Dim cellValue = row.Cells(column.Index).Value
                    Dim value As String = If(cellValue Is Nothing OrElse IsDBNull(cellValue), "", cellValue.ToString().Trim())

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

            Dim reportForm As New Form()
            reportForm.Text = "Teacher Attendance Report"
            reportForm.Size = New Size(1024, 768)
            reportForm.StartPosition = FormStartPosition.CenterScreen
            reportForm.WindowState = FormWindowState.Maximized
            reportForm.TopMost = True

            Dim reportViewer As New Microsoft.Reporting.WinForms.ReportViewer()
            reportViewer.Dock = DockStyle.Fill
            reportForm.Controls.Add(reportViewer)

            Dim dateFrom As String = dtpDateFrom.Value.ToString("yyyy-MM-dd")
            Dim dateTo As String = dtpDateTo.Value.ToString("yyyy-MM-dd")

            ' Check if status filter is applied
            Dim hasStatusFilter As Boolean = False
            Dim selectedStatus As String = ""
            If cboStatus IsNot Nothing AndAlso cboStatus.SelectedItem IsNot Nothing Then
                selectedStatus = cboStatus.SelectedItem.ToString()
                hasStatusFilter = (selectedStatus <> "All Status")
            End If

            Dim query As String = ""
            
            If hasStatusFilter Then
                ' Use subquery when status filter is applied
                query = "SELECT * FROM (
                         SELECT ar.attendanceID, CONCAT(firstname, ' ', lastname) AS NAME, 
                         COALESCE(d.department_name, 'N/A') AS department, 
                         DATE_FORMAT(logDate, '%M %d, %Y') AS logDate, 
                         DATE_FORMAT(arrivalTime, '%h:%i:%s %p') AS arrivalTime, 
                         DATE_FORMAT(departureTime, '%h:%i:%s %p') AS departureTime, 
                         " & GetCombinedStatusSQL("ar.arrivalTime", "ar.departureTime", "ti.shift_start_time", "ti.shift_end_time", "") & " AS STATUS,
                         COALESCE(ar.remarks, '') AS remarks
                         FROM attendance_record ar 
                         JOIN teacherinformation ti ON ar.teacherID = ti.teacherID 
                         LEFT JOIN departments d ON ti.department_id = d.department_id
                         WHERE logDate BETWEEN '" & dateFrom & "' AND '" & dateTo & "'"
                
                If cboDepartmentFilter IsNot Nothing AndAlso cboDepartmentFilter.SelectedValue IsNot Nothing Then
                    Dim selectedDept As String = cboDepartmentFilter.SelectedValue.ToString()
                    If selectedDept <> "ALL" AndAlso IsNumeric(selectedDept) Then
                        query &= " AND ti.department_id = " & selectedDept
                    End If
                End If
                
                If txtSearch.Text.Trim().Length > 0 Then
                    query &= " AND (ti.firstname LIKE '%" & txtSearch.Text.Trim() & "%' OR ti.lastname LIKE '%" & txtSearch.Text.Trim() & "%')"
                End If
                
                query &= ") AS filtered_data WHERE STATUS = '" & selectedStatus & "' ORDER BY logDate DESC, arrivalTime DESC"
            Else
                ' No status filter - use simple query
                query = "SELECT ar.attendanceID, CONCAT(firstname, ' ', lastname) AS NAME, 
                         COALESCE(d.department_name, 'N/A') AS department, 
                         DATE_FORMAT(logDate, '%M %d, %Y') AS logDate, 
                         DATE_FORMAT(arrivalTime, '%h:%i:%s %p') AS arrivalTime, 
                         DATE_FORMAT(departureTime, '%h:%i:%s %p') AS departureTime, 
                         " & GetCombinedStatusSQL("ar.arrivalTime", "ar.departureTime", "ti.shift_start_time", "ti.shift_end_time", "") & " AS STATUS,
                         COALESCE(ar.remarks, '') AS remarks
                         FROM attendance_record ar 
                         JOIN teacherinformation ti ON ar.teacherID = ti.teacherID 
                         LEFT JOIN departments d ON ti.department_id = d.department_id
                         WHERE logDate BETWEEN '" & dateFrom & "' AND '" & dateTo & "'"
                
                If cboDepartmentFilter IsNot Nothing AndAlso cboDepartmentFilter.SelectedValue IsNot Nothing Then
                    Dim selectedDept As String = cboDepartmentFilter.SelectedValue.ToString()
                    If selectedDept <> "ALL" AndAlso IsNumeric(selectedDept) Then
                        query &= " AND ti.department_id = " & selectedDept
                    End If
                End If
                
                If txtSearch.Text.Trim().Length > 0 Then
                    query &= " AND (ti.firstname LIKE '%" & txtSearch.Text.Trim() & "%' OR ti.lastname LIKE '%" & txtSearch.Text.Trim() & "%')"
                End If
                
                query &= " ORDER BY logDate DESC, arrivalTime DESC"
            End If

            Dim dt As DataTable = Nothing
            connectDB()
            Dim cmd As New OdbcCommand(query, con)
            Dim da As New OdbcDataAdapter(cmd)
            dt = New DataTable()
            da.Fill(dt)
            con.Close()

            reportViewer.LocalReport.ReportPath = "ReportTeacherAttendance.rdlc"
            reportViewer.LocalReport.DataSources.Clear()

            Dim rds As New Microsoft.Reporting.WinForms.ReportDataSource("DataSetTeacherAttendance", dt)
            reportViewer.LocalReport.DataSources.Add(rds)

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

    Private Sub btnViewDetails_Click(sender As Object, e As EventArgs) Handles btnViewDetails.Click
        Try
            If dgvAttendance.SelectedRows Is Nothing OrElse dgvAttendance.SelectedRows.Count = 0 Then
                Dim msgForm As New Form() With {.TopMost = True}
                MessageBox.Show(msgForm, "Please select an attendance record to view details.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            If selectedAttendanceId = 0 Then
                _logger.LogWarning("No attendance record selected for view details")
                Dim msgForm As New Form() With {.TopMost = True}
                MessageBox.Show(msgForm, "Please select an attendance record to view details.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            _logger.LogInfo($"Viewing details for attendance ID: {selectedAttendanceId}")

            connectDB()

            Dim cmd As New OdbcCommand("SELECT ar.attendanceID, 
                                        CONCAT(ti.firstname, ' ', ti.lastname) AS teacher_name,
                                        ti.employeeID,
                                        COALESCE(d.department_name, 'N/A') AS department,
                                        DATE_FORMAT(ar.logDate, '%M %d, %Y') AS logDate,
                                        DATE_FORMAT(ar.arrivalTime, '%h:%i:%s %p') AS arrivalTime,
                                        DATE_FORMAT(ar.departureTime, '%h:%i:%s %p') AS departureTime,
                                        " & GetCombinedStatusSQL("ar.arrivalTime", "ar.departureTime", "ti.shift_start_time", "ti.shift_end_time", "") & " AS STATUS,
                                        COALESCE(ar.remarks, '') AS remarks,
                                        ar.edited_by,
                                        DATE_FORMAT(ar.edited_at, '%M %d, %Y %h:%i:%s %p') AS edited_at
                                        FROM attendance_record ar
                                        JOIN teacherinformation ti ON ar.teacherID = ti.teacherID
                                        LEFT JOIN departments d ON ti.department_id = d.department_id
                                        WHERE ar.attendanceID = ?", con)
            cmd.Parameters.AddWithValue("?", selectedAttendanceId)

            Dim reader = cmd.ExecuteReader()

            If reader.Read() Then
                Dim teacherName As String = reader("teacher_name").ToString()
                Dim employeeID As String = If(IsDBNull(reader("employeeID")), "N/A", reader("employeeID").ToString())
                Dim department As String = reader("department").ToString()
                Dim logDate As String = reader("logDate").ToString()
                Dim arrivalTime As String = If(IsDBNull(reader("arrivalTime")), "Not Set", reader("arrivalTime").ToString())
                Dim departureTime As String = If(IsDBNull(reader("departureTime")), "Not Set", reader("departureTime").ToString())
                Dim status As String = If(IsDBNull(reader("STATUS")), "Unknown", reader("STATUS").ToString())
                Dim remarks As String = If(IsDBNull(reader("remarks")), "(No remarks)", reader("remarks").ToString())
                Dim editedBy As String = If(IsDBNull(reader("edited_by")), "", reader("edited_by").ToString())
                Dim editedAt As String = If(IsDBNull(reader("edited_at")), "", reader("edited_at").ToString())

                reader.Close()
                con.Close()

                ' Build detailed message
                Dim detailsMessage As String = $"ATTENDANCE RECORD DETAILS{vbCrLf}" &
                                              $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━{vbCrLf}{vbCrLf}" &
                                              $"Teacher: {teacherName}{vbCrLf}" &
                                              $"Employee ID: {employeeID}{vbCrLf}" &
                                              $"Department: {department}{vbCrLf}" &
                                              $"Date: {logDate}{vbCrLf}{vbCrLf}" &
                                              $"Time In: {arrivalTime}{vbCrLf}" &
                                              $"Time Out: {departureTime}{vbCrLf}" &
                                              $"Status: {status}{vbCrLf}{vbCrLf}" &
                                              $"Remarks:{vbCrLf}{remarks}"

                If Not String.IsNullOrEmpty(editedBy) Then
                    detailsMessage &= $"{vbCrLf}{vbCrLf}Last Edited By: {editedBy}{vbCrLf}Last Edited At: {editedAt}"
                End If

                _logger.LogInfo($"Displaying details for attendance ID: {selectedAttendanceId}")

                Dim msgForm As New Form() With {.TopMost = True}
                MessageBox.Show(msgForm, detailsMessage, "Attendance Details", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                reader.Close()
                con.Close()
                Dim msgForm As New Form() With {.TopMost = True}
                MessageBox.Show(msgForm, "Attendance record not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If

        Catch ex As Exception
            _logger.LogError($"Error viewing attendance details: {ex.Message}")
            Dim msgForm As New Form() With {.TopMost = True}
            MessageBox.Show(msgForm, "Error viewing attendance details: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

End Class