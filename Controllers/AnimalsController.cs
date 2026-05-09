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

        // GET: Animals - show all animals with search
        public async Task<IActionResult> Index(string searchType)
        {
            var animals = from a in _context.Animals
                          select a;

            if (!string.IsNullOrEmpty(searchType))
            {
                animals = animals.Where(s => s.Type.Contains(searchType));
                ViewBag.CurrentFilter = searchType;
                ViewBag.SearchPerformed = true;
            }
            else
            {
                ViewBag.SearchPerformed = false;
            }

            return View(await animals.ToListAsync());
        }

        // GET: Animals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animal = await _context.Animals
                .Include(a => a.Owner)
                .FirstOrDefaultAsync(a => a.ID == id);

            if (animal == null)
            {
                return NotFound();
            }

            return View(animal);
        }

        // GET: Animals/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animal = await _context.Animals
                .Include(a => a.Owner)
                .FirstOrDefaultAsync(a => a.ID == id);

            if (animal == null)
            {
                return NotFound();
            }

            // Check if current user owns this animal
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (animal.OwnerId.ToString() != userId && !User.IsInRole("Admin"))
            {
                TempData["Error"] = "You can only edit your own animals.";
                return RedirectToAction("Index", "Animals");
            }

            return View(animal);
        }

        // POST: Animals/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Type,Age,Description,ImageFile")] Animal animal)
        {
            if (id != animal.ID)
            {
                return NotFound();
            }

            var existingAnimal = await _context.Animals.FindAsync(id);
            if (existingAnimal == null)
            {
                return NotFound();
            }

            // Verify ownership
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (existingAnimal.OwnerId.ToString() != userId && !User.IsInRole("Admin"))
            {
                TempData["Error"] = "You can only edit your own animals.";
                return RedirectToAction("Index", "Animals");
            }

            // Remove validation for fields we don't want to validate
            ModelState.Remove("Owner");
            ModelState.Remove("OwnerId");
            ModelState.Remove("ImgURL");

            if (ModelState.IsValid)
            {
                try
                {
                    // Update animal properties
                    existingAnimal.Name = animal.Name;
                    existingAnimal.Type = animal.Type;
                    existingAnimal.Age = animal.Age;
                    existingAnimal.Description = animal.Description ?? string.Empty;

                    // Handle image update if new file is uploaded
                    if (animal.ImageFile != null && animal.ImageFile.Length > 0)
                    {
                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(existingAnimal.ImgURL))
                        {
                            string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                                existingAnimal.ImgURL.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Save new image
                        string folder = "images/";
                        string serverFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder);
                        if (!Directory.Exists(serverFolder))
                            Directory.CreateDirectory(serverFolder);

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(animal.ImageFile.FileName);
                        string filePath = Path.Combine(serverFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await animal.ImageFile.CopyToAsync(fileStream);
                        }
                        existingAnimal.ImgURL = "/" + folder + uniqueFileName;
                    }

                    // Mark as modified and save
                    _context.Entry(existingAnimal).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"{existingAnimal.Name}'s information has been updated!";
                    return RedirectToAction(nameof(Details), new { id = existingAnimal.ID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnimalExists(animal.ID))
                    {
                        return NotFound();
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error saving changes: {ex.Message}";
                    return View(animal);
                }
            }

            // If validation fails, return to form with errors
            return View(animal);
        }

        // GET: Animals/Add
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        // POST: Animals/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("Name,Type,Age,Description,ImageFile")] Animal animal)
        {
            // Get user id 
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Remove validation for fields we don't need to validate
            ModelState.Remove("Owner");
            ModelState.Remove("ImgURL");
            ModelState.Remove("AdoptedDate");

            if (ModelState.IsValid)
            {
                // Handle image upload
                if (animal.ImageFile != null && animal.ImageFile.Length > 0)
                {
                    string folder = "images/";
                    string serverFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder);
                    if (!Directory.Exists(serverFolder))
                        Directory.CreateDirectory(serverFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(animal.ImageFile.FileName);
                    string filePath = Path.Combine(serverFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await animal.ImageFile.CopyToAsync(fileStream);
                    }
                    animal.ImgURL = "/" + folder + uniqueFileName;
                }

                // Set default values
                animal.OwnerId = int.Parse(userId);
                animal.IsAdopted = false;
                animal.AdoptedDate = null;

                // Add to database
                _context.Animals.Add(animal);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"{animal.Name} has been added to your profile!";
                return RedirectToAction("Details", "UserProfile", new { id = userId });
            }

            // If validation fails, return to form with errors
            return View(animal);
        }

        // GET: Animals/ContactOwner/5
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

        // POST: Animals/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var animal = await _context.Animals.FindAsync(id);
            if (animal != null)
            {
                // Delete image file if exists
                if (!string.IsNullOrEmpty(animal.ImgURL))
                {
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                        animal.ImgURL.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Animals.Remove(animal);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Animal has been deleted successfully!";
            }
            return RedirectToAction("Index");
        }

        // POST: Animals/ToggleAdoption/5 - Redirects to Details page
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdoption(int id)
        {
            try
            {
                var animal = await _context.Animals.FindAsync(id);
                if (animal == null)
                {
                    TempData["Error"] = "Animal not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Toggle adoption status
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

                string status = animal.IsAdopted ? "adopted" : "marked as available for adoption";
                TempData["Success"] = $"{animal.Name} has been {status}!";

                return RedirectToAction(nameof(Details), new { id = animal.ID });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating adoption status: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // Check if animal exists
        private bool AnimalExists(int id)
        {
            return _context.Animals.Any(e => e.ID == id);
        }
    }
}