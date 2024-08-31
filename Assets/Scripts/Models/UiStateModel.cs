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