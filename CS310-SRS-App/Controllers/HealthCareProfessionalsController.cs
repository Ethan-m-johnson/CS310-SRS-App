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
                .Join(_context.Users, // Assuming a link between staff and user details
                      staff => staff.UserId,
                      user => user.UserId,
                      (staff, user) => new { user.FirstName, user.LastName, staff.Specialty })
                .Select(x => new
                {
                    FullName = x.FirstName + " " + x.LastName,
                    Specialty = x.Specialty
                })
                .ToListAsync();
            Console.WriteLine("you made it");
            return Json(staffMembers);
        }
        /*
        [HttpGet]

        public async Task<IActionResult> Index()
        {
            List<staff> docList = _context.staff
                .Join(
                    _context.Users,
                    staff => staff.UserId,
                    user => user.UserId,
                    (staff, user) => new staff
                    {

                        Specialty = staff.Specialty
                        User = new User
                        {
                            FirstName = user.FirstName,
                            LastName = user.LastName,

                        }
                    }
                    )
                .Where(s => s.Specialty != null)
                .ToListAsync();
            
               return View(docList);
                
        }*
        [HttpGet]
        public IActionResult Index(string search)
        {
            IQueryable<staff> staffQuery = _context.staff
                .Include(s => s.User)
                .Where(s => s.Specialty != null);

            if (!string.IsNullOrEmpty(search))
            {
                staffQuery = staffQuery.Where(s => s.User.FirstName.Contains(search) || s.User.LastName.Contains(search));
            }

            List<staff> staffList = staffQuery.ToList();

            return View("staffList");
        } */

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
        }
        
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
        
            [HttpPost]



        /*
            public async Task<IActionResult> RemoveDoctor(string term)
            {
                Console.WriteLine("Search Term: " + term); // Log the received term

                var matchingUsers = await _context.Users
                    .Where(u => u.FirstName.Contains(term) || u.LastName.Contains(term))
                    .Select(u => new { u.UserId, FullName = u.FirstName + " " + u.LastName })
                    .ToListAsync();

                // Step 2: Filter these users to keep only those who are also staff
                var staffUserIDs = await _context.staff
                    .Select(p => p.UserId)
                    .Distinct()
                    .ToListAsync();

                var patientNames = matchingUsers
                   .Where(u => staffUserIDs.Contains(u.UserId)) // Keep only users who are staff
                   .Select(u => u.FullName)
                   .Distinct()
                   .ToList();


                return string("succes");
            } */
        [HttpGet]
        public async Task<IActionResult> SearchDoctorsBySpecialty(string specialty)
        {
            // Step 1: Find users who are doctors with the specified specialty
            // Assuming there's a direct relationship or a way to query users with specialties
            var doctors = await _context.staff
                .Where(s => s.Specialty != null && s.Specialty == specialty)
                .Join(_context.Users, // Assuming staff information includes user details
                    staff => staff.UserId,
                    user => user.UserId,
                    (staff, user) => new { user, staff })
                .Select(x => new { x.user.UserId, FullName = x.user.FirstName + " " + x.user.LastName })
                .ToListAsync();

            // Convert to a format suitable for a dropdown (if needed)
            var doctorNames = doctors.Select(x => x.FullName).Distinct().ToList();

            Console.WriteLine("-------------------------");
            Console.WriteLine(doctorNames.Count + " doctors found for specialty: " + specialty);
            Console.WriteLine("-------------------------");

            return Json(doctorNames);
        }
    }
}