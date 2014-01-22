Imports System.Text.RegularExpressions
Imports System.Net
Imports RazorEngine
Imports RazorEngine.Templating
Imports RazorEngine.Configuration
Imports System.Text
Imports System.Security.Cryptography
Imports System.IO


Module Module1



    Sub Main()




        Dim options = New Options()
        If (CommandLine.Parser.Default.ParseArguments(My.Application.CommandLineArgs.ToArray(), options)) Then
            Dim startlink = options.StartUrl



            Dim url = NormalizedLink(startlink, Nothing)

            If url IsNot Nothing Then
                StartWork(url, options)
            Else
                Console.WriteLine("输入的URL不符合规范。")

            End If

        End If


    End Sub

    ''' <summary>
    ''' 开始工作
    ''' </summary>
    ''' <param name="root">开始根 url</param>
    ''' <remarks></remarks>
    Private Sub StartWork(root As Uri, options As Options)

        Console.WriteLine("正在初始化...")


        Dim Allinks As New Dictionary(Of String, PageInfo)

        Dim trees As New HashSet(Of PageInfo)


        Dim html = New HtmlAgilityPack.HtmlWeb


        Dim rules As New HashSet(Of String)

        If options.Rules.Any Then
            For Each r In options.Rules
                rules.Add(r)
            Next


        Else


            Dim rule = root.ToString()

            If root.Segments.Length > 1 Then
                Dim lastpart = root.Segments.LastOrDefault

                If Not lastpart.EndsWith("/"c) Then
                    Dim str = root.ToString()
                    rule = str.Substring(0, str.LastIndexOf(lastpart))
                End If
            End If

            Dim chars = "\^$*+?{}.()".ToList()

            chars.ForEach(Sub(c) rule = rule.Replace(c.ToString(), "\" & c.ToString()))

            rules.Add(rule)

        End If



        Dim xpath = options.XPath

        Dim maxdeep = options.MaxDeep

        Dim pi As New PageInfo With {.Title = "", .Url = root, .DeepLevel = 0, .Links = New List(Of PageInfo)}

        trees.Add(pi)

        Console.WriteLine("初始化完成...")

        GetPageLinks(Allinks, rules, xpath, html, maxdeep, pi)

        Console.WriteLine("扫描网页完成...")


        '输出

        Console.WriteLine("扫描到{0}个网址。",
                          Allinks.Count)


        Dim fi = New IO.FileInfo(options.FileName & ".html")

        Console.WriteLine("保存结果到文件：{0}", fi.FullName)

        OutputToFileHTML(Allinks, trees, fi.FullName, rules, root, maxdeep)


        fi = New IO.FileInfo(options.FileName & ".csv")

        Console.WriteLine("保存结果到文件：{0}", fi.FullName)

        OutputToFileCSV(Allinks, trees, fi.FullName, rules, root, maxdeep)

        Console.WriteLine("完成")

    End Sub

    ''' <summary>
    ''' 获取链接
    ''' </summary>
    ''' <param name="Allinks">所有链接</param>
    ''' <param name="loader">网页加载器</param>
    ''' <param name="maxdeep">最大深度</param>
    ''' <param name="page">页面信息</param>
    ''' <param name="rules">URL规则</param>
    ''' <param name="DocumentxPath">文档限定Xpath</param>
    ''' <remarks></remarks>
    Private Sub GetPageLinks(Allinks As Dictionary(Of String, PageInfo), rules As HashSet(Of String), DocumentxPath As String,
                             loader As HtmlAgilityPack.HtmlWeb, maxdeep As Integer, page As PageInfo)

        Dim linkey = GetUniquePageLinkHash(page.Url)

        '跳过页面
        '超过深度, 或者已经处理过的
        If page.DeepLevel > maxdeep OrElse (Allinks.ContainsKey(linkey) AndAlso Allinks(linkey).isPrccessed) Then Return

        If Not Allinks.ContainsKey(linkey) Then Allinks(linkey) = page

        For i = 1 To 3
            Console.WriteLine("获取网页（深度：{0}）|{1},重试次数：{2} ...",
                              page.DeepLevel, page.Url, i)


            page.ErrorEX = Nothing

            Try

                Dim doc = loader.Load(page.Url.ToString())



                Dim title = doc.DocumentNode.SelectSingleNode("/html/head/title").InnerText

                page.Title = title

                Allinks(linkey).isPrccessed = True



                Dim linknodes As List(Of HtmlAgilityPack.HtmlNode) = Nothing

                If Not String.IsNullOrWhiteSpace(DocumentxPath) Then

                    Dim parts = doc.DocumentNode.SelectNodes(DocumentxPath)

                    If parts IsNot Nothing Then
                        linknodes = New List(Of HtmlAgilityPack.HtmlNode)

                        For Each p In parts
                            Dim items = p.SelectNodes("//a[@href]")

                            linknodes.AddRange(items)

                        Next
                    End If

                End If

                If linknodes Is Nothing Then

                    Dim nodes = doc.DocumentNode.SelectNodes("//a[@href]")

                    If nodes IsNot Nothing Then linknodes = nodes.ToList()
                End If




                If linknodes IsNot Nothing Then


                    Dim lns = From n In linknodes
                                Let link = WebUtility.HtmlDecode(n.Attributes("href").Value.Trim), text = WebUtility.HtmlDecode(n.InnerText)
                                Where CheckLinkPartString(link)
                                Select New With {Key .Link = link, .Title = text}
                                Distinct



                    Dim links = (From l In lns
                                Let nLink = NormalizedLink(l.Link, page.Url)
                                Where nLink IsNot Nothing AndAlso CheckCrawlerRules(nLink, page, Allinks, rules)
                                Select New PageInfo With {.Title = l.Title,
                                                            .Url = nLink,
                                                            .DeepLevel = page.DeepLevel + 1,
                                                            .Links = New List(Of PageInfo)}).ToList




                    links.ForEach(Sub(link)
                                      Dim key = GetUniquePageLinkHash(link.Url)
                                      If Not Allinks.ContainsKey(key) Then
                                          Allinks.Add(key, link)
                                          page.Links.Add(link)
                                      End If
                                  End Sub)

                    For Each link In page.Links

                        '递归检查链接
                        GetPageLinks(Allinks, rules, DocumentxPath, loader, maxdeep, link)
                    Next




                End If

                Return

            Catch ex As Exception
                Console.ForegroundColor = ConsoleColor.Red
                Console.WriteLine("获取网页（深度：{0}）|{1},遇到错误 {2}",
                              page.DeepLevel, page.Url, ex.ToString())

                Console.ResetColor()

                page.ErrorEX = ex


            End Try

            Dim time = 1000 * i

            Console.WriteLine("休息一会... {0}ms", time)

            Threading.Thread.Sleep(time)


        Next



    End Sub


#Region "URL处理  和链接审查"

    ''' <summary>
    ''' 规范化URL
    ''' </summary>
    ''' <param name="url">要检查的URL</param>
    ''' <param name="baseUrl">基础 Url</param>
    ''' <returns>返回一个合法的URL，如果不合法则返回Nothing</returns>
    ''' <remarks></remarks>
    Private Function NormalizedLink(url As String, baseUrl As Uri) As Uri

        If String.IsNullOrWhiteSpace(url) Then Return Nothing

        Dim link = Uri.UnescapeDataString(url)

        If Uri.IsWellFormedUriString(link, UriKind.Relative) Then
            If baseUrl IsNot Nothing Then
                Return New Uri(baseUrl, link)
            End If

        ElseIf Uri.IsWellFormedUriString(link, UriKind.Absolute) Then

            Dim uri = New Uri(link)

            Dim schemaMatch As Boolean

            If baseUrl IsNot Nothing Then

                schemaMatch = String.Equals(baseUrl.Scheme, uri.Scheme, StringComparison.OrdinalIgnoreCase)

            Else

                schemaMatch = String.Equals(uri.UriSchemeHttp, uri.Scheme, StringComparison.OrdinalIgnoreCase) OrElse
                               String.Equals(uri.UriSchemeHttps, uri.Scheme, StringComparison.OrdinalIgnoreCase)

            End If


            If schemaMatch Then
                Return uri
            End If
        End If

        Return Nothing
    End Function


    ''' <summary>
    ''' 获取页面唯一链接的哈希
    ''' </summary>
    ''' <param name="link">要检查的URL</param>
    ''' <returns>返回一个经过处理的链接，此链接不包含参数。</returns>
    ''' <remarks></remarks>
    Public Function GetUniquePageLinkHash(link As Uri) As String
        If link Is Nothing Then Return String.Empty

        Dim url = link.ToString().ToLower

        Dim index = url.IndexOf("#")

        If index > 0 Then
            url = url.Substring(0, index)
        End If

        Dim hashAlgorithm As New SHA256Managed

        Dim byteValue = System.Text.Encoding.UTF8.GetBytes(url)
        Dim hashValue = hashAlgorithm.ComputeHash(byteValue)
        Dim r = BitConverter.ToString(hashValue).Replace("-", "")
        Return r
    End Function


    ''' <summary>
    ''' 链接爬虫规则检查
    ''' </summary>
    ''' <param name="nLink">要检查链接</param>
    ''' <param name="page">当前页面</param>
    ''' <param name="Allinks">所有链接</param>
    ''' <param name="rules">检查规则</param>
    ''' <returns>返回True代表连接通过，反之表示没有通过</returns>
    ''' <remarks></remarks>
    Private Function CheckCrawlerRules(nLink As Uri, page As PageInfo, Allinks As Dictionary(Of String, PageInfo), rules As HashSet(Of String)) As Boolean

        If nLink Is Nothing Then Return False

        Dim linkey = GetUniquePageLinkHash(nLink)
        '已经处理过的页面排除
        If Allinks.ContainsKey(linkey) AndAlso Allinks(linkey).isPrccessed Then Return False



        For Each r In rules
            If Regex.IsMatch(nLink.ToString(), r, RegexOptions.IgnoreCase) Then
                Return True
            End If
        Next


        Return False
    End Function



    ''' <summary>
    ''' 检查网页里链接内容
    ''' </summary>
    ''' <param name="link"></param>
    ''' <returns></returns>
    ''' <remarks>用于用于过滤链接里的简单错误</remarks>
    Private Function CheckLinkPartString(link As String) As Boolean
        If String.IsNullOrWhiteSpace(link) Then Return False

        Dim temp = link.Trim.ToLower

        Return Not (temp.StartsWith("javascript") OrElse temp.StartsWith("#"))


    End Function

#End Region



#Region "输出到文件"



    ''' <summary>
    ''' 输出到文件
    ''' </summary>
    ''' <param name="Allinks">所有链接</param>
    ''' <param name="trees">生成的树</param>
    ''' <param name="filename">文件名</param>
    ''' <param name="maxdeep">最大深度</param>
    ''' <param name="root">起始地址</param>
    ''' <param name="rules">规则</param>
    ''' <remarks></remarks>
    Private Sub OutputToFileCSV(Allinks As Dictionary(Of String, PageInfo),
                                 trees As HashSet(Of PageInfo),
                                 filename As String,
                                 rules As HashSet(Of String),
                                 root As Uri,
                                 maxdeep As Integer)


        Dim sb As New StringBuilder()
        Dim sw As New StringWriter(sb)


        sw.WriteLine("结果信息")

        sw.WriteLine()
        sw.WriteLine()

        sw.WriteLine("输出文件:, ""{0}""", filename)
        sw.WriteLine("起始地址:, ""{0}""", root.ToString())
        sw.WriteLine("最大深度:, ""{0}""", maxdeep)


        sw.WriteLine("检查规则：")

        For i = 0 To rules.Count - 1
            sw.WriteLine("{0},""{1}""", i + 1, rules(i))
        Next

        sw.WriteLine("生成时间:, ""{0}""", Now)
        sw.WriteLine("最大深度:, ""{0}""", maxdeep)


        sw.WriteLine()
        sw.WriteLine()



        sw.WriteLine("Hash,DeepLevel,Url,Title,isPrccessed,Error")

        For Each kp In Allinks
            Dim item = kp.Value
            sw.WriteLine("""{0}"",""{1}"",""{2}"",""{3}"",""{4}"",""{5}""",
                         kp.Key, item.DeepLevel, item.Url, item.Title, item.isPrccessed, If(item.ErrorEX Is Nothing, String.Empty, item.ErrorEX.ToString))
        Next


        sw.WriteLine("")
     




        My.Computer.FileSystem.WriteAllText(filename, sb.ToString, False, System.Text.Encoding.UTF8)



    End Sub

    ''' <summary>
    ''' 输出到文件
    ''' </summary>
    ''' <param name="Allinks">所有链接</param>
    ''' <param name="trees">生成的树</param>
    ''' <param name="filename">文件名</param>
    ''' <param name="maxdeep">最大深度</param>
    ''' <param name="root">起始地址</param>
    ''' <param name="rules">规则</param>
    ''' <remarks></remarks>
    Private Sub OutputToFileHTML(Allinks As Dictionary(Of String, PageInfo),
                                 trees As HashSet(Of PageInfo),
                                 filename As String,
                                 rules As HashSet(Of String),
                                 root As Uri,
                                 maxdeep As Integer)




        Dim template = My.Resources.ResultsPage


        Dim model = New With {.Allinks = Allinks,
                              .TreeHTML = GetTreeHTML(trees).ToString(),
                              .Filename = filename,
                              .Rules = rules,
                              .Root = root,
                              .Maxdeep = maxdeep}


        RazorHelper.SetRazorForVisualBasic()


        Dim html = Razor.Parse(template, model)

        My.Computer.FileSystem.WriteAllText(filename, html, False, System.Text.Encoding.UTF8)



    End Sub


#Region "生成HTML 列表树内容"
    ''' <summary>
    ''' 生成树内容
    ''' </summary>
    ''' <param name="page"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetTreeHTMLItem(page As PageInfo) As XElement

        If page Is Nothing Then Return Nothing


        Dim xml = <li><a href=<%= page.Url.ToString %>><%= page.Title %></a>
                      <%= GetTreeHTML(page.Links) %>
                  </li>

        Return xml
    End Function

    Private Function GetTreeHTML(list As IEnumerable(Of PageInfo)) As XElement
        If list Is Nothing OrElse Not list.Any Then Return Nothing

        Dim xml = <ol>
                      <%= From item In list
                          Select GetTreeHTMLItem(item)
                      %>
                  </ol>

        Return xml
    End Function



#End Region







#End Region




End Module



