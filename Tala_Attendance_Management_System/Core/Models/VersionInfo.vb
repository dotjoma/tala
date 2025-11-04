Imports System

''' <summary>
''' Model representing version information from remote source
''' </summary>
Public Class VersionInfo
    Public Property Version As String
    Public Property DownloadUrl As String
    Public Property ChangeLog As String

    Public Sub New()
    End Sub

    Public Sub New(version As String, downloadUrl As String, changeLog As String)
        Me.Version = version
        Me.DownloadUrl = downloadUrl
        Me.ChangeLog = changeLog
    End Sub

    Public Function IsNewerThan(otherVersion As String) As Boolean
        Try
            Dim currentVer As New Version(Me.Version)
            Dim otherVer As New Version(otherVersion)
            Return currentVer > otherVer
        Catch ex As Exception
            Return False
        End Try
    End Function
End Class