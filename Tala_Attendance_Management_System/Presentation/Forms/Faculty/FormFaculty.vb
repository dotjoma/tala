Imports System.IO
Imports System.Data.Odbc
Imports System.Drawing
Imports Microsoft.ReportingServices.Rendering.ExcelOpenXmlRenderer
Imports System.Threading.Tasks
Imports System.Threading

Public Class FormFaculty
    Private ReadOnly _logger As ILogger = LoggerFactory.Instance
    Private ReadOnly _auditLogger As AuditLogger = AuditLogger.Instance
    Private _isLoading As Boolean = False
    Private _refreshRequested As Boolean = False
    Private _searchCancellationTokenSource As CancellationTokenSource = Nothing
    Private ReadOnly _searchDelayMs As Integer = 500 ' 500ms delay for search debouncing

    Private Sub LoadAddressComboBoxes(regionId As String, provinceId As String, cityId As String, brgyId As String)
        Try
            _logger.LogInfo($"FormFaculty - Loading address ComboBoxes for editing - Region: {regionId}, Province: {provinceId}, City: {cityId}, Barangay: {brgyId}")

            connectDB()

            FormHelper.LoadComboBox("SELECT * FROM refregion ORDER BY regDesc", "id", "regDesc", AddFaculty.cbRegion)
            AddFaculty.cbRegion.SelectedValue = regionId

            If Not String.IsNullOrEmpty(regionId) Then
                connectDB()
                Dim regionCmd As New OdbcCommand("SELECT regCode, regDesc FROM refregion WHERE id = ?", con)
                regionCmd.Parameters.AddWithValue("?", regionId)
                Dim regionReader = regionCmd.ExecuteReader()

                If regionReader.Read() Then
                    Dim regionCode As String = regionReader("regCode")?.ToString()
                    Dim regionName As String = regionReader("regDesc")?.ToString()
                    regionReader.Close()

                    If ValidationHelper.RegionHasProvinces(regionName, regionCode) Then
                        If Not String.IsNullOrEmpty(regionCode) Then
                            FormHelper.LoadComboBox($"SELECT * FROM refprovince WHERE regCode = {regionCode} ORDER BY provdesc", "id", "provdesc", AddFaculty.cbProvince)
                            AddFaculty.cbProvince.SelectedValue = provinceId
                        End If
                    Else
                        AddFaculty.cbProvince.Visible = False
                        AddFaculty.cbProvince.Enabled = False
                        _logger.LogInfo($"FormFaculty - Province controls hidden for region: {regionName}")
                    End If
                Else
                    regionReader.Close()
                End If
            End If

            If Not String.IsNullOrEmpty(provinceId) Then
                connectDB()
                Dim provinceCmd As New OdbcCommand("SELECT provCode FROM refprovince WHERE id = ?", con)
                provinceCmd.Parameters.AddWithValue("?", provinceId)
                Dim provinceCode As String = provinceCmd.ExecuteScalar()?.ToString()

                If Not String.IsNullOrEmpty(provinceCode) Then
                    FormHelper.LoadComboBox($"SELECT * FROM refcitymun WHERE provcode = {provinceCode} ORDER BY citymundesc", "id", "citymundesc", AddFaculty.cbCity)
                    AddFaculty.cbCity.SelectedValue = cityId
                End If
            End If

            If Not String.IsNullOrEmpty(cityId) Then
                connectDB()
                Dim cityCmd As New OdbcCommand("SELECT citymunCode FROM refcitymun WHERE id = ?", con)
                cityCmd.Parameters.AddWithValue("?", cityId)
                Dim cityCode As String = cityCmd.ExecuteScalar()?.ToString()

                If Not String.IsNullOrEmpty(cityCode) Then
                    loadCBO($"SELECT * FROM refbrgy WHERE citymuncode = {cityCode} ORDER BY brgydesc", "id", "brgydesc", AddFaculty.cbBrgy)
                    AddFaculty.cbBrgy.SelectedValue = brgyId
                End If
            End If

            _logger.LogInfo("FormFaculty - Address ComboBoxes loaded successfully for editing")
        Catch ex As Exception
            _logger.LogError($"FormFaculty - Error loading address ComboBoxes: {ex.Message}")
        Finally
            Try
                If con IsNot Nothing AndAlso con.State = ConnectionState.Open Then
                    con.Close()
                End If
            Catch
                ' Ignore connection close errors
            End Try
        End Try
    End Sub
    Public Sub DefaultSettings()
        Try
            _logger.LogInfo("FormFaculty - Loading default settings and faculty list")

            dgvTeachers.Tag = 0
            dgvTeachers.CurrentCell = Nothing

            dgvTeachers.AutoGenerateColumns = False
            dgvTeachers.AllowUserToAddRows = False
            dgvTeachers.ReadOnly = True
            dgvTeachers.SelectionMode = DataGridViewSelectionMode.FullRowSelect

            LoadStatusFilter()

            LoadDepartmentFilter()
            ' Load faculty list synchronously on initial load
            LoadFacultyList()
            ResetToggleButtonToDefault()

            _logger.LogInfo($"FormFaculty - Faculty list loaded successfully, {dgvTeachers.Rows.Count} records displayed")
        Catch ex As Exception
            _logger.LogError($"FormFaculty - Error in DefaultSettings: {ex.Message}")
            Throw
        End Try
    End Sub
    
    ''' <summary>
    ''' Refreshes the faculty list asynchronously to prevent UI blocking
    ''' </summary>
    Private Async Sub RefreshFacultyListAsync()
        If _isLoading Then
            _refreshRequested = True
            Return
        End If

        Try
            _isLoading = True
            Me.Cursor = Cursors.WaitCursor
            
            ' Get current filter values (capture on UI thread)
            Dim selectedDepartment As String = If(cboDepartment.SelectedValue IsNot Nothing, cboDepartment.SelectedValue.ToString(), "ALL")
            Dim searchTerm As String = txtSearch.Text.Trim()
            Dim selectedStatus As String = If(cboStatusFilter.SelectedItem IsNot Nothing, cboStatusFilter.SelectedItem.ToString(), "All")

            _logger.LogInfo($"FormFaculty - Starting async faculty list refresh - Department: {selectedDepartment}, Status: {selectedStatus}, Search: '{searchTerm}'")

            ' Load data in background thread - actually execute the query off UI thread
            Dim dataTable As DataTable = Await Task.Run(Function()
                                                             Try
                                                                 Dim baseQuery As String = BuildFacultyQuery(selectedDepartment, searchTerm, selectedStatus)
                                                                 Return LoadFacultyDataTable(baseQuery)
                                                             Catch ex As Exception
                                                                 _logger.LogError($"FormFaculty - Error loading data in background: {ex.Message}")
                                                                 Return Nothing
                                                             End Try
                                                         End Function)

            ' Update UI on UI thread
            If dataTable IsNot Nothing Then
                UpdateDataGridView(dataTable)
                _logger.LogInfo($"FormFaculty - Faculty list updated, {dgvTeachers.Rows.Count} records displayed")
            Else
                MessageBox.Show("Error loading faculty list. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If

            ' Reset toggle button
            ResetToggleButtonToDefault()

            ' Check if another refresh was requested while loading
            If _refreshRequested Then
                _refreshRequested = False
                RefreshFacultyListAsync()
            End If

        Catch ex As Exception
            _logger.LogError($"FormFaculty - Error in RefreshFacultyListAsync: {ex.Message}")
            MessageBox.Show("Error refreshing faculty list: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            _isLoading = False
            Me.Cursor = Cursors.Default
        End Try
    End Sub
    
    ''' <summary>
    ''' Loads faculty data into a DataTable (executes on background thread)
    ''' </summary>
    Private Function LoadFacultyDataTable(query As String) As DataTable
        Dim cmd As Odbc.OdbcCommand = Nothing
        Dim da As New Odbc.OdbcDataAdapter
        Dim dt As New DataTable
        Dim connection As Odbc.OdbcConnection = Nothing

        Try
            connection = New Odbc.OdbcConnection("DSN=tala_ams")
            If connection.State = ConnectionState.Closed Then
                connection.Open()
            End If

            cmd = New Odbc.OdbcCommand(query, connection)
            da.SelectCommand = cmd
            da.Fill(dt)
            
            Return dt
        Catch ex As Exception
            _logger.LogError($"FormFaculty - Error loading faculty data table: {ex.Message}")
            Throw
        Finally
            If cmd IsNot Nothing Then
                cmd.Dispose()
            End If
            If da IsNot Nothing Then
                da.Dispose()
            End If
            If connection IsNot Nothing AndAlso connection.State = ConnectionState.Open Then
                connection.Close()
                connection.Dispose()
            End If
        End Try
    End Function
    
    ''' <summary>
    ''' Updates the DataGridView with the loaded data (executes on UI thread)
    ''' </summary>
    Private Sub UpdateDataGridView(dt As DataTable)
        Try
            ' Suspend layout for better performance
            dgvTeachers.SuspendLayout()
            dgvTeachers.DataSource = Nothing
            
            ' Set new data source
            dgvTeachers.DataSource = dt
            
            ' Optimize column sizing
            If dgvTeachers.Columns.Count > 0 Then
                dgvTeachers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
                For Each col As DataGridViewColumn In dgvTeachers.Columns
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                Next
            End If
            
            ' Resume layout
            dgvTeachers.ResumeLayout(False)
        Catch ex As Exception
            _logger.LogError($"FormFaculty - Error updating DataGridView: {ex.Message}")
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Builds the faculty query string based on filters
    ''' </summary>
    Private Function BuildFacultyQuery(departmentFilter As String, searchTerm As String, statusFilter As String) As String
        Dim baseQuery As String = "
                SELECT ti.teacherID AS teacherID, 
                       ti.employeeID AS employeeID, 
                       " & GetNameConcatSQL() & " AS teacher_name, 
                       ti.email, 
                       ti.gender AS gender, 
                       ti.birthdate AS birthdate,
                       ti.phoneNo AS phoneNo,
                       ti.contactNo AS contactNo, 
                       CONCAT(ti.homeadd, ' ', COALESCE(rb.brgyDesc, ''), '. ', COALESCE(rc.citymunDesc, '')) AS teacher_address, 
                       ti.emergencyContact AS emergencyContact,
                       COALESCE(d.department_name, 'No Dept') AS department_name,
                       CASE WHEN ti.isActive = 1 THEN 'Active' ELSE 'Inactive' END AS status_text
                FROM teacherinformation ti 
                LEFT JOIN refregion rg ON ti.regionID = rg.id 
                LEFT JOIN refprovince rp ON ti.provinceID = rp.id 
                LEFT JOIN refcitymun rc ON ti.cityID = rc.id 
                LEFT JOIN refbrgy rb ON ti.brgyID = rb.id 
                LEFT JOIN departments d ON ti.department_id = d.department_id
                WHERE 1=1"

        If statusFilter = "Active" Then
            baseQuery &= " AND ti.isActive = 1"
        ElseIf statusFilter = "Inactive" Then
            baseQuery &= " AND ti.isActive = 0"
        End If

        If departmentFilter <> "ALL" AndAlso IsNumeric(departmentFilter) Then
            baseQuery &= $" AND ti.department_id = {departmentFilter}"
        End If

        If Not String.IsNullOrWhiteSpace(searchTerm) Then
            baseQuery &= $" AND (ti.lastname LIKE '%{searchTerm}%' OR ti.firstname LIKE '%{searchTerm}%' OR ti.employeeID LIKE '%{searchTerm}%')"
        End If

        baseQuery &= " ORDER BY ti.lastname, ti.firstname"

        Return baseQuery
    End Function

    Private Sub FormFaculty_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _logger.LogInfo("FormFaculty - Form loaded, initializing default settings")

        Me.Cursor = Cursors.WaitCursor
        Try
            DefaultSettings()

            _logger.LogInfo("FormFaculty - Design standards and data loaded successfully")
        Catch ex As Exception
            _logger.LogError($"FormFaculty_Load error: {ex.Message}")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Function GetFacultyStatus(facultyId As Integer) As Integer
        Try
            connectDB()
            Dim cmd As New Odbc.OdbcCommand("SELECT isActive FROM teacherinformation WHERE teacherID = ?", con)
            cmd.Parameters.Add("?", Odbc.OdbcType.Int).Value = facultyId
            Dim result As Object = cmd.ExecuteScalar()
            con.Close()

            Return If(result IsNot Nothing, Convert.ToInt32(result), 0)
        Catch ex As Exception
            _logger.LogError($"FormFaculty - Error getting faculty status (ID: {facultyId}): {ex.Message}")
            Try
                If con IsNot Nothing AndAlso con.State = ConnectionState.Open Then
                    con.Close()
                End If
            Catch
            End Try
            Return 0
        End Try
    End Function

    Private Function GetFacultyName(facultyId As Integer) As String
        Try
            connectDB()
            Dim cmd As New Odbc.OdbcCommand("SELECT CONCAT(firstname, ' ', lastname) AS full_name FROM teacherinformation WHERE teacherID = ?", con)
            cmd.Parameters.Add("?", Odbc.OdbcType.Int).Value = facultyId
            Dim result As Object = cmd.ExecuteScalar()
            con.Close()

            Return If(result IsNot Nothing, result.ToString(), "Unknown Faculty")
        Catch ex As Exception
            _logger.LogError($"FormFaculty - Error getting faculty name (ID: {facultyId}): {ex.Message}")
            Try
                If con IsNot Nothing AndAlso con.State = ConnectionState.Open Then
                    con.Close()
                End If
            Catch
            End Try
            Return "Unknown Faculty"
        End Try
    End Function
    Private Sub EditRecord(ByVal id As Integer)
        Dim cmd As Odbc.OdbcCommand
        Dim da As New Odbc.OdbcDataAdapter
        Dim dt As New DataTable
        Dim facultyName As String = ""

        Try
            _logger.LogInfo($"FormFaculty - Loading faculty record for editing - Faculty ID: {id}")

            connectDB()
            cmd = New Odbc.OdbcCommand("SELECT teacherID, profileImg, employeeID,tagID,firstname,middlename,lastname,extName, email, gender,birthdate,phoneNo,homeadd,brgyID, cityID, provinceID, regionID,contactNo, emergencyContact, relationship, department_id FROM teacherinformation WHERE teacherID=?", con)
            cmd.Parameters.AddWithValue("@", id)
            da.SelectCommand = cmd
            da.Fill(dt)

            If dt.Rows.Count > 0 Then
                facultyName = FormatFullName(dt.Rows(0)("firstname").ToString(), dt.Rows(0)("middlename").ToString(), dt.Rows(0)("lastname").ToString(), dt.Rows(0)("extName").ToString())

                AddFaculty.txtID.Text = dt.Rows(0)("teacherID").ToString()
                AddFaculty.txtEmployeeID.Text = dt.Rows(0)("employeeID").ToString()
                AddFaculty.txtTagID.Text = dt.Rows(0)("tagID").ToString()
                AddFaculty.txtFirstName.Text = dt.Rows(0)("firstname").ToString()
                AddFaculty.txtMiddleName.Text = dt.Rows(0)("middlename").ToString()
                AddFaculty.txtLastName.Text = dt.Rows(0)("lastname").ToString()
                AddFaculty.txtExtName.Text = dt.Rows(0)("extName").ToString()
                AddFaculty.txtEmail.Text = dt.Rows(0)("email").ToString()
                AddFaculty.cbGender.Text = dt.Rows(0)("gender").ToString()
                If Not IsDBNull(dt.Rows(0)("birthdate")) Then
                    Dim birthDate As DateTime
                    If DateTime.TryParse(dt.Rows(0)("birthdate").ToString(), birthDate) Then
                        AddFaculty.dtpBirthdate.Value = birthDate
                    Else
                        AddFaculty.dtpBirthdate.Value = DateTime.Today.AddYears(-25)
                        _logger.LogWarning($"FormFaculty - Invalid birthdate for Faculty ID {id}, using default")
                    End If
                Else
                    AddFaculty.dtpBirthdate.Value = DateTime.Today.AddYears(-25)
                End If
                AddFaculty.txtHome.Text = dt.Rows(0)("homeadd").ToString()

                LoadAddressComboBoxes(dt.Rows(0)("regionID").ToString(), dt.Rows(0)("provinceID").ToString(), dt.Rows(0)("cityID").ToString(), dt.Rows(0)("brgyID").ToString())
                AddFaculty.txtPhoneNo.Text = dt.Rows(0)("phoneNo").ToString()
                AddFaculty.txtContactNo.Text = dt.Rows(0)("contactNo").ToString()
                AddFaculty.txtEmergencyContact.Text = dt.Rows(0)("emergencyContact").ToString()
                AddFaculty.cbRelationship.Text = dt.Rows(0)("relationship").ToString()

                If Not IsDBNull(dt.Rows(0)("department_id")) Then
                    Dim departmentId As Integer = Convert.ToInt32(dt.Rows(0)("department_id"))
                    AddFaculty.SetDepartmentSelection(departmentId)
                Else
                    AddFaculty.SetDepartmentSelection(Nothing)
                End If

                Dim myreader As Odbc.OdbcDataReader = cmd.ExecuteReader
                If myreader.Read Then
                    Try
                        Dim profileImg As Byte()
                        profileImg = myreader("profileImg")
                        Dim ms As New MemoryStream(profileImg)
                        If profileImg IsNot Nothing Then
                            AddFaculty.pbProfile.Image = Image.FromStream(ms)
                        End If
                    Catch ex As Exception
                        _logger.LogWarning($"FormFaculty - Could not load profile image for Faculty ID: {id}")
                    End Try
                End If

                _logger.LogInfo($"FormFaculty - Faculty record loaded for editing - Name: '{facultyName}', Employee ID: '{dt.Rows(0)("employeeID")}'")
                AddFaculty.ShowDialog()
                ' Refresh the list asynchronously after editing to prevent UI blocking
                RefreshFacultyListAsync()
                _logger.LogInfo("FormFaculty - Returned from edit mode, faculty list refresh initiated")
            Else
                _logger.LogWarning($"FormFaculty - No faculty record found with ID: {id}")
            End If
        Catch ex As Exception
            _logger.LogError($"FormFaculty - Error loading faculty record for editing (ID: {id}): {ex.Message}")
            MessageBox.Show(ex.Message.ToString())
        Finally
            ' Removed GC.Collect() - let .NET handle garbage collection automatically
            con.Close()
        End Try
    End Sub

    Private Async Sub txtSearch_TextChanged(sender As Object, e As EventArgs) Handles txtSearch.TextChanged
        Try
            ' Cancel previous search if user is still typing
            If _searchCancellationTokenSource IsNot Nothing Then
                _searchCancellationTokenSource.Cancel()
                _searchCancellationTokenSource.Dispose()
            End If

            ' Create new cancellation token for this search
            _searchCancellationTokenSource = New CancellationTokenSource()
            Dim token = _searchCancellationTokenSource.Token

            ' Wait for debounce delay
            Await Task.Delay(_searchDelayMs, token)

            ' If not cancelled, perform search
            If Not token.IsCancellationRequested Then
                RefreshFacultyListAsync()
            End If
        Catch ex As OperationCanceledException
            ' Expected when search is cancelled - ignore
        Catch ex As Exception
            _logger.LogError($"FormFaculty - Error during search operation: {ex.Message}")
        End Try
    End Sub

    Private Sub dgvTeachers_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvTeachers.CellClick
        Try
            If e.RowIndex >= 0 Then
                dgvTeachers.Tag = dgvTeachers.Item(0, e.RowIndex).Value
                _logger.LogInfo($"FormFaculty - Faculty selected - Faculty ID: {dgvTeachers.Tag}")

                ' Update toggle button based on selected row status
                UpdateToggleButtonState(e.RowIndex)
            End If
        Catch ex As Exception
            _logger.LogWarning($"FormFaculty - Error selecting faculty record: {ex.Message}")
        End Try
    End Sub

    Private Sub dgvTeachers_DataBindingComplete(sender As Object, e As DataGridViewBindingCompleteEventArgs) Handles dgvTeachers.DataBindingComplete
        dgvTeachers.CurrentCell = Nothing
    End Sub

    Private Sub dgvTeachers_SelectionChanged(sender As Object, e As EventArgs) Handles dgvTeachers.SelectionChanged
        Try
            If dgvTeachers.SelectedRows.Count > 0 Then
                Dim selectedRowIndex As Integer = dgvTeachers.SelectedRows(0).Index
                UpdateToggleButtonState(selectedRowIndex)
            Else
                ResetToggleButtonToDefault()
            End If
        Catch ex As Exception
            _logger.LogError($"FormFaculty - Error in selection changed: {ex.Message}")
            ResetToggleButtonToDefault()
        End Try
    End Sub

    Private Sub UpdateToggleButtonState(rowIndex As Integer)
        Try
            If rowIndex >= 0 AndAlso rowIndex < dgvTeachers.Rows.Count Then
                Dim row As DataGridViewRow = dgvTeachers.Rows(rowIndex)

                If row.Cells("ColumnStatus") IsNot Nothing AndAlso row.Cells("ColumnStatus").Value IsNot Nothing Then
                    Dim status As String = row.Cells("ColumnStatus").Value.ToString()
                    Dim facultyName As String = If(row.Cells("Column3") IsNot Nothing AndAlso row.Cells("Column3").Value IsNot Nothing,
                                                 row.Cells("Column3").Value.ToString(), "Selected Faculty")

                    If status = "Active" Then
                        btnToggleStat.Text = "&Disable Record"
                        btnToggleStat.ForeColor = Color.Red
                        'btnToggleStat.BackgroundImage = GetDisableIcon()
                        _logger.LogInfo($"FormFaculty - Toggle button set to 'Disable' for active faculty: {facultyName}")
                    Else
                        btnToggleStat.Text = "&Enable Record"
                        btnToggleStat.ForeColor = Color.LimeGreen
                        'btnToggleStat.BackgroundImage = GetEnableIcon()
                        _logger.LogInfo($"FormFaculty - Toggle button set to 'Enable' for inactive faculty: {facultyName}")
                    End If

                    btnToggleStat.Enabled = True
                End If
            End If
        Catch ex As Exception
            _logger.LogError($"FormFaculty - Error updating toggle button state: {ex.Message}")
            ResetToggleButtonToDefault()
        End Try
    End Sub

    Private Sub ResetToggleButtonToDefault()
        Try
            'btnToggleStat.BackgroundImage = My.Resources.enable_default_40x40
            btnToggleStat.Text = "&Select Faculty"
            btnToggleStat.ForeColor = Color.DimGray
            btnToggleStat.Enabled = False
            _logger.LogInfo("FormFaculty - Toggle button reset to default state")
        Catch ex As Exception
            _logger.LogError($"FormFaculty - Error resetting toggle button: {ex.Message}")
        End Try
    End Sub

    Private Function GetEnableIcon() As Image
        Try
            Return My.Resources.enable
        Catch ex As Exception
            _logger.LogError($"FormFaculty - Error loading enable icon from resources: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Function GetDisableIcon() As Image
        Try
            Return My.Resources.disable_40x40
        Catch ex As Exception
            _logger.LogError($"FormFaculty - Error loading disable icon from resources: {ex.Message}")
            Return Nothing
        End Try
    End Function

    Private Sub LoadStatusFilter()
        Try
            _logger.LogInfo("FormFaculty - Loading status filter")

            cboStatusFilter.Items.Clear()
            cboStatusFilter.Items.AddRange({"All", "Active", "Inactive"})
            cboStatusFilter.SelectedIndex = 0

            _logger.LogInfo("FormFaculty - Status filter initialized successfully")
        Catch ex As Exception
            _logger.LogError($"FormFaculty - Error loading status filter: {ex.Message}")
        End Try
    End Sub

    Private Sub LoadDepartmentFilter()
        Try
            _logger.LogInfo("FormFaculty - Loading departments for filtering")

            Dim dt As New DataTable()
            dt.Columns.Add("department_id", GetType(String))
            dt.Columns.Add("department_display", GetType(String))

            Dim allRow As DataRow = dt.NewRow()
            allRow("department_id") = "ALL"
            allRow("department_display") = "All Departments"
            dt.Rows.Add(allRow)

            Dim departmentService As New DepartmentService()
            Dim departments = departmentService.GetActiveDepartments()

            If departments IsNot Nothing AndAlso departments.Count > 0 Then
                For Each dept As Department In departments
                    Dim row As DataRow = dt.NewRow()
                    row("department_id") = dept.DepartmentId.ToString()
                    row("department_display") = $"{dept.DepartmentCode} - {dept.DepartmentName}"
                    dt.Rows.Add(row)
                Next

                _logger.LogInfo($"FormFaculty - {departments.Count} departments loaded for filtering")
            Else
                _logger.LogWarning("FormFaculty - No departments found for filtering")
            End If

            cboDepartment.DataSource = dt
            cboDepartment.ValueMember = "department_id"
            cboDepartment.DisplayMember = "department_display"
            cboDepartment.SelectedIndex = 0

            _logger.LogInfo("FormFaculty - Department filter ComboBox populated successfully")

        Catch ex As Exception
            _logger.LogError($"FormFaculty - Error loading department filter: {ex.Message}")

            Dim errorDt As New DataTable()
            errorDt.Columns.Add("department_id", GetType(String))
            errorDt.Columns.Add("department_display", GetType(String))

            Dim errorRow As DataRow = errorDt.NewRow()
            errorRow("department_id") = "ALL"
            errorRow("department_display") = "All Departments"
            errorDt.Rows.Add(errorRow)

            cboDepartment.DataSource = errorDt
            cboDepartment.ValueMember = "department_id"
            cboDepartment.DisplayMember = "department_display"
            cboDepartment.SelectedIndex = 0
        End Try
    End Sub

    Private Sub LoadFacultyList(Optional departmentFilter As String = "ALL", Optional searchTerm As String = "", Optional statusFilter As String = "All")
        Try
            Dim baseQuery As String = "
                SELECT ti.teacherID AS teacherID, 
                       ti.employeeID AS employeeID, 
                       " & GetNameConcatSQL() & " AS teacher_name, 
                       ti.email, 
                       ti.gender AS gender, 
                       ti.birthdate AS birthdate,
                       ti.phoneNo AS phoneNo,
                       ti.contactNo AS contactNo, 
                       CONCAT(ti.homeadd, ' ', COALESCE(rb.brgyDesc, ''), '. ', COALESCE(rc.citymunDesc, '')) AS teacher_address, 
                       ti.emergencyContact AS emergencyContact,
                       COALESCE(d.department_name, 'No Dept') AS department_name,
                       CASE WHEN ti.isActive = 1 THEN 'Active' ELSE 'Inactive' END AS status_text
                FROM teacherinformation ti 
                LEFT JOIN refregion rg ON ti.regionID = rg.id 
                LEFT JOIN refprovince rp ON ti.provinceID = rp.id 
                LEFT JOIN refcitymun rc ON ti.cityID = rc.id 
                LEFT JOIN refbrgy rb ON ti.brgyID = rb.id 
                LEFT JOIN departments d ON ti.department_id = d.department_id
                WHERE 1=1"

            If statusFilter = "Active" Then
                baseQuery &= " AND ti.isActive = 1"
                _logger.LogInfo("FormFaculty - Filtering by status: Active")
            ElseIf statusFilter = "Inactive" Then
                baseQuery &= " AND ti.isActive = 0"
                _logger.LogInfo("FormFaculty - Filtering by status: Inactive")
            Else
                _logger.LogInfo("FormFaculty - Showing all faculty (Active and Inactive)")
            End If

            If departmentFilter <> "ALL" AndAlso IsNumeric(departmentFilter) Then
                baseQuery &= $" AND ti.department_id = {departmentFilter}"
                _logger.LogInfo($"FormFaculty - Filtering by department ID: {departmentFilter}")
            ElseIf departmentFilter <> "ALL" Then
                _logger.LogWarning($"FormFaculty - Invalid department filter: {departmentFilter}")
            End If

            If Not String.IsNullOrWhiteSpace(searchTerm) Then
                baseQuery &= $" AND (ti.lastname LIKE '%{searchTerm}%' OR ti.firstname LIKE '%{searchTerm}%' OR ti.employeeID LIKE '%{searchTerm}%')"
                _logger.LogInfo($"FormFaculty - Applying search filter: '{searchTerm}'")
            End If

            baseQuery &= " ORDER BY ti.lastname, ti.firstname"

            loadDGV(baseQuery, dgvTeachers)

            _logger.LogInfo($"FormFaculty - Faculty list loaded with filters - Department: {departmentFilter}, Status: {statusFilter}, Search: '{searchTerm}', Results: {dgvTeachers.Rows.Count}")

        Catch ex As Exception
            _logger.LogError($"FormFaculty - Error loading faculty list: {ex.Message}")
            MessageBox.Show("Error loading faculty list. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub cboDepartment_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboDepartment.SelectedIndexChanged
        Try
            If cboDepartment.SelectedValue IsNot Nothing Then
                _logger.LogInfo($"FormFaculty - Department filter changed to: {cboDepartment.Text}")
                ' Use async refresh to prevent UI blocking
                RefreshFacultyListAsync()
            End If
        Catch ex As Exception
            _logger.LogError($"FormFaculty - Error in department filter change: {ex.Message}")
        End Try
    End Sub

    Private Sub cboStatusFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboStatusFilter.SelectedIndexChanged
        Try
            If cboStatusFilter.SelectedItem IsNot Nothing Then
                _logger.LogInfo($"FormFaculty - Status filter changed to: {cboStatusFilter.SelectedItem.ToString()}")
                ' Use async refresh to prevent UI blocking
                RefreshFacultyListAsync()
            End If
        Catch ex As Exception
            _logger.LogError($"FormFaculty - Error in status filter change: {ex.Message}")
        End Try
    End Sub

    Private Sub btnFullDetails_Click(sender As Object, e As EventArgs) Handles btnFullDetails.Click
        Try
            Dim facultyId As Integer = CInt(dgvTeachers.Tag)

            If dgvTeachers.Tag Is Nothing OrElse Not IsNumeric(dgvTeachers.Tag) OrElse facultyId <= 0 Then
                _logger.LogWarning("FormFaculty - Full Details attempted with no faculty selected")
                MessageBox.Show("Please select a faculty member to view full details.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            _logger.LogInfo($"FormFaculty - Opening Full Details for Faculty ID: {facultyId}")
            Dim detailsForm As New FormFacultyDetails(facultyId)
            detailsForm.ShowDialog()
            _logger.LogInfo("FormFaculty - Returned from Full Details form")

        Catch ex As Exception
            _logger.LogError($"FormFaculty - Error opening Full Details: {ex.Message}")
            MessageBox.Show("Error opening faculty details: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnGenerateReport_Click(sender As Object, e As EventArgs) Handles btnGenerateReport.Click
        Try
            If dgvTeachers.Rows.Count = 0 Then
                MessageBox.Show("No data to generate report.", "Generate Report", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim query As String = "
                SELECT 
                    t.teacherID,
                    t.employeeID,
                    CONCAT(t.lastname, ', ', t.firstname, ' ', IFNULL(t.middlename, '')) AS fullname,
                    IFNULL(d.department_name, 'N/A') AS department,
                    t.email,
                    t.phoneNo,
                    t.contactNo,
                    CASE WHEN t.isActive = 1 THEN 'Active' ELSE 'Inactive' END AS status
                FROM teacherinformation t
                LEFT JOIN departments d ON t.department_id = d.department_id
                WHERE 1=1"

            Dim parameters As New List(Of Object)

            If cboStatusFilter.SelectedItem IsNot Nothing Then
                Dim statusFilter As String = cboStatusFilter.SelectedItem.ToString()
                If statusFilter = "Active" Then
                    query &= " AND t.isActive = 1"
                ElseIf statusFilter = "Inactive" Then
                    query &= " AND t.isActive = 0"
                End If
            End If

            If cboDepartment.SelectedValue IsNot Nothing Then
                Dim departmentFilter As String = cboDepartment.SelectedValue.ToString()
                If departmentFilter <> "ALL" AndAlso IsNumeric(departmentFilter) Then
                    query &= $" AND t.department_id = {departmentFilter}"
                End If
            End If

            If Not String.IsNullOrWhiteSpace(txtSearch.Text) Then
                query &= " AND (CONCAT(t.firstname, ' ', t.lastname) LIKE ? OR t.employeeID LIKE ? OR t.email LIKE ? OR d.department_name LIKE ?)"
                Dim searchTerm As String = "%" & txtSearch.Text.Trim() & "%"
                parameters.Add(searchTerm)
                parameters.Add(searchTerm)
                parameters.Add(searchTerm)
                parameters.Add(searchTerm)
            End If

            query &= " ORDER BY t.lastname, t.firstname"

            Dim dt As DataTable = Nothing
            connectDB()
            Dim cmd As New OdbcCommand(query, con)
            For Each p In parameters
                cmd.Parameters.AddWithValue("?", p)
            Next
            Dim da As New OdbcDataAdapter(cmd)
            dt = New DataTable()
            da.Fill(dt)
            con.Close()

            If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                MessageBox.Show("No data matched the current filters.", "Generate Report", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim reportForm As New Form()
            reportForm.Text = "Faculty List Report"
            reportForm.Size = New Size(1024, 768)
            reportForm.StartPosition = FormStartPosition.CenterScreen
            reportForm.WindowState = FormWindowState.Maximized
            reportForm.TopMost = True

            Dim reportViewer As New Microsoft.Reporting.WinForms.ReportViewer()
            reportViewer.Dock = DockStyle.Fill
            reportForm.Controls.Add(reportViewer)

            Try
                AddHandler reportViewer.ReportError, Sub(sender2 As Object, e2 As Microsoft.Reporting.WinForms.ReportErrorEventArgs)
                                                         Try
                                                             Dim ex = e2.Exception
                                                             If ex IsNot Nothing Then
                                                                 _logger.LogError($"ReportViewer.ReportError: {BuildExceptionDetails(ex)}")
                                                             Else
                                                                 _logger.LogError("ReportViewer.ReportError triggered with no exception details.")
                                                                 _logger.LogError("ReportViewer.ReportError triggered with no exception details.")
                                                             End If
                                                         Catch handlerEx As Exception
                                                             _logger.LogError($"ReportViewer.ReportError handler failure: {handlerEx.Message}")
                                                         End Try
                                                     End Sub

                AddHandler reportViewer.RenderingComplete, Sub(sender3 As Object, e3 As Microsoft.Reporting.WinForms.RenderingCompleteEventArgs)
                                                               Try
                                                                   If e3 IsNot Nothing AndAlso e3.Exception IsNot Nothing Then
                                                                       _logger.LogError($"ReportViewer.RenderingComplete exception: {BuildExceptionDetails(e3.Exception)}")
                                                                   Else
                                                                       _logger.LogInfo("ReportViewer.RenderingComplete successful")
                                                                   End If
                                                               Catch handlerEx As Exception
                                                                   _logger.LogError($"ReportViewer.RenderingComplete handler failure: {handlerEx.Message}")
                                                               End Try
                                                           End Sub

                AddHandler reportForm.Shown, Sub()
                                                 _logger.LogInfo("Report form shown")
                                             End Sub
                AddHandler reportForm.FormClosed, Sub()
                                                      _logger.LogInfo("Report form closed")
                                                  End Sub
            Catch hookEx As Exception
                _logger.LogError($"Error attaching ReportViewer/Form handlers: {hookEx.Message}")
            End Try

            Dim rdlcPath As String = System.IO.Path.Combine(Application.StartupPath, "ReportFaculty.rdlc")
            If Not System.IO.File.Exists(rdlcPath) Then
                Dim alt1 As String = System.IO.Path.Combine(Application.StartupPath, "ReportFacultyList.rdlc")
                Dim alt2 As String = "ReportFaculty.rdlc"
                Dim alt3 As String = "ReportFacultyList.rdlc"
                If System.IO.File.Exists(alt1) Then
                    rdlcPath = alt1
                ElseIf System.IO.File.Exists(alt2) Then
                    rdlcPath = alt2
                Else
                    rdlcPath = alt3
                End If
            End If

            reportViewer.LocalReport.ReportPath = rdlcPath
            reportViewer.LocalReport.DataSources.Clear()
            Dim rds As New Microsoft.Reporting.WinForms.ReportDataSource("DataSetFacultyList", dt)
            reportViewer.LocalReport.DataSources.Add(rds)
            Try
                Dim total As String = dt.Rows.Count.ToString()
                Dim p As New List(Of Microsoft.Reporting.WinForms.ReportParameter)
                p.Add(New Microsoft.Reporting.WinForms.ReportParameter("TotalRecords", total))
                reportViewer.LocalReport.SetParameters(p)
            Catch setParamEx As Exception
                _logger.LogError($"Setting report parameters failed: {BuildExceptionDetails(setParamEx)}")
            End Try

            Try
                _logger.LogInfo($"Report RDLC path: {rdlcPath}, exists: {System.IO.File.Exists(rdlcPath)}")
                _logger.LogInfo($"Report dataset rows: {dt.Rows.Count}")
                Dim cols = String.Join(", ", dt.Columns.Cast(Of DataColumn)().Select(Function(c) c.ColumnName))
                _logger.LogInfo($"Report dataset columns: {cols}")
            Catch diagEx As Exception
                _logger.LogError($"Diagnostics logging failed: {diagEx.Message}")
            End Try

            Try
                reportViewer.RefreshReport()
            Catch rvEx As Exception
                _logger.LogError($"ReportViewer.RefreshReport error: {BuildExceptionDetails(rvEx)}")
                Throw
            End Try

            Try
                reportForm.ShowDialog()
            Catch formEx As Exception
                _logger.LogError($"Report form ShowDialog error: {BuildExceptionDetails(formEx)}")
                Throw
            End Try

        Catch ex As Exception
            _logger.LogError($"FormFaculty - Error generating report: {BuildExceptionDetails(ex)}")
        End Try
    End Sub

    Private Function BuildExceptionDetails(ex As Exception) As String
        Try
            Dim sb As New System.Text.StringBuilder()
            Dim level As Integer = 0
            Dim cur As Exception = ex
            While cur IsNot Nothing AndAlso level < 10
                If level = 0 Then
                    sb.AppendLine($"Message: {cur.Message}")
                Else
                    sb.AppendLine($"Inner[{level}] Message: {cur.Message}")
                End If
                If cur.StackTrace IsNot Nothing Then
                    sb.AppendLine("StackTrace:")
                    sb.AppendLine(cur.StackTrace)
                End If
                If cur.Data IsNot Nothing AndAlso cur.Data.Count > 0 Then
                    sb.AppendLine("Data:")
                    For Each key In cur.Data.Keys
                        sb.AppendLine($"  {key}: {cur.Data(key)}")
                    Next
                End If
                cur = cur.InnerException
                level += 1
                If cur IsNot Nothing Then sb.AppendLine("---")
            End While
            Return sb.ToString().TrimEnd()
        Catch
            Return ex.ToString()
        End Try
    End Function

    Private Sub btnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
        _logger.LogInfo("FormFaculty - Add Faculty button clicked, opening AddFaculty form")
        AddFaculty.ShowDialog()
        ' Refresh faculty list asynchronously to prevent UI blocking
        RefreshFacultyListAsync()
        _logger.LogInfo("FormFaculty - Returned from AddFaculty form, faculty list refresh initiated")
    End Sub

    Private Sub btnEditRecord_Click(sender As Object, e As EventArgs) Handles btnEditRecord.Click
        Dim facultyId As Integer = CInt(dgvTeachers.Tag)

        If dgvTeachers.Tag > 0 Then
            _logger.LogInfo($"FormFaculty - Edit button clicked for Faculty ID: {facultyId}")
            If MessageBox.Show("Are you sure you want to edit this record?", "Edit Record", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                _logger.LogInfo($"FormFaculty - User confirmed edit for Faculty ID: {facultyId}")
                EditRecord(Val(dgvTeachers.Tag))
            Else
                _logger.LogInfo($"FormFaculty - User cancelled edit for Faculty ID: {facultyId}")
            End If
        Else
            _logger.LogWarning("FormFaculty - Edit attempted with no faculty selected")
            MessageBox.Show("Please select a record you want to edit", "Edit Record", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub

    Private Sub btnToggleStat_Click(sender As Object, e As EventArgs) Handles btnToggleStat.Click
        Dim cmd As Odbc.OdbcCommand
        Dim facultyId As Integer = 0

        Try
            If dgvTeachers.Tag IsNot Nothing AndAlso IsNumeric(dgvTeachers.Tag) Then
                facultyId = CInt(dgvTeachers.Tag)
            End If

            _logger.LogInfo($"FormFaculty - Toggle Status button clicked for Faculty ID: {facultyId}")

            If facultyId > 0 Then
                Dim currentStatus As Integer = GetFacultyStatus(facultyId)
                Dim action As String = If(currentStatus = 1, "disable", "enable")
                Dim actionTitle As String = If(currentStatus = 1, "Disable", "Enable")

                Dim facultyName As String = GetFacultyName(facultyId)

                Dim result As DialogResult = MessageBox.Show(
                    $"Are you sure you want to {action} '{facultyName}'?{Environment.NewLine}{Environment.NewLine}" &
                    $"This will {If(currentStatus = 1, "deactivate the faculty member and they will no longer appear in active lists", "reactivate the faculty member and they will appear in active lists again")}.",
                    $"Confirm {actionTitle}",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question)

                If result = DialogResult.Yes Then
                    _logger.LogInfo($"FormFaculty - User confirmed {action} for Faculty ID: {facultyId}")

                    connectDB()
                    cmd = New Odbc.OdbcCommand("UPDATE teacherinformation SET isActive = IF(isActive = 1, 0, 1) WHERE teacherID = ?", con)
                    cmd.Parameters.Add("?", Odbc.OdbcType.Int).Value = facultyId
                    cmd.ExecuteNonQuery()
                    con.Close()

                    Dim newStatus As String = If(currentStatus = 1, "disabled", "enabled")

                    _auditLogger.LogUpdate(MainForm.currentUsername, "Faculty",
                        $"{If(currentStatus = 1, "Disabled", "Enabled")} faculty member '{facultyName}' (ID: {facultyId})")

                    _logger.LogInfo($"FormFaculty - Faculty status toggled successfully - Faculty ID: {facultyId}, Status: {newStatus}")
                    MessageBox.Show($"Faculty member has been {newStatus} successfully.", "Status Updated", MessageBoxButtons.OK, MessageBoxIcon.Information)

                    ' Refresh faculty list asynchronously to prevent UI blocking
                    RefreshFacultyListAsync()
                Else
                    _logger.LogInfo($"FormFaculty - User cancelled {action} for Faculty ID: {facultyId}")
                End If
            Else
                _logger.LogWarning("FormFaculty - Toggle Status attempted with no faculty selected")
                MessageBox.Show("Please select a faculty member to change their status.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As Exception
            _logger.LogError($"FormFaculty - Error toggling faculty status (ID: {facultyId}): {ex.Message}")
            MessageBox.Show("Error updating faculty status: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Try
                If con IsNot Nothing AndAlso con.State = ConnectionState.Open Then
                    con.Close()
                End If
            Catch
            End Try
            ' Removed GC.Collect() - let .NET handle garbage collection automatically
        End Try
    End Sub
End Class