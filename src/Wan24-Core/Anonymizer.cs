using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace wan24.Core
{
    /// <summary>
    /// Data anonymization helper
    /// </summary>
    public static partial class Anonymizer
    {
        /// <summary>
        /// Regular expression for the <see cref="AnonymizeIpAddress(IPAddress)"/> method for IPv4 addresses
        /// </summary>
        private static readonly Regex RX_IPv4 = RxIpv4_Generator();
        /// <summary>
        /// Regular expression for the <see cref="AnonymizeIpAddress(IPAddress)"/> method for IPv6 addresses
        /// </summary>
        private static readonly Regex RX_IPv6 = RxIpv6_Generator();
        /// <summary>
        /// Regular expression to remove all non-numeric characters from a credit card number
        /// </summary>
        private static readonly Regex RX_CC = RxCcGenerator();
        /// <summary>
        /// IBAN syntax regular expression (<c>$1</c> is the country, <c>$2</c> the checksum, <c>$3</c> the bank ID and <c>$4</c> the account ID)
        /// </summary>
        private static readonly Regex RX_IBAN = RxIbanGenerator();

        /// <summary>
        /// Anonymize an IP address (works for IPv4/6 addresses)
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <returns>Anonymized IP address</returns>
        public static string AnonymizeIpAddress(this IPAddress ip)
        {
            string str = ip.ToString();
            return ip.AddressFamily switch
               {
                   AddressFamily.InterNetwork => AnonymizeIPv4Address(str),
                   AddressFamily.InterNetworkV6 => AnonymizeIPv6Address(str),
                   _ => str
            };
        }

        /// <summary>
        /// Anonymize an IPv4 address
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <returns>Anonymized IP address</returns>
        public static string AnonymizeIPv4Address(string ip) => RX_IPv4.Replace(ip, "$1.xxx.xxx");

        /// <summary>
        /// Anonymize an IPv6 address
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <returns>Anonymized IP address</returns>
        public static string AnonymizeIPv6Address(string ip) => RX_IPv6.IsMatch(ip) ? RX_IPv6.Replace(ip, "$1:xxxx:xxxx:xxxx:xxxx") : ip;

        /// <summary>
        /// Anonymize an email address (anonymizes the alias only)
        /// </summary>
        /// <param name="email">Email address</param>
        /// <returns>Email address</returns>
        public static string AnonymizeEmailAddress(this MailAddress email) => AnonymizeEmailAddress($"{email.User}@{email.Host}");

        /// <summary>
        /// Anonymize an email address (anonymizes the alias only)
        /// </summary>
        /// <param name="email">Email address</param>
        /// <returns>Email address</returns>
        public static string AnonymizeEmailAddress(in string email)
        {
            string[] parts = email.Split('@', 2);
            return parts[0].Length switch
            {
                0 => parts[0],
                1 => $"x@{parts[1]}",
                2 => $"x{parts[0][1]}@{parts[1]}",
                _ => $"x{parts[0][1..(parts[0].Length - 2)]}x@{parts[1]}",
            };
        }

        /// <summary>
        /// Anonymize a credit card number (the end of the number will be anonymized)
        /// </summary>
        /// <param name="cc">Credit card number</param>
        /// <param name="addSpaces">If to separate with spaces after each 4 digits block</param>
        /// <returns>Anonymized credit card number</returns>
        public static string AnonymizeCreditCardNumber(in string cc, in bool addSpaces = true)
        {
            string number = RX_CC.Replace(cc, string.Empty);
            int len = (int)Math.Floor(number.Length / 2f),
                rest = number.Length - len;
            number = len < 1 ? number : $"{number[0..rest]}{new string('x', len)}";
            len = number.Length;
            StringBuilder res = new(capacity: len);
            for (int i = 0; i < len; i++)
            {
                res.Append(number[i]);
                if (addSpaces && (i + 1) % 4 == 0 && i + 1 < len) res.Append(' ');
            }
            return res.ToString();
        }

        /// <summary>
        /// Anonymize a phone number (the end of the number will be anonymized)
        /// </summary>
        /// <param name="phone">Phone number</param>
        /// <returns>Anonymized phone number</returns>
        public static string AnonymizePhoneNumber(in string phone)
        {
            ReadOnlySpan<char> phoneChars = phone;
            int numbers = phone.Aggregate(0, (a, b) => char.IsNumber(b) ? a++ : a),
                half = (int)Math.Floor(numbers / 2f),
                rest = numbers - half,
                index = -1;
            StringBuilder res = new(capacity: phoneChars.Length);
            for (int i = phoneChars.Length; i < phoneChars.Length; res.Append(char.IsNumber(phoneChars[i]) && ++index < rest ? 'x' : phoneChars[i]), i++) ;
            return res.ToString();
        }

        /// <summary>
        /// Anonymize a bank account number (only the last up to 4 digits will be visible; at last 50% of the bank account number will be anonymized for sure)
        /// </summary>
        /// <param name="ban">Bank account number</param>
        /// <returns>Anonymized bank account number</returns>
        public static string AnonymizeBankAccountNumber(in string ban)
        {
            string normalizedBan = RegularExpressions.RX_WHITESPACE.Replace(ban, string.Empty);
            int leaveDigits = (int)Math.Ceiling(Math.Min(4, normalizedBan.Length / 2f)),
                anonymizeDigits = normalizedBan.Length - leaveDigits;
            return leaveDigits < 1
                ? normalizedBan
                : $"{new string('x', anonymizeDigits)}{normalizedBan[anonymizeDigits..]}";
        }

        /// <summary>
        /// Anonymize an IBAN
        /// </summary>
        /// <param name="iban">IBAN</param>
        /// <returns>Anonymized IBAN</returns>
        public static string AnonymizeIban(in string iban)
        {
            string normalizedIban = RegularExpressions.RX_WHITESPACE.Replace(iban, string.Empty).ToUpper();
            return RX_IBAN.IsMatch(iban)
                ? RX_IBAN.Replace(normalizedIban, $"$1$2$3{AnonymizeBankAccountNumber(RX_IBAN.Replace(normalizedIban, "$4"))}")
                : iban;
        }

        /// <summary>
        /// Generate the <see cref="RX_IPv4"/>
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(@"^((\d+\.){2}).*$", RegexOptions.Compiled | RegexOptions.Singleline, 3000)]
        private static partial Regex RxIpv4_Generator();

        /// <summary>
        /// Generate the <see cref="RX_IPv6"/>
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(@"^(([0-9|a-f]{1,4}){4}).*$", RegexOptions.Compiled | RegexOptions.Singleline, 3000)]
        private static partial Regex RxIpv6_Generator();

        /// <summary>
        /// Generate the <see cref="RX_CC"/>
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(@"[^\d]", RegexOptions.Compiled | RegexOptions.Singleline, 3000)]
        private static partial Regex RxCcGenerator();

        /// <summary>
        /// IBAN syntax regular expression (<c>$1</c> is the country, <c>$2</c> the checksum, <c>$3</c> the bank ID and <c>$4</c> the account ID)
        /// </summary>
        [GeneratedRegex(@"^([A-Z]{2})(\d{2})(\d{8})(\d{10})$", RegexOptions.Compiled | RegexOptions.Singleline, 3000)]
        private static partial Regex RxIbanGenerator();
    }
}
