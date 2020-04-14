using System;
using System.Web.Mvc;
using TransactionalMail.Services;

namespace TransactionalMail.Controllers
{
    public class OurTestController : Controller
    {
        private readonly ITransactionalEmailService _transactionalEmailService;

        public OurTestController(ITransactionalEmailService transactionalEmailService)
        {
            _transactionalEmailService = transactionalEmailService;
        }

        public ActionResult Index()
        {
            //send a transactional email
            var emailResult = _transactionalEmailService.SendPasswordResetEmail("test@test.com", "https://www.test.com/reset", DateTime.Now.AddDays(1));
            
            return View();
        }
    }
}