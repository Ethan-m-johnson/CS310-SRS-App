using System;
using System.Net;
using System.Net.Mail;
using System.Reflection.Metadata;
using CS310_SRS_App.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore;

namespace CS310_SRS_App.Controllers
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
        public async Task<IActionResult> sendMessageEmail(string? body, string docName, string senderName)
        {
            try
            {
                var doctorNameParts = docName.Split(' ');
                var firstName = doctorNameParts[0];
                var lastName = doctorNameParts[1];
                Console.WriteLine("---last-----" + lastName);
                Console.WriteLine("---first-----" + firstName);
                

                //get doctors ID based on name
                var userObject = await _context.Users
                .Where(s => s.FirstName == firstName && s.LastName == lastName)
                .FirstOrDefaultAsync();
                var doctorId = userObject?.UserId;
                Console.WriteLine("---docId-----" + doctorId);
                var emailAddress = userObject?.Email;
                Console.WriteLine("---doc email-----" + emailAddress);

                //get senderID and email
                var sender = senderName.Split(' ');
                var senderfname = sender[0];
                var senderlname = sender[1];
                Console.WriteLine("---last-----" + senderlname);
                Console.WriteLine("---first-----" + senderfname);

                var senderObject = await _context.Users
                .Where(s => s.FirstName == senderfname && s.LastName == senderlname)
                .FirstOrDefaultAsync();
                var senderId = senderObject?.UserId;
                Console.WriteLine("---senderId-----" + senderId);
                var senderEmail = senderObject?.Email;
                Console.WriteLine("---sender email-----" + senderEmail);

                //construct email message
                var message = new MailMessage();
                message.To.Add(emailAddress);
                message.From = new MailAddress("cs310automatedemailnoreply@gmail.com");
                message.Subject = "SRS Message";
                message.Body = $"Hello,Dr. {docName} <br><br>" + body + $"<br><br> Thank you,<br><br>" + $"{senderfname} " + $"{senderlname} <br><br><br><br>"
                    + $"Please DO NOT reply to this email. Please contact {senderEmail} directly." ;
                message.IsBodyHtml = true;

                Console.WriteLine("------------------------------HERE------------------------------");

                Contact createContact = new Contact
                {
                    User1Id = (int)senderId,
                    User2Id = (int)doctorId
                };

                _context.Contacts.Add(createContact);
                await _context.SaveChangesAsync();

                Message createMessage = new Message
                {

                    UserId = (int)senderId,
                    DateSent = DateTime.Now,
                    Message1 = body
                };
                _context.Messages.Add(createMessage);
                await _context.SaveChangesAsync();

                using (var client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true; // Gmail requires SSL
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential("cs310automatedemailnoreply@gmail.com", "rzzk ubqp pgda opju");
                    await client.SendMailAsync(message);
                }

                return View("SuccessMessagePage");
            }
            catch (Exception ex)
            {
                // Log or handle the error more gracefully
                return View("ErrorMessagePage");

            }
        }


    }
}
