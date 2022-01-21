using Microsoft.Playwright;

namespace WPPConnect.Models
{
    internal class Connection
    {
        public Models.Client Client { get; set; }

        public IBrowser Browser { get; set; }

        public IPage BrowserPage { get; set; }

        public Connection(string sessionName)
        {
            Client = new Client(sessionName);
        }
    }
}