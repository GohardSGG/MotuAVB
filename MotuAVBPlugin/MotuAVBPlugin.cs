// MOTU AVB ���ʵ�֣���������ͨ��
namespace Loupedeck.MotuAVBPlugin
{
    using System;
    using System.Net;          // WebClient����
    using System.Threading.Tasks;
    using System.Collections.Generic; // �����������ֵ�
    using Newtonsoft.Json.Linq; // JSON��������
    using Loupedeck;

    public class MotuAVBPlugin : Plugin
    {
        // ����ʵ��������������ʲ������
        public static MotuAVBPlugin Instance { get; private set; }

        // �豸������������UID��IP����
        private DeviceManager _deviceManager;

        // Loupedeck��������
        public override bool UsesApplicationApiOnly => true;
        public override bool HasNoApplication => true;

        public MotuAVBPlugin()
        {
            Instance = this;
            PluginLog.Init(this.Log);       // ��ʼ����־ϵͳ
            PluginResources.Init(this.Assembly); // ��ʼ����Դ�ļ�

            _deviceManager = new DeviceManager(); // �����豸����ʵ��
        }

        // ��̬��������MOTU�豸����API����
        public static JObject GetDatastore(string path)
        {
            try
            {
                using (var client = new WebClient()) // ʹ��WebClient����HTTP����
                {
                    var url = $"http://{DeviceManager.CurrentDeviceIP}/datastore/{path}";
                    var response = client.DownloadString(url); // ͬ����ȡ����
                    return JObject.Parse(response); // ����JSON��Ӧ
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error($"API����ʧ��: {ex.Message}");
                return null;
            }
        }

        // ����������ڷ���
        public override void Load() => PluginLog.Info("Motu AVB����Ѽ���");
        public override void Unload() => PluginLog.Info("Motu AVB�����ж��");
    }

    // �豸�����࣬����UID��IP��״̬����
    public class DeviceManager
    {
        public static string MainDeviceIP = "192.168.1.98"; // ���豸IP��ַ
        private static string _currentUID = "0001f2fffe001e53";  // ��ǰѡ���豸UID
        private static string _currentDeviceIP = "192.168.1.98"; // ��ǰ�豸IP��ַ

        // �豸UID��IP��ӳ���ֵ�
        private static Dictionary<string, string> _deviceIPMap = new Dictionary<string, string>();

        // ��ǰ�豸UID���ԣ�����ʱ�Զ�����IP
        public static string CurrentUID
        {
            get { return _currentUID; }
            set
            {
                _currentUID = value;
                // ��UID���ʱ���¶�ӦIP
                UpdateCurrentDeviceIP();
            }
        }

        // ��ǰ�豸IP����
        public static string CurrentDeviceIP
        {
            get { return _currentDeviceIP; }
            private set { _currentDeviceIP = value; }
        }

        // ���캯��
        public DeviceManager()
        {
            // ��ʼ��ʱˢ��IPӳ��
            Task.Run(async () => await RefreshDeviceIPs());
        }

        // ˢ�������豸��IPӳ��
        public static async Task RefreshDeviceIPs()
        {
            try
            {
                // �������ӳ��
                _deviceIPMap.Clear();

                // ���Ȼ�ȡ���п���UID
                var uids = await GetAvailableUIDs();

                // ��ÿ��UID��ȡ��ӦIP
                foreach (var uid in uids)
                {
                    try
                    {
                        var ipUrl = $"avb/{uid}/url";
                        var json = MotuAVBPlugin.GetDatastore(ipUrl);

                        if (json != null && json["value"] != null)
                        {
                            // �洢ӳ���ϵ
                            var ipValue = json["value"].ToString();
                            _deviceIPMap[uid] = ipValue;
                            PluginLog.Info($"�豸ӳ��: UID {uid} -> IP {ipValue}");
                        }
                    }
                    catch (Exception ex)
                    {
                        PluginLog.Error($"��ȡ�豸IPʧ�� (UID: {uid}): {ex.Message}");
                    }
                }

                // ���µ�ǰIP
                UpdateCurrentDeviceIP();
            }
            catch (Exception ex)
            {
                PluginLog.Error($"ˢ���豸IPӳ��ʧ��: {ex.Message}");
            }
        }

        // ���µ�ǰ�豸IP
        private static void UpdateCurrentDeviceIP()
        {
            // ���ӳ�����Ƿ��и�UID��Ӧ��IP
            if (_deviceIPMap.ContainsKey(_currentUID))
            {
                CurrentDeviceIP = _deviceIPMap[_currentUID];
                PluginLog.Info($"���л����豸IP: {CurrentDeviceIP} (UID: {_currentUID})");
            }
            else
            {
                // ���û��ӳ�䣬ʹ����IP
                CurrentDeviceIP = MainDeviceIP;
                PluginLog.Info($"δ�ҵ�UIDӳ�䣬ʹ����IP: {CurrentDeviceIP}");
            }
        }

        // ��ȡ���п����豸UID�б�
        public static async Task<string[]> GetAvailableUIDs()
        {
            try
            {
                using (var client = new WebClient())
                {
                    var url = $"http://{MainDeviceIP}/datastore/avb/devs"; // ��ѯ�豸�б�
                    var response = await client.DownloadStringTaskAsync(url);
                    var json = JObject.Parse(response);
                    return json["value"].ToString().Split(':'); // ����ð�ŷָ���UID�б�
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error($"��ȡ�����豸ʧ��: {ex.Message}");
                return new string[0]; // �쳣ʱ���ؿ�����
            }
        }

        // ����UID��ȡ�豸����
        public static string GetDeviceName(string uid)
        {
            try
            {
                var json = MotuAVBPlugin.GetDatastore($"avb/{uid}/entity_name");
                return json?["value"]?.ToString() ?? "δ֪�豸"; // ��ֵ����
            }
            catch
            {
                return "δ֪�豸";
            }
        }
    }
}