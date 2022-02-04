namespace WPPConnect.Models
{
    public class Session
    {
        public Models.Client Client { get; set; }

        public Enum.Status Status { get; set; }

        public string? Mensagem { get; set; }

        public Session(Models.Client client)
        {
            Client = client;
        }
    }
}