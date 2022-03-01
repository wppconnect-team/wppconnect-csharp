namespace WPPConnect.Models
{
    public class Config
    {
        public Enum.Browser Browser { get; set; } = Enum.Browser.Chromium;

        public string? BrowserWsUrl { get; set; }

        public bool Debug { get; set; } = true;

        public string DeviceName { get; set; } = "WPPConnect";
        
        public bool Devtools { get; set; } = true;

        public bool Headless { get; set; } = true;

        public bool LogQrCode { get; set; } = true;

        public bool SessionStart { get; set; } = true;

        public string SessionFolderName { get; set; } = "sessions";

        public Enum.TokenStore TokenStore { get; set; } = Enum.TokenStore.File;

        public Enum.LibVersion Version { get; set; } = Enum.LibVersion.Latest;
    }
}