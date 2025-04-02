// 安全偏移量切换按钮实现
namespace Loupedeck.MotuAVBPlugin.Buttons
{
    using System;
    using System.Threading.Tasks;

    using Loupedeck.MotuAVBPlugin.Base;

    public class Safety_Offset_Button : Set_Button_Base
    {
        // 预设安全偏移量值
        private static readonly string[] OffsetPresets = {
            "16", "24", "32", "48", "64", "128", "256"
        };
        private int _currentPresetIndex;

        public Safety_Offset_Button()
            : base(
                displayName: "安全偏移量",
                description: "点击切换安全偏移量",
                groupName: "Setup",
                dataPath: "current_safety_offset_1x", // 精确API路径
                isHostParam: true) // 标记为宿主参数
        {
            // 初始化时匹配当前值
            _ = InitializePresetIndexAsync();
        }

        private async Task InitializePresetIndexAsync()
        {
            var current = await GetValue();
            _currentPresetIndex = Array.IndexOf(OffsetPresets, current);
            if (_currentPresetIndex < 0)
                _currentPresetIndex = 0;
        }

        protected override void RunCommand(string actionParameter)
        {
            _currentPresetIndex = (_currentPresetIndex + 1) % OffsetPresets.Length;
            _ = SetValue(OffsetPresets[_currentPresetIndex]);
        }

        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
        {
            using (var bitmap = new BitmapBuilder(imageSize))
            {
                bitmap.Clear(BitmapColor.Black);

                // 主显示
                bitmap.DrawText(
                    text: $"{_currentValue}",
                    fontSize: 20,
                    color: BitmapColor.White);

                // 添加主机参数指示点
                //bitmap.FillCircle(5, 5, 3, BitmapColor.Orange);

                return bitmap.ToImage();
            }
        }
    }
}