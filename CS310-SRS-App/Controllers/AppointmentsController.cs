using CS310_SRS_App.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS310_SRS_App.Controllers
{
    public class AppointmentsController : Controller
    {

        private CS310SRSDatabaseContext _context;
        
        public AppointmentsController(CS310SRSDatabaseContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var verif = HttpContext.Session.GetString("IsVerified");
            if (verif == null)
            {
                return View("Views/Users/AccountInformation/Login.cshtml");
            }
            else if (!verif.Equals("true"))
            {
                return View("Views/Users/AccountInformation/Login.cshtml");
            }

            var role = HttpContext.Session.GetString("SessionKeyRole");
            var id = int.Parse(HttpContext.Session.GetString("SessionKeyID"));
            List<AppointmentDisplay> displays = new List<AppointmentDisplay>();
            List<Appointment> appointments = new List<Appointment>();
            switch (role){
                case "Patient":
                    var pat = await _context.Patients.Where(p => p.UserId == id).FirstOrDefaultAsync();
                    var patientView = _context.Appointments.Where(a => a.PatientId == pat.PatientId);
                    appointments = await patientView.ToListAsync();
                    //return View(await patientView.ToListAsync());
                    break;
                case "Staff":
                    var st = await _context.staff.Where(s => s.UserId == id).FirstOrDefaultAsync();
                    var staffView = _context.Appointments.Where(a => a.StaffId == st.StaffId);
                    appointments = await staffView.ToListAsync();
                    //return View(await staffView.ToListAsync());
                    break;
                case "Admin":
                    appointments = await _context.Appointments.ToListAsync();
                    //return View(await _context.Appointments.ToListAsync());
                    break;
            }
            foreach (var app in appointments)
            {
                var itpat = await _context.Patients.FindAsync(app.PatientId);
                var patuser = await _context.Users.FindAsync(itpat.UserId);
                string patname = patuser.FirstName + " " + patuser.LastName;

                var itstaff = await _context.staff.FindAsync(app.StaffId);
                var staffuser = await _context.Users.FindAsync(itstaff.UserId);
                string staffname = staffuser.FirstName + " " + staffuser.LastName;

                displays.Add(new AppointmentDisplay
                {
                    PatientId = app.PatientId,
                    StaffId = app.StaffId,
                    dateTime = app.DateTime,
                    Location = app.Location,
                    Topic = app.Topic,
                    PatientName = patname,
                    StaffName = staffname
                });
            }
            displays.Sort(AppointmentDisplay.Compare);
            return View(displays);
        }

        public IActionResult ToCreate()
        {
            var role = HttpContext.Session.GetString("SessionKeyRole");
            if (role.Equals("Staff")||role.Equals("Admin"))
            {
                return View("Create");
            }
            else if (role.Equals("Patient"))
            {
                return View("PatientCreate");
            }
            return View("Views/Home/Index");
        }

        public IActionResult ToEdit(AppointmentDisplay dispapp)
        {
            var role = HttpContext.Session.GetString("SessionKeyRole");
            if (role.Equals("Staff") || role.Equals("Admin"))
            {

                var app = FromDisplay(dispapp);
                var newapp = new AppointmentEdit
                {
                    PatientId = app.PatientId,
                    StaffId = app.StaffId,
                    OrigDateTime = app.DateTime,
                    DateTime = app.DateTime,
                    Location = app.Location,
                    Topic = app.Topic
                };
                return View("StaffEdit", newapp);
            }
            return View("Views/Home/Index");
        }

        public IActionResult ToDelete(AppointmentDisplay dispapp)
        {
            //var app = FromDisplay(dispapp);
            return View("Delete", dispapp);
        }

        public IActionResult ToEditRequest(AppointmentDisplay dispapp)
        {
            var app = FromDisplay(dispapp);
            var newapp = new AppointmentEdit
            {
                PatientId = app.PatientId,
                StaffId = app.StaffId,
                OrigDateTime = app.DateTime,
                DateTime = app.DateTime,
                Location = app.Location,
                Topic = app.Topic
            };
            return View("PatientEdit", newapp);
        }

        public IActionResult Confirmation()
        {
            return View("Confirmation");
        }

        public IActionResult StaffConfirmation()
        {
            return View("ConfirmationStaff");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAppointment(string patientEmail, DateTime time, string location, string topic)
        {
            if(patientEmail == null || time == null || location == null || topic == null)
            {
                TempData["Error"] = "Please enter a value into all fields";
                return RedirectToAction("ToCreate");
            }
            if (time < DateTime.Now)
            {
                TempData["Error"] = "Setting an appoint for the past is not allowed";
                return RedirectToAction("ToCreate");
            }
            var user = await _context.Users.Where
                (u => u.Email.ToLower() == patientEmail.ToLower())
                .FirstOrDefaultAsync();
            if(user == null)
            {
                TempData["Error"] = "Could not find patient from email.";
                return RedirectToAction("ToCreate");
            }
            var patient = await _context.Patients.Where
               (p => p.UserId == user.UserId)
               .FirstOrDefaultAsync();
            if (patient == null) 
            {
                TempData["Error"] = "Could not find patient from email.";
                return RedirectToAction("ToCreate");
            }
            var st = await _context.staff.Where
               (s => s.UserId == int.Parse(HttpContext.Session.GetString("SessionKeyID")))
               .FirstOrDefaultAsync();
            Appointment app = new Appointment
            {
                Patient = patient,
                Staff = st,
                DateTime = time,
                Location = location,
                Topic = topic
            };
            bool duplicate = await _context.Appointments.AnyAsync(a => a.PatientId == patient.PatientId && a.DateTime == time);
            if (duplicate)
            {
                TempData["Error"] = "There is already an appointment for this patient at this time.";
                return RedirectToAction("ToCreate");
            }
            _context.Add(app);
            await _context.SaveChangesAsync();

            var userStaff = await _context.Users.Where
               (u => u.UserId == st.UserId)
               .FirstOrDefaultAsync();
            if( userStaff != null) 
            {
                SendNotificationEmail(user.Email, userStaff.FirstName + " " + userStaff.MiddleName + " " + userStaff.LastName, location, topic);
            }
            return RedirectToAction("StaffConfirmation");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAppointmentRequest(string staffEmail, DateTime time, string location, string topic)
        {
            if (staffEmail == null || time == null || location == null || topic == null)
            {
                TempData["Error"] = "Please enter a value into all fields";
                return RedirectToAction("ToCreate");
            }
            if (time < DateTime.Now)
            {
                TempData["Error"] = "Setting an appoint for the past is not allowed";
                return RedirectToAction("ToCreate");
            }
            var user = await _context.Users.Where
                (u => u.Email.ToLower() == staffEmail.ToLower())
                .FirstOrDefaultAsync();
            if (user == null)
            {
                TempData["Error"] = "Could not find staff from email.";
                return RedirectToAction("ToCreate");
            }
            var staff = await _context.staff.Where
               (s => s.UserId == user.UserId)
               .FirstOrDefaultAsync();
            if (staff == null)
            {
                TempData["Error"] = "Could not find staff from email.";
                return RedirectToAction("ToCreate");
            }
            var patient = await _context.Patients.Where
               (s => s.UserId == int.Parse(HttpContext.Session.GetString("SessionKeyID")))
               .FirstOrDefaultAsync();
            Appointment app = new Appointment
            {
                Patient = patient,
                Staff = staff,
                DateTime = time,
                Location = location,
                Topic = topic
            };
            bool duplicate = await _context.Appointments.AnyAsync(a => a.StaffId == staff.StaffId && a.DateTime == time);
            if (duplicate)
            {
                TempData["Error"] = "There is already an appointment for this staff at this time.";
                return RedirectToAction("ToCreate");
            }

            var userPatient = await _context.Users.Where
               (u => u.UserId == patient.UserId)
               .FirstOrDefaultAsync();
            if (userPatient != null)
            {
                SendCreationRequestEmail(user.Email, userPatient.FirstName + " " + userPatient.MiddleName + " " + userPatient.LastName, location, time.ToString(), topic);
            }
            return RedirectToAction("Confirmation");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAppointment(AppointmentEdit app)
        {
            bool scheduled = await _context.Appointments.AnyAsync(a => a.DateTime == app.DateTime && a.PatientId != app.PatientId && a.StaffId != a.StaffId);

            if (scheduled)
            {
                TempData["Error"] = "There is already an appointment for this patient at this time.";
                return RedirectToAction("ToEdit");
            }

            if(app.DateTime <  DateTime.Now) 
            {
                TempData["Error"] = "Setting an appoint for the past is not allowed";
                return RedirectToAction("ToEdit");
            }

            var newapp = new Appointment
            {
                PatientId = app.PatientId,
                StaffId = app.StaffId,
                DateTime = app.DateTime,
                Location = app.Location,
                Topic = app.Topic
            };

            if (app.DateTime != app.OrigDateTime)
            {
                var tempapp = new Appointment
                {
                    PatientId = app.PatientId,
                    StaffId = app.StaffId,
                    DateTime = app.OrigDateTime,
                    Location = app.Location,
                    Topic = app.Topic
                };
                _context.Appointments.Remove(tempapp);
                _context.Appointments.Add(newapp);
                await _context.SaveChangesAsync();
            }
            else
            {
                _context.Update(newapp);
                await _context.SaveChangesAsync();
            }

            var pat = await _context.Patients.FindAsync(app.PatientId);
            var patuser = await _context.Users.FindAsync(pat.UserId);

            var st = await _context.staff.FindAsync(app.StaffId);
            var stuser = await _context.Users.FindAsync(st.UserId);
            var staffname = stuser.FirstName + " " + stuser.LastName;

            if(patuser != null && stuser != null)
            {
                SendEditNotificationEmail(patuser.Email, staffname, newapp, app);
            }

            return RedirectToAction("StaffConfirmation");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAppointmentRequest(AppointmentEdit app)
        {
            bool scheduled = await _context.Appointments.AnyAsync(a => a.DateTime == app.DateTime);

            if (scheduled)
            {
                TempData["Error"] = "There is already an appointment for this patient at this time.";
                return RedirectToAction("ToEdit");
            }

            if (app.DateTime < DateTime.Now)
            {
                TempData["Error"] = "Setting an appoint for the past is not allowed";
                return RedirectToAction("ToEdit");
            }
            
            var staff = await _context.staff.FindAsync(app.StaffId);
            if(staff == null)
            {
                TempData["Error"] = "There was an unexpected issue. Please try again later";
                return RedirectToAction("ToEdit");
            }
            var user = await _context.Users.FindAsync(staff.UserId);
            if (user == null)
            {
                TempData["Error"] = "There was an unexpected issue. Please try again later";
                return RedirectToAction("ToEdit");
            }
            SendAppointmentEditRequestEmail(user.Email, app);
            return RedirectToAction("Confirmation");
        }

        public async Task<IActionResult> DeleteAppointment(Appointment app)
        {
            if (!HttpContext.Session.GetString("IsVerified").Equals("true"))
            {
                return RedirectToAction("Index");
            }
            _context.Appointments.Remove(app);
            await _context.SaveChangesAsync();

            var pat = await _context.Patients.FindAsync(app.PatientId);
            var patuser = await _context.Users.FindAsync(pat.UserId);
            var patname = patuser.FirstName + " " + patuser.LastName;

            var st = await _context.staff.FindAsync(app.StaffId);
            var stuser = await _context.Users.FindAsync(st.UserId);
            var staffname = stuser.FirstName + " " + stuser.LastName;

            if(stuser != null)
            {
                SendCancelationEmail(app, stuser.Email, patname, staffname);
            }

            if(patuser != null)
            {
                SendCancelationEmail(app, patuser.Email, patname, staffname);
            }

            return RedirectToAction("StaffConfirmation");
        }

        public IActionResult SendNotificationEmail(string? email, string name, string location, string topic)
        {
            try
            {
                var message = new MailMessage();
                message.From = new MailAddress("cs310automatedemailnoreply@gmail.com");

                if (!string.IsNullOrEmpty(email))
                {
                    message.To.Add(email);
                }
                else
                {
                    // If there is no email inputted then it will just return the view without trying to send the email
                    return View();
                }

                message.Subject = "SRS Appointment Set!";
                message.Body = $"Hello,<br><br>" +
                    $"A staff memeber just set an appiontment for you at {location} with {name} <br><br>" +
                    "Here is what the appiontment is about: <br><br>" +
                    $"{topic}<br><br>" +
                    $"To learn more, click <a href='https://localhost:7067/Appointments'>here</a> to go to the appointments page<br>";

                message.IsBodyHtml = true;


                Console.WriteLine("------------------------------HERE------------------------------");

                using (var client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true; // Gmail requires SSL
                    client.UseDefaultCredentials = false;

                    client.Credentials = new NetworkCredential("cs310automatedemailnoreply@gmail.com", "rzzk ubqp pgda opju");


                    client.Send(message);
                }

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString() + " - This is because the email is in incorrect format.");
                return View(); // This exception should be caught if the email address inputted does not follow email guidelines
            }
        }

        public IActionResult SendEditNotificationEmail(string? email, string staffname, Appointment edited, AppointmentEdit orig)
        {
            try
            {
                var message = new MailMessage();
                message.From = new MailAddress("cs310automatedemailnoreply@gmail.com");

                if (!string.IsNullOrEmpty(email))
                {
                    message.To.Add(email);
                }
                else
                {
                    // If there is no email inputted then it will just return the view without trying to send the email
                    return View();
                }

                message.Subject = "SRS Appointment Set!";
                message.Body = $"Hello,<br><br>" +
                    $"A staff memeber just edited an appiontment for you which was originally at {orig.Location} with {staffname} <br><br>" +
                    "Here is what the appiontment is about: <br><br>" +
                    $"Staff: {staffname}<br><br>" +
                    $"Date and Time: {edited.DateTime.ToString()}<br><br>" +
                    $"Location:{edited.Location}<br><br>" +
                    $"Topic:{edited.Topic}<br><br>" +
                    "<br><br>" +
                    $"To learn more, click <a href='https://localhost:7067/Appointments'>here</a> to go to the appointments page<br>";

                message.IsBodyHtml = true;


                Console.WriteLine("------------------------------HERE------------------------------");

                using (var client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true; // Gmail requires SSL
                    client.UseDefaultCredentials = false;

                    client.Credentials = new NetworkCredential("cs310automatedemailnoreply@gmail.com", "rzzk ubqp pgda opju");


                    client.Send(message);
                }

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString() + " - This is because the email is in incorrect format.");
                return View(); // This exception should be caught if the email address inputted does not follow email guidelines
            }
        }

        public IActionResult SendCreationRequestEmail(string? email, string name, string location, string time, string topic)
        {
            try
            {
                var message = new MailMessage();
                message.From = new MailAddress("cs310automatedemailnoreply@gmail.com");

                if (!string.IsNullOrEmpty(email))
                {
                    message.To.Add(email);
                }
                else
                {
                    // If there is no email inputted then it will just return the view without trying to send the email
                    return View();
                }

                message.Subject = "SRS Appointment Request";
                message.Body = $"Hello,<br><br>" +
                    $"A patient named {name} just requested an appointment with you at {location} : {time} <br><br>" +
                    "Here is what the appiontment is about: <br><br>" +
                    $"{topic}<br><br>" +
                    $"To create the appointment, click <a href='https://localhost:7067/Appointments/ToCreate'>here</a> to go to the create page and enter the information<br><br>" +
                    $"To learn more, click <a href='https://localhost:7067/Appointments'>here</a> to go to the appointments page<br>";

                message.IsBodyHtml = true;


                Console.WriteLine("------------------------------HERE------------------------------");

                using (var client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true; // Gmail requires SSL
                    client.UseDefaultCredentials = false;

                    client.Credentials = new NetworkCredential("cs310automatedemailnoreply@gmail.com", "rzzk ubqp pgda opju");


                    client.Send(message);
                }

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString() + " - This is because the email is in incorrect format.");
                return View(); // This exception should be caught if the email address inputted does not follow email guidelines
            }
        }

        public IActionResult SendCancelationEmail(Appointment app, string email, string patname, string staffname)
        {
            try
            {
                var message = new MailMessage();
                message.From = new MailAddress("cs310automatedemailnoreply@gmail.com");

                if (!string.IsNullOrEmpty(email))
                {
                    message.To.Add(email);
                }
                else
                {
                    // If there is no email inputted then it will just return the view without trying to send the email
                    return View();
                }

                message.Subject = "SRS Appointment Request";
                message.Body = $"Hello,<br><br>" +
                    $"An appointment you were a part of just got deleted<br><br>" +
                    "Here are the details of the deleted appointment: <br><br>" +
                    $"Patient: {patname}<br><br>" +
                    $"Staff: {staffname}<br><br>" +
                    $"Date and Time: {app.DateTime.ToString()}<br><br>" +
                    $"Location:{app.Location}<br><br>" +
                    $"Topic:{app.Topic}<br><br>" +
                    "<br><br>"+
                    $"To learn more, click <a href='https://localhost:7067/Appointments'>here</a> to go to the appointments page<br>";

                message.IsBodyHtml = true;


                Console.WriteLine("------------------------------HERE------------------------------");

                using (var client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true; // Gmail requires SSL
                    client.UseDefaultCredentials = false;

                    client.Credentials = new NetworkCredential("cs310automatedemailnoreply@gmail.com", "rzzk ubqp pgda opju");


                    client.Send(message);
                }

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString() + " - This is because the email is in incorrect format.");
                return View(); // This exception should be caught if the email address inputted does not follow email guidelines
            }
        }

        public IActionResult SendAppointmentEditRequestEmail(string? email, AppointmentEdit app)
        {
            try
            {
                var message = new MailMessage();
                message.From = new MailAddress("cs310automatedemailnoreply@gmail.com");

                if (!string.IsNullOrEmpty(email))
                {
                    message.To.Add(email);
                }
                else
                {
                    // If there is no email inputted then it will just return the view without trying to send the email
                    return View();
                }

                message.Subject = "SRS Appointment Edit Request Set!";
                message.Body = $"Hello,<br><br>" +
                    $"A patient just requested an appointment change for your appointment with them on {app.OrigDateTime} <br><br>" +
                    "Here are the requested details: <br><br>" +
                    $"Time: {app.DateTime}<br><br>" +
                    $"Location: {app.Location}<br><br>" +
                    $"Topic: {app.Topic}<br><br>" +
                    $"To edit the appointment, click <a href='https://localhost:7067/Appointments/ToEdit'>here</a> to go to the edit page and enter the information<br><br>"+
                    $"To learn more, click <a href='https://localhost:7067/Appointments'>here</a> to go to the appointments page<br>";

                message.IsBodyHtml = true;


                Console.WriteLine("------------------------------HERE------------------------------");

                using (var client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true; // Gmail requires SSL
                    client.UseDefaultCredentials = false;

                    client.Credentials = new NetworkCredential("cs310automatedemailnoreply@gmail.com", "rzzk ubqp pgda opju");


                    client.Send(message);
                }

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString() + " - This is because the email is in incorrect format.");
                return View(); // This exception should be caught if the email address inputted does not follow email guidelines
            }
        }

        public Appointment FromDisplay(AppointmentDisplay disp)
        {
            Appointment appointment = new Appointment
            {
                PatientId = disp.PatientId,
                StaffId = disp.StaffId,
                DateTime = disp.dateTime,
                Location = disp.Location,
                Topic = disp.Topic,
            };

            return appointment;
        }
    }
}
