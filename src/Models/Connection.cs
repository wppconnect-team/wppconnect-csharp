using Microsoft.Playwright;

namespace WPPConnect.Models
{
    public class Connection
    {
        public IPage BrowserPage { get; set; }

        public IBrowserContext BrowserContext { get; set; }

        public Connection(IBrowserContext browser)
        {
            BrowserContext = browser;
            BrowserPage = browser.Pages[0];
        }
    }
}