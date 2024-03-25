using CS310_SRS_App.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using CS310_SRS_App.Model;
using IDocument = QuestPDF.Infrastructure.IDocument;


namespace CS310_SRS_App.Controllers
{
    public class HealthDataController : Controller
    {
        private readonly CS310SRSDatabaseContext _context;

        public HealthDataController(CS310SRSDatabaseContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> PrintDetails(int id)
        {

            if (id == null)
            {
                return RedirectToAction(nameof(PatientHealthData));
            }

            return RedirectToAction(nameof(PatientHealthData));
        }

        [HttpGet]
        public IActionResult PatientHealthData()
        {
            // Check if the user is authenticated and verified
            /*@*
            if (HttpContext.Session.GetString("IsVerified") == null)
            {
                return NotFound(); // Or redirect to an error page, login page, or perform other actions as appropriate
            }
            */
            // Retrieve the patient's ID from the session
            if(HttpContext.Session.GetString("SessionKeyRole") != "Patient")
            {
                return View("PatientHealthData");
            }


            int currentUserId = Convert.ToInt32(HttpContext.Session.GetString("SessionKeyID"));
            var patient = _context.Patients.FirstOrDefault(p => p.UserId == currentUserId);
            Console.WriteLine("-----------------------------------------------------------");
            Console.WriteLine(patient.PatientId);
            Console.WriteLine(HttpContext.Session.GetString("SessionKeyRole"));
            Console.WriteLine("-----------------------------------------------------------");


            // Fetch patient health data from your data source based on the current user's ID
            var patientHealthData = _context.PatientCharts.Where(p => p.PatientId == patient.PatientId).ToList();

            foreach (var data in patientHealthData)
            {
                Console.WriteLine($"PatientChartID: {data.PatientChartID}, SBloodPressure: {data.SBloodPressure}, DBloodPressure: {data.DBloodPressure}, HeartRate: {data.HeartRate}, RespRate: {data.RespRate}, Tempk: {data.Tempk}");
            }
            // Pass the patient health data to the view
            return View(patientHealthData);
        }
        [HttpGet]
        public IActionResult StaffHealthData()
        {
            /*
            if (HttpContext.Session.GetString("IsVerified") == null)
            {
                return NotFound();
            }
            */
            return View("StaffHealthData");
        }

        [HttpPost]
        public async Task<IActionResult> SavePatientChart(PatientChart model, string selectedPatientName)
        {

            var patientNameParts = selectedPatientName.Split(' ');
            var firstName = patientNameParts[0];
            var lastName = patientNameParts[1];

            // Get the user ID based on the first name and last name
            var UserId = _context.Users
                                        .Where(u => u.FirstName == firstName && u.LastName == lastName)
                                        .Select(u => u.UserId)
                                        .FirstOrDefault();
            var patientId = _context.Patients
                                          .Where(p => p.UserId == UserId)
                                            .Select(p => p.PatientId)
                                            .FirstOrDefault();

            model.SubmissionDate = DateTime.Now;
            model.PatientId = patientId; // Set the PatientId in the model

            // Serialize the model object to a JSON string for logging
            var modelJson = JsonSerializer.Serialize(model, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine($"Received model: {modelJson}");
            Console.WriteLine($"Received Patient UserID: {selectedPatientName}");


            if (ModelState.IsValid)
            {
                // Check if this is a new entry or an update
                if (model.PatientId != null && _context.PatientCharts.Any(pc => pc.PatientId == model.PatientId))
                {
                    _context.Update(model);
                }
                else
                {
                    _context.Add(model);
                }
                await _context.SaveChangesAsync();

                // Redirect to a confirmation page or back to the form with a success message
                ViewBag.SuccessMessage = "Data Succesfully Saved!";
                return RedirectToAction("StaffHealthData"); // Adjust "SuccessPage" as needed
            }

            // If we got this far, something failed; redisplay form
            ViewBag.ErrorMessage = "An Error Occured When Saving The Health Data. Try Again.";
            return View("StaffHealthData");
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
            var patientUserIds = await _context.Patients
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

        public IActionResult Index()
        {
            return View();
        }
    }
}
