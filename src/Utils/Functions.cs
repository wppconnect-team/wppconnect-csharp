namespace WPPConnect.Utils
{
    public static class Functions
    {
        internal async static Task<bool> Validate(this Models.Message message, Models.Instance instance)
        {
            message.Recipient = message.Recipient.Replace("+", "");

            dynamic validateNumber = await instance.Connection.BrowserPage.EvaluateAsync<object>($"async => WPP.contact.queryExists('{message.Recipient}@c.us')");

            if (validateNumber == null)
                throw new Exception($"O número {message.Recipient} não é válido");
            else
                message.Recipient = validateNumber.wid._serialized;

            return true;
        }
    }
}