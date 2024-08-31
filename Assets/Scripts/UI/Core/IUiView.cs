namespace Client.UI.Define
{
    public interface IUiView
    {
        // view 로드 여부
        bool Loaded { get; set; }

        // view open 중인지 여부
        bool Opened { get; set; }
    }
}