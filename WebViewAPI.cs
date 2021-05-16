using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeus
{
    public class WebViewAPI : WebView2
    {
        public bool isDOMLoaded { get; set; } = false;
        private string ToSetText;
        private string LatestRecievedText;

        /// <summary>
        /// Event for when the editor is fully loaded.
        /// </summary>
        public event EventHandler EditorReady;

        public WebViewAPI(string Text = "print(\"Welcome to Zeus\")")
        {
            this.Source = new Uri($"{Directory.GetCurrentDirectory()}\\Monaco\\Monaco.html");//directory of ur monaco editor
            this.DefaultBackgroundColor = System.Drawing.Color.FromArgb(255, 25, 25, 25);
            this.CoreWebView2InitializationCompleted += WebViewAPI_CoreWebView2InitializationCompleted;
            this.ToSetText = Text;
        }

        protected virtual void OnEditorReady()
        {
            isDOMLoaded = true;
            EventHandler handler = EditorReady;
            if (handler != null)
                handler(this, new EventArgs());
        }

        private void WebViewAPI_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            this.CoreWebView2.DOMContentLoaded += CoreWebView2_DOMContentLoaded;
            this.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            this.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            this.CoreWebView2.Settings.AreDevToolsEnabled = false;
        }

        private void CoreWebView2_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e) => LatestRecievedText = e.TryGetWebMessageAsString();

        private async void CoreWebView2_DOMContentLoaded(object sender, Microsoft.Web.WebView2.Core.CoreWebView2DOMContentLoadedEventArgs e)
        {
            await Task.Delay(1000);
            await this.CoreWebView2.ExecuteScriptAsync($"SetText(`{ToSetText}`)");
            OnEditorReady();
        }

        /* Methods */

        /// <summary>
        /// Get the editors text
        /// </summary>
        /// <returns>The editors text</returns>
        public async Task<string> GetText()
        {
            if (isDOMLoaded)
            {
                await this.ExecuteScriptAsync("window.chrome.webview.postMessage(editor.getValue())");
                await Task.Delay(200);

                return LatestRecievedText;
            }
            return string.Empty;
        }

        /// <summary>
        /// Set the editors text to the selected parameter.
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text)
        {
            if (isDOMLoaded)
                CoreWebView2.ExecuteScriptAsync($"SetText(`{text}`)");
        }

        /// <summary>
        /// Add a proposal to the monaco editor
        /// </summary>
        /// <param name="label"></param>
        /// <param name="type"></param>
        /// <param name="description"></param>
        /// <param name="insert"></param>
        public void AddIntellisense(string label, Types type, string description, string insert)
        {
            if (isDOMLoaded)
            {
                string selectedType = type.ToString();
                if (type == Types.None)
                    selectedType = "";
                this.ExecuteScriptAsync($"AddIntellisense({label}, {selectedType}, {description}, {insert});");
            }
        }

        /// <summary>
        /// Refresh the editor
        /// </summary>
        public void Refresh()
        {
            if (isDOMLoaded)
                this.ExecuteScriptAsync($"Refresh();");
        }
    }

    public enum Types
    {
        Class,
        Color,
        Constructor,
        Enum,
        Field,
        File,
        Folder,
        Function,
        Interface,
        Keyword,
        Method,
        Module,
        Property,
        Reference,
        Snippet,
        Text,
        Unit,
        Value,
        Variable,
        None
    }
}