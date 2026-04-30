using AdaPET.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdaPET.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user, string Specialization, string[] ClinicNames, string[] ClinicAddresses)
        {
            try
            {
                // للاختبار: نتأكد من البيانات اللي وصلت
                System.Diagnostics.Debug.WriteLine($"Name: {user.Name}");
                System.Diagnostics.Debug.WriteLine($"Email: {user.Email}");
                System.Diagnostics.Debug.WriteLine($"Phone: {user.phone}");
                System.Diagnostics.Debug.WriteLine($"Role: {user.UserRole}");
                System.Diagnostics.Debug.WriteLine($"Specialization: {Specialization}");

                // 1. حفظ اليوزر
                _context.Users.Add(user);
                int userResult = await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine($"User saved: {userResult} row(s), User ID: {user.Id}");

                // 2. لو دكتور، نحفظ بياناته
                if (user.UserRole == "Doctor" && !string.IsNullOrEmpty(Specialization))
                {
                    var doctor = new Doctor
                    {
                        UserId = user.Id,
                        Name = user.Name,
                        Specialization = Specialization
                    };
                    _context.Doctors.Add(doctor);
                    int doctorResult = await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"Doctor saved: {doctorResult} row(s)");

                    // 3. حفظ العيادات
                    if (ClinicNames != null && ClinicAddresses != null)
                    {
                        for (int i = 0; i < ClinicNames.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(ClinicNames[i]) && !string.IsNullOrEmpty(ClinicAddresses[i]))
                            {
                                var clinic = new Clinic
                                {
                                    Name = ClinicNames[i],
                                    location = ClinicAddresses[i],
                                    DoctorId = doctor.UserId,
                                    Phone = user.phone
                                };
                                _context.Clinics.Add(clinic);
                            }
                        }
                        int clinicsResult = await _context.SaveChangesAsync();
                        System.Diagnostics.Debug.WriteLine($"Clinics saved: {clinicsResult} row(s)");
                    }
                }

                TempData["Success"] = "Account created successfully!";
                return RedirectToAction("login", "Account");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"INNER ERROR: {ex.InnerException.Message}");
                }

                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View("Register", user);
            }
        }
        [HttpGet]
        public async Task<IActionResult> login()
        {
            return View();

        }
        [HttpPost]
        public async Task<IActionResult> login(string email, string password)
        {
            // 1. التحقق من أن الحقول ليست فارغة
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email and password are required.");
                return View(); // سيبقى في نفس الصفحة
            }

            // 2. البحث عن المستخدم
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            // 3. التحقق من صحة البيانات
            if (user != null && user.Password == password)
            {
                TempData["Success"] = "Login successful!";
                return RedirectToAction("homeFeed", "Home");
            }
            else
            {
                // 4. رسالة خطأ عامة
                ModelState.AddModelError("", "Invalid email or password.");

                // ✅ المهم هنا: إعادة القيم المدخلة للـ View مع رسالة الخطأ
                // سنستخدم ViewBag أو ViewData لتمرير الإيميل الذي أدخله المستخدم
                ViewBag.Email = email;
                return View();
            }
        }
    }
}
