using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Client.Attribute;
using Client.Core;
using Client.Data;
using Client.Extention;
using Client.UI;
using Client.UI.Define;
using Core;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Client.Service
{
    public class UiService : IService
    {
        private readonly CompositeDisposable _disposable = new();
        private readonly List<IEscapable> _escapablePresenters = new();
        private readonly List<(IUiPresenter presenter, EUIFocusState focusState)> _focusingPresenters = new();
        private readonly Dictionary<Type, UiEntity> _presenters = new();
        public readonly ReactiveCommand<Type> CloseCommand = new();


        public bool IsEnabled { get; private set; }

        public async UniTask<(IService service, bool result)> Init()
        {
            return (this, true);
        }

        public async UniTask<(IService service, bool result)> Prepare()
        {
            IsEnabled = true;

            TestWithDataPresenter = Generate<TestWithDataPresenter, TestWithDataView>();
            TestPresenter = Generate<TestPresenter, TestView>();
            InitializeRx();
            return (this, true);
        }

        public void Release()
        {
            _disposable.Clear();
            Clear(true);
            IsEnabled = false;
        }


        private P Generate<P, V>() where P : UiPresenter<V>, new() where V : IUiView
        {
            var presenterType = typeof(P);
            if (_presenters.ContainsKey(presenterType) == false)
            {
                var presenter = new P();
                if (presenter is UiPresenter<V> uiPresenter)
                    _presenters[presenterType] =
                        new UiEntity(uiPresenter, typeof(V), state => { presenter.SetState(state); });

                return presenter;
            }

            return default;
        }

        private void InitializeRx()
        {
            _disposable.Clear();

            Observable.EveryUpdate()
                .Where(_ => Input.GetKeyDown(KeyCode.Escape))
                .Subscribe(_ => OnEscape()).AddTo(_disposable);

            CloseCommand.Subscribe(viewType =>
            {
                if (_presenters.Values.FirstOrDefault(t => t.Presenter.View?.GetType() == viewType) is { } uiEntity)
                    Close(uiEntity.Presenter);
            }).AddTo(_disposable);
        }

        private void OnEscape()
        {
            // FocusUiAttribute류 Ui부터 우선 검사 Close 또는 OnEscape 호출
            if (_focusingPresenters.Count > 0)
            {
                var peekedPresenter = _focusingPresenters[0];
                if (FindByPresenterType(peekedPresenter.presenter.GetType()) is var pv &&
                    pv?.Presenter.View?.Opened == true)
                    // esc키 닫힘 기능이 활성화 되어 있는지 체크 후(메시지 박스의 경우 esc 닫힘도 막을 수 있다.)
                    if (pv.Presenter as IEscapable is var escapablePresenter &&
                        escapablePresenter?.IsEnableEscape == true)
                    {
                        // MARK: FocusState != EUIFocusState.None 인 프리젠터들은 OnEscape 구현에서 Close 까지 해줘야함
                        if (escapablePresenter.HasOnEscape)
                        {
                            escapablePresenter.OnEscape();
                        }
                        else
                        {
                            _focusingPresenters.RemoveAt(0);
                            Close(peekedPresenter.presenter);
                        }
                    }
            }
            else
            {
                // 오픈된 FocusUiAttribute류 Ui가 없다면 Escapable UI들에 OnEscape 전파
                foreach (var escapablePresenter in _escapablePresenters)
                    if (FindByPresenterType(escapablePresenter.GetType()) is var pv &&
                        pv.Presenter.View?.Opened == true)
                        escapablePresenter.OnEscape();
            }
        }

        public UiEntity FindByPresenterType(Type type)
        {
            return _presenters.GetValueOrDefault(type);
        }

        public void Open<T>(IPresenterData data = default) where T : class
        {
            if (ViewRootBehaviour.Instance == null) return;

            var value = FindByPresenterType(typeof(T));
            var presenter = value?.Presenter;
            if (presenter != default)
            {
                if (presenter.View != default)
                {
                    if (presenter.View.Opened == false)
                    {
                        CloseUiTypes(presenter);

                        presenter.Bind(presenter.View, data);
                        ViewRootBehaviour.Instance.Open(presenter.View as MonoBehaviour);

                        //PushFocusingPresenter(value).Forget();
                        value.SetState(UiState.Enable);
                    }
                    else
                    {
                        var reopen = presenter.View.GetType().GetCustomAttribute<UiViewAttribute>();
                        if (reopen?.CanReopen == true)
                        {
                            Close(presenter);
                            CloseUiTypes(presenter);

                            presenter.Bind(presenter.View, data);
                            ViewRootBehaviour.Instance.Open(presenter.View as MonoBehaviour);

                            //PushFocusingPresenter(value).Forget();
                            value.SetState(UiState.Enable);
                        }
                    }


                    GlobalAPI.App.MainModels.UiStateModel.SetOpen(presenter.GetType());
                }
                else
                {
                    if (ViewRootBehaviour.Instance.Create(value.ViewType) is var createdView && createdView != default)
                    {
                        presenter.Bind(createdView as IUiView, data);
                        CloseUiTypes(presenter);

                        ViewRootBehaviour.Instance.Open(createdView);

                        //PushFocusingPresenter(value).Forget();

                        value.SetState(UiState.Enable);

                        GlobalAPI.App.MainModels.UiStateModel.SetOpen(presenter.GetType());
                    }
                    else
                    {
                        Debug.LogError($"Presenters Create Fail : {typeof(T).Name}");
                    }
                }
            }
        }

        public void Close<T>() where T : class
        {
            if (FindByPresenterType(typeof(T)) is var value && value != default)
                if (value.Presenter.View != default && value.Presenter.View.Opened)
                {
                    CloseUiTypes(value.Presenter);

                    value.Presenter.View.Opened = false;
                    value.SetState(UiState.Disable);

                    ViewRootBehaviour.Instance.Close(value.Presenter.View as MonoBehaviour);

                    Clear(value);

                    GlobalAPI.App.MainModels.UiStateModel.SetClose(value.Presenter.GetType());
                    // Debug.Log($"Presenters.Close: {typeof(T).Name}");
                }
        }

        public void Close(IUiPresenter mainPresenter)
        {
            if (FindByPresenterType(mainPresenter.GetType()) is var value && value != default)
                if (value.Presenter.View != default && value.Presenter.View.Opened)
                {
                    CloseUiTypes(mainPresenter);

                    value.SetState(UiState.Disable);
                    value.Presenter.View.Opened = false;

                    ViewRootBehaviour.Instance.Close(value.Presenter.View as MonoBehaviour);

                    Clear(value);

                    GlobalAPI.App.MainModels.UiStateModel.SetClose(value.Presenter.GetType());
                    // Debug.Log($"Presenters.Close: {mainPresenter.GetType().Name}");
                }
        }

        private void CloseUiTypes(IUiPresenter presenter)
        {
            if (FindByPresenterType(presenter.GetType()) is var value && value != default)
            {
                var view = value.Presenter.View as MonoBehaviour;
                if (view == default) return;

                // OnEscape 로 닫지히지 않은 포커싱 프리젠터들은 여기에서 삭제해준다
                // TODO: Top 에 있는 프리젠터만 닫을 수 있는 경우 Close 순서에 따라 스택이 꼬이는 문제가 있음
                if (_focusingPresenters.FirstOrDefault(t => t.presenter == presenter) is var focusingPresenter !=
                    default)
                {
                    GlobalAPI.App.MainModels.UiStateModel.SetClosedFocusingUi(focusingPresenter.GetType());
                    _focusingPresenters.Remove(focusingPresenter);
                }

                var attribute = view.GetCustomAttribute<UiViewAttribute>();
                if (attribute?.CloseWith != default)
                {
                    var presenterTypes = attribute.CloseWith;
                    foreach (var presenterType in presenterTypes)
                        if (FindByPresenterType(presenterType) is { } findValue)
                            Close(findValue.Presenter);
                }
            }
        }

        public void CloseAll(IEnumerable<IUiPresenter> presenters)
        {
            if (presenters.Count() == 0) return;

            foreach (var presenter in presenters) Close(presenter);
        }

        public bool IsVisible<T>() where T : class
        {
            if (FindByPresenterType(typeof(T)) is { } value) return value.Presenter.View?.Opened ?? false;

            return false;
        }


        public void Clear(bool removeCache)
        {
            _focusingPresenters.Clear();
            _escapablePresenters.Clear();

            var removeViewList = new List<IUiView>();
            foreach (var value in _presenters.Values)
            {
                var skipRelease = value.Presenter.GetType().GetCustomAttribute<SkipReleaseAttribute>() != default;
                if (skipRelease) continue;

                if (value.Presenter.View != default && value.Presenter.View.Loaded)
                {
                    Close(value.Presenter);
                    removeViewList.Add(value.Presenter.View);
                    value.SetState(UiState.Release);
                }
            }

            foreach (var value in removeViewList)
                if (value != null)
                    ViewRootBehaviour.Instance.Destroy(value as MonoBehaviour, removeCache);
                else
                    Debug.LogWarning("Fail Delete View!!");

            // Debug.Log($"Presenters Released");
        }

        private async UniTaskVoid PushFocusingPresenter(UiEntity value)
        {
            if (value.Presenter.View.Opened == false) return;

            await UniTask.NextFrame();

            if (value.Presenter.View == null || value.Presenter.View.Opened == false) return;

            if (ViewRootBehaviour.Instance != default &&
                ViewRootBehaviour.Instance.IsRequireFocusing(value.Presenter.View.GetType(), out var focusState))
                if (_focusingPresenters.FirstOrDefault(t => t.presenter == value.Presenter) == default)
                {
                    _focusingPresenters.Insert(0, (value.Presenter, focusState));

                    GlobalAPI.App.MainModels.UiStateModel.SetOpenFocusingUi(value.Presenter.GetType());
                }

            if (value.Presenter is IEscapable escapablePresenter)
                if (_escapablePresenters.FirstOrDefault(t => t == escapablePresenter) == default)
                    _escapablePresenters.Add(escapablePresenter);
        }


        private void Clear(UiEntity uiEntity)
        {
            if (uiEntity.Presenter.View?.Opened == true) return;

            if (_focusingPresenters.Count > 0)
            {
                var peekedPresenter = _focusingPresenters[0];
                if (peekedPresenter.presenter == uiEntity.Presenter) _focusingPresenters.RemoveAt(0);
            }
        }

        #region UI 관련 Presenter 정의

        public TestWithDataPresenter TestWithDataPresenter { get; private set; }
        public TestPresenter TestPresenter { get; private set; }

        #endregion
    }
}