using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
    }
}
