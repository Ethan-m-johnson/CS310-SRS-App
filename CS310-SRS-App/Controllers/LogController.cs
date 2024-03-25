using CS310_SRS_App.Model;
using Microsoft.AspNetCore.Mvc;

namespace CS310_SRS_App.Controllers
{
    public class LogController : Controller
    {

        private readonly CS310SRSDatabaseContext _context;

        public LogController(CS310SRSDatabaseContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            return View();
        }
    }
}
