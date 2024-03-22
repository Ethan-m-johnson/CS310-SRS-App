using CS310_SRS_App.Model;
using CS310_SRS_App.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;


namespace CS310_SRS_App.Controllers
{
    public class UsersController : Controller
    {
        private readonly CS310SRSDatabaseContext _context;

        public UsersController(CS310SRSDatabaseContext context)
        {
            _context = context;
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string? Email)
        {
            Console.WriteLine("----------------------------------------------------------------" + Email);
            if (string.IsNullOrEmpty(Email))
            {
                // Email is null or empty; handle accordingly
                return View("ForgotPassword"); // Ensure you have a view for handling errors or re-input
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);
            if (user != null)
            {
                // Generate a secure token
                var token = GenerateToken();

                // Check if an existing token exists for the user
                var existingToken = await _context.ResetToken.FirstOrDefaultAsync(rt => rt.UserId == user.UserId);
                if (existingToken != null)
                {
                    // Update the existing token and expiry date
                    existingToken.Token = token;
                    existingToken.ExpiryDate = DateTime.UtcNow.AddMinutes(15);
                }
                else
                {
                    // Create a new ResetToken object since one doesn't exist
                    var resetToken = new ResetToken
                    {
                        Token = token,
                        ExpiryDate = DateTime.UtcNow.AddMinutes(15),
                        UserId = user.UserId
                    };

                    _context.ResetToken.Add(resetToken);
                }

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Prepare and send the password reset email
                var callbackUrl = Url.Action("ResetPassword", "Users", new { userId = user.UserId, token = token }, protocol: HttpContext.Request.Scheme);
                Console.WriteLine(callbackUrl); // Consider replacing this with a call to send the email
                SendPasswordResetEmail(Email, callbackUrl); // Ensure this method is async if it performs async operations

                // Redirect to a confirmation page
                return View("AccountInformation/ForgotPasswordConfirmation");
            }

            TempData["Message"] = "If an account with that email exists, a password reset link has been sent.";
            return View("AccountInformation/Login");
        }
        [HttpGet]
        public IActionResult ResetPassword(string token, string userId)
        {
            var model = new ResetPasswordViewModel { Token = token, UserId = userId };
            return View("AccountInformation/ResetPassword", model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Convert UserId from string to int
            if (!int.TryParse(model.UserId, out int userId))
            {
                // Handle the case where UserId is not a valid int
                ModelState.AddModelError("", "Invalid user ID.");
                return View(model);
            }

            // Use the converted int UserId to find the user
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !await VerifyToken(user, model.Token))
            {
                // Handle invalid user or token
                ModelState.AddModelError("", "Invalid token or user does not exist.");
                return View(model);
            }

            // Proceed with password reset
            user.Password = HashPassword(model.NewPassword); // Ensure you hash the password securely
            await _context.SaveChangesAsync();

            // Redirect to a confirmation page or login page
            return View("AccountInformation/ResetPasswordConfirmation");
        }
        private async Task<bool> VerifyToken(User user, string token)
        {
            // Retrieve the reset token entry from the database
            var resetToken = await _context.ResetToken
                .Where(rt => rt.Token == token && rt.UserId == user.UserId)
                .SingleOrDefaultAsync();

            // Check if the token exists and has not expired
            if (resetToken != null && resetToken.ExpiryDate > DateTime.UtcNow)
            {
                // Optionally, you might want to invalidate the token after verification
                // by removing it or setting a flag to prevent reuse
                _context.ResetToken.Remove(resetToken);
                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }
        public void SendPasswordResetEmail(string? Email, string callbackUrl)
        {
            try
            {
                var message = new MailMessage();
                message.From = new MailAddress("cs310automatedemailnoreply@gmail.com");

                if (!string.IsNullOrEmpty(Email))
                {
                    message.To.Add(Email);
                }
                else
                {
                    // Log or handle the case where there's no email
                    Console.WriteLine("No email provided for password reset.");
                    return;
                }

                message.Subject = "Password Reset Request";
                message.Body = $"Hello,<br><br>" +
                               "You have requested to reset your password. Please follow the link below to set a new password:<br><br>" +
                               $"<a href='{callbackUrl}'>Reset Password</a><br><br>" +
                               "If you did not request a password reset, please ignore this email or contact support.";

                message.IsBodyHtml = true;

                using (var client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true; // Gmail requires SSL
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential("cs310automatedemailnoreply@gmail.com", "rzzk ubqp pgda opju");
                    client.Send(message);
                }
            }
            catch (Exception ex)
            {
                // Log or handle the error more gracefully
                Console.WriteLine($"Failed to send password reset email: {ex.Message}");
            }
        }
        private string GenerateToken()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[32]; // Generate a 256-bit token
                rng.GetBytes(tokenData);
                return Convert.ToBase64String(tokenData);
            }
        }
        //Get Views/AccountInformation/InviteUsers
        [HttpGet]
        public IActionResult Login()
        {
            //var userRole = HttpContext.Session.GetString("SessionRole"); // Get user role from session
            //ViewBag.UserRole = userRole; // Pass user role to the view
            return View("AccountInformation/Login");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string? email, string? password)
        {
            Console.WriteLine("email: " + email);
            Console.WriteLine("password: " +  password);
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                // Handle missing email or password
                ViewBag.ErrorMessage = "Email and Password are required.";
                return RedirectToAction("Index", "Home");
            }

            var hashedPassword = HashPassword(password);
            


            var user = await _context.Users
                          .Where(u => u.Email.ToLower() == email.ToLower())
                          .FirstOrDefaultAsync();
            
            if (user != null && hashedPassword == user.Password)
            {
                HttpContext.Session.SetString("IsVerified", "true"); //Session variable
                HttpContext.Session.SetString("SessionKeyEmail", email);
                HttpContext.Session.SetString("SessionKeyID", user.UserId.ToString());


                // Determine the role of the user
                var role = await DetermineUserRole(user.UserId);
                HttpContext.Session.SetString("SessionKeyRole", role);

                Console.WriteLine($"IsVerified: {HttpContext.Session.GetString("IsVerified")}");
                Console.WriteLine($"SessionKeyEmail: {HttpContext.Session.GetString("SessionKeyEmail")}");
                Console.WriteLine($"SessionKeyID: {HttpContext.Session.GetString("SessionKeyID")}");
                var userRole = HttpContext.Session.GetString("SessionKeyRole");
                if (userRole == "Unknown")
                {
                    HttpContext.Session.Clear();
                    // Redirect to the login page or an error page
                    return RedirectToAction("Index", "Home");
                }

                Console.WriteLine($"IsVerified: {HttpContext.Session.GetString("IsVerified")}");
                Console.WriteLine($"SessionKeyEmail: {HttpContext.Session.GetString("SessionKeyEmail")}");
                Console.WriteLine($"SessionKeyID: {HttpContext.Session.GetString("SessionKeyID")}");


                return RedirectToAction("Index", "Home");
            }
            ViewBag.ErrorMessage = "Invalid login attempt.";
            return RedirectToAction("Index", "Home");
        }

        private async Task<string> DetermineUserRole(int userId)
        {
            // Check each table to see where the UserId exists as a foreign key
            var isAdmin = await _context.Admins.AnyAsync(a => a.UserId == userId);
            if (isAdmin) return "Admin";

            var isStaff = await _context.staff.AnyAsync(s => s.UserId == userId);
            if (isStaff) return "Staff";

            var isPatient = await _context.Patients.AnyAsync(p => p.UserId == userId);
            if (isPatient) return "Patient";

            return "Unknown"; 
        }


        //Get Views/AccountInformation/InviteUsers
        [HttpGet("users/accountinformation/inviteusers")]
        public IActionResult InviteUsers()
        {
            //var userRole = HttpContext.Session.GetString("SessionRole"); // Get user role from session
            //ViewBag.UserRole = userRole; // Pass user role to the view
            return View("AccountInformation/InviteUsers");
        }

        [HttpPost]
        public async Task<IActionResult> CreateAdmin(string? adminEmail)
        {
            if (string.IsNullOrWhiteSpace(adminEmail))
            {
                return RedirectToAction("Index");//Do this for now
                // Handle the case where adminEmail is null or empty
            }

            bool emailExists = await _context.Users.AnyAsync(u => u.Email == adminEmail);
            if (emailExists)
            {
                //return RedirectToAction("Index");//Do this for now
                // Handle where there is already an account with that email
            }


            string tempPassword = GeneratePassword();
            string hashedPassword = HashPassword(tempPassword);
            User newUser = new User
            {
                Email = adminEmail,
                Password = hashedPassword, //Convert hashed password to bytes to store in DB
                IsVerified = false,
                //accountActivated = false // Add this to the DB
            };

            _context.Add(newUser); //Add new email
            await _context.SaveChangesAsync();

           
            Admin newAdmin = new Admin
            {
                UserId = newUser.UserId // Connect the new user to the admin role
            };

            _context.Add(newAdmin); //Add user role
            await _context.SaveChangesAsync();

            SendInvitationEmail(adminEmail, tempPassword); //Implement later
            
            return RedirectToAction("Index"); //User will be able to see the new account entry 
        }


        [HttpPost]
        public async Task<IActionResult> CreateStaff(string? staffEmail, decimal staffSalary)
        {
            if (string.IsNullOrWhiteSpace(staffEmail) || staffSalary <= 0)
            {
                TempData["Error"] = "Email and/or Salary can't be blank.";
                return RedirectToAction("InviteUsers");
            }

            bool emailExists = await _context.Users.AnyAsync(u => u.Email == staffEmail);
            if (emailExists)
            {
                TempData["Error"] = "An account with this email already exists.";
                return RedirectToAction("InviteUsers");
            }


            string tempPassword = GeneratePassword();
            string hashedPassword = HashPassword(tempPassword); //Convert hashed password to store in DB
            User newUser = new User
            {
                Email = staffEmail,
                Password = hashedPassword, 
                IsVerified = false,
                //accountActivated = false // Add this to the DB
            };

            _context.Add(newUser); //Add new email & password
            await _context.SaveChangesAsync();


            staff newStaff = new staff
            {
                UserId = newUser.UserId // Connect the new user to the staff role
            };

            _context.Add(newStaff); //Add user role
            await _context.SaveChangesAsync();

            SendInvitationEmail(staffEmail, tempPassword); //Implement later

            return RedirectToAction("Index"); //User will be able to see the new account entry 
        }

        [HttpPost]
        public async Task<IActionResult> CreatePatient(string patientEmail)
        {
            if (string.IsNullOrWhiteSpace(patientEmail))
            {
                return RedirectToAction("Index");//Do this for now
                // Handle the case where adminEmail is null or empty
            }

            bool emailExists = await _context.Users.AnyAsync(u => u.Email == patientEmail);
            if (emailExists)
            {
                return RedirectToAction("Index");//Do this for now
                // Handle where there is already an account with that email
            }


            string tempPassword = GeneratePassword();
            string hashedPassword = HashPassword(tempPassword);
            User newUser = new User
            {
                Email = patientEmail,
                Password = hashedPassword,
                IsVerified = false,
                //accountActivated = false // Add this to the DB
            };

            _context.Add(newUser); //Add new email
            await _context.SaveChangesAsync();


            Patient newPatient = new Patient
            {
                UserId = newUser.UserId // Connect the new user to the Patient role
                //PrPhysicianId = HttpContext.Session.GetString("SessionUserID"); //When session variables are implemented we will pull the staff's id number that created the account to initially be set as the primary physician
            };

            _context.Add(newPatient); //Add user role
            await _context.SaveChangesAsync();

            SendInvitationEmail(patientEmail, tempPassword); //Call invitation email

            return RedirectToAction("Index"); //User will be able to see the new account entry 
        }

        public string GeneratePassword(int length = 12)
        {
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()?";
            var random = new Random();
            var chars = Enumerable.Range(0, length)
                                  .Select(x => validChars[random.Next(validChars.Length)]);
            return new string(chars.ToArray());
        }

        public string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        //Get Views/AccountInformation/InviteUsers
        [HttpGet("users/accountinformation/firstTimeUser")]
        public IActionResult FirstTimeUser()
        {
            return View("AccountInformation/FirstTimeUser");
        }


        [HttpPost]
        public async Task<IActionResult> ValidateFirstTimeUser(string email, string tempPassword)
        {


            string HashedPass = HashPassword(tempPassword);

            var user = await _context.Users.Where(s => s.Email == email && s.Password == HashedPass).FirstOrDefaultAsync();
            
            if (user != null)
            {
                ViewBag.IsValidUser = true;
                ViewBag.Email = email;
                ViewBag.UserId = user.UserId;
            }
            else
            {
                ViewBag.IsValidUser = false;
                ViewBag.ErrorMessage = "Invalid email or temporary password. Please try again.";
            }
            return View("AccountInformation/FirstTimeUser"); // Make sure to return the same view
        }

        public IActionResult SendInvitationEmail(string? Email, string? tempPassword)
        {
            try
            {
                var message = new MailMessage();
                message.From = new MailAddress("cs310automatedemailnoreply@gmail.com");

                if (!string.IsNullOrEmpty(Email))
                {
                    message.To.Add(Email);
                }
                else
                {
                    // If there is no email inputted then it will just return the view without trying to send the email
                    return View();
                }

                message.Subject = "SRS Account Creation Invitation";
                message.Body = $"Hello,<br><br>" +
                    "Welcome to SRS, please find the attached temporary password and navigate to the account activation page linked below.<br><br>" +
                    $"Temporary Password: {tempPassword}<br><br>" +
                    $"Account Activation Link: <a href='https://localhost:7067/users/accountinformation/FirstTimeUser'>Activate Account</a><br>";

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




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAccountInformation(int UserId, string Password, string FirstName, string MiddleName, string LastName, string Address, DateTime DateOfBirth, string Gender, string Phone)
        {
            if (ModelState.IsValid)
            {
                // Find the user by UserId
                var user = await _context.Users.FindAsync(UserId);
                if (user == null)
                {
                    // Handle the case where the user doesn't exist
                    return NotFound();
                }
                var hashedPassword = HashPassword(Password);
                // Update user's information
                user.Password = hashedPassword;
                user.FirstName = FirstName;
                user.MiddleName = MiddleName;
                user.LastName = LastName;
                user.Address = Address;
                user.DateOfBirth = DateOfBirth;
                user.Gender = Gender;
                user.Phone = Phone;
                user.IsVerified = true;

                // Since the Email field is disabled in the form, it won't be posted back.
                // Assuming Email doesn't change or is handled elsewhere.
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    // Redirect to a confirmation page, or wherever appropriate
                    return RedirectToAction("Index", "Home"); // Adjust redirection as needed
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    // Log the exception (ex) as needed
                    return StatusCode(500, "Internal server error");
                }
            }

            // If we got this far, something failed, redisplay form with existing data
            ViewBag.IsValidUser = true; // Ensure form remains visible
            ViewBag.UserId = UserId;
            return View("AccountInformation/FirstTimeUser"); // Adjust view name as needed
        }








        // GET: Users
        public async Task<IActionResult> Index()
        {
            return _context.Users != null ?
                        View(await _context.Users.ToListAsync()) :
                        Problem("Entity set 'CS310SRSDatabaseContext.Users'  is null.");
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,FirstName,MiddleName,LastName,Address,DateOfBirth,Gender,Email,Phone,Username,Password")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,FirstName,MiddleName,LastName,Address,DateOfBirth,Gender,Email,Phone,Username,Password")] User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'CS310SRSDatabaseContext.Users'  is null.");
            }
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
          return (_context.Users?.Any(e => e.UserId == id)).GetValueOrDefault();
        }
    }
}
