<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ManageUser
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim DataGridViewCellStyle6 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle7 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle10 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle8 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle9 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.panelHeader = New System.Windows.Forms.Panel()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.cbFilter = New System.Windows.Forms.ComboBox()
        Me.txtSearch = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.panelBottom = New System.Windows.Forms.Panel()
        Me.btnChangePassword = New System.Windows.Forms.Button()
        Me.btnDelete = New System.Windows.Forms.Button()
        Me.btnEdit = New System.Windows.Forms.Button()
        Me.btnNew = New System.Windows.Forms.Button()
        Me.dgvManageUser = New System.Windows.Forms.DataGridView()
        Me.login_id = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.full_name = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.username = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.password = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.email = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.location = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.created_at = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.role = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.status = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.isActive = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.EditBtn = New System.Windows.Forms.DataGridViewButtonColumn()
        Me.deleteBtn = New System.Windows.Forms.DataGridViewButtonColumn()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.PictureBox6 = New System.Windows.Forms.PictureBox()
        Me.panelHeader.SuspendLayout()
        Me.panelBottom.SuspendLayout()
        CType(Me.dgvManageUser, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel1.SuspendLayout()
        CType(Me.PictureBox6, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'panelHeader
        '
        Me.panelHeader.BackColor = System.Drawing.Color.White
        Me.panelHeader.Controls.Add(Me.Label2)
        Me.panelHeader.Controls.Add(Me.Label3)
        Me.panelHeader.Controls.Add(Me.cbFilter)
        Me.panelHeader.Controls.Add(Me.txtSearch)
        Me.panelHeader.Dock = System.Windows.Forms.DockStyle.Top
        Me.panelHeader.Location = New System.Drawing.Point(0, 60)
        Me.panelHeader.Name = "panelHeader"
        Me.panelHeader.Padding = New System.Windows.Forms.Padding(10)
        Me.panelHeader.Size = New System.Drawing.Size(1444, 80)
        Me.panelHeader.TabIndex = 4
        '
        'Label2
        '
        Me.Label2.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Segoe UI Semibold", 11.25!, System.Drawing.FontStyle.Bold)
        Me.Label2.ForeColor = System.Drawing.Color.Black
        Me.Label2.Location = New System.Drawing.Point(394, 30)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(43, 20)
        Me.Label2.TabIndex = 34
        Me.Label2.Text = "Role:"
        '
        'Label3
        '
        Me.Label3.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Segoe UI Semibold", 11.25!, System.Drawing.FontStyle.Bold)
        Me.Label3.ForeColor = System.Drawing.Color.Black
        Me.Label3.Location = New System.Drawing.Point(13, 30)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(59, 20)
        Me.Label3.TabIndex = 31
        Me.Label3.Text = "Search:"
        '
        'cbFilter
        '
        Me.cbFilter.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.cbFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbFilter.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbFilter.FormattingEnabled = True
        Me.cbFilter.Items.AddRange(New Object() {"All", "Admin", "Hr"})
        Me.cbFilter.Location = New System.Drawing.Point(443, 26)
        Me.cbFilter.Name = "cbFilter"
        Me.cbFilter.Size = New System.Drawing.Size(177, 29)
        Me.cbFilter.TabIndex = 30
        '
        'txtSearch
        '
        Me.txtSearch.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtSearch.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtSearch.ForeColor = System.Drawing.Color.DimGray
        Me.txtSearch.Location = New System.Drawing.Point(78, 26)
        Me.txtSearch.Name = "txtSearch"
        Me.txtSearch.Size = New System.Drawing.Size(273, 29)
        Me.txtSearch.TabIndex = 26
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.Font = New System.Drawing.Font("Segoe UI", 18.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.ForeColor = System.Drawing.Color.White
        Me.Label1.Location = New System.Drawing.Point(58, 14)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(223, 32)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "User Management"
        '
        'panelBottom
        '
        Me.panelBottom.BackColor = System.Drawing.Color.SteelBlue
        Me.panelBottom.Controls.Add(Me.btnChangePassword)
        Me.panelBottom.Controls.Add(Me.btnDelete)
        Me.panelBottom.Controls.Add(Me.btnEdit)
        Me.panelBottom.Controls.Add(Me.btnNew)
        Me.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.panelBottom.Location = New System.Drawing.Point(0, 658)
        Me.panelBottom.Name = "panelBottom"
        Me.panelBottom.Padding = New System.Windows.Forms.Padding(10, 5, 10, 5)
        Me.panelBottom.Size = New System.Drawing.Size(1444, 69)
        Me.panelBottom.TabIndex = 5
        '
        'btnChangePassword
        '
        Me.btnChangePassword.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.btnChangePassword.AutoSize = True
        Me.btnChangePassword.BackColor = System.Drawing.Color.FromArgb(CType(CType(155, Byte), Integer), CType(CType(89, Byte), Integer), CType(CType(182, Byte), Integer))
        Me.btnChangePassword.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnChangePassword.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnChangePassword.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnChangePassword.ForeColor = System.Drawing.Color.White
        Me.btnChangePassword.Location = New System.Drawing.Point(467, 14)
        Me.btnChangePassword.Name = "btnChangePassword"
        Me.btnChangePassword.Size = New System.Drawing.Size(170, 40)
        Me.btnChangePassword.TabIndex = 35
        Me.btnChangePassword.Text = "&Change Password"
        Me.btnChangePassword.UseVisualStyleBackColor = False
        '
        'btnDelete
        '
        Me.btnDelete.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.btnDelete.AutoSize = True
        Me.btnDelete.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.btnDelete.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnDelete.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnDelete.ForeColor = System.Drawing.Color.White
        Me.btnDelete.Location = New System.Drawing.Point(317, 14)
        Me.btnDelete.Name = "btnDelete"
        Me.btnDelete.Size = New System.Drawing.Size(140, 40)
        Me.btnDelete.TabIndex = 34
        Me.btnDelete.Text = "&Set User"
        Me.btnDelete.UseVisualStyleBackColor = False
        '
        'btnEdit
        '
        Me.btnEdit.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.btnEdit.AutoSize = True
        Me.btnEdit.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(156, Byte), Integer), CType(CType(18, Byte), Integer))
        Me.btnEdit.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnEdit.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnEdit.ForeColor = System.Drawing.Color.White
        Me.btnEdit.Location = New System.Drawing.Point(167, 14)
        Me.btnEdit.Name = "btnEdit"
        Me.btnEdit.Size = New System.Drawing.Size(140, 40)
        Me.btnEdit.TabIndex = 33
        Me.btnEdit.Text = "&Edit User"
        Me.btnEdit.UseVisualStyleBackColor = False
        '
        'btnNew
        '
        Me.btnNew.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.btnNew.AutoSize = True
        Me.btnNew.BackColor = System.Drawing.Color.FromArgb(CType(CType(46, Byte), Integer), CType(CType(204, Byte), Integer), CType(CType(113, Byte), Integer))
        Me.btnNew.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnNew.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnNew.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnNew.ForeColor = System.Drawing.Color.White
        Me.btnNew.Location = New System.Drawing.Point(17, 14)
        Me.btnNew.Name = "btnNew"
        Me.btnNew.Size = New System.Drawing.Size(140, 40)
        Me.btnNew.TabIndex = 16
        Me.btnNew.Text = "&New User"
        Me.btnNew.UseVisualStyleBackColor = False
        '
        'dgvManageUser
        '
        Me.dgvManageUser.AllowUserToAddRows = False
        Me.dgvManageUser.AllowUserToDeleteRows = False
        Me.dgvManageUser.AllowUserToResizeColumns = False
        Me.dgvManageUser.AllowUserToResizeRows = False
        DataGridViewCellStyle6.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        DataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.SteelBlue
        DataGridViewCellStyle6.SelectionForeColor = System.Drawing.Color.White
        Me.dgvManageUser.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle6
        Me.dgvManageUser.BackgroundColor = System.Drawing.Color.White
        Me.dgvManageUser.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.dgvManageUser.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None
        DataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle7.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        DataGridViewCellStyle7.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle7.ForeColor = System.Drawing.Color.White
        DataGridViewCellStyle7.SelectionBackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        DataGridViewCellStyle7.SelectionForeColor = System.Drawing.Color.White
        DataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.dgvManageUser.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle7
        Me.dgvManageUser.ColumnHeadersHeight = 45
        Me.dgvManageUser.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        Me.dgvManageUser.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.login_id, Me.full_name, Me.username, Me.password, Me.email, Me.location, Me.created_at, Me.role, Me.status, Me.isActive, Me.EditBtn, Me.deleteBtn})
        DataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle10.BackColor = System.Drawing.Color.White
        DataGridViewCellStyle10.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle10.ForeColor = System.Drawing.Color.Black
        DataGridViewCellStyle10.SelectionBackColor = System.Drawing.Color.SteelBlue
        DataGridViewCellStyle10.SelectionForeColor = System.Drawing.Color.White
        DataGridViewCellStyle10.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.dgvManageUser.DefaultCellStyle = DataGridViewCellStyle10
        Me.dgvManageUser.Dock = System.Windows.Forms.DockStyle.Fill
        Me.dgvManageUser.EnableHeadersVisualStyles = False
        Me.dgvManageUser.Location = New System.Drawing.Point(0, 140)
        Me.dgvManageUser.MultiSelect = False
        Me.dgvManageUser.Name = "dgvManageUser"
        Me.dgvManageUser.ReadOnly = True
        Me.dgvManageUser.RowHeadersVisible = False
        Me.dgvManageUser.RowHeadersWidth = 51
        Me.dgvManageUser.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgvManageUser.Size = New System.Drawing.Size(1444, 518)
        Me.dgvManageUser.TabIndex = 21
        Me.dgvManageUser.TabStop = False
        '
        'login_id
        '
        Me.login_id.DataPropertyName = "login_id"
        Me.login_id.HeaderText = "ID"
        Me.login_id.Name = "login_id"
        Me.login_id.ReadOnly = True
        Me.login_id.Visible = False
        '
        'full_name
        '
        Me.full_name.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.full_name.DataPropertyName = "full_name"
        Me.full_name.HeaderText = "FULL NAME"
        Me.full_name.Name = "full_name"
        Me.full_name.ReadOnly = True
        '
        'username
        '
        Me.username.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.username.DataPropertyName = "username"
        Me.username.HeaderText = "USERNAME"
        Me.username.Name = "username"
        Me.username.ReadOnly = True
        '
        'password
        '
        Me.password.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.password.DataPropertyName = "password"
        Me.password.HeaderText = "PASSWORD"
        Me.password.Name = "password"
        Me.password.ReadOnly = True
        Me.password.Visible = False
        '
        'email
        '
        Me.email.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.email.DataPropertyName = "email"
        Me.email.HeaderText = "EMAIL"
        Me.email.Name = "email"
        Me.email.ReadOnly = True
        '
        'location
        '
        Me.location.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.location.DataPropertyName = "address"
        Me.location.HeaderText = "LOCATION"
        Me.location.Name = "location"
        Me.location.ReadOnly = True
        '
        'created_at
        '
        Me.created_at.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.created_at.DataPropertyName = "created_at"
        Me.created_at.HeaderText = "CREATED"
        Me.created_at.Name = "created_at"
        Me.created_at.ReadOnly = True
        '
        'role
        '
        Me.role.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells
        Me.role.DataPropertyName = "role"
        Me.role.HeaderText = "ROLE"
        Me.role.Name = "role"
        Me.role.ReadOnly = True
        Me.role.Width = 71
        '
        'status
        '
        Me.status.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells
        Me.status.DataPropertyName = "status"
        Me.status.HeaderText = "STATUS"
        Me.status.Name = "status"
        Me.status.ReadOnly = True
        Me.status.Width = 86
        '
        'isActive
        '
        Me.isActive.DataPropertyName = "isActive"
        Me.isActive.HeaderText = "isACTIVE"
        Me.isActive.Name = "isActive"
        Me.isActive.ReadOnly = True
        Me.isActive.Visible = False
        '
        'EditBtn
        '
        DataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle8.BackColor = System.Drawing.Color.RoyalBlue
        DataGridViewCellStyle8.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle8.ForeColor = System.Drawing.Color.White
        DataGridViewCellStyle8.SelectionBackColor = System.Drawing.Color.RoyalBlue
        DataGridViewCellStyle8.SelectionForeColor = System.Drawing.Color.White
        Me.EditBtn.DefaultCellStyle = DataGridViewCellStyle8
        Me.EditBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.EditBtn.HeaderText = "ACTIONS"
        Me.EditBtn.Name = "EditBtn"
        Me.EditBtn.ReadOnly = True
        Me.EditBtn.Text = "Edit"
        Me.EditBtn.UseColumnTextForButtonValue = True
        Me.EditBtn.Visible = False
        '
        'deleteBtn
        '
        DataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle9.BackColor = System.Drawing.Color.Crimson
        DataGridViewCellStyle9.Font = New System.Drawing.Font("Segoe UI", 9.75!)
        DataGridViewCellStyle9.ForeColor = System.Drawing.Color.White
        DataGridViewCellStyle9.SelectionBackColor = System.Drawing.Color.Crimson
        DataGridViewCellStyle9.SelectionForeColor = System.Drawing.Color.White
        Me.deleteBtn.DefaultCellStyle = DataGridViewCellStyle9
        Me.deleteBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.deleteBtn.HeaderText = ""
        Me.deleteBtn.Name = "deleteBtn"
        Me.deleteBtn.ReadOnly = True
        Me.deleteBtn.Text = "Delete"
        Me.deleteBtn.UseColumnTextForButtonValue = True
        Me.deleteBtn.Visible = False
        '
        'Panel1
        '
        Me.Panel1.BackColor = System.Drawing.Color.SteelBlue
        Me.Panel1.Controls.Add(Me.PictureBox6)
        Me.Panel1.Controls.Add(Me.Label1)
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Top
        Me.Panel1.Location = New System.Drawing.Point(0, 0)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(1444, 60)
        Me.Panel1.TabIndex = 22
        '
        'PictureBox6
        '
        Me.PictureBox6.BackColor = System.Drawing.Color.Transparent
        Me.PictureBox6.Image = Global.Tala_Attendance_Management_System.My.Resources.Resources.icons8_manage_50
        Me.PictureBox6.Location = New System.Drawing.Point(12, 12)
        Me.PictureBox6.Name = "PictureBox6"
        Me.PictureBox6.Size = New System.Drawing.Size(40, 40)
        Me.PictureBox6.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.PictureBox6.TabIndex = 16
        Me.PictureBox6.TabStop = False
        '
        'ManageUser
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(9.0!, 21.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(1444, 727)
        Me.Controls.Add(Me.dgvManageUser)
        Me.Controls.Add(Me.panelHeader)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.panelBottom)
        Me.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.Name = "ManageUser"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "ManageUser"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.panelHeader.ResumeLayout(False)
        Me.panelHeader.PerformLayout()
        Me.panelBottom.ResumeLayout(False)
        Me.panelBottom.PerformLayout()
        CType(Me.dgvManageUser, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        CType(Me.PictureBox6, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents panelHeader As Panel
    Friend WithEvents panelBottom As Panel
    Public WithEvents dgvManageUser As DataGridView
    Friend WithEvents Label1 As Label
    Friend WithEvents txtSearch As TextBox
    Friend WithEvents Label3 As Label
    Friend WithEvents cbFilter As ComboBox
    Friend WithEvents btnDelete As Button
    Friend WithEvents btnEdit As Button
    Friend WithEvents Panel1 As Panel
    Friend WithEvents PictureBox6 As PictureBox
    Friend WithEvents btnNew As Button
    Friend WithEvents Label2 As Label
    Friend WithEvents btnChangePassword As Button
    Friend WithEvents login_id As DataGridViewTextBoxColumn
    Friend WithEvents full_name As DataGridViewTextBoxColumn
    Friend WithEvents username As DataGridViewTextBoxColumn
    Friend WithEvents password As DataGridViewTextBoxColumn
    Friend WithEvents email As DataGridViewTextBoxColumn
    Friend WithEvents location As DataGridViewTextBoxColumn
    Friend WithEvents created_at As DataGridViewTextBoxColumn
    Friend WithEvents role As DataGridViewTextBoxColumn
    Friend WithEvents status As DataGridViewTextBoxColumn
    Friend WithEvents isActive As DataGridViewTextBoxColumn
    Friend WithEvents EditBtn As DataGridViewButtonColumn
    Friend WithEvents deleteBtn As DataGridViewButtonColumn
End Class