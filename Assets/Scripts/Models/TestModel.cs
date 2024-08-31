using Client.UI.Define;
using UniRx;

namespace UI.Models
{
    public class TestModel : IUiModel
    {
        public readonly IntReactiveProperty TestValue0 = new();
        public readonly BoolReactiveProperty TestValue1 = new();
        public readonly StringReactiveProperty TestValue2 = new();


        public void OnInitialize()
        {
        }

        public void OnRelease()
        {
        }


        public void OnSceneChange()
        {
        }

        public void SetTestValue0(int value)
        {
            TestValue0.Value = value;
        }

        public void SetTestValue1(bool value)
        {
            TestValue1.Value = value;
        }

        public void SetTestValue2(string value)
        {
            TestValue2.Value = value;
        }
    }
}