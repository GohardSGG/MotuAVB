// 基础按钮类：集成通用数据访问方法
namespace Loupedeck.MotuAVBPlugin.Base
{
    using System;
    using System.Net;
    using System.Timers;
    using Newtonsoft.Json.Linq;
    using Loupedeck;

    public abstract class Set_Button_Base : PluginDynamicCommand
    {
        protected Timer _updateTimer;
        protected string _currentValue;
        protected string _dataPath;
        protected string[] _presetValues;

        protected Set_Button_Base(
            string displayName,
            string description,
            string groupName,
            string dataPath,
            int updateInterval = 2000,
            string[] presetValues = null)
            : base(displayName, description, groupName)
        {
            _dataPath = dataPath;
            _presetValues = presetValues;

            // 初始化定时更新
            _updateTimer = new Timer(updateInterval);
            _updateTimer.Elapsed += async (s, e) => await UpdateValue();
            _updateTimer.Start();

            // 默认点击切换预设值
            if (_presetValues != null)
                this.AddParameter("toggle", "切换值", "基本操作");
        }

        // 通用值获取方法
        protected async Task<string> GetValue()
        {
            try
            {
                using (var client = new WebClient())
                {
                    var url = $"http://{DeviceManager.CurrentDeviceIP}/datastore/avb/{DeviceManager.CurrentUID}/{_dataPath}";
                    var response = await client.DownloadStringTaskAsync(url);
                    return JObject.Parse(response)["value"].ToString();
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error($"获取值失败：{ex.Message}");
                return "ERR";
            }
        }

        // 通用值设置方法
        protected async Task SetValue(string newValue)
        {
            try
            {
                using (var client = new WebClient())
                {
                    var url = $"http://{DeviceManager.CurrentDeviceIP}/datastore/avb/{DeviceManager.CurrentUID}/{_dataPath}";
                    client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    await client.UploadStringTaskAsync(url, "PATCH", $"json={{\"value\":{newValue}}}");
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error($"设置值失败：{ex.Message}");
            }
        }

        // 自动更新当前值
        protected async Task UpdateValue()
        {
            var newValue = await GetValue();
            if (_currentValue != newValue)
            {
                _currentValue = newValue;
                ActionImageChanged();
            }
        }

        // 默认点击切换预设值
        protected override void RunCommand(string actionParameter)
        {
            if (_presetValues == null)
                return;

            var currentIndex = Array.IndexOf(_presetValues, _currentValue);
            var newIndex = (currentIndex + 1) % _presetValues.Length;
            _ = SetValue(_presetValues[newIndex]);
        }

        // 基础显示模板
        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
        {
            using (var bitmap = new BitmapBuilder(imageSize))
            {
                bitmap.Clear(BitmapColor.Black);
                bitmap.DrawText(_currentValue, fontSize: 24);
                return bitmap.ToImage();
            }
        }

        public void Dispose() => _updateTimer?.Dispose();
    }
}