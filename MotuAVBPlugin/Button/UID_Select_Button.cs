// 设备切换按钮实现
namespace Loupedeck.MotuAVBPlugin.Buttons
{
    using System.Linq;
    using System.Threading.Tasks;

    using Loupedeck;
    using Loupedeck.MotuAVBPlugin;

    public class UID_Select_Button : PluginDynamicCommand
    {
        private string[] _availableUIDs = new string[0]; // 可用UID列表
        private int _currentIndex;                       // 当前选中索引

        public UID_Select_Button()
            : base("UID Select", "选择当前控制设备", "Choose")
        {
            Task.Run(async () =>
            {
                // 初始化时刷新设备列表和IP映射
                await DeviceManager.RefreshDeviceIPs();
                await RefreshUIDs();
            });
        }

        // 刷新设备UID列表
        private async Task RefreshUIDs()
        {
            _availableUIDs = await DeviceManager.GetAvailableUIDs();
            if (_availableUIDs.Length > 0)
            {
                // 设置当前UID为第一个设备
                // 注意：这将自动触发DeviceManager中的IP更新
                DeviceManager.CurrentUID = _availableUIDs.First();

                // 更新UI
                ActionImageChanged();
            }
        }

        // 按钮点击事件：循环切换设备
        protected override void RunCommand(string actionParameter)
        {
            if (_availableUIDs.Length == 0)
                return;

            _currentIndex = (_currentIndex + 1) % _availableUIDs.Length; // 循环索引
            DeviceManager.CurrentUID = _availableUIDs[_currentIndex]; // 这会自动更新IP
            ActionImageChanged(); // 更新UI
        }

        // 构建按钮图像
        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
        {
            using (var bitmap = new BitmapBuilder(imageSize))
            {
                bitmap.Clear(BitmapColor.Black);

                var deviceName = DeviceManager.GetDeviceName(DeviceManager.CurrentUID);
                bitmap.DrawText(deviceName, fontSize: 18);      // 显示设备名称

                // 显示当前设备IP（可选，用于调试）
                //bitmap.DrawText($"IP: {DeviceManager.CurrentDeviceIP}", y: 30, fontSize: 10);

                return bitmap.ToImage();
            }
        }
    }
}