Attribute VB_Name = "SyncFolder"
' XVBA�p�A�m�e�[�V����
'namespace=vba-files\Folder
' Rubberduck�p�A�m�e�[�V����
'@Folder "Folder"
Option Explicit

Public Sub SyncFolders(ByVal sourceFolder As String, ByVal destinationFolder As String)
    Dim fso As Object
    Set fso = CreateObject("Scripting.FileSystemObject")

    'sourceFolder��destinationFolder�𐳋K������
    sourceFolder = fso.GetAbsolutePathName(sourceFolder)
    destinationFolder = fso.GetAbsolutePathName(destinationFolder)

    'sourceFolder�����݂��Ȃ��ꍇ�͏����𒆎~����
    If Not fso.FolderExists(sourceFolder) Then
        Err.Raise Number:=vbObjectError + 513, Description:="sourceFolder�����݂��Ȃ�"
        Exit Sub
    End If

    'destinationFolder�����݂��Ȃ��ꍇ�͍쐬����
    If Not fso.FolderExists(destinationFolder) Then
        fso.CreateFolder destinationFolder
    End If

    'sourceFolder�̂��ׂẴt�@�C�����擾����
    Dim files As Object
    Set files = fso.GetFolder(sourceFolder).files

    'destinationFolder�̂��ׂẴt�@�C�����擾����
    Dim destFiles As Object
    Set destFiles = fso.GetFolder(destinationFolder).files

    'sourceFolder�̃t�@�C����destinationFolder�ɃR�s�[����
    Dim file As Object
    For Each file In files
        Dim destFilePath As String
        destFilePath = fso.BuildPath(destinationFolder, file.Name)

        '�t�@�C�������݂��Ȃ��ꍇ�A�܂��͍X�V�������قȂ�ꍇ�̓R�s�[����
        If Not fso.FileExists(destFilePath) Then
            fso.CopyFile file.Path, destFilePath, True
        ElseIf file.DateLastModified > fso.GetFile(destFilePath).DateLastModified Then
            fso.CopyFile file.Path, destFilePath, True
        End If
    Next

    'destinationFolder�ɑ��݂��邪�AsourceFolder�ɑ��݂��Ȃ��t�@�C�����폜����
    Dim destFile As Object
    For Each destFile In destFiles
        Dim sourceFilePath As String
        sourceFilePath = fso.BuildPath(sourceFolder, destFile.Name)

        'sourceFolder�Ƀt�@�C�������݂��Ȃ��ꍇ�͍폜����
        If Not fso.FileExists(sourceFilePath) Then
            fso.DeleteFile destFile.Path, True
        End If
    Next
End Sub
