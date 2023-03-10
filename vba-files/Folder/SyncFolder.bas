Attribute VB_Name = "SyncFolder"
' XVBA用アノテーション
'namespace=vba-files\Folder
' Rubberduck用アノテーション
'@Folder "Folder"
Option Explicit

Public Sub SyncFolders(ByVal sourceFolder As String, ByVal destinationFolder As String)
    Dim fso As Object
    Set fso = CreateObject("Scripting.FileSystemObject")

    'sourceFolderとdestinationFolderを正規化する
    sourceFolder = fso.GetAbsolutePathName(sourceFolder)
    destinationFolder = fso.GetAbsolutePathName(destinationFolder)

    'sourceFolderが存在しない場合は処理を中止する
    If Not fso.FolderExists(sourceFolder) Then
        Err.Raise Number:=vbObjectError + 513, Description:="sourceFolderが存在しない"
        Exit Sub
    End If

    'destinationFolderが存在しない場合は作成する
    If Not fso.FolderExists(destinationFolder) Then
        fso.CreateFolder destinationFolder
    End If

    'sourceFolderのすべてのファイルを取得する
    Dim files As Object
    Set files = fso.GetFolder(sourceFolder).files

    'destinationFolderのすべてのファイルを取得する
    Dim destFiles As Object
    Set destFiles = fso.GetFolder(destinationFolder).files

    'sourceFolderのファイルをdestinationFolderにコピーする
    Dim file As Object
    For Each file In files
        Dim destFilePath As String
        destFilePath = fso.BuildPath(destinationFolder, file.Name)

        'ファイルが存在しない場合、または更新日時が異なる場合はコピーする
        If Not fso.FileExists(destFilePath) Then
            fso.CopyFile file.Path, destFilePath, True
        ElseIf file.DateLastModified > fso.GetFile(destFilePath).DateLastModified Then
            fso.CopyFile file.Path, destFilePath, True
        End If
    Next

    'destinationFolderに存在するが、sourceFolderに存在しないファイルを削除する
    Dim destFile As Object
    For Each destFile In destFiles
        Dim sourceFilePath As String
        sourceFilePath = fso.BuildPath(sourceFolder, destFile.Name)

        'sourceFolderにファイルが存在しない場合は削除する
        If Not fso.FileExists(sourceFilePath) Then
            fso.DeleteFile destFile.Path, True
        End If
    Next
End Sub
