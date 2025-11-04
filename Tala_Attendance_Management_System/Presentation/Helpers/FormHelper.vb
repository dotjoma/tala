Public Class FormHelper
    Public Shared Sub ClearFields(container As Control)
        For Each obj As Control In container.Controls
            If TypeOf obj Is TextBox Then
                DirectCast(obj, TextBox).Text = ""
            ElseIf TypeOf obj Is ComboBox Then
                DirectCast(obj, ComboBox).SelectedIndex = -1
            ElseIf TypeOf obj Is PictureBox Then
                DirectCast(obj, PictureBox).Image = My.Resources.default_image
            End If
        Next
    End Sub

    Public Shared Sub HandleEnterKeyPress(e As KeyEventArgs, button As Button)
        If e.KeyCode = Keys.Enter Then
            button.PerformClick()
            e.SuppressKeyPress = True
        End If
    End Sub

    Public Shared Sub LoadDataGridView(sql As String, dgv As DataGridView, Optional searchFields As String() = Nothing, Optional searchValue As String = "")
        Try
            Dim dbContext As New DatabaseContext()
            Dim finalSql As String = sql
            Dim parameters As New List(Of Object)

            If searchFields IsNot Nothing AndAlso searchFields.Length > 0 AndAlso Not String.IsNullOrEmpty(searchValue) Then
                Dim searchConditions As New List(Of String)
                For Each field In searchFields
                    searchConditions.Add($"{field} LIKE ?")
                    parameters.Add($"%{searchValue}%")
                Next
                finalSql += $" AND ({String.Join(" OR ", searchConditions)})"
            End If

            Dim dt As DataTable = dbContext.ExecuteQuery(finalSql, parameters.ToArray())
            dgv.DataSource = dt
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
        Catch ex As Exception
            MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Public Shared Sub LoadComboBox(query As String, valueField As String, displayField As String, comboBox As ComboBox)
        Try
            Dim dbContext As New DatabaseContext()
            Dim dt As DataTable = dbContext.ExecuteQuery(query)

            comboBox.DataSource = dt
            comboBox.ValueMember = valueField
            comboBox.DisplayMember = displayField
        Catch ex As Exception
            MessageBox.Show($"Error loading combo box data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class
