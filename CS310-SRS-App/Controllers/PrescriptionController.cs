using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using CS310_SRS_App.Model;

namespace CS310_SRS_App.Controllers
{
    public class PrescriptionController : Controller
    {
        private readonly CS310SRSDatabaseContext _context;

        public PrescriptionController(CS310SRSDatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult StaffPrescription()
        {
            if (HttpContext.Session.GetString("SessionKeyRole") != "Admin" &&
                HttpContext.Session.GetString("SessionKeyRole") != "Staff")
            {
                return Forbid(); // or any other action you prefer
            }
            return View("StaffPrescription");
        }
        public IActionResult ViewPrescriptions()
        {
            if (HttpContext.Session.GetString("SessionKeyRole") != "Patient")
            {
                return View("ViewPrescriptions");
            }

            int currentUserId = Convert.ToInt32(HttpContext.Session.GetString("SessionKeyID"));
            var patient = _context.Patients.FirstOrDefault(p => p.UserId == currentUserId);

            var ViewPrescriptions = _context.Prescriptions.Where(p => p.PatientId == patient.PatientId).ToList();

            return View(ViewPrescriptions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveStaffPrescription(Prescription model, string selectedPatientName)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Extract patient's first and last name from selectedPatientName
                    var patientNameParts = selectedPatientName.Split(' ');
                    var firstName = patientNameParts[0];
                    var lastName = patientNameParts[1];

                    // Find the patient ID using first and last name
                    var patientId = await _context.Patients
                        .Where(p => p.User.FirstName == firstName && p.User.LastName == lastName)
                        .Select(p => p.PatientId)
                        .FirstOrDefaultAsync();

                    if (patientId == 0)
                    {
                        // Patient not found, handle the error as needed
                        ViewBag.ErrorMessage = "Patient not found.";
                        return View("StaffPrescription");
                    }

                    // Attach the patient ID to the prescription
                    model.PatientId = patientId;

                    // Set other properties of the model as needed
                    model.DatePrescribed = DateTime.Now;

                    // Add the prescription to the database
                    _context.Add(model);
                    await _context.SaveChangesAsync();

                    // Redirect to a confirmation page or back to the form with a success message
                    ViewBag.SuccessMessage = "Prescription saved successfully!";
                    return RedirectToAction(nameof(StaffPrescription));
                }
                catch (Exception ex)
                {
                    // Log the exception and handle the error as needed
                    ViewBag.ErrorMessage = "An error occurred while saving the prescription.";
                    return View("StaffPrescription");
                }
            }

            // If ModelState is invalid, redisplay the form with validation errors
            return View("StaffPrescription", model);
        }

        [HttpGet]
        public async Task<IActionResult> SearchPatients(string term)
        {
            var matchingPatients = await _context.Patients
                .Where(p => EF.Functions.Like(p.User.FirstName, $"%{term}%") ||
                            EF.Functions.Like(p.User.LastName, $"%{term}%"))
                .Select(p => $"{p.User.FirstName} {p.User.LastName}")
                .ToListAsync();

            return Json(matchingPatients);
        }
        public IActionResult PrescriptionRequests()
        {
            var requestedPrescriptions = _context.Prescriptions.Where(p => p.RefillRequested).ToList();
            return View(requestedPrescriptions);
        }
    }
}
