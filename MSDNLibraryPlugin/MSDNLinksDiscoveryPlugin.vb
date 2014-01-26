Imports ExtractWebLinks.Common
Imports System.Text.RegularExpressions
Imports System.Net
Imports Newtonsoft.Json.Linq




<LinksDiscoveryPlugin("MSDN 目录处理插件", "", MSDNLinksDiscoveryPlugin.Rule)>
Public Class MSDNLinksDiscoveryPlugin
    Implements ILinksDiscoveryPlugin




    Public Const Rule = "http://msdn.microsoft.com(.*)\.aspx"




    Public Function CanProcess(url As Uri) As Boolean Implements ILinksDiscoveryPlugin.CanProcess
        Return Regex.IsMatch(url.ToString(), Rule, Text.RegularExpressions.RegexOptions.IgnoreCase)

    End Function

    Public Function DiscoverLinks(Url As Uri, xpath As String, maxdeep As Integer) As IEnumerable(Of LinkInfo) Implements ILinksDiscoveryPlugin.DiscoverLinks

        Dim list As New List(Of LinkInfo)

        If CanProcess(Url) Then

            GetPageTOCNode(list, Url.ToString, xpath, maxdeep, Nothing)

        End If

        Return list

    End Function


    Dim webclient As New WebClient With {.Encoding = Text.Encoding.UTF8}


    ''' <summary>
    ''' 获取TOC节点
    ''' </summary>
    ''' <param name="list">返回的结果集合</param>
    ''' <param name="url">要检查TOC的链接</param>
    ''' <param name="xpath"></param>
    ''' <param name="maxdeep">最大深度</param>
    ''' <param name="link">当前页面</param>
    ''' <remarks></remarks>
    Private Sub GetPageTOCNode(list As List(Of LinkInfo), url As String, xpath As String, maxdeep As Integer, link As LinkInfo)



        If Not My.Settings.SkipDeepChkeck AndAlso link IsNot Nothing AndAlso link.DeepLevel > maxdeep Then Return


        Dim tocuri = Regex.Replace(url, Rule, "$0?toc=1", RegexOptions.IgnoreCase)

        If String.IsNullOrEmpty(tocuri) Then Return


        Console.WriteLine("获取链接 {0}", url)

        Dim json = webclient.DownloadString(tocuri)


        If String.IsNullOrEmpty(json) Then Return

        Dim arrary = JArray.Parse(json)


        If arrary Is Nothing Then Return


        Console.WriteLine("找到 {0} 个链接", arrary.Count)


        Dim count = arrary.Count - 1

        For i = 0 To count

            Dim titlePath = String.Format("[{0}].Title", i)
            Dim hrefPath = String.Format("[{0}].Href", i)
            Dim subtreePath = String.Format("[{0}].ExtendedAttributes.data-tochassubtree", i)


            Try



                Dim Title = arrary.SelectToken(titlePath).Value(Of String)()
                Dim Href = arrary.SelectToken(hrefPath).Value(Of String)()
                Dim subtree = arrary.SelectToken(subtreePath).Value(Of Boolean)()


                If Not String.IsNullOrEmpty(Title) AndAlso Not String.IsNullOrEmpty(Href) Then

                    Dim newlink = "http://msdn.microsoft.com" & Href

                    Console.WriteLine("找到链接： Title:{0}  Url:{1}  hasSubs:{2}",
                                      Title, newlink, subtree)



                    Dim li = New LinkInfo() With {.Url = newlink,
                                                  .Title = Title,
                                                  .DeepLevel = If(link IsNot Nothing, link.DeepLevel + 1, 1)}


                    If link Is Nothing Then
                        list.Add(li)
                    Else
                        link.Links.Add(li)
                    End If


                    If subtree Then
                        GetPageTOCNode(list, newlink, xpath, maxdeep, li)
                    End If


                End If

            Catch ex As Exception
                Console.WriteLine(ex.ToString)
            End Try

        Next




    End Sub


End Class
