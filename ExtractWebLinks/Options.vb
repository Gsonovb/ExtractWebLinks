Imports CommandLine
Imports CommandLine.Text


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
    <CommandLine.Option("u"c, "url", Required:=True, HelpText:="要获取链接的起始的URL地址。")>
    Public Property StartUrl As String


    ''' <summary>
    ''' 最大深度
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <CommandLine.Option("d"c, "maxdeep", defaultValue:=1, Required:=True, HelpText:="最大深度")>
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


    <HelpOption()>
    Public Function GetUsage() As String

        Dim help = New HelpText(New SChineseSentenceBuilder) With {.Heading = New HeadingInfo(My.Application.Info.Title, My.Application.Info.Version.ToString()),
                                                                  .Copyright = New CopyrightInfo("GSonOVB", 2014),
                                                                  .AdditionalNewLineAfterOption = True,
                                                                  .AddDashesToOption = True
                                                                 }

        help.AddPreOptionsLine("  ExtractWebLinks  (-u|--url ""http://www.google.com"" ) (-d|--maxdeep 1) [-o|--filename ""output""] [-x|--xpath ""//*[@id=""mainBody""]"" ][-r|--rules ""(\w*)"" ""(\d*)"" ]")
        help.AddOptions(Me)
        Return help
    End Function


End Class
