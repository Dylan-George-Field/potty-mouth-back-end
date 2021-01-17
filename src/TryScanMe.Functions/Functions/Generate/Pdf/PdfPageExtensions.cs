using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace TryScanMe.Functions.Functions.Generate.pdf
{
    public static class PdfPageExtensions
    {
        private static XGraphics gfx;

        private const string iconPath = "\\Assets\\tears-of-joy.png";
        private const string CameraImagePath = "\\Assets\\camera.png";
        private const string LogoImagePath = "\\Assets\\potty-mouth.png";
        private const string DigitalGraffitiWallPath = "\\Assets\\leave-a-secret-message.png";

        static PdfPageExtensions()
        {
            MyFontResolver.Apply();
        }

        public static PdfPage ConvertToDL8(this PdfPage page, UrlGenerator generator, int offset, string functionDirectory)
        {
            var type = LabelTypes.DL8;
            var width = 270;
            var height= 182;
            var logoWidth = 140;
            var logoOffsetLeft = 35;
            var logoOffsetTop = 160;
            var marginTop = 41;
            var marginLeft = 18;
            var marginRight = 18;
            var marginBottom = 10;
            var qrCodeSize = 178;
            var qrCodeImageSize = 500;
            var qrCodeOffsetTop = 190;
            var qrCodeOffsetLeft = 80;

            gfx = XGraphics.FromPdfPage(page);

            var pen = new XPen(XColors.Black);

            var rectangles = GetRectangles(width, height, (int)type, marginTop, marginBottom, marginLeft, marginRight);

            gfx.DrawRectangles(pen, rectangles.ToArray());

            var totalSquaresAvailable = rectangles.Count;
            var numberOfSquresToPrint = generator.ToUri().Count;

            for (var i = 0; i < numberOfSquresToPrint; i++)
            {
                var squareToPrint = (offset + i) % totalSquaresAvailable;

                DrawQrCode(qrCodeImageSize, qrCodeSize, qrCodeOffsetLeft, qrCodeOffsetTop, rectangles[squareToPrint], generator.ToUri()[i], functionDirectory + iconPath);
                DrawImage(rectangles[squareToPrint], logoWidth, logoOffsetLeft, logoOffsetTop, functionDirectory + LogoImagePath);
            }

            return page;
        }

        public static PdfPage ConvertToDL16(this PdfPage page, UrlGenerator generator, int offset, string functionDirectory)
        {
            var type = LabelTypes.DL16;
            var width = 282;
            var height = 95;

            var marginTop = 36;
            var marginLeft = 13;
            var marginRight = 7;
            var marginBottom = 1;

            var logoWidth = 92;
            var logoOffsetLeft = 3;
            var logoOffsetTop = 94;

            var qrCodeSize = 100;
            var qrCodeImageSize = 500;
            var qrCodeOffsetLeft = 63;
            var qrCodeOffsetTop = 97;

            var subheadingWidth = 80;
            var subheadingOffsetLeft = 159;
            var subheadingOffsetTop = 88;

            var cameraIconSize = 15;
            var cameraIconLeft = 248;
            var cameraIconTop = 26;

            var idOffsetLeft = 250;
            var idOffsetTop = 85;

            var footer3Text = "Do not remove. Reduce graffiti.\nPrint your own at pottymouth.io";
            var footer3Left = 265;
            var footer3Top = 85;

            gfx = XGraphics.FromPdfPage(page);

            var rectangles = GetRectangles(width, height, (int)type, marginTop, marginBottom, marginLeft, marginRight);

            // Use this for debugging margins
            // gfx.DrawRectangles(pen, rectangles.ToArray());

            var totalSquaresAvailable = rectangles.Count;
            var numberOfSquresToPrint = generator.ToUri().Count;

            for (var i = 0; i < numberOfSquresToPrint; i++)
            {
                var squareToPrint = (offset + i) % totalSquaresAvailable;

                DrawQrCode(qrCodeImageSize, qrCodeSize, qrCodeOffsetLeft, qrCodeOffsetTop, rectangles[squareToPrint], generator.ToUri()[i], functionDirectory + iconPath);
                DrawImage(rectangles[squareToPrint], logoWidth, logoOffsetLeft, logoOffsetTop, functionDirectory + LogoImagePath);
                DrawImage(rectangles[squareToPrint], subheadingWidth, subheadingOffsetLeft, subheadingOffsetTop, functionDirectory + DigitalGraffitiWallPath);
                DrawImage(rectangles[squareToPrint], cameraIconSize, cameraIconLeft, cameraIconTop, functionDirectory + CameraImagePath);
                WriteCaption(generator.Guids[i].ToString().Substring(0, 6), idOffsetLeft, idOffsetTop, rectangles[squareToPrint], XBrushes.Black);
                WriteCaption(footer3Text, footer3Left, footer3Top, rectangles[squareToPrint], XBrushes.Gray, 5);
            }

            return page;
        }

        private static void DrawQrCode(int imageSize, int qrCodeSize, int offsetLeft, int offsetTop, XRect rectangle, Uri uri, string iconPath)
        {
            var icon = new Bitmap(Image.FromFile(iconPath));
            var bitmap = new QrCode().GenerateImage(uri, icon);
            var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Bmp);

            var image = XImage.FromStream(ms);

            var state = gfx.Save();
            gfx.RotateAtTransform(-90, new XPoint(rectangle.Left + offsetLeft, rectangle.Top + offsetTop));
            gfx.DrawImage(image, rectangle.Left + offsetLeft, rectangle.Top + offsetTop, qrCodeSize, qrCodeSize);
            gfx.Restore(state);
        }

        private static void DrawImage(XRect rectangle, int width, int offsetLeft, int offsetTop, string imagePath)
        {
            using (XImage image = XImage.FromFile(imagePath))
            {
                double xRatio = image.PixelWidth / width;

                var state = gfx.Save();
                gfx.RotateAtTransform(-90, new XPoint(rectangle.Left + offsetLeft, rectangle.Top + offsetTop));
                gfx.DrawImage(image, rectangle.Left + offsetLeft, rectangle.Top + offsetTop, width, image.PixelHeight / xRatio);
                gfx.Restore(state);
            }
        }

        public static void WriteCaption(string text, int offsetLeft, int offsetTop, XRect rectangle, XBrush brush, double fontsize = 8.5)
        {
            var tf = new XTextFormatter(gfx);

            var font = new XFont("Open Sans", fontsize, XFontStyle.Bold);
            var textbox = new XRect(rectangle.Left + offsetLeft, rectangle.Top + offsetTop, 150, 400);

            var state = gfx.Save();
            gfx.RotateAtTransform(-90, new XPoint(rectangle.Left + offsetLeft, rectangle.Top + offsetTop));
            tf.DrawString(text, font, brush, textbox, XStringFormats.TopLeft);
            gfx.Restore(state);
        }

        private static List<XRect> GetRectangles(int width, int height, int numberToDraw, int marginTop, int marginBottom, int marginLeft, int marginRight)
        {
            // A4 portrait mode
            var size = new XSize(width, height);

            var rectangles = new List<XRect>();
            int i = 0;
            do
            {
                if (i == 0)
                {
                    rectangles.Add(new XRect(new XPoint(marginLeft, marginTop), size));
                }
                else if (i == 1)
                {
                    rectangles.Add(new XRect(new XPoint(marginLeft + width + marginRight, marginTop), size));
                }
                else if (i > 1)
                {
                    rectangles.Add(new XRect(new XPoint(marginLeft, marginTop + (height * (i - 1)) + (marginBottom * (i - 1))), size));
                    rectangles.Add(new XRect(new XPoint(marginLeft + width + marginRight, marginTop + (height * (i - 1)) + (marginBottom * (i - 1))), size));
                }
                i++;
            } while (rectangles.Count != numberToDraw);

            return rectangles;
        }
    }
}
