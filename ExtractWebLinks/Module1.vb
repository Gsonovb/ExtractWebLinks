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

            'LoadConfig 加载配置文件

            If Not String.IsNullOrEmpty(options.LoadConfig) AndAlso File.Exists(options.LoadConfig) Then
                options.LoadFromFile(options.LoadConfig)

            End If




            If options.StartUrls Is Nothing OrElse options.StartUrls.Length = 0 Then
                Console.WriteLine("请指定起始地址")

                Console.WriteLine(options.GetUsage())

                Return
            End If

            If options.MaxDeep < 0 Then options.MaxDeep = 0


            Dim check = options.StartUrls.All(Function(x) NormalizedLink(x, Nothing, Nothing) IsNot Nothing)



            Dim startlink = options.StartUrls(0)



            If check Then

                If Not String.IsNullOrEmpty(options.SaveConfig) Then
                    Dim config = New FileInfo(options.SaveConfig)

                    Console.WriteLine("保存参数的到配置文件:{0}。", config.FullName)
                    options.SaveToFile(config.FullName)


                End If



                StartWork(options)
            Else
                Console.WriteLine("输入的URL不符合规范。")

            End If



        End If


    End Sub

    ''' <summary>
    ''' 开始工作
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub StartWork(options As Options)
        'root As Uri,
        Console.WriteLine("正在初始化...")


        Dim Allinks As New Dictionary(Of String, PageInfo)

        Dim trees As New HashSet(Of PageInfo)


        Dim html = New HtmlAgilityPack.HtmlWeb



        Dim rules As New HashSet(Of String)

        If options.Rules.Any Then
            For Each r In options.Rules
                rules.Add(r)
            Next

        End If


        'URL 排除规则，使用正则表达式。
        Dim exUrlRules As New HashSet(Of String)
        If options.exUrlRules IsNot Nothing Then
            For Each r In options.exUrlRules
                exUrlRules.Add(r)
            Next
        End If

        ''Title 排除规则，使用正则表达式。
        Dim exTitleRules As New HashSet(Of String)
        If options.exTitleRules IsNot Nothing Then
            For Each r In options.exTitleRules
                exTitleRules.Add(r)
            Next
        End If



        Dim startUrls = options.StartUrls.Select(Function(url) New PageInfo With {.Title = "",
                                                                                  .Url = NormalizedLink(url, Nothing, Nothing),
                                                                                  .DeepLevel = 0,
                                                                                  .Links = New List(Of PageInfo)}).ToList


        Dim chars = "\^$*+?{}.()".ToList()

        For Each pi In startUrls
            trees.Add(pi)
            Allinks.Add(pi.HashKey, pi)


            If options.Rules Is Nothing Then
                Dim rule = pi.Url.ToString()

                If pi.Url.Segments.Length > 1 Then
                    Dim lastpart = pi.Url.Segments.LastOrDefault

                    If Not lastpart.EndsWith("/"c) Then
                        Dim str = pi.Url.ToString()
                        rule = str.Substring(0, str.LastIndexOf(lastpart))
                    End If
                End If

                chars.ForEach(Sub(c) rule = rule.Replace(c.ToString(), "\" & c.ToString()))

                rules.Add(rule)
            End If
        Next



        Dim xpath = options.XPath

        Dim maxdeep = options.MaxDeep


        'URL 变换
        Dim UrlTransform As New KeyValuePair(Of String, String)(String.Empty, String.Empty)

        If Not String.IsNullOrEmpty(options.Transform) Then
            Dim parts = options.Transform.Split("||")
            If parts IsNot Nothing Then
                If parts.Length >= 2 Then
                    UrlTransform = New KeyValuePair(Of String, String)(parts(0), parts(1))
                Else
                    UrlTransform = New KeyValuePair(Of String, String)(parts(0), String.Empty)
                End If

            End If

        End If



        Console.WriteLine("初始化完成...")


        Console.WriteLine("检查插件...")


        StartUsePlugins(options, Allinks, trees, rules, exUrlRules, exTitleRules, UrlTransform, xpath, maxdeep)



        Console.WriteLine("开始扫描网页...")

        StartScans(options, Allinks, rules, exUrlRules, exTitleRules, UrlTransform, xpath, html, maxdeep)


        Console.WriteLine("扫描网页完成...")


        '输出

        Console.WriteLine("扫描到{0}个网址。",
                          Allinks.Count)


        Dim fi = New IO.FileInfo(options.FileName & ".html")

        Console.WriteLine("保存结果到文件：{0}", fi.FullName)

        OutputToFileHTML(Allinks, trees, fi.FullName, startUrls, rules, exUrlRules, exTitleRules, UrlTransform, maxdeep, options)


        fi = New IO.FileInfo(options.FileName & ".csv")

        Console.WriteLine("保存结果到文件：{0}", fi.FullName)

        OutputToFileCSV(Allinks, trees, fi.FullName, startUrls, rules, exUrlRules, exTitleRules, UrlTransform, maxdeep, options)

        Console.WriteLine("完成")

    End Sub


#Region "插件处理"

    ''' <summary>
    ''' 使用插件
    ''' </summary>
    ''' <param name="options">参数</param>
    ''' <param name="Allinks">所有链接</param>
    ''' <param name="trees">书</param>
    ''' <param name="rules">规则</param>
    ''' <param name="exUrlRules">排除URL规则</param>
    ''' <param name="exTitleRules">排除标题规则</param>
    ''' <param name="UrlTransform">URL变换</param>
    ''' <param name="xpath"></param>
    ''' <param name="maxdeep">最大深度</param>
    ''' <remarks></remarks>
    Private Sub StartUsePlugins(options As Options,
                                Allinks As Dictionary(Of String, PageInfo),
                                trees As HashSet(Of PageInfo),
                                rules As HashSet(Of String),
                                exUrlRules As HashSet(Of String),
                                exTitleRules As HashSet(Of String),
                                UrlTransform As KeyValuePair(Of String, String),
                                xpath As String,
                                maxdeep As Integer)


        Dim manger = PluginManger.Default


        If manger.LinksDiscovery IsNot Nothing AndAlso manger.LinksDiscovery.Any Then
            Console.WriteLine("找到 {0} 插件.", manger.LinksDiscovery.Count)

            For Each p In manger.LinksDiscovery
                Console.WriteLine("插件:{0}:{1}  Match:{2}", p.Metadata.Name, p.Metadata.Description, p.Metadata.UrlMatchRule)
            Next


            For Each pi In trees
                Dim link = pi.Url.ToString()
                Dim plugin = manger.LinksDiscovery.FirstOrDefault(Function(item) Regex.IsMatch(link, item.Metadata.UrlMatchRule, RegexOptions.IgnoreCase))

                If plugin IsNot Nothing AndAlso plugin.Value.CanProcess(pi.Url) Then

                    Console.WriteLine("使用插件:{0}", plugin.Metadata.Name)

                    Dim links = plugin.Value.DiscoverLinks(pi.Url, xpath, maxdeep)


                    For Each item In links

                        '递归处理
                        ScanLinks(options, Allinks, rules, exUrlRules, exTitleRules, UrlTransform, xpath, maxdeep, item, pi)
                    Next
                End If
            Next

            Console.WriteLine("插件处理完成...")
        Else
            Console.WriteLine("未找到任何插件...")
        End If

    End Sub

    ''' <summary>
    ''' 扫描外部插件获取到的链接
    ''' </summary>
    ''' <param name="options">参数</param>
    ''' <param name="Allinks">所有页面</param>
    ''' <param name="rules">规则</param>
    ''' <param name="excludeTitleRules">Title排除规则</param>
    ''' <param name="excludeUrlRules">URL排除规则</param>
    ''' <param name="UrlTransform">URL规则</param>
    ''' <param name="xpath">文档Xpath</param>
    ''' <param name="maxdeep">最大深度</param>
    ''' <param name="link">链接信息</param>
    ''' <param name="page">上级页面</param>
    ''' <remarks></remarks>
    Private Sub ScanLinks(options As Options,
                          Allinks As Dictionary(Of String, PageInfo),
                          rules As HashSet(Of String),
                          excludeUrlRules As HashSet(Of String),
                          excludeTitleRules As HashSet(Of String),
                          UrlTransform As KeyValuePair(Of String, String),
                          xpath As String,
                          maxdeep As Integer,
                          link As Common.LinkInfo,
                          page As PageInfo)

        '链接不正确
        If Not CheckLinkPartString(link.Url) Then Return



        Dim nLink = NormalizedLink(link.Url, Nothing, UrlTransform)

        If nLink IsNot Nothing AndAlso CheckCrawlerRules(nLink, page, Allinks, rules, excludeUrlRules) AndAlso CheckTitleRules(link.Title, excludeTitleRules) Then
            Dim newpage = New PageInfo With {.Title = link.Title,
                                                .Url = nLink,
                                                .DeepLevel = page.DeepLevel + 1,
                                                .Links = New List(Of PageInfo),
                                               .Parent = page}

            Dim key = newpage.HashKey
            If Not Allinks.ContainsKey(key) Then
                Allinks.Add(key, newpage)

                page.Links.Add(newpage)

            Else
                '如果 已经添加
                newpage = Allinks(key)

            End If

            For Each item In link.Links
                ScanLinks(options, Allinks, rules, excludeUrlRules, excludeTitleRules, UrlTransform, xpath, maxdeep, item, newpage)
            Next

        End If


    End Sub


#End Region


#Region "页面处理"



    ''' <summary>
    ''' 扫描网页
    ''' </summary>
    ''' <param name="options">参数</param>
    ''' <param name="Allinks">所有页面</param>
    ''' <param name="rules">规则</param>
    ''' <param name="exTitleRules">Title排除规则</param>
    ''' <param name="exUrlRules">URL排除规则</param>
    ''' <param name="UrlTransform">URL规则</param>
    ''' <param name="xpath">文档Xpath</param>
    ''' <param name="html">网页加载器</param>
    ''' <param name="maxdeep">最大深度</param>
    ''' <remarks></remarks>
    Private Sub StartScans(options As Options,
                           Allinks As Dictionary(Of String, PageInfo),
                           rules As HashSet(Of String), exUrlRules As HashSet(Of String),
                           exTitleRules As HashSet(Of String),
                           UrlTransform As KeyValuePair(Of String, String),
                           xpath As String,
                           html As HtmlAgilityPack.HtmlWeb,
                           maxdeep As Integer)

        Do

            Dim todo = Allinks.Values.Where(Function(pi) Not pi.isPrccessed AndAlso pi.DeepLevel <= maxdeep).ToList

            For Each pi In todo
                GetPageLinks(Allinks, rules, exUrlRules, exTitleRules, UrlTransform, xpath, html, maxdeep, pi)
            Next


        Loop While Allinks.Values.Where(Function(pi) Not pi.isPrccessed AndAlso pi.DeepLevel <= maxdeep).Any


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
    Private Sub GetPageLinks(Allinks As Dictionary(Of String, PageInfo),
                             rules As HashSet(Of String),
                             excludeUrlRules As HashSet(Of String),
                             excludeTitleRules As HashSet(Of String),
                             UrlTransform As KeyValuePair(Of String, String),
                             DocumentxPath As String,
                             loader As HtmlAgilityPack.HtmlWeb,
                             maxdeep As Integer, page As PageInfo)

        Dim linkey = page.HashKey

        '跳过页面
        '超过深度, 或者已经处理过的
        If page.DeepLevel > maxdeep OrElse (Allinks.ContainsKey(linkey) AndAlso Allinks(linkey).isPrccessed) Then Return


        If page.isDeleted Then
            DeletePage(Allinks, page)
            Return
        End If

        If Not Allinks.ContainsKey(linkey) Then Return



        For i = 1 To 3
            Console.WriteLine("获取网页（深度：{0}）| {1},重试次数：{2} ...",
                              page.DeepLevel, page.Url, i)


            page.ErrorEX = Nothing

            Try

                Dim doc = loader.Load(page.Url.ToString())


                Dim title = doc.DocumentNode.SelectSingleNode("/html/head/title").InnerText.Trim

                '名字检查 
                page.Title = title

                Allinks(linkey).isPrccessed = True

                If Not CheckTitleRules(title, excludeTitleRules) Then
                    DeletePage(Allinks, page)
                    Return
                End If


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

                    '名字检查 

                    Dim links = (From l In lns
                                Let nLink = NormalizedLink(l.Link, page.Url, UrlTransform)
                                Where nLink IsNot Nothing AndAlso CheckCrawlerRules(nLink, page, Allinks, rules, excludeUrlRules) AndAlso CheckTitleRules(l.Title, excludeTitleRules)
                                Select New PageInfo With {.Title = l.Title,
                                                            .Url = nLink,
                                                            .DeepLevel = page.DeepLevel + 1,
                                                            .Links = New List(Of PageInfo),
                                                            .Parent = page}).ToList


                    links.ForEach(Sub(link)
                                      Dim key = link.HashKey
                                      If Not Allinks.ContainsKey(key) Then
                                          Allinks.Add(key, link)
                                          page.Links.Add(link)
                                      End If
                                  End Sub)


                    Dim linksclone = page.Links.ToList

                    For Each link In linksclone

                        '递归检查链接
                        GetPageLinks(Allinks, rules, excludeUrlRules, excludeTitleRules, UrlTransform, DocumentxPath, loader, maxdeep, link)
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



    ''' <summary>
    ''' 删除页面
    ''' </summary>
    ''' <param name="Allinks">所有链接</param>
    ''' <param name="page">要删除的页面</param>
    ''' <remarks></remarks>
    Private Sub DeletePage(Allinks As Dictionary(Of String, PageInfo), page As PageInfo)
        page.isDeleted = True

        Allinks.Remove(page.HashKey)

        Dim clone = page.Links.ToList

        clone.ForEach(Sub(item)
                          DeletePage(Allinks, item)
                      End Sub)

        If page.Parent IsNot Nothing Then
            page.Parent.Links.Remove(page)
        End If
    End Sub



#End Region



#Region "URL处理  和链接审查"

    ''' <summary>
    ''' 规范化URL
    ''' </summary>
    ''' <param name="url">要检查的URL</param>
    ''' <param name="baseUrl">基础 Url</param>
    ''' <param name="UrlTransform">URL变换</param>
    ''' <returns>返回一个合法的URL，如果不合法则返回Nothing</returns>
    ''' <remarks></remarks>
    Private Function NormalizedLink(url As String, baseUrl As Uri, UrlTransform As Nullable(Of KeyValuePair(Of String, String))) As Uri

        If String.IsNullOrWhiteSpace(url) Then Return Nothing

        Dim link = Uri.UnescapeDataString(url)

        Dim result As Uri


        If Uri.IsWellFormedUriString(link, UriKind.Relative) Then
            If baseUrl IsNot Nothing Then
                result = New Uri(baseUrl, link)
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
                result = uri
            End If
        End If


        If result IsNot Nothing AndAlso UrlTransform IsNot Nothing AndAlso Not String.IsNullOrEmpty(UrlTransform.Value.Key) Then
            Dim temp = result.ToString()
            If Regex.IsMatch(temp, UrlTransform.Value.Key) Then
                Dim t2 = Regex.Replace(temp, UrlTransform.Value.Key, UrlTransform.Value.Value)
                result = New Uri(t2)
            End If
        End If


        Return result
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
    ''' 检查Title  排除规则
    ''' </summary>
    ''' <param name="title">标题名字</param>
    ''' <param name="rules">排除规则</param>
    ''' <returns>如过标题中包含规则内容，返回false，否则返回True</returns>
    ''' <remarks></remarks>
    Private Function CheckTitleRules(title As String, rules As HashSet(Of String)) As Boolean

        For Each r In rules
            If Regex.IsMatch(title, r, RegexOptions.IgnoreCase) Then
                Return False
            End If
        Next


        Return True
    End Function




    ''' <summary>
    ''' 链接爬虫规则检查
    ''' </summary>
    ''' <param name="nLink">要检查链接</param>
    ''' <param name="page">当前页面</param>
    ''' <param name="Allinks">所有链接</param>
    ''' <param name="rules">检查规则</param>
    ''' <param name="excludeUrlRules"></param>
    ''' <returns>返回True代表连接通过，反之表示没有通过</returns>
    ''' <remarks></remarks>
    Private Function CheckCrawlerRules(nLink As Uri,
                                       page As PageInfo,
                                       Allinks As Dictionary(Of String, PageInfo),
                                       rules As HashSet(Of String),
                                       excludeUrlRules As HashSet(Of String)) As Boolean

        If nLink Is Nothing Then Return False

        Dim linkstr = nLink.ToString()
        Dim pagelinkstr = page.Url.ToString

        For Each r In excludeUrlRules
            If Regex.IsMatch(linkstr, r, RegexOptions.IgnoreCase) Then
                Return False
            End If
            If Regex.IsMatch(pagelinkstr, r, RegexOptions.IgnoreCase) Then
                Return False
            End If
        Next


        Dim linkey = GetUniquePageLinkHash(nLink)
        '已经处理过的页面排除
        If Allinks.ContainsKey(linkey) AndAlso Allinks(linkey).isPrccessed Then Return False


        For Each r In rules
            If Regex.IsMatch(linkstr, r, RegexOptions.IgnoreCase) Then
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
    ''' <param name="UrlTransform">URL变换</param>
    ''' <param name="startUrls">起始地址</param>
    ''' <param name="rules">规则</param>
    ''' <param name="exTitleRules">Title排除规则</param>
    ''' <param name="exUrlRules">URL排除规则</param>
    ''' <param name="options">参数</param>
    ''' <remarks></remarks>
    Private Sub OutputToFileCSV(Allinks As Dictionary(Of String, PageInfo),
                                 trees As HashSet(Of PageInfo),
                                 filename As String,
                                 startUrls As List(Of PageInfo),
                                 rules As HashSet(Of String),
                                 exUrlRules As HashSet(Of String),
                                 exTitleRules As HashSet(Of String),
                                 UrlTransform As KeyValuePair(Of String, String),
                                 maxdeep As Integer,
                                 options As Options)


        Dim sb As New StringBuilder()
        Dim sw As New StringWriter(sb)


        sw.WriteLine("结果信息")

        sw.WriteLine()
        sw.WriteLine()
        sw.WriteLine("生成时间:, ""{0}""", Now)

        sw.WriteLine()
        sw.WriteLine()


        sw.WriteLine("找到连接数:, ""{0}""", Allinks.Count)

        sw.WriteLine()
        sw.WriteLine()



        sw.WriteLine("输出文件:, ""{0}""", filename)
        sw.WriteLine("起始地址:")
        For i = 0 To startUrls.Count - 1
            sw.WriteLine("{0},""{1}""", i + 1, startUrls(i).ToString)
        Next




        sw.WriteLine()

        sw.WriteLine("检查规则：")

        For i = 0 To rules.Count - 1
            sw.WriteLine("{0},""{1}""", i + 1, rules(i))
        Next

        sw.WriteLine()


        sw.WriteLine("URL排除规则：")

        For i = 0 To exUrlRules.Count - 1
            sw.WriteLine("{0},""{1}""", i + 1, exUrlRules(i))
        Next

        sw.WriteLine()


        sw.WriteLine("Title排除规则：")

        For i = 0 To exTitleRules.Count - 1
            sw.WriteLine("{0},""{1}""", i + 1, exTitleRules(i))
        Next

        sw.WriteLine()
        sw.WriteLine()


        sw.WriteLine("最大深度:, ""{0}""", maxdeep)

        sw.WriteLine("URL变换:, ""{0}""", UrlTransform.ToString())



        sw.WriteLine()
        sw.WriteLine()

        Dim manger = PluginManger.Default

        If manger.LinksDiscovery IsNot Nothing Then
            sw.WriteLine("找到 {0} 插件.", manger.LinksDiscovery.Count)

            For Each p In manger.LinksDiscovery
                sw.WriteLine("插件:{0}:{1}  Match:{2}", p.Metadata.Name, p.Metadata.Description, p.Metadata.UrlMatchRule)
            Next
        End If



        sw.WriteLine("====================================")
        sw.WriteLine("options:")

        With options


            sw.WriteLine("URL变换:, ""{0}""", .Transform)
            sw.WriteLine("XPath:, ""{0}""", .XPath)
            sw.WriteLine("最大深度:, ""{0}""", .MaxDeep)
            sw.WriteLine("输出文件:, ""{0}""", .FileName)
            sw.WriteLine("加载配置文件:, ""{0}""", .LoadConfig)
            sw.WriteLine("保存配置文件:, ""{0}""", .SaveConfig)

            sw.WriteLine("起始地址:")

            If .StartUrls IsNot Nothing Then
                For i = 0 To .StartUrls.Count - 1
                    sw.WriteLine("{0},""{1}""", i + 1, .StartUrls(i))
                Next
            End If


            sw.WriteLine("检查规则:")
            If .Rules IsNot Nothing Then
                For i = 0 To .Rules.Count - 1
                    sw.WriteLine("{0},""{1}""", i + 1, .Rules(i))
                Next
            End If


            sw.WriteLine("Title排除规则:")
            If .exTitleRules IsNot Nothing Then
                For i = 0 To .exTitleRules.Count - 1
                    sw.WriteLine("{0},""{1}""", i + 1, .exTitleRules(i))
                Next
            End If


            sw.WriteLine("URL排除规则")
            If .exUrlRules IsNot Nothing Then
                For i = 0 To .exUrlRules.Count - 1
                    sw.WriteLine("{0},""{1}""", i + 1, .exUrlRules(i))
                Next
            End If




        End With

        sw.WriteLine("====================================")

        sw.WriteLine()

        sw.WriteLine("Hash,DeepLevel,Url,Title,isPrccessed,Error")

        For Each kp In Allinks
            Dim item = kp.Value
            sw.WriteLine("""{0}"",""{1}"",""{2}"",""{3}"",""{4}"",""{5}""",
                         kp.Key, item.DeepLevel, item.Url, item.Title, item.isPrccessed, If(item.ErrorEX Is Nothing, String.Empty, item.ErrorEX.ToString))
        Next


        sw.WriteLine()





        My.Computer.FileSystem.WriteAllText(filename, sb.ToString, False, System.Text.Encoding.UTF8)



    End Sub



    ''' <summary>
    ''' 输出到文件
    ''' </summary>
    ''' <param name="Allinks">所有链接</param>
    ''' <param name="trees">生成的树</param>
    ''' <param name="filename">文件名</param>
    ''' <param name="maxdeep">最大深度</param>
    ''' <param name="UrlTransform">URL变换</param>
    ''' <param name="startUrls">起始地址</param>
    ''' <param name="rules">规则</param>
    ''' <param name="exTitleRules">Title排除规则</param>
    ''' <param name="exUrlRules">URL排除规则</param>
    ''' <param name="options">参数</param>
    ''' <remarks></remarks>
    Private Sub OutputToFileHTML(Allinks As Dictionary(Of String, PageInfo),
                                 trees As HashSet(Of PageInfo),
                                 filename As String,
                                 startUrls As List(Of PageInfo),
                                 rules As HashSet(Of String),
                                 exUrlRules As HashSet(Of String),
                                 exTitleRules As HashSet(Of String),
                                 UrlTransform As KeyValuePair(Of String, String),
                                 maxdeep As Integer,
                                 options As Options)


        Dim template = My.Resources.ResultsPage


        Dim model = New With {.Allinks = Allinks,
                              .TreeHTML = GetTreeHTML(trees).ToString(),
                              .Filename = filename,
                              .Rules = rules,
                              .exUrlRules = exUrlRules,
                              .exTitleRules = exTitleRules,
                              .Transform = UrlTransform,
                              .Urls = startUrls,
                              .Maxdeep = maxdeep,
                              .Options = options}


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
                          Where Not item.isDeleted
                          Select GetTreeHTMLItem(item)
                      %>
                  </ol>

        Return xml
    End Function



#End Region







#End Region




End Module



