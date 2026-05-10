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

        // ✅ تغيير نوع الإرجاع ليشمل قائمة بالأخطاء
        public async Task<(bool Success, List<string> Errors, User? User)> RegisterAsync(RegisterViewModel model)
        {
            var errors = new List<string>();

            try
            {
                // ========== 1. التحقق من صحة البيانات الأساسية ==========

                // التحقق من الاسم
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    errors.Add("Name_Required: Full name is required");
                }
                else if (model.Name.Length < 3)
                {
                    errors.Add("Name_Length: Name must be at least 3 characters");
                }
                else if (model.Name.Length > 50)
                {
                    errors.Add("Name_Length: Name cannot exceed 50 characters");
                }

                // التحقق من الإيميل
                if (string.IsNullOrWhiteSpace(model.Email))
                {
                    errors.Add("Email_Required: Email is required");
                }
                else
                {
                    var emailRegex = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
                    if (!emailRegex.IsMatch(model.Email))
                    {
                        errors.Add("Email_Invalid: Please enter a valid email address");
                    }
                    else if (model.Email.Length > 100)
                    {
                        errors.Add("Email_Length: Email cannot exceed 100 characters");
                    }
                }

                // التحقق من رقم الهاتف
                if (string.IsNullOrWhiteSpace(model.Phone))
                {
                    errors.Add("Phone_Required: Phone number is required");
                }
                else
                {
                    var phoneRegex = new System.Text.RegularExpressions.Regex(@"^01[0125][0-9]{8}$");
                    if (!phoneRegex.IsMatch(model.Phone))
                    {
                        errors.Add("Phone_Invalid: Please enter a valid Egyptian phone number (e.g., 01012345678)");
                    }
                }

                // التحقق من كلمة المرور
                if (string.IsNullOrWhiteSpace(model.Password))
                {
                    errors.Add("Password_Required: Password is required");
                }
                else if (model.Password.Length < 6)
                {
                    errors.Add("Password_Length: Password must be at least 6 characters");
                }
                else if (model.Password.Length > 100)
                {
                    errors.Add("Password_Length: Password cannot exceed 100 characters");
                }

                // التحقق من تطابق كلمة المرور
                if (model.Password != model.ConfirmPassword)
                {
                    errors.Add("ConfirmPassword_Match: Passwords do not match");
                }

                // ========== 2. التحقق من وجود الإيميل في قاعدة البيانات ==========
                if (!string.IsNullOrWhiteSpace(model.Email))
                {
                    var existingUserByEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                    if (existingUserByEmail != null)
                    {
                        errors.Add("Email_Exists: This email is already registered. Please use a different email or login.");
                    }
                }

                // ========== 3. التحقق من وجود رقم الهاتف في قاعدة البيانات ==========
                if (!string.IsNullOrWhiteSpace(model.Phone))
                {
                    var existingUserByPhone = await _context.Users.FirstOrDefaultAsync(u => u.phone == model.Phone);
                    if (existingUserByPhone != null)
                    {
                        errors.Add("Phone_Exists: This phone number is already registered. Please use a different phone number or login.");
                    }
                }

                // إذا كان هناك أي أخطاء، ارجعها كلها
                if (errors.Any())
                {
                    return (false, errors, null);
                }

                // ========== 4. التحقق من بيانات الدكتور إذا كان الدور Doctor ==========
                if (model.UserRole == "Doctor")
                {
                    // التحقق من التخصص
                    if (string.IsNullOrWhiteSpace(model.Specialization))
                    {
                        errors.Add("Specialization_Required: Specialization is required for doctors");
                    }
                    else if (model.Specialization.Length < 3)
                    {
                        errors.Add("Specialization_Length: Specialization must be at least 3 characters");
                    }
                    else if (model.Specialization.Length > 100)
                    {
                        errors.Add("Specialization_Length: Specialization cannot exceed 100 characters");
                    }

                    // التحقق من العيادات
                    if (model.Clinics == null || !model.Clinics.Any())
                    {
                        errors.Add("Clinics_Required: At least one clinic is required for doctors");
                    }
                    else
                    {
                        for (int i = 0; i < model.Clinics.Count; i++)
                        {
                            var clinic = model.Clinics[i];
                            if (string.IsNullOrWhiteSpace(clinic.Name))
                            {
                                errors.Add($"Clinic_{i}_Name_Required: Clinic name is required");
                            }
                            else if (clinic.Name.Length < 3)
                            {
                                errors.Add($"Clinic_{i}_Name_Length: Clinic name must be at least 3 characters");
                            }
                            else if (clinic.Name.Length > 100)
                            {
                                errors.Add($"Clinic_{i}_Name_Length: Clinic name cannot exceed 100 characters");
                            }

                            if (string.IsNullOrWhiteSpace(clinic.Address))
                            {
                                errors.Add($"Clinic_{i}_Address_Required: Clinic address is required");
                            }
                            else if (clinic.Address.Length < 5)
                            {
                                errors.Add($"Clinic_{i}_Address_Length: Clinic address must be at least 5 characters");
                            }
                            else if (clinic.Address.Length > 200)
                            {
                                errors.Add($"Clinic_{i}_Address_Length: Clinic address cannot exceed 200 characters");
                            }
                        }
                    }

                    if (errors.Any())
                    {
                        return (false, errors, null);
                    }
                }

                // ========== 5. إنشاء المستخدم ==========
                var user = new User
                {
                    Name = model.Name.Trim(),
                    Email = model.Email.Trim().ToLower(),
                    phone = model.Phone.Trim(),
                    Password = model.Password,
                    UserRole = model.UserRole,
                    PhotoURL = "/images/default.png" 

                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // ========== 6. إنشاء بيانات الدكتور إذا كان الدور Doctor ==========
                if (model.UserRole == "Doctor" && !string.IsNullOrEmpty(model.Specialization))
                {
                    var doctor = new Doctor
                    {
                        UserId = user.Id,
                        Name = user.Name,
                        Specialization = model.Specialization.Trim()
                    };
                    _context.Doctors.Add(doctor);
                    await _context.SaveChangesAsync();

                    if (model.Clinics != null && model.Clinics.Any())
                    {
                        foreach (var clinicVM in model.Clinics)
                        {
                            if (!string.IsNullOrEmpty(clinicVM.Name) && !string.IsNullOrEmpty(clinicVM.Address))
                            {
                                var clinic = new Clinic
                                {
                                    Name = clinicVM.Name.Trim(),
                                    location = clinicVM.Address.Trim(),
                                    DoctorId = doctor.UserId,
                                    Phone = user.phone
                                };
                                _context.Clinics.Add(clinic);
                            }
                        }
                        await _context.SaveChangesAsync();
                    }
                }

                return (true, new List<string>(), user);
            }
            catch (DbUpdateException ex)
            {
                errors.Add($"Database_Error: Database error: {ex.InnerException?.Message ?? ex.Message}");
                return (false, errors, null);
            }
            catch (Exception ex)
            {
                errors.Add($"System_Error: An error occurred: {ex.Message}");
                return (false, errors, null);
            }
        }

        public async Task<(bool Success, string ErrorMessage, User? User)> LoginAsync(LoginViewModel model)
        {
            try
            {
                // التحقق من صحة الإيميل
                if (string.IsNullOrWhiteSpace(model.Email))
                {
                    return (false, "Email is required", null);
                }

                // التحقق من صحة كلمة المرور
                if (string.IsNullOrWhiteSpace(model.Password))
                {
                    return (false, "Password is required", null);
                }

                // البحث عن المستخدم في قاعدة البيانات
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email.Trim().ToLower());

                // ❌ حالة 1: الإيميل غير موجود في قاعدة البيانات
                if (user == null)
                {
                    return (false, "Email not found. Please sign up first or check your email.", null);
                }

                // ❌ حالة 2: الإيميل موجود ولكن كلمة المرور خاطئة
                if (user.Password != model.Password) // TODO: Use password hashing
                {
                    return (false, "Incorrect password. Please try again.", null);
                }

                // ✅ حالة 3: تسجيل الدخول ناجح
                return (true, string.Empty, user);
            }
            catch (Exception ex)
            {
                return (false, $"Login error: {ex.Message}", null);
            }
        }
    }
}