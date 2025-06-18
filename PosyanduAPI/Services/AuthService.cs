using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PosyanduAPI.Data;
using PosyanduAPI.DTOs;
using PosyanduAPI.Models;

namespace PosyanduAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly PosyanduContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(PosyanduContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<ServiceResponse<UserResponseDto>> Register(UserRegisterDto userRegisterDto)
        {
            var serviceResponse = new ServiceResponse<UserResponseDto>();

            // Validate input fields
            var validationResult = ValidateRegistrationInput(userRegisterDto);
            if (!validationResult.IsValid)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = validationResult.ErrorMessage;
                return serviceResponse;
            }

            if (IsUserExist(userRegisterDto.NIK))
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "User already exists.";
                return serviceResponse;
            }

            // Validate UserType
            if (userRegisterDto.UserType != "Kader" && userRegisterDto.UserType != "Ortu")
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Invalid user type. Must be 'Kader' or 'Ortu'.";
                return serviceResponse;
            }

            CreatePasswordHash(userRegisterDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                NIK = userRegisterDto.NIK,
                Nama = userRegisterDto.Nama,
                TTL = userRegisterDto.TTL,
                Alamat = userRegisterDto.Alamat,
                NoTelp = userRegisterDto.NoTelp,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                UserType = userRegisterDto.UserType
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            serviceResponse.Data = new UserResponseDto
            {
                Id = user.Id,
                NIK = user.NIK,
                Nama = user.Nama,
                TTL = user.TTL,
                Alamat = user.Alamat,
                NoTelp = user.NoTelp,
                UserType = user.UserType,
                Token = CreateToken(user)
            };

            return serviceResponse;
        }

        public async Task<ServiceResponse<UserResponseDto>> Login(UserLoginDto userLoginDto, string userType)
        {
            var serviceResponse = new ServiceResponse<UserResponseDto>();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.NIK == userLoginDto.NIK && u.UserType == userType);

            if (user == null)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "User not found.";
                return serviceResponse;
            }

            if (!VerifyPasswordHash(userLoginDto.Password, user.PasswordHash, user.PasswordSalt))
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Wrong password.";
                return serviceResponse;
            }

            serviceResponse.Data = new UserResponseDto
            {
                Id = user.Id,
                NIK = user.NIK,
                Nama = user.Nama,
                TTL = user.TTL,
                Alamat = user.Alamat,
                NoTelp = user.NoTelp,
                UserType = user.UserType,
                Token = CreateToken(user)
            };

            return serviceResponse;
        }

        public bool IsUserExist(string nik)
        {
            return _context.Users.Any(u => u.NIK == nik);
        }

        private ValidationResult ValidateRegistrationInput(UserRegisterDto userRegisterDto)
        {
            // Check if all required fields are provided
            if (string.IsNullOrWhiteSpace(userRegisterDto.NIK) ||
                string.IsNullOrWhiteSpace(userRegisterDto.Nama) ||
                string.IsNullOrWhiteSpace(userRegisterDto.TTL) ||
                string.IsNullOrWhiteSpace(userRegisterDto.Alamat) ||
                string.IsNullOrWhiteSpace(userRegisterDto.NoTelp) ||
                string.IsNullOrWhiteSpace(userRegisterDto.Password))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Silakan isi semua bidang" };
            }

            // NIK validation (16 digits)
            if (!Regex.IsMatch(userRegisterDto.NIK, @"^\d{16}$"))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "NIK harus berupa 16 digit angka" };
            }

            // Nama validation (only letters and spaces, 2-50 characters)
            if (!Regex.IsMatch(userRegisterDto.Nama.Trim(), @"^[a-zA-Z\s]{2,50}$"))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Nama hanya boleh mengandung huruf dan spasi (2-50 karakter)" };
            }

            // TTL validation (format: "City, DD-MM-YYYY")
            if (!Regex.IsMatch(userRegisterDto.TTL.Trim(), @"^[a-zA-Z\s]+,\s*\d{2}-\d{2}-\d{4}$"))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Format TTL tidak valid. Contoh: Jakarta, 15-01-1990" };
            }

            // Age validation from TTL
            var ageValidation = ValidateAge(userRegisterDto.TTL);
            if (!ageValidation.IsValid)
            {
                return ageValidation;
            }

            // Alamat validation (minimum 10 characters)
            if (userRegisterDto.Alamat.Trim().Length < 10)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Alamat harus minimal 10 karakter" };
            }

            // NoTelp validation (Indonesian phone number format)
            var cleanedPhone = userRegisterDto.NoTelp.Replace(" ", "").Replace("-", "");
            if (!Regex.IsMatch(cleanedPhone, @"^(\+62|62|0)8[1-9][0-9]{6,9}$"))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Nomor telepon tidak valid. Gunakan format Indonesia (contoh: 08123456789)" };
            }

            // Password validation (minimum 8 characters, at least 1 uppercase, 1 lowercase, 1 number)
            if (!Regex.IsMatch(userRegisterDto.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d@$!%*?&]{8,}$"))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Password minimal 8 karakter, harus mengandung huruf besar, huruf kecil, dan angka" };
            }

            return new ValidationResult { IsValid = true };
        }

        private ValidationResult ValidateAge(string ttl)
        {
            try
            {
                var dateMatch = Regex.Match(ttl, @"(\d{2})-(\d{2})-(\d{4})");
                if (dateMatch.Success)
                {
                    int day = int.Parse(dateMatch.Groups[1].Value);
                    int month = int.Parse(dateMatch.Groups[2].Value);
                    int year = int.Parse(dateMatch.Groups[3].Value);

                    var birthDate = new DateTime(year, month, day);
                    var today = DateTime.Now;
                    int age = today.Year - birthDate.Year;

                    if (today < birthDate.AddYears(age))
                        age--;

                    if (age < 17)
                    {
                        return new ValidationResult { IsValid = false, ErrorMessage = "Usia minimal 17 tahun untuk mendaftar" };
                    }

                    if (age > 100)
                    {
                        return new ValidationResult { IsValid = false, ErrorMessage = "Tanggal lahir tidak valid" };
                    }
                }
            }
            catch
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Format tanggal lahir tidak valid" };
            }

            return new ValidationResult { IsValid = true };
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.NIK),
                new Claim(ClaimTypes.Role, user.UserType)
            };

            // Ensure the key is at least 64 bytes (512 bits) for HMAC-SHA512
            string tokenSecret = _configuration.GetSection("AppSettings:Token").Value!;
            byte[] keyBytes;

            // If the configured token secret is too short, pad it to meet the minimum requirement
            if (Encoding.UTF8.GetByteCount(tokenSecret) < 64)
            {
                // Pad the token to ensure it's at least 64 bytes
                keyBytes = new byte[64];
                byte[] originalBytes = Encoding.UTF8.GetBytes(tokenSecret);
                Array.Copy(originalBytes, keyBytes, Math.Min(originalBytes.Length, 64));
            }
            else
            {
                keyBytes = Encoding.UTF8.GetBytes(tokenSecret);
            }

            var key = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}