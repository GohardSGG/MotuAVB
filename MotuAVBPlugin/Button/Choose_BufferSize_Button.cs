// 缓冲区大小选择按钮实现
namespace Loupedeck.MotuAVBPlugin.Buttons
{
    using Loupedeck.MotuAVBPlugin.Base;

    public class Choose_BufferSize_Button : Choose_Button_Base
    {
        public Choose_BufferSize_Button()
            : base(
                displayName: "Buffer Size",
                description: "激活/禁用缓冲区大小旋钮",
                groupName: "Choose",
                dialType: DialActivationManager.BUFFER_SIZE_DIAL)
        {
        }
    }
}
