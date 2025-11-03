Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' Helper class para sa consistent form design sa buong Tala AMS system
''' Based sa form-design-standards.md
''' </summary>
Public Class FormDesignHelper

#Region "Color Constants"
    ' Button Colors
    Public Shared ReadOnly PrimaryBlue As Color = Color.FromArgb(52, 152, 219)
    Public Shared ReadOnly SuccessGreen As Color = Color.FromArgb(46, 204, 113)
    Public Shared ReadOnly Purple As Color = Color.FromArgb(155, 89, 182)
    Public Shared ReadOnly DangerRed As Color = Color.FromArgb(231, 76, 60)
    Public Shared ReadOnly WarningOrange As Color = Color.FromArgb(243, 156, 18)
    Public Shared ReadOnly DarkBlue As Color = Color.FromArgb(52, 73, 94)
    Public Shared ReadOnly SteelBlueColor As Color = Color.SteelBlue

    ' Panel Colors
    Public Shared ReadOnly TitlePanelColor As Color = Color.FromArgb(52, 73, 94)
    Public Shared ReadOnly ControlPanelColor As Color = Color.White
    Public Shared ReadOnly FooterPanelColor As Color = Color.WhiteSmoke
    Public Shared ReadOnly DataGridHeaderColor As Color = Color.FromArgb(52, 152, 219)
    Public Shared ReadOnly AlternatingRowColor As Color = Color.FromArgb(240, 240, 240)
#End Region

#Region "Font Constants"
    Public Shared ReadOnly TitleFont As Font = New Font("Segoe UI", 18.0!, FontStyle.Bold)
    Public Shared ReadOnly SectionHeaderFont As Font = New Font("Segoe UI", 14.0!, FontStyle.Bold)
    Public Shared ReadOnly LabelFont As Font = New Font("Segoe UI", 10.0!, FontStyle.Regular)
    Public Shared ReadOnly ButtonFont As Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
    Public Shared ReadOnly DataGridHeaderFont As Font = New Font("Segoe UI Semibold", 11.0!, FontStyle.Bold)
    Public Shared ReadOnly DataGridCellFont As Font = New Font("Segoe UI", 10.0!, FontStyle.Regular)
#End Region

#Region "Button Styling"
    ''' <summary>
    ''' Apply primary blue button style (Refresh, Search, View, Navigate)
    ''' </summary>
    Public Shared Sub ApplyPrimaryButtonStyle(btn As Button)
        ApplyBaseButtonStyle(btn, PrimaryBlue)
    End Sub

    ''' <summary>
    ''' Apply success green button style (Export, Save, Submit)
    ''' </summary>
    Public Shared Sub ApplySuccessButtonStyle(btn As Button)
        ApplyBaseButtonStyle(btn, SuccessGreen)
    End Sub

    ''' <summary>
    ''' Apply purple button style (Generate Report)
    ''' </summary>
    Public Shared Sub ApplyPurpleButtonStyle(btn As Button)
        ApplyBaseButtonStyle(btn, Purple)
    End Sub

    ''' <summary>
    ''' Apply danger red button style (Delete, Remove)
    ''' </summary>
    Public Shared Sub ApplyDangerButtonStyle(btn As Button)
        ApplyBaseButtonStyle(btn, DangerRed)
    End Sub

    ''' <summary>
    ''' Apply warning orange button style (Edit, Update)
    ''' </summary>
    Public Shared Sub ApplyWarningButtonStyle(btn As Button)
        ApplyBaseButtonStyle(btn, WarningOrange)
    End Sub

    ''' <summary>
    ''' Apply base button styling
    ''' </summary>
    Private Shared Sub ApplyBaseButtonStyle(btn As Button, backColor As Color)
        btn.BackColor = backColor
        btn.ForeColor = Color.White
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderSize = 0
        btn.Font = ButtonFont
        btn.Cursor = Cursors.Hand
        
        ' Set default size kung wala pang size
        If btn.Size = New Size(0, 0) Then
            btn.Size = New Size(120, 35)
        End If
    End Sub
#End Region

#Region "Panel Styling"
    ''' <summary>
    ''' Apply title panel style (dark blue background)
    ''' </summary>
    Public Shared Sub ApplyTitlePanelStyle(panel As Panel, Optional height As Integer = 60)
        panel.BackColor = TitlePanelColor
        panel.Dock = DockStyle.Top
        panel.Size = New Size(panel.Parent.ClientSize.Width, height)
    End Sub

    ''' <summary>
    ''' Apply control panel style (white background)
    ''' </summary>
    Public Shared Sub ApplyControlPanelStyle(panel As Panel, Optional height As Integer = 80)
        panel.BackColor = ControlPanelColor
        panel.Dock = DockStyle.Top
        panel.Padding = New Padding(10)
        panel.Size = New Size(panel.Parent.ClientSize.Width, height)
    End Sub

    ''' <summary>
    ''' Apply footer panel style (WhiteSmoke background)
    ''' </summary>
    Public Shared Sub ApplyFooterPanelStyle(panel As Panel, Optional height As Integer = 50)
        panel.BackColor = FooterPanelColor
        panel.Dock = DockStyle.Bottom
        panel.Size = New Size(panel.Parent.ClientSize.Width, height)
        panel.Padding = New Padding(10, 5, 10, 5)
    End Sub

    ''' <summary>
    ''' Apply main content panel style
    ''' </summary>
    Public Shared Sub ApplyMainPanelStyle(panel As Panel)
        panel.BackColor = Color.White
        panel.Dock = DockStyle.Fill
        panel.Padding = New Padding(10)
    End Sub
#End Region

#Region "DataGridView Styling"
    ''' <summary>
    ''' Apply standard DataGridView styling
    ''' </summary>
    Public Shared Sub ApplyDataGridViewStyle(dgv As DataGridView)
        ' Basic properties
        dgv.AllowUserToAddRows = False
        dgv.AllowUserToDeleteRows = False
        dgv.ReadOnly = True
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgv.MultiSelect = False
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgv.BorderStyle = BorderStyle.None
        dgv.BackgroundColor = Color.White
        dgv.RowHeadersVisible = False
        dgv.EnableHeadersVisualStyles = False

        ' Column header style
        dgv.ColumnHeadersDefaultCellStyle.BackColor = DataGridHeaderColor
        dgv.ColumnHeadersDefaultCellStyle.Font = DataGridHeaderFont
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = DataGridHeaderColor
        dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White
        dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft
        dgv.ColumnHeadersHeight = 40

        ' Default cell style
        dgv.DefaultCellStyle.BackColor = Color.White
        dgv.DefaultCellStyle.ForeColor = Color.Black
        dgv.DefaultCellStyle.SelectionBackColor = SteelBlueColor
        dgv.DefaultCellStyle.SelectionForeColor = Color.White
        dgv.DefaultCellStyle.Font = DataGridCellFont
        dgv.DefaultCellStyle.Padding = New Padding(5)

        ' Alternating row style
        dgv.AlternatingRowsDefaultCellStyle.BackColor = AlternatingRowColor
        dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = SteelBlueColor
        dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.White

        ' Row style
        dgv.RowTemplate.Height = 35
        dgv.CellBorderStyle = DataGridViewCellBorderStyle.None
    End Sub
#End Region

#Region "Label Styling"
    ''' <summary>
    ''' Apply title label style (for title panels)
    ''' </summary>
    Public Shared Sub ApplyTitleLabelStyle(lbl As Label)
        lbl.Font = TitleFont
        lbl.ForeColor = Color.White
        lbl.AutoSize = True
    End Sub

    ''' <summary>
    ''' Apply section header label style
    ''' </summary>
    Public Shared Sub ApplySectionHeaderStyle(lbl As Label)
        lbl.Font = SectionHeaderFont
        lbl.ForeColor = DarkBlue
        lbl.AutoSize = True
    End Sub

    ''' <summary>
    ''' Apply standard label style
    ''' </summary>
    Public Shared Sub ApplyLabelStyle(lbl As Label)
        lbl.Font = LabelFont
        lbl.ForeColor = Color.Black
        lbl.AutoSize = True
    End Sub
#End Region

#Region "Form Styling"
    ''' <summary>
    ''' Apply standard form background
    ''' </summary>
    Public Shared Sub ApplyFormStyle(form As Form)
        form.BackColor = Color.White
    End Sub
#End Region

End Class
