namespace WPPConnect.Models
{
    public static class Enum
    {
        public enum Status
        {
            Connected,
            Disconnected,
            QrCode,
            ERROR
        }

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
    }
}