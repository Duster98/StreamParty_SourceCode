using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Media;
using System.Windows.Threading;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace FilmParty
{

    public partial class MainWindow : Window
    {

        private delegate void StartNewThread();
        private DispatcherTimer HideSnackBar = new DispatcherTimer();
        private DispatcherTimer Restore = new DispatcherTimer();
        bool OnlyMe = true;


        public MainWindow()
        {
            InitializeComponent();
            MqttConnection.setKeyEndToEnd("");

            VerifyConnectionDataFile(MqttConnection.Key_EndToEnd, Directory.GetCurrentDirectory() + "\\Data\\AdaDataConnection.dt", 1);
            VerifyConnectionDataFile(MqttConnection.Key_EndToEnd, Directory.GetCurrentDirectory() + "\\Data\\SolaceDataConnection.dt", 2);

            HideSnackBar.Tick += HideSnackBar_Tick;
            HideSnackBar.Interval = new TimeSpan(0, 0, 0, 2, 0);

            Restore.Tick += Restore_Tick;
            Restore.Interval = new TimeSpan(0, 0, 0, 3, 0);

            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownloadVersionTxt);
            webClient.DownloadStringAsync(new Uri("http://epnbxbkyd4kxzppgghiyviro.altervista.org/StreamParty/version.txt"));
        }

        private void DownloadVersionTxt(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                string NewVersion = e.Result;
                string CurrentVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
                //MessageBox.Show(NewVersion + "---" + CurrentVersion);
                if (NewVersion != CurrentVersion)
                {
                    MessageBoxResult result = MessageBox.Show("New version of stream party is available, do you want to update it?", "New update", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        HyperlinkAutomationPeer peer = new HyperlinkAutomationPeer(Update);
                        IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                        invokeProv.Invoke();
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private async void LogInBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ServerAdafruit)
            {
                if (!string.IsNullOrWhiteSpace(NickName.Text) && !string.IsNullOrWhiteSpace(UsernameTxt.Text) && !string.IsNullOrWhiteSpace(AioKeyTxt.Password))
                {
                    if (NickName.Text.Length < 25 && !NickName.Text.Contains("_"))
                    {
                        ProgressCircle.IsIndeterminate = true;
                        LogInContainer.IsEnabled = false;

                        NickName.Text = NickName.Text.Trim();
                        UsernameTxt.Text = UsernameTxt.Text.Trim();
                        AioKeyTxt.Password = AioKeyTxt.Password.Trim();
                        MqttConnection.setNickname(NickName.Text);
                        MqttConnection.setUser(UsernameTxt.Text);
                        MqttConnection.setAioKey(AioKeyTxt.Password);
                        HttpRequest req = new HttpRequest();
                        string response = await Task.Run(() => req.getJsonData("https://io.adafruit.com/api/v2/user?X-AIO-Key=" + MqttConnection.AioKey));

                        if (response != "")
                        {
                            UserDataJsonReceive.Rootobject UserData = JsonConvert.DeserializeObject<UserDataJsonReceive.Rootobject>(response);
                            if (UsernameTxt.Text == UserData.user.username)
                            {
                                if (await Task.Run(() => MqttConnection.MqttTryToConnect()))
                                {
                                    SaveConnectDataFile("", Directory.GetCurrentDirectory() + "\\Data", "\\AdaDataConnection.dt");
                                    MqttConnection.setChatRoomNameKey("chat");
                                    MqttConnection.client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
                                    MqttConnection.MqttSubscribe(UsernameTxt.Text + "/feeds/" + MqttConnection.ChatRoomNameKey);
                                    MqttConnection.IsSolace = false;
                                    MqttConnection.IsGroup = false;
                                    MqttConnection.IamAdmin = true;
                                    Restore.Start();
                                }
                                else
                                {
                                    ShowSnackBar("Error to connect with server!");
                                    LogInContainer.IsEnabled = true;
                                    ProgressCircle.IsIndeterminate = false;
                                }
                            }
                            else
                            {
                                ShowSnackBar("Bad Username or Aio Key");
                                ProgressCircle.IsIndeterminate = false;
                                LogInContainer.IsEnabled = true;
                            }
                        }
                        else
                        {
                            ShowSnackBar("Bad internet connection!");
                            ProgressCircle.IsIndeterminate = false;
                            LogInContainer.IsEnabled = true;
                        }
                    }
                    else
                    {
                        ShowSnackBar("Max 25 char for Nickname and not '_'");
                    }
                }
                else
                {
                    ShowSnackBar("Please, insert data!");
                }
            }
            else if (ServerSolace)
            {
                if (!string.IsNullOrWhiteSpace(SolaceNickName.Text) && !string.IsNullOrWhiteSpace(SolaceHostPort.Text) && !string.IsNullOrWhiteSpace(SolacePassword.Password))
                {
                    if (SolaceNickName.Text.Length < 25 && !SolaceNickName.Text.Contains("_"))
                    {
                        ProgressCircle.IsIndeterminate = true;
                        LogInContainer.IsEnabled = false;

                        SolaceNickName.Text = SolaceNickName.Text.Trim();
                        SolacePassword.Password = SolacePassword.Password.Trim();
                        SolaceHostPort.Text = SolaceHostPort.Text.Trim();
                        SolaceHostPort.Text = SolaceHostPort.Text.Remove(0, 6);
                        var SplitAddressPort = SolaceHostPort.Text.Split(':');
                        MqttConnection.setMqttServer(SplitAddressPort[0]);
                        MqttConnection.setMqttPort(SplitAddressPort[1]);
                        MqttConnection.setNickname(SolaceNickName.Text);
                        MqttConnection.setUser("solace-cloud-client");
                        MqttConnection.setAioKey(SolacePassword.Password);

                        if (await Task.Run(() => MqttConnection.MqttTryToConnect()))
                        {
                            SaveConnectDataFile("", Directory.GetCurrentDirectory() + "\\Data", "\\SolaceDataConnection.dt");
                            MqttConnection.setChatRoomNameKey("SolaceRoom");
                            MqttConnection.client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
                            MqttConnection.MqttSubscribe(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey);
                            MqttConnection.MqttUnsecurePublish(MqttConnection.UserName + "/clients", "{\"status\":\"connected\",\"at\":\"2020-10-14T00:23:28.481Z\",\"client\":{\"id\":\"dzbpiydeyvrngch_" + MqttConnection.NickName + "\",\"ip\":\"78.74.65.35\"}}");
                            MqttConnection.IsSolace = true;
                            MqttConnection.IsGroup = false;
                            MqttConnection.IamAdmin = true;
                            Restore.Start();
                        }
                        else
                        {
                            ShowSnackBar("Error to connect with server!");
                            LogInContainer.IsEnabled = true;
                            ProgressCircle.IsIndeterminate = false;
                        }
                    }
                    else
                    {
                        ShowSnackBar("Max 25 char for Nickname and not '_'");
                    }
                }
                else
                {
                    ShowSnackBar("Please, insert data!");
                }

            }
        }

        string[] UserOnlineYet = new string[49];
        int i = 0;
        string messageEncrypt = "";
        private void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {            
            messageEncrypt = Encoding.UTF8.GetString(e.Message);
            Dispatcher.Invoke(new StartNewThread(MqttMessageAtStartConnection));
        }

        public void MqttMessageAtStartConnection()
        {
            try
            {
                string messageDecrypt = StringCipher.Decrypt(messageEncrypt, MqttConnection.Key_EndToEnd);
                if (messageDecrypt.Contains("RESPONSE"))
                {
                    var message = messageDecrypt.Split('_');
                    if (message[1] == "NOONLY")
                    {
                        OnlyMe = false;
                        UserOnlineYet[i] = message[2];
                        //MessageBox.Show(MqttConnection.NickName + " --- " + message[2]);
                        if (MqttConnection.NickName == message[2])
                        {
                            //MessageBox.Show("ok");
                            Restore.Stop();
                            LogInContainer.IsEnabled = true;
                            ProgressCircle.IsIndeterminate = false;
                            MqttConnection.MqttDisconnect();
                            UserOnlineYet = new string[49];
                            i = 0;
                            if (!SolaceHostPort.Text.Contains("ssl://"))
                            {
                                SolaceHostPort.Text = "ssl://" + SolaceHostPort.Text;
                            }
                            ShowSnackBar("This nickname has already been used");
                            return;
                        }
                        i++;
                    }
                }
            }
            catch (Exception)
            {
                Restore.Stop();
                LogInContainer.IsEnabled = true;
                ProgressCircle.IsIndeterminate = false;
                if (MqttConnection.IsSolace)
                {
                    MqttConnection.MqttUnsecurePublish("solace-cloud-client/clients", "{\"status\":\"disconnected\",\"at\":\"2020-10-14T00:23:28.481Z\",\"client\":{\"id\":\"dzbpiydeyvrngch_" + MqttConnection.NickName + "\",\"ip\":\"78.74.65.35\"}}");
                }
                MqttConnection.MqttDisconnect();
                UserOnlineYet = new string[49];
                i = 0;
                if (!SolaceHostPort.Text.Contains("ssl://"))
                {
                    SolaceHostPort.Text = "ssl://" + SolaceHostPort.Text;
                }
                MessageBox.Show("Error, BAD link room", "Bad end to end", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveConnectDataFile(string EncKey, string PathSettingDataFile, string NameDataFile)
        {
            if (RememberCheck.IsChecked == true)
            {
                string ClearText = "";
                if (ServerAdafruit)
                {
                    ClearText = NickName.Text + "|" + UsernameTxt.Text + "|" + AioKeyTxt.Password;
                }
                else if (ServerSolace)
                {
                    ClearText = SolaceNickName.Text + "|ssl://" + SolaceHostPort.Text + "|" + SolacePassword.Password;
                }

                string EncryptedText = StringCipher.Encrypt(ClearText, EncKey);
                if (!Directory.Exists(PathSettingDataFile))
                {
                    Directory.CreateDirectory(PathSettingDataFile);
                }
                using (StreamWriter ConnectionData = new StreamWriter(PathSettingDataFile + NameDataFile))
                {
                    ConnectionData.WriteLine(EncryptedText);
                }
            }
            else
            {
                if (File.Exists(PathSettingDataFile + NameDataFile))
                {
                    File.Delete(PathSettingDataFile + NameDataFile);
                }
            }
        }

        private void VerifyConnectionDataFile(string DecKey, string PathSettingDataFile, int ServerHost)
        {
            if (File.Exists(PathSettingDataFile))
            {
                string EncSettingDataFile = "";
                using (StreamReader ConnectionData = new StreamReader(PathSettingDataFile))
                {
                    EncSettingDataFile = ConnectionData.ReadLine();
                }
                try
                {
                    string DecryptedData = StringCipher.Decrypt(EncSettingDataFile, DecKey);
                    var DataSetting = DecryptedData.Split('|');
                    if (ServerHost == 1)
                    {
                        NickName.Text = DataSetting[0];
                        UsernameTxt.Text = DataSetting[1];
                        AioKeyTxt.Password = DataSetting[2];
                    }
                    else if (ServerHost == 2)
                    {
                        SolaceNickName.Text = DataSetting[0];
                        SolaceHostPort.Text = DataSetting[1];
                        SolacePassword.Password = DataSetting[2];
                    }
                    RememberCheck.IsChecked = true;
                    LogInBtn.Focus();
                }
                catch (Exception)
                {
                    File.Delete(PathSettingDataFile);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MqttConnection.IsSolace)
            {
                MqttConnection.MqttUnsecurePublish("solace-cloud-client/clients", "{\"status\":\"disconnected\",\"at\":\"2020-10-14T00:23:28.481Z\",\"client\":{\"id\":\"dzbpiydeyvrngch_" + MqttConnection.NickName + "\",\"ip\":\"78.74.65.35\"}}");
            }
            MqttConnection.MqttDisconnect();
            Process.GetCurrentProcess().Kill();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            MqttConnection.MqttDisconnect();
            Process.GetCurrentProcess().Kill();
        }

        public void HideSnackBar_Tick(object sender, EventArgs e)
        {
            SnackbarLogIn.IsActive = false;
            HideSnackBar.Stop();
        }

        public void Restore_Tick(object sender, EventArgs e)
        {
            LogInContainer.IsEnabled = true;
            ProgressCircle.IsIndeterminate = false;
            StreamingRoom streamingRoom = new StreamingRoom();
            MqttConnection.client.MqttMsgPublishReceived -= client_MqttMsgPublishReceived;
            Restore.Stop();
            Hide();
            streamingRoom.Show();
            streamingRoom.ChatroomTxt.Text = NameGroup;
            streamingRoom.NickNameTxt.Text = MqttConnection.NickName;
            if (OnlyMe)
            {
                if (CreatorGroup == "")
                {
                    streamingRoom.MessageAllertCardBox("Only you are online!", PackIconKind.User, "#FFFFFF", "#2E7D32");
                }
                else
                {
                    streamingRoom.MessageAllertCardBox("You joined the group: " + NameGroup + '\n' + "Created by: " + CreatorGroup + IsAdminTxt() + '\n' + "Only you are online", PackIconKind.UserGroup, "#212121", "#FFFFFF");
                    if (UrlStream != "")
                        streamingRoom.ChangeStreamingUrl(UrlStream, false);
                }
            }
            else
            {
                int useronline = 0;
                string usersList = "";

                for (int i = 0; i < 49; i++)
                {
                    if (UserOnlineYet[i] != null)
                    {
                        usersList = usersList + UserOnlineYet[i] + ", ";
                        streamingRoom.addUserOnline(UserOnlineYet[i]);
                        streamingRoom.addVectOnlineUser(UserOnlineYet[i]);
                        useronline++;
                    }
                    else
                    {
                        usersList = usersList.Substring(0, usersList.Length - 2);
                        break;
                    }
                }

                if (useronline > 1)
                {
                    if (CreatorGroup == "")
                    {
                        streamingRoom.MessageAllertCardBox(usersList + " are online!", PackIconKind.UserGroup, "#FFFFFF", "#2E7D32");
                    }
                    else
                    {
                        streamingRoom.MessageAllertCardBox("You joined the group: " + NameGroup + '\n' + "Created by: " + CreatorGroup + IsAdminTxt() + '\n' + "Online users: " + usersList, PackIconKind.UserGroup, "#212121", "#FFFFFF");
                        if (UrlStream != "")
                            streamingRoom.ChangeStreamingUrl(UrlStream, false);
                    }
                }
                else
                {
                    if (CreatorGroup == "")
                    {
                        streamingRoom.MessageAllertCardBox(usersList + " is online!", PackIconKind.User, "#FFFFFF", "#2E7D32");
                    }
                    else
                    {
                        streamingRoom.MessageAllertCardBox("You joined the group: " + NameGroup + '\n' + "Created by: " + CreatorGroup + IsAdminTxt() + '\n' + "Online user: " + usersList, PackIconKind.UserGroup, "#212121", "#FFFFFF");
                        if (UrlStream != "")
                            streamingRoom.ChangeStreamingUrl(UrlStream, false);
                    }
                }
            }
        }

        public void ShowSnackBar(string Message)
        {
            SnackbarLogIn.Message.Content = Message;
            SnackbarLogIn.IsActive = true;
            HideSnackBar.Start();
        }

        string CreatorGroup = "";
        string NameGroup = "Main-Chat";
        string UrlStream = "";
        private async void PasteKey_Click(object sender, RoutedEventArgs e)
        {
            string key_group_enc = Clipboard.GetText();
            string key_group_dec = StringCipher.Decrypt(key_group_enc, "");
            //MessageBox.Show(key_group_dec);
            var var_key_group_dec = key_group_dec.Split('|');

            HttpRequest req = new HttpRequest();
            try
            {
                MqttConnection.IsGroup = true;                
                MqttConnection.setChatRoomNameKey(var_key_group_dec[0]);
                MqttConnection.setUser(var_key_group_dec[1]);
                MqttConnection.setAioKey(var_key_group_dec[2]);
                MqttConnection.setKeyEndToEnd(var_key_group_dec[3]);
                CreatorGroup = var_key_group_dec[4];
                UrlStream = var_key_group_dec[5];
                MqttConnection.IamAdmin = !bool.Parse(var_key_group_dec[6]);
                MqttConnection.setMqttServer(var_key_group_dec[7]);
                MqttConnection.setMqttPort(var_key_group_dec[8]);

            }
            catch (Exception)
            {
                ShowSnackBar("Invalid Key Room");
                return;
            }
            ProgressCircle.IsIndeterminate = true;
            LogInContainer.IsEnabled = false;


            if (MqttConnection.MqttServer.Contains("adafruit"))
            {
                AdaServer_Click(sender, e);
                if (NickName.Text.Length < 25)
                {
                    if (string.IsNullOrWhiteSpace(NickName.Text))
                    {
                        ShowSnackBar("Please insert a NickName");
                        ProgressCircle.IsIndeterminate = false;
                        LogInContainer.IsEnabled = true;
                        return;
                    }
                    MqttConnection.setNickname(NickName.Text);

                    if (await Task.Run(() => MqttConnection.MqttTryToConnect()))
                    {
                        string data = await req.getJsonData("https://io.adafruit.com/api/v2/" + MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey + "?X-AIO-Key=" + MqttConnection.AioKey);
                        if (data != "")
                        {
                            FeedsJson.FeedProprety feedProprety = JsonConvert.DeserializeObject<FeedsJson.FeedProprety>(data);
                            if (feedProprety.enabled)
                            {
                                NameGroup = feedProprety.name;
                                UsernameTxt.Text = "";
                                AioKeyTxt.Password = "";
                                MqttConnection.client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
                                MqttConnection.MqttSubscribe(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey);
                                MqttConnection.IsSolace = false;
                                MqttConnection.IsGroup = true;
                                Restore.Start();
                            }
                            else
                            {
                                restore();
                            }
                        }
                        else
                        {
                            restore();
                        }
                    }
                    else
                    {
                        ShowSnackBar("Error to connect!");
                        LogInContainer.IsEnabled = true;
                        ProgressCircle.IsIndeterminate = false;
                    }
                }
                else
                {
                    ShowSnackBar("Max 25 char for Nickname");
                    LogInContainer.IsEnabled = true;
                    ProgressCircle.IsIndeterminate = false;
                }
            }
            else if (MqttConnection.MqttServer.Contains("messaging.solace.cloud"))
            {
                SolaceServer_Click(sender, e);
                if (SolaceNickName.Text.Length < 25)
                {
                    if (string.IsNullOrWhiteSpace(SolaceNickName.Text))
                    {
                        ShowSnackBar("Please insert a NickName");
                        ProgressCircle.IsIndeterminate = false;
                        LogInContainer.IsEnabled = true;
                        return;
                    }
                    MqttConnection.setNickname(SolaceNickName.Text);

                    if (await Task.Run(() => MqttConnection.MqttTryToConnect()))
                    {
                        
                        MqttConnection.setChatRoomNameKey("SolaceRoom");
                        MqttConnection.client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
                        MqttConnection.MqttSubscribe(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey);
                        MqttConnection.MqttUnsecurePublish(MqttConnection.UserName + "/clients", "{\"status\":\"connected\",\"at\":\"2020-10-14T00:23:28.481Z\",\"client\":{\"id\":\"dzbpiydeyvrngch_" + MqttConnection.NickName + "\",\"ip\":\"78.74.65.35\"}}");
                        MqttConnection.IsSolace = true;
                        MqttConnection.IsGroup = true;
                        Restore.Start();
                    }
                    else
                    {
                        ShowSnackBar("Error to connect!");
                        LogInContainer.IsEnabled = true;
                        ProgressCircle.IsIndeterminate = false;
                    }
                }
                else
                {
                    ShowSnackBar("Max 25 char for Nickname");
                    LogInContainer.IsEnabled = true;
                    ProgressCircle.IsIndeterminate = false;
                }
            }
            else
            {
                ShowSnackBar("Invalid Server!");
                LogInContainer.IsEnabled = true;
                ProgressCircle.IsIndeterminate = false;
            }

        }

        private string IsAdminTxt()
        {
            if (!MqttConnection.IamAdmin)
            {
                return " (Admin)";
            }
            else
            {
                return "";
            }
        }

        private void restore()
        {
            CreatorGroup = "";
            NameGroup = "";
            UrlStream = "";
            MqttConnection.IsGroup = false;
            MqttConnection.MqttDisconnect();
            MqttConnection.setKeyEndToEnd("");
            MqttConnection.setUser("");
            MqttConnection.setAioKey("");
            MqttConnection.IamAdmin = false;
            LogInContainer.IsEnabled = true;
            ProgressCircle.IsIndeterminate = false;
            ShowSnackBar("The room does not exist or disabled!");
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            if (ServerAdafruit)
            {
                Process.Start("https://accounts.adafruit.com/users/sign_up");
            }
            else if (ServerSolace)
            {
                Process.Start("https://solace.com/");
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                LogInBtn_Click(sender, e);
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(Environment.CurrentDirectory + "\\update\\Update.exe");
            }
            catch (Exception ex)
            {
                try
                {
                    Process.Start(Environment.CurrentDirectory + "\\StreamParty\\update\\Update.exe");
                }
                catch (Exception)
                {
                    MessageBox.Show(ex.Message + Environment.NewLine + Environment.NewLine + "Close and open application please.");
                }
            }
        }

        bool ServerAdafruit = true;
        bool ServerSolace = false;

        private void AdaServer_Click(object sender, RoutedEventArgs e)
        {
            if (!ServerAdafruit)
            {
                ServerAdafruit = true;
                ServerSolace = false;
                AdaServer.Foreground = Brushes.WhiteSmoke;
                AdaServer.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#2f6ea5"));
                SolaceServer.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#2f6ea5"));
                SolaceServer.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF0E1621"));
                AdafruitLoginBox.Visibility = Visibility.Visible;
                SolaceLoginBox.Visibility = Visibility.Hidden;
            }
        }

        private void SolaceServer_Click(object sender, RoutedEventArgs e)
        {
            if (!ServerSolace)
            {
                ServerAdafruit = false;
                ServerSolace = true;
                SolaceServer.Foreground = Brushes.WhiteSmoke;
                SolaceServer.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#2f6ea5"));
                AdaServer.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#2f6ea5"));
                AdaServer.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF0E1621"));
                AdafruitLoginBox.Visibility = Visibility.Hidden;
                SolaceLoginBox.Visibility = Visibility.Visible;
            }
        }

        private void AdaServer_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            AdaServer.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#2f6ea5"));
            AdaServer.Background = Brushes.WhiteSmoke;
        }

        private void AdaServer_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!ServerAdafruit)
            {
                AdaServer.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#2f6ea5"));
                AdaServer.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF0E1621"));
            }
            else
            {
                AdaServer.Foreground = Brushes.WhiteSmoke;
                AdaServer.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#2f6ea5"));
            }
        }

        private void SolaceServer_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            SolaceServer.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#2f6ea5"));
            SolaceServer.Background = Brushes.WhiteSmoke;
        }

        private void SolaceServer_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!ServerSolace)
            {
                SolaceServer.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#2f6ea5"));
                SolaceServer.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF0E1621"));
            }
            else
            {
                SolaceServer.Foreground = Brushes.WhiteSmoke;
                SolaceServer.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#2f6ea5"));
            }
        }

        private void LoginFree_Click(object sender, RoutedEventArgs e)
        {
            StreamingRoom streamingRoom = new StreamingRoom();
            streamingRoom.SetOnlyMe(true);
            Hide();
            streamingRoom.Show();
        }
    }
}
