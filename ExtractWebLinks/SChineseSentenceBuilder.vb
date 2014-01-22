Imports CommandLine.Text

''' <summary>
''' 中文帮助
''' </summary>
''' <remarks></remarks>
Public Class SChineseSentenceBuilder
    Inherits BaseSentenceBuilder



    Public Overrides ReadOnly Property AndWord As String
        Get
            Return "和"
        End Get
    End Property

    Public Overrides ReadOnly Property ErrorsHeadingText As String
        Get
            Return "错误："
        End Get
    End Property

    Public Overrides ReadOnly Property OptionWord As String
        Get
            Return "可选"
        End Get
    End Property

    Public Overrides ReadOnly Property RequiredOptionMissingText As String
        Get
            Return "必须选项没有填写"
        End Get
    End Property

    Public Overrides ReadOnly Property ViolatesFormatText As String
        Get
            Return "格式错误"
        End Get
    End Property

    Public Overrides ReadOnly Property ViolatesMutualExclusivenessText As String
        Get
            Return "违反互斥选项"
        End Get
    End Property
End Class
