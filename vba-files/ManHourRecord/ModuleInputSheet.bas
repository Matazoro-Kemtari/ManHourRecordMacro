Attribute VB_Name = "ModuleInputSheet"
' XVBA�p�A�m�e�[�V����
'namespace=vba-files\ManHourRecord
' Rubberduck�p�A�m�e�[�V����
'@Folder "ManHourRecord"
Option Explicit

Public ManHourRecord As ManHourRecordAddInHelper

Sub EntryManHour()
    Application.Run "WriteAttendance"
End Sub
