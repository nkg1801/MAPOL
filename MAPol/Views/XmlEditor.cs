using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MAPol.Views
{
    public partial class XmlEditor : Form
    {
        public XmlEditor()
        {
            InitializeComponent();
        }

        private async void XmlEditor_Load(object sender, EventArgs e)
        {
            await webView21.EnsureCoreWebView2Async();
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Monaco", "index.html");
            webView21.Source = new Uri(path);
        }

        private async void WebView21_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            await LoadXml();
        }

        public async Task LoadXml()
        {
            string xml = File.ReadAllText(ManifestFile);
            string js = $@"window.editor.setValue({System.Text.Json.JsonSerializer.Serialize(xml)});";
            await webView21.ExecuteScriptAsync(js);
        }

        private async void webView21_NavigationCompleted_1(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            await LoadXml();
        }

        public string ManifestFile { get; set; }


    }
}
