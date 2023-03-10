VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
END
Attribute VB_Name = "ThisWorkbook"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = True
'@Folder "ManHourRecord"
Option Explicit

Private Sub Workbook_Open()
    ItemClassification.Visible = xlSheetHidden
    SheetServiceClassification.Visible = xlSheetHidden
    SheetConfiguration.Visible = xlSheetHidden

#If DEBUG_MODE Then
    ' デバッグモードはここまで
    Exit Sub
#End If

    Set ManHourRecord = New ManHourRecordAddInHelper
    
    If ManHourRecord.EnableAddIn() Then
        If ManHourRecord.CheckManHourInputExcelSerial() Then
            Call ManHourRecord.SettingDepartmentValidationRule
        End If
    End If
End Sub
