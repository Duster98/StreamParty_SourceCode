using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
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

namespace FilmParty
{

    public partial class MyRoom : Window
    {

        WebClient downloadData = new WebClient();
        private DispatcherTimer HideSnackBar = new DispatcherTimer();

        public MyRoom()
        {
            InitializeComponent();

            HideSnackBar.Tick += HideSnackBar_Tick;
            HideSnackBar.Interval = new TimeSpan(0, 0, 0, 3, 0);

            downloadData.DownloadDataCompleted += new DownloadDataCompletedEventHandler(downloadCompleted);

            refreshRoom("");
            if (MqttConnection.IsGroup)
            {
                DrawerHostContent.IsEnabled = false;
            }
            else
            {
                DrawerHostContent.IsEnabled = true;
            }
        }

        public void HideSnackBar_Tick(object sender, EventArgs e)
        {
            SnackbarMyRoom.IsActive = false;
            HideSnackBar.Stop();
        }

        public void ShowSnackBar(string Message)
        {
            SnackbarMyRoom.Message.Content = Message;
            SnackbarMyRoom.IsActive = true;
            HideSnackBar.Start();
        }

        private void downloadCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            assignDataRoom(Encoding.UTF8.GetString(e.Result));
            RoomProgressCircle.IsIndeterminate = false;
        }

        private void refreshRoom(string KeyFeed)
        {
            Uri dataRoom = new Uri("https://io.adafruit.com/api/v2/" + MqttConnection.UserName + "/feeds/" + KeyFeed + "?X-AIO-Key=" + MqttConnection.AioKey);
            RoomProgressCircle.IsIndeterminate = true;
            Rooms.Children.Clear();
            Rooms.RowDefinitions.Clear();
            Rooms.Height = 293;
            downloadData.DownloadDataAsync(dataRoom);
        }

        public void assignDataRoom(string datareceived)
        {
            List<FeedsJson.FeedProprety> dataJson = JsonConvert.DeserializeObject<List<FeedsJson.FeedProprety>>(datareceived);
            
            for (int i = 0; i < dataJson.Count; i++)
            {
                if (dataJson[i].key != "chat")
                {
                    insertRoom(Rooms, dataJson[i]);

                    if (i > 3 && i < dataJson.Count - 1)
                    {
                        Scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                        Rooms.Height = Rooms.Height + 70;
                    }
                }
            }
            RoomProgressCircle.IsIndeterminate = false;
        }

        private void insertRoom(Grid MainGridRooms, FeedsJson.FeedProprety dataJson)
        {
            TextBlock RoomName = textBlockRoom(dataJson.name, TextWrapping.NoWrap, FontStyles.Normal, (Style)FindResource("MaterialDesignHeadline6TextBlock"), 15, VerticalAlignment.Center, HorizontalAlignment.Left, "#303F9F");
            dataJson.created_at.ToLocalTime();
            TextBlock DateGroupCreation = textBlockRoom(dataJson.created_at.Hour + ":" + dataJson.created_at.Minute + "  " + dataJson.created_at.ToShortDateString(), TextWrapping.NoWrap, FontStyles.Normal, (Style)FindResource("MaterialDesignBody2TextBlock"), 11, VerticalAlignment.Center, HorizontalAlignment.Right, "#212121");
            string EncData = dataJson.last_value;
            string[] KeyDec;
            string DecData = "";
            try
            {
                KeyDec = dataJson.license.Split('*');
                DecData = StringCipher.Decrypt(EncData, KeyDec[0]);
            }
            catch (Exception)
            {
                DecData = "ErrorUser_Bad Data / Key End-To-End";
            }
            var message = DecData.Split('_');
            TextBlock UserTypeLastMessage = textBlockRoom(message[0] + ":", TextWrapping.NoWrap, FontStyles.Normal, (Style)FindResource("MaterialDesignSubtitle2TextBlock"), 12, VerticalAlignment.Bottom, HorizontalAlignment.Left, "#212121");
            TextBlock LastMessage = textBlockRoom(message[1], TextWrapping.NoWrap, FontStyles.Italic, (Style)FindResource("MaterialDesignBody2TextBlock"), 11, VerticalAlignment.Bottom, HorizontalAlignment.Left, "#616161");
            TextBlock DateLastMessage = textBlockRoom(dataJson.updated_at.Hour + ":" + dataJson.updated_at.Minute, TextWrapping.NoWrap, FontStyles.Normal, (Style)FindResource("MaterialDesignBody2TextBlock"), 11, VerticalAlignment.Bottom, HorizontalAlignment.Right, "#616161");

            Grid contenentData = gridRoom(RoomName, DateGroupCreation, UserTypeLastMessage, LastMessage, DateLastMessage);
            Button FinalData = finalRoomCard(contenentData, dataJson);

            RowDefinition rowDef = new RowDefinition();
            rowDef.Height = new GridLength(70);
            Grid.SetRow(FinalData, MainGridRooms.RowDefinitions.Count);
            MainGridRooms.RowDefinitions.Add(rowDef);
            MainGridRooms.Children.Add(FinalData);
        }

        Button selectRoom;
        private void selectRoom_Click(object sender, RoutedEventArgs e)
        {
            selectRoom = sender as Button;
            DrawerHost.OpenDrawerCommand.Execute(DrawerHostContent, selectRoom);
        }

        private Button finalRoomCard(Grid roomDataContainer, FeedsJson.FeedProprety dataJson)
        {
            Button FinalRoom = new Button();
            FinalRoom.Padding = new Thickness(8, 8, 8, 4);
            ShadowAssist.SetShadowDepth(FinalRoom, ShadowDepth.Depth2);
            ButtonAssist.SetCornerRadius(FinalRoom, new CornerRadius(1));
            RippleAssist.SetFeedback(FinalRoom, (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121")));
            FinalRoom.BorderBrush = Brushes.White;
            FinalRoom.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
            FinalRoom.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
            FinalRoom.Width = 305;
            FinalRoom.Height = 60;
            FinalRoom.VerticalAlignment = VerticalAlignment.Center;
            FinalRoom.HorizontalAlignment = HorizontalAlignment.Center;
            FinalRoom.Content = roomDataContainer;
            FinalRoom.Name = "Room_" + Rooms.RowDefinitions.Count.ToString() + "_" + dataJson.id;
            FinalRoom.Click += new RoutedEventHandler(selectRoom_Click);
            return FinalRoom;
        }

        private Grid gridRoom(TextBlock RoomName, TextBlock DateCreation, TextBlock UserTypeLastMessage, TextBlock LastMessage, TextBlock DateLastMessage)
        {
            Grid containerData = new Grid();
            containerData.MinWidth = 289;
            RowDefinition rowDef1 = new RowDefinition();
            RowDefinition rowDef2 = new RowDefinition();

            Grid containerTxtSuper = new Grid();
            ColumnDefinition colDef1 = new ColumnDefinition();
            ColumnDefinition colDef2 = new ColumnDefinition();
            colDef2.Width = new GridLength(MeasureString(DateCreation).Width + 10);
            Grid.SetColumn(RoomName, 0);
            Grid.SetColumn(DateCreation, 1);
            containerTxtSuper.ColumnDefinitions.Add(colDef1);
            containerTxtSuper.ColumnDefinitions.Add(colDef2);
            containerTxtSuper.Children.Add(RoomName);
            containerTxtSuper.Children.Add(DateCreation);

            Grid containerTxtDown = new Grid();
            containerTxtDown.Height = 25;
            ColumnDefinition colDef3 = new ColumnDefinition();
            ColumnDefinition colDef4 = new ColumnDefinition();
            colDef4.Width = new GridLength(MeasureString(DateLastMessage).Width + 10);

            Grid containerMessage = new Grid();
            containerMessage.HorizontalAlignment = HorizontalAlignment.Left;
            ColumnDefinition subColDef1 = new ColumnDefinition();
            ColumnDefinition subColDef2 = new ColumnDefinition();
            subColDef1.Width = new GridLength(MeasureString(UserTypeLastMessage).Width + 5);
            Grid.SetColumn(UserTypeLastMessage, 0);
            Grid.SetColumn(LastMessage, 1);
            containerMessage.ColumnDefinitions.Add(subColDef1);
            containerMessage.ColumnDefinitions.Add(subColDef2);

            Grid.SetColumn(containerMessage, 0);
            Grid.SetColumn(DateLastMessage, 1);
            containerTxtDown.ColumnDefinitions.Add(colDef3);
            containerTxtDown.ColumnDefinitions.Add(colDef4);

            containerMessage.Children.Add(UserTypeLastMessage);
            containerMessage.Children.Add(LastMessage);

            containerTxtDown.Children.Add(containerMessage);
            containerTxtDown.Children.Add(DateLastMessage);

            Grid.SetRow(containerTxtSuper, 0);
            Grid.SetRow(containerTxtDown, 1);
            containerData.RowDefinitions.Add(rowDef1);
            containerData.RowDefinitions.Add(rowDef2);
            containerData.Children.Add(containerTxtSuper);
            containerData.Children.Add(containerTxtDown);

            containerData.VerticalAlignment = VerticalAlignment.Top;
            containerData.HorizontalAlignment = HorizontalAlignment.Left;

            return containerData;
        }

        private TextBlock textBlockRoom(string Text, TextWrapping textWrapping, FontStyle fontStyle, Style style, int fontSize, VerticalAlignment alignmentVe, HorizontalAlignment alignmentHo, string foregroundColor)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom(foregroundColor));
            textBlock.TextWrapping = textWrapping;
            textBlock.HorizontalAlignment = alignmentHo;
            textBlock.FontSize = fontSize;
            textBlock.VerticalAlignment = alignmentVe;
            textBlock.FontStyle = fontStyle;
            textBlock.Style = style;
            textBlock.Text = Text;

            return textBlock;
        }

        private Size MeasureString(TextBlock textBlock)
        {
            var formattedText = new FormattedText(
                textBlock.Text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                textBlock.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                TextFormattingMode.Display
                );

            return new Size(formattedText.Width, formattedText.Height);
        }

        private void BtnShare_MouseEnter(object sender, MouseEventArgs e)
        {
            BtnShare.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#303F9F"));
        }

        private void BtnShare_MouseLeave(object sender, MouseEventArgs e)
        {
            BtnShare.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
        }

        private void BtnJoin_MouseEnter(object sender, MouseEventArgs e)
        {
            BtnJoin.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#5E35B1"));
        }

        private void BtnJoin_MouseLeave(object sender, MouseEventArgs e)
        {
            BtnJoin.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
        }

        private void BtnKey_MouseEnter(object sender, MouseEventArgs e)
        {
            BtnKey.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#F98425"));
        }

        private void BtnKey_MouseLeave(object sender, MouseEventArgs e)
        {
            BtnKey.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
        }

        private void BtnTrash_MouseEnter(object sender, MouseEventArgs e)
        {
            BtnTrash.Foreground = Brushes.DarkRed;
        }

        private void BtnTrash_MouseLeave(object sender, MouseEventArgs e)
        {
            BtnTrash.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            refreshRoom("");
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            RoomMaker roomMaker = new RoomMaker();
            roomMaker.Show();
        }

        private async Task<FeedsJson.FeedProprety> getDataJson(string feed)
        {
            HttpRequest req = new HttpRequest();
            string feedData = await req.getJsonData("https://io.adafruit.com/api/v2/" + MqttConnection.UserName + "/feeds/" + feed + "?X-AIO-Key=" + MqttConnection.AioKey);
            FeedsJson.FeedProprety dataJson = JsonConvert.DeserializeObject<FeedsJson.FeedProprety>(feedData);
            //Clipboard.SetText(feedData);
            return dataJson;
        }

        private async void BtnShare_Click(object sender, RoutedEventArgs e)
        {
            var KeyRoomSelected = selectRoom.Name.Split('_');
            FeedsJson.FeedProprety feedProprety = await getDataJson(KeyRoomSelected[2]);
            var licenseValue = feedProprety.license.Split('*');
            string KeyRoomDec = feedProprety.key + "|" + MqttConnection.UserName + "|" + MqttConnection.AioKey + "|" + licenseValue[0] + "|" + MqttConnection.NickName + "|" + licenseValue[1] + "|" + licenseValue[2] + "|" + MqttConnection.MqttServer + "|" + MqttConnection.MqttPort.ToString();
            string KeyRoomEnc = StringCipher.Encrypt(KeyRoomDec, "");
            Clipboard.SetText(KeyRoomEnc);
            DrawerHost.CloseDrawerCommand.Execute(DrawerHostContent, BtnShare);
            ShowSnackBar("Key room copied in ClipBoard!");
        }

        private async void BtnJoin_Click(object sender, RoutedEventArgs e)
        {
            var KeyRoomSelected = selectRoom.Name.Split('_');
            FeedsJson.FeedProprety feedProprety = await getDataJson(KeyRoomSelected[2]);
            if (MqttConnection.ChatRoomNameKey != feedProprety.key)
            {
                MqttConnection.MqttUnsubscribe(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey);
                var valuedata = feedProprety.license.Split('*');
                MqttConnection.ChatRoomNameKey = feedProprety.key;
                MqttConnection.Key_EndToEnd = valuedata[0];
                MqttConnection.MqttSubscribe(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey);
                MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, MqttConnection.NickName + "_JOINGROUP_" + feedProprety.name + "_" + valuedata[1]);
                DrawerHost.CloseDrawerCommand.Execute(DrawerHostContent, BtnJoin);
                ShowSnackBar("Joined successfully!");
            }
            else
            {
                DrawerHost.CloseDrawerCommand.Execute(DrawerHostContent, BtnJoin);
                ShowSnackBar("You are already joined!");
            }
        }

        private async void BtnKey_Click(object sender, RoutedEventArgs e)
        {
            HttpRequest req = new HttpRequest();
            var KeyRoomSelected = selectRoom.Name.Split('_');
            FeedsJson.FeedProprety feedProprety = await getDataJson(KeyRoomSelected[2]);
            if (MqttConnection.ChatRoomNameKey == feedProprety.key)
            {
                var valuedata = feedProprety.license.Split('*');
                string NewKey = downloadData.DownloadString("https://passwordsgenerator.net/calc.php?Length=50&Symbols=0&Lowercase=1&Uppercase=1&Numbers=1&Nosimilar=1&Last=1");
                NewKey = NewKey.Substring(0, 50);
                if (await req.UpdateFeed(feedProprety.name, feedProprety.key, feedProprety.description, NewKey, valuedata[1], valuedata[2]))
                {
                    MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, MqttConnection.NickName + "_KEY_" + NewKey);
                    DrawerHost.CloseDrawerCommand.Execute(DrawerHostContent, BtnKey);
                    ShowSnackBar("Key update successfully!");
                }
                else
                {
                    DrawerHost.CloseDrawerCommand.Execute(DrawerHostContent, BtnKey);
                    ShowSnackBar("Error updating Key!");
                }
            }
            else
            {
                DrawerHost.CloseDrawerCommand.Execute(DrawerHostContent, BtnKey);
                ShowSnackBar("At first, you must be joined to the room!");
            }
        }

        private async void BtnTrash_Click(object sender, RoutedEventArgs e)
        {
            HttpRequest req = new HttpRequest();
            var KeyRoomSelected = selectRoom.Name.Split('_');
            FeedsJson.FeedProprety feedProprety = await getDataJson(KeyRoomSelected[2]);
            MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, MqttConnection.NickName + "_CLOSEROOM_" + feedProprety.key);

            if (await req.DeleteFeed(feedProprety.key))
            {
                DrawerHost.CloseDrawerCommand.Execute(DrawerHostContent, BtnKey);
                ShowSnackBar("Room delete successfully!");
                RoomProgressCircle.IsIndeterminate = true;
                refreshRoom("");
            }
            else
            {
                DrawerHost.CloseDrawerCommand.Execute(DrawerHostContent, BtnKey);
                ShowSnackBar("Error deleting Key!");
            }

        }
    }
}
