using App.Domain.Identity;
using App.Repository.DalUow;
using App.Service.IdentityDto;
using App.Service.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace App.Service.Impl;

public class AccountService : IAccountService
{
    private readonly IAppUow _uow;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IConfiguration _configuration;

    private const string SettingsJwtPrefix = "JWTSecurity";
    private const string SettingsJwtKey = SettingsJwtPrefix + ":Key";
    private const string SettingsJwtIssuer = SettingsJwtPrefix + ":Issuer";
    private const string SettingsJwtAudience = SettingsJwtPrefix + ":Audience";
    private const string SettingsJwtExpiresInSeconds = SettingsJwtPrefix + ":ExpiresInSeconds";
    private const string SettingsJwtRefreshTokenExpiresInSeconds = SettingsJwtPrefix + ":RefreshTokenExpiresInSeconds";



    public AccountService(
        IAppUow uow, 
        UserManager<User> userManager, 
        SignInManager<User> signInManager, IConfiguration configuration)
    {
        _uow = uow;
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }
    
    public async Task<JwtResponseDto> RegisterAsync(RegisterDto dto, int? jwtExpiresInSeconds, int? refreshTokenExpiresInSeconds)
    {
        // is user already registered
        var appUser = await _userManager.FindByEmailAsync(dto.Email);
        if (appUser != null)
        {
            throw new InvalidCredentialException("The user with this email is already registered.");
        }

        // register user
        var refreshToken = new AppRefreshToken()
        {
            Expiration = GetExpirationDateTime(refreshTokenExpiresInSeconds, SettingsJwtRefreshTokenExpiresInSeconds)
        };
        
        var user = new AppUser()
        {
            Email = dto.Email,
            UserName = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            RefreshTokens = new List<AppRefreshToken>()
            {
                refreshToken
            }
        };
        
        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded) throw new InvalidCredentialException("Failed to create user.");

        var personDal = _mapper.Map(dto);
        personDal!.UserId = user.Id;
        
        _uow.PersonRepository.Add(personDal, user.Id);
        await _uow.SaveChangesAsync();
        
        
        await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.GivenName, user.FirstName));
        await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Surname, user.LastName));

        var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(user);
        var jwt = IdentityExtensions.GenerateJwt(
            claimsPrincipal.Claims,
            _configuration[SettingsJwtKey]!,
            _configuration[SettingsJwtIssuer]!,
            _configuration[SettingsJwtAudience]!,
            GetExpirationDateTime(jwtExpiresInSeconds, SettingsJwtExpiresInSeconds)
        );
        
        var responseData = new JwtResponseDto()
        {
            Jwt = jwt,
            RefreshToken = refreshToken.RefreshToken,
        };

        return responseData;
    }

    public async Task<JwtResponseDto> LoginAsync(LoginDto dto, int? jwtExpiresInSeconds, int? refreshTokenExpiresInSeconds)
    {
        // verify user
        var user = await _userManager.FindByEmailAsync(dto.Email) ??
                   throw new InvalidCredentialException("User with email " + dto.Email + " does not exist.");

        if (!(await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false)).Succeeded)
            throw new InvalidCredentialException("Wrong credentials.");

        await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.GivenName, user.FirstName));
        await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Surname, user.LastName));

        var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(user);
        await _uow.RefreshTokenRepository.DeleteExpiredTokenAsync(user.Id);

        var newRefreshToken = new RefreshTokenDalDto()
        {
            UserId = user.Id,
            RefreshToken = Guid.NewGuid().ToString(),
            Expiration = GetExpirationDateTime(refreshTokenExpiresInSeconds, SettingsJwtRefreshTokenExpiresInSeconds)
        };
        
        _uow.RefreshTokenRepository.Add(newRefreshToken);
        await _uow.SaveChangesAsync();
        
        var jwt = IdentityExtensions.GenerateJwt(
            claimsPrincipal.Claims,
            _configuration[SettingsJwtKey]!,
            _configuration[SettingsJwtIssuer]!,
            _configuration[SettingsJwtAudience]!,
            GetExpirationDateTime(jwtExpiresInSeconds, SettingsJwtExpiresInSeconds)
        );

        var role = await _userManager.GetRolesAsync(user);
        var responseData = new JwtResponseDto()
        {
            Jwt = jwt,
            RefreshToken = newRefreshToken.RefreshToken,
            Role = role.FirstOrDefault()!
        };

        return responseData;
    }

    public async Task<JwtResponseDto> RenewTokenAsync(RefreshTokenDto dto, int? jwtExpiresInSeconds, int? refreshTokenExpiresInSeconds)
    {
       JwtSecurityToken jwtToken;
        // get user info from jwt
        try
        {
            jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(dto.Jwt);
        }
        catch (Exception e)
        {
            throw new InvalidCredentialException($"Cannot parse JWT: {e.Message}");
        }
        
        // validate jwt, ignore expiration date
        if (!IdentityExtensions.ValidateJwt(
                dto.Jwt,
                _configuration[SettingsJwtKey]!,
                _configuration[SettingsJwtIssuer]!,
                _configuration[SettingsJwtAudience]!
            ))
        {
            throw new InvalidCredentialException("JWT validation failed");
        }

        var userEmail = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value ??
                        throw new InvalidCredentialException("No email in jwt.");

        // get user and tokens
        var user = await _userManager.FindByEmailAsync(userEmail) ??
                      throw new InvalidCredentialException($"User with email {userEmail} not found");

        await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.GivenName, user.FirstName));
        await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Surname, user.LastName));

        await _uow.RefreshTokenRepository.DeleteExpiredTokenAsync(user.Id);

        var tokens = await _uow.RefreshTokenRepository.FindByTokenAsync(user.Id, dto.RefreshToken);
        if (tokens.Count != 1)
        {
            throw new InvalidCredentialException("No valid refresh tokens found.");
        }

        var refreshToken = tokens.Single();
        if (refreshToken.RefreshToken != dto.RefreshToken)
        {
            throw new InvalidCredentialException("Refresh token is no longer valid");
        }

        if (refreshToken.RefreshToken == dto.RefreshToken)
        {
            refreshToken.PreviousRefreshToken = refreshToken.RefreshToken;
            refreshToken.PreviousExpiration = DateTime.UtcNow.AddMinutes(1);

            refreshToken.RefreshToken = Guid.NewGuid().ToString();
            refreshToken.Expiration =
                GetExpirationDateTime(refreshTokenExpiresInSeconds, SettingsJwtRefreshTokenExpiresInSeconds);
        }

        _uow.RefreshTokenRepository.Update(refreshToken);
        await _uow.SaveChangesAsync();
        
        var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(user);
        
        // generate jwt
        var newJwt = IdentityExtensions.GenerateJwt(
            claimsPrincipal.Claims,
            _configuration[SettingsJwtKey]!,
            _configuration[SettingsJwtIssuer]!,
            _configuration[SettingsJwtAudience]!,
            GetExpirationDateTime(jwtExpiresInSeconds, SettingsJwtExpiresInSeconds)
        );
        
        var roles = await _userManager.GetRolesAsync(user);
        return new JwtResponseDto
        {
            Jwt          = newJwt,
            RefreshToken = refreshToken.RefreshToken,
            Role         = roles.FirstOrDefault()!
        };
    }

    public async Task LogoutAsync(Guid userId, string refreshToken)
    {
        await _uow.RefreshTokenRepository.DeleteByTokenAsync(userId, refreshToken);
    }
    
    private DateTime GetExpirationDateTime(int? expiresInSeconds, string settingsKey)
    {
        if (expiresInSeconds <= 0) expiresInSeconds = int.MaxValue;
        expiresInSeconds = expiresInSeconds <  int.Parse(_configuration[settingsKey]!)
            ? expiresInSeconds
            : int.Parse(_configuration[settingsKey]!);

        return DateTime.UtcNow.AddSeconds(expiresInSeconds ?? 60);
    }
}