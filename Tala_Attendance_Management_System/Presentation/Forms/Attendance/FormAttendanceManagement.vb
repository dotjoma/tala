Imports System.Data.Odbc

''' <summary>
''' Comprehensive Attendance Management Form with DataGridView
''' Features: Edit, Manual Input, Admin Approval, Cut-off Validation, Filtering
''' </summary>
Public Class FormAttendanceManagement
    Private ReadOnly _logger As ILogger = LoggerFactory.Instance

    Private _currentUserRole As String = ""
    Private _isAdminApproved As Boolean = False
    Private _selectedAttendanceId As Integer = 0

    ' Payroll cut-off settings
    Private Const CUTOFF_DAY_1 As Integer = 15
    Private Const CUTOFF_DAY_2 As Integer = 31
    Private Const EDIT_GRACE_DAYS As Integer = 3

    Private Sub FormAttendanceManagement_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            _logger.LogInfo("FormAttendanceManagement - Form loaded")

            Me.WindowState = FormWindowState.Maximized

            ' Get current user role from shared variable
            _currentUserRole = MainForm.currentUserRole.ToLower()
            _logger.LogInfo($"FormAttendanceManagement - Retrieved user role: '{_currentUserRole}'")

            ' Initialize controls
            InitializeControls()

            ' Load filters
            LoadDepartments()
            LoadShifts()

            ' Load attendance data
            LoadAttendanceData()

            ' Show cut-off info
            ShowCutoffInfo()

        Catch ex As Exception
            _logger.LogError($"FormAttendanceManagement - Error loading: {ex.Message}")
            MessageBox.Show("Error loading form. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub InitializeControls()
        ' Date pickers
        dtpDateFrom.Value = DateTime.Today
        dtpDateTo.Value = DateTime.Today
        dtpDateFrom.MaxDate = DateTime.Today
        dtpDateTo.MaxDate = DateTime.Today

        ' DataGridView settings
        dgvAttendance.AutoGenerateColumns = False
        dgvAttendance.AllowUserToAddRows = False
        dgvAttendance.ReadOnly = True
        dgvAttendance.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvAttendance.MultiSelect = False

        ' Show user role
        lblUserRole.Text = $"Logged in as: {MainForm.currentUsername} ({_currentUserRole.ToUpper()})"
    End Sub

    Private Sub ShowCutoffInfo()
        Dim today As Date = DateTime.Today
        Dim currentDay As Integer = today.Day
        Dim daysInMonth As Integer = Date.DaysInMonth(today.Year, today.Month)

        Dim nextCutoff As Date
        If currentDay <= CUTOFF_DAY_1 Then
            nextCutoff = New Date(today.Year, today.Month, CUTOFF_DAY_1)
        Else
            nextCutoff = New Date(today.Year, today.Month, daysInMonth)
        End If

        Dim daysUntilCutoff As Integer = (nextCutoff - today).Days

        lblCutoffInfo.Text = $"Next Cut-off: {nextCutoff:MMM dd, yyyy} ({daysUntilCutoff} days) | Grace Period: {EDIT_GRACE_DAYS} days after cut-off"
    End Sub

    Private Sub LoadDepartments()
        Try
            Dim dt As New DataTable()
            dt.Columns.Add("department_id", GetType(String))
            dt.Columns.Add("department_display", GetType(String))

            Dim allRow As DataRow = dt.NewRow()
            allRow("department_id") = "ALL"
            allRow("department_display") = "All Departments"
            dt.Rows.Add(allRow)

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

            cboDepartment.DataSource = dt
            cboDepartment.ValueMember = "department_id"
            cboDepartment.DisplayMember = "department_display"
            cboDepartment.SelectedIndex = 0

        Catch ex As Exception
            _logger.LogError($"Error loading departments: {ex.Message}")
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

    Private Sub LoadShifts()
        Try
            Dim dt As New DataTable()
            dt.Columns.Add("shift_value", GetType(String))
            dt.Columns.Add("shift_display", GetType(String))

            dt.Rows.Add("ALL", "All Shifts")
            dt.Rows.Add("MORNING", "Morning (7AM-12PM)")
            dt.Rows.Add("AFTERNOON", "Afternoon (12PM-5PM)")

            cboShift.DataSource = dt
            cboShift.ValueMember = "shift_value"
            cboShift.DisplayMember = "shift_display"
            cboShift.SelectedIndex = 0

        Catch ex As Exception
            _logger.LogError($"Error loading shifts: {ex.Message}")
        End Try
    End Sub


    Private Sub LoadAttendanceData()
        Try
            Dim dateFrom As String = dtpDateFrom.Value.ToString("yyyy-MM-dd")
            Dim dateTo As String = dtpDateTo.Value.ToString("yyyy-MM-dd")

            Dim query As String = "SELECT ar.attendanceID, " &
                                 "DATE_FORMAT(ar.logDate, '%M %d, %Y') AS logDate, " &
                                 "CONCAT(ti.firstname, ' ', ti.lastname) AS teacherName, " &
                                 "COALESCE(d.department_name, 'N/A') AS department, " &
                                 "TIME_FORMAT(ti.shift_start_time, '%h:%i %p') AS expectedIn, " &
                                 "TIME_FORMAT(ti.shift_end_time, '%h:%i %p') AS expectedOut, " &
                                 "DATE_FORMAT(ar.arrivalTime, '%h:%i %p') AS timeIn, " &
                                 "DATE_FORMAT(ar.departureTime, '%h:%i %p') AS timeOut, " &
                                 "CASE " &
                                 "  WHEN ar.arrivalTime IS NULL THEN 'Absent' " &
                                 "  WHEN ti.shift_start_time IS NULL THEN 'No shift' " &
                                 "  WHEN ar.departureTime IS NULL AND TIME(ar.arrivalTime) <= ti.shift_start_time THEN 'On time' " &
                                 "  WHEN ar.departureTime IS NULL AND TIME(ar.arrivalTime) > ti.shift_start_time THEN 'Late' " &
                                 "  WHEN TIME(ar.arrivalTime) <= ti.shift_start_time AND TIME(ar.departureTime) >= ti.shift_end_time THEN 'On time - On time' " &
                                 "  WHEN TIME(ar.arrivalTime) <= ti.shift_start_time AND TIME(ar.departureTime) > ti.shift_end_time THEN 'On time - Over time' " &
                                 "  WHEN TIME(ar.arrivalTime) <= ti.shift_start_time AND TIME(ar.departureTime) < ti.shift_end_time THEN 'On time - Early out' " &
                                 "  WHEN TIME(ar.arrivalTime) > ti.shift_start_time AND TIME(ar.departureTime) >= ti.shift_end_time THEN 'Late - On time' " &
                                 "  WHEN TIME(ar.arrivalTime) > ti.shift_start_time AND TIME(ar.departureTime) > ti.shift_end_time THEN 'Late - Over time' " &
                                 "  WHEN TIME(ar.arrivalTime) > ti.shift_start_time AND TIME(ar.departureTime) < ti.shift_end_time THEN 'Late - Early out' " &
                                 "  ELSE 'Unknown' " &
                                 "END AS status, " &
                                 "COALESCE(ar.remarks, '') AS remarks " &
                                 "FROM attendance_record ar " &
                                 "JOIN teacherinformation ti ON ar.teacherID = ti.teacherID " &
                                 "LEFT JOIN departments d ON ti.department_id = d.department_id " &
                                 "WHERE ar.logDate BETWEEN '" & dateFrom & "' AND '" & dateTo & "'"

            ' Apply department filter
            If cboDepartment.SelectedValue IsNot Nothing Then
                Dim deptValue As String = cboDepartment.SelectedValue.ToString()
                If deptValue <> "ALL" AndAlso IsNumeric(deptValue) Then
                    query &= " AND ti.department_id = " & deptValue
                End If
            End If

            ' Apply shift filter
            If cboShift.SelectedValue IsNot Nothing AndAlso cboShift.SelectedValue.ToString() <> "ALL" Then
                Dim shiftFilter As String = cboShift.SelectedValue.ToString()
                Select Case shiftFilter
                    Case "MORNING"
                        ' Morning shift: 7AM to 12PM
                        query &= " AND HOUR(ti.shift_start_time) >= 7 AND HOUR(ti.shift_start_time) < 12"
                    Case "AFTERNOON"
                        ' Afternoon shift: 12PM to 5PM (17:00)
                        query &= " AND HOUR(ti.shift_start_time) >= 12 AND HOUR(ti.shift_start_time) < 17"
                End Select
            End If

            ' Apply teacher search
            If Not String.IsNullOrWhiteSpace(txtSearchTeacher.Text) Then
                query &= " AND (ti.firstname LIKE '%" & txtSearchTeacher.Text.Trim() & "%' OR ti.lastname LIKE '%" & txtSearchTeacher.Text.Trim() & "%')"
            End If

            query &= " ORDER BY ar.logDate DESC, ar.arrivalTime DESC"

            connectDB()
            Dim dt As New DataTable()
            Using cmd As New OdbcCommand(query, con)
                Using adapter As New OdbcDataAdapter(cmd)
                    adapter.Fill(dt)
                End Using
            End Using
            con.Close()

            dgvAttendance.DataSource = dt

            lblRecordCount.Text = $"Total Records: {dt.Rows.Count}"

            _logger.LogInfo($"Loaded {dt.Rows.Count} attendance records")

        Catch ex As Exception
            _logger.LogError($"Error loading attendance data: {ex.Message}")
            MessageBox.Show("Error loading attendance data: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        LoadAttendanceData()
    End Sub

    Private Sub cboDepartment_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboDepartment.SelectedIndexChanged
        LoadAttendanceData()
    End Sub

    Private Sub cboShift_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboShift.SelectedIndexChanged
        LoadAttendanceData()
    End Sub

    Private Sub dtpDateFrom_ValueChanged(sender As Object, e As EventArgs) Handles dtpDateFrom.ValueChanged
        If dtpDateFrom.Value > dtpDateTo.Value Then
            dtpDateTo.Value = dtpDateFrom.Value
        End If
        LoadAttendanceData()
    End Sub

    Private Sub dtpDateTo_ValueChanged(sender As Object, e As EventArgs) Handles dtpDateTo.ValueChanged
        If dtpDateTo.Value < dtpDateFrom.Value Then
            dtpDateFrom.Value = dtpDateTo.Value
        End If
        LoadAttendanceData()
    End Sub

    Private Sub txtSearchTeacher_TextChanged(sender As Object, e As EventArgs) Handles txtSearchTeacher.TextChanged
        LoadAttendanceData()
    End Sub

    Private Sub dgvAttendance_SelectionChanged(sender As Object, e As EventArgs) Handles dgvAttendance.SelectionChanged
        Try
            If dgvAttendance.SelectedRows.Count > 0 Then
                Dim row = dgvAttendance.SelectedRows(0)
                If row.Cells("colAttendanceID").Value IsNot Nothing Then
                    _selectedAttendanceId = Convert.ToInt32(row.Cells("colAttendanceID").Value)
                    btnEdit.Enabled = True
                Else
                    _selectedAttendanceId = 0
                    btnEdit.Enabled = False
                End If
            Else
                _selectedAttendanceId = 0
                btnEdit.Enabled = False
            End If
        Catch ex As Exception
            _logger.LogError($"Error in selection changed: {ex.Message}")
        End Try
    End Sub

    Private Sub btnManualInput_Click(sender As Object, e As EventArgs) Handles btnManualInput.Click
        Try
            _logger.LogInfo("btnManualInput clicked - Checking admin approval")

            ' Check admin approval
            If Not CheckAdminApproval("Manual Attendance Input") Then
                _logger.LogInfo("Admin approval denied or cancelled - Manual input aborted")
                'MessageBox.Show("Admin approval is required to perform manual attendance input.", "Approval Required", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            _logger.LogInfo("Admin approval granted - Opening FormManualAttendance for manual input")

            Using form As New FormManualAttendance()
                form.TopMost = True
                _logger.LogInfo("FormManualAttendance instance created, showing dialog")

                If form.ShowDialog(Me) = DialogResult.OK Then
                    LoadAttendanceData()
                    _logger.LogInfo("Manual input completed successfully")
                End If
            End Using

        Catch ex As Exception
            _logger.LogError($"Error opening manual input: {ex.Message}{vbCrLf}Stack Trace: {ex.StackTrace}")
            MessageBox.Show("Error opening manual input form: " & ex.Message & vbCrLf & vbCrLf & "Please check the logs for more details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnEdit_Click(sender As Object, e As EventArgs) Handles btnEdit.Click
        Try
            If _selectedAttendanceId = 0 Then
                MessageBox.Show("Please select an attendance record to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            ' Check admin approval
            If Not CheckAdminApproval("Edit Attendance Record") Then
                Return
            End If

            _logger.LogInfo($"Opening FormEditAttendance for editing ID: {_selectedAttendanceId}")

            ' Get attendance details
            connectDB()
            Dim cmd As New OdbcCommand("SELECT ar.arrivalTime, ar.departureTime, ar.remarks, " &
                                      "CONCAT(ti.firstname, ' ', ti.lastname) AS teacher_name, ar.logDate " &
                                      "FROM attendance_record ar " &
                                      "JOIN teacherinformation ti ON ar.teacherID = ti.teacherID " &
                                      "WHERE ar.attendanceID = ?", con)
            cmd.Parameters.AddWithValue("?", _selectedAttendanceId)
            Dim reader = cmd.ExecuteReader()

            If reader.Read() Then
                Dim teacherName As String = reader("teacher_name").ToString()
                Dim logDate As Date = Convert.ToDateTime(reader("logDate"))
                Dim currentTimeIn As Object = reader("arrivalTime")
                Dim currentTimeOut As Object = reader("departureTime")
                Dim currentRemarks As String = If(IsDBNull(reader("remarks")), "", reader("remarks").ToString())
                reader.Close()
                con.Close()

                Using editForm As New FormEditAttendance()
                    editForm.AttendanceId = _selectedAttendanceId
                    editForm.TeacherName = teacherName
                    editForm.AttendanceDate = logDate
                    editForm.CurrentRemarks = currentRemarks

                    If Not IsDBNull(currentTimeIn) Then
                        editForm.TimeIn = Convert.ToDateTime(currentTimeIn)
                    End If

                    If Not IsDBNull(currentTimeOut) Then
                        editForm.TimeOut = Convert.ToDateTime(currentTimeOut)
                    End If

                    editForm.TopMost = True

                    If editForm.ShowDialog() = DialogResult.OK Then
                        LoadAttendanceData()
                        _logger.LogInfo("Edit completed successfully")
                    End If
                End Using
            Else
                reader.Close()
                con.Close()
                MessageBox.Show("Attendance record not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If

        Catch ex As Exception
            _logger.LogError($"Error opening edit form: {ex.Message}")
            MessageBox.Show("Error opening edit form: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function CheckAdminApproval(actionDescription As String) As Boolean
        _logger.LogInfo($"CheckAdminApproval called - Action: {actionDescription}, Current Role: {_currentUserRole}")

        ' Only Admin doesn't need approval
        If _currentUserRole = "admin" Then
            _logger.LogInfo("User is admin - approval not required")
            Return True
        End If

        ' HR and other roles need admin approval every time
        Try
            _logger.LogInfo($"Requesting admin approval for {_currentUserRole} - Opening FormAdminApproval")

            Using approvalForm As New FormAdminApproval()
                approvalForm.RequesterRole = _currentUserRole
                approvalForm.ActionDescription = actionDescription

                Dim result As DialogResult = approvalForm.ShowDialog(Me)
                _logger.LogInfo($"FormAdminApproval closed with result: {result}")

                If result = DialogResult.OK Then
                    _logger.LogInfo($"Admin approval granted for {_currentUserRole}")

                    ' Log audit trail
                    _logger.LogInfo($"Admin approval granted for {actionDescription} by {approvalForm.ApprovedByUsername}")

                    Return True
                Else
                    _logger.LogInfo($"Admin approval denied or cancelled - DialogResult: {result}")
                End If
            End Using
        Catch ex As Exception
            _logger.LogError($"Error requesting admin approval: {ex.Message}{vbCrLf}Stack Trace: {ex.StackTrace}")
            MessageBox.Show("Error requesting admin approval: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

        _logger.LogInfo("CheckAdminApproval returning False")
        Return False
    End Function

    Private Sub btnClose_Click(sender As Object, e As EventArgs)

    End Sub

End Class
