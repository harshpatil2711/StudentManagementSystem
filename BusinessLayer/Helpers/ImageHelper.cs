using Serilog;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace BusinessLayer1.Helpers
{
    public static class ImageHelper
    {
        public const int MaxFileSizeBytes = 2 * 1024 * 1024;
        public const int TargetSize = 300;
        public const int JpegQuality = 80;

        private static readonly HashSet<string> AllowedExtensions =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

        public static bool IsValidExtension(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;
            string ext = Path.GetExtension(fileName);
            return AllowedExtensions.Contains(ext);
        }

        public static bool IsValidExtensionOrThrow(string fileName)
        {
            if (!IsValidExtension(fileName))
                throw new InvalidOperationException("Invalid file type. Allowed: .jpg, .jpeg, .png, .webp");
            return true;
        }

        public static string SaveUploadedPhoto(Stream inputStream, string originalFileName, int studentId, string uploadDir, string uploadUrlPrefix)
        {
            if (inputStream == null)
                return null;

            IsValidExtensionOrThrow(originalFileName);

            Directory.CreateDirectory(uploadDir);

            string destFileName = studentId + ".jpg";
            string destPath = Path.Combine(uploadDir, destFileName);

            if (inputStream.CanSeek)
                inputStream.Position = 0;

            using (var original = SKBitmap.Decode(inputStream))
            {
                if (original == null)
                    throw new InvalidOperationException("Unable to decode the uploaded image.");

                using (var resized = ResizeKeepAspect(original, TargetSize))
                using (var image = SKImage.FromBitmap(resized))
                using (var data = image.Encode(SKEncodedImageFormat.Jpeg, JpegQuality))
                using (var stream = File.Create(destPath))
                {
                    data.SaveTo(stream);
                }
            }

            string urlPath = uploadUrlPrefix.TrimEnd('/') + "/" + destFileName;
            return urlPath;
        }

        public static string ReplaceUploadedPhoto(Stream inputStream, string originalFileName, int studentId, string oldPhotoPath, string uploadDir, string uploadUrlPrefix, string serverMapRoot)
        {
            DeleteStudentPhoto(oldPhotoPath, serverMapRoot);
            return SaveUploadedPhoto(inputStream, originalFileName, studentId, uploadDir, uploadUrlPrefix);
        }

        public static void DeleteStudentPhoto(string relativePath, string serverMapRoot)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return;

            string safeRelative = relativePath.Replace('\\', '/').TrimStart('/');
            if (safeRelative.Contains(".."))
                return;

            string fullPath = Path.Combine(serverMapRoot, safeRelative.Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to delete student photo {Path}", relativePath);
                }
            }
        }

        private static SKBitmap ResizeKeepAspect(SKBitmap original, int maxSize)
        {
            int srcW = original.Width;
            int srcH = original.Height;

            if (srcW <= maxSize && srcH <= maxSize)
                return original.Copy();

            float ratio = Math.Min((float)maxSize / srcW, (float)maxSize / srcH);
            int newW = (int)(srcW * ratio);
            int newH = (int)(srcH * ratio);

            var resized = original.Resize(new SKImageInfo(newW, newH), SKFilterQuality.Medium);
            return resized;
        }
    }
}
