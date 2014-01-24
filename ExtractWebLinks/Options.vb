Imports CommandLine
Imports CommandLine.Text
Imports <xmlns="">


''' <summary>
''' 命令行 参数
''' </summary>
''' <remarks></remarks>
Public Class Options


    ''' <summary>
    ''' 起始地址
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <OptionArray("u"c, "url", Required:=False, HelpText:="要获取链接的起始的URL地址。")>
    Public Property StartUrls As String()


    ''' <summary>
    ''' 最大深度
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <CommandLine.Option("d"c, "maxdeep", defaultValue:=1, Required:=False, HelpText:="最大深度")>
    Public Property MaxDeep As Integer




    ''' <summary>
    ''' 文件名
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <CommandLine.Option("o"c, "filename", DefaultValue:="output", HelpText:="输出文件名")>
    Public Property FileName As String




    ''' <summary>
    ''' Xpath
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <CommandLine.Option("x"c, "xpath", HelpText:="提取页面部分内容的链接")>
    Public Property XPath As String




    ''' <summary>
    ''' 检查规则
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <OptionArray("r"c, "rules", HelpText:="URL检查规则，使用正则表达式。")>
    Public Property Rules As String()


    ''' <summary>
    ''' URL 排除规则
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <OptionArray("exUrlRules", HelpText:="URL 排除规则，使用正则表达式。")>
    Public Property exUrlRules As String()


    ''' <summary>
    ''' Title 排除规则
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <OptionArray("exTitleRules", HelpText:="Title 排除规则，使用正则表达式。")>
    Public Property exTitleRules As String()



    ''' <summary>
    ''' URL变换
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <CommandLine.Option("t"c, "transform", HelpText:="URL变换")>
    Public Property Transform As String


    ''' <summary>
    ''' 保存配置文件
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <CommandLine.Option("s"c, "save", HelpText:="保存配置到文件")>
    Public Property SaveConfig As String


    ''' <summary>
    ''' 加载配置文件
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <CommandLine.Option("l"c, "load", HelpText:="加载配置文件")>
    Public Property LoadConfig As String



    ''' <summary>
    ''' 保存配置文件
    ''' </summary>
    Public Sub SaveToFile(filename As String)


        If Me.StartUrls Is Nothing Then Me.StartUrls = Enumerable.Empty(Of String).ToArray()
        If Me.Rules Is Nothing Then Me.Rules = Enumerable.Empty(Of String).ToArray()
        If Me.exTitleRules Is Nothing Then Me.exTitleRules = Enumerable.Empty(Of String).ToArray()
        If Me.exUrlRules Is Nothing Then Me.exUrlRules = Enumerable.Empty(Of String).ToArray()


        Dim xml = <settings maxdeep=<%= Me.MaxDeep %> outputfilename=<%= Me.FileName %> xpath=<%= Me.XPath %> urltransform=<%= Me.Transform %>>
                      <Links>
                          <%= From l In Me.StartUrls
                              Select <link url=<%= l %>/>
                          %>
                      </Links>
                      <Rules>
                          <%= From r In Me.Rules
                              Select <Rule value=<%= r %>/>
                          %>
                      </Rules>
                      <ExcludeTitleRules>
                          <%= From r In Me.exTitleRules
                              Select <Rule value=<%= r %>/>
                          %>
                      </ExcludeTitleRules>
                      <ExcludeUrlRules>
                          <%= From r In Me.exUrlRules
                              Select <Rule value=<%= r %>/>
                          %>
                      </ExcludeUrlRules>
                  </settings>


        xml.Save(filename)

    End Sub

    ''' <summary>
    ''' 加载配置文件
    ''' </summary>
    Public Sub LoadFromFile(filename As String)

        Dim xml = XDocument.Load(filename)

        Dim settings = xml.<settings>.First

        If settings Is Nothing Then Throw New ArgumentException("Config File not have <settings>.")

        Dim deep As Integer

        If Not String.IsNullOrEmpty(settings.@maxdeep) AndAlso Integer.TryParse(settings.@maxdeep, deep) Then
            Me.MaxDeep = deep
        End If


        Me.FileName = settings.@outputfilename
        Me.XPath = settings.@xpath
        Me.Transform = settings.@urltransform



        Dim urls = From link In settings.<Links>.<link>
                   Select link.@url

        Me.StartUrls = urls.ToArray


        Dim rules = From rule In settings.<Rules>.<Rule> Select rule.@value


        Me.Rules = rules.ToArray


        Dim ExcludeTitleRules = From rule In settings.<ExcludeTitleRules>.<Rule> Select rule.@value

        Me.exTitleRules = ExcludeTitleRules.ToArray

        Dim ExcludeUrlRules = From rule In settings.<ExcludeUrlRules>.<Rule> Select rule.@value


        Me.exUrlRules = ExcludeUrlRules.ToArray



    End Sub



    <HelpOption()>
    Public Function GetUsage() As String

        Dim help = New HelpText(New SChineseSentenceBuilder) With {.Heading = New HeadingInfo(My.Application.Info.Title, My.Application.Info.Version.ToString()),
                                                                  .Copyright = New CopyrightInfo("GSonOVB", 2014),
                                                                  .AdditionalNewLineAfterOption = True,
                                                                  .AddDashesToOption = True
                                                                 }

        help.AddPreOptionsLine("   ExtractWebLinks  (-u|--url ""http://www.google.com"" ) (-d|--maxdeep 1) " &
                               "[-o|--filename ""output""] [-x|--xpath ""//*[@id='mainBody']"" ] " &
                               "[-t|--transform ""(\d*)||-""  ]" &
                               "[-r|--rules ""(\w*)"" ""(\d*)"" ]" & "[-exUrlRules ""(\d*)"" ] " &
                               "[-exTitleRules ""(\d*)"" ]" &
                               "[-s|--save ""start.config"" ]" & "[-l|--load ""start.config""  ] ")
        help.AddOptions(Me)
        Return help
    End Function


End Class
