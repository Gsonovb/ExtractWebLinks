

''' <summary>
''' 页面信息
''' </summary>
''' <remarks></remarks>
Public Class PageInfo

    ''' <summary>
    ''' 标题
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Title As String




    ''' <summary>
    ''' 上级
    ''' </summary>
    Public Property Parent As PageInfo


    ''' <summary>
    ''' 标记 是否删除
    ''' </summary>
    Public Property isDeleted As Boolean



    ''' <summary>
    ''' URL 的Hash 
    ''' </summary>
    ''' <remarks></remarks>
    Public ReadOnly Property HashKey As String
        Get
            If String.IsNullOrEmpty(_urlHash) Then
                Me._urlHash = GetUniquePageLinkHash(Me.Url)
            End If

            Return Me._urlHash
        End Get
    End Property

    Dim _urlHash As String

    Dim _url As Uri

    ''' <summary>
    ''' Url
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Url As Uri
        Get
            Return _url
        End Get
        Set(value As Uri)
            If value IsNot Nothing Then
                Me._url = value
                Me._urlHash = GetUniquePageLinkHash(Me.Url)
            End If
        End Set
    End Property

    ''' <summary>
    ''' 链接
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Links As List(Of PageInfo)

    ''' <summary>
    ''' 深度级别
    ''' </summary>
    Public Property DeepLevel As Integer




    ''' <summary>
    ''' 已处理
    ''' </summary>
    Public Property isPrccessed As Boolean


    ''' <summary>
    ''' 异常信息
    ''' </summary>
    Public Property ErrorEX As Exception




    Public Overrides Function ToString() As String
        Return Me.Title & "|" & Me.Url.ToString()
    End Function


End Class
