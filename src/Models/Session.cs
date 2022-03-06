namespace WPPConnect.Models
{
    public class Session
    {
        public string Name { get; set; }

        public Enum.Status Status { get; set; }

        public string QrCode { get; set; }

        public Session(string name)
        {
            Name = name;
            Status = Enum.Status.QrCode;
        }
    }
}