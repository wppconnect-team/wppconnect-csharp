namespace WPPConnect.Utils
{
    public static class Functions
    {
        internal async static Task<bool> Validate(this Models.Message message, Models.Instance instance)
        {
            //Recipient
            message.Recipient = message.Recipient.Replace("+", "");

            dynamic validateNumber = await instance.Connection.BrowserPage.EvaluateAsync<object>($"async => WPP.contact.queryExists('{message.Recipient}@c.us')");

            if (validateNumber == null)
                throw new Exception($"O número {message.Recipient} não é válido");
            else
                message.Recipient = validateNumber.wid._serialized;

            //Content
            if (message.Type == Models.Enum.MessageType.Image || message.Type == Models.Enum.MessageType.Document)
            {
                if (message.File == null || string.IsNullOrEmpty(message.File?.Base64))
                    throw new Exception($"É necessário informar um arquivo");

                if (string.IsNullOrEmpty(message.File?.Name))
                    throw new Exception($"O nome do arquivo não é válido");

                //if (!Base64Valid(message.File.Base64))
                    //throw new Exception($"O conteúdo do arquivo não é válido");
            }

            return true;
        }

        internal static bool Base64Valid(string base64)
        {
            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);

            return Convert.TryFromBase64String(base64, buffer, out int _);
        }
    }
}