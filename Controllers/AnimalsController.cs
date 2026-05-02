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
        public async Task<IActionResult> Index()
        {
            var animalsList = await _context.Animals.ToListAsync();
            return View(animalsList);
        }

        // Add animal 
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        // new animal data
        [HttpPost]
        public async Task<IActionResult> Add(Animal animal)
        {
            // get user id 
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

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
            }
            return RedirectToAction(nameof(Index));
        }
    }
}