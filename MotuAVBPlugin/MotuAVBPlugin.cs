// MOTU AVB ������࣬����ȫ�ֳ�ʼ����HTTPͨ��
namespace Loupedeck.MotuAVBPlugin
{
    using System;
    using System.Net;          // WebClient����
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq; // JSON��������
    using Loupedeck;

    public class MotuAVBPlugin : Plugin
    {
        // ����ʵ��������������ʲ������
        public static MotuAVBPlugin Instance { get; private set; }

        // �豸������������UID��IP����
        private DeviceManager _deviceManager;

        // Loupedeck��Ҫ����
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
        public static string CurrentDeviceIP = "192.168.1.98"; // ��ǰ�豸IP��ַ
        public static string CurrentUID = "0001f2fffe001e53";  // ��ǰѡ���豸UID

        // ��ȡ���п����豸UID�б�
        public static async Task<string[]> GetAvailableUIDs()
        {
            try
            {
                var json = MotuAVBPlugin.GetDatastore("avb/devs"); // ��ѯ�豸�б�
                return json["value"].ToString().Split(':'); // ����ð�ŷָ���UID�б�
            }
            catch
            {
                return new string[0]; // �쳣ʱ���ؿ�����
            }
        }

        // ����UID��ȡ�豸����
        public static string GetDeviceName(string uid)
        {
            var json = MotuAVBPlugin.GetDatastore($"avb/{uid}/entity_name");
            return json?["value"]?.ToString() ?? "δ֪�豸"; // ��ֵ����
        }
    }
}