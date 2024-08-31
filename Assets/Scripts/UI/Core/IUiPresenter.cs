using System;

namespace Client.UI.Define
{
    // UI 상태 정의
    public enum UiState
    {
        /// <summary> UI 생성 전 또는 Release된 이후 시점 상태 </summary>
        None,

        /// <summary> UI Enable 시점 상태 </summary>
        Enable,

        /// <summary> UI Dislable 시점 상태 </summary>
        Disable,

        /// <summary> UI Release 시점 상태 </summary>
        Release,
    }
    
    // UI 라이크 사이클 이벤트 시점에 수행할 액션 리스트
    public struct UiLifecycleEvents
    {
        public static UiLifecycleEvents Empty = new();

        // OnEnable 시점에 수행할 로직
        public Action OnEnable;

        // OnDisable 시점에 수행할 로직
        public Action OnDisable;

        // Release 시점에 수행할 로직
        public Action OnRelease;

        // Esc 키 눌림 시점에 수행할 로직
        public Action OnEscape;
    }

    
    // 추가 데이타를 전달해야하는 경우
    public interface IPresenterData
    {
    }
    
    public interface IUiPresenter
    {
        public IUiView View { get; }
        public void Bind(IUiView view, IPresenterData bindData);
    }
    
    // Esc 키 눌림시 로직 처리
    public interface IEscapable
    {
        void OnEscape();
        bool IsEnableEscape { get; }
        bool HasOnEscape { get; }
    }
}