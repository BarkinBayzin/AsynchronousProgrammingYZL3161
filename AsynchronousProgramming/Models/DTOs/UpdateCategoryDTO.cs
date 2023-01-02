using System.ComponentModel.DataAnnotations;

namespace AsynchronousProgramming.Models.DTOs
{
    public class UpdateCategoryDTO
    {
        public int Id { get; set; }
        [Display(Name = "Category Name")]
        [Required(ErrorMessage = "Must to the type into a name")]
        [MinLength(3, ErrorMessage = "Minimum length is 3")]
        [RegularExpression(@"^[a-zA-Z- ]+$", ErrorMessage = "Only allowed letters")]
        public string Name { get; set; }
        public string Slug => Name.ToLower().Replace(' ', '-');
    }
}
