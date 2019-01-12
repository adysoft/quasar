namespace Crawler
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;

    public static class ImageProcessing
    {
        public static void MakeThumbnail(string sourceImagePath, string thumbnailPath)
        {
            Debug.Assert(File.Exists(sourceImagePath));
            if (File.Exists(thumbnailPath))
            {
                File.Delete(thumbnailPath);
            }

            var image = new Bitmap(sourceImagePath);
            float desiredHeight = 225f;
            float ratio = desiredHeight / image.Height;
            var thumbnail = ResizeImage(image, Convert.ToInt32(image.Width * ratio), (int)desiredHeight);
            thumbnail.Save(thumbnailPath);

        }

        private static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}
