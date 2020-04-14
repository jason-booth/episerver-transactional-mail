using EPiServer.Core;
using TransactionalMail.Models.Pages;

namespace TransactionalMail.Services
{
    public interface ISiteSettingsService
    {
        SettingsPage GetSettingsOrDefault();

        SettingsPage GetSettingsOrDefault(PageReference startPageReference);
    }
}
