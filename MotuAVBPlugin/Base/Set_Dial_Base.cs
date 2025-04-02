// ͨ��������ť���֧࣬������ֵ�����ù���
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
        protected string _defaultValue; // ����ʱʹ�õ�Ĭ��ֵ
        protected string _dialType;     // ��ť���ͱ�ʶ�����ڼ���״̬���

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

            // ���س�ʼֵ
            Task.Run(async () => await UpdateValue());

            // ��Ӳ���
            this.AddParameter($"{groupName}.{displayName}", displayName, groupName);
        }

        // Ӧ�õ�������ť��ת��
        protected override void ApplyAdjustment(string actionParameter, int ticks)
        {
            if (ticks == 0 || string.IsNullOrEmpty(_currentValue))
                return;

            // �����ť�Ƿ񱻼����δ�����������ת
            if (!DialActivationManager.IsDialActive(_dialType))
            {
                PluginLog.Info($"��ť {_dialType} δ���������ת����");
                return;
            }

            // ��ȡ��ǰֵ��Ԥ������
            int currentIndex = GetValueIndex(_currentValue);
            if (currentIndex < 0)
                return;

            // ����������
            int newIndex = ticks > 0
                ? Math.Min(currentIndex + 1, GetPresetsCount() - 1)
                : Math.Max(currentIndex - 1, 0);

            // ��������б仯��������ֵ
            if (newIndex != currentIndex)
            {
                string newValue = GetValueByIndex(newIndex);
                _ = SetValue(newValue);
            }
        }

        // �������������ť��- ����ʼ����Ч�����ܼ���״̬Ӱ��
        protected override void RunCommand(string actionParameter)
        {
            // ����ΪĬ��ֵ
            _ = SetValue(_defaultValue);
        }

        // ͨ��ֵ��ȡ
        protected async Task<string> GetValue()
        {
            try
            {
                string url;

                if (_isHostParameter)
                {
                    // ��������������ʹ�õ�ǰ�豸��IP��ַ
                    url = $"http://{DeviceManager.CurrentDeviceIP}/datastore/host/win/{_dataPath}";
                }
                else
                {
                    // �����豸������ʹ����IP��ַ�������豸UID
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
                PluginLog.Error($"��ȡֵʧ�ܣ�{ex.Message} (·��: {_dataPath})");
                return "ERR";
            }
        }

        // ͨ��ֵ����
        protected async Task SetValue(string newValue)
        {
            try
            {
                string url;

                if (_isHostParameter)
                {
                    // ��������������ʹ�õ�ǰ�豸��IP��ַ
                    url = $"http://{DeviceManager.CurrentDeviceIP}/datastore/host/win/{_dataPath}";
                }
                else
                {
                    // �����豸������ʹ����IP��ַ�������豸UID
                    url = $"http://{DeviceManager.MainDeviceIP}/datastore/avb/{DeviceManager.CurrentUID}/{_dataPath}";
                }

                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    await client.UploadStringTaskAsync(url, "PATCH", $"json={{\"value\":{newValue}}}");

                    // ���µ�ǰֵ
                    _currentValue = newValue;
                    AdjustmentValueChanged();
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error($"����ֵʧ�ܣ�{ex.Message} (·��: {_dataPath}, ֵ: {newValue})");
            }
        }

        // ���µ�ǰֵ
        protected async Task UpdateValue()
        {
            var newValue = await GetValue();
            if (_currentValue != newValue)
            {
                _currentValue = newValue;
                AdjustmentValueChanged();
            }
        }

        // ��ȡ������ʾͼ��
        protected override BitmapImage GetAdjustmentImage(string actionParameter, PluginImageSize imageSize)
        {
            using (var bitmap = new BitmapBuilder(imageSize))
            {
                // ����������ɫ
                bool isError = _currentValue == "ERR";
                bool isActive = DialActivationManager.IsDialActive(_dialType);

                // ����״̬ѡ�񱳾���ɫ������Ϊ��ɫ������Ϊ��ɫ��δ����Ϊ��ɫ
                //BitmapColor bgColor = isError ? BitmapColor.Red :
                //                     isActive ? new BitmapColor(0, 0, 128) : BitmapColor.Black;

                bitmap.Clear(BitmapColor.Black);

                // ��ʾ��ǰֵ
                bitmap.DrawText(_currentValue ?? "...", fontSize: 20, color: BitmapColor.White);

                // ��������ָʾ��
                //if (_isHostParameter)
                //{
                //    bitmap.FillCircle(5, 5, 3, BitmapColor.Orange);
                //}

                return bitmap.ToImage();
            }
        }

        // ���󷽷�����ȡֵ��Ԥ���б��е�����
        protected abstract int GetValueIndex(string value);

        // ���󷽷�������������ȡԤ��ֵ
        protected abstract string GetValueByIndex(int index);

        // ���󷽷�����ȡԤ������
        protected abstract int GetPresetsCount();

        // �ͷ���Դ
        public void Dispose() => _updateTimer?.Dispose();
    }
}