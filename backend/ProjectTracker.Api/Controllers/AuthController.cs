using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProjectTracker.Api.Models;
using ProjectTracker.Api.Models.Requests;
using ProjectTracker.Domain;
using ProjectTracker.Domain.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProjectTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserAuthService userAuthService;
    private readonly IConfiguration configuration;

    public AuthController(IUserAuthService userAuthService,
        IConfiguration configuration)
    {
        this.userAuthService = userAuthService;
        this.configuration = configuration;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var existing = await userAuthService.FindByEmailAsync(request.Email);
        if (existing is not null) return BadRequest(ApiResponse<object>.Fail("Email already registered", 400));
        await userAuthService.CreateAsync(request.Email, request.Password, request.FullName, request.Designation);
        return Ok(ApiResponse<object>.Success(null, "Registered"));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await userAuthService.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Unauthorized(ApiResponse<object>.Fail("Invalid credentials", 401));
        }

        var passwordValid = await userAuthService.VerifyPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            return Unauthorized(ApiResponse<object>.Fail("Invalid credentials", 401));
        }

        var token = GenerateJwtToken(user);
        return Ok(ApiResponse<object>.Success(new { token }, "Logged in"));
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var jwtKey = configuration["Jwt:Key"]!;
        var jwtIssuer = configuration["Jwt:Issuer"]!;
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty)
        };

        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}


