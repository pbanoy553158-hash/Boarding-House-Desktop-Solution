using QRCoder;

namespace PBCRM2.API.Services;

public class QrCodeService
{
    // =========================================================
    // GENERATE QR CODE
    // =========================================================

    public byte[] GenerateQrCode(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException(
                "QR code text cannot be empty.",
                nameof(text));
        }

        using var qrGenerator = new QRCodeGenerator();

        using var qrCodeData =
            qrGenerator.CreateQrCode(
                text,
                QRCodeGenerator.ECCLevel.Q);

        var pngQrCode =
            new PngByteQRCode(qrCodeData);

        return pngQrCode.GetGraphic(20);
    }
}