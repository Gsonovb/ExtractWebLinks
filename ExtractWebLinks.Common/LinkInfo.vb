
''' <summary>
''' 链接信息
''' </summary>
''' <remarks></remarks>
Public Class LinkInfo



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
    Public Property Url As String
 

    ''' <summary>
    ''' 链接
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Links As New List(Of LinkInfo)

    ''' <summary>
    ''' 深度级别
    ''' </summary>
    Public Property DeepLevel As Integer



End Class
