using CS310_SRS_App.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CS310_SRS_App.Controllers
{
    public class CalendarController : Controller
    {
        private readonly CS310SRSDatabaseContext _context;
        public CalendarController(CS310SRSDatabaseContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var _events = _context.CalenderBlocks.ToList();
            return View(_events);
        }
    }
}
