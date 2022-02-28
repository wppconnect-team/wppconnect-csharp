﻿using Newtonsoft.Json;

namespace WPPConnect
{
    public static class WPPClient
    {
        public static async Task<Models.Session> Status(this Models.Client client)
        {
            Models.Session session = new Models.Session(client);

            try
            {
                bool authenticated = await client.Connection.BrowserPage.EvaluateAsync<bool>("async => WPP.conn.isAuthenticated()");

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
                session.Status = Models.Enum.Status.ERROR;
                session.Mensagem = e.Message;

                return session;
            }
        }

        public static async Task<Models.Session> QrCode(this Models.Client client)
        {
            Models.Session session = await Status(client);

            try
            {
                if (session.Status == Models.Enum.Status.Desconectado)
                {
                    dynamic response = await client.Connection.BrowserPage.EvaluateAsync<System.Dynamic.ExpandoObject>("async => WPP.conn.getAuthCode()");

                    string fullCode = response.fullCode;

                    session.Status = Models.Enum.Status.QrCode;
                    session.Mensagem = fullCode;

                    return session;
                }

                return session;
            }
            catch (Exception)
            {
                return session;
            }
        }

        public static async Task<Models.Session> Disconnect(this Models.Client client)
        {
            Models.Session session = await Status(client);

            try
            {
                if (session.Status == Models.Enum.Status.Conectado)
                {
                    bool logout = await client.Connection.BrowserPage.EvaluateAsync<bool>("async => WPP.conn.logout()");

                    await Disconnect(client);

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

        public static async Task<Models.Token> Token(this Models.Client client)
        {
            Models.Session session = await Status(client);

            try
            {
                if (session.Status == Models.Enum.Status.Desconectado)
                {
                    dynamic response = await client.Connection.BrowserPage.EvaluateAsync<System.Dynamic.ExpandoObject>("async => WPP.conn.getAuthCode()");

                    Models.Token token = new Models.Token("", "", "", response.browserId, "");

                    return token;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<bool> SendMessage(this Models.Client client, Models.Message message)
        {
            try
            {
                Models.Session session = await Status(client);

                if (session.Status == Models.Enum.Status.Conectado)
                {
                    await client.Connection.BrowserPage.EvaluateAsync("async => WPP.chat.sendTextMessage('5564992176420@c.us', '{}', { createChat: true })");

                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}