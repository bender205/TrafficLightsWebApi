using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TrafficLights.Data;
using TrafficLights.Model;
using TrafficLights.Model.Auth;
using TrafficLights.Model.Entities;
using TrafficLights.Model.Helpers;


namespace TrafficLights.Core
{
    public interface IUserService
    {
        Task<AuthenticateResponse> GetAuthenticateResponceTokensAsync(UserIdentityEntity user, string ipAddress);
        Task<AuthenticateResponse> RefreshTokenAsync(string token, string ipAddress);
        Task<bool> RevokeTokenAsync(string token, string ipAddress);
        Task<IEnumerable<UserIdentityEntity>> GetAllAsync();
        Task<UserIdentityEntity> GetByIdAsync(int id);
    }

    public class UserService : IUserService
    {
        private readonly AppSettings _appSettings;
        IServiceProvider Services { get; }
        private readonly AuthRepository _repository;
        private IConfiguration _configuration { get; }
        public UserService(IServiceProvider serviceProvider, IOptions<AppSettings> appSettings, IConfiguration configuration)
        {
            _configuration = configuration;
            Services = serviceProvider.CreateScope().ServiceProvider;
            _repository = Services.GetRequiredService<AuthRepository>();
            _appSettings = appSettings.Value;
        }

        public async Task<AuthenticateResponse> GetAuthenticateResponceTokensAsync(UserIdentityEntity user, string ipAddress)
        {
            // authentication successful so generate jwt and refresh tokens
            var jwtToken = await GenerateJwtTokenAsync(user);
            var refreshToken = GenerateRefreshToken(ipAddress);

            // save refresh token
            user.RefreshTokens.Add(refreshToken);
            _repository.Update(user);
            await _repository.SaveChangesAsync(CancellationToken.None);

            return new AuthenticateResponse(/*user,*/ jwtToken, refreshToken.Token);
        }

        public async Task<AuthenticateResponse> RefreshTokenAsync(string token, string ipAddress)
        {
            var user = await _repository.GetByTokenAsync(token, CancellationToken.None);

            // return null if no user found with token
            if (user == null) return null;

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            // return null if token is no longer active
            if (!refreshToken.IsActive) return null;

            // replace old refresh token with a new one and save
            var newRefreshToken = GenerateRefreshToken(ipAddress);
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            user.RefreshTokens.Add(newRefreshToken);
            _repository.Update(user);
            await _repository.SaveChangesAsync(CancellationToken.None);

            // generate new jwt
            var jwtToken = await GenerateJwtTokenAsync(user);

            return new AuthenticateResponse(/*user,*/ jwtToken, newRefreshToken.Token);
        }

        public async Task<bool> RevokeTokenAsync(string token, string ipAddress)
        {
            var user = await _repository.GetByTokenAsync(token, CancellationToken.None);

            // return false if no user found with token
            if (user == null) return false;

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            // return false if token is not active
            if (!refreshToken.IsActive) return false;

            // revoke token and save
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            _repository.Update(user);
            await _repository.SaveChangesAsync(CancellationToken.None);

            return true;
        }

        public async Task<IEnumerable<UserIdentityEntity>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<UserIdentityEntity> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        // helper methods
        private async Task<string> GenerateJwtTokenAsync(UserIdentityEntity user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            // Getting all users claims
            var userRoles = await _repository.GetRolesAsync(user);
            // var claims =  await _repository.GetClaimsAsync(user);
            List<Claim> userClaims = new List<Claim>();
            foreach (var r in userRoles)
            {
                int userRoleId = int.Parse(r.Id);
                var claim = await _repository.GetClaimByRoleIdAsync(userRoleId);
                userClaims.Add(new Claim(claim.ClaimType.ToString(), claim.ClaimValue.ToString()));
            }

            var certificate = new X509Certificate2(@"C:\Users\Developer\certs\mycert.pfx");
            var securityKey = new X509SecurityKey(certificate);
          
            userClaims.Add(new Claim(ClaimTypes.Name, user.Id.ToString()));

            var tokenAudienceSection = _configuration.GetSection("TokenOptions:Audience");
            var tokenIssuerSection = _configuration.GetSection("TokenOptions:Issuer");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(userClaims),
                Expires = DateTime.UtcNow.AddDays(2),
                Issuer = tokenIssuerSection.Value,
                Audience = tokenAudienceSection.Value,
                    SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha512Signature)
            };
           
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private RefreshToken GenerateRefreshToken(string ipAddress)
        {
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[64];
                rngCryptoServiceProvider.GetBytes(randomBytes);
                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomBytes),
                    Expires = DateTime.UtcNow.AddDays(7),
                    Created = DateTime.UtcNow,
                    CreatedByIp = ipAddress
                };
            }
        }


    }
}