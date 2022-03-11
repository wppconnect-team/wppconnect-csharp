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
            Devtools = false,
            Headless = false,
            Version = WPPConnect.Models.Enum.LibVersion.Nightly
        };

        WPPConnect.WPPConnection wppConnect = new WPPConnect.WPPConnection(_Config);

        wppConnect.OnAuthCodeChange += WppConnect_OnAuthChange;
        wppConnect.OnAuthAuthenticated += WppConnect_OnAuthAuthenticated;
        wppConnect.OnAuthLogout += WppConnect_OnAuthLogout;
        wppConnect.OnMessageReceived += WppConnect_OnMessageReceived;
        
        //await wppConnect.SessionCreate("Teste");

        WPPConnect.Models.Instance instance = await wppConnect.Instance("Teste");

        await instance.Testes();

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

            if (keyInfo.Key == ConsoleKey.T)
                await instance.Testes();
        }

        #endregion
    }

    private static void WppConnect_OnAuthAuthenticated(WPPConnect.Models.Instance instance)
    {
        
    }

    private static void WppConnect_OnAuthChange(WPPConnect.Models.Instance instance, string token)
    {
        Console.WriteLine($"[{instance.Session.Name}:connectionChange] {token}");

        if (_Config.LogQrCode)
        {
            QRCodeData qrCodeData = new QRCodeGenerator().CreateQrCode(token, QRCodeGenerator.ECCLevel.L);

            AsciiQRCode qrCode = new AsciiQRCode(qrCodeData);

            string qrCodeAsAsciiArt = qrCode.GetGraphicSmall();

            Console.WriteLine();
            Console.WriteLine(qrCodeAsAsciiArt);
        }
    }

    private static void WppConnect_OnAuthLogout(WPPConnect.Models.Instance instance)
    {
        
    }

    private static void WppConnect_OnMessageReceived(WPPConnect.Models.Instance instance, WPPConnect.Models.Message message)
    {
        
    }
}