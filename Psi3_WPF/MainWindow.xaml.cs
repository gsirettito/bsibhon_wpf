using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Psi3_WPF {
    /*!
 Called when tunnel is starting to get the library consumer's desired configuration.

 @code
 Required fields:
 - `PropagationChannelId`
 - `SponsorId`
 - Remote server list functionality is not strictly required, but absence greatly undermines circumvention ability.
   - `RemoteServerListURLs`
   - `RemoteServerListSignaturePublicKey`
 - Obfuscated server list functionality is also not strictly required, but aids circumvention ability.
   - `ObfuscatedServerListRootURLs`
   - `RemoteServerListSignaturePublicKey`: This is the same field as above. It is required if either `RemoteServerListURLs` or `ObfuscatedServerListRootURLs` is supplied.

 Optional fields (if you don't need them, don't set them):
 - `DataStoreDirectory`: If not set, the library will use a sane location. Override if the client wants to restrict where operational data is kept. If overridden, the directory must already exist and be writable.
 - `RemoteServerListDownloadFilename`: If not set, the library will use a sane location. Override if the client wants to restrict where operational data is kept.
 - `ObfuscatedServerListDownloadDirectory`: If not set, the library will use a sane location. Override if the client wants to restrict where operational data is kept. If overridden, the directory must already exist and be writable.
 - `UpstreamProxyUrl`
 - `EmitDiagnosticNotices`
 - `EgressRegion`
 - `EstablishTunnelTimeoutSeconds`
 - Only set if disabling timeouts (for very slow network connections):
   - `TunnelConnectTimeoutSeconds`
   - `TunnelPortForwardDialTimeoutSeconds`
   - `TunnelSshKeepAliveProbeTimeoutSeconds`
   - `TunnelSshKeepAlivePeriodicTimeoutSeconds`
   - `FetchRemoteServerListTimeoutSeconds`
   - `PsiphonApiServerTimeoutSeconds`
   - `FetchRoutesTimeoutSeconds`
   - `HttpProxyOriginServerTimeoutSeconds`
 - Fields which should only be set by Psiphon proper:
   - `LocalHttpProxyPort`
   - `LocalSocksProxyPort`
 @endcode

 @note All other config fields must not be set.

 See the tunnel-core config code for details about the fields.
 https://github.com/Psiphon-Labs/psiphon-tunnel-core/blob/master/psiphon/config.go

 @return Either JSON NSString with config that should be used to run the Psiphon tunnel,
         or return already parsed JSON as NSDictionary,
         or nil on error.
 */

    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private string coreFileName = "psiphon-tunnel-core.exe";
        private string configFile = "config.json";
        private string serverListFile = "server_list.dat";
        private string config = "";
        private string json;
        private Dictionary<string, string> translator;
        private string error_data;
        private string output_data;
        private int listeningHttpProxyPort;
        private int listeningSocksProxyPort;
        private string proxyKey = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";
        private object bypass;
        private bool proxyEnabled;
        private Process p;
        private bool closeTunnel = true;

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();

        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        public MainWindow() {
            InitializeComponent();
            json = Psi3_WPF.Properties.Resources.psiphon;

            translator = new Dictionary<string, string>() {
                {"Best Performance", ""},
                {"Austria", "AT"},
                {"Belgium", "BE"},
                {"Bulgaria", "BG"},
                {"Canada", "CA"},
                {"Switzerland", "CH"},
                {"Czech Republic", "CZ"},
                {"Germany", "DE"},
                {"Denmark", "DK"},
                {"Estonia", "EE"},
                {"Spain", "ES"},
                {"Finland", "FL"},
                {"France", "FR"},
                {"United Kingdom", "GB"},
                {"Hungary", "HU"},
                {"Ireland", "IE"},
                {"India", "IN"},
                {"Japan", "JP"},
                {"Latvia", "LV"},
                {"Netherlands", "NL"},
                {"Norway", "NO"},
                {"Poland", "PL"},
                {"Romania", "RO"},
                {"Serbia", "RS"},
                {"Sweden", "SE"},
                {"Singapore", "SG"},
                {"Slovakia", "US"}
            };
        }

        public IntPtr Handle { get { return new WindowInteropHelper(this).Handle; } }

        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);

            SendToast("welcome!", TimeSpan.FromSeconds(5));
        }

        protected override void OnClosing(CancelEventArgs e) {
            base.OnClosing(e);
            if (closeTunnel) {
                if (p != null && !p.HasExited)
                    p.Kill();
            }
        }

        private void titlebar_MouseDown(object sender, MouseButtonEventArgs e) {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void home_continue_btn_Click(object sender, RoutedEventArgs e) {
            tabNavigator.SelectedIndex = 1;
        }

        private void core_browse_btn_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "psiphon-tunnel-core|psiphon-tunnel-core*.exe|Any Program|*.exe";
            if (ofd.ShowDialog() == true) {
                core_path_box.Text = ofd.FileName;
                coreFileName = core_path_box.Text;
            }
        }

        private void region_next_btn_Click(object sender, RoutedEventArgs e) {
            var region = cbb_region.Text;
            if (!string.IsNullOrEmpty(region)) {
                json = json.Replace(
                    $"\"EgressRegion\": \"\"",
                    $"\"EgressRegion\": \"{translator[region]}\"");
            }
            if (!chbx_split_own_region.IsChecked == true) {
                json = json.Replace(
                    $"\"SplitTunnelOwnRegion\": true",
                    $"\"SplitTunnelOwnRegion\": false");
            }
            tabNavigator.SelectedIndex = 2;
        }

        private void proxy_next_btn_Click(object sender, RoutedEventArgs e) {
            var hostname = txt_hostname.Text;
            var port = txt_port.Text;
            var username = txt_username.Text;
            var password = pwd_password.Password;
            if (chbx_no_proxy.IsChecked == false)
                if (!string.IsNullOrEmpty(hostname) &&
                    !string.IsNullOrEmpty(port) &&
                    !string.IsNullOrEmpty(username) &&
                    !string.IsNullOrEmpty(password)) {
                    json = json.Replace(
                        $"\"UpstreamProxyUrl\": \"\"",
                        $"\"UpstreamProxyUrl\": \"http://{username}:{password}@{hostname}:{port}\"");
                } else if (!string.IsNullOrEmpty(hostname) &&
                     !string.IsNullOrEmpty(port) &&
                     (string.IsNullOrEmpty(username) ||
                     string.IsNullOrEmpty(password))) {
                    json = json.Replace(
                        $"\"UpstreamProxyUrl\": \"\"",
                        $"\"UpstreamProxyUrl\": \"http://{hostname}:{port}\"");
                }
            using (StreamWriter sw = new StreamWriter(configFile, false)) {
                sw.Write(json);
                sw.Flush();
                sw.Close();
            }
            tabNavigator.SelectedIndex = 3;
        }

        private void proxy_back_btn_Click(object sender, RoutedEventArgs e) {
            tabNavigator.SelectedIndex = 1;
        }

        private void status_back_btn_Click(object sender, RoutedEventArgs e) {
            tabNavigator.SelectedIndex = 2;
        }

        private void status_connect_btn_Click(object sender, RoutedEventArgs e) {
            switch (btn_state.Content) {
                case "Connect":
                    p = new Process() {
                    };
                    ProcessStartInfo startInfo = new ProcessStartInfo(coreFileName, $"--config \"{configFile}\" --serverList \"{serverListFile}\"") {
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                    };

                    p.StartInfo = startInfo;
                    p.EnableRaisingEvents = true;
                    p.ErrorDataReceived += P_ErrorDataReceived;
                    p.OutputDataReceived += P_OutputDataReceived;
                    p.Exited += P_Exited;
                    p.Disposed += P_Disposed;
                    p.Start();
                    p.BeginErrorReadLine();
                    p.BeginOutputReadLine();
                    btn_state.Content = "Abort";
                    break;
                case "Abort":
                case "Disconnect":
                    if (!p.HasExited)
                        p.Kill();
                    btn_state.Content = "Connect";
                    tbk_status.Text = "DISCONNECTED";
                    break;
            }
        }

        private void P_Disposed(object sender, EventArgs e) {
            SendToast("", TimeSpan.FromSeconds(5));
        }

        private void SendToast(string message, TimeSpan duration) {
            Toast a = new Toast() { IconClass = "fa-circle-exclamation", Message = message };
            a.Margin = new Thickness(45);
            grid.Children.Add(a);
            Multimedia.Animations.FadeIn(a, TimeSpan.FromSeconds(0.3), new Action<object, EventArgs>((obj, e) => {
                Multimedia.Animations.FadeOut(a, duration, new Action<object, EventArgs>((obj1, e1) => {
                    grid.Children.Remove(a);
                }));
            }));
        }

        private void P_Exited(object sender, EventArgs e) {
            this.Dispatcher.Invoke(new Action(() => {
                SendToast("Process has exited!", TimeSpan.FromSeconds(5));
            }));
        }

        private void P_OutputDataReceived(object sender, DataReceivedEventArgs e) {
            if (e.Data == null) return;
            output_data += e.Data;
            this.Dispatcher.Invoke(new Action(() => {
                output_screen.Text += e.Data + "\n";
            }));
        }

        private void P_ErrorDataReceived(object sender, DataReceivedEventArgs e) {
            if (e.Data == null) return;
            error_data += e.Data;
            this.Dispatcher.Invoke(new Action(() => {
                output_screen.Text += e.Data + "\n";
                if (e.Data.Contains("ListeningSocksProxyPort")) {
                    JObject jobject = JObject.Parse(e.Data);
                    PortType root = Newtonsoft.Json.JsonSerializer.Create().Deserialize<PortType>(jobject.CreateReader());
                    listeningSocksProxyPort = root.data.port;
                    SendToast($"Socks port: {listeningSocksProxyPort}", TimeSpan.FromSeconds(3));
                } else if (e.Data.Contains("ListeningHttpProxyPort")) {
                    JObject jobject = JObject.Parse(e.Data);
                    PortType root = Newtonsoft.Json.JsonSerializer.Create().Deserialize<PortType>(jobject.CreateReader());
                    listeningHttpProxyPort = root.data.port;
                    SendToast($"Http port: {listeningHttpProxyPort}", TimeSpan.FromSeconds(3));
                } else if (e.Data.Contains("https://ipfounder.net/?sponsor_id")) {
                    tbk_status.Text = "CONNECTED";
                    tbk_status.Foreground = FindResource("PsiphonConnectedBrush") as SolidColorBrush;
                    btn_state.Content = "Disconnect";
                    SendToast($"Connected!", TimeSpan.FromSeconds(3));
                } else if (e.Data.Contains(@"""noticeType"":""Tunnels""")) {
                    JObject jobject = JObject.Parse(e.Data);
                    TunnelsType root = Newtonsoft.Json.JsonSerializer.Create().Deserialize<TunnelsType>(jobject.CreateReader());
                    var tunnels_count = root.data.count;
                    if (tunnels_count == 0) {
                        tbk_status.Text = "DISCONNECTED";
                        tbk_status.Foreground = FindResource("PsiphonDisconnectedBrush") as SolidColorBrush;
                        btn_state.Content = "Connect";
                        SendToast($"Disconnected!", TimeSpan.FromSeconds(3));
                    }
                }
                if (!proxyEnabled && listeningHttpProxyPort != 0 && listeningSocksProxyPort != 0) {
                    proxyEnabled = true;
                    run_httpPort.Text = $"HTTP PORT: {listeningHttpProxyPort}";
                    run_socksPort.Text = $"SOCKS PORT: {listeningSocksProxyPort}";
                    RegistryKey rk = Registry.CurrentUser.OpenSubKey(proxyKey, true);
                    rk.SetValue("ProxyServer", $"http=127.0.0.1:{listeningHttpProxyPort};https=127.0.0.1:{listeningHttpProxyPort};ftp=127.0.0.1:{listeningHttpProxyPort};socks=127.0.0.1:{listeningSocksProxyPort}", RegistryValueKind.String);
                    rk.SetValue("ProxyOverride", "", RegistryValueKind.String);
                    rk.SetValue("ProxyEnable", proxyEnabled, RegistryValueKind.DWord);
                    rk.Close();
                }
            }));
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            // for .NET Core you need to add UseShellExecute = true
            // see https://learn.microsoft.com/dotnet/api/system.diagnostics.processstartinfo.useshellexecute#property-value
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        public class PortData : Data {
            public int port { get; set; }
        }

        public class CountData : Data {
            public int count { get; set; }
        }

        public class Data {
        }

        public class TunnelsType : RootType {
            public CountData data { get; set; }
        }

        public class PortType : RootType {
            public PortData data { get; set; }
        }

        public class RootType {
            public string noticeType { get; set; }
            public DateTime timestamp { get; set; }
        }

        private void fa_logIcon_MouseDown(object sender, MouseButtonEventArgs e) {
            if (tabNavigator.SelectedIndex == 3)
                tabNavigator.SelectedIndex = 4;
            else if (tabNavigator.SelectedIndex == 4)
                tabNavigator.SelectedIndex = 3;
        }

        private void fa_exitIcon_MouseDown(object sender, MouseButtonEventArgs e) {
            this.Close();
        }
    }
}
