namespace WPPConnect.Models
{
    public class Message
    {
        public string Id { get; set; }

        public string Recipient { get; set; }

        public string Content { get; set; }

        public Enum.MessageType Type { get; set; }

        public MessageFile File { get; set; }
    }
}