using System.ComponentModel.DataAnnotations;

namespace Assignment1.dto.request
{
    public class AccountCreatRequestDto
    {
        [Required(ErrorMessage = "Tên tài khoản không được để trống")]
        //[StringLength(100, MinimumLength = 3, ErrorMessage = "Tên tài khoản phải từ 3–100 ký tự")]
        public string AccountName { get; set; } = null!;

        [Required(ErrorMessage = "Email không được để trống")]
        //[EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(255)]
        public string AccountEmail { get; set; } = null!;

        [Required(ErrorMessage = "Role là bắt buộc")]
        [Range(1, 2, ErrorMessage = "Role không hợp lệ")]
        public int AccountRole { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        //[StringLength(32, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8–32 ký tự")]
        //[RegularExpression(
        //    @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).+$",
        //    ErrorMessage = "Mật khẩu phải có chữ hoa, chữ thường và số"
        //)]
        public string AccountPassword { get; set; } = null!;
    }
}
