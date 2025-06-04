using LKS_Mart_API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LKS_Mart_API.Controllers
{
    [Route("api")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly MartDbContext context;
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment env;

        public UserController(MartDbContext context, IConfiguration configuration, IWebHostEnvironment env)
        {
            this.context = context;
            this.configuration = configuration;
            this.env = env;
        }

        public record class LoginRequest
        {
            [Required]
            public string Username { get; set; } = null!;

            [Required, MinLength(4)]
            public string Password { get; set; } = null!;
        }

        private string GenerateToken(User user)
        {
            var config = configuration.GetSection("JWT");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Key"]!));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Tipe.ToString()),
            };
            var token = new JwtSecurityToken(
                issuer: config["Issuer"],
                audience: config["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: cred
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("auth")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            try
            {
                var user = await context.Users.FirstOrDefaultAsync(x => x.Username == request.Username && x.Password == request.Password && x.Tipe == "pelanggan");
                if (user is null)
                {
                    return Unauthorized("Invalid Credentials");
                }
                return Ok(GenerateToken(user));
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        public record class RegisterRequest
        {
            [Required]
            public string FullName { get; set; } = null!;

            [Required]
            public string Username { get; set; } = null!;

            [Required]
            public string Address { get; set; } = null!;

            [Required, Phone]
            public string Phone { get; set; } = null!;

            [Required, MinLength(4)]
            public string Password { get; set; } = null!;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            try
            {
                var user = await context.Users.FirstOrDefaultAsync(x => x.Username == request.Username || x.Telepon == request.Phone);
                if (user is not null)
                {
                    if (user.Username == request.Username)
                    {
                        return Unauthorized("Username already taken");
                    }
                    else
                    {
                        return Unauthorized("Phone number already taken");
                    }
                }
                user = new User
                {
                    Tipe = "pelanggan",
                    Nama = request.FullName,
                    Alamat = request.Address,
                    Username = request.Username,
                    Password = request.Password,
                    Telepon = request.Phone,
                };
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();
                return Ok(GenerateToken(user));
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [HttpGet("me"), Authorize(Roles = ("pelanggan"))]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier)!.ToString());
                if (userId <= 0) return Problem("User ID can't be or less than 0");
                var user = await context.Users.FindAsync(userId);
                if (user is null) return NotFound("User not found");
                return Ok(user);
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        private async Task<string> UploadProfileImage(IFormFile file)
        {
            try
            {
                var validExt = new[] { ".png", ".jpg", ".jpeg" };
                const long maxSize = 1048576;
                var fileExt = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!validExt.Contains(fileExt)) return "File type is not valid";
                if (file.Length > maxSize) return "File max size is 1MB";
                var filePath = Path.Combine(env.WebRootPath, "users");
                if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
                await stream.DisposeAsync();
                return $"users/{file.FileName}";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        [HttpPost("upload-profile"), Authorize(Roles = ("pelanggan"))]
        public async Task<IActionResult> UploadProfile(IFormFile image)
        {
            try
            {
                if (image is null) return BadRequest("Image file is needed!");
                var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier)!.ToString());
                if (userId <= 0) return Problem("User ID can't be or less than 0");
                var user = await context.Users.FindAsync(userId);
                if (user is null) return NotFound("User not found");
                var imagePath = await UploadProfileImage(image);
                if (!imagePath.Contains('/'))
                {
                    return BadRequest(imagePath);
                }
                user.Image = imagePath;
                context.Users.Update(user);
                await context.SaveChangesAsync();
                return Ok(user);
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [HttpGet("products")]
        public async Task<IActionResult> Products(string? search)
        {
            try
            {
                var data = await context.Barangs.ToListAsync();
                if (!string.IsNullOrEmpty(search))
                {
                    data = data.Where(x => x.Nama.Contains(search, StringComparison.InvariantCultureIgnoreCase) ||
                                      x.Kode.Contains(search, StringComparison.InvariantCultureIgnoreCase)).ToList();
                }
                return Ok(data);
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        public record class CheckoutItem
        {
            [Required, Range(1, int.MaxValue)]
            public int ProductId { get; set; }

            [Required, Range(1, int.MaxValue)]
            public int Qty { get; set; }
        }

        [HttpPost("checkout"), Authorize(Roles = ("pelanggan"))]
        public async Task<IActionResult> Checkout([FromBody] List<CheckoutItem> request)
        {
            try
            {
                if (request is null || request.Count == 0) return BadRequest("Item can't be null or empty");
                var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier)!.ToString());
                if (userId <= 0) return Problem("User ID can't be or less than 0");
                var user = await context.Users.FindAsync(userId);
                if (user is null) return NotFound("User not found");
                var customer = await context.Pelanggans.FirstOrDefaultAsync(x => x.Telepon == user.Telepon);
                if (customer is null)
                {
                    customer = new Pelanggan
                    {
                        Nama = user.Nama,
                        Telepon = user.Telepon,
                    };
                    await context.Pelanggans.AddAsync(customer);
                    await context.SaveChangesAsync();
                }
                var transactions = await context.Transaksis.GroupBy(x => x.No).ToListAsync();
                var code = $"TR00{transactions.Count + 1}";
                var date = DateOnly.FromDateTime(DateTime.Now);
                foreach (var item in request)
                {
                    var product = await context.Barangs.FindAsync(item.ProductId);
                    if (product is null) return NotFound("Product not found");
                    if (product.Jumlah < item.Qty) return BadRequest($"{product.Nama} only have ${product.Jumlah} left in stock");
                    var transaction = new Transaksi
                    {
                        No = code,
                        Tanggal = date,
                        TotalBayar = item.Qty * product.HargaSatuan,
                        UserId = userId,
                        PelangganId = customer.Id,
                        BarangId = product.Id,
                    };
                    product.Jumlah -= item.Qty;
                    context.Barangs.Update(product);
                    await context.Transaksis.AddAsync(transaction);
                }
                await context.SaveChangesAsync();
                return Ok("Success");
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }
    }
}
