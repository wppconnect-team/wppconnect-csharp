namespace WPPConnect.Models
{
    public class Client
    {
        public string SessionName { get; set; }

        public Client(string sessionName)
        {
            SessionName = sessionName;
        }
    }
}