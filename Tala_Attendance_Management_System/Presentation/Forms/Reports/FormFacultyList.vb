Imports System.Data.Odbc

Public Class FormFacultyList
    Private ReadOnly _logger As ILogger = LoggerFactory.Instance
    Private searchTimer As Timer

    Private Sub FormFacultyList_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            _logger.LogInfo("========== FormFacultyList Loading ==========")
            _logger.LogInfo("Initializing form controls...")
            InitializeForm()
            
            _logger.LogInfo("Loading faculty data...")
            LoadFacultyList()
            ' Clear any selection on initial load
            Try
                dgvFaculty.ClearSelection()
                dgvFaculty.CurrentCell = Nothing
            Catch
            End Try
            
            _logger.LogInfo("========== FormFacultyList Loaded Successfully ==========")
        Catch ex As Exception
            _logger.LogError($"Critical error loading FormFacultyList: {ex.Message}")
            _logger.LogError($"Stack trace: {ex.StackTrace}")
            MessageBox.Show($"Error loading Faculty List form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub InitializeForm()
        Try
            _logger.LogDebug("Setting up DataGridView properties...")
            
            ' Setup DataGridView
            dgvFaculty.AutoGenerateColumns = False
            dgvFaculty.AllowUserToAddRows = False
            dgvFaculty.ReadOnly = True
            dgvFaculty.SelectionMode = DataGridViewSelectionMode.FullRowSelect
            dgvFaculty.RowTemplate.Height = 40
            _logger.LogDebug("DataGridView basic properties set")

            ' Set column header style - keep it static blue
            _logger.LogDebug("Configuring column header styles...")
            With dgvFaculty.ColumnHeadersDefaultCellStyle
                .Font = New Font("Segoe UI Semibold", 11)
                .Alignment = DataGridViewContentAlignment.MiddleLeft
                .BackColor = Color.FromArgb(52, 152, 219) ' Blue color
                .ForeColor = Color.White
                .SelectionBackColor = Color.FromArgb(52, 152, 219) ' Same as BackColor - prevents highlighting
                .SelectionForeColor = Color.White ' Same as ForeColor
            End With

            dgvFaculty.DefaultCellStyle.Font = New Font("Segoe UI", 10)
            dgvFaculty.EnableHeadersVisualStyles = False
            _logger.LogDebug("Column header styles configured")

            ' Setup status filter combo box
            _logger.LogDebug("Setting up status filter combo box...")
            cboStatusFilter.Items.Clear()
            cboStatusFilter.Items.AddRange({"All Status", "Active", "Inactive"})
            cboStatusFilter.SelectedIndex = 0
            _logger.LogDebug($"Status filter initialized with {cboStatusFilter.Items.Count} items")

            _logger.LogInfo("✓ FormFacultyList initialized successfully")
        Catch ex As Exception
            _logger.LogError($"Error in InitializeForm: {ex.Message}")
            Throw
        End Try
    End Sub

    Private Sub LoadFacultyList()
        Try
            _logger.LogInfo("========== Loading Faculty List ==========")
            
            ' Log filter settings
            Dim statusFilter As String = If(cboStatusFilter.SelectedIndex >= 0, cboStatusFilter.SelectedItem.ToString(), "None")
            Dim searchText As String = If(String.IsNullOrWhiteSpace(txtSearch.Text), "None", txtSearch.Text.Trim())
            _logger.LogInfo($"Filters - Status: {statusFilter}, Search: {searchText}")

            Dim query As String = "
                SELECT 
                    t.teacherID,
                    t.employeeID,
                    CONCAT(t.lastname, ', ', t.firstname, ' ', IFNULL(t.middlename, '')) AS fullname,
                    IFNULL(d.department_name, 'N/A') AS department,
                    t.email,
                    t.contactNo,
                    CASE WHEN t.isActive = 1 THEN 'Active' ELSE 'Inactive' END AS status,
                    t.isActive
                FROM teacherinformation t
                LEFT JOIN departments d ON t.department_id = d.department_id
                WHERE 1=1
            "

            Dim parameters As New List(Of Object)

            ' Add status filter
            If cboStatusFilter.SelectedIndex = 1 Then ' Active
                query &= " AND t.isActive = 1"
                _logger.LogDebug("Applied filter: Active only")
            ElseIf cboStatusFilter.SelectedIndex = 2 Then ' Inactive
                query &= " AND t.isActive = 0"
                _logger.LogDebug("Applied filter: Inactive only")
            End If

            ' Add search filter
            If Not String.IsNullOrWhiteSpace(txtSearch.Text) Then
                query &= " AND (
                    CONCAT(t.firstname, ' ', t.lastname) LIKE ? OR
                    t.employeeID LIKE ? OR
                    t.email LIKE ? OR
                    d.department_name LIKE ?
                )"
                Dim searchTerm As String = "%" & txtSearch.Text.Trim() & "%"
                parameters.Add(searchTerm)
                parameters.Add(searchTerm)
                parameters.Add(searchTerm)
                parameters.Add(searchTerm)
                _logger.LogDebug($"Applied search filter: {searchTerm}")
            End If

            query &= " ORDER BY t.lastname, t.firstname"
            
            _logger.LogDebug($"Executing query with {parameters.Count} parameters")

            Dim dbContext As New DatabaseContext()
            Dim dt = dbContext.ExecuteQuery(query, parameters.ToArray())
            
            _logger.LogInfo($"Query returned {dt.Rows.Count} records")

            If dt.Rows.Count = 0 Then
                _logger.LogWarning("No faculty records found matching the criteria")
            End If

            _logger.LogDebug("Binding data to DataGridView...")
            dgvFaculty.DataSource = dt

            ' Apply alternating row colors (white and light gray)
            ' Don't override with status colors - keep it clean and simple
            _logger.LogDebug("Applying alternating row colors...")

            ' Update record count - use safe conversion
            _logger.LogDebug("Calculating active/inactive counts...")
            Dim activeCount As Integer = 0
            Dim inactiveCount As Integer = 0
            
            For Each row As DataRow In dt.Rows
                Try
                    Dim isActiveValue = row("isActive")
                    ' Handle different data types (TINYINT, BIT, INT, etc.)
                    Dim isActive As Boolean = False
                    If TypeOf isActiveValue Is Boolean Then
                        isActive = CBool(isActiveValue)
                    ElseIf TypeOf isActiveValue Is Byte Then
                        isActive = CByte(isActiveValue) = 1
                    ElseIf TypeOf isActiveValue Is Integer Then
                        isActive = CInt(isActiveValue) = 1
                    ElseIf TypeOf isActiveValue Is Long Then
                        isActive = CLng(isActiveValue) = 1
                    Else
                        isActive = Convert.ToInt32(isActiveValue) = 1
                    End If
                    
                    If isActive Then
                        activeCount += 1
                    Else
                        inactiveCount += 1
                    End If
                Catch ex As Exception
                    _logger.LogWarning($"Could not determine isActive status for row: {ex.Message}")
                    inactiveCount += 1 ' Default to inactive if can't determine
                End Try
            Next
            
            lblRecordCount.Text = $"Total Records: {dt.Rows.Count} | Active: {activeCount} | Inactive: {inactiveCount}"
            _logger.LogInfo($"✓ Successfully loaded {dt.Rows.Count} faculty records (Active: {activeCount}, Inactive: {inactiveCount})")
            _logger.LogInfo("========== Faculty List Load Complete ==========")

        Catch ex As Exception
            _logger.LogError($"Error loading faculty list: {ex.Message}")
            MessageBox.Show("Error loading faculty list: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        _logger.LogInfo("Refresh button clicked")
        LoadFacultyList()
    End Sub

    Private Sub cboStatusFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboStatusFilter.SelectedIndexChanged
        _logger.LogInfo($"Status filter changed to: {cboStatusFilter.SelectedItem}")
        LoadFacultyList()
    End Sub

    Private Sub txtSearch_TextChanged(sender As Object, e As EventArgs) Handles txtSearch.TextChanged
        ' Debounce search
        If searchTimer IsNot Nothing Then
            searchTimer.Stop()
        End If
        searchTimer = New Timer()
        searchTimer.Interval = 500
        AddHandler searchTimer.Tick, Sub()
                                          searchTimer.Stop()
                                          LoadFacultyList()
                                      End Sub
        searchTimer.Start()
    End Sub

    Private Sub btnExport_Click(sender As Object, e As EventArgs) Handles btnExport.Click
        Try
            _logger.LogInfo("Export button clicked")
            
            If dgvFaculty.Rows.Count = 0 Then
                _logger.LogWarning("Export cancelled: No data to export")
                MessageBox.Show("No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            
            _logger.LogInfo($"Preparing to export {dgvFaculty.Rows.Count} records")

            Dim saveDialog As New SaveFileDialog()
            saveDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv"
            saveDialog.FilterIndex = 1
            saveDialog.FileName = $"FacultyList_{DateTime.Now:yyyyMMdd}"

            If saveDialog.ShowDialog() = DialogResult.OK Then
                _logger.LogInfo($"Export file selected: {saveDialog.FileName}")
                _logger.LogInfo($"Export format: {If(saveDialog.FilterIndex = 1, "Excel", "CSV")}")
                
                If saveDialog.FilterIndex = 1 Then
                    ExportToExcel(saveDialog.FileName)
                Else
                    ExportToCSV(saveDialog.FileName)
                End If

                MessageBox.Show("Faculty list exported successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
                _logger.LogInfo($"✓ Faculty list exported successfully to: {saveDialog.FileName}")
            Else
                _logger.LogInfo("Export cancelled by user")
            End If

        Catch ex As Exception
            _logger.LogError($"❌ Error exporting faculty list: {ex.Message}")
            _logger.LogError($"Stack trace: {ex.StackTrace}")
            MessageBox.Show("Error exporting faculty list: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ExportToExcel(filePath As String)
        Try
            Dim excelApp As Object = CreateObject("Excel.Application")
            Dim workbook As Object = excelApp.Workbooks.Add()
            Dim worksheet As Object = workbook.Worksheets(1)

            ' Write headers
            Dim col As Integer = 1
            For Each column As DataGridViewColumn In dgvFaculty.Columns
                If column.Visible AndAlso column.Name <> "colTeacherID" Then
                    worksheet.Cells(1, col).Value = column.HeaderText
                    worksheet.Cells(1, col).Font.Bold = True
                    worksheet.Cells(1, col).Interior.Color = RGB(52, 73, 94)
                    worksheet.Cells(1, col).Font.Color = RGB(255, 255, 255)
                    col += 1
                End If
            Next

            ' Write data with color coding
            Dim row As Integer = 2
            For Each dgvRow As DataGridViewRow In dgvFaculty.Rows
                If dgvRow.IsNewRow Then Continue For

                col = 1
                For Each column As DataGridViewColumn In dgvFaculty.Columns
                    If column.Visible AndAlso column.Name <> "colTeacherID" Then
                        Dim cellValue = dgvRow.Cells(column.Index).Value
                        worksheet.Cells(row, col).Value = If(cellValue Is Nothing OrElse IsDBNull(cellValue), "", cellValue.ToString())

                        ' Apply color coding based on status
                        Dim status As String = dgvRow.Cells("colStatus").Value?.ToString()
                        If status = "Active" Then
                            worksheet.Cells(row, col).Interior.Color = RGB(212, 237, 218)
                            worksheet.Cells(row, col).Font.Color = RGB(27, 94, 32)
                        ElseIf status = "Inactive" Then
                            worksheet.Cells(row, col).Interior.Color = RGB(248, 215, 218)
                            worksheet.Cells(row, col).Font.Color = RGB(114, 28, 36)
                        End If

                        col += 1
                    End If
                Next
                row += 1
            Next

            ' Auto-fit columns
            worksheet.Columns.AutoFit()

            ' Add borders
            Dim lastCol As Integer = col - 1
            Dim lastRow As Integer = row - 1
            Dim range = worksheet.Range(worksheet.Cells(1, 1), worksheet.Cells(lastRow, lastCol))
            range.Borders.LineStyle = 1

            ' Save and close
            workbook.SaveAs(filePath)
            workbook.Close(False)
            excelApp.Quit()

            ' Clean up
            System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet)
            System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook)
            System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp)

        Catch ex As Exception
            _logger.LogWarning($"Excel export failed, falling back to CSV: {ex.Message}")
            ExportToCSV(filePath.Replace(".xlsx", ".csv"))
        End Try
    End Sub

    Private Sub ExportToCSV(filePath As String)
        Using writer As New System.IO.StreamWriter(filePath, False, New System.Text.UTF8Encoding(True))
            ' Write headers
            Dim headers As New List(Of String)
            For Each column As DataGridViewColumn In dgvFaculty.Columns
                If column.Visible AndAlso column.Name <> "colTeacherID" Then
                    headers.Add(column.HeaderText)
                End If
            Next
            writer.WriteLine(String.Join(",", headers))

            ' Write data
            For Each row As DataGridViewRow In dgvFaculty.Rows
                If row.IsNewRow Then Continue For

                Dim values As New List(Of String)
                For Each column As DataGridViewColumn In dgvFaculty.Columns
                    If column.Visible AndAlso column.Name <> "colTeacherID" Then
                        Dim cellValue = row.Cells(column.Index).Value
                        Dim value As String = If(cellValue Is Nothing OrElse IsDBNull(cellValue), "", cellValue.ToString().Trim())
                        values.Add(EscapeCsvValue(value))
                    End If
                Next
                writer.WriteLine(String.Join(",", values))
            Next
        End Using
    End Sub

    Private Function EscapeCsvValue(value As String) As String
        If String.IsNullOrEmpty(value) Then Return ""
        If value.Contains(",") OrElse value.Contains("""") OrElse value.Contains(vbCr) OrElse value.Contains(vbLf) Then
            Return """" & value.Replace("""", """""") & """"
        End If
        Return value
    End Function

    Private Sub btnGenerateReport_Click(sender As Object, e As EventArgs) Handles btnGenerateReport.Click
        Try
            _logger.LogInfo("========== Generate Report Button Clicked ==========")
            
            If dgvFaculty.Rows.Count = 0 Then
                _logger.LogWarning("Report generation cancelled: No data to generate report")
                MessageBox.Show("No data available to generate report.", "Generate Report", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            _logger.LogInfo($"Preparing to generate report with {dgvFaculty.Rows.Count} records")

            ' Create a DataTable from the current DataGridView data
            Dim dt As DataTable = CType(dgvFaculty.DataSource, DataTable)
            
            If dt Is Nothing Then
                _logger.LogError("DataSource is null")
                MessageBox.Show("Unable to generate report. Data source is not available.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            _logger.LogInfo($"DataTable has {dt.Rows.Count} rows and {dt.Columns.Count} columns")

            ' Create report viewer form
            Dim reportForm As New Form()
            reportForm.Text = "Faculty List Report"
            reportForm.Size = New Size(1024, 768)
            reportForm.StartPosition = FormStartPosition.CenterScreen
            reportForm.WindowState = FormWindowState.Maximized
            reportForm.Icon = Me.Icon

            ' Create ReportViewer control
            Dim reportViewer As New Microsoft.Reporting.WinForms.ReportViewer()
            reportViewer.Dock = DockStyle.Fill
            reportViewer.ProcessingMode = Microsoft.Reporting.WinForms.ProcessingMode.Local

            ' Load the RDLC file
            Dim rdlcPath As String = System.IO.Path.Combine(Application.StartupPath, "ReportFacultyList.rdlc")
            _logger.LogInfo($"Loading RDLC file from: {rdlcPath}")

            If Not System.IO.File.Exists(rdlcPath) Then
                _logger.LogError($"RDLC file not found at: {rdlcPath}")
                MessageBox.Show($"Report template not found at: {rdlcPath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            reportViewer.LocalReport.ReportPath = rdlcPath
            _logger.LogInfo("RDLC file loaded successfully")

            ' Create ReportDataSource
            Dim rds As New Microsoft.Reporting.WinForms.ReportDataSource("DataSetFacultyList", dt)
            reportViewer.LocalReport.DataSources.Clear()
            reportViewer.LocalReport.DataSources.Add(rds)
            _logger.LogInfo("Report data source added")

            ' Refresh the report
            _logger.LogInfo("Refreshing report viewer...")
            reportViewer.RefreshReport()
            _logger.LogInfo("Report refreshed successfully")

            ' Add the ReportViewer to the form
            reportForm.Controls.Add(reportViewer)

            ' Show the report form
            _logger.LogInfo("Displaying report form...")
            reportForm.ShowDialog()
            _logger.LogInfo("✓ Report generated and displayed successfully")
            _logger.LogInfo("========== Generate Report Complete ==========")

        Catch ex As Exception
            _logger.LogError($"❌ Error generating report: {ex.Message}")
            _logger.LogError($"Stack trace: {ex.StackTrace}")
            MessageBox.Show($"Error generating report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class