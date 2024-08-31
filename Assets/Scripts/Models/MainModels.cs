using System.Collections.Generic;
using Client.UI.Define;
using UI.Models;
using UniRx;
using UnityEngine;

namespace Client.UI.Model
{
    public class MainModels
    {
        private readonly CompositeDisposable _disposable = new();
        public readonly TestModel TestModel = new();

        public readonly UiStateModel UiStateModel = new();

        public IEnumerable<IUiModel> All
        {
            get
            {
                yield return UiStateModel;
                yield return TestModel;
            }
        }

        public void Init()
        {
            foreach (var model in All) model.OnInitialize();

            Debug.Log("MainModels Initialize");
        }

        public void Release()
        {
            _disposable.Clear();

            foreach (var model in All) model.OnRelease();
        }

        public void Reset()
        {
            foreach (var model in All)
                if (model as IResetableModel is var resetableModel)
                    resetableModel?.OnSceneChange();

            Debug.Log("MainModels Reset");
        }
    }
}