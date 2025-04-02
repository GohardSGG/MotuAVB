// 精确匹配主机缓冲区API的按钮实现
namespace Loupedeck.MotuAVBPlugin.Buttons
{
    using Loupedeck.MotuAVBPlugin.Base;

    public class BufferSize_Button : Set_Button_Base
    {
        // 预设缓冲区大小（根据实际设备支持值）
        private static readonly string[] BufferPresets = {
            "64", "128", "256", "512", "1024"
        };
        private int _currentPresetIndex;

        public BufferSize_Button()
            : base(
                displayName: "缓冲区大小",
                description: "点击切换主机缓冲区",
                groupName: "主机设置",
                dataPath: "current_buffer_size_1x", // 精确API路径
                isHostParam: true) // 标记为宿主参数
        {
            // 初始化时匹配当前值
            _ = InitializePresetIndexAsync();
        }

        private async Task InitializePresetIndexAsync()
        {
            var current = await GetValue();
            _currentPresetIndex = Array.IndexOf(BufferPresets, current);
            if (_currentPresetIndex < 0)
                _currentPresetIndex = 0;
        }

        protected override void RunCommand(string actionParameter)
        {
            _currentPresetIndex = (_currentPresetIndex + 1) % BufferPresets.Length;
            _ = SetValue(BufferPresets[_currentPresetIndex]);
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

                return bitmap.ToImage();
            }
        }
    }
}