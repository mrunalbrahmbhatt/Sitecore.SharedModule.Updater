

<%@ Page Language="C#" AutoEventWireup="true"  %>
<%@ Import Namespace="System"%>
<%@ Import Namespace="System.Collections.Generic"%>
<%@ Import Namespace="Sitecore.Configuration"%>
<%@ Import Namespace="Sitecore.SharedModule.ReferenceUpdater"%>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">



<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script runat="server">
        protected void Unnamed1_Click(object sender, EventArgs e)
        {
			lblTime.Text = string.Empty;
            var db = Factory.GetDatabase("master");
            var item = db.GetItem("Path to newly copied root Item.");
            Dictionary<string, string> mappings = new Dictionary<string, string>();
            //Below is the mapping of path from Source -> Destination replacement.
            //e.g. /Sitecore/Content/Site1 -> /Sitecore/Content/Site2
            //e.g. /Sitecore/Template/Site1 -> /Sitecore/Template/Site2
            //e.g. /Sitecore/Template/Branches/Site1 -> /Sitecore/Template/Branches/Site2
            mappings.Add("Path to Source Root1", "Path to Newly mapped Root(e.g.Templates)");
			mappings.Add("Path to Source Root2", "Path to Newly mapped Root(e.g.Branches)");
            ReferenceUpdater refUpdater = new ReferenceUpdater(item, mappings, true);
            refUpdater.Start();
			lblTime.Text = refUpdater.TimeTaken;
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <asp:Button runat="server" Text="Replace" ID="btnReplace" OnClick="Unnamed1_Click" />
		<asp:Label ID="lblTime" runat="server"></asp:Label>
    </div>
    </form>
</body>
</html>
