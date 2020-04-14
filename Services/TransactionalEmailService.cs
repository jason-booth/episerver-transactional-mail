using EPiServer.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace TransactionalMail.Services
{
    public class TransactionalEmailService : ITransactionalEmailService
    {
        private static readonly ILogger Log = LogManager.GetLogger(typeof(TransactionalEmailService));

        private readonly ISiteSettingsService _siteSettingsService;

        public TransactionalEmailService(ISiteSettingsService siteSettingsService)
        {
            _siteSettingsService = siteSettingsService;
        }

        /// <summary>
        /// Message Types corresponding to message id's in Marketing Automation. Expand the enum for additional messages
        /// </summary>
        private enum MessageType : long
        {
            PasswordReset = 250870904539,
            SendPDF = 255172776624
        };

        /// <summary>
        /// An example sending a password reset emails. Additional similar public methods will be created for more emails.
        /// </summary>
        public bool SendPasswordResetEmail(string recipientEmail, string passwordResetUrl, DateTime expiration)
        {
            var arguments = new Dictionary<MessageArgument, string>
            {
                { MessageArgument.PasswordResetUrl, passwordResetUrl },
                { MessageArgument.PasswordResetExpiration, $"{expiration.ToShortDateString()} {expiration.ToShortTimeString()}" }
            };

            return SendEmail(recipientEmail, MessageType.PasswordReset, arguments);
        }

        /// <summary>
        /// An example including an attachment
        /// </summary>
        public bool SendPDF(string recipientEmail, byte[] pdf, string fileName, string message)
        {
            string attachmentToken = null;
            if (pdf.Length > 0)
            {
                attachmentToken = Upload(pdf, fileName);
            }

            var arguments = new Dictionary<MessageArgument, string>
            {
                {MessageArgument.Message, message}
            };

            if (!string.IsNullOrEmpty(attachmentToken))
            {
                arguments.Add(MessageArgument.AttachmentToken, attachmentToken); //attach a token for a file upload if needed
            }

            return SendEmail(recipientEmail, MessageType.SendPDF, arguments);
        }

        private bool SendEmail(string recipientEmail, MessageType emailId, Dictionary<MessageArgument, string> emailArguments = null)
        {
            try
            {
                if (string.IsNullOrEmpty(recipientEmail))
                {
                    Log.Error($"Error sending email type {emailId}. Recipient email address is null or blank.");
                    return false;
                }

                var campaignApiUrl = _siteSettingsService.GetSettingsOrDefault().CampaignApiUrl; //this is the rest API for MarketingAutomation ()
                var campaignAuthCode = _siteSettingsService.GetSettingsOrDefault().CampaignTransactionalAuthorizationCode;

                if (string.IsNullOrEmpty(campaignApiUrl) || string.IsNullOrEmpty(campaignAuthCode))
                {
                    Log.Error($"Error sending email type {emailId}. Campaign settings are missing from site settings.");
                    return false;
                }

                var url = new StringBuilder();
                url.Append(campaignApiUrl.Trim()); //https://api.campaign.episerver.net/http/form
                url.Append($"/{campaignAuthCode.Trim()}"); //this is your secret key provide by EPiServer - guard it with your life
                url.Append("/sendtransactionmail");
                url.Append($"?bmRecipientId={HttpUtility.UrlEncode(recipientEmail)}");
                url.Append($"&bmMailingId={(long)emailId}");

                //add the arguments
                if (emailArguments != null)
                {
                    foreach (var argument in emailArguments)
                    {
                        var encodedVal = argument.Value == null
                            ? string.Empty
                            : HttpUtility.UrlEncode(argument.Value.Replace("'", "&apos;")); //Campaign does not like to receive apostrophe's in some fields

                        url.Append($"&{argument.Key.Value}={encodedVal}");
                    }
                }

                var request = (HttpWebRequest)WebRequest.Create(url.ToString());
                var response = (HttpWebResponse)request.GetResponse();
                var responseStream = response.GetResponseStream();

                string responseMessage;
                using (var reader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    responseMessage = reader.ReadToEnd();
                }

                if (responseMessage.ToLowerInvariant().StartsWith("enqueued"))
                {
                    return true;
                }

                Log.Error($"Error sending email type {emailId}. Message from Campaign: {responseMessage}. Http API Url: {url}");
                return false;
            }
            catch (Exception e)
            {
                Log.Error($"Error sending email type {emailId}. Exception: {e.Message}");
                return false;
            }
        }

        private string Upload(byte[] bytes, string fileName)
        {
            var campaignApiUrl = _siteSettingsService.GetSettingsOrDefault().CampaignApiUrl;
            var campaignAuthCode = _siteSettingsService.GetSettingsOrDefault().CampaignTransactionalAuthorizationCode;

            if (string.IsNullOrEmpty(campaignApiUrl) || string.IsNullOrEmpty(campaignAuthCode))
            {
                Log.Error($"Error uploading file to Campaign. Campaign settings are missing.");
                return null;
            }

            //using RestSharp for the assistance with the image post
            var restClient = new RestSharp.RestClient(campaignApiUrl.Trim('/'));
            var request = new RestSharp.RestRequest($"{campaignAuthCode.Trim()}/uploadpersonalizedattachments", RestSharp.Method.POST)
            {
                AlwaysMultipartFormData = true
            };

            request.AddHeader("Content-Type", "multipart/form-data");
            request.AddFile(MessageArgument.AttachmentFile.Value, bytes, fileName, System.Web.MimeMapping.GetMimeMapping(fileName));
            var restResponse = restClient.Execute(request);

            if (!restResponse.Content.ToLower().StartsWith("ok"))
            {
                Log.Error($"Error uploading attachment to Campaign. Campaign message: {restResponse.Content}. Filename: {fileName}");
                return string.Empty;
            }

            return restResponse.Content.Replace("ok:", "").Trim(); //return the attachment token value wich will be sent with a message as an additional argument
        }

        /// <summary>
        /// Class used within TransactionalEmailService to foster *less* magic string usage with email variables
        /// NOTE: casing of the values matters.
        /// </summary>
        private class MessageArgument
        {
            private MessageArgument(string value) { Value = value; }

            public string Value { get; }

            public static MessageArgument PasswordResetUrl => new MessageArgument("PasswordResetUrl");
            public static MessageArgument PasswordResetExpiration => new MessageArgument("PasswordResetExpiration");
            public static MessageArgument AttachmentToken => new MessageArgument("bmPersonalizedAttachmentsToken");
            public static MessageArgument AttachmentFile => new MessageArgument("bmFile");
            public static MessageArgument Message => new MessageArgument("Message");
        }
    }
}