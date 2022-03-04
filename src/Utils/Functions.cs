using PhoneNumbers;

namespace WPPConnect.Utils
{
    public static class Functions
    {
        public static bool Validate(this Models.Message message)
        {
            PhoneNumberUtil phoneNumberUtil = PhoneNumberUtil.GetInstance();

            PhoneNumber phoneNumber = phoneNumberUtil.Parse(message.Number, "BR");

            bool valid = phoneNumberUtil.IsValidNumber(phoneNumber);

            if (valid)
                return true;
            else
                throw new Exception($"O número {message.Number} não é válido");
        }
    }
}