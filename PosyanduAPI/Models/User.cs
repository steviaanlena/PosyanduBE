namespace PosyanduAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string NIK { get; set; } = string.Empty;
        public string Nama { get; set; } = string.Empty;
        public string TTL { get; set; } = string.Empty;
        public string Alamat { get; set; } = string.Empty;
        public string NoTelp { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; } = new byte[0];
        public byte[] PasswordSalt { get; set; } = new byte[0];
        public string UserType { get; set; } = string.Empty; 
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}