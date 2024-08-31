using System;

namespace Client.UI.Define
{
    public class UiEntity
    {
        public IUiPresenter Presenter { get; }
        public Type ViewType { get; }
        public Action<UiState> SetState { get; }

        public UiEntity(IUiPresenter presenter, Type viewType, Action<UiState> setState)
        {
            Presenter = presenter;
            ViewType = viewType;
            SetState = setState;
        }
    }
}