using CS310_SRS_App.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CS310_SRS_App.Controllers
{
    public class PrescriptionController : Controller
    {
        private readonly CS310SRSDatabaseContext _context;

        public PrescriptionController(CS310SRSDatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> PrintDetails(int id)
        {
            if (id == null)
            {
                return RedirectToAction(nameof(ViewPrescriptions));
            }

            var infoToPass = await _context.Prescriptions
                .SingleOrDefaultAsync(m => m.PatientId == id);

            if (infoToPass == null)
            {
                ViewBag.ErrorMessage = "We couldn't find the requested patient chart. " +
                    "This may be because the chart does not exist or the ID provided is incorrect. Please verify the information and try again.";

                return RedirectToAction(nameof(ViewPrescriptions));
            }

            return RedirectToAction(nameof(ViewPrescriptions));
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

        [HttpGet]
        public IActionResult StaffPrescription()
        {
            var prescription = new Prescription();
            return View(prescription);
        }

        [HttpPost]
        public async Task<IActionResult> SaveStaffPrescription(Prescription model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(model);
                    await _context.SaveChangesAsync();
                    ViewBag.SuccessMessage = "Prescription successfully saved!";
                    return RedirectToAction(nameof(StaffPrescription));
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = $"Error saving prescription: {ex.Message}";
                }
            }
            return View(nameof(StaffPrescription), model);
        }

        [HttpGet]
        public IActionResult StaffHealthData()
        {
            return View("StaffHealthData");
        }

        [HttpPost]
        public async Task<IActionResult> SavePrescription(Prescription model, string selectedPatientName)
        {
            var patientNameParts = selectedPatientName.Split(' ');
            var firstName = patientNameParts[0];
            var lastName = patientNameParts[1];

            var UserId = _context.Users
                                        .Where(u => u.FirstName == firstName && u.LastName == lastName)
                                        .Select(u => u.UserId)
                                        .FirstOrDefault();
            var patientId = _context.Patients
                                          .Where(p => p.UserId == UserId)
                                            .Select(p => p.PatientId)
                                            .FirstOrDefault();

            model.PatientId = patientId;

            var modelJson = JsonSerializer.Serialize(model, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine($"Received model: {modelJson}");
            Console.WriteLine($"Received Patient UserID: {selectedPatientName}");

            if (ModelState.IsValid)
            {
                if (model.PatientId != null && _context.Prescriptions.Any(pc => pc.PatientId == model.PatientId))
                {
                    _context.Update(model);
                }
                else
                {
                    _context.Add(model);
                }
                await _context.SaveChangesAsync();

                ViewBag.SuccessMessage = "Data Succesfully Saved!";
                return RedirectToAction("StaffHealthData");
            }

            ViewBag.ErrorMessage = "An Error Occured When Saving The Health Data. Try Again.";
            return View("StaffHealthData");
        }

        [HttpGet]
        public async Task<IActionResult> SearchPatients(string term)
        {
            Console.WriteLine("Search Term: " + term);

            var matchingUsers = await _context.Users
                .Where(u => u.FirstName.Contains(term) || u.LastName.Contains(term))
                .Select(u => new { u.UserId, FullName = u.FirstName + " " + u.LastName })
                .ToListAsync();

            var patientUserIds = await _context.Patients
                .Select(p => p.UserId)
                .Distinct()
                .ToListAsync();

            var patientNames = matchingUsers
                .Where(u => patientUserIds.Contains(u.UserId))
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