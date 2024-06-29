using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace mACRON
{
    public static class EncryptionHelper
    {
        private static readonly string EncryptionKey = "4D5F7E9C6B8A1E3D2F4C7A9B0C2E5F6G"; // Должен быть 32 байта для AES-256

        public static string Encrypt(string plainText)
        {
            byte[] key = Encoding.UTF8.GetBytes(EncryptionKey);
            using (Aes aesAlg = Aes.Create())
            {
                using (var encryptor = aesAlg.CreateEncryptor(key, aesAlg.IV))
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        msEncrypt.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int));
                        msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
        }

        public static string Decrypt(string cipherText)
        {
            byte[] fullCipher = Convert.FromBase64String(cipherText);
            byte[] ivLength = new byte[sizeof(int)];
            Array.Copy(fullCipher, ivLength, ivLength.Length);
            int ivSize = BitConverter.ToInt32(ivLength, 0);
            byte[] iv = new byte[ivSize];
            Array.Copy(fullCipher, ivLength.Length, iv, 0, iv.Length);
            byte[] cipher = new byte[fullCipher.Length - ivLength.Length - iv.Length];
            Array.Copy(fullCipher, ivLength.Length + iv.Length, cipher, 0, cipher.Length);

            byte[] key = Encoding.UTF8.GetBytes(EncryptionKey);
            using (Aes aesAlg = Aes.Create())
            {
                using (var decryptor = aesAlg.CreateDecryptor(key, iv))
                using (var msDecrypt = new MemoryStream(cipher))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }

        public static string GenerateEncryptionKey()
        {
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                byte[] keyBytes = new byte[32]; // 256 бит = 32 байта
                rng.GetBytes(keyBytes);
                return Convert.ToBase64String(keyBytes);
            }
        }
    }
}
