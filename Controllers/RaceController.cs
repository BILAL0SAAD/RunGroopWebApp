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

        public RaceController(IRaceRepository raceRepository, IPhotoService photoService)
        {
            _raceRepository = raceRepository;
            _photoService = photoService;
        }

        public async Task<IActionResult> Index()
        {
            var races = await _raceRepository.GetAll();
            return View(races);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var race = await _raceRepository.GetByIdAsync(id);
            return View(race);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
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
                        RaceCategory = raceVM.RaceCategory,
                        Address = new Address
                        {
                            Street = raceVM.Address.Street,
                            City = raceVM.Address.City,
                            State = raceVM.Address.State,
                        }
                    };

                    _raceRepository.Add(race);
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
        public async Task<IActionResult> Edit(int id)
        {
            var race = await _raceRepository.GetByIdAsync(id);
            if (race == null) return View("Error");
            var RaceVM = new EditRaceViewModel
            {
                Title = race.Title,
                Description = race.Description,
                URL = race.Image,
                Address = race.Address,
                AddressId = race.AddressId,
                RaceCategory = race.RaceCategory,

            };
            return View(RaceVM);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditRaceViewModel RaceVM)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Failed to edit club");
                return View("Edit", RaceVM);
            }
            var userClub = await _raceRepository.GetByIdAsyncNoTracking(id);
            if (userClub != null)
            {
                try
                {
                    await _photoService.DeletePhotoAsync(userClub.Image);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Could not delet photo");
                    return View(RaceVM);
                }
                var photoResult = await _photoService.AddPhotoAsync(RaceVM.Image);

                var race = new Race
                {
                    Id = id,
                    Title = RaceVM.Title,
                    Description = RaceVM.Description,
                    Image = photoResult.Url.ToString(),
                    AddressId = RaceVM.AddressId,
                    Address = RaceVM.Address,
                };

                _raceRepository.Update(race);

                return RedirectToAction("index");

            }
            else
            {
                return View(RaceVM);
            }
        }
    }
}