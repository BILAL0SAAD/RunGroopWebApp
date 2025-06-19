using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RunGroopWebApp.Data;
using RunGroopWebApp.Interfaces;
using RunGroopWebApp.Models;
using RunGroopWebApp.Repository;
using RunGroopWebApp.ViewModels;
using System.Threading.Tasks;

namespace RunGroopWebApp.Controllers
{
    public class RaceController : Controller
    {
        private readonly IRaceRepository _raceRepository;
        private readonly IPhotoService _photoService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RaceController(IRaceRepository raceRepository, IPhotoService photoService, IHttpContextAccessor httpcontextAccessor)
        {
            _raceRepository = raceRepository;
            _photoService = photoService;
            _httpContextAccessor = httpcontextAccessor;
        }

        // Public - everyone can view races
        public async Task<IActionResult> Index()
        {
            var races = await _raceRepository.GetAll();
            return View(races);
        }

        // Public - everyone can view race details
        public async Task<IActionResult> Detail(int id)
        {
            var race = await _raceRepository.GetByIdAsync(id);
            return View(race);
        }

        // ADMIN ONLY - Create Race GET
        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            var curUserID = _httpContextAccessor.HttpContext.User.GetUserId();
            var createRaceViewModel = new CreateRaceViewModel { AppUserId = curUserID };
            return View(createRaceViewModel);
        }

        // ADMIN ONLY - Create Race POST
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create(CreateRaceViewModel raceVM)
        {
            Console.WriteLine($"Form submitted - Title: {raceVM.Title}");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _photoService.AddPhotoAsync(raceVM.Image);
                    Console.WriteLine($"Photo uploaded: {result.Url}");

                    var race = new Race
                    {
                        Title = raceVM.Title,
                        Description = raceVM.Description,
                        Image = result.Url.ToString(),
                        AppUserId = raceVM.AppUserId,
                        RaceCategory = raceVM.RaceCategory,
                        Address = new Address
                        {
                            Street = raceVM.Address.Street,
                            City = raceVM.Address.City,
                            State = raceVM.Address.State,
                        }
                    };

                    _raceRepository.Add(race);
                    TempData["Success"] = "Race created successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    ModelState.AddModelError("", $"Error creating race: {ex.Message}");
                }
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation error: {error.ErrorMessage}");
                }
            }

            return View(raceVM);
        }

        // ADMIN ONLY - Edit Race GET
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var race = await _raceRepository.GetByIdAsync(id);
            if (race == null) return View("Error");

            var raceVM = new EditRaceViewModel
            {
                Title = race.Title,
                Description = race.Description,
                URL = race.Image,
                Address = race.Address,
                AddressId = race.AddressId,
                RaceCategory = race.RaceCategory,
            };
            return View(raceVM);
        }

        // ADMIN ONLY - Edit Race POST
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id, EditRaceViewModel raceVM)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Failed to edit race");
                return View("Edit", raceVM);
            }

            var userRace = await _raceRepository.GetByIdAsyncNoTracking(id);
            if (userRace != null)
            {
                try
                {
                    // Delete old photo if it exists
                    if (!string.IsNullOrEmpty(userRace.Image))
                    {
                        await _photoService.DeletePhotoAsync(userRace.Image);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Could not delete photo");
                    return View(raceVM);
                }

                var photoResult = await _photoService.AddPhotoAsync(raceVM.Image);

                var race = new Race
                {
                    Id = id,
                    Title = raceVM.Title,
                    Description = raceVM.Description,
                    Image = photoResult.Url.ToString(),
                    AddressId = raceVM.AddressId,
                    Address = raceVM.Address,
                    RaceCategory = raceVM.RaceCategory
                };

                _raceRepository.Update(race);
                TempData["Success"] = "Race updated successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                return View(raceVM);
            }
        }

        // ADMIN ONLY - Delete Race
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var race = await _raceRepository.GetByIdAsync(id);
            if (race == null)
            {
                TempData["Error"] = "Race not found.";
                return RedirectToAction("Index");
            }

            try
            {
                // Delete the image from cloud storage
                if (!string.IsNullOrEmpty(race.Image))
                {
                    await _photoService.DeletePhotoAsync(race.Image);
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the deletion
                Console.WriteLine($"Error deleting photo: {ex.Message}");
            }

            // Delete the race from database
            _raceRepository.Delete(race);
            TempData["Success"] = "Race deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}