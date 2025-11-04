Imports System.Runtime.CompilerServices
Public Module Extensions
    <Extension()>
    Public Function IsNullOrEmpty(value As String) As Boolean
        Return String.IsNullOrEmpty(value)
    End Function

    <Extension()>
    Public Function IsNullOrWhiteSpace(value As String) As Boolean
        Return String.IsNullOrWhiteSpace(value)
    End Function

    <Extension()>
    Public Function ToIntOrDefault(value As Object, Optional defaultValue As Integer = 0) As Integer
        If value Is Nothing OrElse IsDBNull(value) Then
            Return defaultValue
        End If

        Dim result As Integer
        If Integer.TryParse(value.ToString(), result) Then
            Return result
        End If

        Return defaultValue
    End Function

    <Extension()>
    Public Function ToStringOrEmpty(value As Object) As String
        If value Is Nothing OrElse IsDBNull(value) Then
            Return String.Empty
        End If
        Return value.ToString()
    End Function

    <Extension()>
    Public Function ToDisplayDate(value As Date) As String
        Return value.ToString(Constants.DISPLAY_DATE_FORMAT)
    End Function

    <Extension()>
    Public Function ToDisplayTime(value As Date) As String
        Return value.ToString(Constants.DISPLAY_TIME_FORMAT)
    End Function
End Module
