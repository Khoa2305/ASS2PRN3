namespace UI.dto.response
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = null!;
        public int Role { get; set; }
    }
}
