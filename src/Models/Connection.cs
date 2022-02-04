using Microsoft.Playwright;

namespace WPPConnect.Models
{
    public class Connection
    {
        public IBrowser Browser { get; set; }

        public IPage BrowserPage { get; set; }


        public Connection(IBrowser browser)
        {
            Browser = browser;
        }
    }
}