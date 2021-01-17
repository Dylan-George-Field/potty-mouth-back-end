using System.Security.Cryptography;

namespace TryScanMe.Functions.Security
{
    public static class SampleAESKey
    {
        public static byte[] Key { get; }

        static SampleAESKey()
        {
            using (Aes aesAlg = Aes.Create())
            {
                Key = aesAlg.Key;
            }
        }
    }
}
