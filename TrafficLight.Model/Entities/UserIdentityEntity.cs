using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace TrafficLights.Model.Entities
{
    public class UserIdentityEntity : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
      public  List<RefreshToken> RefreshTokens { get; set; }
    }
}
