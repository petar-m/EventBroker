using System.Threading.Tasks;

namespace M.EventBroker.Async.Tests
{
    public interface IActionAsync
    {
        Task Action();
    }
}
