using System.ComponentModel.DataAnnotations;

namespace Assignment1.dto.request
{
    public class UpdateAccountRequestDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên không được để trống")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên phải từ 2 đến 100 ký tự")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Email không được để trống")]
        //[EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(255)]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Role là bắt buộc")]
        [Range(1, 5, ErrorMessage = "Role không hợp lệ")]
        public int Role { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        //[StringLength(32, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8 đến 32 ký tự")]
        //[RegularExpression(
        //    @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).+$",
        //    ErrorMessage = "Mật khẩu phải có chữ hoa, chữ thường và số"
        //)]
        public string Password { get; set; } = null!;
    }
}
