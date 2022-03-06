namespace WPPConnect.Models
{
    public static class Enum
    {
        public enum Browser
        {
            Chromium,
            Firefox
        }

        public enum LibVersion
        {
            Latest,
            Nightly
        }

        public enum Status
        {
            Connected,
            Disconnected,
            QrCode,
            ERROR
        }

        public enum MessageType
        {
            Document,
            Image,
            Text
        }
    }
}