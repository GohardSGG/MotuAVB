// 缓冲区大小按钮
namespace Loupedeck.MotuAVBPlugin.Buttons
{
    using Loupedeck.MotuAVBPlugin.Base;

    public class BufferSize_Button : Set_Button_Base
    {
        // 预设缓冲区大小（根据API支持值调整）
        private static readonly string[] BufferPresets = {
            "64",
            "128",
            "256",
            "512"
        };

        public BufferSize_Button()
            : base(
                displayName: "缓冲区",
                description: "点击切换缓冲区大小",
                groupName: "设备控制",
                dataPath: "cfg/0/buffer_size",
                presetValues: BufferPresets)
        {
        }

        // 自定义显示格式
        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
        {
            using (var bitmap = new BitmapBuilder(imageSize))
            {
                bitmap.Clear(BitmapColor.Black);

                // 显示数值和单位
                bitmap.DrawText(
                    text: $"{_currentValue} ",
                    fontSize: 18,
                    color: BitmapColor.White);

                return bitmap.ToImage();
            }
        }
    }
}