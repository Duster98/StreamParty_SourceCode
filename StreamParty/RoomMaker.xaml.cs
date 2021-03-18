using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Net.Http;
using System.IO;

namespace FilmParty
{

    public partial class RoomMaker : Window
    {
        private DispatcherTimer HideSnackBar = new DispatcherTimer();

        WebClient KeyEndToEnd = new WebClient();

        public RoomMaker()
        {
            InitializeComponent();

            HideSnackBar.Tick += HideSnackBar_Tick;
            HideSnackBar.Interval = new TimeSpan(0, 0, 0, 3, 0);

            KeyEndToEnd.DownloadStringCompleted += new DownloadStringCompletedEventHandler(getKEY);
            if (!MqttConnection.IsGroup)
            {
                UsernameTxt.Text = MqttConnection.UserName;
                AioKeyTxt.Password = MqttConnection.AioKey;
            }
            else
            {
                CurrentData.IsEnabled = true;
                CheckCurrentData.IsChecked = false;
                CheckCurrentData.IsEnabled = false;
                UsernameTxt.Text = "";
                AioKeyTxt.Password = "";
            }
        }

        public void HideSnackBar_Tick(object sender, EventArgs e)
        {
            SnackbarRoom.IsActive = false;
            HideSnackBar.Stop();
        }

        private void getKEY(object sender, DownloadStringCompletedEventArgs e)
        {
            BtnRegenerate.IsEnabled = true;
            try
            {
                Key_EndToEnd.Text = e.Result.Substring(0, 50);
                if (Key_EndToEnd.Text.Contains("|"))
                {
                    Key_EndToEnd.Text = "PLEASE GENERATE NEW KEY";
                }
            }
            catch (Exception)
            {
                Key_EndToEnd.Text = "CONNECTION ERROR";
            }
        }

        private void CkeckCurrentData_Click(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentData.IsChecked == false)
            {
                AioKeyTxt.Password = "";
                CurrentData.IsEnabled = true;
            }
            else if (CheckCurrentData.IsChecked == true)
            {
                UsernameTxt.Text = MqttConnection.UserName;
                AioKeyTxt.Password = MqttConnection.AioKey;
                CurrentData.IsEnabled = false;
            }
        }

        private void BtnRegenerate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BtnRegenerate.IsEnabled = false;
                KeyEndToEnd.DownloadStringTaskAsync("https://passwordsgenerator.net/calc.php?Length=50&Symbols=0&Lowercase=1&Uppercase=1&Numbers=1&Nosimilar=1&Last=1");
            }
            catch (Exception)
            {
                BtnRegenerate.IsEnabled = true;
                Key_EndToEnd.Text = "CONNECTION ERROR";
            }
        }

        private void BtnRegenerate_MouseEnter(object sender, MouseEventArgs e)
        {
            BtnRegenerate.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
        }

        private void BtnRegenerate_MouseLeave(object sender, MouseEventArgs e)
        {
            BtnRegenerate.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
        }

        private async void GenerateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (NameRoom.Text != "" && !string.IsNullOrWhiteSpace(UsernameTxt.Text) && !string.IsNullOrWhiteSpace(AioKeyTxt.Password) && !string.IsNullOrWhiteSpace(Key_EndToEnd.Text) && !Key_EndToEnd.Text.Contains("GENERATE"))
            {
                HttpRequest req = new HttpRequest();

                string KeyRoomDec = req.getCorretName(NameRoom.Text) + "|" + UsernameTxt.Text + "|" + AioKeyTxt.Password + "|" + Key_EndToEnd.Text + "|" + MqttConnection.NickName + "|" + UrlTxt.Text + "|" + CheckControll.IsChecked + "|" + MqttConnection.MqttServer + "|" + MqttConnection.MqttPort.ToString();
                string KeyRoomEnc = StringCipher.Encrypt(KeyRoomDec, "");

                if (CheckCurrentData.IsChecked == true)
                {
                    if (getNumFeed(req.getAllFeeds()) != 10)
                    {
                        MqttConnection.MqttUnsubscribe(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey);
                        if (req.createFeed(NameRoom.Text, DescriptionTxt.Text, Key_EndToEnd.Text, UrlTxt.Text, CheckControll.IsChecked.ToString()))
                        {
                            MqttConnection.setKeyEndToEnd(Key_EndToEnd.Text);
                            MqttConnection.MqttSubscribe(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey);
                            MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, MqttConnection.NickName + "_ROOMGROUP_" + NameRoom.Text + "_" + UrlTxt.Text);
                            Clipboard.SetText(KeyRoomEnc);
                            MessageBox.Show("Room created!!!" + '\n' + '\n' + "Don't worry, I copy the access key to the group, share it with your friends, you just have to paste and they just have to copy it and click on \"I have a key\"");
                            Close();
                        }
                        else
                        {
                            MqttConnection.setKeyEndToEnd("");
                            MqttConnection.setChatRoomNameKey("chat");
                            MqttConnection.MqttSubscribe(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey);
                            ShowSnackBar("Error creating room!");
                        }
                    }
                    else
                    {
                        ShowSnackBar("Max number of group created, delete one!");
                    }
                }
                else
                {
                    string tempUser = MqttConnection.UserName;
                    string tempAioKey = MqttConnection.AioKey;
                    MqttConnection.MqttDisconnect();
                    MqttConnection.setUser(UsernameTxt.Text);
                    MqttConnection.setAioKey(AioKeyTxt.Password);
                    if (!(await MqttConnection.MqttFastConnect()))
                    {
                        MqttConnection.setUser(tempUser);
                        MqttConnection.setAioKey(tempAioKey);
                        ShowSnackBar("Erron to connect with new data!");
                        UsernameTxt.Focus();
                        return;
                    }

                    if (getNumFeed(req.getAllFeeds()) != 10)
                    {
                        if (req.createFeed(NameRoom.Text, DescriptionTxt.Text, Key_EndToEnd.Text, UrlTxt.Text, CheckControll.IsChecked.ToString()))
                        {
                            MqttConnection.setKeyEndToEnd(Key_EndToEnd.Text);
                            MqttConnection.MqttSubscribe(MqttConnection.UserName + "/clients");
                            MqttConnection.MqttSubscribe(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey);
                            MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, MqttConnection.NickName + "_ROOMGROUP_" + NameRoom.Text + "_" + UrlTxt.Text);
                            Clipboard.SetText(KeyRoomEnc);
                            MessageBox.Show("Room created!!!" + '\n' + '\n' + "Don't worry, I copy the access key to the group, share it with your friends, you just have to paste and they just have to copy it and click on \"I have a key\"");
                            Close();
                        }
                        else
                        {
                            MqttConnection.setKeyEndToEnd("");
                            MqttConnection.setChatRoomNameKey("chat");
                            MqttConnection.MqttSubscribe(MqttConnection.UserName + "/clients");
                            MqttConnection.MqttSubscribe(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey);
                            ShowSnackBar("Error creating room!");
                        }
                    }
                    else
                    {
                        ShowSnackBar("Max number of group created, delete one!");
                    }
                }

            }
            else
            {
                ShowSnackBar("Complete the box please");
            }
        }

        public void ShowSnackBar(string Message)
        {
            SnackbarRoom.Message.Content = Message;
            SnackbarRoom.IsActive = true;
            HideSnackBar.Start();
        }

        private int getNumFeed(string[] feeds)
        {
            int num = 0;
            for (int i = 0; i < feeds.Length; i++)
            {
                if (feeds[i] != null)
                {
                    num++;
                }
                else
                {
                    break;
                }
            }

            return num;
        }
    }
}
