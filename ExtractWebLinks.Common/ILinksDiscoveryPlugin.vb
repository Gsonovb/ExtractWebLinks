

''' <summary>
''' 链接探索器
''' </summary>
''' <remarks></remarks>
Public Interface ILinksDiscoveryPlugin


    ''' <summary>
    ''' 是否可以处理指定的链接
    ''' </summary>
    ''' <param name="url">要处理的链接地址</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function CanProcess(url As Uri) As Boolean




    ''' <summary>
    ''' 探索链接
    ''' </summary>
    ''' <param name="Url">要探索链接</param>
    ''' <param name="xpath">限定文档区域</param>
    ''' <param name="maxdeep">最大深度</param>
    ''' <returns>返回找到的链接枚举</returns>
    ''' <remarks></remarks>
    Function DiscoverLinks(Url As Uri,
                             xpath As String,
                             maxdeep As Integer) As IEnumerable(Of LinkInfo)





End Interface
