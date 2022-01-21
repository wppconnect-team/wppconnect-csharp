namespace WPPConnect.Models
{
    public class Token
    {
        public string WAToken1 { get; set; }

        public string WAToken2 { get; set; }

        public string WASecretBundle { get; set; }

        public string WABrowserId { get; set; }

        public Token(string waToken1, string waToken2, string waSecretBundle, string waBrowserId)
        {
            WAToken1 = waToken1;
            WAToken2 = waToken2;
            WASecretBundle = waSecretBundle;
            WABrowserId = waBrowserId;
        }
    }
}