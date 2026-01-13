using System.Text.RegularExpressions;

namespace AccountManagement

{
    partial class Helper
    {
        public class  InputValidation() {
            // AI 
            private const string EmailPattern = 
                @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
                + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
                + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

            // not ai 
            private const string StringPattern100Chars =  @"^.{100}$";  // limit to 100 characters only
            

            private static readonly Regex EmailRegex = 
                new Regex(EmailPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            private static readonly Regex StringRegex = new Regex(StringPattern100Chars);



            public static bool IsValidEmail(string email)
            {
                if (string.IsNullOrWhiteSpace(email))
                    return false;

                return EmailRegex.IsMatch(email);
            }


            public static bool IsString(string stringValue)
            {
                if (string.IsNullOrWhiteSpace(stringValue))
                    return false;

                return StringRegex.IsMatch(stringValue);
            }


        }
    }
}
