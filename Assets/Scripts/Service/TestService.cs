using Client.Core;
using Client.UI;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Client.Service
{
    public class TestService : IService
    {
        public bool IsEnabled { get; private set; }

        public async UniTask<(IService service, bool result)> Init()
        {
            Debug.Log("TestService Init");

            return (this, true);
        }

        public async UniTask<(IService service, bool result)> Prepare()
        {
            // 모든 서비스가 Init으로 인해 생성된 이후에 불린다.
            // 다른 이벤트를 구독할 수 있게 된다.
            Debug.Log("TestService Prepare");
            // 보통 여기서 IsEnabled를 true로 바꿈
            IsEnabled = true;

            // ui 오픈
            GlobalAPI.UiService.Open<TestPresenter>();
            GlobalAPI.UiService.Open<TestWithDataPresenter>(new TestWithDataPresenter.Data(true));

            return (this, true);
        }

        public void Release()
        {
            // 게임이 종료될때 가장 먼저 불림.
            // Rx 구독 취소나 Dispose를 여기서 진행
            Debug.Log("TestService Release");
            // 보통 여기서 IsEnabled를 false로 바꿈
            IsEnabled = false;
        }
    }
}