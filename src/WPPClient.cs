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
                    message.Validate();

                    await instance.Connection.BrowserPage.EvaluateAsync("async => WPP.chat.sendTextMessage('" + message.Number + "@c.us', '" + message.Content + "', { createChat: true })");

                    return true;
                }

                throw new Exception("Você não está conectado a session");
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}