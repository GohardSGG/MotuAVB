// 采样率旋钮控制实现
namespace Loupedeck.MotuAVBPlugin.Dials
{
    using Loupedeck.MotuAVBPlugin.Base;

    public class SampleRate_Dial : Set_Dial_Base
    {
        // 预设采样率值
        private static readonly string[] RatePresets = {
            "44100", "48000", "88200", "96000", "176000", "192000"
        };

        public SampleRate_Dial()
            : base(
                displayName: "SampleRate Dial",
                description: "旋转调整采样率，按下重置",
                groupName: "Setup",
                dataPath: "cfg/0/current_sampling_rate", // 设备参数路径
                defaultValue: "48000", // 默认值设为48kHz
                dialType: DialActivationManager.SAMPLE_RATE_DIAL, // 旋钮类型
                isHostParam: false) // 设备参数
        {
        }

        // 获取值在预设列表中的索引
        protected override int GetValueIndex(string value)
        {
            for (int i = 0; i < RatePresets.Length; i++)
            {
                if (RatePresets[i] == value)
                    return i;
            }
            return -1; // 未找到匹配值
        }

        // 根据索引获取预设值
        protected override string GetValueByIndex(int index)
        {
            if (index >= 0 && index < RatePresets.Length)
                return RatePresets[index];
            return _defaultValue;
        }

        // 获取预设总数
        protected override int GetPresetsCount()
        {
            return RatePresets.Length;
        }

        // 自定义显示
        protected override BitmapImage GetAdjustmentImage(string actionParameter, PluginImageSize imageSize)
        {
            using (var bitmap = new BitmapBuilder(imageSize))
            {
                // 根据激活状态选择背景颜色
                //bool isActive = DialActivationManager.IsDialActive(_dialType);
                bitmap.Clear(BitmapColor.Black);

                // 分析当前值并显示更友好的格式
                if (int.TryParse(_currentValue, out int rate))
                {
                    // 格式化为kHz显示
                    if (rate >= 1000)
                    {
                        double kHz = rate / 1000.0;
                        bitmap.DrawText($"{kHz:F1}k", fontSize: 18, color: BitmapColor.White);
                    }
                    else
                    {
                        bitmap.DrawText($"{rate}Hz", fontSize: 16, color: BitmapColor.White);
                    }
                }
                else
                {
                    // 显示原始值（如出错）
                    bitmap.DrawText($"{_currentValue ?? "..."}", fontSize: 16, color: BitmapColor.White);
                }

                // 标题
                //bitmap.DrawText("采样率", y: 40, fontSize: 12, color: BitmapColor.White);

                return bitmap.ToImage();
            }
        }
    }
}