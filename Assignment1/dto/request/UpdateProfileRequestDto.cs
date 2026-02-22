using System.ComponentModel.DataAnnotations;

namespace Assignment1.dto.request
{
    public class UpdateProfileRequestDto
    {
        [Required(ErrorMessage = "Tên không được để trống")]
        public string AccountName { get; set; } = null!;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string AccountEmail { get; set; } = null!;
    }
}
