namespace WPPConnect.Models
{
    public static class Enum
    {
        public enum Status
        {
            Conectado,
            Desconectado,
            QrCode
        }

        public enum TokenStore
        {
            None,
            File
        }

        public enum Browser
        {
            Chromium,
            Firefox,
            Webkit
        }
    }
}
