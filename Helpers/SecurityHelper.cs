using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Net;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Helper class for security operations
    /// کلاس کمکی برای عملیات امنیتی
    /// </summary>
    public static class SecurityHelper
    {
        #region Password Hashing

        /// <summary>
        /// Hashes a password using SHA256
        /// Hash کردن رمز عبور
        /// </summary>
        /// <param name="password">Password to hash</param>
        /// <returns>Hashed password</returns>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password));

            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        /// <summary>
        /// Hashes a password with salt
        /// Hash کردن رمز عبور با Salt
        /// </summary>
        public static string HashPasswordWithSalt(string password, string salt)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password));

            if (string.IsNullOrWhiteSpace(salt))
                throw new ArgumentNullException(nameof(salt));

            return HashPassword(password + salt);
        }

        #endregion

        #region Salt Generation

        /// <summary>
        /// Generates a random salt
        /// تولید Salt تصادفی
        /// </summary>
        /// <param name="size">Size in bytes (default: 32)</param>
        /// <returns>Base64 encoded salt</returns>
        public static string GenerateSalt(int size = 32)
        {
            if (size <= 0)
                throw new ArgumentException("Size must be greater than 0", nameof(size));

            var buffer = new byte[size];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(buffer);
            }
            return Convert.ToBase64String(buffer);
        }

        #endregion

        #region Encryption/Decryption

        /// <summary>
        /// Encrypts a string using AES
        /// رمزنگاری رشته
        /// </summary>
        /// <param name="plainText">Text to encrypt</param>
        /// <param name="key">Encryption key (32 characters)</param>
        /// <returns>Encrypted text (Base64)</returns>
        public static string Encrypt(string plainText, string key)
        {
            if (string.IsNullOrWhiteSpace(plainText))
                return plainText;

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            // Ensure key is 32 characters
            key = key.PadRight(32).Substring(0, 32);

            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    // Write IV to the beginning
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        /// <summary>
        /// Decrypts an encrypted string
        /// رمزگشایی رشته
        /// </summary>
        /// <param name="cipherText">Encrypted text (Base64)</param>
        /// <param name="key">Encryption key (32 characters)</param>
        /// <returns>Decrypted text</returns>
        public static string Decrypt(string cipherText, string key)
        {
            if (string.IsNullOrWhiteSpace(cipherText))
                return cipherText;

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            // Ensure key is 32 characters
            key = key.PadRight(32).Substring(0, 32);

            var buffer = Convert.FromBase64String(cipherText);

            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);

                // Extract IV from the beginning
                var iv = new byte[aes.IV.Length];
                Array.Copy(buffer, 0, iv, 0, iv.Length);
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream(buffer, iv.Length, buffer.Length - iv.Length))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        #endregion

        #region Token Generation

        /// <summary>
        /// Generates a random token
        /// تولید Token تصادفی
        /// </summary>
        /// <param name="length">Token length (default: 32)</param>
        /// <returns>Random token</returns>
        public static string GenerateRandomToken(int length = 32)
        {
            if (length <= 0)
                throw new ArgumentException("Length must be greater than 0", nameof(length));

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var result = new char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }

            return new string(result);
        }

        /// <summary>
        /// Generates a cryptographically secure random token
        /// تولید Token امن
        /// </summary>
        public static string GenerateSecureToken(int length = 32)
        {
            var buffer = new byte[length];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(buffer);
            }
            return Convert.ToBase64String(buffer);
        }

        #endregion

        #region Input Sanitization

        /// <summary>
        /// Sanitizes input to prevent XSS attacks
        /// پاکسازی ورودی برای جلوگیری از XSS
        /// </summary>
        /// <param name="input">User input</param>
        /// <returns>Sanitized input</returns>
        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            return WebUtility.HtmlEncode(input);
        }

        /// <summary>
        /// Removes potentially dangerous HTML tags
        /// حذف تگ‌های خطرناک HTML
        /// </summary>
        public static string RemoveDangerousTags(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return html;

            // Remove script tags
            html = System.Text.RegularExpressions.Regex.Replace(html, @"<script[^>]*>.*?</script>", "", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);

            // Remove iframe tags
            html = System.Text.RegularExpressions.Regex.Replace(html, @"<iframe[^>]*>.*?</iframe>", "", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);

            // Remove onclick and other event handlers
            html = System.Text.RegularExpressions.Regex.Replace(html, @"on\w+\s*=\s*[""'][^""']*[""']", "", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            return html;
        }

        #endregion

        #region GUID Generation

        /// <summary>
        /// Generates a new GUID
        /// تولید GUID جدید
        /// </summary>
        public static string GenerateGuid()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Generates a short unique ID (8 characters)
        /// تولید شناسه کوتاه یکتا
        /// </summary>
        public static string GenerateShortId()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        #endregion
    }
}
