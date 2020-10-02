﻿<%@ Page Title="" Language="vb" AutoEventWireup="false" CodeBehind="404.aspx.vb" Inherits="sselUser._404" %>

<!doctype html>
<html lang="en">
<head runat="server">
    <!-- Required meta tags -->
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">

    <!-- Bootstrap CSS -->
    <link rel="stylesheet" href="https://ssel-apps.eecs.umich.edu/static/lib/bootstrap4/css/bootstrap.min.css" />

    <title>Page Not Found</title>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container-fluid mt-4">
            <h3>
                404 page not found...
            </h3>
            <div class="mb-3">
                [<asp:HyperLink runat="server" NavigateUrl="~/">LNF User Home</asp:HyperLink>] [<a href="/login">Logout</a>]
            </div>
            <div class="alert alert-danger" role="alert">
                <asp:Literal runat="server" ID="litMessage"></asp:Literal>
            </div>
        </div>
    </form>

    <!-- Optional JavaScript -->
    <!-- jQuery first, then Popper.js, then Bootstrap JS -->
    <script src="https://ssel-apps.eecs.umich.edu/static/lib/jquery/jquery.min.js"></script>
    <script src="https://ssel-apps.eecs.umich.edu/static/lib/popper/1.14.7/umd/popper.min.js"></script>
    <script src="https://ssel-apps.eecs.umich.edu/static/lib/bootstrap4/js/bootstrap.min.js"></script>
</body>
</html>

