// 采样率按钮：显示并切换预设采样率
namespace Loupedeck.MotuAVBPlugin.Buttons
{
    using Loupedeck.MotuAVBPlugin.Base;

    public class SampleRate_Button : Set_Button_Base
    {
        // 预设采样率值（根据API文档调整）
        private static readonly string[] RatePresets = {
            "44100",
            "48000",
            "88200",
            "96000",
            "176400",
            "192000"
        };

        public SampleRate_Button()
            : base(
                displayName: "采样率",
                description: "点击切换采样率",
                groupName: "设备控制",
                dataPath: "cfg/0/current_sampling_rate",
                presetValues: RatePresets)
        {
        }

        // 自定义显示格式
        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
        {
            using (var bitmap = new BitmapBuilder(imageSize))
            {
                //bitmap.Clear(_currentValue == "ERR" ? BitmapColor.Red : BitmapColor.Black);

                // 显示数值和单位
                bitmap.DrawText(
                    text: $"{_currentValue}",
                    fontSize: 20,
                    color: BitmapColor.White);

                // 显示当前预设位置
                //var index = Array.IndexOf(RatePresets, _currentValue) + 1;
               // if (index > 0)
                //    bitmap.DrawText(
               //         text: $"Preset {index}/{RatePresets.Length}",
               //         y: bitmap.Height - 20,
               //         fontSize: 12,
                //        color: BitmapColor.Gray);

                return bitmap.ToImage();
            }
        }
    }
}