namespace WPPConnect.Models
{
    public class Session
    {
        public Models.Client Client { get; set; } = new Client();

        public Enum.Status Status { get; set; }

        public string? Mensagem { get; set; }
    }
}