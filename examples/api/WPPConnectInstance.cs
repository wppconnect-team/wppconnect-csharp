using System.Threading.Tasks;

namespace WPPConnect.API
{
    public class WPPConnectInstance
    {
        private readonly WPPConnect.WPPConnection _WPPConnection;

        public WPPConnectInstance()
        {
            WPPConnect.Models.Config config = new WPPConnect.Models.Config()
            {
                Headless = false,
                Version = WPPConnect.Models.Enum.LibVersion.Nightly
            };

            _WPPConnection = new WPPConnect.WPPConnection(config);
        }

        public async Task SessionCreate(string sessionName, WPPConnect.Models.Token token)
        {
            await _WPPConnection.CreateSession(sessionName, token);
        }

        public async Task<Models.Session> SessionQrCode(string sessionName)
        {
            Models.Client client = _WPPConnection.Client(sessionName);

            Models.Session session = await client.QrCode();

            return session;
        }

        public async Task<Models.Session> SessionStatus(string sessionName)
        {
            Models.Client client = _WPPConnection.Client(sessionName);

            Models.Session session = await client.Status();

            return session;
        }

        public async Task<Models.Session> SessionLogout(string sessionName)
        {
            Models.Client client = _WPPConnection.Client(sessionName);

            Models.Session session = await client.Disconnect();

            return session;
        }

        public async Task<Models.Token> SessionToken(string sessionName)
        {
            Models.Client client = _WPPConnection.Client(sessionName);

            Models.Token token = await client.Token();

            return token;
        }
    }
}