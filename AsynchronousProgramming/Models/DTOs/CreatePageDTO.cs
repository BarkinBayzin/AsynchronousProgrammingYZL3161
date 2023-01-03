using System.ComponentModel.DataAnnotations;

namespace AsynchronousProgramming.Models.DTOs
{
    public class CreatePageDTO
    {
        [Required(ErrorMessage = "Please type into page Title")]
        [MinLength(3, ErrorMessage = "Minimum length is 3")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Please type into page Content")]
        [MinLength(3, ErrorMessage = "Minimum length is 3")]
        public string Content { get; set; }
        public string Slug => Title.ToLower().Replace(' ', '-');
    }
}
