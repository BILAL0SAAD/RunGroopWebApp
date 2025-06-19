using RunGroopWebApp.Data.Enum;
using RunGroopWebApp.Models;
using System.ComponentModel.DataAnnotations;

namespace RunGroopWebApp.ViewModels
{
    public class CreateRaceViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Race title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please select a race category")]
        public RaceCategory RaceCategory { get; set; }

        public Address Address { get; set; } = new Address(); // Initialize here

        public IFormFile? Image { get; set; } // Make optional

        public string? AppUserId { get; set; }
    }
}