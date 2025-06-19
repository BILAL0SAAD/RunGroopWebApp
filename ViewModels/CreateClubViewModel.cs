using RunGroopWebApp.Data.Enum;
using RunGroopWebApp.Models;
using System.ComponentModel.DataAnnotations;

namespace RunGroopWebApp.ViewModels
{
    public class CreateClubViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Club name is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please select a category")]
        public ClubCategory ClubCategory { get; set; }

        public Address Address { get; set; } = new Address(); // Initialize here

        public IFormFile? Image { get; set; } // Make optional

        public string? AppUserId { get; set; }
    }
}