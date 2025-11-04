Imports System.Data.Odbc

Public Class FormUserActivityLogs
    Private ReadOnly _logger As ILogger = LoggerFactory.Instance
    Private searchTimer As Timer
    
    ' Pagination variables
    Private currentPage As Integer = 1
    Private pageSize As Integer = 50
    Private totalRecords As Integer = 0
    Private totalPages As Integer = 0

    Private Sub FormUserActivityLogs_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        InitializeForm()
        LoadActivityLogs()
        Try
            dgvLogs.ClearSelection()
            dgvLogs.CurrentCell = Nothing
        Catch
        End Try
    End Sub

    Private Sub InitializeForm()
        dtpFrom.Value = DateTime.Now.AddDays(-7)
        dtpTo.Value = DateTime.Now

        dgvLogs.AutoGenerateColumns = False
        dgvLogs.AllowUserToAddRows = False
        dgvLogs.ReadOnly = True
        dgvLogs.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvLogs.RowTemplate.Height = 40

        With dgvLogs.ColumnHeadersDefaultCellStyle
            .Font = New Font("Segoe UI Semibold", 12)
            .Alignment = DataGridViewContentAlignment.MiddleLeft
        End With

        dgvLogs.DefaultCellStyle.Font = New Font("Segoe UI", 11)

        cboActionFilter.Items.Clear()
        cboActionFilter.Items.AddRange({"All Actions", "LOGIN", "LOGOUT", "CREATE", "UPDATE", "DELETE", "PASSWORD_CHANGE"})
        cboActionFilter.SelectedIndex = 0
    End Sub

    Private Sub LoadActivityLogs()
        Try
            _logger.LogInfo($"Loading user activity logs - Page {currentPage}")

            Dim countQuery As String = "
                SELECT COUNT(*) as total
                FROM user_activity_logs
                WHERE timestamp BETWEEN ? AND ?
            "

            Dim parameters As New List(Of Object)
            parameters.Add(dtpFrom.Value.ToString("yyyy-MM-dd 00:00:00"))
            parameters.Add(dtpTo.Value.ToString("yyyy-MM-dd 23:59:59"))

            If cboActionFilter.SelectedIndex > 0 Then
                countQuery &= " AND action_type = ?"
                parameters.Add(cboActionFilter.SelectedItem.ToString())
            End If

            If Not String.IsNullOrWhiteSpace(txtSearch.Text) Then
                countQuery &= " AND (username LIKE ? OR description LIKE ?)"
                Dim searchTerm As String = "%" & txtSearch.Text.Trim() & "%"
                parameters.Add(searchTerm)
                parameters.Add(searchTerm)
            End If

            Dim dbContext As New DatabaseContext()
            Dim countDt = dbContext.ExecuteQuery(countQuery, parameters.ToArray())
            totalRecords = If(countDt.Rows.Count > 0, Convert.ToInt32(countDt.Rows(0)("total")), 0)
            totalPages = Math.Ceiling(totalRecords / pageSize)

            Dim query As String = "
                SELECT 
                    log_id,
                    username,
                    action_type,
                    module,
                    description,
                    DATE_FORMAT(timestamp, '%Y-%m-%d %H:%i:%s') AS log_timestamp
                FROM user_activity_logs
                WHERE timestamp BETWEEN ? AND ?
            "

            parameters.Clear()
            parameters.Add(dtpFrom.Value.ToString("yyyy-MM-dd 00:00:00"))
            parameters.Add(dtpTo.Value.ToString("yyyy-MM-dd 23:59:59"))

            If cboActionFilter.SelectedIndex > 0 Then
                query &= " AND action_type = ?"
                parameters.Add(cboActionFilter.SelectedItem.ToString())
            End If

            If Not String.IsNullOrWhiteSpace(txtSearch.Text) Then
                query &= " AND (username LIKE ? OR description LIKE ?)"
                Dim searchTerm As String = "%" & txtSearch.Text.Trim() & "%"
                parameters.Add(searchTerm)
                parameters.Add(searchTerm)
            End If

            ' Add pagination
            Dim offset As Integer = (currentPage - 1) * pageSize
            query &= " ORDER BY timestamp DESC LIMIT ? OFFSET ?"
            parameters.Add(pageSize)
            parameters.Add(offset)

            _logger.LogDebug($"Query: {query}")

            Dim dt = dbContext.ExecuteQuery(query, parameters.ToArray())
            
            dgvLogs.DataSource = dt
            UpdatePaginationInfo()
            
            _logger.LogInfo($"Loaded {dt.Rows.Count} activity log records (Page {currentPage} of {totalPages})")

        Catch ex As Exception
            _logger.LogError($"Error loading activity logs: {ex.Message}")
            MessageBox.Show("Error loading activity logs: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    
    Private Sub UpdatePaginationInfo()
        Dim startRecord As Integer = If(totalRecords > 0, (currentPage - 1) * pageSize + 1, 0)
        Dim endRecord As Integer = Math.Min(currentPage * pageSize, totalRecords)
        
        lblRecordCount.Text = $"Showing {startRecord}-{endRecord} of {totalRecords} records | Page {currentPage} of {totalPages}"

        btnFirstPage.Enabled = currentPage > 1
        btnPrevPage.Enabled = currentPage > 1
        btnNextPage.Enabled = currentPage < totalPages
        btnLastPage.Enabled = currentPage < totalPages
        
        lblPageInfo.Text = $"Page {currentPage} / {totalPages}"
    End Sub
    
    Private Sub btnFirstPage_Click(sender As Object, e As EventArgs) Handles btnFirstPage.Click
        currentPage = 1
        LoadActivityLogs()
    End Sub
    
    Private Sub btnPrevPage_Click(sender As Object, e As EventArgs) Handles btnPrevPage.Click
        If currentPage > 1 Then
            currentPage -= 1
            LoadActivityLogs()
        End If
    End Sub
    
    Private Sub btnNextPage_Click(sender As Object, e As EventArgs) Handles btnNextPage.Click
        If currentPage < totalPages Then
            currentPage += 1
            LoadActivityLogs()
        End If
    End Sub
    
    Private Sub btnLastPage_Click(sender As Object, e As EventArgs) Handles btnLastPage.Click
        currentPage = totalPages
        LoadActivityLogs()
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        LoadActivityLogs()
    End Sub

    Private Sub btnExport_Click(sender As Object, e As EventArgs) Handles btnExport.Click
        Try
            If dgvLogs.Rows.Count = 0 Then
                Dim msgForm As New Form() With {.TopMost = True}
                MessageBox.Show(msgForm, "No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim saveDialog As New SaveFileDialog()
            saveDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv"
            saveDialog.FilterIndex = 1
            saveDialog.FileName = $"UserActivityLogs_{dtpFrom.Value:yyyyMMdd}_to_{dtpTo.Value:yyyyMMdd}"

            ' Make dialog topmost
            Dim helper As New NativeWindow()
            helper.AssignHandle(Me.Handle)

            If saveDialog.ShowDialog(helper) = DialogResult.OK Then
                If saveDialog.FilterIndex = 1 Then
                    ExportToExcel(saveDialog.FileName)
                Else
                    ExportToCSV(saveDialog.FileName)
                End If
                
                Dim msgForm As New Form() With {.TopMost = True}
                MessageBox.Show(msgForm, "Logs exported successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
                _logger.LogInfo($"Activity logs exported to: {saveDialog.FileName}")
            End If

        Catch ex As Exception
            _logger.LogError($"Error exporting logs: {ex.Message}")
            Dim msgForm As New Form() With {.TopMost = True}
            MessageBox.Show(msgForm, "Error exporting logs: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
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
                For Each column As DataGridViewColumn In dgvLogs.Columns
                    If column.Visible Then
                        worksheet.Cells(1, col).Value = column.HeaderText
                        worksheet.Cells(1, col).Font.Bold = True
                        worksheet.Cells(1, col).Interior.Color = RGB(52, 73, 94)
                        worksheet.Cells(1, col).Font.Color = RGB(255, 255, 255)
                        col += 1
                    End If
                Next
                
                Dim row As Integer = 2
                For Each dgvRow As DataGridViewRow In dgvLogs.Rows
                    If dgvRow.IsNewRow Then Continue For
                    
                    col = 1
                    For Each column As DataGridViewColumn In dgvLogs.Columns
                        If column.Visible Then
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
            Dim headers As New List(Of String)
            For Each column As DataGridViewColumn In dgvLogs.Columns
                If column.Visible Then
                    headers.Add(EscapeCsvValue(column.HeaderText))
                End If
            Next
            writer.WriteLine(String.Join(",", headers))

            For Each row As DataGridViewRow In dgvLogs.Rows
                If row.IsNewRow Then Continue For
                
                Dim values As New List(Of String)
                For Each column As DataGridViewColumn In dgvLogs.Columns
                    If column.Visible Then
                        Dim cellValue = row.Cells(column.Index).Value
                        Dim value As String = If(cellValue Is Nothing OrElse IsDBNull(cellValue), "", cellValue.ToString().Trim())
                        values.Add(EscapeCsvValue(value))
                    End If
                Next
                writer.WriteLine(String.Join(",", values))
            Next
        End Using
        
        _logger.LogInfo($"Exported {dgvLogs.Rows.Count} records to CSV: {filePath}")
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

    Private Sub cboActionFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboActionFilter.SelectedIndexChanged
        currentPage = 1
        LoadActivityLogs()
    End Sub

    Private Sub cboModuleFilter_SelectedIndexChanged(sender As Object, e As EventArgs)
        currentPage = 1
        LoadActivityLogs()
    End Sub

    Private Sub txtSearch_TextChanged(sender As Object, e As EventArgs) Handles txtSearch.TextChanged
        If searchTimer IsNot Nothing Then
            searchTimer.Stop()
        End If
        searchTimer = New Timer()
        searchTimer.Interval = 500
        AddHandler searchTimer.Tick, Sub()
                                          searchTimer.Stop()
                                          currentPage = 1
                                          LoadActivityLogs()
                                      End Sub
        searchTimer.Start()
    End Sub

    Private Sub dtpFrom_ValueChanged(sender As Object, e As EventArgs) Handles dtpFrom.ValueChanged
        If dtpFrom.Value > dtpTo.Value Then
            dtpTo.Value = dtpFrom.Value
        End If
        currentPage = 1
        LoadActivityLogs()
    End Sub

    Private Sub dtpTo_ValueChanged(sender As Object, e As EventArgs) Handles dtpTo.ValueChanged
        If dtpTo.Value < dtpFrom.Value Then
            dtpFrom.Value = dtpTo.Value
        End If
        currentPage = 1
        LoadActivityLogs()
    End Sub
    
    Private Sub btnGenerateReport_Click(sender As Object, e As EventArgs) Handles btnGenerateReport.Click
        Try
            _logger.LogInfo("Generating User Activity Logs report")
            
            Dim reportForm As New Form()
            reportForm.Text = "User Activity Logs Report"
            reportForm.Size = New Size(1024, 768)
            reportForm.StartPosition = FormStartPosition.CenterScreen
            reportForm.WindowState = FormWindowState.Maximized
            reportForm.TopMost = True

            Dim reportViewer As New Microsoft.Reporting.WinForms.ReportViewer()
            reportViewer.Dock = DockStyle.Fill
            reportForm.Controls.Add(reportViewer)
            
            Dim query As String = "
                SELECT 
                    username,
                    action_type,
                    description,
                    DATE_FORMAT(timestamp, '%Y-%m-%d %H:%i:%s') AS log_timestamp
                FROM user_activity_logs
                WHERE timestamp BETWEEN ? AND ?
            "
            
            Dim parameters As New List(Of Object)
            parameters.Add(dtpFrom.Value.ToString("yyyy-MM-dd 00:00:00"))
            parameters.Add(dtpTo.Value.ToString("yyyy-MM-dd 23:59:59"))
            
            If cboActionFilter.SelectedIndex > 0 Then
                query &= " AND action_type = ?"
                parameters.Add(cboActionFilter.SelectedItem.ToString())
            End If
            
            If Not String.IsNullOrWhiteSpace(txtSearch.Text) Then
                query &= " AND (username LIKE ? OR description LIKE ?)"
                Dim searchTerm As String = "%" & txtSearch.Text.Trim() & "%"
                parameters.Add(searchTerm)
                parameters.Add(searchTerm)
            End If
            
            query &= " ORDER BY timestamp DESC"
            
            Dim dbContext As New DatabaseContext()
            Dim dt = dbContext.ExecuteQuery(query, parameters.ToArray())
            
            reportViewer.LocalReport.ReportPath = "ReportUserActivityLogs.rdlc"
            reportViewer.LocalReport.DataSources.Clear()
            
            Dim rds As New Microsoft.Reporting.WinForms.ReportDataSource("DataSetUserActivityLogs", dt)
            reportViewer.LocalReport.DataSources.Add(rds)
            
            Dim reportParams As New List(Of Microsoft.Reporting.WinForms.ReportParameter)
            reportParams.Add(New Microsoft.Reporting.WinForms.ReportParameter("DateFrom", dtpFrom.Value.ToString("MMMM dd, yyyy")))
            reportParams.Add(New Microsoft.Reporting.WinForms.ReportParameter("DateTo", dtpTo.Value.ToString("MMMM dd, yyyy")))
            reportParams.Add(New Microsoft.Reporting.WinForms.ReportParameter("TotalRecords", dt.Rows.Count.ToString()))
            reportViewer.LocalReport.SetParameters(reportParams)
            
            reportViewer.RefreshReport()
            reportForm.ShowDialog()
            
            _logger.LogInfo($"Generated report with {dt.Rows.Count} records")
            
        Catch ex As Exception
            _logger.LogError($"Error generating report: {ex.Message}")
            Dim msgForm As New Form() With {.TopMost = True}
            MessageBox.Show(msgForm, "Error generating report: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class