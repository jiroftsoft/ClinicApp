using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Helper class for image operations
    /// کلاس کمکی برای عملیات تصویر
    /// </summary>
    public static class ImageHelper
    {
        #region Image Resizing

        /// <summary>
        /// Resizes an image to fit within maximum dimensions
        /// تغییر اندازه تصویر
        /// </summary>
        /// <param name="imageBytes">Image as byte array</param>
        /// <param name="maxWidth">Maximum width</param>
        /// <param name="maxHeight">Maximum height</param>
        /// <returns>Resized image as byte array</returns>
        public static byte[] ResizeImage(byte[] imageBytes, int maxWidth, int maxHeight)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return imageBytes;

            using (var ms = new MemoryStream(imageBytes))
            using (var image = Image.FromStream(ms))
            {
                var ratioX = (double)maxWidth / image.Width;
                var ratioY = (double)maxHeight / image.Height;
                var ratio = Math.Min(ratioX, ratioY);

                // If image is already smaller, return original
                if (ratio >= 1)
                    return imageBytes;

                var newWidth = (int)(image.Width * ratio);
                var newHeight = (int)(image.Height * ratio);

                using (var newImage = new Bitmap(newWidth, newHeight))
                using (var graphics = Graphics.FromImage(newImage))
                {
                    graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                    graphics.DrawImage(image, 0, 0, newWidth, newHeight);

                    using (var outputMs = new MemoryStream())
                    {
                        newImage.Save(outputMs, image.RawFormat);
                        return outputMs.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// Resizes image from file path
        /// تغییر اندازه تصویر از مسیر فایل
        /// </summary>
        public static byte[] ResizeImage(string imagePath, int maxWidth, int maxHeight)
        {
            if (!File.Exists(imagePath))
                return null;

            var imageBytes = File.ReadAllBytes(imagePath);
            return ResizeImage(imageBytes, maxWidth, maxHeight);
        }

        #endregion

        #region Base64 Conversion

        /// <summary>
        /// Converts image bytes to Base64 string
        /// تبدیل تصویر به Base64
        /// </summary>
        public static string ToBase64(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return string.Empty;

            return Convert.ToBase64String(imageBytes);
        }

        /// <summary>
        /// Converts Base64 string to image bytes
        /// تبدیل Base64 به تصویر
        /// </summary>
        public static byte[] FromBase64(string base64)
        {
            if (string.IsNullOrWhiteSpace(base64))
                return null;

            try
            {
                return Convert.FromBase64String(base64);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets Base64 data URL for image
        /// دریافت Data URL برای تصویر
        /// </summary>
        public static string ToDataUrl(byte[] imageBytes, string mimeType = "image/jpeg")
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return string.Empty;

            var base64 = ToBase64(imageBytes);
            return $"data:{mimeType};base64,{base64}";
        }

        #endregion

        #region Image Validation

        /// <summary>
        /// Validates if bytes represent a valid image
        /// اعتبارسنجی تصویر
        /// </summary>
        public static bool IsValidImage(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return false;

            try
            {
                using (var ms = new MemoryStream(imageBytes))
                using (var image = Image.FromStream(ms))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates image file extension
        /// بررسی پسوند فایل تصویر
        /// </summary>
        public static bool IsImageFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            var ext = Path.GetExtension(fileName)?.ToLower();
            return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" || ext == ".bmp";
        }

        #endregion

        #region Image Information

        /// <summary>
        /// Gets image dimensions
        /// دریافت ابعاد تصویر
        /// </summary>
        public static (int width, int height) GetImageDimensions(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return (0, 0);

            try
            {
                using (var ms = new MemoryStream(imageBytes))
                using (var image = Image.FromStream(ms))
                {
                    return (image.Width, image.Height);
                }
            }
            catch
            {
                return (0, 0);
            }
        }

        /// <summary>
        /// Gets image format
        /// دریافت فرمت تصویر
        /// </summary>
        public static string GetImageFormat(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return null;

            try
            {
                using (var ms = new MemoryStream(imageBytes))
                using (var image = Image.FromStream(ms))
                {
                    return image.RawFormat.ToString();
                }
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Thumbnail Creation

        /// <summary>
        /// Creates a thumbnail image
        /// ایجاد تصویر بندانگشتی
        /// </summary>
        public static byte[] CreateThumbnail(byte[] imageBytes, int thumbnailSize = 150)
        {
            return ResizeImage(imageBytes, thumbnailSize, thumbnailSize);
        }

        #endregion

        #region Image Cropping

        /// <summary>
        /// Crops an image to specified dimensions
        /// برش تصویر
        /// </summary>
        public static byte[] CropImage(byte[] imageBytes, int x, int y, int width, int height)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return imageBytes;

            using (var ms = new MemoryStream(imageBytes))
            using (var original = Image.FromStream(ms))
            {
                var cropRect = new Rectangle(x, y, width, height);
                
                using (var croppedImage = new Bitmap(width, height))
                using (var graphics = Graphics.FromImage(croppedImage))
                {
                    graphics.DrawImage(original, new Rectangle(0, 0, width, height), cropRect, GraphicsUnit.Pixel);

                    using (var outputMs = new MemoryStream())
                    {
                        croppedImage.Save(outputMs, original.RawFormat);
                        return outputMs.ToArray();
                    }
                }
            }
        }

        #endregion

        #region Format Conversion

        /// <summary>
        /// Converts image to JPEG format
        /// تبدیل به فرمت JPEG
        /// </summary>
        public static byte[] ConvertToJpeg(byte[] imageBytes, long quality = 90L)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return imageBytes;

            using (var ms = new MemoryStream(imageBytes))
            using (var image = Image.FromStream(ms))
            {
                var encoder = GetEncoder(ImageFormat.Jpeg);
                var encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

                using (var outputMs = new MemoryStream())
                {
                    image.Save(outputMs, encoder, encoderParameters);
                    return outputMs.ToArray();
                }
            }
        }

        /// <summary>
        /// Converts image to PNG format
        /// تبدیل به فرمت PNG
        /// </summary>
        public static byte[] ConvertToPng(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return imageBytes;

            using (var ms = new MemoryStream(imageBytes))
            using (var image = Image.FromStream(ms))
            using (var outputMs = new MemoryStream())
            {
                image.Save(outputMs, ImageFormat.Png);
                return outputMs.ToArray();
            }
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();
            foreach (var codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                    return codec;
            }
            return null;
        }

        #endregion
    }
}
