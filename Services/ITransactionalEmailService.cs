using System;

namespace TransactionalMail.Services
{
    public interface ITransactionalEmailService
    {
        bool SendPasswordResetEmail(string recipientEmail, string passwordResetUrl, DateTime expiration);

        bool SendPDF(string recipientEmail, byte[] pdf, string fileName, string message);
    }
}
