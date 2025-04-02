// 更新采样率按钮（设备参数示例）
namespace Loupedeck.MotuAVBPlugin.Buttons
{
    using Loupedeck.MotuAVBPlugin.Base;

    public class SampleRate_Button : Set_Button_Base
    {
        private static readonly string[] RatePresets = {
            "44100", "48000", "88200", "96000", "176000", "192000"
        };

        public SampleRate_Button()
            : base(
                displayName: "采样率",
                description: "点击切换设备采样率",
                groupName: "Setup",
                dataPath: "cfg/0/current_sampling_rate") // 设备参数路径
        {
            // 添加预设值切换支持
            this.AddParameter("toggle", "切换采样率", "基本操作");
        }

        protected override void RunCommand(string actionParameter)
        {
            if (int.TryParse(_currentValue, out int currentRate))
            {
                var nextIndex = (Array.IndexOf(RatePresets, currentRate.ToString()) + 1);
                _ = SetValue(RatePresets[nextIndex % RatePresets.Length]);
            }
        }

        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
        {
            using (var bitmap = new BitmapBuilder(imageSize))
            {
                bitmap.Clear(BitmapColor.Black);
                bitmap.DrawText($"{_currentValue}", fontSize: 22);
                return bitmap.ToImage();
            }
        }
    }
}