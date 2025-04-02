// 增强版基类：支持主机/设备双模式路径
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
        protected bool _isHostParameter; // 新增：标识是否为宿主参数

        protected Set_Button_Base(
            string displayName,
            string description,
            string groupName,
            string dataPath,
            bool isHostParam = false,
            int updateInterval = 300)
            : base(displayName, description, groupName)
        {
            _dataPath = dataPath;
            _isHostParameter = isHostParam;

            _updateTimer = new Timer(updateInterval);
            _updateTimer.Elapsed += async (s, e) => await UpdateValue();
            _updateTimer.Start();
        }

        // 通用值获取（自动处理host/device路径）
        protected async Task<string> GetValue()
        {
            try
            {
                var prefix = _isHostParameter ? "host/win" : $"avb/{DeviceManager.CurrentUID}";
                var url = $"http://{DeviceManager.CurrentDeviceIP}/datastore/{prefix}/{_dataPath}";

                using (var client = new WebClient())
                {
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

        // 通用值设置（自动处理host/device路径）
        protected async Task SetValue(string newValue)
        {
            try
            {
                var prefix = _isHostParameter ? "host/win" : $"avb/{DeviceManager.CurrentUID}";
                var url = $"http://{DeviceManager.CurrentDeviceIP}/datastore/{prefix}/{_dataPath}";

                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    await client.UploadStringTaskAsync(url, "PATCH", $"json={{\"value\":{newValue}}}");
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error($"设置值失败：{ex.Message}");
            }
        }

        protected async Task UpdateValue()
        {
            var newValue = await GetValue();
            if (_currentValue != newValue)
            {
                _currentValue = newValue;
                ActionImageChanged();
            }
        }

        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
        {
            using (var bitmap = new BitmapBuilder(imageSize))
            {
                //bitmap.Clear(_currentValue == "ERR" ? BitmapColor.Red : BitmapColor.Black);
                bitmap.DrawText(_currentValue, fontSize: 24);
                return bitmap.ToImage();
            }
        }

        public void Dispose() => _updateTimer?.Dispose();
    }
}