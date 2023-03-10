Attribute VB_Name = "ModuleInputSheet"
' XVBA用アノテーション
'namespace=vba-files\ManHourRecord
' Rubberduck用アノテーション
'@Folder "ManHourRecord"
Option Explicit

Public ManHourRecord As ManHourRecordAddInHelper

Sub EntryManHour()
    Application.Run "WriteAttendance"
End Sub
