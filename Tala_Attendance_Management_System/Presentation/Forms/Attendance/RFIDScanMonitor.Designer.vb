<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class RFIDScanMonitor
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(RFIDScanMonitor))
        Me.pnlHeader = New System.Windows.Forms.Panel()
        Me.lblSubtitle = New System.Windows.Forms.Label()
        Me.lblTitle = New System.Windows.Forms.Label()
        Me.pnlMain = New System.Windows.Forms.Panel()
        Me.pnlCardContainer = New System.Windows.Forms.Panel()
        Me.lblWaitingMessage = New System.Windows.Forms.Label()
        Me.picRFIDIcon = New System.Windows.Forms.PictureBox()
        Me.pbSchool = New System.Windows.Forms.PictureBox()
        Me.pnlFooter = New System.Windows.Forms.Panel()
        Me.lblClock = New System.Windows.Forms.Label()
        Me.pnlHeader.SuspendLayout()
        Me.pnlMain.SuspendLayout()
        CType(Me.picRFIDIcon, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.pbSchool, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlFooter.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlHeader
        '
        Me.pnlHeader.BackColor = System.Drawing.Color.FromArgb(CType(CType(30, Byte), Integer), CType(CType(100, Byte), Integer), CType(CType(160, Byte), Integer))
        Me.pnlHeader.Controls.Add(Me.lblSubtitle)
        Me.pnlHeader.Controls.Add(Me.lblTitle)
        Me.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlHeader.Location = New System.Drawing.Point(0, 0)
        Me.pnlHeader.Name = "pnlHeader"
        Me.pnlHeader.Padding = New System.Windows.Forms.Padding(20, 15, 20, 15)
        Me.pnlHeader.Size = New System.Drawing.Size(1024, 150)
        Me.pnlHeader.TabIndex = 0
        '
        'lblSubtitle
        '
        Me.lblSubtitle.Dock = System.Windows.Forms.DockStyle.Top
        Me.lblSubtitle.Font = New System.Drawing.Font("Segoe UI", 18.0!)
        Me.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(200, Byte), Integer), CType(CType(230, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.lblSubtitle.Location = New System.Drawing.Point(20, 90)
        Me.lblSubtitle.Name = "lblSubtitle"
        Me.lblSubtitle.Size = New System.Drawing.Size(984, 45)
        Me.lblSubtitle.TabIndex = 1
        Me.lblSubtitle.Text = "Faculty Attendance Monitoring System"
        Me.lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblTitle
        '
        Me.lblTitle.Dock = System.Windows.Forms.DockStyle.Top
        Me.lblTitle.Font = New System.Drawing.Font("Segoe UI", 42.0!, System.Drawing.FontStyle.Bold)
        Me.lblTitle.ForeColor = System.Drawing.Color.White
        Me.lblTitle.Location = New System.Drawing.Point(20, 15)
        Me.lblTitle.Name = "lblTitle"
        Me.lblTitle.Size = New System.Drawing.Size(984, 75)
        Me.lblTitle.TabIndex = 0
        Me.lblTitle.Text = "TALA HIGH SCHOOL"
        Me.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'pnlMain
        '
        Me.pnlMain.BackColor = System.Drawing.Color.White
        Me.pnlMain.Controls.Add(Me.pnlCardContainer)
        Me.pnlMain.Controls.Add(Me.lblWaitingMessage)
        Me.pnlMain.Controls.Add(Me.picRFIDIcon)
        Me.pnlMain.Controls.Add(Me.pbSchool)
        Me.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlMain.Location = New System.Drawing.Point(0, 150)
        Me.pnlMain.Name = "pnlMain"
        Me.pnlMain.Padding = New System.Windows.Forms.Padding(30)
        Me.pnlMain.Size = New System.Drawing.Size(1024, 498)
        Me.pnlMain.TabIndex = 1
        '
        'pnlCardContainer
        '
        Me.pnlCardContainer.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.pnlCardContainer.BackColor = System.Drawing.Color.Transparent
        Me.pnlCardContainer.Location = New System.Drawing.Point(62, 51)
        Me.pnlCardContainer.Name = "pnlCardContainer"
        Me.pnlCardContainer.Size = New System.Drawing.Size(900, 400)
        Me.pnlCardContainer.TabIndex = 2
        Me.pnlCardContainer.Visible = False
        '
        'lblWaitingMessage
        '
        Me.lblWaitingMessage.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.lblWaitingMessage.BackColor = System.Drawing.Color.Transparent
        Me.lblWaitingMessage.Font = New System.Drawing.Font("Segoe UI", 28.0!, System.Drawing.FontStyle.Bold)
        Me.lblWaitingMessage.ForeColor = System.Drawing.Color.Black
        Me.lblWaitingMessage.Location = New System.Drawing.Point(50, 445)
        Me.lblWaitingMessage.Name = "lblWaitingMessage"
        Me.lblWaitingMessage.Size = New System.Drawing.Size(924, 70)
        Me.lblWaitingMessage.TabIndex = 1
        Me.lblWaitingMessage.Text = "TAP YOUR CARD"
        Me.lblWaitingMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'picRFIDIcon
        '
        Me.picRFIDIcon.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.picRFIDIcon.BackColor = System.Drawing.Color.Transparent
        Me.picRFIDIcon.Image = Global.Tala_Attendance_Management_System.My.Resources.Resources.rfid_nobg
        Me.picRFIDIcon.Location = New System.Drawing.Point(312, 54)
        Me.picRFIDIcon.Name = "picRFIDIcon"
        Me.picRFIDIcon.Size = New System.Drawing.Size(400, 400)
        Me.picRFIDIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.picRFIDIcon.TabIndex = 0
        Me.picRFIDIcon.TabStop = False
        '
        'pbSchool
        '
        Me.pbSchool.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pbSchool.Image = Global.Tala_Attendance_Management_System.My.Resources.Resources.school
        Me.pbSchool.Location = New System.Drawing.Point(0, 0)
        Me.pbSchool.Name = "pbSchool"
        Me.pbSchool.Size = New System.Drawing.Size(1024, 498)
        Me.pbSchool.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.pbSchool.TabIndex = 3
        Me.pbSchool.TabStop = False
        '
        'pnlFooter
        '
        Me.pnlFooter.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(73, Byte), Integer), CType(CType(94, Byte), Integer))
        Me.pnlFooter.Controls.Add(Me.lblClock)
        Me.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlFooter.Location = New System.Drawing.Point(0, 648)
        Me.pnlFooter.Name = "pnlFooter"
        Me.pnlFooter.Padding = New System.Windows.Forms.Padding(20, 5, 20, 5)
        Me.pnlFooter.Size = New System.Drawing.Size(1024, 80)
        Me.pnlFooter.TabIndex = 2
        '
        'lblClock
        '
        Me.lblClock.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblClock.Font = New System.Drawing.Font("Segoe UI", 22.0!, System.Drawing.FontStyle.Bold)
        Me.lblClock.ForeColor = System.Drawing.Color.FromArgb(CType(CType(236, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(241, Byte), Integer))
        Me.lblClock.Location = New System.Drawing.Point(20, 5)
        Me.lblClock.Name = "lblClock"
        Me.lblClock.Size = New System.Drawing.Size(984, 70)
        Me.lblClock.TabIndex = 0
        Me.lblClock.Text = "Sunday, October 19, 2025 - 00:00:00 AM"
        Me.lblClock.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'RFIDScanMonitor
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(1024, 728)
        Me.Controls.Add(Me.pnlMain)
        Me.Controls.Add(Me.pnlFooter)
        Me.Controls.Add(Me.pnlHeader)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "RFIDScanMonitor"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "RFID Scan Monitor - Tala High School"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.pnlHeader.ResumeLayout(False)
        Me.pnlMain.ResumeLayout(False)
        CType(Me.picRFIDIcon, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.pbSchool, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlFooter.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents pnlHeader As Panel
    Friend WithEvents lblTitle As Label
    Friend WithEvents lblSubtitle As Label
    Friend WithEvents pnlMain As Panel
    Friend WithEvents picRFIDIcon As PictureBox
    Friend WithEvents lblWaitingMessage As Label
    Friend WithEvents pnlCardContainer As Panel
    Friend WithEvents pnlFooter As Panel
    Friend WithEvents lblClock As Label
    Friend WithEvents pbSchool As PictureBox
End Class
