// 安全偏移量旋钮控制实现
namespace Loupedeck.MotuAVBPlugin.Dials
{
    using Loupedeck.MotuAVBPlugin.Base;

    public class Safety_Offset_Dial : Set_Dial_Base
    {
        // 预设安全偏移量值
        private static readonly string[] OffsetPresets = {
            "16", "24", "32", "48", "64", "128", "256"
        };

        public Safety_Offset_Dial()
            : base(
                displayName: "Safety Offset Dial",
                description: "旋转调整安全偏移量，按下重置",
                groupName: "Setup",
                dataPath: "current_safety_offset_1x", // 精确API路径
                defaultValue: "48", // 默认值设为32
                dialType: DialActivationManager.SAFETY_OFFSET_DIAL, // 旋钮类型
                isHostParam: true) // 标记为宿主参数
        {
        }

        // 获取值在预设列表中的索引
        protected override int GetValueIndex(string value)
        {
            for (int i = 0; i < OffsetPresets.Length; i++)
            {
                if (OffsetPresets[i] == value)
                    return i;
            }
            return -1; // 未找到匹配值
        }

        // 根据索引获取预设值
        protected override string GetValueByIndex(int index)
        {
            if (index >= 0 && index < OffsetPresets.Length)
                return OffsetPresets[index];
            return _defaultValue;
        }

        // 获取预设总数
        protected override int GetPresetsCount()
        {
            return OffsetPresets.Length;
        }

        // 自定义显示
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
                //bitmap.DrawText("安全偏移", y: 40, fontSize: 12, color: BitmapColor.White);

                // 主机参数指示点
                //bitmap.FillCircle(5, 5, 3, BitmapColor.Orange);

                return bitmap.ToImage();
            }
        }
    }
}