using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TrafficLights.Data.DataAccess;
using TrafficLights.Model.Entities;

namespace TrafficLights.Data
{
    public class TrafficLightRepository
    {
        private readonly TraficLightsContext _databaseContext;
        public TrafficLightRepository(TraficLightsContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task<TrafficLightEntity> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _databaseContext.Lights.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        }
        public async Task AddTrafficLightAsync(TrafficLightEntity traficLight, CancellationToken cancellationToken)
        {
            await _databaseContext.AddAsync(traficLight);
            await _databaseContext.SaveChangesAsync(cancellationToken);
        }
        public async Task<int> GetMaxTraficLightsIdAsync(CancellationToken cancellationToken)
        {
            return await _databaseContext.Lights.MaxAsync(t => t.Id, cancellationToken);
        }
        public async Task<IEnumerable<TrafficLightEntity>> GetAllTraficLights()
        {
            return await Task.Run(() => _databaseContext.Lights);
        }
    }
}
