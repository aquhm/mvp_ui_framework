Overview
=============

- UniRx 기반의 MVP 구조
- 관심있는 event 시점만 정의하도록 event driven 구조
- UiViewAttribute로 load할 프리팹 경로 등 ui optional property 설정



UniRx 기반의 mvp 구조
-------------
- Player, Game 상태등 주요 Propety model 정보를 기반으로 Presenter가 View에 상태 갱신 및 View에 Event 정보로 모델 갱신 및 Business 로직 처리
![image](https://github.com/user-attachments/assets/6391f644-9db2-4cb9-b0b5-fa34d4cb61d0)

![image](https://github.com/user-attachments/assets/00a28a12-d340-490d-9c54-f3e3a4accb25)

Event driven presenter 구조
-------------
- UiPresneter는 추상화하여 event 시점을 Implement Presenter에 제공하여 관심있는 event만 등록하여 구현하도록 제안
![image](https://github.com/user-attachments/assets/d5c694a0-370e-4101-ad7b-bcb2018403aa)


Presenter 구조
-------------
<pre>
<code>
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
</code>
</pre>

View 구조
-------------
<pre>
<code>
using Client.Attribute;
using Client.Data;
using Client.Extention;
using Client.UI.Define;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Client.UI
{
    [UiView("TestPopup", layer: EUILayer.Popup, focusState: EUIFocusState.Modal)]
    public class TestView : MonoBehaviour, IUiView
    {
        public Button OkButton { get; private set; }
        public Button CancelButton { get; private set; }
        public TextMeshProUGUI TitleText { get; private set; }
        public TextMeshProUGUI ContentText { get; private set; }

        public void Awake()
        {
            OkButton = transform.Find<Button>("Ok").Assert();
            CancelButton = transform.Find<Button>("Cancel").Assert();
            TitleText = transform.Find<TextMeshProUGUI>("TitleText").Assert();
            ContentText = transform.Find<TextMeshProUGUI>("ContentText").Assert();
        }

        public bool Loaded { get; set; }
        public bool Opened { get; set; }
    }
}
</code>
</pre>

Model 구조
-------------
<pre>
<code>
using System;
using Client.UI.Define;
using UniRx;

namespace UI.Models
{
    public class UiStateModel : IUiModel, IResetableModel
    {
        public readonly ReactiveCommand<Type> Closed = new();
        public readonly ReactiveCommand<Type> ClosedFocusingUi = new();
        public readonly ReactiveCommand<Type> Opened = new();
        public readonly ReactiveCommand<Type> OpenedFocusingUi = new();
        public readonly ReactiveCommand<Type> OutAreaClickClosed = new();

        public void OnSceneChange()
        {
        }


        public void OnInitialize()
        {
        }

        public void OnRelease()
        {
        }

        public void SetOpen(Type type)
        {
            Opened.Execute(type);
        }

        public void SetClose(Type type)
        {
            Closed.Execute(type);
        }

        public void SetOutAreaClickClose(Type type)
        {
            OutAreaClickClosed.Execute(type);
        }

        public void SetClosedFocusingUi(Type type)
        {
            ClosedFocusingUi.Execute(type);
        }

        public void SetOpenFocusingUi(Type type)
        {
            OpenedFocusingUi.Execute(type);
        }
    }
}
</code>
</pre>



