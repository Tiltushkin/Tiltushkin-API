using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyApp.Api.DTOs;
using MyApp.Api.Services;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly JwtTokenService _jwt;

        public AuthController(UserManager<IdentityUser> userManager,
                              SignInManager<IdentityUser> signInManager,
                              JwtTokenService jwt)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwt = jwt;
        }

        /// <summary>
        /// Register new user and return JWT
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req)
        {
            var user = new IdentityUser
            {
                UserName = req.Username,
                Email = req.Email
            };

            var result = await _userManager.CreateAsync(user, req.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            await _userManager.AddToRoleAsync(user, "user");
            var roles = await _userManager.GetRolesAsync(user);
            var (token, exp) = _jwt.CreateToken(user, roles);
            return Ok(new AuthResponse { Token = token, ExpiresAtUtc = exp });
        }

        /// <summary>
        /// Login and receive JWT
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
        {
            var user = await _userManager.FindByEmailAsync(req.Email);
            if (user == null) return Unauthorized("Invalid credentials");

            var passOk = await _userManager.CheckPasswordAsync(user, req.Password);
            if (!passOk) return Unauthorized("Invalid credentials");

            var roles = await _userManager.GetRolesAsync(user);
            var (token, exp) = _jwt.CreateToken(user, roles);
            return Ok(new AuthResponse { Token = token, ExpiresAtUtc = exp });
        }
    }
}
