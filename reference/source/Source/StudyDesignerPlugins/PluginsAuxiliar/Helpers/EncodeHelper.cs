namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers
{
    using System.Security.Cryptography;
    using System.Text;

    public static class EncodeHelper
    {
        public static string ComputeSha256Hash(string rawData)
        {
            using (var sha256Hash = SHA256.Create())
            {
                // Convert the input string to a byte array and compute the hash
                var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a hex string
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2")); // lowercase hex
                }
                return builder.ToString();
            }
        }
    }
}
