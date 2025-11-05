<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormBackup
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
        Me.components = New System.ComponentModel.Container()
        Me.grpBackup = New System.Windows.Forms.GroupBox()
        Me.btnCreateBackup = New System.Windows.Forms.Button()
        Me.chkIncludeDatabase = New System.Windows.Forms.CheckBox()
        Me.btnBrowseBackup = New System.Windows.Forms.Button()
        Me.txtBackupPath = New System.Windows.Forms.TextBox()
        Me.lblBackupDest = New System.Windows.Forms.Label()
        Me.grpRestore = New System.Windows.Forms.GroupBox()
        Me.btnRestore = New System.Windows.Forms.Button()
        Me.chkRestoreDatabase = New System.Windows.Forms.CheckBox()
        Me.btnBrowseRestore = New System.Windows.Forms.Button()
        Me.txtRestorePath = New System.Windows.Forms.TextBox()
        Me.lblRestoreSource = New System.Windows.Forms.Label()
        Me.progressBar = New System.Windows.Forms.ProgressBar()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.lstLog = New System.Windows.Forms.ListBox()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.grpBackup.SuspendLayout()
        Me.grpRestore.SuspendLayout()
        Me.SuspendLayout()
        '
        'grpBackup
        '
        Me.grpBackup.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grpBackup.Controls.Add(Me.btnCreateBackup)
        Me.grpBackup.Controls.Add(Me.chkIncludeDatabase)
        Me.grpBackup.Controls.Add(Me.btnBrowseBackup)
        Me.grpBackup.Controls.Add(Me.txtBackupPath)
        Me.grpBackup.Controls.Add(Me.lblBackupDest)
        Me.grpBackup.Font = New System.Drawing.Font("Segoe UI", 14.0!, System.Drawing.FontStyle.Bold)
        Me.grpBackup.Location = New System.Drawing.Point(10, 10)
        Me.grpBackup.Name = "grpBackup"
        Me.grpBackup.Size = New System.Drawing.Size(476, 147)
        Me.grpBackup.TabIndex = 0
        Me.grpBackup.TabStop = False
        Me.grpBackup.Text = "Create Backup"
        '
        'btnCreateBackup
        '
        Me.btnCreateBackup.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnCreateBackup.BackColor = System.Drawing.Color.FromArgb(CType(CType(46, Byte), Integer), CType(CType(204, Byte), Integer), CType(CType(113, Byte), Integer))
        Me.btnCreateBackup.FlatAppearance.BorderSize = 0
        Me.btnCreateBackup.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnCreateBackup.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
        Me.btnCreateBackup.ForeColor = System.Drawing.Color.White
        Me.btnCreateBackup.Location = New System.Drawing.Point(350, 112)
        Me.btnCreateBackup.Name = "btnCreateBackup"
        Me.btnCreateBackup.Size = New System.Drawing.Size(120, 29)
        Me.btnCreateBackup.TabIndex = 8
        Me.btnCreateBackup.Text = "Create Backup"
        Me.btnCreateBackup.UseVisualStyleBackColor = False
        '
        'chkIncludeDatabase
        '
        Me.chkIncludeDatabase.AutoSize = True
        Me.chkIncludeDatabase.Checked = True
        Me.chkIncludeDatabase.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkIncludeDatabase.Enabled = False
        Me.chkIncludeDatabase.Location = New System.Drawing.Point(9, 112)
        Me.chkIncludeDatabase.Name = "chkIncludeDatabase"
        Me.chkIncludeDatabase.Size = New System.Drawing.Size(230, 29)
        Me.chkIncludeDatabase.TabIndex = 3
        Me.chkIncludeDatabase.Text = "Database (tables/data)"
        Me.chkIncludeDatabase.UseVisualStyleBackColor = True
        '
        'btnBrowseBackup
        '
        Me.btnBrowseBackup.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnBrowseBackup.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.btnBrowseBackup.FlatAppearance.BorderSize = 0
        Me.btnBrowseBackup.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnBrowseBackup.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
        Me.btnBrowseBackup.ForeColor = System.Drawing.Color.White
        Me.btnBrowseBackup.Location = New System.Drawing.Point(425, 46)
        Me.btnBrowseBackup.Name = "btnBrowseBackup"
        Me.btnBrowseBackup.Size = New System.Drawing.Size(45, 32)
        Me.btnBrowseBackup.TabIndex = 2
        Me.btnBrowseBackup.Text = "..."
        Me.ToolTip1.SetToolTip(Me.btnBrowseBackup, "Choose backup destination (.zip)")
        Me.btnBrowseBackup.UseVisualStyleBackColor = False
        '
        'txtBackupPath
        '
        Me.txtBackupPath.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtBackupPath.Location = New System.Drawing.Point(9, 46)
        Me.txtBackupPath.Name = "txtBackupPath"
        Me.txtBackupPath.Size = New System.Drawing.Size(410, 32)
        Me.txtBackupPath.TabIndex = 1
        '
        'lblBackupDest
        '
        Me.lblBackupDest.AutoSize = True
        Me.lblBackupDest.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.lblBackupDest.ForeColor = System.Drawing.Color.Black
        Me.lblBackupDest.Location = New System.Drawing.Point(6, 27)
        Me.lblBackupDest.Name = "lblBackupDest"
        Me.lblBackupDest.Size = New System.Drawing.Size(225, 19)
        Me.lblBackupDest.TabIndex = 0
        Me.lblBackupDest.Text = "Backup destination file (ZIP archive)"
        '
        'grpRestore
        '
        Me.grpRestore.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grpRestore.Controls.Add(Me.btnRestore)
        Me.grpRestore.Controls.Add(Me.chkRestoreDatabase)
        Me.grpRestore.Controls.Add(Me.btnBrowseRestore)
        Me.grpRestore.Controls.Add(Me.txtRestorePath)
        Me.grpRestore.Controls.Add(Me.lblRestoreSource)
        Me.grpRestore.Font = New System.Drawing.Font("Segoe UI", 14.0!, System.Drawing.FontStyle.Bold)
        Me.grpRestore.Location = New System.Drawing.Point(10, 163)
        Me.grpRestore.Name = "grpRestore"
        Me.grpRestore.Size = New System.Drawing.Size(476, 147)
        Me.grpRestore.TabIndex = 1
        Me.grpRestore.TabStop = False
        Me.grpRestore.Text = "Restore Backup"
        '
        'btnRestore
        '
        Me.btnRestore.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnRestore.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.btnRestore.FlatAppearance.BorderSize = 0
        Me.btnRestore.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnRestore.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
        Me.btnRestore.ForeColor = System.Drawing.Color.White
        Me.btnRestore.Location = New System.Drawing.Point(350, 114)
        Me.btnRestore.Name = "btnRestore"
        Me.btnRestore.Size = New System.Drawing.Size(120, 29)
        Me.btnRestore.TabIndex = 8
        Me.btnRestore.Text = "Restore"
        Me.btnRestore.UseVisualStyleBackColor = False
        '
        'chkRestoreDatabase
        '
        Me.chkRestoreDatabase.AutoSize = True
        Me.chkRestoreDatabase.Checked = True
        Me.chkRestoreDatabase.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkRestoreDatabase.Enabled = False
        Me.chkRestoreDatabase.Location = New System.Drawing.Point(9, 114)
        Me.chkRestoreDatabase.Name = "chkRestoreDatabase"
        Me.chkRestoreDatabase.Size = New System.Drawing.Size(230, 29)
        Me.chkRestoreDatabase.TabIndex = 4
        Me.chkRestoreDatabase.Text = "Database (tables/data)"
        Me.chkRestoreDatabase.UseVisualStyleBackColor = True
        '
        'btnBrowseRestore
        '
        Me.btnBrowseRestore.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnBrowseRestore.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.btnBrowseRestore.FlatAppearance.BorderSize = 0
        Me.btnBrowseRestore.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnBrowseRestore.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
        Me.btnBrowseRestore.ForeColor = System.Drawing.Color.White
        Me.btnBrowseRestore.Location = New System.Drawing.Point(425, 46)
        Me.btnBrowseRestore.Name = "btnBrowseRestore"
        Me.btnBrowseRestore.Size = New System.Drawing.Size(45, 32)
        Me.btnBrowseRestore.TabIndex = 3
        Me.btnBrowseRestore.Text = "..."
        Me.ToolTip1.SetToolTip(Me.btnBrowseRestore, "Select backup ZIP to restore")
        Me.btnBrowseRestore.UseVisualStyleBackColor = False
        '
        'txtRestorePath
        '
        Me.txtRestorePath.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtRestorePath.Location = New System.Drawing.Point(9, 46)
        Me.txtRestorePath.Name = "txtRestorePath"
        Me.txtRestorePath.Size = New System.Drawing.Size(410, 32)
        Me.txtRestorePath.TabIndex = 2
        '
        'lblRestoreSource
        '
        Me.lblRestoreSource.AutoSize = True
        Me.lblRestoreSource.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.lblRestoreSource.ForeColor = System.Drawing.Color.Black
        Me.lblRestoreSource.Location = New System.Drawing.Point(6, 27)
        Me.lblRestoreSource.Name = "lblRestoreSource"
        Me.lblRestoreSource.Size = New System.Drawing.Size(153, 19)
        Me.lblRestoreSource.TabIndex = 0
        Me.lblRestoreSource.Text = "Backup file (ZIP archive)"
        '
        'progressBar
        '
        Me.progressBar.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.progressBar.Location = New System.Drawing.Point(10, 468)
        Me.progressBar.Name = "progressBar"
        Me.progressBar.Size = New System.Drawing.Size(476, 15)
        Me.progressBar.TabIndex = 2
        '
        'lblStatus
        '
        Me.lblStatus.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblStatus.Location = New System.Drawing.Point(10, 445)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(476, 17)
        Me.lblStatus.TabIndex = 3
        Me.lblStatus.Text = "Ready"
        '
        'lstLog
        '
        Me.lstLog.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lstLog.FormattingEnabled = True
        Me.lstLog.Location = New System.Drawing.Point(10, 315)
        Me.lstLog.Name = "lstLog"
        Me.lstLog.Size = New System.Drawing.Size(477, 121)
        Me.lstLog.TabIndex = 4
        '
        'FormBackup
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(497, 493)
        Me.Controls.Add(Me.lstLog)
        Me.Controls.Add(Me.lblStatus)
        Me.Controls.Add(Me.progressBar)
        Me.Controls.Add(Me.grpRestore)
        Me.Controls.Add(Me.grpBackup)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormBackup"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "System Backup and Restore"
        Me.grpBackup.ResumeLayout(False)
        Me.grpBackup.PerformLayout()
        Me.grpRestore.ResumeLayout(False)
        Me.grpRestore.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents grpBackup As System.Windows.Forms.GroupBox
    Friend WithEvents btnCreateBackup As System.Windows.Forms.Button
    Friend WithEvents chkIncludeDatabase As System.Windows.Forms.CheckBox
    Friend WithEvents btnBrowseBackup As System.Windows.Forms.Button
    Friend WithEvents txtBackupPath As System.Windows.Forms.TextBox
    Friend WithEvents lblBackupDest As System.Windows.Forms.Label
    Friend WithEvents grpRestore As System.Windows.Forms.GroupBox
    Friend WithEvents btnRestore As System.Windows.Forms.Button
    Friend WithEvents chkRestoreDatabase As System.Windows.Forms.CheckBox
    Friend WithEvents btnBrowseRestore As System.Windows.Forms.Button
    Friend WithEvents txtRestorePath As System.Windows.Forms.TextBox
    Friend WithEvents lblRestoreSource As System.Windows.Forms.Label
    Friend WithEvents progressBar As System.Windows.Forms.ProgressBar
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents lstLog As System.Windows.Forms.ListBox
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
End Class
