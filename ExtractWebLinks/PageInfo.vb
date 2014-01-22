

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
    ''' Url
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Url As Uri

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





    Public Overrides Function Equals(obj As Object) As Boolean
        Dim t = TryCast(obj, PageInfo)
        If t Is Nothing Then
            Return Me.Url.Equals(t.Url)
        End If
        Return False
    End Function

    Public Overrides Function GetHashCode() As Integer
        Return Me.Url.GetHashCode()
    End Function

    Public Overrides Function ToString() As String
        Return Me.Title & "|" & Me.Url.ToString()
    End Function


End Class
