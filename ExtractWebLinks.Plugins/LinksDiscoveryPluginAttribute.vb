Imports System.ComponentModel.Composition
Imports ExtractWebLinks.Common


<MetadataAttribute()>
   <AttributeUsage(AttributeTargets.Class, AllowMultiple:=False)>
Public Class LinksDiscoveryPluginAttribute
    Inherits ExportAttribute
   

    ''' <summary>
    ''' 创建新的实例
    ''' </summary>
    ''' <param name="name">名称</param>
    ''' <param name="description">描述</param>
    ''' <param name="urlMatchRule">匹配规则</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal name As String, description As String, urlMatchRule As String)
        MyBase.New(GetType(ILinksDiscoveryPlugin))


        Me.Name = name
        Me.Description = description
        Me.UrlMatchRule = urlMatchRule



    End Sub




    ''' <summary>
    ''' 插件的描述
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Description As String

    ''' <summary>
    ''' 可以处理页面规则
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property UrlMatchRule As String

    ''' <summary>
    ''' 名称
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Name As String






End Class
