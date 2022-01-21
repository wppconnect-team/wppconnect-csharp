using PuppeteerSharp;

namespace WPPConnect.Models
{
    internal class Connection
    {
        public Models.Client Client { get; set; } = new Client();

        public Browser Browser { get; set; }

        public Page BrowserPage { get; set; }
    }
}