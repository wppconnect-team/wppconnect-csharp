using Newtonsoft.Json.Linq;
using WPPConnect.Utils;

namespace WPPConnect
{
    public static class WPPClient
    {
        public static async Task<Models.Session> Status(this Models.Instance instance)
        {
            try
            {
                bool authenticated = await instance.Connection.BrowserPage.EvaluateAsync<bool>("async => WPP.conn.isAuthenticated()");

                if (authenticated)
                    instance.Session.Status = Models.Enum.Status.Connected;
                else
                    instance.Session.Status = Models.Enum.Status.Disconnected;

                return instance.Session;
            }
            catch (Exception)
            {
                instance.Session.Status = Models.Enum.Status.ERROR;

                return instance.Session;
            }
        }

        public static async Task<Models.Session> QrCode(this Models.Instance instance)
        {
            Models.Session session = await Status(instance);

            try
            {
                if (session.Status == Models.Enum.Status.Disconnected)
                {
                    dynamic response = await instance.Connection.BrowserPage.EvaluateAsync<System.Dynamic.ExpandoObject>("async => WPP.conn.getAuthCode()");

                    string fullCode = response.fullCode;

                    session.Status = Models.Enum.Status.QrCode;
                    session.QrCode = fullCode;

                    return session;
                }

                return session;
            }
            catch (Exception)
            {
                session.Status = Models.Enum.Status.ERROR;

                return session;
            }
        }

        public static async Task<Models.Session> Logout(this Models.Instance instance)
        {
            Models.Session session = await Status(instance);

            try
            {
                if (session.Status == Models.Enum.Status.Connected)
                {
                    bool logout = await instance.Connection.BrowserPage.EvaluateAsync<bool>("async => WPP.conn.logout()");

                    await Logout(instance);

                    session.Status = Models.Enum.Status.Disconnected;

                    return session;
                }

                session.Status = Models.Enum.Status.Disconnected;

                return session;
            }
            catch (Exception)
            {
                session.Status = Models.Enum.Status.ERROR;

                return session;
            }
        }

        public static async Task<bool> SendMessage(this Models.Instance instance, Models.Message message)
        {
            try
            {
                Models.Session session = await Status(instance);

                if (session.Status == Models.Enum.Status.Connected)
                {
                    await message.ValidateMessage(instance);

                    switch (message.Type)
                    {
                        case Models.Enum.MessageType.Document:
                            {
                                string command = "async => WPP.chat.sendFileMessage('" + message.Recipient + "', '" + message.File.Base64 + "', { createChat: true, type: 'document', filename: '" + message.File.Name + "', caption: '" + message.Content + "' })";

                                await instance.Connection.BrowserPage.EvaluateAsync(command);

                                break;
                            }
                        case Models.Enum.MessageType.Image:
                            {
                                string command = "async => WPP.chat.sendFileMessage('" + message.Recipient + "', '" + message.File.Base64 + "', { createChat: true, type: 'image', filename: '" + message.File.Name + "', caption: '" + message.Content + "' })";

                                await instance.Connection.BrowserPage.EvaluateAsync(command);

                                break;
                            }
                        case Models.Enum.MessageType.Text:
                            {
                                string command = "async => WPP.chat.sendTextMessage('" + message.Recipient + "', '" + message.Content + "', { createChat: true })";

                                await instance.Connection.BrowserPage.EvaluateAsync(command);

                                break;
                            }
                    }

                    return true;
                }

                throw new Exception("Você não está conectado a session");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static async Task GetMessages(this Models.Instance instance, string sender, int quantidade)
        {
            try
            {
                Models.Session session = await Status(instance);

                if (session.Status == Models.Enum.Status.Connected)
                {
                    await sender.ValidateNumber(instance);

                    var messages = await instance.Connection.BrowserPage.EvaluateHandleAsync("WPP.chat.getMessages('" + sender + "@c.us', { count: " + quantidade + " }).then(messages => JSON.stringify(messages));");
                    
                    string messagesJSON = await messages.JsonValueAsync<string>();

                    JArray messagesJArray = JArray.Parse(messagesJSON);

                    foreach (dynamic item in messagesJArray)
                    {
                        string id = item.id.id;
                    }
                }

                throw new Exception("Você não está conectado a session");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task Testes(this Models.Instance instance)
        {
            try
            {
                Models.Session session = await Status(instance);

                if (session.Status == Models.Enum.Status.Connected)
                {
                    await GetMessages(instance, "556492015016", 3);

                    var messages = await instance.Connection.BrowserPage.EvaluateHandleAsync("WPP.chat.getMessages('556492015016@c.us', { count: 3 }).then(messages => JSON.stringify(messages));");

                    string messagesJSON = await messages.JsonValueAsync<string>();

                    JArray messagesJArray = JArray.Parse(messagesJSON);

                    foreach (dynamic item in messagesJArray)
                    {
                        string id = item.id.id;
                    }

                    var messages2 = await instance.Connection.BrowserPage.EvaluateHandleAsync("WPP.chat.getMessages('556492015016@c.us', { count: 3 }).then(messages => messages);");
                    var messages3 = await messages2.JsonValueAsync<string>();

                    var messages4 = await instance.Connection.BrowserPage.EvaluateHandleAsync("WPP.chat.getMessages('556492015016@c.us', { count: 3 });");
                    var messages5 = await messages4.JsonValueAsync<string>();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}