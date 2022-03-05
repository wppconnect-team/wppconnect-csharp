using PuppeteerSharp;

namespace WPPConnect.Models
{
    public class Connection
    {
        public Page BrowserPage { get; set; }

        public Browser BrowserContext { get; set; }

        public Connection(Browser browser)
        {
            BrowserContext = browser;
            BrowserPage = browser.PagesAsync().Result.FirstOrDefault() == null ? browser.NewPageAsync().Result : browser.PagesAsync().Result.First();
        }
    }
}