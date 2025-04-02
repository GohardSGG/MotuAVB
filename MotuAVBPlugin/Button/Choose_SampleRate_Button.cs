// 采样率选择按钮实现
namespace Loupedeck.MotuAVBPlugin.Buttons
{
    using Loupedeck.MotuAVBPlugin.Base;

    public class Choose_SampleRate_Button : Choose_Button_Base
    {
        public Choose_SampleRate_Button()
            : base(
                displayName: "Sample Rate",
                description: "激活/禁用采样率旋钮",
                groupName: "Choose",
                dialType: DialActivationManager.SAMPLE_RATE_DIAL)
        {
        }
    }
}
