// 缓冲区大小旋钮控制实现
namespace Loupedeck.MotuAVBPlugin.Dials
{
    using Loupedeck.MotuAVBPlugin.Base;

    public class BufferSize_Dial : Set_Dial_Base
    {
        // 预设缓冲区大小（根据实际设备支持值）
        private static readonly string[] BufferPresets = {
            "64", "128", "256", "512", "1024"
        };

        public BufferSize_Dial()
            : base(
                displayName: "BufferSize Dial",
                description: "旋转调整缓冲区大小，按下重置",
                groupName: "Setup",
                dataPath: "current_buffer_size_1x", // 精确API路径
                defaultValue: "512", // 默认值设为256
                dialType: DialActivationManager.BUFFER_SIZE_DIAL, // 旋钮类型
                isHostParam: true) // 标记为宿主参数
        {
        }

        // 获取值在预设列表中的索引
        protected override int GetValueIndex(string value)
        {
            for (int i = 0; i < BufferPresets.Length; i++)
            {
                if (BufferPresets[i] == value)
                    return i;
            }
            return -1; // 未找到匹配值
        }

        // 根据索引获取预设值
        protected override string GetValueByIndex(int index)
        {
            if (index >= 0 && index < BufferPresets.Length)
                return BufferPresets[index];
            return _defaultValue;
        }

        // 获取预设总数
        protected override int GetPresetsCount()
        {
            return BufferPresets.Length;
        }

        // 自定义显示（可选）
        protected override BitmapImage GetAdjustmentImage(string actionParameter, PluginImageSize imageSize)
        {
            using (var bitmap = new BitmapBuilder(imageSize))
            {
                // 根据激活状态选择背景颜色
                //bool isActive = DialActivationManager.IsDialActive(_dialType);
                bitmap.Clear(BitmapColor.Black);

                // 显示当前值
                bitmap.DrawText($"{_currentValue}", fontSize: 20, color: BitmapColor.White);

                // 显示单位或标识
                //bitmap.DrawText("缓冲区", y: 40, fontSize: 12, color: BitmapColor.White);

                // 主机参数指示点
                //bitmap.FillCircle(5, 5, 3, BitmapColor.Orange);

                return bitmap.ToImage();
            }
        }
    }
}