Imports System
Imports System.Data
Imports System.Data.Odbc
Imports System.IO
Imports System.IO.Compression
Imports System.Text
Imports System.Windows.Forms

Public Class BackupService
    Private ReadOnly _logger As ILogger

    Public Sub New(logger As ILogger)
        _logger = logger
    End Sub

    Public Sub CreateBackup(owner As IWin32Window)
        Try
            Dim saveDialog As New SaveFileDialog()
            saveDialog.Title = "Save System Backup"
            saveDialog.Filter = "Backup Files (*.zip)|*.zip"
            saveDialog.FileName = $"TalaAMS_Backup_{Date.Now:yyyyMMdd_HHmmss}.zip"

            If saveDialog.ShowDialog(owner) <> DialogResult.OK Then
                Return
            End If

            DoCreateBackup(saveDialog.FileName, owner)
        Catch ex As Exception
            _logger.LogError($"Backup failed: {ex.Message}")
            MessageBox.Show(owner, "Backup failed: " & ex.Message, "Backup Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Public Sub CreateBackup(owner As IWin32Window, targetZipPath As String)
        Try
            If String.IsNullOrWhiteSpace(targetZipPath) Then
                CreateBackup(owner)
                Return
            End If
            DoCreateBackup(targetZipPath, owner)
        Catch ex As Exception
            _logger.LogError($"Backup failed: {ex.Message}")
            MessageBox.Show(owner, "Backup failed: " & ex.Message, "Backup Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Public Sub RestoreBackup(owner As IWin32Window)
        Try
            Dim openDialog As New OpenFileDialog()
            openDialog.Title = "Select Backup File"
            openDialog.Filter = "Backup Files (*.zip)|*.zip"
            If openDialog.ShowDialog(owner) <> DialogResult.OK Then
                Return
            End If

            DoRestoreBackup(openDialog.FileName, owner)
        Catch ex As Exception
            _logger.LogError($"Restore failed: {ex.Message}")
            MessageBox.Show(owner, "Restore failed: " & ex.Message, "Restore Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Public Sub RestoreBackup(owner As IWin32Window, sourceZipPath As String)
        Try
            If String.IsNullOrWhiteSpace(sourceZipPath) Then
                RestoreBackup(owner)
                Return
            End If
            DoRestoreBackup(sourceZipPath, owner)
        Catch ex As Exception
            _logger.LogError($"Restore failed: {ex.Message}")
            MessageBox.Show(owner, "Restore failed: " & ex.Message, "Restore Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Public Sub RestoreBackup(owner As IWin32Window, sourceZipPath As String, progress As Action(Of Integer, String))
        Try
            If String.IsNullOrWhiteSpace(sourceZipPath) Then
                RestoreBackup(owner)
                Return
            End If
            DoRestoreBackup(sourceZipPath, owner, progress)
        Catch ex As Exception
            _logger.LogError($"Restore failed: {ex.Message}")
            MessageBox.Show(owner, "Restore failed: " & ex.Message, "Restore Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub DoCreateBackup(targetZipPath As String, owner As IWin32Window)
        Dim tempRoot As String = Path.Combine(Path.GetTempPath(), "TalaAMS_Backup_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(tempRoot)

        Dim dataDir As String = Path.Combine(tempRoot, "data")
        Directory.CreateDirectory(dataDir)

        ExportAllTablesToCsv(dataDir)
        CopyConfiguration(tempRoot)
        WriteManifest(tempRoot)
        If File.Exists(targetZipPath) Then
            File.Delete(targetZipPath)
        End If
        ZipFile.CreateFromDirectory(tempRoot, targetZipPath, CompressionLevel.Optimal, False)

        MessageBox.Show(owner, "Backup created successfully.", "Backup Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub DoRestoreBackup(sourceZipPath As String, owner As IWin32Window, Optional progress As Action(Of Integer, String) = Nothing)
        If MessageBox.Show(owner, "This will overwrite existing data. Continue?", "Confirm Restore", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then
            Return
        End If

        Dim extractRoot As String = Path.Combine(Path.GetTempPath(), "TalaAMS_Restore_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(extractRoot)
        If progress IsNot Nothing Then progress(5, "Extracting backup...")
        ZipFile.ExtractToDirectory(sourceZipPath, extractRoot)

        Dim manifestPath As String = Path.Combine(extractRoot, "manifest.txt")
        If Not File.Exists(manifestPath) Then
            Throw New InvalidOperationException("Invalid backup: manifest not found.")
        End If

        Dim dataDir As String = Path.Combine(extractRoot, "data")
        If Directory.Exists(dataDir) Then
            Using localCon As New Odbc.OdbcConnection("DSN=tala_ams")
                localCon.Open()
                ImportAllTablesFromCsvWithConn(localCon, dataDir, Sub(pct As Integer, msg As String)
                                                                      If progress IsNot Nothing Then
                                                                          progress(pct, msg)
                                                                      End If
                                                                  End Sub)
            End Using
        End If

        Dim configDir As String = Path.Combine(extractRoot, "Config")
        If Directory.Exists(configDir) Then
            RestoreConfiguration(configDir)
        End If

        If progress IsNot Nothing Then progress(100, "Restore complete")
        MessageBox.Show(owner, "Restore completed successfully.", "Restore Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub ExportAllTablesToCsv(outputDir As String)
        connectDB()
        Try
            Dim schema As DataTable = con.GetSchema("Tables")
            For Each row As DataRow In schema.Rows
                Dim tableName As String = row("TABLE_NAME").ToString()

                If String.IsNullOrWhiteSpace(tableName) Then
                    Continue For
                End If

                Dim safeName As String = tableName.Replace("`", "").Replace(" ", "_")
                Dim filePath As String = Path.Combine(outputDir, safeName & ".csv")

                Using cmd As New OdbcCommand($"SELECT * FROM `{tableName}`", con)
                    Using adapter As New OdbcDataAdapter(cmd)
                        Dim dt As New DataTable()
                        adapter.Fill(dt)
                        WriteDataTableToCsv(dt, filePath)
                    End Using
                End Using
            Next
        Finally
            If con IsNot Nothing AndAlso con.State = ConnectionState.Open Then
                con.Close()
            End If
        End Try
    End Sub

    Private Sub ImportAllTablesFromCsv(inputDir As String, Optional progress As Action(Of Integer, String) = Nothing)
        connectDB()
        Dim transaction As OdbcTransaction = Nothing
        Try
            transaction = con.BeginTransaction()

            ExecuteNonQuerySafe("SET AUTOCOMMIT=0;", transaction)
            ExecuteNonQuerySafe("SET UNIQUE_CHECKS=0;", transaction)
            ExecuteNonQuerySafe("SET FOREIGN_KEY_CHECKS=0;", transaction)
            ExecuteNonQuerySafe("ALTER DATABASE CURRENT SET READ_COMMITTED_SNAPSHOT ON;", transaction)

            Dim allFiles As String() = Directory.GetFiles(inputDir, "*.csv")
            Dim total As Integer = Math.Max(allFiles.Length, 1)
            Dim processed As Integer = 0

            For Each csvPath As String In allFiles
                Dim tableName As String = Path.GetFileNameWithoutExtension(csvPath)

                Dim dt As DataTable = ReadCsvToDataTable(csvPath)
                If dt.Columns.Count = 0 Then
                    Continue For
                End If

                If Not ExecuteNonQuerySafe($"TRUNCATE TABLE `{tableName}`;", transaction) Then
                    ExecuteNonQuerySafe($"DELETE FROM `{tableName}`;", transaction)
                End If

                Dim columnNames As New List(Of String)
                For Each col As DataColumn In dt.Columns
                    columnNames.Add($"`{col.ColumnName}`")
                Next

                Dim placeholders As String = String.Join(",", Enumerable.Repeat("?", dt.Columns.Count))
                Dim insertSql As String = $"INSERT INTO `{tableName}` ({String.Join(",", columnNames)}) VALUES ({placeholders})"

                Dim columnsInfo As Dictionary(Of String, ColumnInfo) = GetTableColumnsInfo(tableName, transaction)

                Using insertCmd As New OdbcCommand(insertSql, con, transaction)
                    For i As Integer = 0 To dt.Columns.Count - 1
                        Dim p As New OdbcParameter()
                        Dim colName As String = dt.Columns(i).ColumnName
                        If columnsInfo IsNot Nothing AndAlso columnsInfo.ContainsKey(colName) Then
                            p.OdbcType = MapToOdbcType(columnsInfo(colName).DataType)
                        End If
                        insertCmd.Parameters.Add(p)
                    Next

                    For Each dr As DataRow In dt.Rows
                        For i As Integer = 0 To dt.Columns.Count - 1
                            Dim colName As String = dt.Columns(i).ColumnName
                            Dim rawVal As Object = If(dr.IsNull(i), DBNull.Value, dr(i))

                            If columnsInfo IsNot Nothing AndAlso columnsInfo.ContainsKey(colName) Then
                                Dim info = columnsInfo(colName)

                                If TypeOf rawVal Is String AndAlso String.IsNullOrWhiteSpace(CType(rawVal, String)) Then
                                    rawVal = DBNull.Value
                                End If

                                If rawVal Is DBNull.Value AndAlso Not info.IsNullable Then
                                    rawVal = If(info.HasDefault, info.DefaultValue, GetTypeDefaultValue(info.DataType))
                                End If

                                rawVal = ConvertValueForColumn(rawVal, info)
                            Else
                                If TypeOf rawVal Is String AndAlso String.IsNullOrWhiteSpace(CType(rawVal, String)) Then
                                    rawVal = DBNull.Value
                                End If
                            End If

                            insertCmd.Parameters(i).Value = rawVal
                        Next
                        insertCmd.ExecuteNonQuery()
                    Next
                End Using

                processed += 1
                Dim pct As Integer = CInt((processed / CDbl(total)) * 100)
                Try
                    If progress IsNot Nothing Then
                        progress(pct, $"Imported {tableName}")
                    Else
                        _logger.LogInfo($"Restore progress: {pct}% - {tableName}")
                    End If
                Catch
                End Try
            Next

            ExecuteNonQuerySafe("SET FOREIGN_KEY_CHECKS=1;", transaction)
            ExecuteNonQuerySafe("SET UNIQUE_CHECKS=1;", transaction)

            transaction.Commit()
        Catch
            If transaction IsNot Nothing Then
                Try
                    transaction.Rollback()
                Catch
                End Try
            End If
            Throw
        Finally
            If con IsNot Nothing AndAlso con.State = ConnectionState.Open Then
                con.Close()
            End If
        End Try
    End Sub

    Private Sub ImportAllTablesFromCsvWithConn(conn As OdbcConnection, inputDir As String, Optional progress As Action(Of Integer, String) = Nothing)
        Dim transaction As OdbcTransaction = Nothing
        Try
            transaction = conn.BeginTransaction()

            ExecuteNonQuerySafe(conn, "SET AUTOCOMMIT=0;", transaction)
            ExecuteNonQuerySafe(conn, "SET UNIQUE_CHECKS=0;", transaction)
            ExecuteNonQuerySafe(conn, "SET FOREIGN_KEY_CHECKS=0;", transaction)

            Dim allFiles As String() = Directory.GetFiles(inputDir, "*.csv")
            Dim total As Integer = Math.Max(allFiles.Length, 1)
            Dim processed As Integer = 0

            For Each csvPath As String In allFiles
                Dim tableName As String = Path.GetFileNameWithoutExtension(csvPath)

                Dim dt As DataTable = ReadCsvToDataTable(csvPath)
                If dt.Columns.Count = 0 Then Continue For

                If Not ExecuteNonQuerySafe(conn, $"TRUNCATE TABLE `{tableName}`;", transaction) Then
                    ExecuteNonQuerySafe(conn, $"DELETE FROM `{tableName}`;", transaction)
                End If

                Dim columnNames As New List(Of String)
                For Each col As DataColumn In dt.Columns
                    columnNames.Add($"`{col.ColumnName}`")
                Next

                Dim placeholders As String = String.Join(",", Enumerable.Repeat("?", dt.Columns.Count))
                Dim insertSql As String = $"INSERT INTO `{tableName}` ({String.Join(",", columnNames)}) VALUES ({placeholders})"

                Dim columnsInfo As Dictionary(Of String, ColumnInfo) = GetTableColumnsInfo(conn, tableName, transaction)

                Using insertCmd As New OdbcCommand(insertSql, conn, transaction)
                    For i As Integer = 0 To dt.Columns.Count - 1
                        Dim p As New OdbcParameter()
                        Dim colName As String = dt.Columns(i).ColumnName
                        If columnsInfo IsNot Nothing AndAlso columnsInfo.ContainsKey(colName) Then
                            p.OdbcType = MapToOdbcType(columnsInfo(colName).DataType)
                        End If
                        insertCmd.Parameters.Add(p)
                    Next

                    For Each dr As DataRow In dt.Rows
                        For i As Integer = 0 To dt.Columns.Count - 1
                            Dim colName As String = dt.Columns(i).ColumnName
                            Dim rawVal As Object = If(dr.IsNull(i), DBNull.Value, dr(i))

                            If columnsInfo IsNot Nothing AndAlso columnsInfo.ContainsKey(colName) Then
                                Dim info = columnsInfo(colName)
                                If TypeOf rawVal Is String AndAlso String.IsNullOrWhiteSpace(CType(rawVal, String)) Then
                                    rawVal = DBNull.Value
                                End If
                                If rawVal Is DBNull.Value AndAlso Not info.IsNullable Then
                                    rawVal = If(info.HasDefault, info.DefaultValue, GetTypeDefaultValue(info.DataType))
                                End If
                                rawVal = ConvertValueForColumn(rawVal, info)
                            Else
                                If TypeOf rawVal Is String AndAlso String.IsNullOrWhiteSpace(CType(rawVal, String)) Then
                                    rawVal = DBNull.Value
                                End If
                            End If
                            insertCmd.Parameters(i).Value = rawVal
                        Next
                        insertCmd.ExecuteNonQuery()
                    Next
                End Using

                processed += 1
                Dim pct As Integer = CInt((processed / CDbl(total)) * 100)
                If progress IsNot Nothing Then progress(pct, $"Imported {tableName}")
            Next

            ExecuteNonQuerySafe(conn, "SET FOREIGN_KEY_CHECKS=1;", transaction)
            ExecuteNonQuerySafe(conn, "SET UNIQUE_CHECKS=1;", transaction)
            transaction.Commit()
        Catch
            If transaction IsNot Nothing Then
                Try
                    transaction.Rollback()
                Catch
                End Try
            End If
            Throw
        End Try
    End Sub

    Private Class ColumnInfo
        Public Property Name As String
        Public Property IsNullable As Boolean
        Public Property HasDefault As Boolean
        Public Property DefaultValue As Object
        Public Property DataType As String
    End Class

    Private Function GetTableColumnsInfo(tableName As String, tx As OdbcTransaction) As Dictionary(Of String, ColumnInfo)
        Try
            Dim dict As New Dictionary(Of String, ColumnInfo)(StringComparer.OrdinalIgnoreCase)
            Dim sql As String = "SELECT COLUMN_NAME, IS_NULLABLE, DATA_TYPE, COLUMN_DEFAULT FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = ?"
            Using cmd As New OdbcCommand(sql, con, tx)
                cmd.Parameters.AddWithValue("?", tableName)
                Using r = cmd.ExecuteReader()
                    While r.Read()
                        Dim info As New ColumnInfo()
                        info.Name = r("COLUMN_NAME").ToString()
                        info.IsNullable = String.Equals(r("IS_NULLABLE").ToString(), "YES", StringComparison.OrdinalIgnoreCase)
                        info.DataType = r("DATA_TYPE").ToString()
                        If Not r.IsDBNull(r.GetOrdinal("COLUMN_DEFAULT")) Then
                            info.HasDefault = True
                            info.DefaultValue = r("COLUMN_DEFAULT")
                        Else
                            info.HasDefault = False
                            info.DefaultValue = Nothing
                        End If
                        dict(info.Name) = info
                    End While
                End Using
            End Using
            Return dict
        Catch
            Return New Dictionary(Of String, ColumnInfo)(StringComparer.OrdinalIgnoreCase)
        End Try
    End Function

    Private Function GetTableColumnsInfo(conn As OdbcConnection, tableName As String, tx As OdbcTransaction) As Dictionary(Of String, ColumnInfo)
        Try
            Dim dict As New Dictionary(Of String, ColumnInfo)(StringComparer.OrdinalIgnoreCase)
            Dim sql As String = "SELECT COLUMN_NAME, IS_NULLABLE, DATA_TYPE, COLUMN_DEFAULT FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = ?"
            Using cmd As New OdbcCommand(sql, conn, tx)
                cmd.Parameters.AddWithValue("?", tableName)
                Using r = cmd.ExecuteReader()
                    While r.Read()
                        Dim info As New ColumnInfo()
                        info.Name = r("COLUMN_NAME").ToString()
                        info.IsNullable = String.Equals(r("IS_NULLABLE").ToString(), "YES", StringComparison.OrdinalIgnoreCase)
                        info.DataType = r("DATA_TYPE").ToString()
                        If Not r.IsDBNull(r.GetOrdinal("COLUMN_DEFAULT")) Then
                            info.HasDefault = True
                            info.DefaultValue = r("COLUMN_DEFAULT")
                        Else
                            info.HasDefault = False
                            info.DefaultValue = Nothing
                        End If
                        dict(info.Name) = info
                    End While
                End Using
            End Using
            Return dict
        Catch
            Return New Dictionary(Of String, ColumnInfo)(StringComparer.OrdinalIgnoreCase)
        End Try
    End Function

    Private Function GetTypeDefaultValue(dataType As String) As Object
        Dim t As String = If(dataType, "").ToLowerInvariant()
        If t.Contains("int") OrElse t = "decimal" OrElse t = "numeric" OrElse t = "float" OrElse t = "double" OrElse t = "real" OrElse t = "bit" OrElse t = "tinyint" Then
            Return 0
        End If
        If t = "date" Then Return New DateTime(1970, 1, 1)
        If t = "datetime" OrElse t = "timestamp" OrElse t = "datetime2" Then Return New DateTime(1970, 1, 1, 0, 0, 0)
        If t = "time" Then Return "00:00:00"
        Return ""
    End Function

    Private Function MapToOdbcType(dataType As String) As OdbcType
        Dim t As String = If(dataType, "").ToLowerInvariant()
        Select Case t
            Case "varchar", "char", "tinytext", "text", "mediumtext", "longtext"
                Return OdbcType.VarChar
            Case "nvarchar", "nchar"
                Return OdbcType.NVarChar
            Case "int", "integer"
                Return OdbcType.Int
            Case "bigint"
                Return OdbcType.BigInt
            Case "smallint"
                Return OdbcType.SmallInt
            Case "tinyint"
                Return OdbcType.TinyInt
            Case "decimal", "numeric"
                Return OdbcType.Decimal
            Case "float", "double", "real"
                Return OdbcType.Double
            Case "bit", "bool", "boolean"
                Return OdbcType.Bit
            Case "date"
                Return OdbcType.Date
            Case "datetime", "timestamp", "datetime2"
                Return OdbcType.DateTime
            Case "time"
                Return OdbcType.Time
            Case "blob", "varbinary", "binary"
                Return OdbcType.VarBinary
            Case Else
                Return OdbcType.VarChar
        End Select
    End Function

    Private Function ConvertValueForColumn(value As Object, info As ColumnInfo) As Object
        If value Is DBNull.Value Then Return DBNull.Value

        Dim t As String = If(info.DataType, "").ToLowerInvariant()
        Try
            Select Case t
                Case "varchar", "char", "tinytext", "text", "mediumtext", "longtext", "nvarchar", "nchar"
                    Return Convert.ToString(value)
                Case "int", "integer"
                    Return Convert.ToInt32(value)
                Case "bigint"
                    Return Convert.ToInt64(value)
                Case "smallint"
                    Return Convert.ToInt16(value)
                Case "tinyint"
                    Dim s = Convert.ToString(value)
                    If String.Equals(s, "true", StringComparison.OrdinalIgnoreCase) Then Return 1
                    If String.Equals(s, "false", StringComparison.OrdinalIgnoreCase) Then Return 0
                    Return Convert.ToByte(value)
                Case "decimal", "numeric"
                    Return Convert.ToDecimal(value)
                Case "float", "double", "real"
                    Return Convert.ToDouble(value)
                Case "bit", "bool", "boolean"
                    Dim b As Boolean
                    If Boolean.TryParse(Convert.ToString(value), b) Then
                        Return If(b, 1, 0)
                    End If
                    Return If(Convert.ToInt32(value) <> 0, 1, 0)
                Case "date"
                    Dim dt As DateTime
                    If DateTime.TryParse(Convert.ToString(value), dt) Then
                        Return dt.Date
                    End If
                    Return New DateTime(1970, 1, 1)
                Case "datetime", "timestamp", "datetime2"
                    Dim dt2 As DateTime
                    If DateTime.TryParse(Convert.ToString(value), dt2) Then
                        Return dt2
                    End If
                    Return New DateTime(1970, 1, 1, 0, 0, 0)
                Case "time"
                    Dim ts As TimeSpan
                    If TypeOf value Is TimeSpan Then Return CType(value, TimeSpan)
                    If TimeSpan.TryParse(Convert.ToString(value), ts) Then
                        Return ts
                    End If
                    Dim s As String = Convert.ToString(value)
                    If Not String.IsNullOrWhiteSpace(s) Then
                        Dim parts As String() = s.Split(":"c)
                        If parts.Length >= 2 Then
                            Dim hh As Integer, mm As Integer, ss As Integer
                            If Integer.TryParse(parts(0), hh) AndAlso Integer.TryParse(parts(1), mm) Then
                                ss = 0
                                If parts.Length >= 3 Then
                                    Integer.TryParse(parts(2), ss)
                                End If
                                Return New TimeSpan(hh, mm, ss)
                            End If
                        End If
                    End If
                    Return TimeSpan.Zero
                Case "blob", "varbinary", "binary"
                    If TypeOf value Is Byte() Then Return value
                    Return DBNull.Value
                Case Else
                    Return Convert.ToString(value)
            End Select
        Catch
            If t.Contains("int") OrElse t = "decimal" OrElse t = "numeric" OrElse t = "float" OrElse t = "double" OrElse t = "real" OrElse t = "bit" OrElse t = "tinyint" Then
                Return 0
            End If
            Return Convert.ToString(value)
        End Try
    End Function

    Private Function ExecuteNonQuerySafe(sql As String, tx As OdbcTransaction) As Boolean
        Try
            Using cmd As New OdbcCommand(sql, con, tx)
                cmd.ExecuteNonQuery()
            End Using
            Return True
        Catch
            Return False
        End Try
    End Function

    Private Function ExecuteNonQuerySafe(conn As OdbcConnection, sql As String, tx As OdbcTransaction) As Boolean
        Try
            Using cmd As New OdbcCommand(sql, conn, tx)
                cmd.ExecuteNonQuery()
            End Using
            Return True
        Catch
            Return False
        End Try
    End Function

    Private Sub CopyConfiguration(rootDir As String)
        Try
            Dim appStart As String = Application.StartupPath
            Dim cfgSrc As String = Path.Combine(appStart, "Config")
            Dim cfgDest As String = Path.Combine(rootDir, "Config")
            If Directory.Exists(cfgSrc) Then
                Directory.CreateDirectory(cfgDest)
                For Each file In Directory.GetFiles(cfgSrc, "*.json", SearchOption.TopDirectoryOnly)
                    System.IO.File.Copy(file, Path.Combine(cfgDest, Path.GetFileName(file)), True)
                Next
            End If
        Catch
        End Try
    End Sub

    Private Sub RestoreConfiguration(cfgDir As String)
        Try
            Dim appStart As String = Application.StartupPath
            Dim cfgDest As String = Path.Combine(appStart, "Config")
            Directory.CreateDirectory(cfgDest)
            For Each file In Directory.GetFiles(cfgDir, "*.json", SearchOption.TopDirectoryOnly)
                System.IO.File.Copy(file, Path.Combine(cfgDest, Path.GetFileName(file)), True)
            Next
        Catch ex As Exception
            _logger.LogError($"Failed to restore config files: {ex.Message}")
        End Try
    End Sub

    Private Sub WriteManifest(rootDir As String)
        Dim manifestPath As String = Path.Combine(rootDir, "manifest.txt")
        Dim sb As New StringBuilder()
        sb.AppendLine($"App: Tala Attendance Management System")
        sb.AppendLine($"Version: {Constants.APP_VERSION}")
        sb.AppendLine($"DateUtc: {Date.UtcNow:O}")
        sb.AppendLine($"Database: ODBC DSN=tala_ams")
        File.WriteAllText(manifestPath, sb.ToString())
    End Sub

    Private Sub WriteDataTableToCsv(table As DataTable, filePath As String)
        Using writer As New StreamWriter(filePath, False, Encoding.UTF8)
            Dim headers As New List(Of String)
            For Each col As DataColumn In table.Columns
                headers.Add(EscapeCsv(col.ColumnName))
            Next
            writer.WriteLine(String.Join(",", headers))

            For Each row As DataRow In table.Rows
                Dim fields As New List(Of String)
                For Each col As DataColumn In table.Columns
                    fields.Add(EscapeCsv(If(row.IsNull(col), "", row(col).ToString())))
                Next
                writer.WriteLine(String.Join(",", fields))
            Next
        End Using
    End Sub

    Private Function ReadCsvToDataTable(csvPath As String) As DataTable
        Dim dt As New DataTable()
        Using reader As New StreamReader(csvPath, Encoding.UTF8)
            Dim headerLine As String = reader.ReadLine()
            If headerLine Is Nothing Then
                Return dt
            End If

            Dim headers = ParseCsvLine(headerLine)
            For Each h In headers
                dt.Columns.Add(h)
            Next

            While Not reader.EndOfStream
                Dim line = reader.ReadLine()
                Dim values = ParseCsvLine(line)
                Dim row = dt.NewRow()
                For i As Integer = 0 To dt.Columns.Count - 1
                    If i < values.Count Then
                        row(i) = If(String.IsNullOrEmpty(values(i)), DBNull.Value, CType(values(i), Object))
                    Else
                        row(i) = DBNull.Value
                    End If
                Next
                dt.Rows.Add(row)
            End While
        End Using
        Return dt
    End Function

    Private Function EscapeCsv(value As String) As String
        If value Is Nothing Then Return ""
        Dim mustQuote As Boolean = value.IndexOfAny(New Char() {","c, """"c, Chr(10), Chr(13)}) >= 0
        Dim escaped As String = value.Replace("""", """""")
        If mustQuote Then
            Return """" & escaped & """"
        End If
        Return escaped
    End Function

    Private Function ParseCsvLine(line As String) As List(Of String)
        Dim result As New List(Of String)()
        Dim sb As New StringBuilder()
        Dim inQuotes As Boolean = False
        Dim i As Integer = 0
        While i < line.Length
            Dim c As Char = line(i)
            If inQuotes Then
                If c = """"c Then
                    If i + 1 < line.Length AndAlso line(i + 1) = """"c Then
                        sb.Append(""""c)
                        i += 1
                    Else
                        inQuotes = False
                    End If
                Else
                    sb.Append(c)
                End If
            Else
                If c = """"c Then
                    inQuotes = True
                ElseIf c = ","c Then
                    result.Add(sb.ToString())
                    sb.Clear()
                Else
                    sb.Append(c)
                End If
            End If
            i += 1
        End While
        result.Add(sb.ToString())
        Return result
    End Function
End Class