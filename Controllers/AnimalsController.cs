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

        // 1. GET: Animals - عرض الجاليري مع ميزة البحث
        public async Task<IActionResult> Index(string searchType)
        {
            var animals = from a in _context.Animals select a;

            if (!string.IsNullOrEmpty(searchType))
            {
                animals = animals.Where(s => s.Type.Contains(searchType) || s.Name.Contains(searchType));
                ViewBag.CurrentFilter = searchType;
                ViewBag.SearchPerformed = true;
            }
            else
            {
                ViewBag.SearchPerformed = false;
            }

            return View(await animals.ToListAsync());
        }

        // 2. إضافة حيوان جديد (Add)
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
                else
                {
                    animal.ImgURL = "/images/default.png";
                }

                animal.OwnerId = int.Parse(userId);
                animal.IsAdopted = false;

                _context.Animals.Add(animal);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Pet added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(animal);
        }

        public async Task<IActionResult> ToggleAdoptionStatus(int id)
        {
            var animal = await _context.Animals.FindAsync(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (animal == null) return NotFound();

            if (userId != animal.OwnerId.ToString())
            {
                return RedirectToAction(nameof(Index));
            }

            animal.IsAdopted = !animal.IsAdopted;

            if (animal.IsAdopted)
            {
                animal.AdoptedDate = DateTime.Now;
            }
            else
            {
                animal.AdoptedDate = null;
            }

            _context.Update(animal);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index)); 
        }

        // 4. ميثود التواصل مع المالك (Contact Owner)
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

        // 5. حذف حيوان (Delete)
        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Delete(int id)
        {
            var animal = await _context.Animals.FindAsync(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (animal != null)
            {
                
                if (userId == animal.OwnerId.ToString())
                {
                    _context.Animals.Remove(animal);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Pet removed successfully.";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // 6. تعديل بيانات حيوان (Edit)
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var animal = await _context.Animals.FindAsync(id);
            if (animal == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != animal.OwnerId.ToString()) return Forbid();

            return View(animal);
        }


        private bool AnimalExists(int id)
        {
            return _context.Animals.Any(e => e.ID == id);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Description,Age,Type,ImgURL,OwnerId,IsAdopted,AdoptedDate")] Animal animal, IFormFile? ImageFile)
        {
            if (id != animal.ID) return NotFound();

            
            ModelState.Remove(nameof(ImageFile));
            ModelState.Remove(nameof(animal.Description));

            if (ModelState.IsValid)
            {
                try
                {
                    
                    var existingAnimal = await _context.Animals.AsNoTracking().FirstOrDefaultAsync(a => a.ID == id);
                    if (existingAnimal == null) return NotFound();

                    
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        string folder = "images/";
                        string serverFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder);

                        if (!Directory.Exists(serverFolder)) Directory.CreateDirectory(serverFolder);

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                        string filePath = Path.Combine(serverFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(fileStream);
                        }

                        
                        animal.ImgURL = "/" + folder + uniqueFileName;
                    }
                    else
                    {
                        
                        animal.ImgURL = existingAnimal.ImgURL;
                    }

                    
                    animal.OwnerId = existingAnimal.OwnerId;
                    if (animal.IsAdopted)
                    {
                        animal.AdoptedDate = existingAnimal.AdoptedDate ?? DateTime.Now;
                    }
                    else
                    {
                        animal.AdoptedDate = null;
                    }

                    _context.Update(animal);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Changes saved successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnimalExists(animal.ID)) return NotFound();
                    throw;
                }
            }

            
            return View(animal);
        }

       

        
    }
}