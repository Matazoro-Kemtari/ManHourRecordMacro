VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
END
Attribute VB_Name = "ManHourRecordAddInHelper"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = False
' XVBA用アノテーション
'namespace=vba-files\ManHourRecord
' Rubberduck用アノテーション
'@Folder "ManHourRecord"
Option Explicit

#If VBA7 And Win64 Then
    Const file As String = "\\192.168.11.16\Public\04共通\開発・設計受渡し\工数記録システム\プログラム\ManHourRecordAddIn-AddIn64.xll"
#Else
    Const file As String = "\\192.168.11.16\Public\04共通\開発・設計受渡し\工数記録システム\プログラム\ManHourRecordAddIn-AddIn.xll"
#End If

Private AddInLoaded As Boolean

Private Sub class_initialize()
    AddInLoaded = False
End Sub

Private Sub Class_Terminate()
    On Error Resume Next
    If AddInLoaded Then
        TestExcel4MacroRemove
    End If
End Sub

Public Function EnableAddIn() As Boolean
    If AddInLoaded Then
        EnableAddIn = True
        Exit Function
    End If

    Dim fso As Object
    Set fso = CreateObject("Scripting.FileSystemObject")

    ' 特殊フォルダ取得
    Dim spFolder As New SpecialFolder
    Dim wadaWorkDir As String
    wadaWorkDir = fso.BuildPath(spFolder.GetShellSpecialFolderNameSpace(AppData), "Wadass")
    If Not fso.FolderExists(wadaWorkDir) Then
        Call fso.CreateFolder(wadaWorkDir)
    End If
    Dim LocalLibPath As String
    LocalLibPath = fso.BuildPath(wadaWorkDir, "ManHourRecordAddIn")
    Set spFolder = Nothing

    Dim sourceFolder As String
    sourceFolder = fso.GetParentFolderName(file)

    ' ファイルの同期
    Call SyncFolders(sourceFolder, LocalLibPath)

    Dim localLibName As String
    localLibName = fso.BuildPath(LocalLibPath, fso.GetFileName(file))
    Call TestExcel4MacroLoad(localLibName)
    AddInLoaded = True
    EnableAddIn = True
End Function

Public Function CheckManHourInputExcelSerial() As Boolean
    Dim result As Variant
    result = Application.Run("CheckManHourInputExcelSerial")
    CheckManHourInputExcelSerial = result = True
End Function

Public Sub SettingDepartmentValidationRule()
    Application.Run "SettingDepartmentValidationRule"
End Sub

Public Sub SettingWorkingClassification()
    Application.Run "SettingWorkingClassification"
End Sub

' http://blog.livedoor.jp/hiroto_o/archives/48433180.html
Sub TestExcel4MacroLoad(ByVal libFile As String)
    Dim v As Variant
    v = Application.RegisterXLL(libFile)
End Sub

Sub TestExcel4MacroRemove(ByVal libFile As String)
    Dim vXlAutoRemoveHandle As Variant
    Dim s As String
    s = "REGISTER(""" & libFile & """, " & """xlAutoRemove""" & "," & """J""" & "," & """TestAutoRemoveCode""" & ",,1)"
    vXlAutoRemoveHandle = Application.ExecuteExcel4Macro(s)
    'Debug.Print vXlAutoRemoveHandle

    Dim vResultXlAutoRemove As Variant
    s = "CALL(" & vXlAutoRemoveHandle & ")"
    vResultXlAutoRemove = Application.ExecuteExcel4Macro(s)

    Dim vResultUnregisterXlAutoRemove As Variant
    s = "UNREGISTER(" & vXlAutoRemoveHandle & ")"
    vResultUnregisterXlAutoRemove = Application.ExecuteExcel4Macro(s)
    'Debug.Print vResultUnregisterXlAutoRemove

    Dim vResultUnregister As Variant
    s = "UNREGISTER(""" & libFile & """)"
    vResultUnregister = Application.ExecuteExcel4Macro(s)
    'Debug.Print vResultUnregister
End Sub
