VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
END
Attribute VB_Name = "SpecialFolder"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = False
' XVBA用アノテーション
'namespace=vba-files\Folder
' Rubberduck用アノテーション
'@Folder "Folder"
Option Explicit

'ShellSpecialFolder定数
Public Enum ShellSpecialFolderConstants
    AllUsersDesktop = 0     ' すべてのユーザーに共通のデスクトップ
    AllUsersStartMenu = 1     ' すべてのユーザーに共通のスタートメニュー
    AllUsersPrograms = 2     ' すべてのユーザーに共通のプログラムメニュー
    AllUsersStartup = 3     ' すべてのユーザーに共通のスタートアップ
    Desktop = 4     ' ログインユーザーのデスクトップ
    AppData = 5     ' ログインユーザーのアプリ用データ
    PrintHood = 6     ' ログインユーザーのプリンタ
    Templates = 7     ' ログインユーザーの新規作成のテンプレート
    Fonts = 8     ' フォント
    NetHood = 9     ' ログインユーザーのネットワーク
    'Desktop = 10     ' ログインユーザーのデスクトップ
    StartMenu = 11     ' ログインユーザーのスタートメニュー
    SendTo = 12     ' ログインユーザーの送る
    Recent = 13     ' ログインユーザーの最近使ったファイル
    Startup = 14     ' ログインユーザーのスタートアップ
    Favorites = 15     ' ログインユーザーのお気に入り
    MyDocuments = 16     ' ログインユーザーのマイドキュメント
    Programs = 17     ' ログインユーザーのプログラムメニュー
End Enum

Public Function GetShellSpecialFolderNameSpace(ByVal vDir As ShellSpecialFolderConstants) As String
'特殊フォルダのパスまたはネームスペースを返す
    Dim wsh
    Set wsh = CreateObject("WScript.Shell")
    GetShellSpecialFolderNameSpace = wsh.SpecialFolders(vDir)
End Function
