using Client.Attribute;
using Client.Extention;
using Client.UI.Define;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Client.UI
{
    [UiView("TestPopup")]
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