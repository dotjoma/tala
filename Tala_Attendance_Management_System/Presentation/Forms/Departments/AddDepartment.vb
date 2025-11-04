Imports System.ComponentModel

Public Class AddDepartment
    Private ReadOnly _logger As ILogger = LoggerFactory.Instance
    Private ReadOnly _auditLogger As AuditLogger = AuditLogger.Instance
    Private ReadOnly _departmentService As IDepartmentService
    Private _departmentId As Integer = 0
    Private _isEditMode As Boolean = False

    Public Sub New()
        InitializeComponent()
        _departmentService = New DepartmentService()
        _isEditMode = False
    End Sub

    Public Sub New(departmentId As Integer)
        InitializeComponent()
        _departmentService = New DepartmentService()
        _departmentId = departmentId
        _isEditMode = True
    End Sub

    Private Sub AddDepartment_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            _logger.LogInfo($"AddDepartment - Form loaded, Mode: {If(_isEditMode, "Edit", "Add")}")

            InitializeForm()
            LoadHeadTeachers()

            If _isEditMode Then
                LoadDepartmentData()
            End If
        Catch ex As Exception
            _logger.LogError($"AddDepartment - Error in Form_Load: {ex.Message}")
            MessageBox.Show("Error loading form. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub InitializeForm()
        Try
            Me.Text = If(_isEditMode, "Edit Department", "Add New Department")
            lblTitle.Text = If(_isEditMode, "Edit Department", "Add New Department")
            btnSave.Text = If(_isEditMode, "Update", "Save")
            txtDepartmentCode.MaxLength = 10
            txtDepartmentName.MaxLength = 100
            txtDescription.MaxLength = 500
            txtDepartmentCode.Focus()

            _logger.LogInfo("AddDepartment - Form initialized successfully")
        Catch ex As Exception
            _logger.LogError($"AddDepartment - Error initializing form: {ex.Message}")
        End Try
    End Sub

    Private Sub LoadHeadTeachers()
        Try
            _logger.LogInfo("AddDepartment - Loading head teachers for ComboBox")

            Dim dt As New DataTable()
            Dim teacherIdColumn As New DataColumn("teacherID", GetType(Integer))
            teacherIdColumn.AllowDBNull = True
            dt.Columns.Add(teacherIdColumn)
            dt.Columns.Add("teacher_name", GetType(String))

            Dim emptyRow As DataRow = dt.NewRow()
            emptyRow("teacherID") = DBNull.Value
            emptyRow("teacher_name") = "-- Select Department Head --"
            dt.Rows.Add(emptyRow)

            Dim dbContext As New DatabaseContext()
            Dim teacherDt = dbContext.ExecuteQuery("SELECT teacherID, CONCAT(firstname, ' ', lastname) AS teacher_name FROM teacherinformation WHERE isActive = 1 ORDER BY firstname, lastname")

            For Each row As DataRow In teacherDt.Rows
                Dim newRow As DataRow = dt.NewRow()
                newRow("teacherID") = row("teacherID")
                newRow("teacher_name") = row("teacher_name").ToString()
                dt.Rows.Add(newRow)
            Next

            cboHeadTeacher.DataSource = dt
            cboHeadTeacher.ValueMember = "teacherID"
            cboHeadTeacher.DisplayMember = "teacher_name"
            cboHeadTeacher.SelectedIndex = 0

            _logger.LogInfo($"AddDepartment - {teacherDt.Rows.Count} head teachers loaded successfully")
        Catch ex As Exception
            _logger.LogError($"AddDepartment - Error loading head teachers: {ex.Message}")

            Dim errorDt As New DataTable()
            errorDt.Columns.Add("teacherID", GetType(Integer))
            errorDt.Columns.Add("teacher_name", GetType(String))

            Dim errorRow As DataRow = errorDt.NewRow()
            errorRow("teacherID") = DBNull.Value
            errorRow("teacher_name") = "-- Error Loading Teachers --"
            errorDt.Rows.Add(errorRow)

            cboHeadTeacher.DataSource = errorDt
            cboHeadTeacher.ValueMember = "teacherID"
            cboHeadTeacher.DisplayMember = "teacher_name"
            cboHeadTeacher.SelectedIndex = 0
        End Try
    End Sub

    Private Sub LoadDepartmentData()
        Try
            _logger.LogInfo($"AddDepartment - Loading department data for ID: {_departmentId}")

            Dim department As Department = _departmentService.GetDepartmentById(_departmentId)

            If department IsNot Nothing Then
                txtDepartmentCode.Text = department.DepartmentCode
                txtDepartmentName.Text = department.DepartmentName
                txtDescription.Text = department.Description
                chkIsActive.Checked = department.IsActive

                If department.HeadTeacherId.HasValue Then
                    cboHeadTeacher.SelectedValue = department.HeadTeacherId.Value
                Else
                    cboHeadTeacher.SelectedIndex = 0
                End If

                _logger.LogInfo($"AddDepartment - Department data loaded: {department.DepartmentName}")
            Else
                _logger.LogWarning($"AddDepartment - Department not found with ID: {_departmentId}")
                MessageBox.Show("Department not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Me.DialogResult = DialogResult.Cancel
                Me.Close()
            End If
        Catch ex As Exception
            _logger.LogError($"AddDepartment - Error loading department data: {ex.Message}")
            MessageBox.Show("Error loading department data. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            _logger.LogInfo($"AddDepartment - Save button clicked, Mode: {If(_isEditMode, "Update", "Create")}")

            If Not ValidateInput() Then
                Return
            End If

            Dim headTeacherId As Integer? = Nothing
            If cboHeadTeacher.SelectedValue IsNot Nothing AndAlso Not IsDBNull(cboHeadTeacher.SelectedValue) Then
                Dim selectedValue = cboHeadTeacher.SelectedValue.ToString()
                If IsNumeric(selectedValue) AndAlso Convert.ToInt32(selectedValue) > 0 Then
                    headTeacherId = Convert.ToInt32(selectedValue)
                    _logger.LogInfo($"AddDepartment - Head teacher selected: ID {headTeacherId.Value}")
                Else
                    _logger.LogInfo("AddDepartment - No head teacher selected (invalid value)")
                End If
            Else
                _logger.LogInfo("AddDepartment - No head teacher selected (null/DBNull)")
            End If

            Dim department As New Department() With {
                .DepartmentId = _departmentId,
                .DepartmentCode = txtDepartmentCode.Text.Trim().ToUpper(),
                .DepartmentName = txtDepartmentName.Text.Trim(),
                .Description = txtDescription.Text.Trim(),
                .HeadTeacherId = headTeacherId,
                .IsActive = chkIsActive.Checked
            }

            Dim success As Boolean = False
            Dim operation As String = ""

            If _isEditMode Then
                success = _departmentService.UpdateDepartment(department)
                operation = "updated"
            Else
                success = _departmentService.CreateDepartment(department)
                operation = "created"
            End If

            If success Then
                If _isEditMode Then
                    _auditLogger.LogUpdate(MainForm.currentUsername, "Department",
                        $"Updated department - Code: '{department.DepartmentCode}', Name: '{department.DepartmentName}' (ID: {department.DepartmentId})")
                Else
                    _auditLogger.LogCreate(MainForm.currentUsername, "Department",
                        $"Created department - Code: '{department.DepartmentCode}', Name: '{department.DepartmentName}'")
                End If

                _logger.LogInfo($"AddDepartment - Department {operation} successfully: {department.DepartmentName}")
                MessageBox.Show($"Department {operation} successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Me.DialogResult = DialogResult.OK
                Me.Close()
            Else
                _logger.LogWarning($"AddDepartment - Failed to {operation.Replace("d", "")} department: {department.DepartmentName}")
                MessageBox.Show($"Failed to {operation.Replace("d", "")} department. Please check if the department code already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If

        Catch ex As Exception
            _logger.LogError($"AddDepartment - Error in Save button: {ex.Message}")
            MessageBox.Show("An error occurred while saving. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function ValidateInput() As Boolean
        Try
            If String.IsNullOrWhiteSpace(txtDepartmentCode.Text) Then
                MessageBox.Show("Department code is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtDepartmentCode.Focus()
                Return False
            End If

            If txtDepartmentCode.Text.Trim().Length > 10 Then
                MessageBox.Show("Department code cannot exceed 10 characters.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtDepartmentCode.Focus()
                txtDepartmentCode.SelectAll()
                Return False
            End If

            If String.IsNullOrWhiteSpace(txtDepartmentName.Text) Then
                MessageBox.Show("Department name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtDepartmentName.Focus()
                Return False
            End If

            If txtDepartmentName.Text.Trim().Length > 100 Then
                MessageBox.Show("Department name cannot exceed 100 characters.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtDepartmentName.Focus()
                txtDepartmentName.SelectAll()
                Return False
            End If

            Dim code As String = txtDepartmentCode.Text.Trim().ToUpper()
            If Not _departmentService.IsDepartmentCodeUnique(code, _departmentId) Then
                MessageBox.Show($"Department code '{code}' already exists. Please use a different code.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtDepartmentCode.Focus()
                txtDepartmentCode.SelectAll()
                Return False
            End If

            _logger.LogInfo("AddDepartment - Input validation passed")
            Return True
        Catch ex As Exception
            _logger.LogError($"AddDepartment - Error in validation: {ex.Message}")
            MessageBox.Show("Error during validation. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Try
            _logger.LogInfo("AddDepartment - Cancel button clicked")
            Me.DialogResult = DialogResult.Cancel
            Me.Close()
        Catch ex As Exception
            _logger.LogError($"AddDepartment - Error in Cancel button: {ex.Message}")
            Me.Close()
        End Try
    End Sub

    Private Sub txtDepartmentCode_Leave(sender As Object, e As EventArgs) Handles txtDepartmentCode.Leave
        Try
            txtDepartmentCode.Text = txtDepartmentCode.Text.Trim().ToUpper()
        Catch ex As Exception
            _logger.LogWarning($"AddDepartment - Error in txtDepartmentCode_Leave: {ex.Message}")
        End Try
    End Sub

    Private Sub txtDepartmentCode_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtDepartmentCode.KeyPress
        Try
            If Not Char.IsLetterOrDigit(e.KeyChar) AndAlso Not Char.IsControl(e.KeyChar) Then
                e.Handled = True
                MessageBox.Show("Department code can only contain letters and numbers.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As Exception
            _logger.LogWarning($"AddDepartment - Error in txtDepartmentCode_KeyPress: {ex.Message}")
        End Try
    End Sub

    Private Sub AddDepartment_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        Try
            If e.KeyCode = Keys.Escape Then
                btnCancel_Click(sender, e)
            ElseIf e.KeyCode = Keys.Enter AndAlso e.Control Then
                btnSave_Click(sender, e)
            End If
        Catch ex As Exception
            _logger.LogWarning($"AddDepartment - Error in KeyDown: {ex.Message}")
        End Try
    End Sub

    Private Sub AddDepartment_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            _logger.LogInfo($"AddDepartment - Form closing, DialogResult: {Me.DialogResult}")
        Catch ex As Exception
            _logger.LogError($"AddDepartment - Error in FormClosing: {ex.Message}")
        End Try
    End Sub

    Private Sub ClearFields()
        Try
            txtDepartmentCode.Clear()
            txtDepartmentName.Clear()
            txtDescription.Clear()
            cboHeadTeacher.SelectedIndex = 0
            chkIsActive.Checked = True
            txtDepartmentCode.Focus()
        Catch ex As Exception
            _logger.LogWarning($"AddDepartment - Error clearing fields: {ex.Message}")
        End Try
    End Sub

End Class
