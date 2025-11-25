<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormManageAttendance
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormManageAttendance))
        Me.panelHeader = New System.Windows.Forms.Panel()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.lblTitle = New System.Windows.Forms.Label()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.cboShift = New System.Windows.Forms.ComboBox()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.cboDepartment = New System.Windows.Forms.ComboBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.lblCutoffWarning = New System.Windows.Forms.Label()
        Me.pnlTeacherInfo = New System.Windows.Forms.Panel()
        Me.pbTeacherPhoto = New System.Windows.Forms.PictureBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.lblTeacherName = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.lblEmployeeID = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.lblDepartment = New System.Windows.Forms.Label()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.lblExpectedIn = New System.Windows.Forms.Label()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.lblExpectedOut = New System.Windows.Forms.Label()
        Me.lblExistingRecord = New System.Windows.Forms.Label()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.rbBoth = New System.Windows.Forms.RadioButton()
        Me.rbTimeOutOnly = New System.Windows.Forms.RadioButton()
        Me.rbTimeInOnly = New System.Windows.Forms.RadioButton()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.chkTimeOut = New System.Windows.Forms.CheckBox()
        Me.dtpTimeOut = New System.Windows.Forms.DateTimePicker()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.chkTimeIn = New System.Windows.Forms.CheckBox()
        Me.dtpTimeIn = New System.Windows.Forms.DateTimePicker()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.Label14 = New System.Windows.Forms.Label()
        Me.txtRemarks = New System.Windows.Forms.TextBox()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.btnClear = New System.Windows.Forms.Button()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.panelHeader.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox1.SuspendLayout()
        Me.pnlTeacherInfo.SuspendLayout()
        CType(Me.pbTeacherPhoto, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.SuspendLayout()
        '
        'panelHeader
        '
        Me.panelHeader.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.panelHeader.Controls.Add(Me.PictureBox1)
        Me.panelHeader.Controls.Add(Me.lblTitle)
        Me.panelHeader.Dock = System.Windows.Forms.DockStyle.Top
        Me.panelHeader.Location = New System.Drawing.Point(0, 0)
        Me.panelHeader.Name = "panelHeader"
        Me.panelHeader.Size = New System.Drawing.Size(900, 60)
        Me.panelHeader.TabIndex = 0
        '
        'PictureBox1
        '
        Me.PictureBox1.Image = CType(resources.GetObject("PictureBox1.Image"), System.Drawing.Image)
        Me.PictureBox1.Location = New System.Drawing.Point(12, 12)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(40, 36)
        Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.PictureBox1.TabIndex = 1
        Me.PictureBox1.TabStop = False
        '
        'lblTitle
        '
        Me.lblTitle.AutoSize = True
        Me.lblTitle.Font = New System.Drawing.Font("Segoe UI", 18.0!, System.Drawing.FontStyle.Bold)
        Me.lblTitle.ForeColor = System.Drawing.Color.White
        Me.lblTitle.Location = New System.Drawing.Point(58, 15)
        Me.lblTitle.Name = "lblTitle"
        Me.lblTitle.Size = New System.Drawing.Size(346, 32)
        Me.lblTitle.TabIndex = 0
        Me.lblTitle.Text = "MANAGE ATTENDANCE"
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.cboShift)
        Me.GroupBox1.Controls.Add(Me.Label8)
        Me.GroupBox1.Controls.Add(Me.cboDepartment)
        Me.GroupBox1.Controls.Add(Me.Label7)
        Me.GroupBox1.Controls.Add(Me.cboTeacher)
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Controls.Add(Me.dtpDate)
        Me.GroupBox1.Controls.Add(Me.Label2)
        Me.GroupBox1.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
        Me.GroupBox1.Location = New System.Drawing.Point(12, 75)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(876, 120)
        Me.GroupBox1.TabIndex = 1
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Search && Filter"
        '
        'cboShift
        '
        Me.cboShift.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboShift.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.cboShift.FormattingEnabled = True
        Me.cboShift.Location = New System.Drawing.Point(450, 75)
        Me.cboShift.Name = "cboShift"
        Me.cboShift.Size = New System.Drawing.Size(400, 25)
        Me.cboShift.TabIndex = 7
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.Label8.Location = New System.Drawing.Point(446, 52)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(40, 19)
        Me.Label8.TabIndex = 6
        Me.Label8.Text = "Shift:"
        '
        'cboDepartment
        '
        Me.cboDepartment.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboDepartment.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.cboDepartment.FormattingEnabled = True
        Me.cboDepartment.Location = New System.Drawing.Point(20, 75)
        Me.cboDepartment.Name = "cboDepartment"
        Me.cboDepartment.Size = New System.Drawing.Size(400, 25)
        Me.cboDepartment.TabIndex = 5
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.Label7.Location = New System.Drawing.Point(16, 52)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(87, 19)
        Me.Label7.TabIndex = 4
        Me.Label7.Text = "Department:"
        '
        'cboTeacher
        '
        Me.cboTeacher.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboTeacher.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.cboTeacher.FormattingEnabled = True
        Me.cboTeacher.Location = New System.Drawing.Point(450, 24)
        Me.cboTeacher.Name = "cboTeacher"
        Me.cboTeacher.Size = New System.Drawing.Size(400, 25)
        Me.cboTeacher.TabIndex = 3
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.Label1.Location = New System.Drawing.Point(446, 25)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(60, 19)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "Teacher:"
        '
        'dtpDate
        '
        Me.dtpDate.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.dtpDate.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.dtpDate.Location = New System.Drawing.Point(20, 24)
        Me.dtpDate.Name = "dtpDate"
        Me.dtpDate.Size = New System.Drawing.Size(150, 25)
        Me.dtpDate.TabIndex = 1
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.Label2.Location = New System.Drawing.Point(16, 25)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(42, 19)
        Me.Label2.TabIndex = 0
        Me.Label2.Text = "Date:"
        '
        'lblCutoffWarning
        '
        Me.lblCutoffWarning.AutoSize = True
        Me.lblCutoffWarning.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
        Me.lblCutoffWarning.ForeColor = System.Drawing.Color.Red
        Me.lblCutoffWarning.Location = New System.Drawing.Point(12, 198)
        Me.lblCutoffWarning.Name = "lblCutoffWarning"
        Me.lblCutoffWarning.Size = New System.Drawing.Size(350, 19)
        Me.lblCutoffWarning.TabIndex = 2
        Me.lblCutoffWarning.Text = "⚠ This date is beyond the editable cut-off period!"
        Me.lblCutoffWarning.Visible = False
        '
        'pnlTeacherInfo
        '
        Me.pnlTeacherInfo.BackColor = System.Drawing.Color.FromArgb(CType(CType(236, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(241, Byte), Integer))
        Me.pnlTeacherInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlTeacherInfo.Controls.Add(Me.lblExistingRecord)
        Me.pnlTeacherInfo.Controls.Add(Me.lblExpectedOut)
        Me.pnlTeacherInfo.Controls.Add(Me.Label11)
        Me.pnlTeacherInfo.Controls.Add(Me.lblExpectedIn)
        Me.pnlTeacherInfo.Controls.Add(Me.Label9)
        Me.pnlTeacherInfo.Controls.Add(Me.lblDepartment)
        Me.pnlTeacherInfo.Controls.Add(Me.Label6)
        Me.pnlTeacherInfo.Controls.Add(Me.lblEmployeeID)
        Me.pnlTeacherInfo.Controls.Add(Me.Label5)
        Me.pnlTeacherInfo.Controls.Add(Me.lblTeacherName)
        Me.pnlTeacherInfo.Controls.Add(Me.Label3)
        Me.pnlTeacherInfo.Controls.Add(Me.pbTeacherPhoto)
        Me.pnlTeacherInfo.Location = New System.Drawing.Point(12, 220)
        Me.pnlTeacherInfo.Name = "pnlTeacherInfo"
        Me.pnlTeacherInfo.Size = New System.Drawing.Size(876, 120)
        Me.pnlTeacherInfo.TabIndex = 3
        Me.pnlTeacherInfo.Visible = False
        '
        'pbTeacherPhoto
        '
        Me.pbTeacherPhoto.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pbTeacherPhoto.Location = New System.Drawing.Point(15, 15)
        Me.pbTeacherPhoto.Name = "pbTeacherPhoto"
        Me.pbTeacherPhoto.Size = New System.Drawing.Size(90, 90)
        Me.pbTeacherPhoto.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.pbTeacherPhoto.TabIndex = 0
        Me.pbTeacherPhoto.TabStop = False
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.Label3.Location = New System.Drawing.Point(120, 15)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(46, 15)
        Me.Label3.TabIndex = 1
        Me.Label3.Text = "Name:"
        '
        'lblTeacherName
        '
        Me.lblTeacherName.AutoSize = True
        Me.lblTeacherName.Font = New System.Drawing.Font("Segoe UI", 11.0!, System.Drawing.FontStyle.Bold)
        Me.lblTeacherName.Location = New System.Drawing.Point(200, 13)
        Me.lblTeacherName.Name = "lblTeacherName"
        Me.lblTeacherName.Size = New System.Drawing.Size(16, 20)
        Me.lblTeacherName.TabIndex = 2
        Me.lblTeacherName.Text = "-"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.Label5.Location = New System.Drawing.Point(120, 40)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(77, 15)
        Me.Label5.TabIndex = 3
        Me.Label5.Text = "Employee ID:"
        '
        'lblEmployeeID
        '
        Me.lblEmployeeID.AutoSize = True
        Me.lblEmployeeID.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.lblEmployeeID.Location = New System.Drawing.Point(200, 40)
        Me.lblEmployeeID.Name = "lblEmployeeID"
        Me.lblEmployeeID.Size = New System.Drawing.Size(12, 15)
        Me.lblEmployeeID.TabIndex = 4
        Me.lblEmployeeID.Text = "-"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.Label6.Location = New System.Drawing.Point(120, 60)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(73, 15)
        Me.Label6.TabIndex = 5
        Me.Label6.Text = "Department:"
        '
        'lblDepartment
        '
        Me.lblDepartment.AutoSize = True
        Me.lblDepartment.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.lblDepartment.Location = New System.Drawing.Point(200, 60)
        Me.lblDepartment.Name = "lblDepartment"
        Me.lblDepartment.Size = New System.Drawing.Size(12, 15)
        Me.lblDepartment.TabIndex = 6
        Me.lblDepartment.Text = "-"
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.Label9.Location = New System.Drawing.Point(120, 80)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(74, 15)
        Me.Label9.TabIndex = 7
        Me.Label9.Text = "Expected In:"
        '
        'lblExpectedIn
        '
        Me.lblExpectedIn.AutoSize = True
        Me.lblExpectedIn.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.lblExpectedIn.ForeColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.lblExpectedIn.Location = New System.Drawing.Point(200, 80)
        Me.lblExpectedIn.Name = "lblExpectedIn"
        Me.lblExpectedIn.Size = New System.Drawing.Size(13, 15)
        Me.lblExpectedIn.TabIndex = 8
        Me.lblExpectedIn.Text = "-"
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.Label11.Location = New System.Drawing.Point(350, 80)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(84, 15)
        Me.Label11.TabIndex = 9
        Me.Label11.Text = "Expected Out:"
        '
        'lblExpectedOut
        '
        Me.lblExpectedOut.AutoSize = True
        Me.lblExpectedOut.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.lblExpectedOut.ForeColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.lblExpectedOut.Location = New System.Drawing.Point(440, 80)
        Me.lblExpectedOut.Name = "lblExpectedOut"
        Me.lblExpectedOut.Size = New System.Drawing.Size(13, 15)
        Me.lblExpectedOut.TabIndex = 10
        Me.lblExpectedOut.Text = "-"
        '
        'lblExistingRecord
        '
        Me.lblExistingRecord.AutoSize = True
        Me.lblExistingRecord.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.lblExistingRecord.ForeColor = System.Drawing.Color.OrangeRed
        Me.lblExistingRecord.Location = New System.Drawing.Point(120, 100)
        Me.lblExistingRecord.Name = "lblExistingRecord"
        Me.lblExistingRecord.Size = New System.Drawing.Size(200, 15)
        Me.lblExistingRecord.TabIndex = 11
        Me.lblExistingRecord.Text = "⚠ Existing Record: Time In: 08:00"
        Me.lblExistingRecord.Visible = False
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.rbBoth)
        Me.GroupBox2.Controls.Add(Me.rbTimeOutOnly)
        Me.GroupBox2.Controls.Add(Me.rbTimeInOnly)
        Me.GroupBox2.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
        Me.GroupBox2.Location = New System.Drawing.Point(12, 346)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(876, 60)
        Me.GroupBox2.TabIndex = 4
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Entry Mode"
        '
        'rbTimeInOnly
        '
        Me.rbTimeInOnly.AutoSize = True
        Me.rbTimeInOnly.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.rbTimeInOnly.Location = New System.Drawing.Point(20, 25)
        Me.rbTimeInOnly.Name = "rbTimeInOnly"
        Me.rbTimeInOnly.Size = New System.Drawing.Size(115, 23)
        Me.rbTimeInOnly.TabIndex = 0
        Me.rbTimeInOnly.TabStop = True
        Me.rbTimeInOnly.Text = "Time In Only"
        Me.rbTimeInOnly.UseVisualStyleBackColor = True
        '
        'rbTimeOutOnly
        '
        Me.rbTimeOutOnly.AutoSize = True
        Me.rbTimeOutOnly.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.rbTimeOutOnly.Location = New System.Drawing.Point(200, 25)
        Me.rbTimeOutOnly.Name = "rbTimeOutOnly"
        Me.rbTimeOutOnly.Size = New System.Drawing.Size(127, 23)
        Me.rbTimeOutOnly.TabIndex = 1
        Me.rbTimeOutOnly.TabStop = True
        Me.rbTimeOutOnly.Text = "Time Out Only"
        Me.rbTimeOutOnly.UseVisualStyleBackColor = True
        '
        'rbBoth
        '
        Me.rbBoth.AutoSize = True
        Me.rbBoth.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.rbBoth.Location = New System.Drawing.Point(400, 25)
        Me.rbBoth.Name = "rbBoth"
        Me.rbBoth.Size = New System.Drawing.Size(169, 23)
        Me.rbBoth.TabIndex = 2
        Me.rbBoth.TabStop = True
        Me.rbBoth.Text = "Both Time In && Time Out"
        Me.rbBoth.UseVisualStyleBackColor = True
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.chkTimeOut)
        Me.GroupBox3.Controls.Add(Me.dtpTimeOut)
        Me.GroupBox3.Controls.Add(Me.Label13)
        Me.GroupBox3.Controls.Add(Me.chkTimeIn)
        Me.GroupBox3.Controls.Add(Me.dtpTimeIn)
        Me.GroupBox3.Controls.Add(Me.Label12)
        Me.GroupBox3.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
        Me.GroupBox3.Location = New System.Drawing.Point(12, 412)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(876, 80)
        Me.GroupBox3.TabIndex = 5
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "Time Entry"
        '
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.Label12.Location = New System.Drawing.Point(16, 35)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(60, 19)
        Me.Label12.TabIndex = 0
        Me.Label12.Text = "Time In:"
        '
        'dtpTimeIn
        '
        Me.dtpTimeIn.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.dtpTimeIn.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpTimeIn.Location = New System.Drawing.Point(120, 32)
        Me.dtpTimeIn.Name = "dtpTimeIn"
        Me.dtpTimeIn.ShowUpDown = True
        Me.dtpTimeIn.Size = New System.Drawing.Size(120, 25)
        Me.dtpTimeIn.TabIndex = 1
        '
        'chkTimeIn
        '
        Me.chkTimeIn.AutoSize = True
        Me.chkTimeIn.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.chkTimeIn.Location = New System.Drawing.Point(250, 35)
        Me.chkTimeIn.Name = "chkTimeIn"
        Me.chkTimeIn.Size = New System.Drawing.Size(15, 14)
        Me.chkTimeIn.TabIndex = 2
        Me.chkTimeIn.UseVisualStyleBackColor = True
        Me.chkTimeIn.Visible = False
        '
        'Label13
        '
        Me.Label13.AutoSize = True
        Me.Label13.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.Label13.Location = New System.Drawing.Point(450, 35)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(72, 19)
        Me.Label13.TabIndex = 3
        Me.Label13.Text = "Time Out:"
        '
        'dtpTimeOut
        '
        Me.dtpTimeOut.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.dtpTimeOut.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpTimeOut.Location = New System.Drawing.Point(530, 32)
        Me.dtpTimeOut.Name = "dtpTimeOut"
        Me.dtpTimeOut.ShowUpDown = True
        Me.dtpTimeOut.Size = New System.Drawing.Size(120, 25)
        Me.dtpTimeOut.TabIndex = 4
        '
        'chkTimeOut
        '
        Me.chkTimeOut.AutoSize = True
        Me.chkTimeOut.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.chkTimeOut.Location = New System.Drawing.Point(660, 35)
        Me.chkTimeOut.Name = "chkTimeOut"
        Me.chkTimeOut.Size = New System.Drawing.Size(15, 14)
        Me.chkTimeOut.TabIndex = 5
        Me.chkTimeOut.UseVisualStyleBackColor = True
        Me.chkTimeOut.Visible = False
        '
        'Label14
        '
        Me.Label14.AutoSize = True
        Me.Label14.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
        Me.Label14.Location = New System.Drawing.Point(12, 505)
        Me.Label14.Name = "Label14"
        Me.Label14.Size = New System.Drawing.Size(240, 19)
        Me.Label14.TabIndex = 6
        Me.Label14.Text = "Remarks (Reason for manual entry):"
        '
        'txtRemarks
        '
        Me.txtRemarks.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.txtRemarks.Location = New System.Drawing.Point(12, 527)
        Me.txtRemarks.Multiline = True
        Me.txtRemarks.Name = "txtRemarks"
        Me.txtRemarks.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtRemarks.Size = New System.Drawing.Size(876, 80)
        Me.txtRemarks.TabIndex = 7
        '
        'Panel2
        '
        Me.Panel2.BackColor = System.Drawing.Color.FromArgb(CType(CType(236, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(241, Byte), Integer))
        Me.Panel2.Controls.Add(Me.btnClose)
        Me.Panel2.Controls.Add(Me.btnClear)
        Me.Panel2.Controls.Add(Me.btnSave)
        Me.Panel2.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.Panel2.Location = New System.Drawing.Point(0, 625)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(900, 65)
        Me.Panel2.TabIndex = 8
        '
        'btnSave
        '
        Me.btnSave.BackColor = System.Drawing.Color.FromArgb(CType(CType(46, Byte), Integer), CType(CType(204, Byte), Integer), CType(CType(113, Byte), Integer))
        Me.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnSave.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
        Me.btnSave.ForeColor = System.Drawing.Color.White
        Me.btnSave.Location = New System.Drawing.Point(558, 13)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(100, 40)
        Me.btnSave.TabIndex = 0
        Me.btnSave.Text = "Save"
        Me.btnSave.UseVisualStyleBackColor = False
        '
        'btnClear
        '
        Me.btnClear.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnClear.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
        Me.btnClear.ForeColor = System.Drawing.Color.White
        Me.btnClear.Location = New System.Drawing.Point(670, 13)
        Me.btnClear.Name = "btnClear"
        Me.btnClear.Size = New System.Drawing.Size(100, 40)
        Me.btnClear.TabIndex = 1
        Me.btnClear.Text = "Clear"
        Me.btnClear.UseVisualStyleBackColor = False
        '
        'btnClose
        '
        Me.btnClose.BackColor = System.Drawing.Color.FromArgb(CType(CType(149, Byte), Integer), CType(CType(165, Byte), Integer), CType(CType(166, Byte), Integer))
        Me.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnClose.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
        Me.btnClose.ForeColor = System.Drawing.Color.White
        Me.btnClose.Location = New System.Drawing.Point(782, 13)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(100, 40)
        Me.btnClose.TabIndex = 2
        Me.btnClose.Text = "Close"
        Me.btnClose.UseVisualStyleBackColor = False
        '
        'FormManageAttendance
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(900, 690)
        Me.Controls.Add(Me.Panel2)
        Me.Controls.Add(Me.txtRemarks)
        Me.Controls.Add(Me.Label14)
        Me.Controls.Add(Me.GroupBox3)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.pnlTeacherInfo)
        Me.Controls.Add(Me.lblCutoffWarning)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.panelHeader)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormManageAttendance"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Manage Attendance"
        Me.panelHeader.ResumeLayout(False)
        Me.panelHeader.PerformLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.pnlTeacherInfo.ResumeLayout(False)
        Me.pnlTeacherInfo.PerformLayout()
        CType(Me.pbTeacherPhoto, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        Me.Panel2.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents panelHeader As Panel
    Friend WithEvents lblTitle As Label
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents Label2 As Label
    Friend WithEvents dtpDate As DateTimePicker
    Friend WithEvents Label1 As Label
    Friend WithEvents cboTeacher As ComboBox
    Friend WithEvents Label7 As Label
    Friend WithEvents cboDepartment As ComboBox
    Friend WithEvents Label8 As Label
    Friend WithEvents cboShift As ComboBox
    Friend WithEvents lblCutoffWarning As Label
    Friend WithEvents pnlTeacherInfo As Panel
    Friend WithEvents pbTeacherPhoto As PictureBox
    Friend WithEvents Label3 As Label
    Friend WithEvents lblTeacherName As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents lblEmployeeID As Label
    Friend WithEvents Label6 As Label
    Friend WithEvents lblDepartment As Label
    Friend WithEvents Label9 As Label
    Friend WithEvents lblExpectedIn As Label
    Friend WithEvents Label11 As Label
    Friend WithEvents lblExpectedOut As Label
    Friend WithEvents lblExistingRecord As Label
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents rbTimeInOnly As RadioButton
    Friend WithEvents rbTimeOutOnly As RadioButton
    Friend WithEvents rbBoth As RadioButton
    Friend WithEvents GroupBox3 As GroupBox
    Friend WithEvents Label12 As Label
    Friend WithEvents dtpTimeIn As DateTimePicker
    Friend WithEvents chkTimeIn As CheckBox
    Friend WithEvents Label13 As Label
    Friend WithEvents dtpTimeOut As DateTimePicker
    Friend WithEvents chkTimeOut As CheckBox
    Friend WithEvents Label14 As Label
    Friend WithEvents txtRemarks As TextBox
    Friend WithEvents Panel2 As Panel
    Friend WithEvents btnSave As Button
    Friend WithEvents btnClear As Button
    Friend WithEvents btnClose As Button
End Class
