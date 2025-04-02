// 通用设置旋钮基类，支持增减值和重置功能
namespace Loupedeck.MotuAVBPlugin.Base
{
    using System;
    using System.Net;
    using System.Timers;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using Loupedeck;

    public abstract class Set_Dial_Base : PluginDynamicAdjustment
    {
        protected Timer _updateTimer;
        protected string _currentValue;
        protected string _dataPath;
        protected bool _isHostParameter;
        protected string _defaultValue; // 重置时使用的默认值
        protected string _dialType;     // 旋钮类型标识，用于激活状态检查

        protected Set_Dial_Base(
            string displayName,
            string description,
            string groupName,
            string dataPath,
            string defaultValue,
            string dialType,
            bool isHostParam = false,
            int updateInterval = 300)
            : base(displayName, description, groupName, hasReset: true)
        {
            _dataPath = dataPath;
            _isHostParameter = isHostParam;
            _defaultValue = defaultValue;
            _dialType = dialType;

            _updateTimer = new Timer(updateInterval);
            _updateTimer.Elapsed += async (s, e) => await UpdateValue();
            _updateTimer.Start();

            // 加载初始值
            Task.Run(async () => await UpdateValue());

            // 添加参数
            this.AddParameter($"{groupName}.{displayName}", displayName, groupName);
        }

        // 应用调整（旋钮旋转）
        protected override void ApplyAdjustment(string actionParameter, int ticks)
        {
            if (ticks == 0 || string.IsNullOrEmpty(_currentValue))
                return;

            // 检查旋钮是否被激活，如未激活则忽略旋转
            if (!DialActivationManager.IsDialActive(_dialType))
            {
                PluginLog.Info($"旋钮 {_dialType} 未激活，忽略旋转操作");
                return;
            }

            // 获取当前值的预设索引
            int currentIndex = GetValueIndex(_currentValue);
            if (currentIndex < 0)
                return;

            // 计算新索引
            int newIndex = ticks > 0
                ? Math.Min(currentIndex + 1, GetPresetsCount() - 1)
                : Math.Max(currentIndex - 1, 0);

            // 如果索引有变化，设置新值
            if (newIndex != currentIndex)
            {
                string newValue = GetValueByIndex(newIndex);
                _ = SetValue(newValue);
            }
        }

        // 重置命令（按下旋钮）- 按下始终生效，不受激活状态影响
        protected override void RunCommand(string actionParameter)
        {
            // 重置为默认值
            _ = SetValue(_defaultValue);
        }

        // 通用值获取
        protected async Task<string> GetValue()
        {
            try
            {
                string url;

                if (_isHostParameter)
                {
                    // 对于主机参数，使用当前设备的IP地址
                    url = $"http://{DeviceManager.CurrentDeviceIP}/datastore/host/win/{_dataPath}";
                }
                else
                {
                    // 对于设备参数，使用主IP地址但包含设备UID
                    url = $"http://{DeviceManager.MainDeviceIP}/datastore/avb/{DeviceManager.CurrentUID}/{_dataPath}";
                }

                using (var client = new WebClient())
                {
                    var response = await client.DownloadStringTaskAsync(url);
                    var json = JObject.Parse(response);
                    return json["value"].ToString();
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error($"获取值失败：{ex.Message} (路径: {_dataPath})");
                return "ERR";
            }
        }

        // 通用值设置
        protected async Task SetValue(string newValue)
        {
            try
            {
                string url;

                if (_isHostParameter)
                {
                    // 对于主机参数，使用当前设备的IP地址
                    url = $"http://{DeviceManager.CurrentDeviceIP}/datastore/host/win/{_dataPath}";
                }
                else
                {
                    // 对于设备参数，使用主IP地址但包含设备UID
                    url = $"http://{DeviceManager.MainDeviceIP}/datastore/avb/{DeviceManager.CurrentUID}/{_dataPath}";
                }

                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    await client.UploadStringTaskAsync(url, "PATCH", $"json={{\"value\":{newValue}}}");

                    // 更新当前值
                    _currentValue = newValue;
                    AdjustmentValueChanged();
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error($"设置值失败：{ex.Message} (路径: {_dataPath}, 值: {newValue})");
            }
        }

        // 更新当前值
        protected async Task UpdateValue()
        {
            var newValue = await GetValue();
            if (_currentValue != newValue)
            {
                _currentValue = newValue;
                AdjustmentValueChanged();
            }
        }

        // 获取调整显示图像
        protected override BitmapImage GetAdjustmentImage(string actionParameter, PluginImageSize imageSize)
        {
            using (var bitmap = new BitmapBuilder(imageSize))
            {
                // 基础背景颜色
                bool isError = _currentValue == "ERR";
                bool isActive = DialActivationManager.IsDialActive(_dialType);

                // 根据状态选择背景颜色：出错为红色，激活为蓝色，未激活为黑色
                //BitmapColor bgColor = isError ? BitmapColor.Red :
                //                     isActive ? new BitmapColor(0, 0, 128) : BitmapColor.Black;

                bitmap.Clear(BitmapColor.Black);

                // 显示当前值
                bitmap.DrawText(_currentValue ?? "...", fontSize: 20, color: BitmapColor.White);

                // 主机参数指示点
                //if (_isHostParameter)
                //{
                //    bitmap.FillCircle(5, 5, 3, BitmapColor.Orange);
                //}

                return bitmap.ToImage();
            }
        }

        // 抽象方法：获取值在预设列表中的索引
        protected abstract int GetValueIndex(string value);

        // 抽象方法：根据索引获取预设值
        protected abstract string GetValueByIndex(int index);

        // 抽象方法：获取预设总数
        protected abstract int GetPresetsCount();

        // 释放资源
        public void Dispose() => _updateTimer?.Dispose();
    }
}