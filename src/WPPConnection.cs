using Microsoft.Playwright;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Dynamic;

namespace WPPConnect
{
    public class WPPConnection
    {
        #region Properties

        public Models.Config Config { get; internal set; }

        private static List<Models.Client> _Clients = new List<Models.Client>();

        #endregion

        #region EventHandler

        //Auth - Authenticated
        public delegate void OnAuthAuthenticatedEventHandler(Models.Client client, Models.Token token);

        public event OnAuthAuthenticatedEventHandler OnAuthAuthenticated;

        //Auth - CodeChange
        public delegate void OnAuthCodeChangeEventHandler(Models.Client client, string token);

        public event OnAuthCodeChangeEventHandler OnAuthCodeChange;

        //Auth - Logout
        public delegate void OnAuthLogoutEventHandler(Models.Client client);

        public event OnAuthLogoutEventHandler OnAuthLogout;

        //Auth - Require
        public delegate void OnAuthRequireEventHandler(Models.Client client, string token);

        public event OnAuthRequireEventHandler OnAuthRequire;

        //Chat - OnMessageReceived
        public delegate void OnMessageReceivedEventHandler(Models.Client client, Models.Message message);

        public event OnMessageReceivedEventHandler OnMessageReceived;

        #endregion

        #region Events

        private async Task<bool> BrowserPage_OnAuthAuthenticated(string sessionName)
        {
            if (this.OnAuthAuthenticated != null)
            {
                Models.Client client = _Clients.Single(c => c.SessionName == sessionName);

                Models.Token token = await client.Token();

                SessionSave(client, token);

                OnAuthAuthenticated(client, token);
            }

            return true;
        }

        private bool BrowserPage_OnAuthCodeChange(string sessionName, dynamic token)
        {
            if (token != null)
            {
                if (this.OnAuthCodeChange != null)
                {
                    Models.Client client = _Clients.Single(c => c.SessionName == sessionName);

                    string fullCode = token.fullCode;

                    OnAuthCodeChange(client, fullCode);
                }
            }

            return true;
        }

        private bool BrowserPage_OnAuthLogout(string sessionName)
        {
            if (this.OnAuthLogout != null)
            {
                Models.Client client = Client(sessionName);

                client.Connection.Browser.CloseAsync();

                _Clients.Remove(client);

                SessionRemove(client);

                OnAuthLogout(client);
            }

            return true;
        }

        private bool BrowserPage_OnAuthRequire(string sessionName, dynamic token)
        {
            if (token != null)
            {
                if (this.OnAuthRequire != null)
                {
                    Models.Client client = _Clients.Single(c => c.SessionName == sessionName);

                    string fullCode = token.fullCode;

                    OnAuthRequire(client, fullCode);
                }
            }

            return true;
        }

        private bool BrowserPage_OnMessageReceived(string sessionName, ExpandoObject message)
        {
            Models.Client client = _Clients.Single(c => c.SessionName == sessionName);

            dynamic response = message;

            Models.Message messageObj = new Models.Message()
            {
                Id = response.id.id,
                Body = response.caption,
                From = response.from.Substring(0, response.from.IndexOf('@'))
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

            CheckVersion();

            SessionStart();
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
        }

        private void CheckVersion()
        {
            string versionUrl;

            if (Config.Version == Models.Enum.LibVersion.Latest)
                versionUrl = "https://api.github.com/repos/wppconnect-team/wa-js/releases/latest";
            else
                versionUrl = "https://api.github.com/repos/wppconnect-team/wa-js/releases/tags/nightly";

            RestClient client = new RestClient(versionUrl);

            RestRequest request = new RestRequest();

            RestResponse response = client.GetAsync(request).Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
            {
                JObject json = JObject.Parse(response.Content);

                string version = (string)json["name"];

                Console.WriteLine($"[wa-js : {version}]");
            }
            else
                Console.WriteLine("[wa-js version:não foi possível obter a versão]");

            Console.WriteLine("");
        }

        private void SessionStart()
        {
            if (Config.SessionStart)
            {
                Console.WriteLine($"[wa-js : Sessions Starting]");

                List<string> listSessionFiles = Directory.GetFiles($"{AppDomain.CurrentDomain.BaseDirectory}\\{Config.SessionFolderName}").ToList();

                foreach (string fileSession in listSessionFiles)
                {
                    string jsonTxt = File.ReadAllText(fileSession);

                    Models.Token token = JsonConvert.DeserializeObject<Models.Token>(jsonTxt);

                    string sessionName = Path.GetFileName(fileSession).Replace(Path.GetExtension(fileSession), "");

                    await CreateSession(sessionName, token);
                }
            }
        }

        private void SessionSave(Models.Client client, Models.Token token)
        {
            if (Config.TokenStore == Models.Enum.TokenStore.File)
            {
                string tokenJson = JsonConvert.SerializeObject(token);

                Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}\\{Config.SessionFolderName}");

                File.WriteAllText($"{AppDomain.CurrentDomain.BaseDirectory}{Config.SessionFolderName}\\{client.SessionName}.json", tokenJson);
            }
        }

        private void SessionRemove(Models.Client client)
        {
            File.Delete($"{AppDomain.CurrentDomain.BaseDirectory}\\{Config.SessionFolderName}\\{client.SessionName}.json");
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

                    #region Inject

                    PageAddScriptTagOptions pageAddScriptTagOptions = new PageAddScriptTagOptions();

                    if (Config.Version == Models.Enum.LibVersion.Latest)
                        pageAddScriptTagOptions.Url = "https://github.com/wppconnect-team/wa-js/releases/latest/download/wppconnect-wa.js";
                    if (Config.Version == Models.Enum.LibVersion.Nightly)
                        pageAddScriptTagOptions.Url = "https://github.com/wppconnect-team/wa-js/releases/download/nightly/wppconnect-wa.js";

                    await client.Connection.BrowserPage.AddScriptTagAsync(pageAddScriptTagOptions);

                    #endregion

                    #region Events

                    await client.Connection.BrowserPage.EvaluateAsync("async => WPP.webpack.onReady(function () { console.log('onReady') })");

                    //Auth - Require
                    await client.Connection.BrowserPage.ExposeFunctionAsync<string, object, bool>("browserPage_OnAuthRequire", BrowserPage_OnAuthRequire);
                    await client.Connection.BrowserPage.EvaluateAsync("async => WPP.conn.on('require_auth', function(e) { console.log('require_auth') })");

                    //Auth - Authenticated
                    await client.Connection.BrowserPage.ExposeFunctionAsync<string, Task<bool>>("browserPage_OnAuthAuthenticated", BrowserPage_OnAuthAuthenticated);
                    await client.Connection.BrowserPage.EvaluateAsync("async => WPP.conn.on('authenticated', function(e) { browserPage_OnAuthAuthenticated('" + client.SessionName + "') })");

                    //Auth - CodeChange
                    await client.Connection.BrowserPage.ExposeFunctionAsync<string, object, bool>("browserPage_OnAuthCodeChange", BrowserPage_OnAuthCodeChange);
                    await client.Connection.BrowserPage.EvaluateAsync("async => WPP.conn.on('auth_code_change', function(e) { console.log('auth_code_change') })");

                    //Auth - Logout
                    await client.Connection.BrowserPage.ExposeFunctionAsync<string, bool>("browserPage_OnAuthLogout", BrowserPage_OnAuthLogout);
                    await client.Connection.BrowserPage.EvaluateAsync("async => WPP.conn.on('logout', function() { browserPage_OnAuthLogout('" + client.SessionName + "') })");

                    //Chat - OnMessageReceived
                    //await client.Connection.BrowserPage.ExposeFunctionAsync<string, ExpandoObject, bool>("browserPage_OnMessageReceived", BrowserPage_OnMessageReceived);
                    //await client.Connection.BrowserPage.EvaluateAsync("async => WPP.whatsapp.MsgStore.on('change', function(e) { browserPage_OnMessageReceived('" + client.SessionName + "', e) })");

                    #endregion

                    _Clients.Add(client);
                }
                else
                    throw new Exception($"Já existe uma session com o nome {sessionName}");

                if (Config.Debug)
                    Console.WriteLine($"[{sessionName}:client] Initialized");

                Models.Session session = await client.QrCode();

                if (session.Status == Models.Enum.Status.QrCode)
                {
                    dynamic qrCodeJson = new JObject();
                    qrCodeJson.fullCode = session.Mensagem;

                    BrowserPage_OnAuthCodeChange(client.SessionName, qrCodeJson);
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