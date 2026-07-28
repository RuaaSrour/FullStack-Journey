using Assig1.Data;
using Assig1.DTOs;
using Assig1.Models;
using Assig1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assig1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;
        private readonly PasswordHasher<UserAccount> _passwordHasher = new();

        public AuthController(
            AppDbContext context,
            TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [AllowAnonymous]
        [HttpPost("register/student")]
        public async Task<ActionResult<UserAccountDto>> RegisterStudent(
            RegisterStudentDto registerDto)
        {
            string username = registerDto.Username.Trim();

            bool usernameTaken = await _context.Users
                .AnyAsync(u => u.Username == username);

            if (usernameTaken)
            {
                return BadRequest("Username is already taken.");
            }

            Student? student = await _context.Students
                .FindAsync(registerDto.StudentId);

            if (student == null)
            {
                return BadRequest(
                    "No student was found with the given StudentId.");
            }

            bool studentAlreadyLinked = await _context.Users
                .AnyAsync(u => u.StudentId == registerDto.StudentId);

            if (studentAlreadyLinked)
            {
                return BadRequest(
                    "This student already has an account.");
            }

            UserAccount user = new()
            {
                Username = username,
                Role = UserRole.Student,
                StudentId = registerDto.StudentId,
                TeacherId = null
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                registerDto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            UserAccountDto response = new()
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role,
                StudentId = user.StudentId,
                TeacherId = user.TeacherId
            };

            return StatusCode(
                StatusCodes.Status201Created,
                response);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(
            LoginDto loginDto)
        {
            string username = loginDto.Username.Trim();

            UserAccount? user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return Unauthorized(
                    "Invalid username or password.");
            }

            PasswordVerificationResult result =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    loginDto.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized(
                    "Invalid username or password.");
            }

            if (result ==
                PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash =
                    _passwordHasher.HashPassword(
                        user,
                        loginDto.Password);

                await _context.SaveChangesAsync();
            }

            (string token, DateTime expiresAt) =
                _tokenService.GenerateToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Username = user.Username,
                Role = user.Role,
                ExpiresAt = expiresAt
            });
        }
    }
}