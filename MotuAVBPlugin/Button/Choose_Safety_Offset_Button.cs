// 安全偏移量选择按钮实现
namespace Loupedeck.MotuAVBPlugin.Buttons
{
    using Loupedeck.MotuAVBPlugin.Base;

    public class Choose_Safety_Offset_Button : Choose_Button_Base
    {
        public Choose_Safety_Offset_Button()
            : base(
                displayName: "Safety Offset",
                description: "激活/禁用安全偏移量旋钮",
                groupName: "Choose",
                dialType: DialActivationManager.SAFETY_OFFSET_DIAL)
        {
        }
    }
}
