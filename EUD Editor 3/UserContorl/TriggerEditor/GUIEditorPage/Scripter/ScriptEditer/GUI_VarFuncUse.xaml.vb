﻿Imports Dragablz
Imports Microsoft.DwayneNeed.Win32.Gdi32

Public Class GUI_VarFuncUse
    Public Event RefreshEvent As RoutedEventHandler


    Private scr As ScriptBlock
    Private dotscr As ScriptBlock
    Private GUIEditorUI As GUIScriptEditorUI
    Private ArgEditor As GUI_ArgEditor

    Public Sub CrlInit(_scr As ScriptBlock, _dotscr As ScriptBlock, _GUIEditorUI As GUIScriptEditorUI, _ArgEditor As GUI_ArgEditor)
        scr = _scr
        dotscr = _dotscr
        GUIEditorUI = _GUIEditorUI
        ArgEditor = _ArgEditor

        tLabel.Visibility = Visibility.Collapsed
        MainPanel.Children.Clear()
        tLabel.Content = _scr.name & ":" & _scr.value & ":" & _scr.value2


        Dim flag As Boolean = False


        'v1::value
        'Test:f1:fields
        'tt.o2:!default:
        'testObject:alloc:
        If scr.value2 = "value" Then
            flag = True
        End If
        If scr.value2 = "fields" Then
            flag = True
        End If
        If scr.value = "!default" Then
            flag = True
        End If
        If scr.value = "alloc" Then
            flag = True
        End If
        If flag Then
            scr.child.Clear()
            Return
        End If


        'tt.o2:!index:
        'testObject:cast:
        Dim isCast As Boolean = False
        If scr.value = "cast" Then
            If scr.child.Count = 0 Then
                Dim tscr As New ScriptBlock(ScriptBlock.EBlockType.varuse, "init", True, False, "init", scr.Scripter)
                tscr.value2 = "value"
                scr.AddChild(tscr)
            End If
            isCast = True
            flag = True
        End If
        If scr.value = "!index" Then
            If scr.child.Count = 0 Then
                scr.AddChild(New ScriptBlock(ScriptBlock.EBlockType.constVal, "Number", True, False, "0", scr.Scripter))
            End If
            isCast = False
            flag = True
        End If
        If flag Then
            Dim tb As New TextBlock
            If isCast Then
                tb.Text = "Cast : "
            Else
                tb.Text = "Index : "
            End If
            tb.VerticalAlignment = VerticalAlignment.Center

            MainPanel.Children.Add(tb)

            Dim b As New Button
            If isCast Then
                b.Tag = {b, scr.child(0), "캐스트 벨류"}
            Else
                b.Tag = {b, scr.child(0), "인덱스 설정"}
            End If

            AddHandler b.Click, AddressOf BtnClick

            b.Content = scr.child(0).ValueCoder


            MainPanel.Children.Add(b)
            Return
        End If


        Dim methodname As String = ""
        Dim varname As String = ""
        Dim objname As String = ""

        If scr.value2 = "method" Then
            flag = True

            varname = scr.name
            methodname = scr.value

        End If
        If scr.value = "constructor" Then
            flag = False

            objname = scr.name
            methodname = scr.value
        End If


        Dim objscr As ScriptBlock
        If flag Then
            'method

            'varname을 이용해 변수를 먼저 찾아가야됨
            '.은 항상 있음.
            '.
            Dim tstr() As String = varname.Split(".")
            If tstr.Count = 1 Then
                '변수가 내부에 있음

                Dim scrlist As New List(Of ScriptBlock)
                scrlist.AddRange(tescm.GetLocalVar(dotscr, "object", varname))
                scrlist.AddRange(tescm.GetGlobalVar(GUIEditorUI.Script, "object", varname))

                If scrlist.Count <> 0 Then
                    Dim tscr As ScriptBlock = scrlist.First

                    objname = tscr.child(0).name

                    'tLabel.Content = scrlist.First.name & ":" & scrlist.First.value & ":" & scrlist.First.value2
                Else
                    tLabel.Visibility = Visibility.Visible
                    tLabel.Content = "존재하지 않는 벨류"
                End If
            ElseIf tstr.Count = 2 Then
                '변수가 외부에 있음

                Dim spacename As String = tstr(0)
                varname = tstr(1)
                'MsgBox(spacename & "," & varname)

                Dim scrlist As New List(Of ScriptBlock)
                scrlist.AddRange(tescm.GetExternVar(GUIEditorUI.Script, "object", varname, spacename))

                If scrlist.Count <> 0 Then
                    Dim tscr As ScriptBlock = scrlist.First

                    objname = tscr.child(0).name

                    'tLabel.Content = scrlist.First.name & ":" & scrlist.First.value & ":" & scrlist.First.value2
                Else
                    tLabel.Visibility = Visibility.Visible
                    tLabel.Content = "존재하지 않는 벨류"
                    Return
                End If
            End If

        End If

        Dim isExternObj As Boolean
        Dim ttstr() As String = objname.Split(".")
        If ttstr.Count = 1 Then
            '오브젝트가 바로있음

            Dim scrlist As New List(Of ScriptBlock)
            scrlist.AddRange(tescm.GetGlobalObject(GUIEditorUI.Script))

            If scrlist.Count <> 0 Then
                objscr = scrlist.First
                isExternObj = False
            Else
                tLabel.Visibility = Visibility.Visible
                tLabel.Content = "존재하지 않는 벨류"
                Return
            End If
        Else
            '오브젝트가 외부에있음

            Dim spacename As String = ttstr(0)
            objname = ttstr(1)

            Dim scrlist As New List(Of ScriptBlock)
            scrlist.AddRange(tescm.GetExternObject(GUIEditorUI.Script, spacename))

            If scrlist.Count <> 0 Then
                objscr = scrlist.First
                isExternObj = True
            Else
                tLabel.Visibility = Visibility.Visible
                tLabel.Content = "존재하지 않는 벨류"
                Return
            End If
        End If


        tLabel.Content = "오브젝트 이름" & objscr.value

        Dim methodlist As List(Of ScriptBlock) = tescm.GetObjectMethod(objscr)
        Dim IsExistFunc As Boolean = False
        Dim methodfunc As ScriptBlock = Nothing
        For i = 0 To methodlist.Count - 1
            If methodname = methodlist(i).value Then
                IsExistFunc = True
                methodfunc = methodlist(i)
                Exit For
            End If
        Next
        If IsExistFunc And methodfunc IsNot Nothing Then
            If scr.child.Count <> methodfunc.child.Count Then
                If methodfunc.child.Count < scr.child.Count Then
                    For i = 0 To scr.child.Count - methodfunc.child.Count - 1
                        scr.child.RemoveAt(methodfunc.child.Count)
                    Next
                Else
                    If isExternObj Then
                        Dim cfun As CFunc = methodfunc.tobject
                        Dim cfunindex As Integer = methodfunc.value2

                        Dim functooltip As FunctionToolTip = cfun.GetFuncTooltip(cfunindex)
                        Dim argument As String = cfun.GetFuncArgument(cfunindex)

                        If argument.Trim <> "" Then
                            Dim args() As String = argument.Split(",")
                            For i = 0 To args.Length - 1
                                If scr.child.Count <= i Then
                                    Dim tstr() As String = args(i).Split(":")

                                    Dim vname As String = "defaultvalue;" & tstr.First.Trim
                                    Dim vtype As String = tstr.Last

                                    'ReplaceChild(New ScriptBlock(vtype, False, False, vname, Scripter), i)

                                    scr.AddChild(New ScriptBlock(ScriptBlock.EBlockType.constVal, vtype, False, False, vname, GUIEditorUI.Script))
                                End If
                            Next
                        End If
                    Else
                        Dim func As ScriptBlock = methodfunc
                        'DEBUG
                        Dim args As List(Of ScriptBlock) = tescm.GetFuncArgs(func)


                        For i = 0 To args.Count - 1
                            If scr.child.Count <= i Then
                                Dim vname As String = "defaultvalue;" & args(i).value.Trim
                                Dim vtype As String = args(i).name

                                'ReplaceChild(New ScriptBlock(vtype, False, False, vname, Scripter), i)

                                Dim tscr As New ScriptBlock(ScriptBlock.EBlockType.constVal, vtype, False, False, vname, GUIEditorUI.Script)
                                tscr.value2 = args(i).value2
                                scr.AddChild(tscr)
                            End If
                        Next

                    End If
                End If
            End If


            If isExternObj Then
                EnCoding(isExternObj, methodfunc, methodfunc.tobject, methodfunc.value2)
            Else
                EnCoding(isExternObj, methodfunc)
            End If
        Else
            tLabel.Visibility = Visibility.Visible
            If methodname = "constructor" Then
                tLabel.Content = "기본생성자"
            Else
                tLabel.Content = "존재하지 않는 함수"
            End If
        End If

        'Test:method:method
        'testObject:constructor:
    End Sub

    Private Sub EnCoding(isScrBlock As Boolean, func As ScriptBlock, Optional cfun As CFunc = Nothing, Optional findex As Integer = 0)
        If Not isScrBlock Then
            If func Is Nothing Then
                DefaultCoder()
            Else
                Dim argumentstr As String = func.GetFuncTooltip
                'ExtraTip.Text = argumentstr
                Dim argsb As List(Of ScriptBlock) = tescm.GetFuncArgs(func)
                Dim args As New List(Of String)
                Dim argtooltip As New List(Of String)

                Dim arglist As New List(Of String)
                Dim argTooltiplist As New List(Of String)
                For i = 0 To argsb.Count - 1
                    Dim argname As String = argsb(i).value
                    Dim sargtooltip As String = argsb(i).value2
                    argtooltip.Add(sargtooltip)
                    args.Add(argname)
                Next

                Dim vcount As Integer = 0
                For k = 0 To args.Count - 1
                    Dim aname As String = args(k)

                    arglist.Add(aname)
                    argTooltiplist.Add(argtooltip(k))

                    If argumentstr.IndexOf("[" & aname & "]") <> -1 Then
                        vcount += 1
                    End If
                    argumentstr = argumentstr.Replace("[" & aname & "]", "ᐱᐳ" & aname & "ᐱ")

                Next
                Dim values() As String = argumentstr.Split("ᐱ")

                If vcount <> args.Count Then
                    DefaultCoder()
                    Return
                End If

                For k = 0 To values.Count - 1
                    If values(k).Trim <> "" Then
                        If values(k)(0) = "ᐳ" Then
                            Dim vname As String = Mid(values(k), 2)

                            Dim btn As New Button
                            btn.Padding = New Thickness(5, 0, 5, 0)
                            btn.Height = 22
                            Dim listindex As Integer = arglist.IndexOf(vname)

                            btn.Tag = {btn, scr.child(listindex), argTooltiplist(listindex)}
                            AddHandler btn.Click, AddressOf BtnClick

                            btn.Content = scr.child(listindex).ValueCoder()
                            MainPanel.Children.Add(btn)
                            'btnlist.Add(btn)
                        Else
                            Dim tbox As New TextBlock
                            tbox.TextWrapping = TextWrapping.Wrap
                            tbox.VerticalAlignment = VerticalAlignment.Center
                            tbox.HorizontalAlignment = HorizontalAlignment.Center
                            tbox.Text = values(k)

                            MainPanel.Children.Add(tbox)
                        End If
                    End If

                Next
            End If
        Else
            Dim i As Integer = findex

            If i >= 0 Then
                Dim functooltip As FunctionToolTip = cfun.GetFuncTooltip(i)
                If functooltip.Summary.Trim = "" Then
                    DefaultCoder()
                    Return
                End If

                Dim argumentstr As String = functooltip.Summary.Split(vbCrLf).First
                argumentstr = functooltip.Summary.Replace(argumentstr, "").Trim
                'ExtraTip.Text = argumentstr

                Dim arglist As New List(Of String)
                Dim argTooltiplist As New List(Of String)
                Dim args() As String = cfun.GetFuncArgument(i).Split(",")

                Dim vcount As Integer = 0
                For k = 0 To args.Count - 1
                    Dim aname As String = args(k).Split(":").First.Trim

                    arglist.Add(aname)
                    argTooltiplist.Add(functooltip.GetTooltip(k))

                    If argumentstr.IndexOf("[" & aname & "]") <> -1 Then
                        vcount += 1
                    End If
                    argumentstr = argumentstr.Replace("[" & aname & "]", "ᐱᐳ" & aname & "ᐱ")

                Next
                Dim values() As String = argumentstr.Split("ᐱ")

                If vcount <> args.Count Then
                    DefaultCoder()
                    Return
                End If


                For k = 0 To values.Count - 1
                    If values(k).Trim <> "" Then
                        If values(k)(0) = "ᐳ" Then
                            Dim vname As String = Mid(values(k), 2)

                            Dim btn As New Button
                            btn.Padding = New Thickness(5, 0, 5, 0)
                            btn.Height = 22
                            Dim listindex As Integer = arglist.IndexOf(vname)


                            btn.Tag = {btn, scr.child(listindex), argTooltiplist(listindex)}
                            AddHandler btn.Click, AddressOf BtnClick

                            btn.Content = scr.child(arglist.IndexOf(vname)).ValueCoder()
                            MainPanel.Children.Add(btn)
                            'btnlist.Add(btn)
                        Else
                            Dim tbox As New TextBlock
                            tbox.TextWrapping = TextWrapping.Wrap
                            tbox.VerticalAlignment = VerticalAlignment.Center
                            tbox.HorizontalAlignment = HorizontalAlignment.Center
                            tbox.Text = values(k)

                            MainPanel.Children.Add(tbox)
                        End If
                    End If

                Next

            Else
                DefaultCoder()
            End If
        End If
    End Sub


    Private Sub DefaultCoder()
        If True Then
            Dim tbox As New TextBlock
            tbox.TextWrapping = TextWrapping.Wrap
            tbox.VerticalAlignment = VerticalAlignment.Center
            tbox.HorizontalAlignment = HorizontalAlignment.Center
            tbox.Text = scr.name & "("
            MainPanel.Children.Add(tbox)
        End If

        For i = 0 To scr.child.Count - 1
            If i <> 0 Then
                Dim tbox As New TextBlock
                tbox.TextWrapping = TextWrapping.Wrap
                tbox.VerticalAlignment = VerticalAlignment.Center
                tbox.HorizontalAlignment = HorizontalAlignment.Center
                tbox.Text = " , "
                MainPanel.Children.Add(tbox)
            End If

            Dim des As String = scr.child(i).value2

            Dim btn As New Button
            btn.Padding = New Thickness(5, 0, 5, 0)
            btn.Height = 22


            btn.Tag = {btn, scr.child(i), des}
            AddHandler btn.Click, AddressOf BtnClick
            'btn.Tag = New GUI_Action.tagcontainer(scr.child(i), EditValues(i), des)

            'AddHandler btn.Click, AddressOf argBtnClick

            btn.Content = scr.child(i).ValueCoder()
            MainPanel.Children.Add(btn)
            'btnlist.Add(btn)
        Next


        If True Then
            Dim tbox As New TextBlock
            tbox.TextWrapping = TextWrapping.Wrap
            tbox.VerticalAlignment = VerticalAlignment.Center
            tbox.HorizontalAlignment = HorizontalAlignment.Center
            tbox.Text = ")"
            MainPanel.Children.Add(tbox)
        End If

    End Sub



    Public Event ArgBtnRefreshEvent As RoutedEventHandler
    Public Event ArgBtnClickEvent As RoutedEventHandler

    'Private btnlist As New List(Of Button)


    Public Sub BtnClick(sender As Button, e As RoutedEventArgs)

        Dim objs() As Object = sender.Tag

        Dim btn As Button = objs(0)
        Dim scr As ScriptBlock = objs(1)
        Dim des As String = objs(2)



        Dim graywindow As New GUI_GrayWindow


        graywindow.Height = GUIEditorUI.TEGUIPage.ActualHeight
        graywindow.Width = GUIEditorUI.TEGUIPage.ActualWidth
        Dim cpos As Point = GUIEditorUI.TEGUIPage.PointToScreen(New Point(0, 0))

        graywindow.Top = cpos.Y
        graywindow.Left = cpos.X

        graywindow.Show()

        Dim argwindow As New GUI_ArgEditorWindow(graywindow)

        argwindow.Top = cpos.Y + GUIEditorUI.TEGUIPage.ActualHeight / 2 - argwindow.Height / 2
        argwindow.Left = cpos.X + GUIEditorUI.TEGUIPage.ActualWidth / 2 - argwindow.Width / 2


        'ArgSelecter를 건내줘야되 그래야 리프레쉬됨
        'GUI_ArgEditor


        argwindow.ArgEditor.Init(argwindow.ValueSelecter, dotscr, GUIEditorUI)
        'If scr.ScriptType = ScriptBlock.EBlockType.exp Then
        '    AddHandler argwindow.ArgEditor.BtnRefresh, AddressOf ArgEditor.ArgExpress.argBtnRefresh
        'Else
        '    AddHandler argwindow.ArgEditor.BtnRefresh, AddressOf ArgEditor.ArgSelecter.argBtnRefresh
        'End If
        destb.Text = des


        argwindow.ArgEditor.Visibility = Visibility.Visible
        argwindow.ArgEditor.ComboboxInit(scr)
        argwindow.ShowDialog()

        btn.Content = scr.ValueCoder

        RaiseEvent RefreshEvent(sender, e)
    End Sub
End Class