using Microsoft.Playwright;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Dynamic;
using System.Security.AccessControl;

namespace WPPConnect
{
    public class WPPConnection
    {
        #region Properties

        public Models.Config Config { get; internal set; }

        private static List<Models.Instance> _Instances = new List<Models.Instance>();

        #endregion

        #region Events

        //Auth - Authenticated
        public delegate void OnAuthAuthenticatedEventHandler(Models.Instance instance);

        public event OnAuthAuthenticatedEventHandler OnAuthAuthenticated;

        private async Task BrowserPage_OnAuthAuthenticated(string sessionName)
        {
            Models.Instance instance = _Instances.Single(c => c.Session.Name == sessionName);

            Console.WriteLine($"[{instance.Session.Name}:client] Authenticated");

            if (this.OnAuthAuthenticated != null)
                OnAuthAuthenticated(instance);
        }

        //Auth - CodeChange
        public delegate void OnAuthCodeChangeEventHandler(Models.Instance insntance, string token);

        public event OnAuthCodeChangeEventHandler OnAuthCodeChange;

        private async Task BrowserPage_OnAuthCodeChange(string sessionName, dynamic token)
        {
            if (this.OnAuthCodeChange != null && token != null)
            {
                Models.Instance instance = _Instances.Single(c => c.Session.Name == sessionName);

                string fullCode = token.fullCode;

                OnAuthCodeChange(instance, fullCode);
            }
        }

        //Auth - Logout
        public delegate void OnAuthLogoutEventHandler(Models.Instance instance);

        public event OnAuthLogoutEventHandler OnAuthLogout;

        private async Task BrowserPage_OnAuthLogout(string sessionName)
        {
            Models.Instance instance = await Instance(sessionName);

            await InstanceClose(instance);

            if (this.OnAuthLogout != null)
            {
                OnAuthLogout(instance);
            }
        }

        //Auth - Require
        public delegate void OnAuthRequireEventHandler(Models.Instance instance, string token);

        public event OnAuthRequireEventHandler OnAuthRequire;

        private async Task BrowserPage_OnAuthRequire(string sessionName, dynamic token)
        {
            if (this.OnAuthRequire != null && token != null)
            {
                Models.Instance instance = _Instances.Single(c => c.Session.Name == sessionName);

                string fullCode = token.fullCode;

                OnAuthRequire(instance, fullCode);
            }
        }

        //Chat - OnMessageReceived
        public delegate void OnMessageReceivedEventHandler(Models.Instance instance, Models.Message message);

        public event OnMessageReceivedEventHandler OnMessageReceived;

        private async Task BrowserPage_OnMessageReceived(string sessionName, ExpandoObject message)
        {
            if (this.OnMessageReceived != null)
            {
                Models.Instance instance = _Instances.Single(c => c.Session.Name == sessionName);

                dynamic response = message;

                Models.Message messageObj = new Models.Message()
                {
                    Id = response.id.id,
                    Content = response.body,
                    Recipient = response.from.Substring(0, response.from.IndexOf('@'))
                };

                OnMessageReceived(instance, messageObj);
            }
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

            SessionsStart();
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
            try
            {
                string versionUrl;

                if (Config.Version == Models.Enum.LibVersion.Latest)
                    versionUrl = "https://api.github.com/repos/wppconnect-team/wa-js/releases/latest";
                else
                    versionUrl = "https://api.github.com/repos/wppconnect-team/wa-js/releases/tags/nightly";

                RestClient client = new RestClient(versionUrl);

                RestRequest request = new RestRequest();
                request.Timeout = 5000;

                RestResponse response = client.GetAsync(request).Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
                {
                    JObject json = JObject.Parse(response.Content);

                    string version = (string)json["name"];

                    Console.WriteLine($"[wa-js : {version}]");
                }
                else
                    throw new Exception("[wa-js version:não foi possível obter a versão]");

                Console.WriteLine("");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void SessionsStart()
        {
            if (Config.SessionsStart)
            {
                Console.WriteLine($"[wa-js : Sessions Starting]");
                Console.WriteLine();

                string directory = $"{AppDomain.CurrentDomain.BaseDirectory}\\{Config.SessionsFolderName}";

                Directory.CreateDirectory(directory);

                List<string> listSessionFolders = Directory.GetDirectories(directory).ToList();

                foreach (string sessionFolderPath in listSessionFolders)
                {
                    DirectoryInfo folder = new DirectoryInfo(sessionFolderPath);

                    if (folder.GetDirectories().Length >= 2)
                    {
                        SessionCreate(folder.Name, true).Wait();
                    }
                    else
                    {
                        Directory.Delete($"{AppDomain.CurrentDomain.BaseDirectory}\\{Config.SessionsFolderName}\\{folder.Name}", true);
                    }
                }

                Console.WriteLine($"[wa-js : Sessions Started]");
                Console.WriteLine();
            }
        }

        private async Task InstanceClose(Models.Instance instance)
        {
            await instance.Connection.BrowserContext.Pages[0].CloseAsync();
            await instance.Connection.BrowserContext.CloseAsync();

            Console.WriteLine($"[{instance.Session.Name}:browser] Closed");

            _Instances.Remove(instance);

            Console.WriteLine($"[{instance.Session.Name}:session] Closed");

            Directory.Delete($"{AppDomain.CurrentDomain.BaseDirectory}\\{Config.SessionsFolderName}\\{instance.Session.Name}", true);
        }

        #endregion

        #region Methods - Public

        public async Task SessionCreate(string sessionName, bool token = false)
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

                Models.Instance instance = _Instances.SingleOrDefault(i => i.Session.Name == sessionName);

                if (instance == null)
                {
                    Console.WriteLine($"[{sessionName}:browser] Initializing");

                    if (!string.IsNullOrEmpty(Config.BrowserWsUrl))
                    {
                        BrowserTypeConnectOverCDPOptions browserTypeConnectOptions = new BrowserTypeConnectOverCDPOptions()
                        {
                            Timeout = 5000
                        };

                        IBrowser browser = await playwrightBrowser.ConnectOverCDPAsync(Config.BrowserWsUrl);

                        IBrowserContext browserContext = browser.Contexts[0];

                        instance = new Models.Instance(sessionName, browserContext);
                    }
                    else
                    {
                        string[] args = new string[]
                                {
                                  "--enable-gpu",
                                  "--display-entrypoints",
                                  "--disable-http-cache",
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
                                  "--disable-popup-blocking",
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

                        BrowserTypeLaunchPersistentContextOptions launchOptions = new BrowserTypeLaunchPersistentContextOptions
                        {
                            Args = Config.Headless == true ? args : new string[0],
                            Headless = Config.Headless,
                            Devtools = Config.Devtools,
                            Channel = "chrome",
                            BypassCSP = true,
                            UserAgent = "WhatsApp/2.2043.8 Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.121 Safari/537.36"
                        };

                        IBrowserContext browserContext = await playwrightBrowser.LaunchPersistentContextAsync($"{AppDomain.CurrentDomain.BaseDirectory}\\{Config.SessionsFolderName}\\{sessionName}", launchOptions);

                        instance = new Models.Instance(sessionName, browserContext);
                    }

                    Console.WriteLine($"[{sessionName}:browser] Initialized");

                    Console.WriteLine($"[{instance.Session.Name}:client] Initializing");

                    await instance.Connection.BrowserPage.GotoAsync("https://web.whatsapp.com");

                    #region Inject

                    PageAddScriptTagOptions pageAddScriptTagOptions = new PageAddScriptTagOptions();

                    if (Config.Version == Models.Enum.LibVersion.Latest)
                        pageAddScriptTagOptions.Url = "https://github.com/wppconnect-team/wa-js/releases/latest/download/wppconnect-wa.js";
                    if (Config.Version == Models.Enum.LibVersion.Nightly)
                        pageAddScriptTagOptions.Url = "https://github.com/wppconnect-team/wa-js/releases/download/nightly/wppconnect-wa.js";

                    await instance.Connection.BrowserPage.AddScriptTagAsync(pageAddScriptTagOptions);

                    #endregion

                    bool isAuthenticated = await instance.Connection.BrowserPage.EvaluateAsync<bool>("async () => WPP.conn.isAuthenticated()");

                    if (!isAuthenticated && token)
                    {
                        Console.WriteLine($"[{instance.Session.Name}:client] Authentication Failed");

                        await InstanceClose(instance);

                        return;
                    }

                    #region Events

                    //Auth - Require
                    await instance.Connection.BrowserPage.ExposeFunctionAsync<string, object, Task>("browserPage_OnAuthRequire", BrowserPage_OnAuthRequire);
                    await instance.Connection.BrowserPage.EvaluateAsync("async => WPP.conn.on('require_auth', function(e) { browserPage_OnAuthRequire('" + instance.Session.Name + "') })");

                    //Auth - Authenticated
                    await instance.Connection.BrowserPage.ExposeFunctionAsync<string, Task>("browserPage_OnAuthAuthenticated", BrowserPage_OnAuthAuthenticated);
                    await instance.Connection.BrowserPage.EvaluateAsync("async => WPP.conn.on('authenticated', function(e) { browserPage_OnAuthAuthenticated('" + instance.Session.Name + "') })");

                    //Auth - CodeChange
                    await instance.Connection.BrowserPage.ExposeFunctionAsync<string, object, Task>("browserPage_OnAuthCodeChange", BrowserPage_OnAuthCodeChange);
                    await instance.Connection.BrowserPage.EvaluateAsync("async => WPP.conn.on('auth_code_change', function(e) { browserPage_OnAuthCodeChange('" + instance.Session.Name + "', e) })");

                    //Auth - Logout
                    await instance.Connection.BrowserPage.ExposeFunctionAsync<string, Task>("browserPage_OnAuthLogout", BrowserPage_OnAuthLogout);
                    await instance.Connection.BrowserPage.EvaluateAsync("async => WPP.conn.on('logout', function() { browserPage_OnAuthLogout('" + instance.Session.Name + "') })");

                    //Chat - OnMessageReceived
                    await instance.Connection.BrowserPage.ExposeFunctionAsync<string, ExpandoObject, Task>("browserPage_OnMessageReceived", BrowserPage_OnMessageReceived);
                    await instance.Connection.BrowserPage.EvaluateAsync("async => WPP.whatsapp.MsgStore.on('change', function(e) { browserPage_OnMessageReceived('" + instance.Session.Name + "', e) })");

                    #endregion

                    _Instances.Add(instance);
                }
                else
                    throw new Exception($"Já existe uma session com o nome {sessionName}");

                Models.Session session = await instance.QrCode();

                if (session.Status == Models.Enum.Status.QrCode)
                {
                    Console.WriteLine($"[{sessionName}:client] Authentication Required");

                    dynamic qrCodeJson = new JObject();
                    qrCodeJson.fullCode = session.QrCode;

                    BrowserPage_OnAuthCodeChange(instance.Session.Name, qrCodeJson);
                }

                Console.WriteLine($"[{sessionName}:client] Initialized");
                Console.WriteLine();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("WPP is not defined") || e.Message.Contains("Execution context was destroyed"))
                    return;
                else
                    throw new Exception(e.Message);
            }
        }

        public async Task<Models.Instance> Instance(string sessionName)
        {
            Models.Instance instance = _Instances.SingleOrDefault(c => c.Session.Name == sessionName);

            if (instance == null)
                throw new Exception($"Não foi encontrado nenhuma sessão com o nome {sessionName}");
            else
                return instance;
        }

        #endregion
    }
}