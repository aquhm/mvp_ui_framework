using System;
using System.Collections.Generic;
using System.Threading;
using Client.Core;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Client.Service
{
    public class Services
    {
        private bool _isInitialized;
        internal CancellationTokenSource CancellationSource = new();
        internal CompositeDisposable Disposables = new();

        internal UiService UiService { get; private set; }
        internal TestService TestService { get; }

        public IEnumerable<IService> All
        {
            get
            {
                yield return UiService;
                yield return TestService;
            }
        }

        private async UniTask<(IService service, bool isSuccess)> TryInitializeServiceAsync(IService service)
        {
            try
            {
                var result = await service.Init();
                return result;
            }
            catch
            {
                return (service, false);
            }
        }

        private async UniTask<(IService service, bool isSuccess)> TryPrepareServiceAsync(IService service)
        {
            try
            {
                var result = await service.Prepare();
                return result;
            }
            catch
            {
                return (service, false);
            }
        }

        public async UniTask<bool> Init()
        {
            UiService ??= new UiService();

            var success = true;
            try
            {
                var tasks = new List<UniTask<(IService service, bool result)>>
                {
                    TryInitializeServiceAsync(UiService),
                    TryInitializeServiceAsync(TestService)
                };

                var results = await UniTask.WhenAll(tasks);

                foreach (var result in results)
                {
                    var type = result.service.GetType();
                    if (result.result == false)
                    {
                        Debug.LogError($@"<color=red>{type}</color> Service Init Failed");
                        success = false;
                        CancellationSource?.Cancel();
                    }
                    else
                    {
                        Debug.Log($@"<color=green>{type}</color> Service Init Success ");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }

            return success;
        }

        public async UniTask<bool> Prepare()
        {
            var success = true;
            try
            {
                var tasks = new List<UniTask<(IService service, bool result)>>
                {
                    TryPrepareServiceAsync(UiService),
                    TryPrepareServiceAsync(TestService)
                };

                var results = await UniTask.WhenAll(tasks);

                foreach (var result in results)
                {
                    var type = result.service.GetType();
                    if (result.result == false)
                    {
                        Debug.LogError($@"<color=red>{type}</color> Service Init Failed");
                        success = false;
                        CancellationSource?.Cancel();
                    }
                    else
                    {
                        Debug.Log($@"<color=green>{type}</color> Service Init Success ");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }

            _isInitialized = true;

            return success;
        }

        public void Release()
        {
            Disposables?.Clear();
            foreach (var service in All)
            {
                if (service is null) continue;

                try
                {
                    service?.Release();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error releasing service {service.GetType()}: {e}");
                }
            }

            UiService = null;
        }

        public void DoUpdate(float deltaTime)
        {
            if (_isInitialized)
            {
                // 서비스 Late 업데이트
                // SomeService.DoUpdate(deltaTime);
            }
        }

        public void DoLateUpdate(float deltaTime)
        {
            if (_isInitialized)
            {
                // 서비스 Late 업데이트
                //SomeService.DoLateUpdate(deltaTime);
            }
        }
    }
}