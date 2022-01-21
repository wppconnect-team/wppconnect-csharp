using QRCoder;

internal class Program
{
    private static bool _Quit { get; set; }

    public static async Task Main(string[] args)
    {
        WPPConnect.Models.Config wppConnectConfig = new WPPConnect.Models.Config()
        {
            Headless = false,
            Devtools = true
        };

        WPPConnect.WPPConnect wppConnect = new WPPConnect.WPPConnect(wppConnectConfig);

        WPPConnect.Models.Session session = await wppConnect.CreateSession("Teste");

        wppConnect.OnAuthLogout += WppConnect_OnAuthLogout;
        wppConnect.OnAuthChange += WppConnect_OnAuthChange;
        wppConnect.OnMessageReceived += WppConnect_OnMessageReceived;

        while (!_Quit)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey();

            _Quit = keyInfo.Modifiers == ConsoleModifiers.Control && keyInfo.Key == ConsoleKey.C;
        }
    }

    private static void WppConnect_OnAuthChange(WPPConnect.Models.Client client, string token)
    {
        Console.WriteLine($"[{client.SessionName}:connectionChange] {token}");

        QRCodeData qrCodeData = new QRCodeGenerator().CreateQrCode(token, QRCodeGenerator.ECCLevel.Q);

        AsciiQRCode qrCode = new AsciiQRCode(qrCodeData);

        string qrCodeAsAsciiArt = qrCode.GetGraphic(1);

        Console.WriteLine(qrCodeAsAsciiArt);
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