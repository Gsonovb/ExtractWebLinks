
''' <summary>
''' 链接探索器元数据
''' </summary>
''' <remarks></remarks>
Public Interface ILinksDiscoveryPluginMetadata


    ''' <summary>
    ''' 名称
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ReadOnly Property Name As String


    ''' <summary>
    ''' 描述
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ReadOnly Property Description As String


    ''' <summary>
    ''' 匹配正则表达式
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ReadOnly Property UrlMatchRule As String



End Interface
