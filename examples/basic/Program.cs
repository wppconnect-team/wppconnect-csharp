using QRCoder;
using WPPConnect;

internal class Program
{
    private static WPPConnect.Models.Config _Config { get; set; }

    private static bool _Quit { get; set; }

    public static async Task Main(string[] args)
    {
        //Config
        _Config = new WPPConnect.Models.Config()
        {
            Headless = false,
            Version = WPPConnect.Models.Enum.LibVersion.Nightly
        };

        WPPConnect.WPPConnection wppConnect = new WPPConnect.WPPConnection(_Config);

        #region Tokens

        //Token
        //string token = File.ReadAllText(@"C:\Users\Rener\Desktop\Teste.json");
        //WPPConnect.Models.Token tokenObj = JsonConvert.DeserializeObject<WPPConnect.Models.Token>(token);
        //WPPConnect.Models.Session session = await wppConnect.CreateSession("Teste", tokenObj);

        #endregion

        wppConnect.OnAuthCodeChange += WppConnect_OnAuthChange;
        wppConnect.OnAuthAuthenticated += WppConnect_OnAuthAuthenticated;
        wppConnect.OnAuthLogout += WppConnect_OnAuthLogout;
        

        //WPPConnect.Models.Client clientCreate = await wppConnect.CreateSession("Teste");

        #region Client

        //WPPConnect.Models.Client client = wppConnect.Client("Teste");

        //WPPConnect.Models.Session clientStatus = await client.Status();
        //WPPConnect.Models.Session clientQrCode = await client.QrCode();

        //await client.SendMessage(new WPPConnect.Models.Message()
        //{
        //    Body = "Teste"
        //});

        #endregion

        #region Exit

        while (!_Quit)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey();

            _Quit = keyInfo.Modifiers == ConsoleModifiers.Control && keyInfo.Key == ConsoleKey.C;
        }

        #endregion
    }

    private static void WppConnect_OnAuthAuthenticated(WPPConnect.Models.Client client, WPPConnect.Models.Token token)
    {
        Console.WriteLine($"[{client.SessionName}:login]");
    }

    private static void WppConnect_OnAuthChange(WPPConnect.Models.Client client, string token)
    {
        Console.WriteLine($"[{client.SessionName}:connectionChange] {token}");

        if (_Config.LogQrCode)
        {
            QRCodeData qrCodeData = new QRCodeGenerator().CreateQrCode(token, QRCodeGenerator.ECCLevel.L);

            AsciiQRCode qrCode = new AsciiQRCode(qrCodeData);

            string qrCodeAsAsciiArt = qrCode.GetGraphicSmall();

            Console.WriteLine();
            Console.WriteLine(qrCodeAsAsciiArt);
        }
    }

    private static void WppConnect_OnAuthLogout(WPPConnect.Models.Client client)
    {
        Console.WriteLine($"[{client.SessionName}:logout]");
    }

    private static void WppConnect_OnMessageReceived(WPPConnect.Models.Client client, WPPConnect.Models.Message message)
    {
        Console.WriteLine($"[{client.SessionName}:messageReceived] {message}");
    }
}