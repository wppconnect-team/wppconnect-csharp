using PuppeteerSharp;

namespace WPPConnect.Models
{
    internal class Connection
    {
        public Models.Client Client { get; set; }

        public Browser Browser { get; set; }

        public Page BrowserPage { get; set; }

        public Connection(string sessionName)
        {
            Client = new Client(sessionName);
        }
    }
}