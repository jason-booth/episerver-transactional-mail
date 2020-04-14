using EPiServer;
using EPiServer.Core;
using EPiServer.Web;
using TransactionalMail.Models.Pages;

namespace TransactionalMail.Services
{
    public class SiteSettingsService : ISiteSettingsService
    {
        private readonly IContentLoader _contentLoader;
        private readonly ISiteDefinitionRepository _siteDefinitionRepository;

        public SiteSettingsService(IContentLoader contentLoader, ISiteDefinitionRepository siteDefinitionRepository)
        {
            _contentLoader = contentLoader;
            _siteDefinitionRepository = siteDefinitionRepository;
        }

        public SettingsPage GetSettingsOrDefault()
        {
            var startPagePageReference = ContentReference.StartPage;
            return GetSettingsOrDefault(startPagePageReference);
        }

        public SettingsPage GetSettingsOrDefault(PageReference startPageReference)
        {
            var startPage = ContentReference.IsNullOrEmpty(startPageReference)
                ? null
                : _contentLoader.Get<StartPage>(startPageReference);
            return startPage == null || ContentReference.IsNullOrEmpty(startPage.SettingsPageReference)
                ? new SettingsPage()
                : _contentLoader.Get<SettingsPage>(startPage.SettingsPageReference);
        }
    }
}