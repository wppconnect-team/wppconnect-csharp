using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using QRCoder;

namespace WPPConnect
{
    public class WPPConnect
    {
        #region Properties

        public Models.Config Config { get; internal set; }

        private static List<Models.Connection> _Connections = new List<Models.Connection>();

        #endregion

        #region EventHandler

        //Auth - Change
        public delegate void OnAuthChangeEventHandler(Models.Client client, string token);

        public event OnAuthChangeEventHandler OnAuthChange;

        public delegate void OnAuthLoginEventHandler(Models.Client client);

        public event OnAuthLoginEventHandler OnAuthLogin;

        //Auth - Logout
        public delegate void OnAuthLogoutEventHandler(Models.Client client);

        public event OnAuthLogoutEventHandler OnAuthLogout;

        //Chat - OnMessageReceived
        public delegate void OnMessageReceivedEventHandler(Models.Client client, Models.Message message);

        public event OnMessageReceivedEventHandler OnMessageReceived;

        #endregion

        #region Constructors

        public WPPConnect()
        {
            Start();
        }

        public WPPConnect(Models.Config config)
        {
            Config = config;

            Start();
        }

        #endregion

        #region Methods - Private

        private void Start()
        {
            Console.WriteLine(@" _       ______  ____  ______                            __ ");
            Console.WriteLine(@"| |     / / __ \/ __ \/ ____/___  ____  ____  ___  _____/ /_");
            Console.WriteLine(@"| | /| / / /_/ / /_/ / /   / __ \/ __ \/ __ \/ _ \/ ___/ __/");
            Console.WriteLine(@"| |/ |/ / ____/ ____/ /___/ /_/ / / / / / / /  __/ /__/ /_  ");
            Console.WriteLine(@"|__/|__/_/   /_/    \____/\____/_/ /_/_/ /_/\___/\___/\__/  ");
            Console.WriteLine();

            CheckVersion();
        }

        private void CheckVersion()
        {
            Console.WriteLine("TODO: Checking for updates...");
        }

        private async void Disconnect(Models.Client client)
        {
            Models.Connection connection = _Connections.SingleOrDefault(c => c.Client.SessionName == client.SessionName);

            if (connection != null)
            {
                await connection.Browser.CloseAsync();

                _Connections.Remove(connection);
            }
        }

        private Models.Connection ConnectionValidate(string sessionName)
        {
            Models.Connection connection = _Connections.SingleOrDefault(c => c.Client.SessionName == sessionName);

            if (connection == null)
                throw new Exception($"Não foi encontrado nenhuma sessão com o nome {sessionName}");
            else
                return connection;
        }

        private void BrowserPage_Request(object sender, RequestEventArgs e)
        {
            if (e.Request.ResourceType != ResourceType.Document)
            {
                e.Request.ContinueAsync();
                return;
            }

            ResponseData responseData = new ResponseData()
            {
                Status = System.Net.HttpStatusCode.OK,
                ContentType = "text/html",
                Body = @"<!doctype html><html lang=en><head><title>Initializing WhatsApp</title></head><body><div><h1>Initializing WhatsApp ...</h1></div></body></html>"
            };

            e.Request.RespondAsync(responseData);
        }

        private bool BrowserPage_OnAuthChange(string sessionName, object token)
        {
            Models.Client client = _Connections.Single(c => c.Client.SessionName == sessionName).Client;

            JObject response = JsonConvert.DeserializeObject<JObject>(token.ToString());

            string fullCode = response["fullCode"].ToString();

            OnAuthChange(client, fullCode);

            return true;
        }

        private bool BrowserPage_OnAuthLogout(string sessionName)
        {
            Models.Client client = _Connections.Single(c => c.Client.SessionName == sessionName).Client;

            Disconnect(client);

            OnAuthLogout(client);

            return true;
        }

        private bool BrowserPage_OnMessageReceived(string sessionName, object message)
        {
            Models.Client client = _Connections.Single(c => c.Client.SessionName == sessionName).Client;

            JObject response = JsonConvert.DeserializeObject<JObject>(message.ToString());

            Models.Message messageObj = new Models.Message();
            {

            };

            OnMessageReceived(client, messageObj);

            return true;
        }

        #endregion

        #region Methods

        public async Task<Models.Session> CreateSession(string sessionName, Models.Token? token = null)
        {
            try
            {
                Models.Connection connection = _Connections.SingleOrDefault(i => i.Client.SessionName == sessionName);

                if (connection == null)
                {
                    connection = new Models.Connection(sessionName);

                    if (Config.Debug)
                        Console.WriteLine($"[{connection.Client.SessionName}:browser] Initializing browser...");

                    if (!string.IsNullOrEmpty(Config.BrowserWsUrl))
                    {
                        ConnectOptions connectOptions = new ConnectOptions
                        {
                            BrowserWSEndpoint = Config.BrowserWsUrl
                        };

                        connection.Browser = await Puppeteer.ConnectAsync(connectOptions);
                    }
                    else
                    {
                        await new BrowserFetcher().DownloadAsync();

                        string[] args = new string[]
                                {
                                  "--enable-gpu",
                                  "--display-entrypoints",
                                  "--disable-http-cache",
                                  "no-sandbox",
                                  "--no-sandbox",
                                  "--disable-setuid-sandbox",
                                  "--disable-2d-canvas-clip-aa",
                                  "--disable-2d-canvas-image-chromium",
                                  "--disable-3d-apis",
                                  "--disable-accelerated-2d-canvas",
                                  "--disable-accelerated-jpeg-decoding",
                                  "--disable-accelerated-mjpeg-decode",
                                  "--disable-accelerated-video-decode",
                                  "--disable-app-list-dismiss-on-blur",
                                  "--disable-audio-output",
                                  "--disable-background-timer-throttling",
                                  "--disable-backgrounding-occluded-windows",
                                  "--disable-breakpad",
                                  "--disable-canvas-aa",
                                  "--disable-client-side-phishing-detection",
                                  "--disable-component-extensions-with-background-pages",
                                  "--disable-composited-antialiasing",
                                  "--disable-default-apps",
                                  "--disable-dev-shm-usage",
                                  "--disable-extensions",
                                  "--disable-features=TranslateUI,BlinkGenPropertyTrees",
                                  "--disable-field-trial-config",
                                  "--disable-fine-grained-time-zone-detection",
                                  "--disable-geolocation",
                                  "--disable-gl-extensions",
                                  "--disable-gpu",
                                  "--disable-gpu-early-init",
                                  "--disable-gpu-sandbox",
                                  "--disable-gpu-watchdog",
                                  "--disable-histogram-customizer",
                                  "--disable-in-process-stack-traces",
                                  "--disable-infobars",
                                  "--disable-ipc-flooding-protection",
                                  "--disable-notifications",
                                  "--disable-renderer-backgrounding",
                                  "--disable-session-crashed-bubble",
                                  "--disable-setuid-sandbox",
                                  "--disable-site-isolation-trials",
                                  "--disable-software-rasterizer",
                                  "--disable-sync",
                                  "--disable-threaded-animation",
                                  "--disable-threaded-scrolling",
                                  "--disable-translate",
                                  "--disable-webgl",
                                  "--disable-webgl2",
                                  "--enable-features=NetworkService",
                                  "--force-color-profile=srgb",
                                  "--hide-scrollbars",
                                  "--ignore-certifcate-errors",
                                  "--ignore-certifcate-errors-spki-list",
                                  "--ignore-certificate-errors",
                                  "--ignore-certificate-errors-spki-list",
                                  "--ignore-gpu-blacklist",
                                  "--ignore-ssl-errors",
                                  "--log-level=3",
                                  "--metrics-recording-only",
                                  "--mute-audio",
                                  "--no-crash-upload",
                                  "--no-default-browser-check",
                                  "--no-experiments",
                                  "--no-first-run",
                                  "--no-sandbox",
                                  "--no-zygote",
                                  "--renderer-process-limit=1",
                                  "--safebrowsing-disable-auto-update",
                                  "--silent-debugger-extension-api",
                                  "--single-process",
                                  "--unhandled-rejections=strict",
                                  "--window-position=0,0" };

                        LaunchOptions launchOptions = new LaunchOptions
                        {
                            Args = Config.Headless == true ? args : new string[0],
                            Headless = Config.Headless,
                            Devtools = Config.Devtools,
                            DefaultViewport = new ViewPortOptions
                            {
                                Width = 1920,
                                Height = 1080
                            }
                        };

                        connection.Browser = await Puppeteer.LaunchAsync(launchOptions);
                    }

                    if (Config.Debug)
                        Console.WriteLine($"[{connection.Client.SessionName}:client] Initializing...");

                    connection.BrowserPage = connection.Browser.PagesAsync().Result.FirstOrDefault() == null ? await connection.Browser.NewPageAsync() : connection.Browser.PagesAsync().Result.First();

                    await connection.BrowserPage.SetUserAgentAsync("WhatsApp/2.2043.8 Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.121 Safari/537.36");

                    await connection.BrowserPage.SetBypassCSPAsync(true);

                    await connection.BrowserPage.SetRequestInterceptionAsync(true);

                    connection.BrowserPage.Request += BrowserPage_Request;

                    await connection.BrowserPage.GoToAsync("https://web.whatsapp.com");

                    if (token != null)
                    {
                        await connection.BrowserPage.EvaluateFunctionAsync("async => window.localStorage.clear()");
                        await connection.BrowserPage.EvaluateFunctionAsync($"async => localStorage.setItem('WABrowserId','{token.WABrowserId}')");
                        await connection.BrowserPage.EvaluateFunctionAsync($"async => localStorage.setItem('WASecretBundle','{token.WASecretBundle}')");
                        await connection.BrowserPage.EvaluateFunctionAsync($"async => localStorage.setItem('WAToken1','{token.WAToken1}')");
                        await connection.BrowserPage.EvaluateFunctionAsync($"async => localStorage.setItem('WAToken2','{token.WAToken2}')");
                    }

                    await connection.BrowserPage.SetRequestInterceptionAsync(false);

                    await connection.BrowserPage.GoToAsync("https://web.whatsapp.com");

                    await connection.BrowserPage.AddScriptTagAsync("https://arquivos.netplustecnologia.com/wppconnect-wa.js");

                    #region Events

                    //Auth - Logout
                    await connection.BrowserPage.ExposeFunctionAsync<string, bool>("browserPage_OnConnectionLogout", BrowserPage_OnAuthLogout);
                    await connection.BrowserPage.EvaluateFunctionAsync("async => WPP.auth.on('logout', function() { browserPage_OnConnectionLogout('" + connection.Client.SessionName + "') })");

                    //Auth - Change
                    await connection.BrowserPage.ExposeFunctionAsync<string, object, bool>("browserPage_OnAuthChange", BrowserPage_OnAuthChange);
                    await connection.BrowserPage.EvaluateFunctionAsync("async => WPP.auth.on('change', function(e) { browserPage_OnAuthChange('" + connection.Client.SessionName + "', e) })");

                    //Chat - OnMessageReceived
                    await connection.BrowserPage.ExposeFunctionAsync<string, object, bool>("browserPage_OnMessageReceived", BrowserPage_OnMessageReceived);
                    await connection.BrowserPage.EvaluateFunctionAsync("async => WPP.whatsapp.MsgStore.on('change', function(e) { browserPage_OnMessageReceived('" + connection.Client.SessionName + "', e) })");

                    #endregion

                    _Connections.Add(connection);
                }

                return await QrCode(sessionName);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<Models.Session> Status(string sessionName)
        {
            Models.Session session = new Models.Session(sessionName);

            try
            {
                Models.Connection connection = ConnectionValidate(sessionName);

                bool authenticated = await connection.BrowserPage.EvaluateFunctionAsync<bool>("async => WPP.auth.isAuthenticated()");

                if (authenticated)
                {
                    session.Status = Models.Enum.Status.Conectado;

                    return session;
                }
                else
                {
                    session.Status = Models.Enum.Status.Desconectado;

                    return session;
                }
            }
            catch (Exception e)
            {
                session.Status = Models.Enum.Status.Desconectado;
                session.Mensagem = e.Message;

                return session;
            }
        }

        public async Task<Models.Session> QrCode(string sessionName)
        {
            Models.Session session = await Status(sessionName);

            try
            {
                if (session.Status == Models.Enum.Status.Desconectado)
                {
                    Models.Connection connection = ConnectionValidate(sessionName);

                    JToken response = await connection.BrowserPage.EvaluateFunctionAsync<JToken>("async => WPP.auth.getAuthCode()");

                    string fullCode = response["fullCode"].ToString();

                    session.Status = Models.Enum.Status.QrCode;
                    session.Mensagem = fullCode;

                    if (Config.LogQrCode)
                    {
                        Console.WriteLine($"[{connection.Client.SessionName}:qrcode] {session.Mensagem}");

                        QRCodeData qrCodeData = new QRCodeGenerator().CreateQrCode(session.Mensagem, QRCodeGenerator.ECCLevel.Q);

                        AsciiQRCode qrCode = new AsciiQRCode(qrCodeData);

                        string qrCodeAsAsciiArt = qrCode.GetGraphic(1);

                        Console.WriteLine(qrCodeAsAsciiArt);
                    }

                    return session;
                }

                return session;
            }
            catch (Exception)
            {
                return session;
            }
        }

        public async Task<Models.Session> Disconnect(string sessionName)
        {
            Models.Session session = await Status(sessionName);

            try
            {
                if (session.Status == Models.Enum.Status.Conectado)
                {
                    Models.Connection connection = ConnectionValidate(sessionName);

                    bool logout = await connection.BrowserPage.EvaluateFunctionAsync<bool>("async => WPP.auth.logout()");

                    await Disconnect(sessionName);

                    session.Status = Models.Enum.Status.Desconectado;

                    return session;
                }

                session.Status = Models.Enum.Status.Desconectado;

                return session;
            }
            catch (Exception e)
            {
                session.Status = Models.Enum.Status.Desconectado;
                session.Mensagem = e.Message;

                return session;
            }
        }

        public async Task<bool> SendMessage(string sessionName, Models.Message message)
        {
            try
            {
                Models.Session session = await Status(sessionName);

                if (session.Status == Models.Enum.Status.Conectado)
                {
                    Models.Connection connection = ConnectionValidate(sessionName);

                    await connection.BrowserPage.EvaluateFunctionAsync("async => WPP.chat.sendTextMessage('5564992176420@c.us', 'Teste 1', { createChat: true })");

                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}