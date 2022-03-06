using Microsoft.Playwright;

namespace WPPConnect.Models
{
    public class Instance
    {
        public Models.Session Session { get; set; }

        public Models.Connection Connection { get; set; }

        public Instance(string sessionName, IBrowserContext browserContext)
        {
            Session = new Session(sessionName);
            Connection = new Connection(browserContext);
        }
    }
}