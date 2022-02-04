using Microsoft.Playwright;

namespace WPPConnect.Models
{
    public class Client
    {
        public string SessionName { get; set; }

        public Models.Connection Connection { get; set; }

        public Client(string sessionName, IBrowser browser)
        {
            SessionName = sessionName;
            Connection = new Connection(browser);
        }
    }
}