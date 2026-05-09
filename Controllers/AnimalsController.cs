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

        // الجاليري + البحث
        public async Task<IActionResult> Index(string searchType)
        {
            var animals = from a in _context.Animals select a;
            if (!string.IsNullOrEmpty(searchType))
            {
                animals = animals.Where(s => s.Type.Contains(searchType));
            }
            return View(await animals.ToListAsync());
        }

        // 2. إضافة حيوان جديد (Add) - GET & POST
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Animal animal)
        {
            

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

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
                else { animal.ImgURL = "/images/default.png"; }

                animal.OwnerId = int.Parse(userId);
                _context.Animals.Add(animal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(animal);
        }

        // 3. حذف حيوان (Delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var animal = await _context.Animals.FindAsync(id);
            if (animal != null)
            {
                _context.Animals.Remove(animal);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // 4. تعديل بيانات حيوان (Edit) - GET & POST
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var animal = await _context.Animals.FindAsync(id);
            if (animal == null) return NotFound();
            return View(animal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Animal animal)
        {
            if (id != animal.ID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingAnimal = await _context.Animals.AsNoTracking().FirstOrDefaultAsync(a => a.ID == id);
                    if (animal.ImageFile != null)
                    {
                        string folder = "images/";
                        string serverFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder);
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + animal.ImageFile.FileName;
                        string filePath = Path.Combine(serverFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await animal.ImageFile.CopyToAsync(fileStream);
                        }
                        animal.ImgURL = "/" + folder + uniqueFileName;
                    }
                    else { animal.ImgURL = existingAnimal.ImgURL; }

                    animal.OwnerId = existingAnimal.OwnerId;
                    _context.Update(animal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnimalExists(animal.ID)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(animal);
        }

        // 5. تفاصيل الحيوان (Details)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var animal = await _context.Animals.Include(a => a.Owner).FirstOrDefaultAsync(a => a.ID == id);
            if (animal == null) return NotFound();
            return View(animal);
        }

        private bool AnimalExists(int id)
        {
            return _context.Animals.Any(e => e.ID == id);
        }
    }
}