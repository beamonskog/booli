using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IBooliApiHandler
    {
        //Task SyncSoldObjects();
        Task SyncSoldObjectsItteratively();
    }
}