using System.Threading.Tasks;
using Common.Models;

namespace BooliPriceEstimator
{
    public interface IBooliPriceEstimator
    {
        //Task<int> GetMeanPricePerSquareMeter(string area);
        Task<int> GetPriceEstimation(PriceEstimationInputModel inputModel);
    }
}