Imports System.Data.Odbc
Imports System.Resources
Module myModule
    Public con As Odbc.OdbcConnection
    Public btnItemsClicked As Integer = 0
    Public sectionListGradeID As Integer = 0
    Public Sub connectDB()
        Try
            con = New Odbc.OdbcConnection("DSN=tala_ams")
            If con.State = ConnectionState.Closed Then
                con.Open()
            End If
        Catch ex As Exception

        End Try
    End Sub

    Public Function IsUsernameUnique(username As String) As Boolean
        Try
            connectDB()
            Dim query As String = "SELECT COUNT(*) FROM logins WHERE username = ? AND isActive=1"
            Dim command As New OdbcCommand(query, con)
            command.Parameters.AddWithValue("?", username)

            Dim result As Integer = Convert.ToInt32(command.ExecuteScalar())
            If result > 0 Then
                Return False 
            Else
                Return True
            End If

        Catch ex As Exception
            MessageBox.Show("Error checking for duplicate username: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        Finally
            con.Close()
            GC.Collect()
        End Try
    End Function
    Public Sub loadDGV(ByVal sql As String, ByVal dgv As DataGridView, Optional ByVal fieldname As String = "", Optional ByVal fieldnametwo As String = "", Optional ByVal fieldnamethree As String = "", Optional ByVal value As String = "")

        Dim cmd As Odbc.OdbcCommand
        Dim da As New Odbc.OdbcDataAdapter
        Dim dt As New DataTable

        Try
            ' Suspend layout updates for better performance
            dgv.SuspendLayout()
            dgv.DataSource = Nothing
            
            Call connectDB()
            If fieldname.Length <> 0 Then
                sql = sql + " WHERE (" + fieldname + " LIKE ? OR " + fieldnametwo + " LIKE ? OR " + fieldnamethree + " LIKE ?)"
                value = "%" + value + "%"
            End If
            cmd = New Odbc.OdbcCommand(sql, con)
            For i As Integer = 1 To 3
                cmd.Parameters.AddWithValue("@", value)
            Next
            da.SelectCommand = cmd
            da.Fill(dt)
            
            ' Set data source
            dgv.DataSource = dt
            
            ' Optimize column sizing - use Fill mode for maximum performance
            ' AllCells and DisplayedCells modes measure cells which is VERY slow for large datasets
            ' Fill mode distributes available space without measuring cells
            If dgv.Columns.Count > 0 Then
                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
                For Each col As DataGridViewColumn In dgv.Columns
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                Next
            End If
            
            ' Resume layout updates
            dgv.ResumeLayout(False)
        Catch ex As Exception
            MsgBox(ex.Message.ToString)
        Finally
            con.Close()
            ' Remove GC.Collect() - let .NET handle garbage collection automatically
            ' GC.Collect() causes unnecessary pauses
        End Try
    End Sub

    Public Sub loadCBO(ByVal query As String, ByVal id As String, ByVal display As String, ByVal cbo As ComboBox)
        Dim cmd As OdbcCommand
        Dim da As New OdbcDataAdapter()
        Dim dt As New DataTable()

        Try
            connectDB()
            cmd = New OdbcCommand(query, con)
            da.SelectCommand = cmd
            da.Fill(dt)
            cbo.DataSource = dt
            cbo.ValueMember = id
            cbo.DisplayMember = display
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        Finally
            con.Close()
        End Try
    End Sub

    Function fieldChecker(ByVal pnl As Panel, Optional logErrors As Boolean = True, Optional excludeFields As String() = Nothing) As Boolean
        Dim logger As ILogger = LoggerFactory.Instance
        Dim defaultExcluded As String() = {"txtMiddleName", "txtExtName"}
        Dim allExcluded As New List(Of String)(defaultExcluded)

        If excludeFields IsNot Nothing Then
            allExcluded.AddRange(excludeFields)
        End If

        For Each obj As Control In pnl.Controls
            If TypeOf obj Is TextBox OrElse TypeOf obj Is ComboBox Then
                If allExcluded.Contains(obj.Name) Then
                    Continue For
                End If

                If String.IsNullOrEmpty(obj.Text.Trim()) Then
                    fieldChecker = False

                    If logErrors Then
                        logger.LogWarning($"Field validation failed - Empty field: '{obj.Name}' in panel '{pnl.Name}'")
                    End If

                    MsgBox("Please fill up every field in the form.", vbCritical, "Warning")
                    obj.Focus()
                    Exit Function
                End If
            End If
        Next
        Return True
    End Function

    Public Sub ClearFields(ByVal container As Control)
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

    Public Sub ClickEnter(ByVal e As KeyEventArgs, ByVal btn As Button)
        If e.KeyCode = Keys.Enter Then
            btn.PerformClick()
        Else
            Exit Sub
        End If
        e.SuppressKeyPress = True
    End Sub

    Public Function FormatFullName(firstName As String, Optional middleName As String = "", Optional lastName As String = "", Optional extName As String = "") As String
        Dim nameParts As New List(Of String)

        If Not String.IsNullOrWhiteSpace(firstName) Then
            nameParts.Add(firstName.Trim())
        End If

        If Not String.IsNullOrWhiteSpace(middleName) AndAlso 
           middleName.Trim() <> "--" AndAlso 
           middleName.Trim().ToUpper() <> "N/A" Then
            nameParts.Add(middleName.Trim())
        End If

        If Not String.IsNullOrWhiteSpace(lastName) Then
            nameParts.Add(lastName.Trim())
        End If

        If Not String.IsNullOrWhiteSpace(extName) AndAlso 
           extName.Trim() <> "--" AndAlso 
           extName.Trim().ToUpper() <> "N/A" Then
            nameParts.Add(extName.Trim())
        End If
        
        Return String.Join(" ", nameParts)
    End Function

    Public Function GetNameConcatSQL() As String
        Return "CONCAT(firstname, " &
               "IF(middlename IS NULL OR middlename = '' OR middlename = '--' OR UPPER(middlename) = 'N/A', '', CONCAT(' ', middlename)), " &
               "' ', lastname, " &
               "IF(extName IS NULL OR extName = '' OR extName = '--' OR UPPER(extName) = 'N/A', '', CONCAT(' ', extName)))"
    End Function

    ''' <summary>
    ''' Calculates Under time or Over time remarks based on departure time and shift end time
    ''' </summary>
    ''' <param name="departureTime">The actual departure time</param>
    ''' <param name="shiftEndTime">The teacher's shift end time (TimeSpan or Nothing)</param>
    ''' <param name="existingRemarks">Existing remarks that should be preserved (Manual Input, etc.)</param>
    ''' <returns>Remarks string with Under time or Over time added, preserving existing remarks (except old Under/Over time)</returns>
    Public Function CalculateTimeRemarks(departureTime As DateTime, shiftEndTime As Object, Optional existingRemarks As String = "") As String
        Try
            Dim remarksList As New List(Of String)
            
            ' Process existing remarks - exclude prior Manual Input/Edit and old Under/Over time
            If Not String.IsNullOrWhiteSpace(existingRemarks) Then
                Dim remarksParts As String() = existingRemarks.Split({";"}, StringSplitOptions.RemoveEmptyEntries)
                For Each part In remarksParts
                    Dim trimmedPart As String = part.Trim()
                    ' Skip action parts (Manual Input / Edit) and old Under/Over time; keep only other custom remarks
                    If Not trimmedPart.StartsWith("Manual Input:", StringComparison.OrdinalIgnoreCase) AndAlso
                       Not trimmedPart.StartsWith("Edit:", StringComparison.OrdinalIgnoreCase) AndAlso
                       Not trimmedPart.StartsWith("Under time", StringComparison.OrdinalIgnoreCase) AndAlso
                            Not trimmedPart.StartsWith("Over time", StringComparison.OrdinalIgnoreCase) Then
                        remarksList.Add(trimmedPart)
                    End If
                Next
            End If
            
            ' Check if shift end time is available
            If shiftEndTime IsNot Nothing AndAlso Not IsDBNull(shiftEndTime) Then
                Dim shiftEnd As TimeSpan
                
                ' Handle different types of shiftEndTime
                If TypeOf shiftEndTime Is TimeSpan Then
                    shiftEnd = DirectCast(shiftEndTime, TimeSpan)
                ElseIf TypeOf shiftEndTime Is DateTime Then
                    shiftEnd = DirectCast(shiftEndTime, DateTime).TimeOfDay
                Else
                    ' Try to parse as string or convert
                    Dim shiftEndStr As String = shiftEndTime.ToString()
                    If TimeSpan.TryParse(shiftEndStr, shiftEnd) Then
                        ' Successfully parsed
                    Else
                        ' Cannot parse, skip time calculation but return preserved remarks
                        Return If(remarksList.Count > 0, String.Join("; ", remarksList), "")
                    End If
                End If
                
                ' Get the time portion of departure time
                Dim departureTimeOfDay As TimeSpan = departureTime.TimeOfDay
                
                ' Calculate difference in minutes
                Dim timeDifferenceMinutes As Integer = CInt((departureTimeOfDay.TotalMinutes - shiftEnd.TotalMinutes))
                
                ' Under time: left before shift end (more than 15 minutes early)
                ' Over time: left after shift end (more than 15 minutes late)
                If timeDifferenceMinutes < -15 Then
                    Dim minutesUnder As Integer = Math.Abs(timeDifferenceMinutes)
                    Dim hoursUnder As Integer = minutesUnder \ 60
                    Dim minsUnder As Integer = minutesUnder Mod 60
                    
                    If hoursUnder > 0 Then
                        remarksList.Add($"Under time: {hoursUnder}h {minsUnder}m")
                    Else
                        remarksList.Add($"Under time: {minsUnder}m")
                    End If
                ElseIf timeDifferenceMinutes > 15 Then
                    Dim minutesOver As Integer = timeDifferenceMinutes
                    Dim hoursOver As Integer = minutesOver \ 60
                    Dim minsOver As Integer = minutesOver Mod 60
                    
                    If hoursOver > 0 Then
                        remarksList.Add($"Over time: {hoursOver}h {minsOver}m")
                    Else
                        remarksList.Add($"Over time: {minsOver}m")
                    End If
                End If
            End If
            
            Return If(remarksList.Count > 0, String.Join("; ", remarksList), "")
        Catch ex As Exception
            ' Log error but don't break the flow - return existing remarks if available
            Return If(String.IsNullOrWhiteSpace(existingRemarks), "", existingRemarks)
        End Try
    End Function

End Module
