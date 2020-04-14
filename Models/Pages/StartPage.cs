using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace TransactionalMail.Models.Pages
{
    [ContentType(DisplayName = "Start Page", GUID = "3A348608-6209-4449-9568-5520FA9DE517", Description = "Home Page")]
    public class StartPage : PageData
    {
        [Display(
            Name = "Settings Page",
            Description = "Sitewide Settings Page",
            GroupName = SystemTabNames.Settings)]
        [Required]
        [AllowedTypes(typeof(SettingsPage))]
        public virtual PageReference SettingsPageReference { get; set; }
    }
}