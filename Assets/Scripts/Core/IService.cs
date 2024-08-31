using Cysharp.Threading.Tasks;

namespace Client.Core
{
    public interface IService
    {
        public bool IsEnabled { get; }
        public UniTask<(IService service, bool result)> Init();
        public UniTask<(IService service, bool result)> Prepare();
        public void Release();
    }
}