<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormIDScanner
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormIDScanner))
        Me.txtTagID = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtFlag = New System.Windows.Forms.Label()
        Me.labelButtonClose = New System.Windows.Forms.Label()
        Me.txtComPort = New System.Windows.Forms.Label()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.Panel3 = New System.Windows.Forms.Panel()
        Me.Panel4 = New System.Windows.Forms.Panel()
        Me.Panel5 = New System.Windows.Forms.Panel()
        Me.cboCOMPort = New System.Windows.Forms.ComboBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.btnRefresh = New System.Windows.Forms.Button()
        Me.btnConnect = New System.Windows.Forms.Button()
        Me.Panel6 = New System.Windows.Forms.Panel()
        Me.pbUsbImage = New System.Windows.Forms.PictureBox()
        Me.Panel5.SuspendLayout()
        Me.Panel6.SuspendLayout()
        CType(Me.pbUsbImage, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'txtTagID
        '
        Me.txtTagID.Font = New System.Drawing.Font("Segoe UI", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtTagID.Location = New System.Drawing.Point(209, 143)
        Me.txtTagID.Name = "txtTagID"
        Me.txtTagID.Size = New System.Drawing.Size(353, 33)
        Me.txtTagID.TabIndex = 0
        Me.txtTagID.Visible = False
        '
        'Label1
        '
        Me.Label1.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.Label1.Font = New System.Drawing.Font("Segoe UI Semibold", 13.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(147, 195)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(415, 25)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "PLACE THE CARD ON THE RFID READER"
        '
        'txtFlag
        '
        Me.txtFlag.AutoSize = True
        Me.txtFlag.Font = New System.Drawing.Font("Segoe UI Semibold", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtFlag.Location = New System.Drawing.Point(568, 149)
        Me.txtFlag.Name = "txtFlag"
        Me.txtFlag.Size = New System.Drawing.Size(19, 21)
        Me.txtFlag.TabIndex = 2
        Me.txtFlag.Text = "0"
        Me.txtFlag.Visible = False
        '
        'labelButtonClose
        '
        Me.labelButtonClose.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.labelButtonClose.AutoSize = True
        Me.labelButtonClose.Cursor = System.Windows.Forms.Cursors.Hand
        Me.labelButtonClose.Font = New System.Drawing.Font("Segoe UI", 13.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.labelButtonClose.ForeColor = System.Drawing.Color.Crimson
        Me.labelButtonClose.Location = New System.Drawing.Point(629, 12)
        Me.labelButtonClose.Name = "labelButtonClose"
        Me.labelButtonClose.Size = New System.Drawing.Size(24, 25)
        Me.labelButtonClose.TabIndex = 3
        Me.labelButtonClose.Text = "X"
        '
        'txtComPort
        '
        Me.txtComPort.AutoSize = True
        Me.txtComPort.Font = New System.Drawing.Font("Century Gothic", 20.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtComPort.Location = New System.Drawing.Point(141, 144)
        Me.txtComPort.Name = "txtComPort"
        Me.txtComPort.Size = New System.Drawing.Size(71, 32)
        Me.txtComPort.TabIndex = 4
        Me.txtComPort.Text = "Port:"
        '
        'Panel1
        '
        Me.Panel1.BackColor = System.Drawing.Color.SteelBlue
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.Panel1.Location = New System.Drawing.Point(0, 363)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(680, 10)
        Me.Panel1.TabIndex = 5
        '
        'Panel2
        '
        Me.Panel2.BackColor = System.Drawing.Color.SteelBlue
        Me.Panel2.Dock = System.Windows.Forms.DockStyle.Right
        Me.Panel2.Location = New System.Drawing.Point(670, 0)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(10, 363)
        Me.Panel2.TabIndex = 6
        '
        'Panel3
        '
        Me.Panel3.BackColor = System.Drawing.Color.SteelBlue
        Me.Panel3.Dock = System.Windows.Forms.DockStyle.Top
        Me.Panel3.Location = New System.Drawing.Point(0, 0)
        Me.Panel3.Name = "Panel3"
        Me.Panel3.Size = New System.Drawing.Size(670, 10)
        Me.Panel3.TabIndex = 7
        '
        'Panel4
        '
        Me.Panel4.BackColor = System.Drawing.Color.SteelBlue
        Me.Panel4.Dock = System.Windows.Forms.DockStyle.Left
        Me.Panel4.Location = New System.Drawing.Point(0, 10)
        Me.Panel4.Name = "Panel4"
        Me.Panel4.Size = New System.Drawing.Size(10, 353)
        Me.Panel4.TabIndex = 8
        '
        'Panel5
        '
        Me.Panel5.BackColor = System.Drawing.Color.SteelBlue
        Me.Panel5.Controls.Add(Me.cboCOMPort)
        Me.Panel5.Controls.Add(Me.Label2)
        Me.Panel5.Controls.Add(Me.btnRefresh)
        Me.Panel5.Controls.Add(Me.btnConnect)
        Me.Panel5.Location = New System.Drawing.Point(11, 315)
        Me.Panel5.Name = "Panel5"
        Me.Panel5.Size = New System.Drawing.Size(659, 48)
        Me.Panel5.TabIndex = 10
        '
        'cboCOMPort
        '
        Me.cboCOMPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboCOMPort.Font = New System.Drawing.Font("Segoe UI", 10.2!)
        Me.cboCOMPort.FormattingEnabled = True
        Me.cboCOMPort.Location = New System.Drawing.Point(90, 11)
        Me.cboCOMPort.Name = "cboCOMPort"
        Me.cboCOMPort.Size = New System.Drawing.Size(217, 27)
        Me.cboCOMPort.TabIndex = 0
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Segoe UI Semibold", 10.2!, System.Drawing.FontStyle.Bold)
        Me.Label2.ForeColor = System.Drawing.Color.White
        Me.Label2.Location = New System.Drawing.Point(15, 15)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(75, 19)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "COM Port:"
        '
        'btnRefresh
        '
        Me.btnRefresh.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnRefresh.Font = New System.Drawing.Font("Segoe UI Semibold", 10.2!, System.Drawing.FontStyle.Bold)
        Me.btnRefresh.ForeColor = System.Drawing.Color.White
        Me.btnRefresh.Location = New System.Drawing.Point(419, 8)
        Me.btnRefresh.Name = "btnRefresh"
        Me.btnRefresh.Size = New System.Drawing.Size(100, 32)
        Me.btnRefresh.TabIndex = 2
        Me.btnRefresh.Text = "Refresh"
        Me.btnRefresh.UseVisualStyleBackColor = False
        '
        'btnConnect
        '
        Me.btnConnect.BackColor = System.Drawing.Color.FromArgb(CType(CType(46, Byte), Integer), CType(CType(204, Byte), Integer), CType(CType(113, Byte), Integer))
        Me.btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnConnect.Font = New System.Drawing.Font("Segoe UI Semibold", 10.2!, System.Drawing.FontStyle.Bold)
        Me.btnConnect.ForeColor = System.Drawing.Color.White
        Me.btnConnect.Location = New System.Drawing.Point(313, 8)
        Me.btnConnect.Name = "btnConnect"
        Me.btnConnect.Size = New System.Drawing.Size(100, 32)
        Me.btnConnect.TabIndex = 1
        Me.btnConnect.Text = "Connect"
        Me.btnConnect.UseVisualStyleBackColor = False
        '
        'Panel6
        '
        Me.Panel6.BackColor = System.Drawing.Color.SteelBlue
        Me.Panel6.Controls.Add(Me.labelButtonClose)
        Me.Panel6.Location = New System.Drawing.Point(11, 10)
        Me.Panel6.Name = "Panel6"
        Me.Panel6.Size = New System.Drawing.Size(658, 48)
        Me.Panel6.TabIndex = 11
        '
        'pbUsbImage
        '
        Me.pbUsbImage.ErrorImage = Nothing
        Me.pbUsbImage.Image = Global.Tala_Attendance_Management_System.My.Resources.Resources.usb_connected
        Me.pbUsbImage.Location = New System.Drawing.Point(94, 143)
        Me.pbUsbImage.Name = "pbUsbImage"
        Me.pbUsbImage.Size = New System.Drawing.Size(52, 39)
        Me.pbUsbImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.pbUsbImage.TabIndex = 9
        Me.pbUsbImage.TabStop = False
        '
        'FormIDScanner
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 19.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.AutoSize = True
        Me.BackColor = System.Drawing.Color.Gainsboro
        Me.ClientSize = New System.Drawing.Size(680, 373)
        Me.Controls.Add(Me.Panel6)
        Me.Controls.Add(Me.Panel5)
        Me.Controls.Add(Me.pbUsbImage)
        Me.Controls.Add(Me.Panel4)
        Me.Controls.Add(Me.Panel3)
        Me.Controls.Add(Me.Panel2)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.txtComPort)
        Me.Controls.Add(Me.txtFlag)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtTagID)
        Me.Font = New System.Drawing.Font("Segoe UI", 10.2!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.Name = "FormIDScanner"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "FormIDScanner"
        Me.TopMost = True
        Me.Panel5.ResumeLayout(False)
        Me.Panel5.PerformLayout()
        Me.Panel6.ResumeLayout(False)
        Me.Panel6.PerformLayout()
        CType(Me.pbUsbImage, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents txtTagID As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents txtFlag As Label
    Friend WithEvents labelButtonClose As Label
    Friend WithEvents txtComPort As Label
    Friend WithEvents Panel1 As Panel
    Friend WithEvents Panel2 As Panel
    Friend WithEvents Panel3 As Panel
    Friend WithEvents Panel4 As Panel
    Friend WithEvents Panel6 As Panel
    Friend WithEvents pbUsbImage As PictureBox
    Friend WithEvents Panel5 As Panel
    Friend WithEvents cboCOMPort As ComboBox
    Friend WithEvents btnConnect As Button
    Friend WithEvents btnRefresh As Button
    Friend WithEvents Label2 As Label
End Class
