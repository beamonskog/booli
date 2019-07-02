using System.Threading.Tasks;

namespace Common
{
    public interface IBooliApi
    {
        Task<string> GetAllSoldObjects(string area, int sinceYears);
    }
}