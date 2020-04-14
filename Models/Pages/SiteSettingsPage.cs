using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace TransactionalMail.Models.Pages
{
    [ContentType(DisplayName = "Settings Page", GUID = "F89D7DB7-7292-4576-AC65-727D4278EE07", Description = "Sitewide Settings Page")]
    [AvailableContentTypes(Availability.None)]
    public class SettingsPage : PageData
    {
        [Display(
            Name = "Campaign API Url",
            Description = "The base Campaign API Url",
            Order = 10
        )]
        public virtual string CampaignApiUrl { get; set; }

        [Display(
            Name = "Campaign Email List Authorization Code for Transactional",
            Description = "The authorization code for the transactional email list - used to send transactional emails",
            Order = 20
        )]
        public virtual string CampaignTransactionalAuthorizationCode { get; set; }
    }
}
