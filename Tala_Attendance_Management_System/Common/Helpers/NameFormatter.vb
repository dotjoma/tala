Public Class NameFormatter
    Public Shared Function FormatFullName(firstName As String, Optional middleName As String = "", Optional lastName As String = "", Optional extName As String = "") As String
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
    Public Shared Function GetNameConcatSQL() As String
        Return "CONCAT(firstname, " &
               "IF(middlename IS NULL OR middlename = '' OR middlename = '--' OR UPPER(middlename) = 'N/A', '', CONCAT(' ', middlename)), " &
               "' ', lastname, " &
               "IF(extName IS NULL OR extName = '' OR extName = '--' OR UPPER(extName) = 'N/A', '', CONCAT(' ', extName)))"
    End Function
End Class
