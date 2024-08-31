using Client.UI.Define;
using Core;
using UniRx;

namespace Client.UI
{
    public class TestWithDataPresenter : UiPresenter<TestWithDataView, TestWithDataPresenter.Data>
    {
        private UiLifecycleEvents _uiLifecycleEvents;
        protected override UiLifecycleEvents UiLifecycleEvents => _uiLifecycleEvents;

        // 최초 생성시 1회 호출
        public override void OnInitialize()
        {
            // 관심 이벤트 설정
            _uiLifecycleEvents = new UiLifecycleEvents
            {
                OnEnable = Enable, OnDisable = Disable, OnRelease = Release
            };

            // rx 설정
            InitializeRx();
        }

        private void InitializeRx()
        {
            BindView.OkButton.OnClickAsObservable().Subscribe(_ =>
            {
                //View의 버튼 이벤트를 요청
            }).AddTo(_disposable);

            BindView.CancelButton.OnClickAsObservable().Subscribe(_ =>
            {
                //View의 버튼 이벤트를 요청
            }).AddTo(_disposable);

            GlobalAPI.App.MainModels.TestModel.TestValue0.Subscribe(_ =>
            {
                // 모델의 이벤트 처리
            }).AddTo(_disposable);

            GlobalAPI.App.MainModels.TestModel.TestValue1.Subscribe(_ =>
            {
                // 모델의 이벤트 처리
            }).AddTo(_disposable);

            GlobalAPI.App.MainModels.TestModel.TestValue2.Subscribe(_ =>
            {
                // 모델의 이벤트 처리
            }).AddTo(_disposable);
        }

        // OnEnable 시점 로직 정의
        private void Enable()
        {
            if (BindData?.IsTest == true)
            {
                // Bind Data 활용.
            }
        }

        // OnDisable 시점 로직 정의
        private void Disable()
        {
        }

        // OnRelease 시점 로직 정의
        private void Release()
        {
        }

        // ui 오픈시 필요한 데이타를 설정
        public class Data : IPresenterData
        {
            public Data(bool isTest)
            {
                IsTest = isTest;
            }

            public bool IsTest { get; }
        }
    }
}