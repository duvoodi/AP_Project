using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AP_Project.Data
{
    public static class ComputeHash
    {
        public static string Sha1(string input)
        {
            using (var sha1 = SHA1.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha1.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}