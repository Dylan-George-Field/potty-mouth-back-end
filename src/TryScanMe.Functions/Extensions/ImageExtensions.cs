using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace TryScanMe.Functions.Extensions
{
    public static class ImageExtensions
    {
        public static Image AppendImage(this Image image, Image imageToAppend, int paddingwidth, int paddingheight)
        {
            double xRatio = (double)image.Width / (double)imageToAppend.Width; //scale based on width

            double paddingWidth = paddingwidth;
            double paddingHeight = paddingheight;

            //scale image2 to be same width as image 1
            var resizedImage = ResizeImage(image, imageToAppend.Width , image.Height / xRatio, image.Width / xRatio, image.Height / xRatio, paddingHeight, paddingWidth); //half the original image (2000px / 400px)

            //calculate image1.height + image2.height
            var height = resizedImage.Height + imageToAppend.Height;
            var width = imageToAppend.Width;

            var marginTop = 0; 

            Image combinedImage = new Bitmap(width, (int) (height + marginTop));
            Graphics combinedGraphic = Graphics.FromImage(combinedImage);
            combinedGraphic.Clear(Color.White);
            combinedGraphic.DrawImage(resizedImage, new Point(0, (int) marginTop));
            combinedGraphic.DrawImage(imageToAppend, new Point(0, (int) (resizedImage.Height + marginTop)));

            return combinedImage;
        }

        public static Image ResizeImage(this Image image, double width, double height, double originalWidth, double originalHeight)
        {
            return ResizeImage(image, width, height, originalWidth, originalHeight, 0, 0);
        }

        private static Image ResizeImage(
            Image image,
            double canvasWidth, double canvasHeight,
            double originalWidth, double originalHeight,
            double paddingTop, double paddingSides
        )
        {
            Image thumbnail = new Bitmap(Convert.ToInt32(canvasWidth), Convert.ToInt32(canvasHeight)); // changed parm names
            Graphics graphic = Graphics.FromImage(thumbnail);

            graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphic.SmoothingMode = SmoothingMode.HighQuality;
            graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphic.CompositingQuality = CompositingQuality.HighQuality;

            /* ------------------ new code --------------- */

            // Figure out the ratio
            double ratioX = canvasWidth / originalWidth;
            double ratioY = canvasHeight / originalHeight;
            // use whichever multiplier is smaller
            double ratio = ratioX < ratioY ? ratioX : ratioY;

            // now we can get the new height and width
            int newHeight = Convert.ToInt32(originalHeight * ratio);
            int newWidth = Convert.ToInt32(originalWidth * ratio);

            // Now calculate the X,Y position of the upper-left corner 
            // (one of these will always be zero)
            int posX = Convert.ToInt32((canvasWidth - (originalWidth * ratio)) / 2);
            int posY = Convert.ToInt32((canvasHeight - (originalHeight * ratio)) / 2);

            graphic.Clear(Color.White); // white padding
            graphic.DrawImage(image, posX + Convert.ToInt32(paddingSides / 2), posY + Convert.ToInt32(paddingTop / 2), newWidth - Convert.ToInt32(paddingSides), newHeight - Convert.ToInt32(paddingTop));

            return thumbnail;
        }

        public static Image WriteText(this Image image, string text, PointF point)
        {
            using(Graphics graphics = Graphics.FromImage(image))
{
                using (Font arialFont = new Font("Arial", 25))
                {
                    graphics.DrawString(text, arialFont, Brushes.Black, point);
                }
            }

            return image;
        }

    }
}
