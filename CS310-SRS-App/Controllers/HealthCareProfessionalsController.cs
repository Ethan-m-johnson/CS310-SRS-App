using CS310_SRS_App.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.CodeAnalysis;
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
        public async Task<IActionResult> ListSpecialties(string term ="")
        {
            var specialties =await _context.staff.Where(s => s.Specialty.Contains(term))
                    // Filter specialties based on the term
                    .Select(s => s.Specialty).Distinct().OrderBy(s => s).ToListAsync();
            return Json(specialties);
        }

        [HttpGet]
        public async Task<IActionResult> GetStaffBySpecialty(string specialty)
        {
            var staffMembers = await _context.staff
                .Where(s => s.Specialty == specialty)
                .Join(_context.Users,
                      staff => staff.UserId,
                      user => user.UserId,
                      (staff, user) => new { FullName = user.FirstName + " " + user.LastName, Specialty = staff.Specialty, Email = user.Email })
                .ToListAsync();


            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(staffMembers);
            Console.WriteLine(jsonResponse);



            return Json(staffMembers);
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



        [HttpGet]
        public async Task<IActionResult> SearchDoctors(string term)
        {
            Console.WriteLine("Search Term: " + term); // Log the received term

            // Step 1: Find matching users based on the search term and ensure they have a non-null Specialty
            var matchingUsers = await _context.Users
                .Where(u => (u.FirstName.Contains(term) || u.LastName.Contains(term)) && u.staff.Any(s => s.Specialty != null))
                .Select(u => new { u.UserId, FullName = u.FirstName + " " + u.LastName })
                .ToListAsync();

            Console.WriteLine("-------------------------");
            Console.WriteLine(matchingUsers.Count); // Log the count of matching users
            Console.WriteLine("-------------------------");

            // No need to use Distinct here since we're returning objects with UserId which should be unique
            return Json(matchingUsers);
        }


        [HttpPost]
        public async Task<IActionResult> RemoveDoctorFromSystem(int UserId)
        {
            // Find the first staff member associated with the provided UserId
            var staffMember = await _context.staff
                .FirstOrDefaultAsync(s => s.UserId == UserId && s.Specialty != null);

            if (staffMember != null)
            {
                // Set Specialty to null
                staffMember.Specialty = null;

                // Save the changes to the database
                await _context.SaveChangesAsync();

                // Optionally, return a success message or redirect
                TempData["SuccessMessage"] = "Doctor has been successfully removed from the system.";
                return RedirectToAction("RemoveDoctor"); // Adjust as necessary
            }
            else
            {
                // Handle the case where no matching staff member is found or they already have no specialty
                TempData["ErrorMessage"] = "Doctor not found or already removed.";
                return RedirectToAction("RemoveDoctor"); // Adjust as necessary
            }
        }
    }
}