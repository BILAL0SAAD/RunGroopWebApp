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
        public ClubController(IClubRepository clubRepository, IPhotoService photoService)
        {

            _clubRepository = clubRepository;
            _photoService = photoService;

        }
        public async Task<IActionResult> Index()
        {
            var clubs = await _clubRepository.GetAll();
            return View(clubs);
        }


        public async Task<IActionResult> Detail(int id)
        {
            var club = await _clubRepository.GetByIdAsync(id);
            return View(club);

        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
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
                        ClubCategory = clubVM.ClubCategory,
                        Address = new Address
                        {
                            Street = clubVM.Address.Street,
                            City = clubVM.Address.City,
                            State = clubVM.Address.State,
                        }
                    };

                    _clubRepository.Add(club);
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
        [HttpPost]
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
                    await _photoService.DeletePhotoAsync(userClub.Image);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Could not delet photo");
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
                };

                _clubRepository.Update(club);

                return RedirectToAction("index");

            }
            else
            {
                return View(clubVM);
            }
        }
    }
   
}

