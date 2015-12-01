<%@ Async="true" Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ServiceBusWebFrontend._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="">
    <asp:Button ID="cmdSend1" runat="server" Text="Wyślij - 1" OnClick="cmdSend1_Click" />
    <asp:Button ID="cmdSend2" runat="server" Text="Wyślij - 2" OnClick="cmdSend2_Click" />
</div>
<div>
    <asp:Label ID="lblInfo" runat="server" Text="Tu wynik(i)"></asp:Label>
</div>

</asp:Content>
