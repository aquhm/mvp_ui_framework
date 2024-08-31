using System;
using Client.Data;

namespace Client.Attribute
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class UiViewAttribute : System.Attribute
    {
        public UiViewAttribute(string prefabPath, EUILayer layer = EUILayer.Hud,
            int order = int.MaxValue, bool canReopen = false, Type[] types = default,
            EUIFocusState focusState = EUIFocusState.None)
        {
            PrefabPath = prefabPath;
            Layer = layer;
            Order = order;
            CanReopen = canReopen;
            CloseWith = types;
            FocusState = focusState;
        }

        // Ui Attach될 레이어 위치 설정
        public EUILayer Layer { get; }
        
        // 로드할 프리팹 이름 설정(Assets/Resources/UI/Prefabs에 있는 파일명으로 로드)
        public string PrefabPath { get; }
        
        // 이미 열린 상태에서 UI 오픈 시도시 Close 후 다시 OPen이 필요한 UI 타입시 설정.
        public bool CanReopen { get; }
        
        // Layer에 배치된 Ui Gameobject 간의 Order를 설정(값이 클 수록 나중에 그려지도록 위치 조정)
        public int Order { get; }

        // Ui 종료시 같이 close될 Ui Type 설정(IMainPresneter 타입으로 설정 해야 함)
        public Type[] CloseWith { get; set; }

        // Ui가 오픈시 Focus 타입 지정(Fullscreen, Modal, None)
        public EUIFocusState FocusState { get; }

    }
}