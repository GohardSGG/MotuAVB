// MOTU AVB 插件主类，负责全局初始化和HTTP通信
namespace Loupedeck.MotuAVBPlugin
{
    using System;
    using System.Net;          // WebClient依赖
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq; // JSON解析依赖
    using Loupedeck;

    public class MotuAVBPlugin : Plugin
    {
        // 单例实例，供其他类访问插件功能
        public static MotuAVBPlugin Instance { get; private set; }

        // 设备管理器，负责UID和IP管理
        private DeviceManager _deviceManager;

        // Loupedeck必要配置
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
        public static string CurrentDeviceIP = "192.168.1.98"; // 当前设备IP地址
        public static string CurrentUID = "0001f2fffe001e53";  // 当前选中设备UID

        // 获取所有可用设备UID列表
        public static async Task<string[]> GetAvailableUIDs()
        {
            try
            {
                var json = MotuAVBPlugin.GetDatastore("avb/devs"); // 查询设备列表
                return json["value"].ToString().Split(':'); // 解析冒号分隔的UID列表
            }
            catch
            {
                return new string[0]; // 异常时返回空数组
            }
        }

        // 根据UID获取设备名称
        public static string GetDeviceName(string uid)
        {
            var json = MotuAVBPlugin.GetDatastore($"avb/{uid}/entity_name");
            return json?["value"]?.ToString() ?? "未知设备"; // 空值处理
        }
    }
}