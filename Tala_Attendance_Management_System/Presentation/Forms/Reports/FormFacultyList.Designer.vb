<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormFacultyList
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
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle3 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.pnlTop = New System.Windows.Forms.Panel()
        Me.lblTitle = New System.Windows.Forms.Label()
        Me.pnlFilters = New System.Windows.Forms.Panel()
        Me.btnGenerateReport = New System.Windows.Forms.Button()
        Me.btnExport = New System.Windows.Forms.Button()
        Me.btnRefresh = New System.Windows.Forms.Button()
        Me.txtSearch = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.cboStatusFilter = New System.Windows.Forms.ComboBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.dgvFaculty = New System.Windows.Forms.DataGridView()
        Me.colTeacherID = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colEmployeeID = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colFullName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colDepartment = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colEmail = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colContactNumber = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colStatus = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.pnlBottom = New System.Windows.Forms.Panel()
        Me.btnLastPage = New System.Windows.Forms.Button()
        Me.btnNextPage = New System.Windows.Forms.Button()
        Me.lblPageInfo = New System.Windows.Forms.Label()
        Me.btnPrevPage = New System.Windows.Forms.Button()
        Me.btnFirstPage = New System.Windows.Forms.Button()
        Me.lblRecordCount = New System.Windows.Forms.Label()
        Me.cboDepartmentFilter = New System.Windows.Forms.ComboBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.pnlTop.SuspendLayout()
        Me.pnlFilters.SuspendLayout()
        CType(Me.dgvFaculty, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlBottom.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlTop
        '
        Me.pnlTop.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(73, Byte), Integer), CType(CType(94, Byte), Integer))
        Me.pnlTop.Controls.Add(Me.lblTitle)
        Me.pnlTop.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlTop.Location = New System.Drawing.Point(0, 0)
        Me.pnlTop.Name = "pnlTop"
        Me.pnlTop.Size = New System.Drawing.Size(1235, 60)
        Me.pnlTop.TabIndex = 0
        '
        'lblTitle
        '
        Me.lblTitle.BackColor = System.Drawing.Color.SteelBlue
        Me.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblTitle.Font = New System.Drawing.Font("Segoe UI", 18.0!, System.Drawing.FontStyle.Bold)
        Me.lblTitle.ForeColor = System.Drawing.Color.White
        Me.lblTitle.Location = New System.Drawing.Point(0, 0)
        Me.lblTitle.Name = "lblTitle"
        Me.lblTitle.Padding = New System.Windows.Forms.Padding(20, 0, 0, 0)
        Me.lblTitle.Size = New System.Drawing.Size(1235, 60)
        Me.lblTitle.TabIndex = 0
        Me.lblTitle.Text = "Faculty List"
        Me.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'pnlFilters
        '
        Me.pnlFilters.BackColor = System.Drawing.Color.White
        Me.pnlFilters.Controls.Add(Me.cboDepartmentFilter)
        Me.pnlFilters.Controls.Add(Me.Label3)
        Me.pnlFilters.Controls.Add(Me.btnGenerateReport)
        Me.pnlFilters.Controls.Add(Me.btnExport)
        Me.pnlFilters.Controls.Add(Me.btnRefresh)
        Me.pnlFilters.Controls.Add(Me.txtSearch)
        Me.pnlFilters.Controls.Add(Me.Label2)
        Me.pnlFilters.Controls.Add(Me.cboStatusFilter)
        Me.pnlFilters.Controls.Add(Me.Label1)
        Me.pnlFilters.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlFilters.Location = New System.Drawing.Point(0, 60)
        Me.pnlFilters.Name = "pnlFilters"
        Me.pnlFilters.Padding = New System.Windows.Forms.Padding(20, 10, 20, 10)
        Me.pnlFilters.Size = New System.Drawing.Size(1235, 70)
        Me.pnlFilters.TabIndex = 1
        '
        'btnGenerateReport
        '
        Me.btnGenerateReport.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.btnGenerateReport.BackColor = System.Drawing.Color.FromArgb(CType(CType(155, Byte), Integer), CType(CType(89, Byte), Integer), CType(CType(182, Byte), Integer))
        Me.btnGenerateReport.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnGenerateReport.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
        Me.btnGenerateReport.ForeColor = System.Drawing.Color.White
        Me.btnGenerateReport.Location = New System.Drawing.Point(1087, 20)
        Me.btnGenerateReport.Name = "btnGenerateReport"
        Me.btnGenerateReport.Size = New System.Drawing.Size(140, 30)
        Me.btnGenerateReport.TabIndex = 6
        Me.btnGenerateReport.Text = "Generate Report"
        Me.btnGenerateReport.UseVisualStyleBackColor = False
        '
        'btnExport
        '
        Me.btnExport.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.btnExport.BackColor = System.Drawing.Color.FromArgb(CType(CType(39, Byte), Integer), CType(CType(174, Byte), Integer), CType(CType(96, Byte), Integer))
        Me.btnExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnExport.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.btnExport.ForeColor = System.Drawing.Color.White
        Me.btnExport.Location = New System.Drawing.Point(951, 20)
        Me.btnExport.Name = "btnExport"
        Me.btnExport.Size = New System.Drawing.Size(130, 30)
        Me.btnExport.TabIndex = 5
        Me.btnExport.Text = "Export to Excel"
        Me.btnExport.UseVisualStyleBackColor = False
        '
        'btnRefresh
        '
        Me.btnRefresh.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.btnRefresh.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnRefresh.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.btnRefresh.ForeColor = System.Drawing.Color.White
        Me.btnRefresh.Location = New System.Drawing.Point(845, 20)
        Me.btnRefresh.Name = "btnRefresh"
        Me.btnRefresh.Size = New System.Drawing.Size(100, 30)
        Me.btnRefresh.TabIndex = 4
        Me.btnRefresh.Text = "Refresh"
        Me.btnRefresh.UseVisualStyleBackColor = False
        '
        'txtSearch
        '
        Me.txtSearch.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.txtSearch.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.txtSearch.Location = New System.Drawing.Point(85, 23)
        Me.txtSearch.Name = "txtSearch"
        Me.txtSearch.Size = New System.Drawing.Size(206, 25)
        Me.txtSearch.TabIndex = 3
        '
        'Label2
        '
        Me.Label2.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Segoe UI Semibold", 11.25!, System.Drawing.FontStyle.Bold)
        Me.Label2.Location = New System.Drawing.Point(20, 25)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(59, 20)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "Search:"
        '
        'cboStatusFilter
        '
        Me.cboStatusFilter.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.cboStatusFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboStatusFilter.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.cboStatusFilter.FormattingEnabled = True
        Me.cboStatusFilter.Location = New System.Drawing.Point(650, 23)
        Me.cboStatusFilter.Name = "cboStatusFilter"
        Me.cboStatusFilter.Size = New System.Drawing.Size(150, 25)
        Me.cboStatusFilter.TabIndex = 1
        '
        'Label1
        '
        Me.Label1.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Segoe UI Semibold", 11.25!, System.Drawing.FontStyle.Bold)
        Me.Label1.Location = New System.Drawing.Point(590, 25)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(54, 20)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Status:"
        '
        'dgvFaculty
        '
        Me.dgvFaculty.AllowUserToAddRows = False
        Me.dgvFaculty.AllowUserToDeleteRows = False
        Me.dgvFaculty.AllowUserToResizeColumns = False
        Me.dgvFaculty.AllowUserToResizeRows = False
        DataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer))
        DataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.SteelBlue
        DataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.White
        Me.dgvFaculty.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle1
        Me.dgvFaculty.BackgroundColor = System.Drawing.Color.White
        Me.dgvFaculty.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.dgvFaculty.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None
        DataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        DataGridViewCellStyle2.Font = New System.Drawing.Font("Segoe UI Semibold", 11.0!, System.Drawing.FontStyle.Bold)
        DataGridViewCellStyle2.ForeColor = System.Drawing.Color.White
        DataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        DataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White
        DataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.dgvFaculty.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle2
        Me.dgvFaculty.ColumnHeadersHeight = 45
        Me.dgvFaculty.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        Me.dgvFaculty.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colTeacherID, Me.colEmployeeID, Me.colFullName, Me.colDepartment, Me.colEmail, Me.colContactNumber, Me.colStatus})
        DataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle3.BackColor = System.Drawing.Color.White
        DataGridViewCellStyle3.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        DataGridViewCellStyle3.ForeColor = System.Drawing.Color.Black
        DataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.SteelBlue
        DataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White
        DataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.dgvFaculty.DefaultCellStyle = DataGridViewCellStyle3
        Me.dgvFaculty.Dock = System.Windows.Forms.DockStyle.Fill
        Me.dgvFaculty.EnableHeadersVisualStyles = False
        Me.dgvFaculty.Location = New System.Drawing.Point(0, 130)
        Me.dgvFaculty.MultiSelect = False
        Me.dgvFaculty.Name = "dgvFaculty"
        Me.dgvFaculty.ReadOnly = True
        Me.dgvFaculty.RowHeadersVisible = False
        Me.dgvFaculty.RowTemplate.Height = 40
        Me.dgvFaculty.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgvFaculty.Size = New System.Drawing.Size(1235, 470)
        Me.dgvFaculty.TabIndex = 2
        '
        'colTeacherID
        '
        Me.colTeacherID.DataPropertyName = "teacherID"
        Me.colTeacherID.HeaderText = "ID"
        Me.colTeacherID.Name = "colTeacherID"
        Me.colTeacherID.ReadOnly = True
        Me.colTeacherID.Visible = False
        '
        'colEmployeeID
        '
        Me.colEmployeeID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colEmployeeID.DataPropertyName = "employeeID"
        Me.colEmployeeID.FillWeight = 12.0!
        Me.colEmployeeID.HeaderText = "EMPLOYEE ID"
        Me.colEmployeeID.Name = "colEmployeeID"
        Me.colEmployeeID.ReadOnly = True
        '
        'colFullName
        '
        Me.colFullName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colFullName.DataPropertyName = "fullname"
        Me.colFullName.FillWeight = 25.0!
        Me.colFullName.HeaderText = "FULL NAME"
        Me.colFullName.Name = "colFullName"
        Me.colFullName.ReadOnly = True
        '
        'colDepartment
        '
        Me.colDepartment.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colDepartment.DataPropertyName = "department"
        Me.colDepartment.FillWeight = 15.0!
        Me.colDepartment.HeaderText = "DEPARTMENT"
        Me.colDepartment.Name = "colDepartment"
        Me.colDepartment.ReadOnly = True
        '
        'colEmail
        '
        Me.colEmail.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colEmail.DataPropertyName = "email"
        Me.colEmail.FillWeight = 20.0!
        Me.colEmail.HeaderText = "EMAIL"
        Me.colEmail.Name = "colEmail"
        Me.colEmail.ReadOnly = True
        '
        'colContactNumber
        '
        Me.colContactNumber.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colContactNumber.DataPropertyName = "contactNo"
        Me.colContactNumber.FillWeight = 13.0!
        Me.colContactNumber.HeaderText = "CONTACT NO"
        Me.colContactNumber.Name = "colContactNumber"
        Me.colContactNumber.ReadOnly = True
        '
        'colStatus
        '
        Me.colStatus.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colStatus.DataPropertyName = "status"
        Me.colStatus.FillWeight = 10.0!
        Me.colStatus.HeaderText = "STATUS"
        Me.colStatus.Name = "colStatus"
        Me.colStatus.ReadOnly = True
        '
        'pnlBottom
        '
        Me.pnlBottom.BackColor = System.Drawing.Color.WhiteSmoke
        Me.pnlBottom.Controls.Add(Me.btnLastPage)
        Me.pnlBottom.Controls.Add(Me.btnNextPage)
        Me.pnlBottom.Controls.Add(Me.lblPageInfo)
        Me.pnlBottom.Controls.Add(Me.btnPrevPage)
        Me.pnlBottom.Controls.Add(Me.btnFirstPage)
        Me.pnlBottom.Controls.Add(Me.lblRecordCount)
        Me.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlBottom.Location = New System.Drawing.Point(0, 600)
        Me.pnlBottom.Name = "pnlBottom"
        Me.pnlBottom.Size = New System.Drawing.Size(1235, 50)
        Me.pnlBottom.TabIndex = 3
        '
        'btnLastPage
        '
        Me.btnLastPage.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.btnLastPage.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.btnLastPage.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnLastPage.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.btnLastPage.ForeColor = System.Drawing.Color.White
        Me.btnLastPage.Location = New System.Drawing.Point(1167, 10)
        Me.btnLastPage.Name = "btnLastPage"
        Me.btnLastPage.Size = New System.Drawing.Size(60, 30)
        Me.btnLastPage.TabIndex = 5
        Me.btnLastPage.Text = ">>"
        Me.btnLastPage.UseVisualStyleBackColor = False
        '
        'btnNextPage
        '
        Me.btnNextPage.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.btnNextPage.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.btnNextPage.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnNextPage.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.btnNextPage.ForeColor = System.Drawing.Color.White
        Me.btnNextPage.Location = New System.Drawing.Point(1101, 10)
        Me.btnNextPage.Name = "btnNextPage"
        Me.btnNextPage.Size = New System.Drawing.Size(60, 30)
        Me.btnNextPage.TabIndex = 4
        Me.btnNextPage.Text = ">"
        Me.btnNextPage.UseVisualStyleBackColor = False
        '
        'lblPageInfo
        '
        Me.lblPageInfo.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.lblPageInfo.Font = New System.Drawing.Font("Segoe UI Semibold", 10.0!, System.Drawing.FontStyle.Bold)
        Me.lblPageInfo.Location = New System.Drawing.Point(995, 15)
        Me.lblPageInfo.Name = "lblPageInfo"
        Me.lblPageInfo.Size = New System.Drawing.Size(100, 20)
        Me.lblPageInfo.TabIndex = 3
        Me.lblPageInfo.Text = "Page 1 / 1"
        Me.lblPageInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'btnPrevPage
        '
        Me.btnPrevPage.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.btnPrevPage.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.btnPrevPage.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnPrevPage.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.btnPrevPage.ForeColor = System.Drawing.Color.White
        Me.btnPrevPage.Location = New System.Drawing.Point(929, 10)
        Me.btnPrevPage.Name = "btnPrevPage"
        Me.btnPrevPage.Size = New System.Drawing.Size(60, 30)
        Me.btnPrevPage.TabIndex = 2
        Me.btnPrevPage.Text = "<"
        Me.btnPrevPage.UseVisualStyleBackColor = False
        '
        'btnFirstPage
        '
        Me.btnFirstPage.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.btnFirstPage.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.btnFirstPage.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnFirstPage.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.btnFirstPage.ForeColor = System.Drawing.Color.White
        Me.btnFirstPage.Location = New System.Drawing.Point(863, 10)
        Me.btnFirstPage.Name = "btnFirstPage"
        Me.btnFirstPage.Size = New System.Drawing.Size(60, 30)
        Me.btnFirstPage.TabIndex = 1
        Me.btnFirstPage.Text = "<<"
        Me.btnFirstPage.UseVisualStyleBackColor = False
        '
        'lblRecordCount
        '
        Me.lblRecordCount.AutoSize = True
        Me.lblRecordCount.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.lblRecordCount.Location = New System.Drawing.Point(20, 15)
        Me.lblRecordCount.Name = "lblRecordCount"
        Me.lblRecordCount.Size = New System.Drawing.Size(105, 19)
        Me.lblRecordCount.TabIndex = 0
        Me.lblRecordCount.Text = "Total Records: 0"
        '
        'cboDepartmentFilter
        '
        Me.cboDepartmentFilter.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.cboDepartmentFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboDepartmentFilter.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.cboDepartmentFilter.FormattingEnabled = True
        Me.cboDepartmentFilter.Location = New System.Drawing.Point(413, 23)
        Me.cboDepartmentFilter.Name = "cboDepartmentFilter"
        Me.cboDepartmentFilter.Size = New System.Drawing.Size(150, 25)
        Me.cboDepartmentFilter.TabIndex = 8
        '
        'Label3
        '
        Me.Label3.Anchor = System.Windows.Forms.AnchorStyles.Left
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Segoe UI Semibold", 11.25!, System.Drawing.FontStyle.Bold)
        Me.Label3.Location = New System.Drawing.Point(312, 25)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(95, 20)
        Me.Label3.TabIndex = 7
        Me.Label3.Text = "Department:"
        '
        'FormFacultyList
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1235, 650)
        Me.Controls.Add(Me.dgvFaculty)
        Me.Controls.Add(Me.pnlBottom)
        Me.Controls.Add(Me.pnlFilters)
        Me.Controls.Add(Me.pnlTop)
        Me.Name = "FormFacultyList"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Faculty List"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.pnlTop.ResumeLayout(False)
        Me.pnlFilters.ResumeLayout(False)
        Me.pnlFilters.PerformLayout()
        CType(Me.dgvFaculty, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlBottom.ResumeLayout(False)
        Me.pnlBottom.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents pnlTop As Panel
    Friend WithEvents lblTitle As Label
    Friend WithEvents pnlFilters As Panel
    Friend WithEvents Label1 As Label
    Friend WithEvents cboStatusFilter As ComboBox
    Friend WithEvents Label2 As Label
    Friend WithEvents txtSearch As TextBox
    Friend WithEvents btnRefresh As Button
    Friend WithEvents btnExport As Button
    Friend WithEvents dgvFaculty As DataGridView
    Friend WithEvents pnlBottom As Panel
    Friend WithEvents lblRecordCount As Label
    Friend WithEvents btnFirstPage As Button
    Friend WithEvents btnPrevPage As Button
    Friend WithEvents lblPageInfo As Label
    Friend WithEvents btnNextPage As Button
    Friend WithEvents btnLastPage As Button
    Friend WithEvents btnGenerateReport As Button
    Friend WithEvents colTeacherID As DataGridViewTextBoxColumn
    Friend WithEvents colEmployeeID As DataGridViewTextBoxColumn
    Friend WithEvents colFullName As DataGridViewTextBoxColumn
    Friend WithEvents colDepartment As DataGridViewTextBoxColumn
    Friend WithEvents colEmail As DataGridViewTextBoxColumn
    Friend WithEvents colContactNumber As DataGridViewTextBoxColumn
    Friend WithEvents colStatus As DataGridViewTextBoxColumn
    Friend WithEvents cboDepartmentFilter As ComboBox
    Friend WithEvents Label3 As Label
End Class
