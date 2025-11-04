Imports System.Drawing.Drawing2D

Public Class SuccessDialog
    Inherits Form

    Private lblSuccess As Label
    Private lblMessage As Label
    Private btnYes As Button
    Private btnNo As Button
    Private btnOk As Button
    Private checkmarkPanel As Panel
    Private autoCloseTimer As Timer
    Private animationTimer As Timer
    Private animationProgress As Single = 0
    Private checkmarkSize As Integer = 120

    Public Property Message As String = "Your action has been completed successfully."
    Public Property AutoCloseSeconds As Integer = 0 ' 0 = no auto-close
    Public Property ShowYesNo As Boolean = False
    Public Property ShowOk As Boolean = False
    Public Property DialogResultValue As DialogResult = DialogResult.None

    Public Sub New()
        InitializeComponent()
        SetupAnimation()
    End Sub

    Private Sub InitializeComponent()
        Me.FormBorderStyle = FormBorderStyle.None
        Me.StartPosition = FormStartPosition.CenterParent
        Me.Size = New Size(450, 400)
        Me.BackColor = Color.White
        Me.ShowInTaskbar = False
        Me.TopMost = True

        checkmarkPanel = New Panel()
        checkmarkPanel.Size = New Size(checkmarkSize, checkmarkSize)
        checkmarkPanel.Location = New Point((Me.Width - checkmarkSize) \ 2, 40)
        checkmarkPanel.BackColor = Color.Transparent
        AddHandler checkmarkPanel.Paint, AddressOf CheckmarkPanel_Paint
        Me.Controls.Add(checkmarkPanel)

        ' Success label
        lblSuccess = New Label()
        lblSuccess.Text = "Success!"
        lblSuccess.Font = New Font("Segoe UI", 28, FontStyle.Bold)
        lblSuccess.ForeColor = Color.FromArgb(52, 73, 94)
        lblSuccess.AutoSize = True
        lblSuccess.Location = New Point((Me.Width - 200) \ 2, 180)
        Me.Controls.Add(lblSuccess)

        ' Message label
        lblMessage = New Label()
        lblMessage.Text = Message
        lblMessage.Font = New Font("Segoe UI", 12)
        lblMessage.ForeColor = Color.FromArgb(127, 140, 141)
        lblMessage.AutoSize = False
        lblMessage.Size = New Size(380, 80)
        lblMessage.Location = New Point(35, 240)
        lblMessage.TextAlign = ContentAlignment.TopCenter
        Me.Controls.Add(lblMessage)

        ' Yes button
        btnYes = New Button()
        btnYes.Text = "Yes"
        btnYes.Size = New Size(100, 40)
        btnYes.Location = New Point(120, 330)
        btnYes.BackColor = Color.FromArgb(46, 204, 113)
        btnYes.ForeColor = Color.White
        btnYes.FlatStyle = FlatStyle.Flat
        btnYes.Font = New Font("Segoe UI", 11, FontStyle.Bold)
        btnYes.Cursor = Cursors.Hand
        btnYes.Visible = False
        AddHandler btnYes.Click, AddressOf BtnYes_Click
        Me.Controls.Add(btnYes)

        ' No button
        btnNo = New Button()
        btnNo.Text = "No"
        btnNo.Size = New Size(100, 40)
        btnNo.Location = New Point(230, 330)
        btnNo.BackColor = Color.FromArgb(231, 76, 60)
        btnNo.ForeColor = Color.White
        btnNo.FlatStyle = FlatStyle.Flat
        btnNo.Font = New Font("Segoe UI", 11, FontStyle.Bold)
        btnNo.Cursor = Cursors.Hand
        btnNo.Visible = False
        AddHandler btnNo.Click, AddressOf BtnNo_Click
        Me.Controls.Add(btnNo)

        ' OK button
        btnOk = New Button()
        btnOk.Text = "OK"
        btnOk.Size = New Size(120, 40)
        btnOk.Location = New Point((Me.Width - 120) \ 2, 330)
        btnOk.BackColor = Color.FromArgb(52, 152, 219)
        btnOk.ForeColor = Color.White
        btnOk.FlatStyle = FlatStyle.Flat
        btnOk.Font = New Font("Segoe UI", 11, FontStyle.Bold)
        btnOk.Cursor = Cursors.Hand
        btnOk.Visible = False
        AddHandler btnOk.Click, AddressOf BtnOk_Click
        Me.Controls.Add(btnOk)
        Me.Region = New Region(CreateRoundedRectangle(Me.ClientRectangle, 10))
    End Sub

    Private Sub SetupAnimation()
        animationTimer = New Timer()
        animationTimer.Interval = 20 ' 50 FPS
        AddHandler animationTimer.Tick, AddressOf AnimationTimer_Tick
    End Sub

    Private Sub AnimationTimer_Tick(sender As Object, e As EventArgs)
        animationProgress += 0.15F
        If animationProgress >= 1.0F Then
            animationProgress = 1.0F
            animationTimer.Stop()
        End If
        checkmarkPanel.Invalidate()
    End Sub

    Private Sub CheckmarkPanel_Paint(sender As Object, e As PaintEventArgs)
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias

        Dim circleRect As New Rectangle(10, 10, checkmarkSize - 20, checkmarkSize - 20)
        Using circleBrush As New SolidBrush(Color.FromArgb(46, 204, 113))
            g.FillEllipse(circleBrush, circleRect)
        End Using

        If animationProgress > 0 Then
            Using pen As New Pen(Color.White, 8)
                pen.StartCap = LineCap.Round
                pen.EndCap = LineCap.Round

                Dim centerX As Integer = checkmarkSize \ 2
                Dim centerY As Integer = checkmarkSize \ 2
                Dim pt1 As New PointF(centerX - 20, centerY)
                Dim pt2 As New PointF(centerX - 5, centerY + 15)
                Dim pt3 As New PointF(centerX + 25, centerY - 20)
                Dim progress1 As Single = Math.Min(animationProgress * 2, 1.0F)
                Dim progress2 As Single = Math.Max((animationProgress - 0.5F) * 2, 0)

                If progress1 > 0 Then
                    Dim x As Single = pt1.X + (pt2.X - pt1.X) * progress1
                    Dim y As Single = pt1.Y + (pt2.Y - pt1.Y) * progress1
                    g.DrawLine(pen, pt1, New PointF(x, y))
                End If

                If progress2 > 0 Then
                    Dim x As Single = pt2.X + (pt3.X - pt2.X) * progress2
                    Dim y As Single = pt2.Y + (pt3.Y - pt2.Y) * progress2
                    g.DrawLine(pen, pt2, New PointF(x, y))
                End If
            End Using
        End If
    End Sub

    Private Function CreateRoundedRectangle(rect As Rectangle, radius As Integer) As GraphicsPath
        Dim path As New GraphicsPath()
        path.AddArc(rect.X, rect.Y, radius, radius, 180, 90)
        path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90)
        path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90)
        path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90)
        path.CloseFigure()
        Return path
    End Function

    Private Sub BtnYes_Click(sender As Object, e As EventArgs)
        DialogResultValue = DialogResult.Yes
        Me.Close()
    End Sub

    Private Sub BtnNo_Click(sender As Object, e As EventArgs)
        DialogResultValue = DialogResult.No
        Me.Close()
    End Sub

    Private Sub BtnOk_Click(sender As Object, e As EventArgs)
        DialogResultValue = DialogResult.OK
        Me.Close()
    End Sub

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)

        lblMessage.Text = Message
        lblSuccess.Location = New Point((Me.Width - lblSuccess.Width) \ 2, 180)

        If ShowYesNo Then
            btnYes.Visible = True
            btnNo.Visible = True
            btnOk.Visible = False
        ElseIf ShowOk Then
            btnYes.Visible = False
            btnNo.Visible = False
            btnOk.Visible = True
        Else
            btnYes.Visible = False
            btnNo.Visible = False
            btnOk.Visible = False
        End If

        animationProgress = 0
        animationTimer.Start()

        If AutoCloseSeconds > 0 Then
            autoCloseTimer = New Timer()
            autoCloseTimer.Interval = AutoCloseSeconds * 1000
            AddHandler autoCloseTimer.Tick, Sub(s, ev)
                                                autoCloseTimer.Stop()
                                                DialogResultValue = DialogResult.OK
                                                Me.Close()
                                            End Sub
            autoCloseTimer.Start()
        End If
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        If animationTimer IsNot Nothing Then animationTimer.Stop()
        If autoCloseTimer IsNot Nothing Then autoCloseTimer.Stop()
        MyBase.OnFormClosing(e)
    End Sub

    Public Shared Function ShowSuccess(message As String, Optional autoCloseSeconds As Integer = 0) As DialogResult
        Using dialog As New SuccessDialog()
            dialog.Message = message
            dialog.AutoCloseSeconds = autoCloseSeconds
            dialog.ShowDialog()
            Return dialog.DialogResultValue
        End Using
    End Function

    Public Shared Function ShowWithYesNo(message As String) As DialogResult
        Using dialog As New SuccessDialog()
            dialog.Message = message
            dialog.ShowYesNo = True
            dialog.ShowDialog()
            Return dialog.DialogResultValue
        End Using
    End Function

    Public Shared Function ShowWithOk(message As String) As DialogResult
        Using dialog As New SuccessDialog()
            dialog.Message = message
            dialog.ShowOk = True
            dialog.ShowDialog()
            Return dialog.DialogResultValue
        End Using
    End Function
End Class
