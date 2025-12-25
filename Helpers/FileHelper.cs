using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Helper class for file operations
    /// کلاس کمکی برای عملیات فایل
    /// </summary>
    public static class FileHelper
    {
        #region Read Operations

        /// <summary>
        /// Reads all text from a file safely
        /// خواندن متن فایل به صورت امن
        /// </summary>
        public static string ReadAllText(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            return File.Exists(path) ? File.ReadAllText(path) : null;
        }

        /// <summary>
        /// Reads JSON from a file
        /// خواندن JSON از فایل
        /// </summary>
        public static T ReadJson<T>(string path)
        {
            var json = ReadAllText(path);
            return string.IsNullOrEmpty(json) ? default(T) : JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Reads all lines from a file
        /// خواندن تمام خطوط فایل
        /// </summary>
        public static string[] ReadAllLines(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return new string[0];

            return File.ReadAllLines(path);
        }

        #endregion

        #region Write Operations

        /// <summary>
        /// Writes text to a file
        /// نوشتن متن در فایل
        /// </summary>
        public static void WriteAllText(string path, string content)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));

            // Create directory if it doesn't exist
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, content ?? string.Empty);
        }

        /// <summary>
        /// Writes JSON to a file
        /// نوشتن JSON در فایل
        /// </summary>
        public static void WriteJson<T>(string path, T data, Formatting formatting = Formatting.Indented)
        {
            var json = JsonConvert.SerializeObject(data, formatting);
            WriteAllText(path, json);
        }

        #endregion

        #region File Information

        /// <summary>
        /// Gets file size in bytes
        /// دریافت حجم فایل
        /// </summary>
        public static long GetFileSize(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return 0;

            return new FileInfo(path).Length;
        }

        /// <summary>
        /// Checks if file has specific extension
        /// بررسی پسوند فایل
        /// </summary>
        public static bool HasExtension(string fileName, params string[] extensions)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            var ext = Path.GetExtension(fileName)?.ToLower();
            return extensions.Any(e => e.ToLower() == ext || $".{e.ToLower()}" == ext);
        }

        /// <summary>
        /// Gets file extension without dot
        /// دریافت پسوند بدون نقطه
        /// </summary>
        public static string GetExtensionWithoutDot(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return string.Empty;

            var ext = Path.GetExtension(fileName);
            return ext?.TrimStart('.');
        }

        #endregion

        #region File Name Operations

        /// <summary>
        /// Generates a unique file name
        /// تولید نام فایل یکتا
        /// </summary>
        public static string GenerateUniqueFileName(string originalFileName)
        {
            if (string.IsNullOrWhiteSpace(originalFileName))
                return Guid.NewGuid().ToString();

            var ext = Path.GetExtension(originalFileName);
            var name = Path.GetFileNameWithoutExtension(originalFileName);
            return $"{name}_{Guid.NewGuid():N}{ext}";
        }

        /// <summary>
        /// Generates a file name with timestamp
        /// تولید نام فایل با زمان
        /// </summary>
        public static string GenerateFileNameWithTimestamp(string originalFileName)
        {
            if (string.IsNullOrWhiteSpace(originalFileName))
                return $"{DateTime.Now:yyyyMMddHHmmss}";

            var ext = Path.GetExtension(originalFileName);
            var name = Path.GetFileNameWithoutExtension(originalFileName);
            return $"{name}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
        }

        /// <summary>
        /// Sanitizes file name (removes invalid characters)
        /// پاکسازی نام فایل
        /// </summary>
        public static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return string.Empty;

            var invalidChars = Path.GetInvalidFileNameChars();
            return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        }

        #endregion

        #region File Operations

        /// <summary>
        /// Copies a file safely
        /// کپی امن فایل
        /// </summary>
        public static bool SafeCopy(string source, string destination, bool overwrite = false)
        {
            if (string.IsNullOrWhiteSpace(source) || !File.Exists(source))
                return false;

            try
            {
                var directory = Path.GetDirectoryName(destination);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.Copy(source, destination, overwrite);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Moves a file safely
        /// انتقال امن فایل
        /// </summary>
        public static bool SafeMove(string source, string destination, bool overwrite = false)
        {
            if (string.IsNullOrWhiteSpace(source) || !File.Exists(source))
                return false;

            try
            {
                var directory = Path.GetDirectoryName(destination);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (overwrite && File.Exists(destination))
                {
                    File.Delete(destination);
                }

                File.Move(source, destination);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Deletes a file safely
        /// حذف امن فایل
        /// </summary>
        public static bool SafeDelete(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return false;

            try
            {
                File.Delete(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Directory Operations

        /// <summary>
        /// Creates directory if it doesn't exist
        /// ایجاد پوشه در صورت عدم وجود
        /// </summary>
        public static void EnsureDirectoryExists(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Gets all files in directory with specific extensions
        /// دریافت فایل‌ها با پسوند خاص
        /// </summary>
        public static string[] GetFilesByExtensions(string directoryPath, params string[] extensions)
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
                return new string[0];

            return Directory.GetFiles(directoryPath)
                .Where(file => HasExtension(file, extensions))
                .ToArray();
        }

        #endregion
    }
}
