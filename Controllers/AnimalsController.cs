using Microsoft.AspNetCore.Mvc;
using AdaPET.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AdaPET.Controllers
{
    public class AnimalsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnimalsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // show all animals
        public async Task<IActionResult> Index(string searchType)
        {
            
            var animals = from a in _context.Animals
                          select a;

            if (!string.IsNullOrEmpty(searchType))
            {
                animals = animals.Where(s => s.Type.Contains(searchType));
            }

            return View(await animals.ToListAsync());
        }

        // Add animal 
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        // new animal data
        [HttpPost]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Animal animal)
        {
            
            if (!ModelState.IsValid)
            {
                
                return View(animal);
            }

           
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            
            if (animal.ImageFile != null)
            {
                string folder = "images/";
                string serverFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder);

                if (!Directory.Exists(serverFolder))
                    Directory.CreateDirectory(serverFolder);

                // بنعمل اسم فريد للصورة عشان لو يوزر تاني رفع صورة بنفس الاسم ميمسحوش بعض
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + animal.ImageFile.FileName;
                string filePath = Path.Combine(serverFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await animal.ImageFile.CopyToAsync(fileStream);
                }

                // بنخزن المسار اللي هيتحفظ في الداتابيز عشان نعرف نعرضها في الـ View
                animal.ImgURL = "/" + folder + uniqueFileName;
            }

            // 4. ربط الحيوان بصاحبه وحفظ البيانات
            animal.OwnerId = int.Parse(userId);
            _context.Animals.Add(animal);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // View owner
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

        // delete animal
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

        //adapted or not 
        [HttpPost]
        public async Task<IActionResult> ToggleAdoption(int id)
        {
            var animal = await _context.Animals.FindAsync(id);
            if (animal != null)
            {
                // بنعكس الحالة بس: لو true تبقى false والعكس
                animal.IsAdopted = !animal.IsAdopted;

                _context.Update(animal);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}