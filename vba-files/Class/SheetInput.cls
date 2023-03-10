VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
END
Attribute VB_Name = "SheetInput"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = True
'@Folder "ManHourRecord"
Option Explicit

Private Sub Worksheet_Change(ByVal Target As Range)
    If Target.Address <> "$F$2" Then
        Exit Sub
    End If
    
    If IsEmpty(Target) Then
        Exit Sub
    End If
    
    Application.Run "SettingWorkingClassification"
End Sub
