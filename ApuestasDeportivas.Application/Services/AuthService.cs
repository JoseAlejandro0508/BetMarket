using ApuestasDeportivas.Application.Common.Interfaces;
using ApuestasDeportivas.Contracts.Auth;
using ApuestasDeportivas.Domain.Constants;
using ApuestasDeportivas.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace ApuestasDeportivas.Application.Services;

/// <summary>
/// Caso de uso para registro, login y lectura de perfil.
/// </summary>
public class AuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<AppUser> userManager,
        IJwtTokenService jwtTokenService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            throw new InvalidOperationException("Ya existe una cuenta con ese correo.");
        }

        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            Balance = 100m,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(errors);
        }

        await _userManager.AddToRoleAsync(user, AppRoles.User);

        var roles = await _userManager.GetRolesAsync(user);
        return BuildAuthResponse(user, roles);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
                   ?? throw new InvalidOperationException("Credenciales inválidas.");

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            throw new InvalidOperationException("Credenciales inválidas.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        return BuildAuthResponse(user, roles);
    }

    public async Task<UserProfileDto> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
                   ?? throw new InvalidOperationException("Usuario no encontrado.");

        return new UserProfileDto(user.Id, user.DisplayName, user.Email ?? string.Empty, user.Balance);
    }

    private AuthResponse BuildAuthResponse(AppUser user, IList<string> roles)
    {
        var token = _jwtTokenService.GenerateToken(user, roles);
        var expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var minutes) ? minutes : 240;

        return new AuthResponse(
            token,
            DateTimeOffset.UtcNow.AddMinutes(expiryMinutes),
            new UserProfileDto(user.Id, user.DisplayName, user.Email ?? string.Empty, user.Balance),
            roles.ToList());
    }
}
