using Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IBooliPriceEstimatorRepository
    {
        Task SyncSoldObjects(List<SoldObject> soldObjects);
        Task<List<SoldObject>> GetSoldObjectsInArea(string areaName);
        Task<List<SoldObject>> GetSoldObject(int booliId);
    }
}
