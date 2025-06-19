using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RunGroopWebApp.Data;
using Microsoft.EntityFrameworkCore;
using RunGroopWebApp.Models;
using RunGroopWebApp.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RunGroopWebApp.ViewModels;

namespace RunGroopWebApp.Controllers
{
    public class ClubController : Controller
    {
        private readonly IClubRepository _clubRepository;
        private readonly IPhotoService _photoService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClubController(IClubRepository clubRepository, IPhotoService photoService, IHttpContextAccessor httpContextAccessor)
        {
            _clubRepository = clubRepository;
            _photoService = photoService;
            _httpContextAccessor = httpContextAccessor;
        }

        // Public - everyone can view clubs
        public async Task<IActionResult> Index()
        {
            var clubs = await _clubRepository.GetAll();
            return View(clubs);
        }

        // Public - everyone can view club details
        public async Task<IActionResult> Detail(int id)
        {
            var club = await _clubRepository.GetByIdAsync(id);
            return View(club);
        }

        // ADMIN ONLY - Create Club GET
        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            var curUserID = _httpContextAccessor.HttpContext.User.GetUserId();
            var createClubViewModel = new CreateClubViewModel { AppUserId = curUserID };
            return View(createClubViewModel);
        }

        // ADMIN ONLY - Create Club POST
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create(CreateClubViewModel clubVM)
        {
            Console.WriteLine($"Form submitted - Title: {clubVM.Title}");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _photoService.AddPhotoAsync(clubVM.Image);
                    Console.WriteLine($"Photo uploaded: {result.Url}");

                    var club = new Club
                    {
                        Title = clubVM.Title,
                        Description = clubVM.Description,
                        Image = result.Url.ToString(),
                        AppUserId = clubVM.AppUserId,
                        ClubCategory = clubVM.ClubCategory,
                        Address = new Address
                        {
                            Street = clubVM.Address.Street,
                            City = clubVM.Address.City,
                            State = clubVM.Address.State,
                        }
                    };

                    _clubRepository.Add(club);
                    TempData["Success"] = "Club created successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    ModelState.AddModelError("", $"Error creating club: {ex.Message}");
                }
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation error: {error.ErrorMessage}");
                }
            }
            return View(clubVM);
        }

        // ADMIN ONLY - Edit Club GET
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var club = await _clubRepository.GetByIdAsync(id);
            if (club == null) return View("Error");

            var clubVM = new EditClubViewModel
            {
                Title = club.Title,
                Description = club.Description,
                URL = club.Image,
                Address = club.Address,
                AddressId = club.AddressId,
                ClubCategory = club.ClubCategory,
            };
            return View(clubVM);
        }

        // ADMIN ONLY - Edit Club POST
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id, EditClubViewModel clubVM)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Failed to edit club");
                return View("Edit", clubVM);
            }

            var userClub = await _clubRepository.GetByIdAsyncNoTracking(id);
            if (userClub != null)
            {
                try
                {
                    // Delete old photo if it exists
                    if (!string.IsNullOrEmpty(userClub.Image))
                    {
                        await _photoService.DeletePhotoAsync(userClub.Image);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Could not delete photo");
                    return View(clubVM);
                }

                var photoResult = await _photoService.AddPhotoAsync(clubVM.Image);

                var club = new Club
                {
                    Id = id,
                    Title = clubVM.Title,
                    Description = clubVM.Description,
                    Image = photoResult.Url.ToString(),
                    AddressId = clubVM.AddressId,
                    Address = clubVM.Address,
                    ClubCategory = clubVM.ClubCategory
                };

                _clubRepository.Update(club);
                TempData["Success"] = "Club updated successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                return View(clubVM);
            }
        }

        // ADMIN ONLY - Delete Club
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var club = await _clubRepository.GetByIdAsync(id);
            if (club == null)
            {
                TempData["Error"] = "Club not found.";
                return RedirectToAction("Index");
            }

            try
            {
                // Delete the image from cloud storage
                if (!string.IsNullOrEmpty(club.Image))
                {
                    await _photoService.DeletePhotoAsync(club.Image);
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the deletion
                Console.WriteLine($"Error deleting photo: {ex.Message}");
            }

            // Delete the club from database
            _clubRepository.Delete(club);
            TempData["Success"] = "Club deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}