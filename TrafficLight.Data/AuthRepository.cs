using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TrafficLights.Data.DataAccess;
using TrafficLights.Model;
using TrafficLights.Model.Entities;

namespace TrafficLights.Data
{
    public class AuthRepository
    {
        private readonly TraficLightsContext _databaseContext;

        public AuthRepository(TraficLightsContext databaseContext)
        {
            _databaseContext = databaseContext;
        }
         
        public async Task<UserIdentityEntity> GetByCredentialsAsync(string userName, string passWord,
            CancellationToken cancellationToken)
        {
            return await _databaseContext.Users.FirstOrDefaultAsync(
                (x => x.UserName == userName && x.PasswordHash == passWord), cancellationToken);
            throw new NotImplementedException();
        }
        public async Task<UserIdentityEntity> GetByTokenAsync(string token, CancellationToken cancellationToken)
        {
            return await _databaseContext.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token), cancellationToken);
        }
        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _databaseContext.SaveChangesAsync(cancellationToken);
        }

        public void Update(UserIdentityEntity user)
        {
           _databaseContext.Users.Update(user);
        }
        public async Task<IEnumerable<UserIdentityEntity>> GetAllAsync()
        {
            return await Task.Run(() => _databaseContext.Users);
        }
        public async Task<UserIdentityEntity> GetByIdAsync(int id)
        {          
              return await _databaseContext.Users.FindAsync(id);
        }
        public async Task<IEnumerable<IdentityRole>> /*Task<IEnumerable<IdentityRoleClaim<string>>>*/ GetRolesAsync(UserIdentityEntity user)
        {
            var roleIds = await _databaseContext.UserRoles.Where(s => s.UserId == user.Id).Select(s => s.RoleId).ToListAsync();
            return await _databaseContext.Roles.Where(s => roleIds.Contains(s.Id)).ToListAsync();
           // return await _databaseContext.UserClaims.Where(x => x.Id == userId).ToListAsync();
            //return await _databaseContext.RoleClaims.FindAsync(user);
        }
        // TO DO edit method return type
        public async Task<IdentityRoleClaim<string>>  GetClaimByRoleIdAsync(int roleId)
        {           
             return await _databaseContext.RoleClaims.FirstOrDefaultAsync(x => x.RoleId == roleId.ToString());
             //return await _databaseContext.RoleClaims.FirstOrDefaultAsync(x => x.Id == roleId);
            //return await _databaseContext.RoleClaims.FindAsync(user);
        }
    }
}
