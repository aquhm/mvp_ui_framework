namespace Client.UI.Define
{
    public interface IUiModel
    {
        // 앱 실행 최초 1회 실행
        void OnInitialize();
        
        void OnRelease();
    }
}