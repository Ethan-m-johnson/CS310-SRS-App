using System;
using System.Net;
using System.Net.Mail;
using CS310_SRS_App.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore;

namespace CS310_SRS_App.Views.Message
{
    public class MessageController : Controller
    {
        private readonly CS310SRSDatabaseContext _context;

        public MessageController(CS310SRSDatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult MessagePage()
        {
            //get list of patients or doctors
            //var users = _context.User.ToList();
            return View("MessagePage");

        }

        [HttpPost]
        public IActionResult sendMessage(int senderId, int recieverId, string content)
        {
            /*//check if patient or a doctor
            var senderIsPatient = _context.Patients.Any(p => p.UserId == senderId);

            //check if reciever is doctor or patient
            var recieverIsDoctor = _context.Doctors.Any(p => p.StaffId == recieverId);

            //make sure sender and reciver exist
            if (!senderIsPatient && !recieverIsDoctor)
            {
                return BadRequest("Sender and reciver are not valid users");
            }*/

            //create new message
            var message = new Model.Message
            {
                UserId = senderId,
                ContactId = recieverId,
                Message1 = content,
                DateSent = DateTime.Now,

            };

            _context.Messages.Add(message);
            //_context.SaveChanges();

            return RedirectToAction("MessagePage");

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
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendPatientToDocEmail(String? emailBody, string selectedDoctorName)
        {
            try
            {
                var doctorNameParts = selectedDoctorName.Split(' ');
                var firstName = doctorNameParts[0];
                var lastName = doctorNameParts[1];

                //get doctors ID based on name
                /*var staffId = await _context.staff
            .Where(s => s.FirstName == firstName && s.LastName == lastName)
            .Select(s => s.StaffId)
            .FirstOrDefaultAsync();*/

                var emailAddress = await _context.Users
                    .Where(u => u.FirstName == firstName && u.LastName == lastName)
                    .Select(u => u.Email)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(emailAddress))
                {
                    var message = new MailMessage();
                    message.From = new MailAddress("cs310automatedemailnoreply@gmail.com");
                    message.To.Add(emailAddress);
                    message.Subject = "Message From Patient";
                    message.Body = $"Hello,<br><br>" + emailBody;
                    message.IsBodyHtml = true;

                    using (var client = new SmtpClient("smtp.gmail.com", 587))
                    {
                        client.EnableSsl = true; // Gmail requires SSL
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential("cs310automatedemailnoreply@gmail.com", "rzzk ubqp pgda opju");
                        await client.SendMailAsync(message);
                    }

                    return Ok("Email sent successfully.");
                }
                else
                {
                    // Log or handle the case where there's no email
                    Console.WriteLine("No email provided for sending the message.");
                    return BadRequest("No email provided for sending the message.");
                }
            }
            catch (Exception ex)
            {
                // Log or handle the error more gracefully
                Console.WriteLine($"Failed to send message: {ex.Message}");
                return StatusCode(500, "An error occurred while sending the message.");
            }
        }


        /*
        [HttpPost]
        public async Task<IActionResult> SendPatientToDocEmail(String? emailBody, string selectedDoctorName)
        {

            var doctorNameParts = selectedDoctorName.Split(' ');
            var firstName = doctorNameParts[0];
            var lastName = doctorNameParts[1];



            // Get the user ID based on the first name and last name
            var UserObject =  _context.Users
                .Where(u => u.FirstName == firstName && u.LastName == lastName)
                .Select(u => u.UserId)
                .FirstOrDefault();



            var emailAddress = await _context.Users
                .Where(u => u.FirstName == firstName && u.LastName == lastName)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();


            try
            {
                var message = new MailMessage();
                message.From = new MailAddress("cs310automatedemailnoreply@gmail.com");



                if (!string.IsNullOrEmpty(emailAddress))
                {
                    message.To.Add(emailAddress);
                }
                else
                {
                    // Log or handle the case where there's no email
                    Console.WriteLine("No email provided for sending the message.");
                    return BadRequest("No email provided for sending the message.");
                }



                message.Subject = "Message From Patient";
                message.Body = $"Hello,<br><br>" +
                               emailBody;



                message.IsBodyHtml = true;



                using (var client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true; // Gmail requires SSL
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential("cs310automatedemailnoreply@gmail.com", "rzzk ubqp pgda opju");
                    await client.SendMailAsync(message);
                    //client.Send(message);
                }

                return Ok("Enail sent successfully.");
            }
            catch (Exception ex)
            {
                // Log or handle the error more gracefully
                Console.WriteLine($"Failed to send message: {ex.Message}");
            }

        }*/

    }
}