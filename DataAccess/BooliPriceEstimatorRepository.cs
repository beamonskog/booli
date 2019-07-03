using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Interfaces;
using Common.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class BooliPriceEstimatorRepository : IBooliPriceEstimatorRepository
    {
        private readonly BooliPriceEstimatorContext _context;

        public BooliPriceEstimatorRepository(BooliPriceEstimatorContext booliPriceEstimatorContext)
        {
            _context = booliPriceEstimatorContext;
        }

        public async Task<List<SoldObject>> GetSoldObject(int booliId)
        {
            return await _context.SoldHousingObjects.Where(_ => _.BooliId == booliId).ToListAsync();
        }

        public async Task<List<SoldObject>> GetSoldObjectsInArea(string areaName)
        {
            return await _context.SoldHousingObjects
                .Where(s => s.Location.NamedAreas
                    .Any(n => n.Area == areaName)).ToListAsync();
        }

        public async Task SyncSoldObjects(List<SoldObject> soldObjects)
        {
            foreach (var soldObject in soldObjects)
            {
                if (Exists(soldObject.BooliId))
                {
                    continue;
                }

                await _context.SoldHousingObjects.AddAsync(soldObject);
            }
            await _context.SaveChangesAsync();
        }

        private bool Exists(int booliId)
        {
            return _context.SoldHousingObjects.Any(_ => _.BooliId == booliId);
        }
    }
}
