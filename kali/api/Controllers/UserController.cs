using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Models;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordHelper _passwordHelper;
        private readonly TokenService _tokenService;
        private readonly IWebHostEnvironment _environment;

        public UserController(
            IUserRepository userRepository,
            PasswordHelper passwordHelper,
            TokenService tokenService,
            IWebHostEnvironment environment)
        {
            _userRepository = userRepository;
            _passwordHelper = passwordHelper;
            _tokenService = tokenService;
            _environment = environment;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto registerDto)
        {
            if (_userRepository.GetByEmail(registerDto.Email) != null)
            {
                return BadRequest("Email already registered");
            }

            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = _passwordHelper.HashPassword(registerDto.Password),
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber ?? string.Empty,
                IsAdmin = registerDto.Email == "test@example.com" // Make this specific user an admin
            };

            _userRepository.Insert(user);

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            var user = _userRepository.GetByEmail(loginDto.Email);
            if (user == null)
            {
                return Unauthorized("Invalid email or password");
            }

            if (!_passwordHelper.VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid email or password");
            }

            var token = _tokenService.GenerateToken(user);
            return Ok(new { token });
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_userRepository.GetAll());
        }

        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            var user = _userRepository.GetById(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [Authorize]
        [HttpPut("{id}")]
        public IActionResult Update(string id, [FromBody] User user)
        {
            if (id != user.Id)
            {
                return BadRequest("ID mismatch");
            }

            var existingUser = _userRepository.GetById(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            if (!User.HasClaim(ClaimTypes.NameIdentifier, id) && !User.HasClaim(ClaimTypes.Role, "Admin"))
            {
                return Forbid();
            }

            if (_userRepository.Update(user))
            {
                return Ok(user);
            }

            return BadRequest("Update failed");
        }

        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var existingUser = _userRepository.GetById(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            if (!User.HasClaim(ClaimTypes.NameIdentifier, id) && !User.HasClaim(ClaimTypes.Role, "Admin"))
            {
                return Forbid();
            }

            if (_userRepository.Delete(id))
            {
                return NoContent();
            }

            return BadRequest("Delete failed");
        }

        [Authorize]
        [HttpPost("{id}/upload-profile-picture")]
        public async Task<IActionResult> UploadProfilePicture(string id, IFormFile file)
        {
            var user = _userRepository.GetById(id);
            if (user == null)
            {
                return NotFound();
            }

            if (!User.HasClaim(ClaimTypes.NameIdentifier, id) && !User.HasClaim(ClaimTypes.Role, "Admin"))
            {
                return Forbid();
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "User_images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Delete old profile picture if it exists
            if (!string.IsNullOrEmpty(user.ProfilePicturePath))
            {
                var oldFilePath = Path.Combine(_environment.WebRootPath, user.ProfilePicturePath);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{id}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            user.ProfilePicturePath = Path.Combine("User_images", fileName);
            _userRepository.Update(user);

            return Ok(new { user.ProfilePicturePath });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/make-admin")]
        public IActionResult MakeAdmin(string id)
        {
            var user = _userRepository.GetById(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsAdmin = true;
            if (_userRepository.Update(user))
            {
                return Ok(user);
            }

            return BadRequest("Update failed");
        }
    }
}