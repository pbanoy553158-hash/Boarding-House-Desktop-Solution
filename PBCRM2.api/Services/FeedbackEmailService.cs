using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using PBCRM2.API.Settings;
using PBCRM2.Domain.Entities;
using System.Drawing;
using System.Runtime.Intrinsics.Arm;

namespace PBCRM2.API.Services;

public class FeedbackEmailService
{
    private readonly FeedbackEmailSettings _settings;

    public FeedbackEmailService(
        IOptions<FeedbackEmailSettings> options)
    {
        _settings = options.Value;
    }

    // =========================================================
    // SEND FEEDBACK REQUEST EMAIL
    // =========================================================

    public async Task SendFeedbackRequestAsync(
        string tenantName,
        string tenantEmail,
        string feedbackUrl,
        byte[] qrCode)
    {
        // =====================================================
        // VALIDATION
        // =====================================================

        if (string.IsNullOrWhiteSpace(tenantEmail))
        {
            throw new ArgumentException(
                "Tenant email address is required.");
        }

        if (string.IsNullOrWhiteSpace(tenantName))
        {
            throw new ArgumentException(
                "Tenant name is required.");
        }

        if (string.IsNullOrWhiteSpace(feedbackUrl))
        {
            throw new ArgumentException(
                "Feedback URL is required.",
                nameof(feedbackUrl));
        }

        if (qrCode == null || qrCode.Length == 0)
        {
            throw new ArgumentException(
                "QR code image is required.",
                nameof(qrCode));
        }

        if (string.IsNullOrWhiteSpace(_settings.Host))
        {
            throw new InvalidOperationException(
                "Feedback email SMTP host is not configured.");
        }

        if (_settings.Port <= 0)
        {
            throw new InvalidOperationException(
                "Feedback email SMTP port is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_settings.Username))
        {
            throw new InvalidOperationException(
                "Feedback email username is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_settings.Password))
        {
            throw new InvalidOperationException(
                "Feedback email password is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            throw new InvalidOperationException(
                "Feedback email sender address is not configured.");
        }

        // =====================================================
        // CREATE MIME MESSAGE
        // =====================================================

        var message = new MimeMessage();

        message.From.Add(
            new MailboxAddress(
                _settings.FromName,
                _settings.FromEmail));

        message.To.Add(
            new MailboxAddress(
                tenantName,
                tenantEmail));

        message.Subject =
            "Percy's Boarding House - Feedback Request";

        // =====================================================
        // ESCAPE VALUES FOR HTML
        // =====================================================

        var safeTenantName =
            System.Net.WebUtility.HtmlEncode(
                tenantName);

        var safeFeedbackUrl =
            System.Net.WebUtility.HtmlEncode(
                feedbackUrl);

        // =====================================================
        // CREATE BODY
        // =====================================================

        var builder = new BodyBuilder();

        // =====================================================
        // PLAIN TEXT VERSION
        // =====================================================

        builder.TextBody =
            $"""
            Hello {tenantName},

            We would appreciate your feedback about your experience at Percy's Boarding House.

            Please open the link below to submit your feedback:

            {feedbackUrl}

            You can also scan the QR code included in this email.

            Thank you for taking the time to share your experience with us.

            Percy's Boarding House
            """;

        // =====================================================
        // HTML VERSION
        // =====================================================

        builder.HtmlBody =
            $"""
            <!DOCTYPE html>

            <html>

            <head>

                <meta charset="UTF-8">

                <meta
                    name="viewport"
                    content="width=device-width, initial-scale=1.0">

                <title>
                    Percy's Boarding House
                </title>

            </head>

            <body style="
                margin:0;
                padding:0;
                background-color:#f8f5ef;
                font-family:Arial,Helvetica,sans-serif;
                color:#37302a;
            ">

                <table
                    width="100%"
                    cellpadding="0"
                    cellspacing="0"
                    border="0"
                    style="
                        background-color:#f8f5ef;
                        padding:30px 15px;
                    "
                >

                    <tr>

                        <td align="center">

                            <table
                                width="600"
                                cellpadding="0"
                                cellspacing="0"
                                border="0"
                                style="
                                    max-width:600px;
                                    width:100%;
                                    background-color:#ffffff;
                                    border:1px solid #e1d7c8;
                                    border-radius:12px;
                                "
                            >

                                <!-- ================================= -->
                                <!-- HEADER -->
                                <!-- ================================= -->

                                <tr>

                                    <td
                                        style="
                                            background-color:#483320;
                                            padding:25px 30px;
                                            border-radius:12px 12px 0 0;
                                        "
                                    >

                                        <div style="
                                            color:#ffffff;
                                            font-size:22px;
                                            font-weight:bold;
                                        ">
                                            Percy's Boarding House
                                        </div>

                                        <div style="
                                            color:#e0c28c;
                                            font-size:13px;
                                            margin-top:6px;
                                        ">
                                            Tenant Feedback
                                        </div>

                                    </td>

                                </tr>

                                <!-- ================================= -->
                                <!-- MAIN CONTENT -->
                                <!-- ================================= -->

                                <tr>

                                    <td
                                        style="
                                            padding:30px;
                                        "
                                    >

                                        <p style="
                                            margin:0 0 18px 0;
                                            font-size:16px;
                                            color:#37302a;
                                        ">
                                            Hello
                                            <strong>
                                                {safeTenantName}
                                            </strong>,
                                        </p>

                                        <p style="
                                            margin:0 0 14px 0;
                                            font-size:14px;
                                            line-height:1.7;
                                            color:#554d46;
                                        ">
                                            We would appreciate your
                                            feedback about your
                                            experience at Percy's
                                            Boarding House.
                                        </p>

                                        <p style="
                                            margin:0;
                                            font-size:14px;
                                            line-height:1.7;
                                            color:#554d46;
                                        ">
                                            Please click the button
                                            below to open your
                                            feedback form.
                                        </p>

                                        <!-- ================================= -->
                                        <!-- BUTTON -->
                                        <!-- ================================= -->

                                        <table
                                            width="100%"
                                            cellpadding="0"
                                            cellspacing="0"
                                            border="0"
                                            style="
                                                margin-top:25px;
                                                margin-bottom:25px;
                                            "
                                        >

                                            <tr>

                                                <td align="center">

                                                    <a
                                                        href="{safeFeedbackUrl}"
                                                        target="_blank"
                                                        style="
                                                            display:inline-block;
                                                            background-color:#aa8250;
                                                            color:#ffffff;
                                                            text-decoration:none;
                                                            padding:14px 28px;
                                                            border-radius:8px;
                                                            font-size:14px;
                                                            font-weight:bold;
                                                        "
                                                    >
                                                        Give Your Feedback
                                                    </a>

                                                </td>

                                            </tr>

                                        </table>

                                        <!-- ================================= -->
                                        <!-- QR CODE -->
                                        <!-- ================================= -->

                                        <p style="
                                            margin:0 0 15px 0;
                                            text-align:center;
                                            font-size:14px;
                                            color:#554d46;
                                        ">
                                            Or scan this QR code
                                            using your phone:
                                        </p>

                                        <table
                                            width="100%"
                                            cellpadding="0"
                                            cellspacing="0"
                                            border="0"
                                        >

                                            <tr>

                                                <td align="center">

                                                    <img
                                                        src="cid:feedbackqr"
                                                        alt="Feedback QR Code"
                                                        width="220"
                                                        height="220"
                                                        style="
                                                            display:block;
                                                            width:220px;
                                                            height:220px;
                                                            margin:auto;
                                                            border:0;
                                                        "
                                                    >

                                                </td>

                                            </tr>

                                        </table>

                                        <!-- ================================= -->
                                        <!-- DIRECT LINK -->
                                        <!-- ================================= -->

                                        <p style="
                                            margin:25px 0 8px 0;
                                            font-size:12px;
                                            color:#777777;
                                        ">
                                            If the button does not work,
                                            copy and open this link in
                                            your browser:
                                        </p>

                                        <p style="
                                            margin:0;
                                            font-size:12px;
                                            line-height:1.5;
                                            word-break:break-all;
                                            color:#483320;
                                        ">
                                            {safeFeedbackUrl}
                                        </p>

                                        <!-- ================================= -->
                                        <!-- THANK YOU -->
                                        <!-- ================================= -->

                                        <p style="
                                            margin:25px 0 0 0;
                                            font-size:14px;
                                            line-height:1.7;
                                            color:#554d46;
                                        ">
                                            Thank you for taking the
                                            time to share your experience
                                            with us.
                                        </p>

                                        <p style="
                                            margin:18px 0 0 0;
                                            font-size:14px;
                                            font-weight:bold;
                                            color:#483320;
                                        ">
                                            Percy's Boarding House
                                        </p>

                                    </td>

                                </tr>

                                <!-- ================================= -->
                                <!-- FOOTER -->
                                <!-- ================================= -->

                                <tr>

                                    <td
                                        style="
                                            background-color:#faf7f2;
                                            padding:18px 30px;
                                            border-top:1px solid #e1d7c8;
                                            border-radius:0 0 12px 12px;
                                            text-align:center;
                                            font-size:11px;
                                            color:#887e73;
                                        "
                                    >

                                        This is an automated feedback
                                        request from Percy's Boarding House.

                                    </td>

                                </tr>

                            </table>

                        </td>

                    </tr>

                </table>

            </body>

            </html>
            """;

        // =====================================================
        // ADD QR CODE AS INLINE MIME RESOURCE
        // =====================================================

        var qrImage =
            builder.LinkedResources.Add(
                "feedback-qr.png",
                qrCode,
                new ContentType(
                    "image",
                    "png"));

        qrImage.ContentId =
            "feedbackqr";

        qrImage.ContentDisposition =
            new ContentDisposition(
                ContentDisposition.Inline);

        // =====================================================
        // BUILD FINAL MIME BODY
        // =====================================================

        message.Body =
            builder.ToMessageBody();

        // =====================================================
        // CONNECT TO SMTP
        // =====================================================

        using var smtp =
            new SmtpClient();

        await smtp.ConnectAsync(
            _settings.Host,
            _settings.Port,
            SecureSocketOptions.StartTls);

        // =====================================================
        // AUTHENTICATE
        // =====================================================

        await smtp.AuthenticateAsync(
            _settings.Username,
            _settings.Password);

        // =====================================================
        // SEND
        // =====================================================

        await smtp.SendAsync(
            message);

        // =====================================================
        // DISCONNECT
        // =====================================================

        await smtp.DisconnectAsync(
            true);
    }
}