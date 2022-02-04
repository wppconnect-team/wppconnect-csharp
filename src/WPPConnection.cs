using Microsoft.Playwright;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace WPPConnect
{
    public class WPPConnection
    {
        #region Properties

        public Models.Config Config { get; internal set; }

        private static List<Models.Client> _Clients = new List<Models.Client>();

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

        #region Events

        private bool BrowserPage_OnAuthChange(string sessionName, dynamic token)
        {
            Models.Client client = _Clients.Single(c => c.SessionName == sessionName);

            string fullCode = token.fullCode;

            OnAuthChange(client, fullCode);

            return true;
        }

        private bool BrowserPage_OnAuthLogout(string sessionName)
        {
            Models.Client client = _Clients.Single(c => c.SessionName == sessionName);

            client.Connection.Browser.CloseAsync().Wait();

            _Clients.Remove(client);

            OnAuthLogout(client);

            return true;
        }

        private bool BrowserPage_OnMessageReceived(string sessionName, object message)
        {
            Models.Client client = _Clients.Single(c => c.SessionName == sessionName);

            dynamic response = (System.Dynamic.ExpandoObject)message;

            Models.Message messageObj = new Models.Message()
            {
                Id = response.id.id,
                Body = response.body
            };

            OnMessageReceived(client, messageObj);

            return true;
        }

        #endregion

        #region Constructors

        public WPPConnection()
        {
            new WPPConnection(new Models.Config());
        }

        public WPPConnection(Models.Config config)
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

        private async void CheckVersion()
        {
            RestClient client = new RestClient("https://api.github.com/repos/wppconnect-team/wa-js/releases/latest");

            RestRequest request = new RestRequest();

            RestResponse response = await client.GetAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                JObject json = JObject.Parse(response.Content);

                string version = json["name"].ToString();

                Console.WriteLine($"[wa-js : {version}]");
            }
            else
                Console.WriteLine("[wa-js version:não foi possível obter a versão]");
        }

        #endregion

        #region Methods - Public

        public async Task<Models.Client> CreateSession(string sessionName, Models.Token? token = null)
        {
            try
            {
                IPlaywright playwright = await Playwright.CreateAsync();

                IBrowserType playwrightBrowser = playwright.Chromium;

                switch (Config.Browser)
                {
                    case Models.Enum.Browser.Chromium:
                        playwrightBrowser = playwright.Chromium;
                        break;
                    case Models.Enum.Browser.Firefox:
                        playwrightBrowser = playwright.Firefox;
                        break;
                    case Models.Enum.Browser.Webkit:
                        playwrightBrowser = playwright.Webkit;
                        break;
                }

                Models.Client client = _Clients.SingleOrDefault(i => i.SessionName == sessionName);

                if (client == null)
                {
                    if (Config.Debug)
                        Console.WriteLine($"[{sessionName}:browser] Initializing browser...");

                    if (!string.IsNullOrEmpty(Config.BrowserWsUrl))
                    {
                        IBrowser browser = await playwrightBrowser.ConnectAsync(Config.BrowserWsUrl);

                        client = new Models.Client(sessionName, browser);
                    }
                    else
                    {
                        //await new BrowserFetcher().DownloadAsync();

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

                        BrowserTypeLaunchOptions launchOptions = new BrowserTypeLaunchOptions
                        {
                            Args = Config.Headless == true ? args : new string[0],
                            Headless = Config.Headless,
                            Devtools = Config.Devtools,
                            Channel = "chrome"
                        };

                        IBrowser browser = await playwrightBrowser.LaunchAsync(launchOptions);

                        client = new Models.Client(sessionName, browser);
                    }

                    if (Config.Debug)
                        Console.WriteLine($"[{client.SessionName}:client] Initializing...");

                    client.Connection.BrowserPage = await client.Connection.Browser.NewPageAsync(new BrowserNewPageOptions()
                    {
                        BypassCSP = true,
                        UserAgent = "WhatsApp/2.2043.8 Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.121 Safari/537.36"
                    });

                    await client.Connection.BrowserPage.GotoAsync("https://web.whatsapp.com");

                    if (token != null)
                    {
                        await client.Connection.BrowserPage.EvaluateAsync("async => window.localStorage.clear()");
                        await client.Connection.BrowserPage.EvaluateAsync($"async => localStorage.setItem('WABrowserId','{token.WABrowserId}')");
                        await client.Connection.BrowserPage.EvaluateAsync($"async => localStorage.setItem('WASecretBundle','{token.WASecretBundle}')");
                        await client.Connection.BrowserPage.EvaluateAsync($"async => localStorage.setItem('WAToken1','{token.WAToken1}')");
                        await client.Connection.BrowserPage.EvaluateAsync($"async => localStorage.setItem('WAToken2','{token.WAToken2}')");

                        await client.Connection.BrowserPage.GotoAsync("https://web.whatsapp.com");
                    }

                    await client.Connection.BrowserPage.AddScriptTagAsync(new PageAddScriptTagOptions()
                    {
                        Url = "https://github.com/wppconnect-team/wa-js/releases/latest/download/wppconnect-wa.js"
                    });

                    #region Events

                    //Auth - Logout
                    await client.Connection.BrowserPage.ExposeFunctionAsync<string, bool>("browserPage_OnConnectionLogout", BrowserPage_OnAuthLogout);
                    await client.Connection.BrowserPage.EvaluateAsync("async => WPP.auth.on('logout', function() { browserPage_OnConnectionLogout('" + client.SessionName + "') })");

                    //Auth - Change
                    await client.Connection.BrowserPage.ExposeFunctionAsync<string, object, bool>("browserPage_OnAuthChange", BrowserPage_OnAuthChange);
                    await client.Connection.BrowserPage.EvaluateAsync("async => WPP.auth.on('change', function(e) { browserPage_OnAuthChange('" + client.SessionName + "', e) })");

                    //Chat - OnMessageReceived
                    await client.Connection.BrowserPage.ExposeFunctionAsync<string, object, bool>("browserPage_OnMessageReceived", BrowserPage_OnMessageReceived);
                    await client.Connection.BrowserPage.EvaluateAsync("async => WPP.whatsapp.MsgStore.on('change', function(e) { browserPage_OnMessageReceived('" + client.SessionName + "', e) })");

                    #endregion

                    _Clients.Add(client);
                }
                else
                    throw new Exception($"Já existe uma session com o nome {sessionName}");

                if (Config.Debug)
                    Console.WriteLine($"[{sessionName}:client] Initialized");

                Models.Session session = await client.QrCode();

                if (Config.LogQrCode && session.Status == Models.Enum.Status.QrCode)
                {
                    dynamic qrCodeJson = new JObject();
                    qrCodeJson.fullCode = session.Mensagem;

                    BrowserPage_OnAuthChange(client.SessionName, qrCodeJson);
                }

                return client;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Models.Client Client(string sessionName)
        {
            Models.Client client = _Clients.SingleOrDefault(c => c.SessionName == sessionName);

            if (client == null)
                throw new Exception($"Não foi encontrado nenhuma sessão com o nome {sessionName}");
            else
                return client;
        }

        #endregion
    }
}