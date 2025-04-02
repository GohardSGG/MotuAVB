// 选择按钮基类，用于激活/禁用对应的旋钮
namespace Loupedeck.MotuAVBPlugin.Base
{
    using System;
    using System.Timers;
    using Loupedeck;

    public abstract class Choose_Button_Base : PluginDynamicCommand
    {
        // 激活状态
        protected bool _isActive = false;
        
        // 旋钮类型标识符
        protected readonly string _dialType;
        
        // 显示名称
        protected readonly string _displayName;

        protected Choose_Button_Base(
            string displayName,
            string description,
            string groupName,
            string dialType)
            : base(displayName, description, groupName)
        {
            _displayName = displayName;
            _dialType = dialType;
            
            // 添加参数
            this.AddParameter("toggle", "切换激活状态", groupName);
        }

        // 命令执行（按钮点击）
        protected override void RunCommand(string actionParameter)
        {
            // 切换激活状态
            _isActive = !_isActive;
            
            // 调用全局状态更新器
            DialActivationManager.SetDialActivationState(_dialType, _isActive);
            
            // 更新按钮图像
            ActionImageChanged();
        }

        // 生成按钮图像
        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
        {
            using (var bitmap = new BitmapBuilder(imageSize))
            {
                // 激活状态：白底黑字，未激活状态：黑底白字
                bitmap.Clear(_isActive ? BitmapColor.White : BitmapColor.Black);
                
                bitmap.DrawText(
                    text: _displayName,
                    fontSize: 19,
                    color: _isActive ? BitmapColor.Black : BitmapColor.White);
                
                return bitmap.ToImage();
            }
        }
    }

    // 静态管理类，处理旋钮激活状态
    public static class DialActivationManager
    {
        // 存储各类型旋钮的激活状态
        private static bool _sampleRateActive = false;
        private static bool _bufferSizeActive = false;
        private static bool _safetyOffsetActive = false;

        // 旋钮类型常量
        public const string SAMPLE_RATE_DIAL = "SAMPLE_RATE_DIAL";
        public const string BUFFER_SIZE_DIAL = "BUFFER_SIZE_DIAL";
        public const string SAFETY_OFFSET_DIAL = "SAFETY_OFFSET_DIAL";

        // 设置旋钮激活状态
        public static void SetDialActivationState(string dialType, bool isActive)
        {
            switch (dialType)
            {
                case SAMPLE_RATE_DIAL:
                    _sampleRateActive = isActive;
                    break;
                case BUFFER_SIZE_DIAL:
                    _bufferSizeActive = isActive;
                    break;
                case SAFETY_OFFSET_DIAL:
                    _safetyOffsetActive = isActive;
                    break;
            }
            
            PluginLog.Info($"旋钮 {dialType} 状态已更新: {(isActive ? "激活" : "禁用")}");
        }

        // 检查旋钮是否激活
        public static bool IsDialActive(string dialType)
        {
            switch (dialType)
            {
                case SAMPLE_RATE_DIAL:
                    return _sampleRateActive;
                case BUFFER_SIZE_DIAL:
                    return _bufferSizeActive;
                case SAFETY_OFFSET_DIAL:
                    return _safetyOffsetActive;
                default:
                    return false;
            }
        }
    }
}
