namespace WPPConnect.Models
{
    public class Session
    {
        public Models.Client Client { get; set; }

        public Enum.Status Status { get; set; }

        public string? Mensagem { get; set; }

        public Session(string sessionName)
        {
            Client = new Models.Client(sessionName);
        }
    }
}