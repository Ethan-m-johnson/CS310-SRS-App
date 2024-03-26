using CS310_SRS_App.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CS310_SRS_App.Controllers
{
    public class HealthCareProfessionalsController : Controller
{
    private readonly CS310SRSDatabaseContext _context;

    public HealthCareProfessionalsController(CS310SRSDatabaseContext context)
    {
        _context = context;
    }
        [HttpGet]
        public IActionResult HealthCareProviders()
        {
            return View("HealthCareProviders");
        }



       
       
        [HttpGet]
        public IActionResult RemoveDoctor()
        {
            return View("RemoveDoctor");
        }


        /*
        [HttpGet]
        public async Task<IActionResult> SearchStaff(string term)
        {
            Console.WriteLine("Search Term: " + term); // Log the received term

            // Step 1: Find matching users based on the search term
            var matchingUsers = await _context.Users
                .Where(u => u.FirstName.Contains(term) || u.LastName.Contains(term))
                .Select(u => new { u.UserId, FullName = u.FirstName + " " + u.LastName })
                .ToListAsync();

            // Step 2: Filter these users to keep only those who are also patients
            var doctorStaffIds = await _context.staff
                .Select(p => p.StaffId)
                .Distinct()
                .ToListAsync();

            var doctorNames = matchingUsers
                .Where(u => doctorStaffIds.Contains(u.UserId)) // Keep only users who are patients
                .Select(u => u.FullName)
                .Distinct()
                .ToList();

            Console.WriteLine("-------------------------");
            Console.WriteLine(doctorNames.Count);
            Console.WriteLine("-------------------------");

            return Json(doctorNames);
        }*/

        [HttpGet]
        public async Task<IActionResult> SearchPatients(string term)
        {
            Console.WriteLine("Search Term: " + term); // Log the received term

            // Step 1: Find matching users based on the search term
            var matchingUsers = await _context.Users
                .Where(u => u.FirstName.Contains(term) || u.LastName.Contains(term))
                .Select(u => new { u.UserId, FullName = u.FirstName + " " + u.LastName })
                .ToListAsync();

            // Step 2: Filter these users to keep only those who are also patients
            var patientUserIds = await _context.staff
                .Select(p => p.UserId)
                .Distinct()
                .ToListAsync();

            var patientNames = matchingUsers
                .Where(u => patientUserIds.Contains(u.UserId)) // Keep only users who are patients
                .Select(u => u.FullName)
                .Distinct()
                .ToList();

            Console.WriteLine("-------------------------");
            Console.WriteLine(patientNames.Count);
            Console.WriteLine("-------------------------");

            return Json(patientNames);
        }
    }
}