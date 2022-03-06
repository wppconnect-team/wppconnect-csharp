namespace WPPConnect.Utils
{
    public static class Functions
    {
        internal async static Task<bool> Validate(this Models.Message message, Models.Instance instance)
        {
            message.Number = message.Number.Replace("+", "");

            dynamic validateNumber = await instance.Connection.BrowserPage.EvaluateAsync<object>($"async => WPP.contact.queryExists('{message.Number}@c.us')");

            if (validateNumber == null)
                throw new Exception($"O número {message.Number} não é válido");
            else
                message.Number = validateNumber.wid._serialized;

            return true;
        }
    }
}