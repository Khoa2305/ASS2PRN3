using System.ComponentModel.DataAnnotations;

namespace Assignment1.dto.request
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = null!;
    }
}
