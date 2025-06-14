namespace PosyanduAPI.DTOs
{
    public class UserRegisterDto
    {
        public string NIK { get; set; } = string.Empty;
        public string Nama { get; set; } = string.Empty;
        public string TTL { get; set; } = string.Empty; 
        public string Alamat { get; set; } = string.Empty; 
        public string NoTelp { get; set; } = string.Empty; 
        public string Password { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty; 
    }

    public class UserLoginDto
    {
        public string NIK { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UserResponseDto
    {
        public int Id { get; set; }
        public string NIK { get; set; } = string.Empty;
        public string Nama { get; set; } = string.Empty;
        public string TTL { get; set; } = string.Empty;
        public string Alamat { get; set; } = string.Empty;
        public string NoTelp { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}