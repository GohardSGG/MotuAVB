// MOTU AVB 插件实现，负责整体通信
namespace Loupedeck.MotuAVBPlugin
{
    using System;
    using System.Net;          // WebClient类型
    using System.Threading.Tasks;
    using System.Collections.Generic; // 新增：用于字典
    using Newtonsoft.Json.Linq; // JSON解析类型
    using Loupedeck;

    public class MotuAVBPlugin : Plugin
    {
        // 单例实例，供其他类访问插件功能
        public static MotuAVBPlugin Instance { get; private set; }

        // 设备管理器，负责UID和IP管理
        private DeviceManager _deviceManager;

        // Loupedeck必需配置
        public override bool UsesApplicationApiOnly => true;
        public override bool HasNoApplication => true;

        public MotuAVBPlugin()
        {
            Instance = this;
            PluginLog.Init(this.Log);       // 初始化日志系统
            PluginResources.Init(this.Assembly); // 初始化资源文件

            _deviceManager = new DeviceManager(); // 创建设备管理实例
        }

        // 静态方法：向MOTU设备发起API请求
        public static JObject GetDatastore(string path)
        {
            try
            {
                using (var client = new WebClient()) // 使用WebClient发起HTTP请求
                {
                    var url = $"http://{DeviceManager.CurrentDeviceIP}/datastore/{path}";
                    var response = client.DownloadString(url); // 同步获取数据
                    return JObject.Parse(response); // 解析JSON响应
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error($"API请求失败: {ex.Message}");
                return null;
            }
        }

        // 插件生命周期方法
        public override void Load() => PluginLog.Info("Motu AVB插件已加载");
        public override void Unload() => PluginLog.Info("Motu AVB插件已卸载");
    }

    // 设备管理类，负责UID和IP的状态管理
    public class DeviceManager
    {
        public static string MainDeviceIP = "192.168.1.98"; // 主设备IP地址
        private static string _currentUID = "0001f2fffe001e53";  // 当前选中设备UID
        private static string _currentDeviceIP = "192.168.1.98"; // 当前设备IP地址

        // 设备UID到IP的映射字典
        private static Dictionary<string, string> _deviceIPMap = new Dictionary<string, string>();

        // 当前设备UID属性，设置时自动更新IP
        public static string CurrentUID
        {
            get { return _currentUID; }
            set
            {
                _currentUID = value;
                // 当UID变更时更新对应IP
                UpdateCurrentDeviceIP();
            }
        }

        // 当前设备IP属性
        public static string CurrentDeviceIP
        {
            get { return _currentDeviceIP; }
            private set { _currentDeviceIP = value; }
        }

        // 构造函数
        public DeviceManager()
        {
            // 初始化时刷新IP映射
            Task.Run(async () => await RefreshDeviceIPs());
        }

        // 刷新所有设备的IP映射
        public static async Task RefreshDeviceIPs()
        {
            try
            {
                // 清空现有映射
                _deviceIPMap.Clear();

                // 首先获取所有可用UID
                var uids = await GetAvailableUIDs();

                // 对每个UID获取对应IP
                foreach (var uid in uids)
                {
                    try
                    {
                        var ipUrl = $"avb/{uid}/url";
                        var json = MotuAVBPlugin.GetDatastore(ipUrl);

                        if (json != null && json["value"] != null)
                        {
                            // 存储映射关系
                            var ipValue = json["value"].ToString();
                            _deviceIPMap[uid] = ipValue;
                            PluginLog.Info($"设备映射: UID {uid} -> IP {ipValue}");
                        }
                    }
                    catch (Exception ex)
                    {
                        PluginLog.Error($"获取设备IP失败 (UID: {uid}): {ex.Message}");
                    }
                }

                // 更新当前IP
                UpdateCurrentDeviceIP();
            }
            catch (Exception ex)
            {
                PluginLog.Error($"刷新设备IP映射失败: {ex.Message}");
            }
        }

        // 更新当前设备IP
        private static void UpdateCurrentDeviceIP()
        {
            // 检查映射中是否有该UID对应的IP
            if (_deviceIPMap.ContainsKey(_currentUID))
            {
                CurrentDeviceIP = _deviceIPMap[_currentUID];
                PluginLog.Info($"已切换到设备IP: {CurrentDeviceIP} (UID: {_currentUID})");
            }
            else
            {
                // 如果没有映射，使用主IP
                CurrentDeviceIP = MainDeviceIP;
                PluginLog.Info($"未找到UID映射，使用主IP: {CurrentDeviceIP}");
            }
        }

        // 获取所有可用设备UID列表
        public static async Task<string[]> GetAvailableUIDs()
        {
            try
            {
                using (var client = new WebClient())
                {
                    var url = $"http://{MainDeviceIP}/datastore/avb/devs"; // 查询设备列表
                    var response = await client.DownloadStringTaskAsync(url);
                    var json = JObject.Parse(response);
                    return json["value"].ToString().Split(':'); // 解析冒号分隔的UID列表
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error($"获取可用设备失败: {ex.Message}");
                return new string[0]; // 异常时返回空数组
            }
        }

        // 根据UID获取设备名称
        public static string GetDeviceName(string uid)
        {
            try
            {
                var json = MotuAVBPlugin.GetDatastore($"avb/{uid}/entity_name");
                return json?["value"]?.ToString() ?? "未知设备"; // 空值处理
            }
            catch
            {
                return "未知设备";
            }
        }
    }
}