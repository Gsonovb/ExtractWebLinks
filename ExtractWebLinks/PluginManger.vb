Imports System.ComponentModel.Composition
Imports System.ComponentModel.Composition.Hosting
Imports ExtractWebLinks.Common




''' <summary>
''' 扩展管理器
''' </summary>
''' <remarks></remarks>
Public Class PluginManger

    Dim _container As CompositionContainer


    Public Sub New()
        'An aggregate catalog that combines multiple catalogs
        Dim catalog = New AggregateCatalog()

        'Adds all the parts found in the same assembly as the Program class
        catalog.Catalogs.Add(New AssemblyCatalog(GetType(PluginManger).Assembly))

        catalog.Catalogs.Add(New DirectoryCatalog(My.Application.Info.DirectoryPath))


        'Create the CompositionContainer with the parts in the catalog
        _container = New CompositionContainer(catalog)

        'Fill the imports of this object
        Try
            _container.ComposeParts(Me)
        Catch ex As Exception
            Console.WriteLine(ex.ToString)
        End Try
    End Sub



    <ImportMany("ExtractWebLinks.Common.ILinksDiscoveryPlugin")>
    Public Property LinksDiscovery As IEnumerable(Of Lazy(Of ILinksDiscoveryPlugin, ILinksDiscoveryPluginMetadata))


    Private Shared _default As PluginManger


    Public Shared ReadOnly Property [Default]() As PluginManger
        Get
            If _default Is Nothing Then _default = New PluginManger
            Return _default
        End Get
    End Property



End Class
