using Microsoft.AspNetCore.Mvc;
using AdaPET.Models;
using Microsoft.EntityFrameworkCore;

namespace AdaPET.Controllers
{
    public class AnimalsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnimalsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // عرض قائمة جميع الحيوانات في الجاليري
        public async Task<IActionResult> Index()
        {
            var animalsList = await _context.Animals.ToListAsync();
            return View(animalsList);
        }

        // فتح صفحة إضافة حيوان جديد (GET)
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        // استقبال بيانات الحيوان الجديد وحفظ الصورة وربطه بصاحبه (POST)
        [HttpPost]
        public async Task<IActionResult> Add(Animal animal)
        {
            if (animal.ImageFile != null)
            {
                string folder = "images/";
                string serverFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder);
                if (!Directory.Exists(serverFolder)) Directory.CreateDirectory(serverFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + animal.ImageFile.FileName;
                string filePath = Path.Combine(serverFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await animal.ImageFile.CopyToAsync(fileStream);
                }
                animal.ImgURL = "/" + folder + uniqueFileName;
            }

            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId != null)
            {
                animal.OwnerId = userId.Value;
                _context.Animals.Add(animal);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return RedirectToAction("Login", "Account");
        }

        // عرض صفحة التواصل مع صاحب الحيوان وعرض بياناته
        public async Task<IActionResult> ContactOwner(int id)
        {
            var animal = await _context.Animals
                .Include(a => a.Owner)
                .FirstOrDefaultAsync(a => a.ID == id);

            if (animal == null)
            {
                return NotFound();
            }

            return View(animal);
        }

        // حذف حيوان من النظام بناءً على الـ ID الخاص به
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var animal = await _context.Animals.FindAsync(id);
            if (animal != null)
            {
                _context.Animals.Remove(animal);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // تبديل حالة الحيوان بين (متاح للتبني) أو (تم التبني)
        [HttpPost]
        public async Task<IActionResult> ToggleAdoption(int id)
        {
            var animal = await _context.Animals.FindAsync(id);
            if (animal != null)
            {
                animal.IsAdopted = !animal.IsAdopted;
                _context.Update(animal);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}