using Client.UI.Define;
using Core;
using UniRx;

namespace Client.UI
{
    public class TestPresenter : UiPresenter<TestView>
    {
        private UiLifecycleEvents _uiLifecycleEvents;
        protected override UiLifecycleEvents UiLifecycleEvents => _uiLifecycleEvents;

        // 최초 생성시 1회 호출
        public override void OnInitialize()
        {
            // 관심 이벤트 설정
            _uiLifecycleEvents = new UiLifecycleEvents
            {
                OnEnable = Enable, OnDisable = Disable
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
            BindView.TitleText.text = "타이틀";
            BindView.ContentText.text = "초기화";
        }

        // OnDisable 시점 로직 정의
        private void Disable()
        {
        }
    }
}