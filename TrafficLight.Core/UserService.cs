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

        public UserService(IServiceProvider serviceProvider, IOptions<AppSettings> appSettings)
        {
            Services = serviceProvider.CreateScope().ServiceProvider;
            _repository = Services.GetRequiredService<AuthRepository>();
            _appSettings = appSettings.Value;
        }


        
        public async Task<AuthenticateResponse> GetAuthenticateResponceTokensAsync(UserIdentityEntity user, string ipAddress)
        {
           // var user = await _repository.GetByCredentialsAsync(userEntity.UserName, userEntity.PasswordHash, CancellationToken.None);

        //    if (user == null) return null;

            // authentication successful so generate jwt and refresh tokens
            var jwtToken = GenerateJwtToken(user);
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
            var jwtToken = GenerateJwtToken(user);

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

        private string GenerateJwtToken(UserIdentityEntity user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            // var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            X509Certificate2 cert = new X509Certificate2(@"C:\Users\Developer\certs\mycert.pfx");
            SecurityKey signingKey = new X509SecurityKey(cert);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(15),


               SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256)
                //SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
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