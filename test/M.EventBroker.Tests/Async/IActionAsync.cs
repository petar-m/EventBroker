using System.Threading.Tasks;

namespace M.EventBroker.Tests
{
    public interface IActionAsync
    {
        Task Action();
    }
}
