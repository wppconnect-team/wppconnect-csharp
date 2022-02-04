namespace WPPConnect.Models
{
    public class Config
    {
        public bool Debug { get; set; } = true;

        public bool LogQrCode { get; internal set; } = true;

        public string? BrowserWsUrl { get; internal set; }

        public Enum.TokenStore TokenStore { get; internal set; } = Enum.TokenStore.File;

        public string TokenFolderName { get; internal set; } = "tokens";

        public string DeviceName { get; set; } = "WPPConnect";

        public bool Headless { get; set; } = true;

        public bool Devtools { get; set; } = true;

        public Enum.Browser Browser { get; internal set; } = Enum.Browser.Chromium;
    }
}