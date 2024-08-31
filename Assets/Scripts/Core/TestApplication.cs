using Client.Service;
using Client.UI.Model;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting;

namespace Core
{
    public class TestApplication : MonoBehaviour
    {
        private CompositeDisposable _disposable = new();

        private bool _isQuitUIOpened;
        public MainModels MainModels { get; private set; }

        public Services Services { get; private set; }

        public bool IsInitialized { get; private set; }
        public bool ConfirmedApplicationQuit { get; private set; }

        private void Awake()
        {
            DontDestroyOnLoad(this);

            Services = new Services();
            GlobalAPI.Init(this);

            MainModels ??= new MainModels();
        }

        private void Update()
        {
            Services.DoUpdate(Time.deltaTime);
            GarbageCollector.CollectIncremental(2_000_000);
        }

        private void LateUpdate()
        {
            Services.DoLateUpdate(Time.deltaTime);
        }

        public async UniTask StandBy()
        {
            if (IsInitialized == false)
            {
                await Init();
                await Prepare();
            }

            IsInitialized = true;
        }

        private async UniTask Init()
        {
            var result = await Services.Init();
            if (result)
                MainModels?.Init();
            else
                Debug.LogError("Application Init Failed");
        }

        public async UniTask Prepare()
        {
            var result = await Services.Prepare();
        }

        private void Release()
        {
            MainModels?.Release();
        }

        private void OnQuitting()
        {
            Debug.Log("[Application] OnQuitting Start");

            ConfirmedApplicationQuit = true;

            //EventBetter.Raise(new OnApplicationQuit());
            Debug.LogWarning("AppEnv Release");
            Release();
            _disposable?.Dispose();
            _disposable = null;

            //GlobalAPI.Destroy();
            GlobalAPI.OnRelease();
            GlobalAPI.Destroy();

            Debug.Log("[Application] OnApplicationQuit Finish");
        }

        private bool WantsToQuit()
        {
            if (ConfirmedApplicationQuit == false)
            {
                if (_isQuitUIOpened) QuitApplication(true).Forget();

                _isQuitUIOpened = true;
            }

            return ConfirmedApplicationQuit;
        }

        public async UniTaskVoid QuitApplication(bool isForce)
        {
            Debug.Log($"[Application] isForce = {isForce}");
            await UniTask.DelayFrame(5);
            if (isForce)
            {
                ConfirmedApplicationQuit = true;
            }
            else
            {
                WantsToQuit();
                return;
            }

            await UniTask.Delay(500);

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}