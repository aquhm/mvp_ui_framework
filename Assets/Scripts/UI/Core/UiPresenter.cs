using System;
using UniRx;

namespace Client.UI.Define
{
    // Present에 바인딩될 Data는 없고 기본 View만 있는 UI
    public abstract class UiPresenter<V> : IEscapable, IUiPresenter where V : IUiView
    {
        protected readonly CompositeDisposable _disposable = new();
        private readonly CompositeDisposable _eventDisposable = new();

        protected readonly ReactiveCommand _onBind = new();
        private readonly ReactiveCommand _onDisable = new();
        private readonly ReactiveCommand _onEnable = new();
        private readonly ReactiveCommand _onEscape = new();
        private readonly ReactiveCommand _onRelease = new();

        private readonly Subject<UiState> _statusChanged = new();

        protected bool _initialize;
        public V BindView { get; protected set; }
        public UiState State { private set; get; }

        // UI의 라이프 사이클 이벤트 시점에 수행할 액션 리스트 설정(OnEnable, OnDisable, OnRelease, OnEscape, OnDisposeRx)
        protected abstract UiLifecycleEvents UiLifecycleEvents { get; }
        public bool IsEnableEscape { get; protected set; } = true;
        public bool HasOnEscape => UiLifecycleEvents.OnEscape != default;

        public void OnEscape()
        {
            _onEscape.Execute();
        }

        public IUiView View => BindView;

        // UI의 최초 생성 또는 OnEnable 시점에 호출
        public virtual void Bind(IUiView view, IPresenterData data)
        {
            BindView = (V)view;

            _onBind.Execute();

            if (_initialize == false)
            {
                InitializeRx();
                OnInitialize();
                Listen(UiLifecycleEvents);

                _initialize = true;
            }
        }

        // UI의 최초 생성시 1회 호출되는 시점 (UiLifecycleEvents 설정을 이 시점에 권장)
        public abstract void OnInitialize();

        private void InitializeRx()
        {
            _eventDisposable.Clear();
            _statusChanged.Subscribe(state =>
            {
                switch (state)
                {
                    case UiState.Enable:
                        InternalEnable();
                        break;
                    case UiState.Disable:
                        InternalDisable();
                        break;
                    case UiState.Release:
                        InternalRelease();
                        break;
                }
            }).AddTo(_eventDisposable);
        }

        public void SetState(UiState state)
        {
            var changed = State != state;
            State = state;
            if (changed) _statusChanged.OnNext(state);
        }

        private void InternalEnable()
        {
            _onEnable.Execute();
        }

        private void InternalDisable()
        {
            _onDisable.Execute();
        }

        protected virtual void InternalRelease()
        {
            _onRelease.Execute();

            _disposable.Clear();

            SetState(UiState.None);

            _initialize = false;
            BindView = default;
        }

        protected void Close()
        {
            //GlobalAPI.AppEnv.MainPresenters.Close(this);
        }

        protected void SubscribeEvent<T>(IObservable<T> observable, Action action)
        {
            if (action != default) observable.Subscribe(_ => action.Invoke()).AddTo(_eventDisposable);
        }

        protected void Listen(UiLifecycleEvents setting)
        {
            SubscribeEvent(_onEnable, setting.OnEnable);
            SubscribeEvent(_onDisable, setting.OnDisable);
            SubscribeEvent(_onRelease, setting.OnRelease);
            SubscribeEvent(_onEscape, setting.OnEscape);
        }
    }

    // Present에 View와 함께 Data도 바인딩이 필요한 UI
    public abstract class UiPresenter<V, D> : UiPresenter<V> where V : IUiView where D : IPresenterData
    {
        public D BindData { protected set; get; }

        public override void Bind(IUiView view, IPresenterData data)
        {
            BindData = (D)data;
            base.Bind(view, data);
        }

        protected override void InternalRelease()
        {
            base.InternalRelease();
            BindData = default;
        }
    }
}