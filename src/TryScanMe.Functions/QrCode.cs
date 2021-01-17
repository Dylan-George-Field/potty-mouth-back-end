using System;
using System.Drawing;
using QRCoder;

namespace TryScanMe.Functions
{
    public class QrCode
    {
        private readonly QRCodeGenerator Generator = new QRCodeGenerator();

        private const int DefaultPixelsPerModule = 20;

        public Bitmap GenerateImage(Uri uri, int pixelsPerModule = DefaultPixelsPerModule)
        {
            var qrCodeData = Generator.CreateQrCode(uri.ToString(), QRCodeGenerator.ECCLevel.Q);

            var qrCode = new QRCode(qrCodeData);

            return qrCode.GetGraphic(pixelsPerModule);
        }

        public Bitmap GenerateImage(Uri uri, Bitmap icon, int pixelsPerModule = DefaultPixelsPerModule)
        {
            var qrCodeData = Generator.CreateQrCode(uri.ToString(), QRCodeGenerator.ECCLevel.Q);

            var qrCode = new QRCode(qrCodeData);

            return qrCode.GetGraphic(pixelsPerModule, Color.Black, Color.White, icon);
        }

    }
}

