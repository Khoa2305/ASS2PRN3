using System.ComponentModel.DataAnnotations;

namespace Assignment1.dto.request
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        //[EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password là bắt buộc")]
        //[StringLength(50, MinimumLength = 6,
        //    ErrorMessage = "Password phải từ 6 đến 50 ký tự")]
        public string Password { get; set; } = null!;
    }
}
