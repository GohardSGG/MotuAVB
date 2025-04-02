// 增强版基类：支持主机/设备双模式路径
namespace Loupedeck.MotuAVBPlugin.Base
{
    using System;
    using System.Net;
    using System.Timers;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using Loupedeck;

    public abstract class Set_Button_Base : PluginDynamicCommand
    {
        protected Timer _updateTimer;
        protected string _currentValue;
        protected string _dataPath;
        protected bool _isHostParameter; // 标识是否为宿主参数

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

        // 通用值设置（自动处理host/device路径）
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
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error($"设置值失败：{ex.Message} (路径: {_dataPath}, 值: {newValue})");
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

                // 主机参数指示点
                //if (_isHostParameter)
                //{
                //    bitmap.FillCircle(5, 5, 3, BitmapColor.Orange);
                //}

                return bitmap.ToImage();
            }
        }

        public void Dispose() => _updateTimer?.Dispose();
    }
}