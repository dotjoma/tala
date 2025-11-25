Imports System.Data.Odbc

Public Class FormManageAttendance
    Private ReadOnly _logger As ILogger = LoggerFactory.Instance

    Private _currentUserRole As String = ""
    Private _isAdminApproved As Boolean = False
    Private _selectedTeacherId As Integer = 0
    Private _selectedAttendanceId As Integer = 0
    Private _isEditMode As Boolean = False

    ' Payroll cut-off settings
    Private Const CUTOFF_DAY_1 As Integer = 15  ' First cut-off
    Private Const CUTOFF_DAY_2 As Integer = 31  ' Second cut-off (end of month)
    Private Const EDIT_GRACE_DAYS As Integer = 3 ' Days after cut-off when editing is still allowed

    Public Sub New(Optional attendanceId As Integer = 0)
        InitializeComponent()
        _selectedAttendanceId = attendanceId
        _isEditMode = (attendanceId > 0)
    End Sub

    Private Sub FormManageAttendance_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            _logger.LogInfo("FormManageAttendance - Form loaded")

            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.StartPosition = FormStartPosition.CenterParent

            ' Get current user role
            _currentUserRole = MainForm.currentUserRole.ToLower()

            ' Check if admin approval is required
            If _currentUserRole <> "admin" Then
                ' Require admin password for HR and other roles
                If Not RequestAdminApproval() Then
                    _logger.LogWarning($"FormManageAttendance - Admin approval denied for user role: {_currentUserRole}")
                    Me.Close()
                    Return
                End If
            Else
                _isAdminApproved = True
            End If

            ' Initialize controls
            InitializeControls()

            ' Load data
            LoadDepartments()
            LoadShifts()
            LoadTeachers()

            ' If edit mode, load existing record
            If _isEditMode Then
                LoadAttendanceRecord(_selectedAttendanceId)
            End If

        Catch ex As Exception
            _logger.LogError($"FormManageAttendance - Error in Form_Load: {ex.Message}")
            MessageBox.Show("Error loading form. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Me.Close()
        End Try
    End Sub

    Private Function RequestAdminApproval() As Boolean
        Try
            Using approvalForm As New FormAdminApproval()
                approvalForm.RequesterRole = _currentUserRole
                approvalForm.ActionDescription = If(_isEditMode, "Edit Attendance Record", "Manual Attendance Input")

                If approvalForm.ShowDialog() = DialogResult.OK Then
                    _isAdminApproved = True
                    _logger.LogInfo($"FormManageAttendance - Admin approval granted for {_currentUserRole} by {approvalForm.ApprovedByUsername}")
                    Return True
                End If
            End Using
        Catch ex As Exception
            _logger.LogError($"FormManageAttendance - Error requesting admin approval: {ex.Message}")
        End Try

        Return False
    End Function

    Private Sub InitializeControls()
        ' Date picker settings
        dtpDate.Value = DateTime.Today
        dtpDate.MaxDate = DateTime.Today

        ' Time pickers
        dtpTimeIn.Format = DateTimePickerFormat.Time
        dtpTimeIn.ShowUpDown = True
        dtpTimeOut.Format = DateTimePickerFormat.Time
        dtpTimeOut.ShowUpDown = True

        ' Entry mode radio buttons
        rbTimeInOnly.Checked = True

        ' Disable time pickers based on entry mode
        UpdateTimePickerStates()

        ' Set form title
        Me.Text = If(_isEditMode, "Edit Attendance Record", "Manual Attendance Input")
        lblTitle.Text = If(_isEditMode, "EDIT ATTENDANCE RECORD", "MANUAL ATTENDANCE INPUT")

        ' Show cut-off warning
        ShowCutoffWarning()
    End Sub


    Private Sub ShowCutoffWarning()
        Try
            Dim selectedDate As Date = dtpDate.Value.Date
            Dim today As Date = DateTime.Today

            ' Check if selected date is beyond cut-off period
            If Not IsWithinEditablePeriod(selectedDate) Then
                lblCutoffWarning.Text = "⚠ This date is beyond the editable cut-off period!"
                lblCutoffWarning.ForeColor = Color.Red
                lblCutoffWarning.Visible = True

                ' Disable save button if not admin
                If _currentUserRole <> "admin" Then
                    btnSave.Enabled = False
                    MessageBox.Show(
                        $"Cannot edit attendance records beyond the cut-off period.{vbCrLf}{vbCrLf}" &
                        $"Cut-off dates: {CUTOFF_DAY_1}th and end of month{vbCrLf}" &
                        $"Grace period: {EDIT_GRACE_DAYS} days after cut-off{vbCrLf}{vbCrLf}" &
                        $"Please contact an administrator for assistance.",
                        "Cut-off Period Exceeded",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    )
                End If
            Else
                lblCutoffWarning.Visible = False
                btnSave.Enabled = True
            End If
        Catch ex As Exception
            _logger.LogError($"FormManageAttendance - Error showing cut-off warning: {ex.Message}")
        End Try
    End Sub

    Private Function IsWithinEditablePeriod(targetDate As Date) As Boolean
        ' Admin can edit anytime
        If _currentUserRole = "admin" Then
            Return True
        End If

        Dim today As Date = DateTime.Today
        Dim daysDiff As Integer = (today - targetDate).Days

        ' Cannot edit future dates
        If daysDiff < 0 Then
            Return False
        End If

        ' Determine which cut-off period the target date falls into
        Dim targetMonth As Integer = targetDate.Month
        Dim targetYear As Integer = targetDate.Year
        Dim targetDay As Integer = targetDate.Day

        Dim cutoffDate As Date

        ' First half of month (1-15)
        If targetDay <= CUTOFF_DAY_1 Then
            cutoffDate = New Date(targetYear, targetMonth, CUTOFF_DAY_1)
        Else
            ' Second half of month (16-end)
            Dim lastDayOfMonth As Integer = Date.DaysInMonth(targetYear, targetMonth)
            cutoffDate = New Date(targetYear, targetMonth, lastDayOfMonth)
        End If

        ' Calculate days since cut-off
        Dim daysSinceCutoff As Integer = (today - cutoffDate).Days

        ' Allow editing within grace period after cut-off
        Return daysSinceCutoff <= EDIT_GRACE_DAYS
    End Function

    Private Sub LoadDepartments()
        Try
            _logger.LogInfo("FormManageAttendance - Loading departments")

            Dim dt As New DataTable()
            dt.Columns.Add("department_id", GetType(String))
            dt.Columns.Add("department_display", GetType(String))

            ' Add "All Departments" option
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

            _logger.LogInfo($"FormManageAttendance - {dt.Rows.Count - 1} departments loaded")

        Catch ex As Exception
            _logger.LogError($"FormManageAttendance - Error loading departments: {ex.Message}")
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

    Private Sub LoadShifts()
        Try
            _logger.LogInfo("FormManageAttendance - Loading shifts")

            Dim dt As New DataTable()
            dt.Columns.Add("shift_value", GetType(String))
            dt.Columns.Add("shift_display", GetType(String))

            ' Add "All Shifts" option
            Dim allRow As DataRow = dt.NewRow()
            allRow("shift_value") = "ALL"
            allRow("shift_display") = "All Shifts"
            dt.Rows.Add(allRow)

            ' Add shift options
            Dim morningRow As DataRow = dt.NewRow()
            morningRow("shift_value") = "MORNING"
            morningRow("shift_display") = "Morning Shift (6:00 AM - 11:59 AM)"
            dt.Rows.Add(morningRow)

            Dim afternoonRow As DataRow = dt.NewRow()
            afternoonRow("shift_value") = "AFTERNOON"
            afternoonRow("shift_display") = "Afternoon Shift (12:00 PM - 5:59 PM)"
            dt.Rows.Add(afternoonRow)

            Dim eveningRow As DataRow = dt.NewRow()
            eveningRow("shift_value") = "EVENING"
            eveningRow("shift_display") = "Evening Shift (6:00 PM - 5:59 AM)"
            dt.Rows.Add(eveningRow)

            cboShift.DataSource = dt
            cboShift.ValueMember = "shift_value"
            cboShift.DisplayMember = "shift_display"
            cboShift.SelectedIndex = 0

            _logger.LogInfo("FormManageAttendance - Shifts loaded")

        Catch ex As Exception
            _logger.LogError($"FormManageAttendance - Error loading shifts: {ex.Message}")
        End Try
    End Sub

    Private Sub LoadTeachers()
        Try
            _logger.LogInfo("FormManageAttendance - Loading teachers")

            connectDB()

            Dim query As String = "SELECT ti.teacherID, " &
                                 "CONCAT(ti.firstname, ' ', ti.lastname) AS teacher_name, " &
                                 "ti.employeeID, " &
                                 "ti.tagID, " &
                                 "ti.profileImg, " &
                                 "COALESCE(d.department_name, 'N/A') AS department, " &
                                 "ti.shift_start_time, " &
                                 "ti.shift_end_time, " &
                                 "ti.department_id " &
                                 "FROM teacherinformation ti " &
                                 "LEFT JOIN departments d ON ti.department_id = d.department_id " &
                                 "WHERE ti.isActive = 1"

            ' Apply department filter
            If cboDepartment.SelectedValue IsNot Nothing AndAlso cboDepartment.SelectedValue.ToString() <> "ALL" Then
                query &= " AND ti.department_id = " & cboDepartment.SelectedValue.ToString()
            End If

            ' Apply shift filter
            If cboShift.SelectedValue IsNot Nothing AndAlso cboShift.SelectedValue.ToString() <> "ALL" Then
                Dim shiftFilter As String = cboShift.SelectedValue.ToString()
                Select Case shiftFilter
                    Case "MORNING"
                        query &= " AND HOUR(ti.shift_start_time) >= 6 AND HOUR(ti.shift_start_time) < 12"
                    Case "AFTERNOON"
                        query &= " AND HOUR(ti.shift_start_time) >= 12 AND HOUR(ti.shift_start_time) < 18"
                    Case "EVENING"
                        query &= " AND (HOUR(ti.shift_start_time) >= 18 OR HOUR(ti.shift_start_time) < 6)"
                End Select
            End If

            query &= " ORDER BY ti.firstname, ti.lastname"

            Dim dt As New DataTable()
            Using cmd As New OdbcCommand(query, con)
                Using adapter As New OdbcDataAdapter(cmd)
                    adapter.Fill(dt)
                End Using
            End Using
            con.Close()

            cboTeacher.DataSource = dt
            cboTeacher.DisplayMember = "teacher_name"
            cboTeacher.ValueMember = "teacherID"
            cboTeacher.SelectedIndex = -1

            _logger.LogInfo($"FormManageAttendance - {dt.Rows.Count} teachers loaded")

        Catch ex As Exception
            _logger.LogError($"FormManageAttendance - Error loading teachers: {ex.Message}")
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub


    Private Sub cboTeacher_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboTeacher.SelectedIndexChanged
        If cboTeacher.SelectedIndex >= 0 Then
            LoadTeacherInfo()
        Else
            ClearTeacherInfo()
        End If
    End Sub

    Private Sub LoadTeacherInfo()
        Try
            If cboTeacher.SelectedValue Is Nothing Then Return

            _selectedTeacherId = Convert.ToInt32(cboTeacher.SelectedValue)

            Dim dt As DataTable = CType(cboTeacher.DataSource, DataTable)
            Dim row As DataRow = dt.Select($"teacherID = {_selectedTeacherId}")(0)

            ' Display teacher information
            lblTeacherName.Text = row("teacher_name").ToString()
            lblEmployeeID.Text = If(IsDBNull(row("employeeID")), "N/A", row("employeeID").ToString())
            lblDepartment.Text = row("department").ToString()

            ' Display expected schedule
            If Not IsDBNull(row("shift_start_time")) AndAlso Not IsDBNull(row("shift_end_time")) Then
                Dim startTime As TimeSpan = CType(row("shift_start_time"), TimeSpan)
                Dim endTime As TimeSpan = CType(row("shift_end_time"), TimeSpan)
                lblExpectedIn.Text = FormatTimeSpan(startTime)
                lblExpectedOut.Text = FormatTimeSpan(endTime)
            Else
                lblExpectedIn.Text = "Not Set"
                lblExpectedOut.Text = "Not Set"
            End If

            ' Load profile image
            If Not IsDBNull(row("profileImg")) Then
                Dim imgData As Byte() = CType(row("profileImg"), Byte())
                Using ms As New IO.MemoryStream(imgData)
                    pbTeacherPhoto.Image = Image.FromStream(ms)
                    pbTeacherPhoto.SizeMode = PictureBoxSizeMode.Zoom
                End Using
            Else
                pbTeacherPhoto.Image = Nothing ' No image available
            End If

            ' Show teacher info panel
            pnlTeacherInfo.Visible = True

            ' Check for existing attendance on selected date
            CheckExistingAttendance()

        Catch ex As Exception
            _logger.LogError($"FormManageAttendance - Error loading teacher info: {ex.Message}")
        End Try
    End Sub

    Private Sub ClearTeacherInfo()
        lblTeacherName.Text = ""
        lblEmployeeID.Text = ""
        lblDepartment.Text = ""
        lblExpectedIn.Text = ""
        lblExpectedOut.Text = ""
        pbTeacherPhoto.Image = Nothing
        pnlTeacherInfo.Visible = False
        lblExistingRecord.Visible = False
    End Sub

    Private Function FormatTimeSpan(timeValue As TimeSpan) As String
        Try
            Dim hours As Integer = timeValue.Hours
            Dim minutes As Integer = timeValue.Minutes
            Dim period As String = If(hours >= 12, "PM", "AM")

            If hours = 0 Then
                hours = 12
            ElseIf hours > 12 Then
                hours = hours - 12
            End If

            Return $"{hours:D2}:{minutes:D2} {period}"
        Catch ex As Exception
            Return "Invalid Time"
        End Try
    End Function

    Private Sub CheckExistingAttendance()
        Try
            If _selectedTeacherId = 0 Then Return

            Dim selectedDate As Date = dtpDate.Value.Date

            connectDB()
            Dim query As String = "SELECT attendanceID, " &
                                 "DATE_FORMAT(arrivalTime, '%h:%i %p') AS timeIn, " &
                                 "DATE_FORMAT(departureTime, '%h:%i %p') AS timeOut " &
                                 "FROM attendance_record " &
                                 "WHERE teacherID = ? AND DATE(logDate) = ?"

            Dim cmd As New OdbcCommand(query, con)
            cmd.Parameters.AddWithValue("?", _selectedTeacherId)
            cmd.Parameters.AddWithValue("?", selectedDate.ToString("yyyy-MM-dd"))

            Dim reader As OdbcDataReader = cmd.ExecuteReader()

            If reader.Read() Then
                Dim timeIn As String = If(IsDBNull(reader("timeIn")), "Not Set", reader("timeIn").ToString())
                Dim timeOut As String = If(IsDBNull(reader("timeOut")), "Not Set", reader("timeOut").ToString())

                lblExistingRecord.Text = $"⚠ Existing Record: Time In: {timeIn} | Time Out: {timeOut}"
                lblExistingRecord.ForeColor = Color.OrangeRed
                lblExistingRecord.Visible = True
            Else
                lblExistingRecord.Visible = False
            End If

            reader.Close()
            con.Close()

        Catch ex As Exception
            _logger.LogError($"FormManageAttendance - Error checking existing attendance: {ex.Message}")
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

    Private Sub LoadAttendanceRecord(attendanceId As Integer)
        Try
            _logger.LogInfo($"FormManageAttendance - Loading attendance record ID: {attendanceId}")

            connectDB()
            Dim query As String = "SELECT ar.*, " &
                                 "CONCAT(ti.firstname, ' ', ti.lastname) AS teacher_name, " &
                                 "ti.employeeID, " &
                                 "COALESCE(d.department_name, 'N/A') AS department " &
                                 "FROM attendance_record ar " &
                                 "JOIN teacherinformation ti ON ar.teacherID = ti.teacherID " &
                                 "LEFT JOIN departments d ON ti.department_id = d.department_id " &
                                 "WHERE ar.attendanceID = ?"

            Dim cmd As New OdbcCommand(query, con)
            cmd.Parameters.AddWithValue("?", attendanceId)

            Dim reader As OdbcDataReader = cmd.ExecuteReader()

            If reader.Read() Then
                ' Set teacher
                _selectedTeacherId = Convert.ToInt32(reader("teacherID"))
                cboTeacher.SelectedValue = _selectedTeacherId

                ' Set date
                dtpDate.Value = Convert.ToDateTime(reader("logDate"))

                ' Set times
                If Not IsDBNull(reader("arrivalTime")) Then
                    dtpTimeIn.Value = Convert.ToDateTime(reader("arrivalTime"))
                    chkTimeIn.Checked = True
                End If

                If Not IsDBNull(reader("departureTime")) Then
                    dtpTimeOut.Value = Convert.ToDateTime(reader("departureTime"))
                    chkTimeOut.Checked = True
                End If

                ' Set entry mode
                If chkTimeIn.Checked AndAlso chkTimeOut.Checked Then
                    rbBoth.Checked = True
                ElseIf chkTimeIn.Checked Then
                    rbTimeInOnly.Checked = True
                Else
                    rbTimeOutOnly.Checked = True
                End If

                ' Set remarks
                If Not IsDBNull(reader("remarks")) Then
                    txtRemarks.Text = reader("remarks").ToString()
                End If

                ' Disable teacher and date selection in edit mode
                cboTeacher.Enabled = False
                dtpDate.Enabled = False

            End If

            reader.Close()
            con.Close()

        Catch ex As Exception
            _logger.LogError($"FormManageAttendance - Error loading attendance record: {ex.Message}")
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

    Private Sub rbEntryMode_CheckedChanged(sender As Object, e As EventArgs) Handles rbTimeInOnly.CheckedChanged, rbTimeOutOnly.CheckedChanged, rbBoth.CheckedChanged
        UpdateTimePickerStates()
    End Sub

    Private Sub UpdateTimePickerStates()
        If rbTimeInOnly.Checked Then
            chkTimeIn.Checked = True
            chkTimeOut.Checked = False
            dtpTimeIn.Enabled = True
            dtpTimeOut.Enabled = False
        ElseIf rbTimeOutOnly.Checked Then
            chkTimeIn.Checked = False
            chkTimeOut.Checked = True
            dtpTimeIn.Enabled = False
            dtpTimeOut.Enabled = True
        ElseIf rbBoth.Checked Then
            chkTimeIn.Checked = True
            chkTimeOut.Checked = True
            dtpTimeIn.Enabled = True
            dtpTimeOut.Enabled = True
        End If
    End Sub

    Private Sub dtpDate_ValueChanged(sender As Object, e As EventArgs) Handles dtpDate.ValueChanged
        ShowCutoffWarning()
        CheckExistingAttendance()
    End Sub

    Private Sub cboDepartment_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboDepartment.SelectedIndexChanged
        LoadTeachers()
    End Sub

    Private Sub cboShift_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboShift.SelectedIndexChanged
        LoadTeachers()
    End Sub


    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If Not ValidateForm() Then
            Return
        End If

        Try
            btnSave.Enabled = False
            btnClear.Enabled = False
            btnClose.Enabled = False

            If _isEditMode Then
                UpdateAttendanceRecord()
            Else
                CreateAttendanceRecord()
            End If

        Catch ex As Exception
            _logger.LogError($"FormManageAttendance - Error saving: {ex.Message}")
            MessageBox.Show($"Error saving attendance record: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            btnSave.Enabled = True
            btnClear.Enabled = True
            btnClose.Enabled = True
        End Try
    End Sub

    Private Function ValidateForm() As Boolean
        ' Check admin approval
        If Not _isAdminApproved Then
            MessageBox.Show("Admin approval is required to proceed.", "Approval Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        ' Check cut-off period
        If Not IsWithinEditablePeriod(dtpDate.Value.Date) Then
            MessageBox.Show("Cannot edit attendance records beyond the cut-off period.", "Cut-off Period Exceeded", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        ' Check teacher selection
        If cboTeacher.SelectedIndex = -1 Then
            MessageBox.Show("Please select a teacher.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            cboTeacher.Focus()
            Return False
        End If

        ' Check entry mode
        If Not chkTimeIn.Checked AndAlso Not chkTimeOut.Checked Then
            MessageBox.Show("Please select at least Time-In or Time-Out.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        ' Check remarks
        If String.IsNullOrWhiteSpace(txtRemarks.Text) Then
            MessageBox.Show("Please provide a reason for this manual entry.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtRemarks.Focus()
            Return False
        End If

        ' Validate time logic
        If chkTimeIn.Checked AndAlso chkTimeOut.Checked Then
            Dim timeIn As DateTime = New DateTime(dtpDate.Value.Year, dtpDate.Value.Month, dtpDate.Value.Day,
                                                  dtpTimeIn.Value.Hour, dtpTimeIn.Value.Minute, dtpTimeIn.Value.Second)
            Dim timeOut As DateTime = New DateTime(dtpDate.Value.Year, dtpDate.Value.Month, dtpDate.Value.Day,
                                                   dtpTimeOut.Value.Hour, dtpTimeOut.Value.Minute, dtpTimeOut.Value.Second)

            If timeOut <= timeIn Then
                MessageBox.Show("Time-Out must be after Time-In.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return False
            End If

            Dim duration As TimeSpan = timeOut.Subtract(timeIn)
            If duration.TotalHours > 24 Then
                MessageBox.Show("Time duration cannot exceed 24 hours.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return False
            End If
        End If

        ' Check future dates
        Dim currentDateTime As DateTime = DateTime.Now
        If chkTimeIn.Checked Then
            Dim timeIn As DateTime = New DateTime(dtpDate.Value.Year, dtpDate.Value.Month, dtpDate.Value.Day,
                                                  dtpTimeIn.Value.Hour, dtpTimeIn.Value.Minute, dtpTimeIn.Value.Second)
            If timeIn > currentDateTime Then
                MessageBox.Show("Time-In cannot be in the future.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return False
            End If
        End If

        If chkTimeOut.Checked Then
            Dim timeOut As DateTime = New DateTime(dtpDate.Value.Year, dtpDate.Value.Month, dtpDate.Value.Day,
                                                   dtpTimeOut.Value.Hour, dtpTimeOut.Value.Minute, dtpTimeOut.Value.Second)
            If timeOut > currentDateTime Then
                MessageBox.Show("Time-Out cannot be in the future.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return False
            End If
        End If

        Return True
    End Function

    Private Sub CreateAttendanceRecord()
        Try
            Dim teacherName As String = cboTeacher.Text
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

            ' Confirm with user
            Dim message As String = $"Create manual attendance record?{vbCrLf}{vbCrLf}" &
                                   $"Teacher: {teacherName}{vbCrLf}" &
                                   $"Date: {logDate:MMMM dd, yyyy}{vbCrLf}" &
                                   $"Time-In: {If(timeIn.HasValue, timeIn.Value.ToString("hh:mm tt"), "Not Set")}{vbCrLf}" &
                                   $"Time-Out: {If(timeOut.HasValue, timeOut.Value.ToString("hh:mm tt"), "Not Set")}{vbCrLf}" &
                                   $"Reason: {txtRemarks.Text}"

            If MessageBox.Show(message, "Confirm Manual Entry", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.No Then
                Return
            End If

            ' Get teacher info
            connectDB()
            Dim getTeacherCmd As New OdbcCommand("SELECT tagID, shift_start_time, shift_end_time FROM teacherinformation WHERE teacherID = ?", con)
            getTeacherCmd.Parameters.AddWithValue("?", _selectedTeacherId)
            Dim reader = getTeacherCmd.ExecuteReader()

            Dim tagID As String = ""
            Dim shiftStartTime As Object = Nothing
            Dim shiftEndTime As Object = Nothing

            If reader.Read() Then
                If Not IsDBNull(reader("tagID")) Then
                    tagID = reader("tagID").ToString().Trim()
                End If
                If Not IsDBNull(reader("shift_start_time")) Then
                    shiftStartTime = reader("shift_start_time")
                End If
                If Not IsDBNull(reader("shift_end_time")) Then
                    shiftEndTime = reader("shift_end_time")
                End If
            End If
            reader.Close()

            ' Validate tagID
            If String.IsNullOrWhiteSpace(tagID) OrElse tagID = "--" Then
                con.Close()
                MessageBox.Show($"Teacher '{teacherName}' does not have a valid RFID tag ID.{vbCrLf}Please assign a tag ID first.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' Calculate status
            Dim arrStatus As String = CalculateArrivalStatus(timeIn, shiftStartTime)
            Dim depStatus As String = CalculateDepartureStatus(timeOut, shiftEndTime)

            ' Build remarks
            Dim finalRemarks As String = $"Manual Input by {MainForm.currentUsername}: {txtRemarks.Text.Trim()}"

            ' Check for existing record
            Dim checkCmd As New OdbcCommand("SELECT attendanceID FROM attendance_record WHERE teacherID = ? AND DATE(logDate) = ?", con)
            checkCmd.Parameters.AddWithValue("?", _selectedTeacherId)
            checkCmd.Parameters.AddWithValue("?", logDate.ToString("yyyy-MM-dd"))
            Dim existingId = checkCmd.ExecuteScalar()

            If existingId IsNot Nothing Then
                ' Update existing record
                Dim updateQuery As String = "UPDATE attendance_record SET "
                Dim params As New List(Of Object)

                If chkTimeIn.Checked Then
                    updateQuery &= "arrivalTime = ?, arrStatus = ?, "
                    params.Add(timeIn.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                    params.Add(arrStatus)
                End If

                If chkTimeOut.Checked Then
                    updateQuery &= "departureTime = ?, depStatus = ?, "
                    params.Add(timeOut.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                    params.Add(depStatus)
                End If

                updateQuery &= "remarks = ?, edited_by = ?, edited_at = NOW() WHERE attendanceID = ?"
                params.Add(finalRemarks)
                params.Add(MainForm.currentUsername)
                params.Add(existingId)

                Dim updateCmd As New OdbcCommand(updateQuery, con)
                For Each param In params
                    updateCmd.Parameters.AddWithValue("?", param)
                Next
                updateCmd.ExecuteNonQuery()

                _logger.LogInfo($"Updated attendance record ID {existingId} for teacher '{teacherName}'")
            Else
                ' Insert new record
                Dim insertQuery As String = "INSERT INTO attendance_record (tag_id, teacherID, logDate, arrivalTime, departureTime, arrStatus, depStatus, remarks, manual_input_by, manual_input_reason) " &
                                           "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)"

                Dim insertCmd As New OdbcCommand(insertQuery, con)
                insertCmd.Parameters.AddWithValue("?", tagID)
                insertCmd.Parameters.AddWithValue("?", _selectedTeacherId)
                insertCmd.Parameters.AddWithValue("?", logDate.ToString("yyyy-MM-dd"))
                insertCmd.Parameters.AddWithValue("?", If(timeIn.HasValue, CObj(timeIn.Value.ToString("yyyy-MM-dd HH:mm:ss")), DBNull.Value))
                insertCmd.Parameters.AddWithValue("?", If(timeOut.HasValue, CObj(timeOut.Value.ToString("yyyy-MM-dd HH:mm:ss")), DBNull.Value))
                insertCmd.Parameters.AddWithValue("?", If(String.IsNullOrEmpty(arrStatus), DBNull.Value, CObj(arrStatus)))
                insertCmd.Parameters.AddWithValue("?", If(String.IsNullOrEmpty(depStatus), DBNull.Value, CObj(depStatus)))
                insertCmd.Parameters.AddWithValue("?", finalRemarks)
                insertCmd.Parameters.AddWithValue("?", MainForm.currentUsername)
                insertCmd.Parameters.AddWithValue("?", txtRemarks.Text.Trim())
                insertCmd.ExecuteNonQuery()

                _logger.LogInfo($"Created new attendance record for teacher '{teacherName}'")
            End If

            con.Close()

            ' Log action
            _logger.LogInfo($"CREATE_ATTENDANCE by {MainForm.currentUsername} - Manual attendance entry for {teacherName} on {logDate:yyyy-MM-dd}, TimeIn: {If(timeIn.HasValue, timeIn.Value.ToString("HH:mm"), "N/A")}, TimeOut: {If(timeOut.HasValue, timeOut.Value.ToString("HH:mm"), "N/A")}")

            MessageBox.Show("Attendance record saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)

            ' Refresh main form if open
            If FormAttendace.CurrentInstance IsNot Nothing Then
                FormAttendace.CurrentInstance.RefreshAttendanceData()
            End If

            Me.DialogResult = DialogResult.OK
            Me.Close()

        Catch ex As Exception
            _logger.LogError($"FormManageAttendance - Error creating record: {ex.Message}")
            If con.State = ConnectionState.Open Then con.Close()
            Throw
        End Try
    End Sub


    Private Sub UpdateAttendanceRecord()
        Try
            Dim teacherName As String = cboTeacher.Text
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

            ' Confirm with user
            Dim message As String = $"Update attendance record?{vbCrLf}{vbCrLf}" &
                                   $"Teacher: {teacherName}{vbCrLf}" &
                                   $"Date: {logDate:MMMM dd, yyyy}{vbCrLf}" &
                                   $"Time-In: {If(timeIn.HasValue, timeIn.Value.ToString("hh:mm tt"), "Not Set")}{vbCrLf}" &
                                   $"Time-Out: {If(timeOut.HasValue, timeOut.Value.ToString("hh:mm tt"), "Not Set")}{vbCrLf}" &
                                   $"Reason: {txtRemarks.Text}"

            If MessageBox.Show(message, "Confirm Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.No Then
                Return
            End If

            connectDB()

            ' Get shift times
            Dim getShiftCmd As New OdbcCommand("SELECT shift_start_time, shift_end_time FROM teacherinformation WHERE teacherID = ?", con)
            getShiftCmd.Parameters.AddWithValue("?", _selectedTeacherId)
            Dim reader = getShiftCmd.ExecuteReader()

            Dim shiftStartTime As Object = Nothing
            Dim shiftEndTime As Object = Nothing

            If reader.Read() Then
                If Not IsDBNull(reader("shift_start_time")) Then
                    shiftStartTime = reader("shift_start_time")
                End If
                If Not IsDBNull(reader("shift_end_time")) Then
                    shiftEndTime = reader("shift_end_time")
                End If
            End If
            reader.Close()

            ' Calculate status
            Dim arrStatus As String = CalculateArrivalStatus(timeIn, shiftStartTime)
            Dim depStatus As String = CalculateDepartureStatus(timeOut, shiftEndTime)

            ' Build update query
            Dim updateQuery As String = "UPDATE attendance_record SET "
            Dim params As New List(Of Object)

            If chkTimeIn.Checked Then
                updateQuery &= "arrivalTime = ?, arrStatus = ?, "
                params.Add(timeIn.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                params.Add(arrStatus)
            Else
                updateQuery &= "arrivalTime = NULL, arrStatus = NULL, "
            End If

            If chkTimeOut.Checked Then
                updateQuery &= "departureTime = ?, depStatus = ?, "
                params.Add(timeOut.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                params.Add(depStatus)
            Else
                updateQuery &= "departureTime = NULL, depStatus = NULL, "
            End If

            updateQuery &= "remarks = ?, edited_by = ?, edited_at = NOW() WHERE attendanceID = ?"
            params.Add($"Edited by {MainForm.currentUsername}: {txtRemarks.Text.Trim()}")
            params.Add(MainForm.currentUsername)
            params.Add(_selectedAttendanceId)

            Dim updateCmd As New OdbcCommand(updateQuery, con)
            For Each param In params
                updateCmd.Parameters.AddWithValue("?", param)
            Next
            updateCmd.ExecuteNonQuery()

            con.Close()

            ' Log action
            _logger.LogInfo($"EDIT_ATTENDANCE by {MainForm.currentUsername} - Edited attendance record ID {_selectedAttendanceId} for teacher '{teacherName}' on {logDate:yyyy-MM-dd}")

            MessageBox.Show("Attendance record updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)

            ' Refresh main form if open
            If FormAttendace.CurrentInstance IsNot Nothing Then
                FormAttendace.CurrentInstance.RefreshAttendanceData()
            End If

            Me.DialogResult = DialogResult.OK
            Me.Close()

        Catch ex As Exception
            _logger.LogError($"FormManageAttendance - Error updating record: {ex.Message}")
            If con.State = ConnectionState.Open Then con.Close()
            Throw
        End Try
    End Sub

    Private Function CalculateArrivalStatus(timeIn As DateTime?, shiftStartTime As Object) As String
        If Not timeIn.HasValue Then Return Nothing

        If shiftStartTime Is Nothing Then Return "On Time"

        Dim shiftStart As TimeSpan
        If TypeOf shiftStartTime Is TimeSpan Then
            shiftStart = DirectCast(shiftStartTime, TimeSpan)
        Else
            TimeSpan.TryParse(shiftStartTime.ToString(), shiftStart)
        End If

        Return If(timeIn.Value.TimeOfDay <= shiftStart, "On Time", "Late")
    End Function

    Private Function CalculateDepartureStatus(timeOut As DateTime?, shiftEndTime As Object) As String
        If Not timeOut.HasValue Then Return Nothing

        If shiftEndTime Is Nothing Then Return "On Time"

        Dim shiftEnd As TimeSpan
        If TypeOf shiftEndTime Is TimeSpan Then
            shiftEnd = DirectCast(shiftEndTime, TimeSpan)
        Else
            TimeSpan.TryParse(shiftEndTime.ToString(), shiftEnd)
        End If

        Return If(timeOut.Value.TimeOfDay <= shiftEnd, "On Time", "Late")
    End Function

    Private Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click
        If MessageBox.Show("Clear all fields?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            cboTeacher.SelectedIndex = -1
            dtpDate.Value = DateTime.Today
            dtpTimeIn.Value = DateTime.Now
            dtpTimeOut.Value = DateTime.Now
            rbTimeInOnly.Checked = True
            txtRemarks.Clear()
            ClearTeacherInfo()
        End If
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

End Class
