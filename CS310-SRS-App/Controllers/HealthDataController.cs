using CS310_SRS_App.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using IDocument = QuestPDF.Infrastructure.IDocument;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using System.Diagnostics;
using QuestPDF.Helpers;
using Microsoft.IO;

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

            var patientDocToPass = await _context.PatientCharts
                .SingleOrDefaultAsync(m => m.PatientChartID == id);

            if (patientDocToPass == null)
            {

                ViewBag.ErrorMessage = "We couldn't find the requested patient chart. " +
                    "This may be because the chart does not exist or the ID provided is incorrect. Please verify the information and try again.";

                return RedirectToAction(nameof(PatientHealthData));
            }

            //---------------------------------------------------------------
            //This section is for local pdf
            //var fileName = "HealthDataReport.pdf";
            //var report = new InvoiceDocument(patientDocToPass);
            //report.GeneratePdf(fileName);
            //var startInfo = new ProcessStartInfo("explorer.exe", fileName);
            //Process.Start(startInfo);
            //----------------------------------------------------------------


            //---------------------------------------------------------------
            //This section is for live pdf


            var streamManager = HttpContext.RequestServices.GetRequiredService<RecyclableMemoryStreamManager>();
            using var memoryStream = streamManager.GetStream();

            // Ensure your PDF generation logic writes directly into `memoryStream`.
            var report = new InvoiceDocument(patientDocToPass);
            report.GeneratePdf(memoryStream); // This should be modified to accept a stream if it doesn't already.

            memoryStream.Position = 0;

            HttpContext.Response.ContentType = "application/pdf";
            HttpContext.Response.Headers.ContentDisposition = $"attachment; filename=\"HealthDataReport.pdf\"";

            await memoryStream.CopyToAsync(HttpContext.Response.Body);
            //---------------------------------------------------------------
            return new EmptyResult();
        }
        public class InvoiceDocument : IDocument
        {
            private PatientChart patientDocToPass;

            public InvoiceDocument(PatientChart patientDocToPass)
            {
                this.patientDocToPass = patientDocToPass;
            }

            public DocumentMetadata GetMetadata() => DocumentMetadata.Default;



            public void Compose(IDocumentContainer container)
            {
                container
                     .Page(page =>
                     {
                         page.Margin(50);

                         page.Header().Element(ComposeHeader);
                         page.Content().Element(ComposeContent);
                         page.Footer().Height(10).Background(Colors.Grey.Lighten1);
                     });
            }

            void ComposeHeader(QuestPDF.Infrastructure.IContainer container)
            {
                container.Row(row =>
                {
                    var titleStyle = TextStyle.Default.FontSize(10).SemiBold().Italic().FontColor(Colors.Black);
                    //row.ConstantItem(50).Height(50).Image("wwwroot/Assets/dot.png"); //Add picture later


                    row.RelativeItem().Column(column =>
                    {
                        column.Item().AlignCenter().Text(text =>
                        {
                            text.Span("SRS: Health Data Information Request").Style(titleStyle);
                        });

                        column.Item().AlignRight().Text(text =>
                        {
                            text.Span("Date Requested: " + DateTime.Now).Style(titleStyle);
                        });

                    });
                });
            }

            void ComposeContent(IContainer container)
            {
                var tableStyle = TextStyle.Default.FontSize(10).FontColor(Colors.Black);
                if (patientDocToPass == null)
                {
                    // Handle the case where patientDocToPass is null
                    // For example, display a message indicating that the data is unavailable
                    container.Text("Error: Patient chart data is unavailable.").Style(tableStyle);
                    return;
                }
                container
                        .Background(Colors.White)
                        .AlignLeft()
                        .AlignCenter()
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Cell().Row(1).ColumnSpan(3).Column(1).PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                            table.Cell().Row(2).ColumnSpan(3).Column(1).Text($"Health Data, Date Recorded: {patientDocToPass.SubmissionDate?.ToString("MMMM d, yyyy") ?? "N/A"}").Style(tableStyle);
                            table.Cell().Row(3).ColumnSpan(3).Column(1).Text($"Systolic Blood Pressure: {patientDocToPass.SBloodPressure?.ToString("F2") ?? "N/A"}").Style(tableStyle);
                            table.Cell().Row(4).ColumnSpan(3).Column(1).Text($"Diastolic Blood Pressure: {patientDocToPass.DBloodPressure?.ToString("F2") ?? "N/A"}").Style(tableStyle);
                            table.Cell().Row(5).ColumnSpan(3).Column(1).Text($"Heart Rate: {patientDocToPass.HeartRate?.ToString("F2") ?? "N/A"}").Style(tableStyle);
                            table.Cell().Row(6).ColumnSpan(3).Column(1).Text($"Respiratory Rate: {patientDocToPass.RespRate?.ToString("F2") ?? "N/A"}").Style(tableStyle);
                            table.Cell().Row(7).ColumnSpan(3).Column(1).Text($"Temperature (F): {patientDocToPass.Tempk?.ToString("F2") ?? "N/A"}").Style(tableStyle);
                            table.Cell().Row(8).ColumnSpan(3).Column(1).PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                        });
            }
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
