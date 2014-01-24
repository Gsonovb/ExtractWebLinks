<!DOCTYPE html>

<html>
<head>
    <title>Results</title>
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
    <meta charset="utf-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <link rel="stylesheet" href="http://cdn.bootcss.com/twitter-bootstrap/3.0.3/css/bootstrap.min.css">
    <link rel="stylesheet" href="http://cdn.bootcss.com/twitter-bootstrap/3.0.3/css/bootstrap-theme.min.css">
    <script src="http://cdn.bootcss.com/jquery/1.10.2/jquery.min.js"></script>
    <script src="http://cdn.bootcss.com/twitter-bootstrap/3.0.3/js/bootstrap.min.js"></script>
    <link href="http://getbootstrap.com/examples/sticky-footer-navbar/sticky-footer-navbar.css" rel="stylesheet">
</head>
<body>
    <div id="wrap">
        <div class="navbar navbar-default navbar-static-top" role="navigation">
            <div class="container">
                <div class="collapse navbar-collapse">
                    <ul class="nav navbar-nav">
                        <li class="active"><a href="#home" data-toggle="tab">首页</a></li>
                        <li>
                            <a href="#links" data-toggle="tab">
                                链接
                                <span class="badge">@Model.Allinks.Count</span>
                            </a>
                        </li>
                        <li><a href="#tree" data-toggle="tab">树</a></li>
                    </ul>
                </div>
            </div>
        </div>
        <div class="tab-content">
            <div class="tab-pane active" id="home">
                <div class="container">
                    <div>
                        <a class="page-header">
                            <h1>结果信息</h1>
                        </a>
                        <div>
                            <div class="row">
                                <label class="col-sm-2">输出文件：</label>
                                <div class="col-sm-10">
                                    <p class="form-control-static">@Model.Filename</p>
                                </div>
                            </div>
                            <div class="row">
                                <label class="col-sm-2">最大深度：</label>
                                <div class="col-sm-10">
                                    <p class="form-control-static">@Model.Maxdeep </p>
                                </div>
                            </div>
                            <div class="row">
                                <label class="col-sm-2">URL变换：</label>
                                <div class="col-sm-10">
                                    <p class="form-control-static">@Model.Transform.ToString() </p>
                                </div>
                            </div>
                            <div class="row">
                                <label class="col-sm-2">起始地址：</label>
                                <ol class="col-sm-10">
                                    @For Each url In Model.Urls
                                        @<li class="code">@url.ToString()</li>
                                    Next
                                </ol>
                            </div>
                            <div class="row">
                                <label class="col-sm-2">检查规则：</label>
                                <ol class="col-sm-10">
                                    @For Each rule In Model.Rules
                                        @<li class="code">@rule</li>
                                    Next
                                </ol>
                            </div>
                            <div class="row">
                                <label class="col-sm-2">URL排除规则：</label>
                                <ol class="col-sm-10">
                                    @For Each rule In Model.exUrlRules
                                        @<li class="code">@rule</li>
                                    Next
                                </ol>
                            </div>
                            <div class="row">
                                <label class="col-sm-2">Title排除规则：</label>
                                <ol class="col-sm-10">
                                    @For Each rule In Model.exTitleRules
                                        @<li class="code">@rule</li>
                                    Next
                                </ol>
                            </div>
                        </div>
                    </div>
                    <div>
                        <a class="page-header">
                            <h1>Options 信息</h1>
                        </a>
                        <div>
                            <div class="row">
                                <label class="col-sm-2">输出文件：</label>
                                <div class="col-sm-10">
                                    <p class="form-control-static">@Model.Options.FileName</p>
                                </div>
                            </div>
                            <div class="row">
                                <label class="col-sm-2">XPath：</label>
                                <div class="col-sm-10">
                                    <p class="form-control-static">@Model.Options.XPath </p>
                                </div>
                            </div>
                            <div class="row">
                                <label class="col-sm-2">URL变换：</label>
                                <div class="col-sm-10">
                                    <p class="form-control-static">@Model.Options.Transform </p>
                                </div>
                            </div>
                            <div class="row">
                                <label class="col-sm-2">起始地址：</label>
                                <ol class="col-sm-10">
                                    @If Model.Options.StartUrls IsNot Nothing Then
                                        For Each url In Model.Options.StartUrls
                                        @<li class="code">@url</li>
                                        Next
                                    End If
                                </ol>
                            </div>
                            <div class="row">
                                <label class="col-sm-2">检查规则：</label>
                                <ol class="col-sm-10">

                                    @If Model.Options.Rules IsNot Nothing Then
                                        For Each rule In Model.Options.Rules
                                        @<li class="code">@rule</li>
                                        Next
                                    End If
                                </ol>
                            </div>
                            <div class="row">
                                <label class="col-sm-2">URL排除规则：</label>
                                <ol class="col-sm-10">
                                    @If Model.Options.exUrlRules IsNot Nothing Then
                                        For Each rule In Model.Options.exUrlRules
                                        @<li class="code">@rule</li>
                                        Next
                                    End If
                                </ol>
                            </div>
                            <div class="row">
                                <label class="col-sm-2">Title排除规则：</label>
                                <ol class="col-sm-10">
                                    @If Model.Options.exTitleRules IsNot Nothing Then
                                        For Each rule In Model.Options.exTitleRules
                                        @<li class="code">@rule</li>
                                        Next
                                    End If
                                </ol>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="tab-pane" id="links">
                <div class="container">
                    <a class="page-header">
                        <h1>链接</h1>
                        <span class="badge">@Model.Allinks.Count </span>
                    </a>
                    <div class="panel-group" id="accordion">
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                <h4 class="panel-title">
                                    <a data-toggle="collapse" data-toggle="collapse" data-parent="#accordion" href="#collapseOne">
                                        页面信息
                                    </a>
                                </h4>
                            </div>
                            <div id="collapseOne" class="panel-collapse collapse in">
                                <div class="panel-body">
                                    <div>
                                        <ul class="list-group ">
                                            @For Each link In Model.Allinks.Values
                                                Dim cls = ""

                                                If link.ErrorEX IsNot Nothing Then
                                                    cls = "list-group-item alert alert-danger"
                                                ElseIf link.isPrccessed Then
                                                    cls = "list-group-item alert alert-success"
                                                Else
                                                    cls = "list-group-item alert alert-info"
                                                End If
                                                @<li class="@cls">
                                                    <a class="list-group-item-heading" href="@link.Url.ToString()">
                                                        <span class="badge pull-left">@link.DeepLevel</span>
                                                        <span>@link.Title </span>
                                                        <span class="badge">已处理：@link.isPrccessed </span>
                                                    </a>
                                                    <p class="list-group-item-text">
                                                        @If link.ErrorEX IsNot Nothing Then
                                                            @link.ErrorEX.ToString()
                                                        End If
                                                    </p>
                                                </li>
                                            Next
                                        </ul>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                <h4 class="panel-title">
                                    <a data-toggle="collapse" data-toggle="collapse" data-parent="#accordion" href="#collapseTwo">
                                        URL列表
                                    </a>
                                </h4>
                            </div>
                            <div id="collapseTwo" class="panel-collapse collapse">
                                <div class="panel-body">
                                    <ol>
                                        @For Each link In Model.Allinks.Values
                                            @<li class="">@link.Url.ToString() </li>
                                        Next
                                    </ol>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="tab-pane" id="tree">
                <div class="container">
                    <a class="page-header">
                        <h1>树</h1>
                    </a>
                    <div>
                        @Raw(Model.TreeHTML)
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div id="footer">
        <div class="container">
            <p class="text-muted">生成时间： @DateTime.Now.ToString </p>
        </div>
    </div>
</body>
</html>
