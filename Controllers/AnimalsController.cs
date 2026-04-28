
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

        
        public async Task<IActionResult> Index()
        {
            var animalsList = await _context.Animals.ToListAsync();
            return View(animalsList);
        }

        
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        
        [HttpPost]
        public async Task<IActionResult> Add(Animal animal)
        {
            

             _context.Animals.Add(animal);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            

            
            return View(animal);
        
        }
    }
}