using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using TrafficLights.Model.Entities;

namespace TrafficLights.Model.Auth
{
    public class AuthenticateResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }

        public AuthenticateResponse(UserIdentityEntity user, string jwtToken, string refreshToken)
        {
            Id = user.Id;
            Name = user.UserName;            
            JwtToken = jwtToken;
            RefreshToken = refreshToken;
        }
    }
}