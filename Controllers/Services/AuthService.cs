//using AdaPET.Helpers;
using AdaPET.Models;
using AdaPET.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AdaPET.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string ErrorMessage, User? User)> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                // 1. التحقق من عدم وجود البريد مسبقاً
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    return (false, "البريد الإلكتروني مسجل بالفعل", null);
                }

                // 2. تشفير كلمة المرور
              //  var hashedPassword = PasswordHasher.HashPassword(model.Password);

                // 3. إنشاء المستخدم
                var user = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    phone = model.Phone,
                    Password = model.Password,
                  //  Password = hashedPassword, // 👈 مخزنة بشكل مشفر
                    UserRole = model.UserRole
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // 4. معالجة بيانات الدكتور
                if (model.UserRole == "Doctor" && !string.IsNullOrEmpty(model.Specialization))
                {
                    var doctor = new Doctor
                    {
                        UserId = user.Id,
                        Name = user.Name,
                        Specialization = model.Specialization
                    };
                    _context.Doctors.Add(doctor);
                    await _context.SaveChangesAsync();

                    // 5. معالجة العيادات
                    if (model.Clinics != null && model.Clinics.Any())
                    {
                        foreach (var clinicVM in model.Clinics)
                        {
                            if (!string.IsNullOrEmpty(clinicVM.Name) && !string.IsNullOrEmpty(clinicVM.Address))
                            {
                                var clinic = new Clinic
                                {
                                    Name = clinicVM.Name,
                                    location = clinicVM.Address,
                                    DoctorId = doctor.UserId,
                                    Phone = user.phone
                                };
                                _context.Clinics.Add(clinic);
                            }
                        }
                        await _context.SaveChangesAsync();
                    }
                }

                return (true, string.Empty, user);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string ErrorMessage, User? User)> LoginAsync(LoginViewModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                return (false, "Email or password is incorrect", null);
            }

            // التحقق من كلمة المرور المشفرة
            //  bool isPasswordValid = PasswordHasher.VerifyPassword(user.Password, model.Password);

            /*/if (!isPasswordValid)
             {
                 return (false, "البريد الإلكتروني أو كلمة المرور غير صحيحة", null);
             }
            */
            return (true, string.Empty, user);
         
        }
    }
}