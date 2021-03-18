using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using uPLibrary.Networking.M2Mqtt.Messages;
using MaterialDesignThemes.Wpf;
using System.Windows.Threading;
using System.Net.NetworkInformation;
using LibVLCSharp.Shared;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;
using System.Reflection;
using System.Globalization;
using System.Collections;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Net;
using System.Threading;
using System.Windows.Navigation;
using System.Windows.Media.Imaging;
using System.Windows.Documents;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Runtime.InteropServices;
using System.IO;
using mshtml;
using System.Windows.Interop;

namespace FilmParty
{

    public partial class StreamingRoom : Window
    {
        LibVLC _libVLC;
        MediaPlayer _mediaPlayer;

        private delegate void StartNewThread();
        private DispatcherTimer MeOnline = new DispatcherTimer();
        private DispatcherTimer CheckBorker = new DispatcherTimer();

        private DispatcherTimer BufferingProgressCirle = new DispatcherTimer();
        private DispatcherTimer PlayingVideo = new DispatcherTimer();
        private DispatcherTimer EndTackTimer = new DispatcherTimer();
        private DispatcherTimer PausedStatus = new DispatcherTimer();

        private DispatcherTimer StreamingDataDownload = new DispatcherTimer();
        private DispatcherTimer PingServer = new DispatcherTimer();

        private DispatcherTimer TimeBanned = new DispatcherTimer();
        Stopwatch stopWatch = new Stopwatch();

        public int NickNameTextSize = 13;
        public int MessageTextSize = 15;
        public int TimeTextSize = 13;
        public int AllertTextSize = 15;

        string MessageReceived = "";
        int[] TimeLastMessage = { 0, 0 };
        string[] Onlineuser = new string[20];
        string ServerAddress = "";

        string NickUpload;

        WebClient GetHdPassHtml = new WebClient();
        WebClient GetHdPassResLink = new WebClient();
        WebClient GetHdPassHostLink = new WebClient();
        WebClient GetAndDecryptHostLink = new WebClient();

        WebClient streamtape = new WebClient();
        WebClient supervideo = new WebClient();

        WebClient KeyEndToEnd = new WebClient();

        bool WritterDown = true;
        string[] PeopleWritting = { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""};

        bool OnlyMe = false;

        public StreamingRoom()
        {
            InitializeComponent();

            Title = "Stream Party - V " + Assembly.GetEntryAssembly().GetName().Version.ToString() + " - by Nimbus";

            KeyDown += new KeyEventHandler(StreamingRoom_KeyDown);

            stopWatch.Start();

            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);
            VideoView.Loaded += (sender, e) => VideoView.MediaPlayer = _mediaPlayer;

            ChatroomTxt.Text = "Chat - " + MqttConnection.NickName;
            IamAdmin(MqttConnection.IamAdmin);
            
            MeOnline.Tick += MeOnline_Tick;
            MeOnline.Interval = new TimeSpan(0, 0, 0, 1, 200);

            CheckBorker.Tick += CheckBorker_Tick;
            CheckBorker.Interval = new TimeSpan(0, 0, 0, 2, 0);

            EndTackTimer.Tick += EndTrack_Tick;
            EndTackTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);

            BufferingProgressCirle.Tick += BufferingProgressCirle_Tick;
            BufferingProgressCirle.Interval = new TimeSpan(0, 0, 0, 0, 10);

            PlayingVideo.Tick += PlayingVideo_Tick;
            PlayingVideo.Interval = new TimeSpan(0, 0, 0, 0, 10);

            PausedStatus.Tick += PausedStatus_Tick;
            PausedStatus.Interval = new TimeSpan(0, 0, 0, 1, 0);

            StreamingDataDownload.Tick += StreamingDataDownload_Tick;
            StreamingDataDownload.Interval = new TimeSpan(0, 0, 0, 1, 0);

            PingServer.Tick += PingServer_Tick;
            PingServer.Interval = new TimeSpan(0, 0, 0, 10, 0);

            TimeBanned.Tick += TimeBanned_Tick;
            TimeBanned.Interval = new TimeSpan(0, 0, 0, 15, 0);

            GetHdPassHtml.DownloadStringCompleted += new DownloadStringCompletedEventHandler(altaDefGetHtmlResHost);
            GetHdPassResLink.DownloadStringCompleted += new DownloadStringCompletedEventHandler(altaDefGetResLink);
            GetHdPassHostLink.DownloadStringCompleted += new DownloadStringCompletedEventHandler(altaDefGetHostLink);
            GetAndDecryptHostLink.DownloadStringCompleted += new DownloadStringCompletedEventHandler(altaDefGetAndDecryptHostLink);
            Host_Ecrypted.Add(Host_links_360);
            Host_Ecrypted.Add(Host_links_1080);
            Host_Ecrypted.Add(Host_links_2K);
            Host_Ecrypted.Add(Host_links_4K);
            Host_Decrypted.Add(Host_Decrypted_360);
            Host_Decrypted.Add(Host_Decrypted_1080);
            Host_Decrypted.Add(Host_Decrypted_2K);
            Host_Decrypted.Add(Host_Decrypted_4K);

            streamtape.DownloadStringCompleted += new DownloadStringCompletedEventHandler(stramtapeGenLink);
            supervideo.DownloadStringCompleted += new DownloadStringCompletedEventHandler(supervideoGenM3u8);

            KeyEndToEnd.DownloadStringCompleted += new DownloadStringCompletedEventHandler(getKEY);            

            if (MqttConnection.IsSolace)
            {
                RoomMakerBtn.IsEnabled = false;
                MyRoomBtn.IsEnabled = false;
                MySolaceRoomBtn.IsEnabled = true;
                ShareSolaceRoomBtn.IsEnabled = true;
            }
            else
            {
                RoomMakerBtn.IsEnabled = true;
                MyRoomBtn.IsEnabled = true;
                MySolaceRoomBtn.IsEnabled = false;
                ShareSolaceRoomBtn.IsEnabled = false;
            }
            if (MqttConnection.IsGroup)
            {
                MyRoomBtn.IsEnabled = false;
                MySolaceRoomBtn.IsEnabled = false;
            }

            Web1.Navigated += new NavigatedEventHandler(wbMain_Navigated);
        }

        private void StreamingRoom_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (WindowState == WindowState.Maximized && !SendText.IsSelectionActive)
                {
                    if (IcnExpandReduce.Kind == PackIconKind.ArrowExpandAll)
                    {
                        WindowStyle = WindowStyle.None;
                        WindowState = WindowState.Maximized;
                        IcnExpandReduce.Kind = PackIconKind.ArrowCollapseAll;
                    }
                    else
                    {
                        WindowStyle = WindowStyle.SingleBorderWindow;
                        WindowState = WindowState.Normal;
                        IcnExpandReduce.Kind = PackIconKind.ArrowExpandAll;
                    }
                    if (ControlPannel.Height == 0)
                    {
                        ToggleArrowControl.Kind = PackIconKind.KeyboardArrowDown;
                        SubControlPannel.IsEnabled = true;
                        ControlPannel.Height = 110;
                        ControlPannel.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0E1621"));
                    }
                    if (ChatRoom.Width == 0 && SuppressChat.Height == 0)
                    {
                        SuppressChat.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0E1621"));
                        SuppressChat.Height = 0;
                        ChatRoom.Width = 320;
                        SubChatRoom.Visibility = Visibility.Visible;
                    }
                }
            }
            else if (e.Key == Key.Space)
            {
                if (VideoView.MediaPlayer.WillPlay && WindowState == WindowState.Maximized)
                {
                    SubControlPannel.IsEnabled = true;
                    ButtonAutomationPeer peer = new ButtonAutomationPeer(BtnPlay);
                    IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                    invokeProv.Invoke();
                    if (ControlPannel.Height <= 14)
                    {
                        SubControlPannel.IsEnabled = false;
                    }
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!OnlyMe)
            {
                MqttConnection.client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
                MqttConnection.MqttSubscribe(MqttConnection.UserName + "/clients");
                CheckBorker.Start();
            }
            else
            {
                MqttConnection.setNickname("You");
                BtnSync.IsEnabled = false;
                //BtnUpdateUrl.IsEnabled = false;
                ChatroomTxt.Text = "Private room";
                ToggleButtonOpenMenu1.IsEnabled = false;
                MessageAllertCardBox("Hi :) you are in \"offline mode\", you can't chat with nobody but you can search and watch any film or series you want, enjoy! :)", PackIconKind.Warning, "#FFFFFF", "#c43e00");
            }
        }

        private void altaDefGetHtmlResHost(object sender, DownloadStringCompletedEventArgs e)
        {
            LinkStream.RowDefinitions.Clear();
            LinkStream.ColumnDefinitions.Clear();
            LinkStream.Children.Clear();
            HintAssist.SetHint(SendText, "Type a message");
            search = false;
            SendText.Text = "";
            TextLink.Text = "Select a host";
            Storyboard CollectingLabelShow = (Storyboard)TryFindResource("CollectingLabelShow");
            CollectingLabelShow.Begin();
            ProgressFilmLink.IsIndeterminate = true;
            Storyboard QualityLinkListUp = (Storyboard)TryFindResource("FilmLinkListUp");
            QualityLinkListUp.Begin();


            string Html_Movie = e.Result;

            try
            {
                var hdPass = e.Result.Split(new string[] { "<iframe id=\"iframeVid\" width=\"100%\" height=\"500px\" src=\"" }, StringSplitOptions.None);
                string linkHdPass = hdPass[1].Substring(0, hdPass[1].IndexOf("\""));
                //MessageBox.Show(linkHdPass);
                //Clipboard.SetText(linkHdPass);
                GetHdPassResLink.DownloadStringAsync(new Uri(linkHdPass));
            }
            catch (Exception)
            {
                EndTackTimer.Start();
                MessageAllertCardBox("Could not find link in AltaDefinizione :(", PackIconKind.DatabaseRemove, "#FFFFFF", "#9B0000");
            }
        }

        List<List<string>> Host_Ecrypted = new List<List<string>>();
        int CounterResolutionLink = 0;
        List<string> Resolution_links = new List<string>();
        List<string> Host_links_360 = new List<string>();
        List<string> Host_links_1080 = new List<string>();
        List<string> Host_links_2K = new List<string>();
        List<string> Host_links_4K = new List<string>();
        private void altaDefGetResLink(object sender, DownloadStringCompletedEventArgs e)
        {
            CounterResolutionLink = 0;
            Resolution_links.Clear();
            Host_links_360.Clear();
            Host_links_1080.Clear();
            Host_links_2K.Clear();
            Host_links_4K.Clear();

            LinkStream.RowDefinitions.Clear();
            LinkStream.ColumnDefinitions.Clear();
            LinkStream.Children.Clear();

            try
            {
                var Links = e.Result.Split(new string[] { "\"><a href=\"" }, StringSplitOptions.None);
                for (int i = 1; i < Links.Length; i++)
                {
                    string link = Links[i].Substring(0, Links[i].IndexOf("\""));
                    if (!link.Contains("host"))
                    {
                        Resolution_links.Add(link);
                    }
                }
                GetHdPassHostLink.DownloadStringAsync(new Uri(Resolution_links[CounterResolutionLink]));
            }
            catch (Exception)
            {
                EndTackTimer.Start();
                MessageAllertCardBox("Could not find Resolution link in AltaDefinizione :(", PackIconKind.DatabaseRemove, "#FFFFFF", "#9B0000");
            }
        }

        private void altaDefGetHostLink(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                var Links = e.Result.Split(new string[] { "\"><a href=\"" }, StringSplitOptions.None);
                for (int i = 1; i < Links.Length; i++)
                {
                    string link = Links[i].Substring(0, Links[i].IndexOf("\""));
                    if (link.Contains("host"))
                    {
                        if (link.Contains("resolution=5"))
                            Host_links_360.Add(link.Replace("amp;",""));
                        else if (link.Contains("resolution=3"))
                            Host_links_1080.Add(link.Replace("amp;", ""));
                        else if (link.Contains("resolution=2"))
                            Host_links_2K.Add(link.Replace("amp;", ""));
                        else if (link.Contains("resolution=1"))
                            Host_links_4K.Add(link.Replace("amp;", ""));
                    }
                }
                CounterResolutionLink++;
                if (CounterResolutionLink < Resolution_links.Count)
                {
                    GetHdPassHostLink.DownloadStringAsync(new Uri(Resolution_links[CounterResolutionLink]));
                }
                else
                {
                    CounterHoster = 0;
                    CounterHosterLink = 0;
                    Host_Decrypted_360.Clear();
                    Host_Decrypted_1080.Clear();
                    Host_Decrypted_2K.Clear();
                    Host_Decrypted_4K.Clear();
                    GetAndDecryptHostLink_start();
                }
            }
            catch (Exception)
            {
                EndTackTimer.Start();
                MessageAllertCardBox("Could not find Resolution or Host link in AltaDefinizione :(", PackIconKind.DatabaseRemove, "#FFFFFF", "#9B0000");
            }
        }

        List<List<string>> Host_Decrypted = new List<List<string>>();
        int CounterHoster = 0;
        int CounterHosterLink = 0;
        List<string> Host_Decrypted_360 = new List<string>();
        List<string> Host_Decrypted_1080 = new List<string>();
        List<string> Host_Decrypted_2K = new List<string>();
        List<string> Host_Decrypted_4K = new List<string>();

        private void GetAndDecryptHostLink_start()
        {
            if (CounterHosterLink < Host_Ecrypted[CounterHoster].Count)
            {
                GetAndDecryptHostLink.DownloadStringAsync(new Uri(Host_Ecrypted[CounterHoster][CounterHosterLink]));
            }
            else
            {
                CounterHosterLink = 0;
                CounterHoster++;
                if (CounterHoster == Host_Ecrypted.Count)
                {
                    InsertHostLinkBtn();
                    Storyboard CollectingLabelShow = (Storyboard)TryFindResource("CollectingLabelHide");
                    CollectingLabelShow.Begin();
                    ProgressFilmLink.IsIndeterminate = false;
                    return;
                }
                else
                    GetAndDecryptHostLink_start();
            }
        }

        private void altaDefGetAndDecryptHostLink(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                if (e.Result.IndexOf("<iframe allowfullscreen custom-src=\"") > 0)
                {
                    var LinksEncrypted = e.Result.Split(new string[] { "<iframe allowfullscreen custom-src=\"" }, StringSplitOptions.None);
                    string LinkEncBase64 = LinksEncrypted[1].Substring(0, LinksEncrypted[1].IndexOf("\""));
                    var base64EncodedBytes = Convert.FromBase64String(LinkEncBase64);
                    string DecBase64LinkStream = Encoding.UTF8.GetString(base64EncodedBytes);
                    //MessageBox.Show(DecBase64LinkStream);
                    Host_Decrypted[CounterHoster].Add(DecBase64LinkStream);
                }
                else
                {
                    var LinksEncrypted = e.Result.Split(new string[] { "<iframe allowfullscreen src=\"" }, StringSplitOptions.None);
                    string LinkDecBase64 = LinksEncrypted[1].Substring(0, LinksEncrypted[1].IndexOf("\""));
                    Host_Decrypted[CounterHoster].Add(LinkDecBase64);
                }

                CounterHosterLink++;
                GetAndDecryptHostLink_start();
            }
            catch (Exception)
            {
                EndTackTimer.Start();
                MessageAllertCardBox("Error while retrieving data :(", PackIconKind.DatabaseRemove, "#FFFFFF", "#9B0000");
            }
        }

        string[] quality = { "360p", "1080p", "2K - Beta", "4K - Beta" };
        ColumnDefinition ColumnDownloadBtn = new ColumnDefinition();
        private void InsertHostLinkBtn()
        {
            ColumnDownloadBtn.Width = new GridLength(1, GridUnitType.Auto);
            LinkStream.ColumnDefinitions.Add(ColumnDownloadBtn);

            for (int i = 0; i < Host_Decrypted.Count; i++)
            {
                for (int j = 0; j < Host_Decrypted[i].Count; j++)
                {
                    if (Host_Decrypted[i][j].Contains("streamtape") || Host_Decrypted[i][j].Contains("supervideo") || Host_Decrypted[i][j].Contains("hdmario"))
                    {
                        CreateBtnLinkHost(Host_Decrypted[i][j], quality[i], handlerLinksClick);
                    }
                }
            }
        }
        
        private void CreateBtnLinkHost(string LinkHost, string Quality, RoutedEventHandler Handler)
        {
            Button LinkButton = new Button();
            if (Quality.Contains("QUALITY"))
            {
                LinkButton = ReplayCreator(Quality, LinkHost, (SolidColorBrush)(new BrushConverter().ConvertFrom("#0D151F")), (SolidColorBrush)(FindResource("SecondaryColorMessage")), "", true);
                LinkButton.Tag = LinkHost;
                Button downloadBtn = generateCirleButton(PackIconKind.Download, 40, 20);
                downloadBtn.Tag = LinkHost;
                Grid.SetRow(downloadBtn, LinkStream.RowDefinitions.Count);
                Grid.SetColumn(downloadBtn, 1);
                LinkStream.Children.Add(downloadBtn);
                downloadBtn.Click += handlerDownloaderClick;
                LinkButton.Width = 320 - downloadBtn.Width - 6;
            }
            else if (Quality.Contains("CARD"))
            {
                LinkButton = ReplayCreator(Quality, LinkHost, (SolidColorBrush)(new BrushConverter().ConvertFrom("#0D151F")), (SolidColorBrush)(FindResource("SecondaryColorMessage")), "", true);
                LinkButton.Tag = Quality;
                LinkButton.Width = 320;
            }
            else
            {
                LinkButton = ReplayCreator("Host (" + Quality + "): " + LinkHost.Substring(8, LinkHost.IndexOf(".") - 8), "Link: " + LinkHost, (SolidColorBrush)(new BrushConverter().ConvertFrom("#0D151F")), (SolidColorBrush)(FindResource("SecondaryColorMessage")), "", true);
                LinkButton.Tag = LinkHost;
                LinkButton.Width = 320;
            }
            RippleAssist.SetFeedback(LinkButton, (SolidColorBrush)(new BrushConverter().ConvertFrom("#BDBDBD")));
            RowDefinition row = new RowDefinition();
            row.Height = new GridLength(1, GridUnitType.Auto);
            Grid.SetRow(LinkButton, LinkStream.RowDefinitions.Count);
            ColumnDefinition Col = new ColumnDefinition();
            Col.Width = new GridLength(1, GridUnitType.Auto);
            Grid.SetColumn(LinkButton, 0);
            LinkStream.ColumnDefinitions.Add(Col);
            LinkStream.RowDefinitions.Add(row);

            LinkStream.Children.Add(LinkButton);
            LinkButton.Click += Handler;
        }

        private void handlerLinksClick(object sender, RoutedEventArgs e)
        {
            Button ButtonLink = (Button)sender;
            string link = ButtonLink.Tag.ToString();
            //MessageBox.Show(link);
            if (link.Contains("streamtape"))
            {
                streamtape.DownloadStringAsync(new Uri(link));
            }
            else if (link.Contains("supervideo"))
            {
                supervideo.DownloadStringAsync(new Uri(link));
            }
            else if (link.Contains("hdmario"))
            {
                getHdMarioM3u8(link);
            }
            Storyboard s = (Storyboard)TryFindResource("FilmLinkListDown");
            s.Begin();
        }

        private void handlerDownloaderClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This feature will be implemented in future versions with a premium version.");
        }

        private void stramtapeGenLink(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                //Clipboard.SetText(e.Result);
                var token_Link = e.Result.Split(new string[] { "<div id=\"videolink\" style=\"display:none;\">" }, StringSplitOptions.None);
                string fakeLink = token_Link[1].Substring(0, token_Link[1].IndexOf("<"));
                var SplitFakelink = fakeLink.Split(new string[] { "token=" }, StringSplitOptions.None);
                string PartialTrueLink = SplitFakelink[0];
                //MessageBox.Show(PartialTrueLink);

                var TokenSplit = e.Result.Split(new string[] { "token=" }, StringSplitOptions.None);
                //MessageBox.Show(TokenSplit[3]);
                string TrueBrokenlink = TokenSplit[3].Substring(0, TokenSplit[3].IndexOf("</script>"));
                string Token = TrueBrokenlink.Substring(0, 12);

                string linkstreamTape = "http:" + PartialTrueLink + "token=" + Token;
                //MessageBox.Show(linkstreamTape);
                setVideoPlayer(NickUpload, "URL:" + linkstreamTape);
            }
            catch (Exception)
            {
                EndTackTimer.Start();
                MessageAllertCardBox("Could not find link stream :(", PackIconKind.DatabaseRemove, "#FFFFFF", "#9B0000");
            }

        }

        private void supervideoGenM3u8(object sender, DownloadStringCompletedEventArgs e)
        {
            string ServerNumber = "";
            string MovieToken = "";
            var UniqueVarInHtml = e.Result.Split('|');
            for (int i = 3; i < UniqueVarInHtml.Length; i++)
            {
                if (UniqueVarInHtml[i].Contains("zfs"))
                {
                    ServerNumber = UniqueVarInHtml[i];
                }
                else if (UniqueVarInHtml[i].Length == 60)
                {
                    MovieToken = UniqueVarInHtml[i];
                }
            }
            string linkSupervideoM3u8 = "https://" + ServerNumber + ".serversicuro.cc/hls/" + MovieToken + "/index-v1-a1.m3u8";
            setVideoPlayer(NickUpload, "URL:" + linkSupervideoM3u8);
        }

        string Movie_Id = "";
        private void getHdMarioM3u8(string link)
        {
            var Id = link.Split('/');
            Movie_Id = Id[4];
            Web1.LoadCompleted += new LoadCompletedEventHandler(completeLoadWebHdMario);
            Web1.Navigate(new Uri("https://hdmario.live/login/" + Movie_Id));
        }

        long MovieTime = 0;
        bool HdMarioMovie = false;
        string AccessKeyMovie = "";
        private void completeLoadWebHdMario(object sender, NavigationEventArgs na)
        {
            var doc = Web1.Document as HTMLDocument;
            string html = doc.body.outerHTML;

            if (html.Contains("<INPUT id=email class=form-control name=email required placeholder=\"Email\">"))
            {
                //MessageBox.Show("login page");
                AccessKeyMovie = "";
                var inputEmail = doc.getElementsByName("email");
                var inputPassword = doc.getElementsByName("password");
                foreach (IHTMLElement element in inputEmail)
                {
                    element.setAttribute("value", "email");
                    break;
                }
                foreach (IHTMLElement element in inputPassword)
                {
                    element.setAttribute("value", "password");
                    break;
                }
                IHTMLElementCollection buttons = doc.getElementsByTagName("button");
                foreach (IHTMLElement el in buttons)
                {
                    el.click();
                }
            }
            else if (html.Contains("#EXTM3U"))
            {
                //Clipboard.SetText(html);
                Web1.LoadCompleted -= completeLoadWebHdMario;
                Web1.Navigate("about:blank");
                HdMarioMovie = true;

                // TOTAL PARSING FILE M3U8
                var M3u8_movie_pars_1 = html.Split('>');
                var M3u8_movie_pars_2 = M3u8_movie_pars_1[1].Split('<');
                string M3u8_Worst = M3u8_movie_pars_2[0];
                var M3u8_Parsing_1 = M3u8_Worst.Split('#');
                string M3u8_Parsed_1 = "";
                for (int i = 1; i < M3u8_Parsing_1.Length; i++)
                {
                    M3u8_Parsed_1 = M3u8_Parsed_1 + "#" + M3u8_Parsing_1[i] + '\n';
                }
                var M3u8_Parsing_2 = M3u8_Parsed_1.Split(new string[] { ", " }, StringSplitOptions.None);
                string M3u8_Parsed_2 = "";
                for (int i = 0; i < M3u8_Parsing_2.Length; i++)
                {
                    M3u8_Parsed_2 = M3u8_Parsed_2 + M3u8_Parsing_2[i] + "," + '\n';
                }
                string M3u8_Parsed = M3u8_Parsed_2.Substring(0, M3u8_Parsed_2.Length - 3).Replace(" ", "");

                // SAVE FILE M3U8
                /*if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\m3u8"))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\m3u8");
                }
                using (StreamWriter ConnectionData = new StreamWriter(Directory.GetCurrentDirectory() + "\\m3u8\\" + Movie_Id + ".m3u8"))
                {
                    ConnectionData.WriteLine(M3u8_Parsed);
                }*/
                //Clipboard.SetText(M3u8_Parsed);
                //MessageBox.Show("done m3u8");


                // GET VIDEO DURATION
                var SplitSeconds = M3u8_Parsed.Split(new string[] { "#EXTINF:" }, StringSplitOptions.None);
                List<double> only_seconds = new List<double>();
                double video_seconds = 0;
                for (int i = 1; i < SplitSeconds.Length; i++)
                {
                    only_seconds.Add(double.Parse(SplitSeconds[i].Substring(0, SplitSeconds[i].IndexOf(','))));
                    video_seconds = video_seconds + double.Parse(SplitSeconds[i].Substring(0, SplitSeconds[i].IndexOf(',')));
                }
                MovieTime = (long)video_seconds * 1000;


                // BITRATE SELECTOR                
                LinkStream.RowDefinitions.Clear();
                LinkStream.ColumnDefinitions.Clear();
                LinkStream.Children.Clear();
                CreateBtnLinkHost("UHD - 3840x2160 - 30000 kb/s", "BEST QUALITY (you must have a good PC)", handlerBitrateClick);
                CreateBtnLinkHost("QHD - 2560x1440 - 17000 kb/s", "BEST-MEDIUM QUALITY (you should have a good PC)", handlerBitrateClick);
                CreateBtnLinkHost("FHD - 1920x1080 - 15000 kb/s", "MEDIUM QUALITY (you can use a normal PC)", handlerBitrateClick);
                CreateBtnLinkHost("HD - 1280x720 - 10000 kb/s", "MEDIUM-LOW QUALITY (any PC)", handlerBitrateClick);
                CreateBtnLinkHost("Bad - 854x480 - 5000 kb/s", "LOW QUALITY (any PC)", handlerBitrateClick);
                HintAssist.SetHint(SendText, "Type a message");
                search = false;
                SendText.Text = "";
                TextLink.Text = "Select streaming bitrate";
                ProgressFilmLink.IsIndeterminate = false;
                Storyboard BitrateLinkListUp = (Storyboard)TryFindResource("FilmLinkListUp");
                BitrateLinkListUp.Begin();
            }
            else
            {
                //MessageBox.Show("movie page");
                //Clipboard.SetText(html);
                var pageSplitBest = html.Split(new string[] { "var|function" }, StringSplitOptions.None);
                var SplittingWords = pageSplitBest[1].Split('|');
                string words = "if-url-return-this-index-new-window-cast-data-indexOf-else-tvu-match-hostname-ua-callback-TextEncoder-utf8-chrome-false-userAgent-length-https-split-true-typeof-player-str-encode-encoder-result-for-module-TV-target-undefined-sParameterName-navigator-lines-source-link-Object-exports-ITALIANO-dl-file-instantiate-encoderIE-utf-IsTVClass-type-key-xhr-encodeM3u8Playlist-click-isTV-define-100-replace-m3u8-IM-Proof-Secure-http-global-hdpass-storage-movieSubtitleFiles-srt-sURLVariables-kind-captions-default-label-json-startJwPlayer-on-getUrlParameter-view_saved-arguments-line-prototype-sendCorrupted-log-get-request-media-console-jQuery-onError-responseURL-apple-origOpen-tv_other-extractHostname-live-tv_panasonic-tv_toshiba-tv_lg-tv_sony-tv_samsung-tv_google-tv_apple-regex-assign-hdmario-samsung-document-preventDefault-null-apiConfig-sParam-sessionRequest-sPageURL-jwplayer-location-button-setRequestHeader-onSessionRequestSuccess-google-convertLinkToBlob-general_playlist_url-other-panasonic-toshiba-lg-sony-INGLESE-FORCED-position-SmartTV-receiverListener-charCodeAt-isUC-tv-XMLHttpRequest-body-textData-unescape-encodeURIComponent-Uint8Array-mediaInfo-mpegURL-Date-application-tid-test-bef-af-getTime-session-iOS-Sony-encodeDatas-sv-event-addEventListener-URL-error-availability-open-createObjectURL-Blob-setInterval-onInitSuccess-debugger-substring-stopPropagation-master-requestSession-search-amd-initialize-decodeURIComponent-Viera-Class-CrKey-Espial-NETTV-TSB-NetCast-LG-Tizen-converted-SMART-GoogleTV-clearInterval-AppleTV-beforeSend-ajaxSetup-captionLabel-removeItem-localStorage-HbbTV-href-HDMI-auto-throw-viewable-atob-AVAILABLE-ReceiverAvailability-started-load-60-seekRange-MediaInfo-metadata-duration-currentTime-time-500-play-show-video-seeked-MSStream-join-headers-ajax-Mac-appVersion-UCBrowser-iPod-mp4-iPhone-iPad-fail-let-onMediaLoadSuccess-loadMedia-LoadRequest-setTimeout-evt-TypeError-video_container-autostart-mute-playbackRateControls-onXhrOpen-hls-sources-secure-success-DEFAULT_MEDIA_RECEIVER_APP_ID-ApiConfig-call-hasOwnProperty-sessionListener-in-preload-appid-setup-initializeCastApi-status-404-output_-corrupted-apply-ready-tracks-00000000-hlshtml-html5-primary-androidhls-SessionRequest-volume-base";
                List<string> probe_X_Secure_Proof = new List<string>();
                for (int i = 0; i < SplittingWords.Length; i++)
                {
                    if (!words.Contains(SplittingWords[i]) && !SplittingWords[i].Contains("</SCRIPT>") && !SplittingWords[i].Contains("contextmenu") && SplittingWords[i] != Movie_Id)
                    {
                        probe_X_Secure_Proof.Add(SplittingWords[i]);
                    }
                }
                for (int i = 0; i < probe_X_Secure_Proof.Count; i++)
                {
                    if (probe_X_Secure_Proof[i].Length == 22)
                    {
                        //MessageBox.Show("https://hdmario.live/pl/" + Movie_Id + ".m3u8?s=" + probe_X_Secure_Proof[i]);
                        //Clipboard.SetText("https://hdmario.live/pl/" + Movie_Id + ".m3u8?s=" + probe_X_Secure_Proof[i]);
                        AccessKeyMovie = probe_X_Secure_Proof[i];
                        Web1.Navigate(new Uri("https://hdmario.live/pl/" + Movie_Id + ".m3u8?s=" + AccessKeyMovie));
                        return;
                    }
                    else if (probe_X_Secure_Proof[i].Length == 12)
                    {
                        for (int j = 0; j < probe_X_Secure_Proof.Count; j++)
                        {
                            if (probe_X_Secure_Proof[j].Length == 9)
                            {
                                AccessKeyMovie = probe_X_Secure_Proof[j] + "-" + probe_X_Secure_Proof[i];
                                Web1.Navigate(new Uri("https://hdmario.live/pl/" + Movie_Id + ".m3u8?s=" + AccessKeyMovie));
                                return;
                            }
                        }
                    }
                }
                MessageAllertCardBox("Sorry, currently unable to access the resource :(", PackIconKind.DatabaseRemove, "#FFFFFF", "#9B0000");
            }
        }

        string selected_Video_Res = "";
        string selected_Video_Bit = "";
        private async void handlerBitrateClick(object sender, RoutedEventArgs e)
        {
            MessageAllertCardBox("Searching m3u8 playlist crypto stream...", PackIconKind.PlaylistCheck, "#FFFFFF", "#123B82");
            Storyboard BitrateLinkListDown = (Storyboard)TryFindResource("FilmLinkListDown");
            BitrateLinkListDown.Begin();
            selected_Video_Res = "";
            selected_Video_Bit = "";
            Button BitrateLinkBtn = (Button)sender;
            BufferingCircle.IsIndeterminate = true;
            string tagBtn = BitrateLinkBtn.Tag.ToString();
            var Quality_Bitrate = tagBtn.Split(new string[] { " - " }, StringSplitOptions.None);
            selected_Video_Res = Quality_Bitrate[1].Replace("x", ":");
            selected_Video_Bit = Quality_Bitrate[1].Substring(0, 2) + "M";

            // START FFMPEG STREAMING
            startTcpLocalStreaming("https://hdmario.live/pl/" + Movie_Id + ".m3u8?s=" + AccessKeyMovie, "0", selected_Video_Res, selected_Video_Bit);

            // STOP MAIN THREAD 2 SEC (Waiting ffmpeg streaming)
            await Task.Run(() => Thread.Sleep(2300));

            // SET VIDEO ON PLAYER
            setVideoPlayer(NickUpload, "URL:tcp://127.0.0.1:8080");
        }

        void wbMain_Navigated(object sender, NavigationEventArgs e)
        {
            SetSilent(Web1, true); // make it silent
        }
        public static void SetSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");

            // get an IWebBrowser2 from the document
            IOleServiceProvider sp = browser.Document as IOleServiceProvider;
            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }

        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }

        public Process ffmpeg_process = new Process();
        private void startTcpLocalStreaming(string link_M3u8, string seconds, string resolution, string v_bitrate)
        {
            ffmpeg_process.StartInfo.FileName = "streamer.exe";
            if (seconds != "0")
            {
                ffmpeg_process.StartInfo.Arguments = "-protocol_whitelist file,http,https,tcp,tls,crypto -ss " + seconds + " -i " + link_M3u8 + " -vf \"scale = " + resolution + "\" -b:v " + v_bitrate + " -f mpegts tcp://127.0.0.1:8080?listen";
            }
            else
            {
                ffmpeg_process.StartInfo.Arguments = "-protocol_whitelist file,http,https,tcp,tls,crypto -i " + link_M3u8 + " -vf \"scale = " + resolution + "\" -b:v " + v_bitrate + " -f mpegts tcp://127.0.0.1:8080?listen";
            }
            ffmpeg_process.StartInfo.CreateNoWindow = true;
            ffmpeg_process.StartInfo.UseShellExecute = false;
            //ffmpeg_process.StartInfo.RedirectStandardOutput = true;
            //ffmpeg_process.StartInfo.RedirectStandardInput = true;
            //ffmpeg_process.OutputDataReceived += new DataReceivedEventHandler(read_ffmpegOutput);
            ffmpeg_process.Start();
        }

        string output = "";
        private void read_ffmpegOutput(object sender, DataReceivedEventArgs e)
        {
            output = output + e.Data;
            Debug.WriteLine(output);
        }






        private void IamAdmin(bool value)
        {
            if (!value)
            {
                AdminControl.IsEnabled = false;
            }
            else
            {
                AdminControl.IsEnabled = true;
            }
        }

        string copyNickname = "";
        public void InsertMessage()
        {
            if (MessageReceived.Contains("client") && MessageReceived.Contains("status") && MessageReceived.Contains("id") && (MessageReceived.Contains("remoteAddress") || MessageReceived.Contains("ip")))
            {
                try
                {
                    //Clipboard.SetText(MessageReceived);
                    //MessageBox.Show(MessageReceived);
                    var value = MessageReceived.Split('"');
                    var User_nickName = value[13].Split('_');
                    if (value[3] == "connected")
                    {
                        if (controllUserOnlineYet(User_nickName[1]))
                        {
                            if (MqttConnection.NickName != User_nickName[1])
                            {
                                MessageAllertCardBox(User_nickName[1] + " joined in chatroom", PackIconKind.SubdirectoryArrowRight, "#FFFFFF", "#5E35B1");
                                addUserOnline(User_nickName[1]);
                                addVectOnlineUser(User_nickName[1]);
                            }
                            else
                            {
                                copyNickname = User_nickName[1];
                            }
                            MeOnline.Start();
                        }
                    }
                    else if (value[3] == "disconnected")
                    {
                        if (MqttConnection.NickName != User_nickName[1])
                        {
                            if (User_nickName[1] != copyNickname)
                            {
                                MessageAllertCardBox(User_nickName[1] + " left chatroom", PackIconKind.SubdirectoryArrowLeft, "#FFFFFF", "#5E35B1");
                                StopWritting(User_nickName[1]);
                                removeUserOnline(User_nickName[1]);
                                removeOfflineUser(User_nickName[1]);
                            }
                            else
                            {
                                copyNickname = "";
                            }
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
            else
            {
                try
                {
                    string DecryptedText = StringCipher.Decrypt(MessageReceived, MqttConnection.Key_EndToEnd);
                    var SplitDecryptedText = DecryptedText.Split('_');
                    if (SplitDecryptedText[1] != "NOONLY")
                    {
                        if (!CheckVectOnlineUser(Onlineuser, SplitDecryptedText[0]))
                        {
                            addUserOnline(SplitDecryptedText[0]);
                            addVectOnlineUser(SplitDecryptedText[0]);
                        }

                        if (SplitDecryptedText[1] == "START-TYPING")
                        {
                            if (MqttConnection.NickName != SplitDecryptedText[0])
                            {
                                int count = 0;
                                string peopletyping = "";
                                for (int i = 0; i < 49; i++)
                                {
                                    if (PeopleWritting[i] == "")
                                    {
                                        PeopleWritting[i] = SplitDecryptedText[0];
                                        break;
                                    }
                                }

                                bool manypeople = false;
                                for (int i = 0; i < 49; i++)
                                {
                                    if (PeopleWritting[i] != "")
                                    {
                                        peopletyping = peopletyping + PeopleWritting[i] + ", ";
                                        count++;
                                    }
                                }

                                if (count > 4)
                                {
                                    manypeople = true;
                                }

                                if (!manypeople)
                                {
                                    string pTy = peopletyping.Substring(0, peopletyping.Length - 2);
                                    if (pTy.Contains(","))
                                    {
                                        TypingUser.Text = pTy + " are typing...";
                                    }
                                    else
                                    {
                                        TypingUser.Text = pTy + " is typing...";
                                    }
                                }
                                else
                                {
                                    TypingUser.Text = "Many people are typing...";
                                }

                                if (WritterDown)
                                {
                                    Storyboard AnimationDots = (Storyboard)TryFindResource("DotsTyping");
                                    Storyboard WritingUp = (Storyboard)TryFindResource("WritingUp");
                                    AnimationDots.Begin();
                                    WritingUp.Begin();
                                }
                                WritterDown = false;
                            }
                        }
                        else if (SplitDecryptedText[1] == "STOP-TYPING")
                        {
                            StopWritting(SplitDecryptedText[0]);
                        }
                        else if (SplitDecryptedText[1] == "VIDEOPLAYER")
                        {
                            string Url = DecryptedText.Substring(SplitDecryptedText[0].Length + SplitDecryptedText[1].Length + 2);
                            ChosePlayerMood(SplitDecryptedText[0], Url);
                        }
                        else if (SplitDecryptedText[1] == "SERIEPLAYER")
                        {
                            if (DecryptedText.Contains("_URL:"))
                            {
                                string Url = DecryptedText.Substring(SplitDecryptedText[0].Length + SplitDecryptedText[1].Length + 2);
                                if (Url.Contains("https://seriehd."))
                                {
                                    MessageAllertCardBox("Searching series in SerieHD database...", PackIconKind.DatabaseSearch, "#FFFFFF", "#123B82");
                                    NickUpload = SplitDecryptedText[0];
                                    SerieHD_Client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DumpSeriesMetaData);
                                    SerieHD_Client.DownloadStringAsync(new Uri(Url.Substring(4)));
                                }
                            }

                            if (SplitDecryptedText[2].Contains("PLAYURL"))
                            {
                                NickUpload = SplitDecryptedText[0];
                                var SplitSeasonUrl = DecryptedText.Split(':');
                                ActionOnPlay(SplitSeasonUrl[1], SplitSeasonUrl[2]);
                                CloseLinks.Click -= CloseLinks_Click;
                                CloseLinks.Click += SeriesMiniBanner;
                                streamtape.DownloadStringAsync(new Uri("http:" + SplitSeasonUrl[4]));
                            }
                            else if (SplitDecryptedText[2].Contains("EXIT"))
                            {
                                stopSeries(SplitDecryptedText[0] + " ");
                            }
                            else if (SplitDecryptedText[2].Contains("SKIPP"))
                            {
                                SkipperEpisode(SplitDecryptedText[0]);
                            }
                            else if (SplitDecryptedText[2].Contains("CLOSEVIEWER"))
                            {
                                CloseSeriesViewer(SplitDecryptedText[0]);
                            }

                        }
                        else if (SplitDecryptedText[1] == "ROOMGROUP")
                        {
                            ChatroomTxt.Text = SplitDecryptedText[2];
                            ChatBoxContainer.Children.Clear();
                            ChatBoxContainer.RowDefinitions.Clear();
                            MessageAllertCardBox(SplitDecryptedText[0] + " have created new room: " + SplitDecryptedText[2] + " and updated link stream, please wait...", PackIconKind.UserGroup, "#212121", "#FFFFFF");
                            ChangeStreamingUrl(SplitDecryptedText[3], false);
                        }
                        else if (SplitDecryptedText[1] == "JOINGROUP")
                        {
                            if (!MqttConnection.IsGroup)
                            {
                                ChatroomTxt.Text = SplitDecryptedText[2];
                                ChatBoxContainer.Children.Clear();
                                ChatBoxContainer.RowDefinitions.Clear();
                                MessageAllertCardBox("Join in the room: " + SplitDecryptedText[2] + '\n' + "Sync link stream, please wait...", PackIconKind.Share, "#FFFFFF", "#5E35B1");
                                ChangeStreamingUrl(SplitDecryptedText[3], false);
                            }
                            else
                            {
                                MessageAllertCardBox(SplitDecryptedText[0] + "(Admin) joined in room", PackIconKind.Share, "#FFFFFF", "#5E35B1");
                            }
                        }
                        else if (SplitDecryptedText[1] == "KEY")
                        {
                            MessageAllertCardBox(SplitDecryptedText[0] + " updated Key End-To-End", PackIconKind.VpnKey, "#FFFFFF", "#FF6F00");
                            MqttConnection.Key_EndToEnd = SplitDecryptedText[2];
                        }
                        else if (SplitDecryptedText[1] == "CLOSEROOM")
                        {
                            if (MqttConnection.ChatRoomNameKey == SplitDecryptedText[2])
                            {
                                MessageAllertCardBox(SplitDecryptedText[0] + " delete this room", PackIconKind.Trash, "#FFFFFF", "#9B0000");
                                EndTackTimer.Start();
                                ChatroomTxt.Text = "Main-Chat";
                                MqttConnection.MqttUnsubscribe(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey);
                                MqttConnection.ChatRoomNameKey = "chat";
                                MqttConnection.setKeyEndToEnd("");
                                MqttConnection.MqttSubscribe(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey);
                            }
                        }
                        else if (SplitDecryptedText[1] == "u$4gy@54nvnd" && SplitDecryptedText[2] == "BANNED")
                        {
                            MessageAllertCardBox(SplitDecryptedText[0] + " was banned for 15 seconds.", PackIconKind.Ban, "#FFFFFF", "#5E35B1");
                            MessageReceived = "";
                        }
                        else
                        {
                            controllBan(SplitDecryptedText[0]);
                            if (SplitDecryptedText[1] == "REPLAYMESSAGE")
                            {
                                var nickToResponse_textToResponse_messageReponse = SplitDecryptedText[2].Split(new string[] { "}£(&{$)(" }, StringSplitOptions.None);
                                AddMessage(SplitDecryptedText[0], nickToResponse_textToResponse_messageReponse[2], true, nickToResponse_textToResponse_messageReponse[0], nickToResponse_textToResponse_messageReponse[1], nickToResponse_textToResponse_messageReponse[3]);
                            }
                            else
                            {
                                StopWritting(SplitDecryptedText[0]);
                                AddMessage(SplitDecryptedText[0], SplitDecryptedText[1], false, "", "", "");
                            }

                            //TypingUser.Text = "";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    MessageAllertCardBox("Error while decrypting the message: bad key End-To-End", PackIconKind.AlertBoxOutline, "#FFFFFF", "#9B0000");
                }
            }

            MessageReceived = "";
        }

        private void StopWritting(string NickName)
        {
            if (MqttConnection.NickName != NickName)
            {
                if (!WritterDown)
                {
                    for (int i = 0; i < 49; i++)
                    {
                        if (PeopleWritting[i] == NickName)
                        {
                            PeopleWritting[i] = "";

                            int count = 0;
                            string peopletyping = "";
                            if (TypingUser.Text == "Many people are typing...")
                            {
                                bool manypeople = false;
                                for (int j = 0; j < 49; j++)
                                {
                                    if (PeopleWritting[j] != "")
                                    {
                                        peopletyping = peopletyping + PeopleWritting[j] + ", ";
                                        count++;
                                    }
                                }
                                if (count > 4)
                                {
                                    manypeople = true;
                                }

                                if (!manypeople)
                                {
                                    string pTy = peopletyping.Substring(0, peopletyping.Length - 2);
                                    if (pTy.Contains(","))
                                    {
                                        TypingUser.Text = pTy + " are typing...";
                                    }
                                    else
                                    {
                                        TypingUser.Text = pTy + " is typing...";
                                    }
                                }
                                else
                                {
                                    TypingUser.Text = "Many people are typing...";
                                }
                            }
                            else
                            {
                                RemoveUserTyping(NickName);
                            }                            

                            break;
                        }
                    }
                }
            }
        }

        private void RemoveUserTyping(string NickName)
        {
            TypingUser.Text = TypingUser.Text.Replace(" " + NickName + ",", "");
            TypingUser.Text = TypingUser.Text.Replace(NickName + ", ", "");
            TypingUser.Text = TypingUser.Text.Replace(", " + NickName, "");
            TypingUser.Text = TypingUser.Text.Replace(NickName + " ", "");

            if (!TypingUser.Text.Contains(","))
            {
                TypingUser.Text = TypingUser.Text.Replace("are", "is");
            }

            if (TypingUser.Text == "is typing..." || TypingUser.Text == "are typing...")
            {
                if (!WritterDown)
                {
                    Storyboard AnimationDots = (Storyboard)TryFindResource("DotsTyping");
                    Storyboard WritingDown = (Storyboard)TryFindResource("WritingDown");
                    AnimationDots.Stop();
                    WritingDown.Begin();
                    WritterDown = true;
                }
            }
        }

        private void controllBan(string NickName)
        {
            if (NickName == MqttConnection.NickName)
            {
                if (TimeLastMessage[0] == 0)
                {
                    TimeLastMessage[0] = (int)stopWatch.ElapsedMilliseconds;
                    return;
                }
                else if (TimeLastMessage[1] == 0)
                {
                    TimeLastMessage[1] = (int)stopWatch.ElapsedMilliseconds;
                    return;
                }
                else if (TimeLastMessage[0] != 0 && TimeLastMessage[1] != 0)
                {
                    TimeLastMessage[0] = TimeLastMessage[1];
                    TimeLastMessage[1] = (int)stopWatch.ElapsedMilliseconds;
                }

                if (TimeLastMessage[1] - TimeLastMessage[0] < 450)
                {
                    TimeLastMessage[0] = 0;
                    TimeLastMessage[0] = 0;
                    SendText.IsEnabled = false;
                    HintAssist.SetHint(SendText, "BANNED FOR 15 SECONDS");
                    MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, MqttConnection.NickName + "_u$4gy@54nvnd_BANNED");
                    TimeBanned.Start();
                }
            }
            else
            {
                return;
            }
        }

        public void TimeBanned_Tick(object sender, EventArgs e)
        {
            SendText.IsEnabled = true;
            SendText.Focus();
            HintAssist.SetHint(SendText, "Type a message");
            TimeBanned.Stop();
        }

        public void MeOnline_Tick(object sender, EventArgs e)
        {
            MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, "RESPONSE_NOONLY_" + MqttConnection.NickName);
            MeOnline.Stop();
        }

        private void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            MessageReceived = Encoding.UTF8.GetString(e.Message);
            this.Dispatcher.Invoke(new StartNewThread(InsertMessage));
        }

        bool ErrorConnectedYet = false;
        public async void CheckBorker_Tick(object sender, EventArgs e)
        {
            if (!MqttConnection.client.IsConnected)
            {
                SendText.IsEnabled = false;
                if (!ErrorConnectedYet)
                {
                    MqttConnection.MqttDisconnect();
                    MessageAllertCardBox("Disconnected from server, try to reconnect...", PackIconKind.ServerOff, "#FFFFFF", "#B71C1C");
                    ErrorConnectedYet = true;
                    return;
                }
                if (await MqttConnection.MqttFastConnect())
                {
                    MqttConnection.MqttSubscribe(MqttConnection.UserName + "/clients");
                    MqttConnection.MqttSubscribe(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey);
                    MessageAllertCardBox("OK reconnected", PackIconKind.Tick, "#FFFFFF", "#2E7D32");
                    ErrorConnectedYet = false;
                    SendText.IsEnabled = true;
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

        bool sendUrl = false;
        bool search = false;
        bool SerieSearch = false;
        private async void SendText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                {
                    SendText.AppendText(Environment.NewLine);
                    SendText.SelectionStart = SendText.Text.Length;
                    SendText.SelectionLength = 0;
                    return;
                }
                if (!string.IsNullOrWhiteSpace(SendText.Text))
                {
                    if (!sendUrl && !search && !SerieSearch && !OnlyMe)
                    {
                        string message = SendText.Text;
                        if (ResponseContainer.Height == 0)
                        {
                            await Task.Run(() => MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, MqttConnection.NickName + "_" + message));
                        }
                        else if (ResponseContainer.Height == 50)
                        {
                            string nicknameResponse = ResponseNickName.Text;
                            string messageResponse = ResponseMessage.Text;
                            string positionReplay = ResponseNickName.Tag.ToString();
                            await Task.Run(() => MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, MqttConnection.NickName + "_REPLAYMESSAGE_" + nicknameResponse + "}£(&{$)(" + messageResponse + "}£(&{$)(" + message + "}£(&{$)(" + positionReplay));
                            Storyboard WritingDown = (Storyboard)TryFindResource("ReplayDown");
                            WritingDown.Begin();
                        }
                        SendText.Text = "";
                    }
                    else if (sendUrl && !search && !SerieSearch)
                    {
                        if (!OnlyMe)
                        {
                            MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, MqttConnection.NickName + "_VIDEOPLAYER_URL:" + SendText.Text);
                        }
                        else
                        {
                            ChosePlayerMood("You", "URL:" + SendText.Text);                            
                        }
                        HintAssist.SetHint(SendText, "Type a message");
                        BtnStop.IsEnabled = true;
                        BtnUpdateUrl.IsEnabled = false;
                        sendUrl = false;
                        SendText.Text = "";
                    }
                    else if (!sendUrl && search && !SerieSearch)
                    {
                        if (linkDNS == "")
                        {
                            WebClient webClient = new WebClient();
                            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownloadNewLink);
                            webClient.DownloadStringAsync(new Uri("https://altadefinizione-nuovo.net/"));
                        }
                        else
                        {
                            try
                            {
                                SearchLinkAltaDef(linkDNS);
                            }
                            catch (Exception)
                            {
                                EndTackTimer.Start();
                                HintAssist.SetHint(SendText, "Type a message");
                                SendText.Text = "";
                                SendText.Focus();
                                Storyboard s = (Storyboard)TryFindResource("FilmLinkListDown");
                                s.Begin();
                                MessageAllertCardBox("Could not find data for stream :(", PackIconKind.DatabaseRemove, "#FFFFFF", "#9B0000");
                            }
                        }

                    }
                    else if (!sendUrl && !search && SerieSearch)
                    {
                        if (linkSeriesDNS == "")
                        {
                            WebClient webClient = new WebClient();
                            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownloadNewLink);
                            webClient.DownloadStringAsync(new Uri("https://nuovoindirizzo.info/seriehd/"));
                        }
                        else
                        {
                            try
                            {
                                SeriesSearch(linkSeriesDNS + "?s=" + SendText.Text);
                            }
                            catch (Exception)
                            {
                                EndTackTimer.Start();
                                HintAssist.SetHint(SendText, "Type a message");
                                SendText.Text = "";
                                SendText.Focus();
                                Storyboard s = (Storyboard)TryFindResource("FilmLinkListDown");
                                s.Begin();
                                MessageAllertCardBox("Could not find data for stream :(", PackIconKind.DatabaseRemove, "#FFFFFF", "#9B0000");
                            }
                        }
                    }
                }
            }
            else if (e.Key == Key.Escape)
            {
                HintAssist.SetHint(SendText, "Type a message");
                //if (!OnlyMe)
                    //BtnUpdateUrl.IsEnabled = true;
                BtnSearch.IsEnabled = true;
                BtnNetflix.IsEnabled = true;
                sendUrl = false;
                search = false;
                SerieSearch = false;
                SendText.Text = "";
            }
        }

        private void SeriesSearch(string URL)
        {
            WebClient searchSeries = new WebClient();
            searchSeries.DownloadStringCompleted += new DownloadStringCompletedEventHandler(SeriesList);
            searchSeries.DownloadStringAsync(new Uri(URL));
            HintAssist.SetHint(SendText, "Type a message");
            SerieSearch = false;
            SendText.Text = "";
            TextLink.Text = "Select a series";
            LinkStream.RowDefinitions.Clear();
            LinkStream.ColumnDefinitions.Clear();
            LinkStream.Children.Clear();
            ProgressFilmLink.IsIndeterminate = true;
            Storyboard FilmLinkListUp = (Storyboard)TryFindResource("FilmLinkListUp");
            FilmLinkListUp.Begin();
        }

        private void SeriesList(object sender, DownloadStringCompletedEventArgs e)
        {
            //Clipboard.SetText(e.Result);
            try
            {
                var splitDataSeries = e.Result.Split(new string[] { "<div class=\"col-xl-3 col-lg-3 col-md-3 col-sm-6 col-6\">" }, StringSplitOptions.None);
                for (int i = 0; i < splitDataSeries.Length - 1; i++)
                {
                    var SplitLink = splitDataSeries[i + 1].Split(new string[] { "<a href=\"" }, StringSplitOptions.None);
                    string SeriesLink = SplitLink[1].Substring(0, SplitLink[1].IndexOf('"'));
                    var SplitName = splitDataSeries[i + 1].Split(new string[] { "<h2>" }, StringSplitOptions.None);
                    string SeriesName = SplitName[1].Substring(0, SplitName[1].IndexOf('<'));
                    var SplitIMDB = splitDataSeries[i + 1].Split(new string[] { "<span class=\"imdb\">" }, StringSplitOptions.None);
                    string SeriesIMDB = SplitIMDB[1].Substring(0, SplitIMDB[1].IndexOf('<'));
                    var SplitImg = splitDataSeries[i + 1].Split(new string[] { "<img src=\"" }, StringSplitOptions.None);
                    string SeriesImg = SplitImg[1].Substring(0, SplitImg[1].IndexOf('"'));
                    //MessageBox.Show(SeriesLink + Environment.NewLine + SeriesName + Environment.NewLine + SeriesIMDB + Environment.NewLine + SeriesImg);
                    double NumStar = 0;
                    try
                    {
                        NumStar = double.Parse(SeriesIMDB);
                    }
                    catch (Exception)
                    {
                        NumStar = 8;
                    }
                    Button SeriesButton = new Button();
                    string ReduceLink = SeriesLink;
                    if (SeriesLink.Length > 40)
                    {
                        ReduceLink = SeriesLink.Substring(0, 40) + "...";
                    }
                    SeriesButton = FilmBlockCreator(SeriesName, "Link: " + ReduceLink, SeriesImg, NumStar, (SolidColorBrush)(new BrushConverter().ConvertFrom("#0D151F")), (SolidColorBrush)(FindResource("SecondaryColorMessage")), "");
                    SeriesButton.Tag = SeriesLink;
                    RowDefinition row = new RowDefinition();
                    row.Height = new GridLength(1, GridUnitType.Auto);
                    Grid.SetRow(SeriesButton, LinkStream.RowDefinitions.Count);
                    LinkStream.RowDefinitions.Add(row);
                    LinkStream.Children.Add(SeriesButton);
                    SeriesButton.Click += new RoutedEventHandler(SeriesButtonLinkClick);
                }
                ProgressFilmLink.IsIndeterminate = false;
            }
            catch (Exception)
            {
                SerieHD_Client.CancelAsync();
                BtnSearch.IsEnabled = true;
                BtnNetflix.IsEnabled = true;
                ProgressFilmLink.IsIndeterminate = false;
                CollectingLabelContainer.Width = 0;
                contSeasons = 0;
                NumbersOfSeasons = 0;
                NumberOfEpisodies = 0;
                contEpisodies = 0;
                Storyboard s = (Storyboard)TryFindResource("FilmLinkListDown");
                s.Begin();
                MessageAllertCardBox("Could not find data for stream :(", PackIconKind.DatabaseRemove, "#FFFFFF", "#9B0000");
            }

        }

        WebClient SerieHD_Client = new WebClient();
        private void SeriesButtonLinkClick(object sender, RoutedEventArgs e)
        {
            Button SeriesLinkBtn = (Button)sender;
            if (!OnlyMe)
            {
                MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, MqttConnection.NickName + "_SERIEPLAYER_URL:" + SeriesLinkBtn.Tag.ToString());
            }
            else
            {
                MessageAllertCardBox("Searching series in SerieHD database...", PackIconKind.DatabaseSearch, "#FFFFFF", "#123B82");
                SerieHD_Client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DumpSeriesMetaData);
                SerieHD_Client.DownloadStringAsync(new Uri(SeriesLinkBtn.Tag.ToString()));
            }
            HintAssist.SetHint(SendText, "Type a message");
            BtnUpdateUrl.IsEnabled = false;
            BtnSearch.IsEnabled = false;
            sendUrl = false;
            SendText.Text = "";
            SendText.Focus();
            Storyboard s = (Storyboard)TryFindResource("FilmLinkListDown");
            s.Begin();
        }

        string IdSeries = "";
        private void DumpSeriesMetaData(object sender, DownloadStringCompletedEventArgs e)
        {
            contSeasons = 0;
            NumbersOfSeasons = 0;
            NumberOfEpisodies = 0;
            contEpisodies = 0;

            if (e.Cancelled)
            {
                SerieHD_Client.DownloadStringCompleted -= new DownloadStringCompletedEventHandler(DumpSeriesMetaData);
                return;
            }

            string SeriesPageHtml = e.Result;
            var SplitSeriesInnerLink = SeriesPageHtml.Split(new string[] { "https://hdpass.click/tvShow/" }, StringSplitOptions.None);
            IdSeries = SplitSeriesInnerLink[1].Substring(0, SplitSeriesInnerLink[1].IndexOf('"'));
            var SplitTitle = SeriesPageHtml.Split(new string[] { "<meta property=\"og:title\" content=\"" }, StringSplitOptions.None);
            string Title = SplitTitle[1].Substring(0, SplitTitle[1].IndexOf('"'));
            if (Title.Length > 14)
            {
                TitleVideo.Text = Title.Substring(0, 13) + "...";
            }
            else
            {
                TitleVideo.Text = Title;
            }            
            var SplitDescription = SeriesPageHtml.Split(new string[] { "<meta property=\"og:description\" content=\"" }, StringSplitOptions.None);
            string Description = SplitDescription[1].Substring(0, SplitDescription[1].IndexOf('"'));
            var SplitImage = SeriesPageHtml.Split(new string[] { "<meta property=\"og:image\" content=\"" }, StringSplitOptions.None);
            string Image = SplitImage[1].Substring(0, SplitImage[1].IndexOf('"'));
            var SplitYear = SeriesPageHtml.Split(new string[] { "<a href=\"https://seriehd.email/anno/" }, StringSplitOptions.None);
            string Year = SplitYear[1].Substring(0, SplitYear[1].IndexOf('/'));
            var SplitTrailer = SeriesPageHtml.Split(new string[] { "src=\"https://www.youtube.com/" }, StringSplitOptions.None);
            var TrailerIdSplit = SplitTrailer[1].Split('/');
            string Trailerid = TrailerIdSplit[1].Substring(0, TrailerIdSplit[1].IndexOf('"'));
            string LinkTrailer = "https://www.youtube.com/watch?v=" + Trailerid;
            var SplitCast = SeriesPageHtml.Split(new string[] { "<label>Cast</label>"}, StringSplitOptions.None);
            string Casting = SplitCast[1].Substring(0, SplitCast[1].IndexOf("</div>"));
            var SplitCasting = Casting.Split(',');
            string cast = "";
            for (int i = 0; i < SplitCasting.Length; i++)
            {
                int start = SplitCasting[i].IndexOf('>') + 1;
                int end = SplitCasting[i].LastIndexOf('<') - 1;
                cast = cast + SplitCasting[i].Substring(start, end - start) + ", ";
            }
            cast = cast.Remove(cast.LastIndexOf(", "));

            LinkStream.RowDefinitions.Clear();
            LinkStream.ColumnDefinitions.Clear();
            LinkStream.Children.Clear();
            Grid SeriesData = SeriesDataSheet(Title, Year, cast, Image, LinkTrailer, Description);
            RowDefinition row = new RowDefinition();
            row.Height = new GridLength(1, GridUnitType.Auto);
            Grid.SetRow(SeriesData, LinkStream.RowDefinitions.Count);
            LinkStream.RowDefinitions.Add(row);
            LinkStream.Children.Add(SeriesData);
            HintAssist.SetHint(SendText, "Type a message");
            search = false;
            SendText.Text = "";
            TextLink.Text = "Series Info";
            Storyboard CollectingLabelShow = (Storyboard)TryFindResource("CollectingLabelShow");
            CollectingLabelShow.Begin();
            ProgressFilmLink.IsIndeterminate = true;
            Storyboard FilmLinkListUp = (Storyboard)TryFindResource("FilmLinkListUp");
            FilmLinkListUp.Begin();

            //MessageBox.Show(Image);
            //MessageBox.Show(Description);
            //MessageBox.Show(Title);
            //MessageBox.Show(IdSeries);
            //MessageBox.Show(Year);
            //MessageBox.Show(LinkTrailer);
            //MessageBox.Show(cast);
            SerieHD_Client.DownloadStringCompleted -= new DownloadStringCompletedEventHandler(DumpSeriesMetaData);
            SerieHD_Client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DumpSeriesLinks);
            SerieHD_Client.DownloadStringAsync(new Uri("https://hdpass.click/tvShow/" + IdSeries));
        }

        int NumbersOfSeasons = 0;
        private void DumpSeriesLinks(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                SerieHD_Client.DownloadStringCompleted -= new DownloadStringCompletedEventHandler(DumpSeriesLinks);
                return;
            }
            string SeriesPageHtml = e.Result;
            var SplitSeriesInnerLink = SeriesPageHtml.Split(new string[] { "<ul class=\"full-screen-select\">" }, StringSplitOptions.None);
            var SplitSeason = SplitSeriesInnerLink[2].Split(new string[] { "https://hdpass.click/tvShow/" + IdSeries + "?season=" }, StringSplitOptions.None);
            NumbersOfSeasons = SplitSeason.Length - 1;
            //MessageBox.Show(NumbersOfSeasons.ToString());
            SerieHD_Client.DownloadStringCompleted -= new DownloadStringCompletedEventHandler(DumpSeriesLinks);
            SerieHD_Client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DumpNumberEpisodes);
            DumpNumberEpisodesIterative();
        }

        int contSeasons = 0;
        private void DumpNumberEpisodesIterative()
        {
            SerieHD_Client.DownloadStringAsync(new Uri("https://hdpass.click/tvShow/" + IdSeries + "?season=" + contSeasons));
        }

        int NumberOfEpisodies = 0;
        int contEpisodies = 0;
        private void DumpNumberEpisodes(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                SerieHD_Client.DownloadStringCompleted -= new DownloadStringCompletedEventHandler(DumpNumberEpisodes);
                return;
            }
            string SeriesPageHtml = e.Result;
            var SplitSeriesInnerLink = SeriesPageHtml.Split(new string[] { "<ul class=\"full-screen-select\">" }, StringSplitOptions.None);
            var SplitEpisodie = SplitSeriesInnerLink[1].Split(new string[] { "https://hdpass.click/tvShow/" + IdSeries + "?season=" }, StringSplitOptions.None);
            NumberOfEpisodies = SplitEpisodie.Length - 1;
            SerieHD_Client.DownloadStringCompleted -= new DownloadStringCompletedEventHandler(DumpNumberEpisodes);
            SerieHD_Client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DumpLinksEpisodes);
            if (contSeasons < NumbersOfSeasons)
            {
                //MessageBox.Show((contSeasons + 1) + " ---> " + NumberOfEpisodies);
                Grid season = SeasonCard(contSeasons + 1, NumberOfEpisodies);
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(1, GridUnitType.Auto);
                Grid.SetRow(season, LinkStream.RowDefinitions.Count);
                LinkStream.RowDefinitions.Add(row);
                LinkStream.Children.Add(season);
                DumpEncLinks();
            }
            else
            {
                contSeasons = 0;
                SerieHD_Client.DownloadStringCompleted -= new DownloadStringCompletedEventHandler(DumpLinksEpisodes);
                ProgressFilmLink.IsIndeterminate = false;
                if (ProgressFilmLink.Visibility != Visibility.Collapsed)
                {
                    Storyboard CollectingLabelHide = (Storyboard)TryFindResource("CollectingLabelHide");
                    CollectingLabelHide.Begin();
                }
            }
        }

        private void DumpEncLinks()
        {
            SerieHD_Client.DownloadStringAsync(new Uri("https://hdpass.click/tvShow/" + IdSeries + "?season=" + contSeasons + "&episode=" + contEpisodies));
        }

        private void DumpLinksEpisodes(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                SerieHD_Client.DownloadStringCompleted -= new DownloadStringCompletedEventHandler(DumpLinksEpisodes);
                return;
            }
            string SeriesPageHtml = e.Result;
            var SplitEnc64Link = SeriesPageHtml.Split(new string[] { "<iframe allowfullscreen custom-src=\"" }, StringSplitOptions.None);
            string CodeLinkenc = SplitEnc64Link[1].Substring(0, SplitEnc64Link[1].IndexOf('"'));
            var base64EncodedBytes = Convert.FromBase64String(CodeLinkenc);
            string DecBase64LinkStream = Encoding.UTF8.GetString(base64EncodedBytes);

            //Clipboard.SetText(DecBase64LinkStream);
            //MessageBox.Show(DecBase64LinkStream + " --- " + (contEpisodies + 1).ToString() + " --- " + (contSeasons + 1).ToString());

            IEnumerable<Grid> SeasonContainer = LinkStream.Children.OfType<Grid>();
            foreach (var Season in SeasonContainer)
            {
                if (Season.Name == "SeasonContainer_" + (contSeasons + 1).ToString())
                {
                    IEnumerable<Grid> EpisodeContainer = Season.Children.OfType<Grid>();
                    foreach (var Episode in EpisodeContainer)
                    {
                        if (Episode.Name == "EpisodiesContainer_" + (contSeasons + 1).ToString())
                        {
                            //MessageBox.Show(Episode.Name + " ---> ep. " + (contEpisodies + 1));
                            Card EpisodeBtn = EpisodeButton(contSeasons + 1, contEpisodies + 1, DecBase64LinkStream);
                            RowDefinition row = new RowDefinition();
                            row.Height = new GridLength(1, GridUnitType.Auto);
                            Grid.SetRow(EpisodeBtn, Episode.RowDefinitions.Count);
                            Episode.RowDefinitions.Add(row);
                            Episode.Children.Add(EpisodeBtn);
                        }
                    }
                }
            }

            if (contEpisodies < NumberOfEpisodies - 1)
            {
                contEpisodies++;
                DumpEncLinks();
            }
            else
            {
                contEpisodies = 0;
                contSeasons++;
                SerieHD_Client.DownloadStringCompleted -= new DownloadStringCompletedEventHandler(DumpLinksEpisodes);
                SerieHD_Client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DumpNumberEpisodes);
                DumpNumberEpisodesIterative();
            }
        }

        private Grid SeriesDataSheet(string Title, string Year, string Cast, string Image, string TrailerLinkstr, string DesctriptionTxt)
        {
            Grid SeriesGridData = new Grid();
            Grid Sub1SeriesData = new Grid();
            Grid Sub2SeriesData = new Grid();
            Grid Sub3SeriesData = new Grid();

            Card ImageContainer = new Card();

            Image ImageSkin = new Image();
            var image = new Image();
            var fullFilePath = Image;
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(fullFilePath, UriKind.Absolute);
            bitmap.EndInit();
            image.Source = bitmap;
            ImageSkin.Height = 120;
            ImageSkin.Width = 80;
            ImageSkin.Source = image.Source;
            ImageSkin.VerticalAlignment = VerticalAlignment.Top;
            ImageSkin.HorizontalAlignment = HorizontalAlignment.Left;

            ImageContainer.Padding = new Thickness(0);
            ShadowAssist.SetShadowDepth(ImageContainer, ShadowDepth.Depth4);
            ImageContainer.UniformCornerRadius = 1;
            ImageContainer.BorderThickness = new Thickness(0);
            ImageContainer.VerticalAlignment = VerticalAlignment.Top;
            ImageContainer.HorizontalAlignment = HorizontalAlignment.Left;
            ImageContainer.Margin = new Thickness(5, 5, 5, 5);
            ImageContainer.Content = ImageSkin;

            TextBlock TitleSeries = generateTextBlock(Title, FontStyles.Italic, (Style)FindResource("MaterialDesignHeadline2TextBlock"), 19, VerticalAlignment.Top, HorizontalAlignment.Left, new Thickness(5, 0, 0, 23));
            TextBlock YearSeries = generateTextBlock("Year: " + Year, FontStyles.Normal, (Style)FindResource("MaterialDesignHeadline2TextBlock"), 12, VerticalAlignment.Top, HorizontalAlignment.Left, new Thickness(5, 0, 0, 7));
            TextBlock CastSeries = generateTextBlock("Casting: " + Cast, FontStyles.Normal, (Style)FindResource("MaterialDesignHeadline2TextBlock"), 12, VerticalAlignment.Top, HorizontalAlignment.Left, new Thickness(5, 0, 0, 7));
            TextBlock HDSeries = generateTextBlock("HD", FontStyles.Normal, (Style)FindResource("MaterialDesignSubtitle2TextBlock"), 13, VerticalAlignment.Top, HorizontalAlignment.Left, new Thickness(5, 0, 0, 7));
            TextBlock TrailerText = generateTextBlock("Watch trailer", FontStyles.Normal, (Style)FindResource("MaterialDesignSubtitle2TextBlock"), 13, VerticalAlignment.Top, HorizontalAlignment.Left, new Thickness(5, 0, 0, 0));
            TextBlock DesctriptionTXT = generateTextBlock("Description:", FontStyles.Normal, (Style)FindResource("MaterialDesignSubtitle2TextBlock"), 13, VerticalAlignment.Top, HorizontalAlignment.Left, new Thickness(5, 5, 5, 5));
            TextBlock Desctription = generateTextBlock(DesctriptionTxt, FontStyles.Italic, (Style)FindResource("MaterialDesignHeadline2TextBlock"), 12.3, VerticalAlignment.Top, HorizontalAlignment.Left, new Thickness(5, 5, 5, 5));

            if (TitleSeries.Text.Length > 24)
            {
                TitleSeries.FontSize = 16;
            }

            if (CastSeries.Text.Length > 40)
            {
                CastSeries.Text = CastSeries.Text.Substring(0, 38) + "...";
            }

            Hyperlink TrailerLink = new Hyperlink();
            TrailerLink.NavigateUri = new Uri(TrailerLinkstr);
            TrailerLink.Click += new RoutedEventHandler(Hyperlink_RequestNavigate);
            TrailerText.Inlines.Add(TrailerLink);
            TrailerText.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#2f6ea5"));

            RowDefinition rowDef1 = new RowDefinition();
            RowDefinition rowDef2 = new RowDefinition();
            RowDefinition rowDef7 = new RowDefinition();
            rowDef1.Height = new GridLength(1, GridUnitType.Auto);
            rowDef2.Height = new GridLength(1, GridUnitType.Auto);
            rowDef7.Height = new GridLength(1, GridUnitType.Auto);
            SeriesGridData.RowDefinitions.Add(rowDef1);
            SeriesGridData.RowDefinitions.Add(rowDef2);
            SeriesGridData.RowDefinitions.Add(rowDef7);

            Grid.SetRow(Sub1SeriesData, 0);
            Grid.SetRow(DesctriptionTXT, 1);
            Grid.SetRow(Desctription, 2);
            SeriesGridData.Children.Add(Sub1SeriesData);
            SeriesGridData.Children.Add(DesctriptionTXT);
            SeriesGridData.Children.Add(Desctription);

            ColumnDefinition colDef1 = new ColumnDefinition();
            ColumnDefinition colDef2 = new ColumnDefinition();
            colDef1.Width = new GridLength(1, GridUnitType.Auto);
            colDef2.Width = new GridLength(1, GridUnitType.Auto);
            Sub1SeriesData.ColumnDefinitions.Add(colDef1);
            Sub1SeriesData.ColumnDefinitions.Add(colDef2);

            Grid.SetColumn(ImageContainer, 0);
            Grid.SetColumn(Sub2SeriesData, 1);
            Sub1SeriesData.Children.Add(ImageContainer);
            Sub1SeriesData.Children.Add(Sub2SeriesData);

            RowDefinition rowDef3 = new RowDefinition();
            RowDefinition rowDef4 = new RowDefinition();
            RowDefinition rowDef5 = new RowDefinition();
            RowDefinition rowDef6 = new RowDefinition();
            RowDefinition rowDef8 = new RowDefinition();
            rowDef3.Height = new GridLength(1, GridUnitType.Auto);
            rowDef4.Height = new GridLength(1, GridUnitType.Auto);
            rowDef5.Height = new GridLength(1, GridUnitType.Auto);
            rowDef6.Height = new GridLength(1, GridUnitType.Auto);
            rowDef8.Height = new GridLength(1, GridUnitType.Auto);
            Sub2SeriesData.RowDefinitions.Add(rowDef3);
            Sub2SeriesData.RowDefinitions.Add(rowDef4);
            Sub2SeriesData.RowDefinitions.Add(rowDef5);
            Sub2SeriesData.RowDefinitions.Add(rowDef6);
            Sub2SeriesData.RowDefinitions.Add(rowDef8);

            Grid.SetRow(TitleSeries, 0);
            Grid.SetRow(YearSeries, 1);
            Grid.SetRow(CastSeries, 2);
            Grid.SetRow(HDSeries, 3);
            Grid.SetRow(TrailerText, 4);
            Sub2SeriesData.Children.Add(TitleSeries);
            Sub2SeriesData.Children.Add(YearSeries);
            Sub2SeriesData.Children.Add(CastSeries);
            Sub2SeriesData.Children.Add(HDSeries);
            Sub2SeriesData.Children.Add(TrailerText);

            return SeriesGridData;
        }

        private Grid SeasonCard(int SeasonNum, int EpisodeNum)
        {
            Grid SeasonFinalGrid = new Grid();
            SeasonFinalGrid.Name = "SeasonContainer_" + SeasonNum;
            Grid EpisodeFinalGrid = new Grid();
            EpisodeFinalGrid.Name = "EpisodiesContainer_" + SeasonNum;

            EpisodeFinalGrid.Height = 0;
            addScrollEffectToObject("ScrollDownEpList_" + SeasonNum, EpisodeFinalGrid, HeightProperty, 0, (EpisodeNum * 30) + (4 * EpisodeNum) + 7);
            addScrollEffectToObject("ScrollUpEpList_" + SeasonNum, EpisodeFinalGrid, HeightProperty, (EpisodeNum * 30) + (4 * EpisodeNum) + 7, 0);

            Grid SeasonInnerGrid = new Grid();
            SeasonInnerGrid.Height = 30;
            ColumnDefinition colDef1 = new ColumnDefinition();
            ColumnDefinition colDef2 = new ColumnDefinition();
            ColumnDefinition colDef3 = new ColumnDefinition();
            colDef1.Width = new GridLength(1, GridUnitType.Auto);
            colDef2.Width = new GridLength(1, GridUnitType.Star);
            colDef3.Width = new GridLength(1, GridUnitType.Auto);
            SeasonInnerGrid.ColumnDefinitions.Add(colDef1);
            SeasonInnerGrid.ColumnDefinitions.Add(colDef2);
            SeasonInnerGrid.ColumnDefinitions.Add(colDef3);
            Button Expand = generateCirleButton(PackIconKind.KeyboardArrowRight, 30, 15);
            addRotateEffectToObject("RotateOnClock_" + SeasonNum, Expand, 0, 90, 400);
            addRotateEffectToObject("RotateAntClock_" + SeasonNum, Expand, 90, 0, 400);
            Expand.Tag = SeasonNum;
            TextBlock SeasonTxt = generateTextBlock("Season " + SeasonNum + " - " + NumberOfEpisodies + " Ep.", FontStyles.Normal, (Style)FindResource("MaterialDesignHeadline2TextBlock"), 15, VerticalAlignment.Center, HorizontalAlignment.Left, new Thickness(0));
            Button ShareLinks = generateCirleButton(PackIconKind.ShareVariant, 30, 15);
            ShareLinks.Tag = SeasonNum;
            Grid.SetColumn(Expand, 0);
            Grid.SetColumn(SeasonTxt, 1);
            Grid.SetColumn(ShareLinks, 2);
            SeasonInnerGrid.Children.Add(Expand);
            SeasonInnerGrid.Children.Add(SeasonTxt);
            SeasonInnerGrid.Children.Add(ShareLinks);

            ShareLinks.Click += ShareLinks_Click;
            ShareLinks.MouseEnter += Copy_MouseEnter;
            ShareLinks.MouseLeave += Copy_MouseLeave;
            Expand.Click += Expand_Click;
            Expand.MouseEnter += Expand_MouseEnter;
            Expand.MouseLeave += Expand_MouseLeave;

            Card SeasonCard = new Card();            
            SeasonCard.Padding = new Thickness(0);
            ShadowAssist.SetShadowDepth(SeasonCard, ShadowDepth.Depth3);
            SeasonCard.UniformCornerRadius = 1;
            SeasonCard.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0d151f"));
            SeasonCard.BorderThickness = new Thickness(0);
            SeasonCard.VerticalAlignment = VerticalAlignment.Center;
            SeasonCard.HorizontalAlignment = HorizontalAlignment.Stretch;
            SeasonCard.Margin = new Thickness(5, 5, 5, 5);
            SeasonCard.Content = SeasonInnerGrid;

            RowDefinition rowDef1 = new RowDefinition();
            rowDef1.Height = new GridLength(1, GridUnitType.Auto);
            SeasonFinalGrid.RowDefinitions.Add(rowDef1);
            Grid.SetRow(SeasonCard, 0);
            SeasonFinalGrid.Children.Add(SeasonCard);

            RowDefinition rowDef2 = new RowDefinition();
            rowDef2.Height = new GridLength(1, GridUnitType.Auto);
            SeasonFinalGrid.RowDefinitions.Add(rowDef2);
            Grid.SetRow(EpisodeFinalGrid, 1);
            SeasonFinalGrid.Children.Add(EpisodeFinalGrid);

            return SeasonFinalGrid;
        }

        private void ShareLinks_Click(object sender, RoutedEventArgs e)
        {
            Button ShareBtn = (Button)sender;
            string seasonNum = ShareBtn.Tag.ToString();
            string resultLinkList = "Season " + seasonNum + " links list" + Environment.NewLine + Environment.NewLine;

            IEnumerable<Grid> SeasonContainer = LinkStream.Children.OfType<Grid>();
            foreach (var Season in SeasonContainer)
            {
                if (Season.Name == "SeasonContainer_" + seasonNum)
                {
                    IEnumerable<Grid> EpisodeContainer = Season.Children.OfType<Grid>();
                    foreach (var Episode in EpisodeContainer)
                    {
                        IEnumerable<Card> EpisodeCard = Episode.Children.OfType<Card>();
                        foreach (var EpisodeBtnCard in EpisodeCard)
                        {
                            Grid InnerGrid = EpisodeBtnCard.Content as Grid;
                            IEnumerable<Button> ButtonPlay = InnerGrid.Children.OfType<Button>();
                            foreach (var LinkPlayBtn in ButtonPlay)
                            {
                                if (LinkPlayBtn.Name.Contains("PlayBtn"))
                                {
                                    var splitName = LinkPlayBtn.Tag.ToString().Split(',');
                                    resultLinkList = resultLinkList + "Episode " + splitName[1] + " --> " + splitName[2] + Environment.NewLine;
                                }
                            }
                        }
                    }
                }
            }

            Clipboard.SetText(resultLinkList);
            MessageBox.Show("Links list of Season " + seasonNum + " copied in clipboard!");
        }

        private void addScrollEffectToObject(string name, DependencyObject objectValue, DependencyProperty property, double from, double to)
        {
            Storyboard AnimationSmart = new Storyboard();
            QuinticEase quinticEase = new QuinticEase();
            quinticEase.EasingMode = EasingMode.EaseOut;
            DoubleAnimation ShowingAnimation = new DoubleAnimation();
            ShowingAnimation.From = from;
            ShowingAnimation.To = to;            
            ShowingAnimation.EasingFunction = quinticEase;
            Storyboard.SetTarget(ShowingAnimation, objectValue);
            Storyboard.SetTargetProperty(ShowingAnimation, new PropertyPath(property));
            AnimationSmart.Children.Add(ShowingAnimation);
            if (Resources.Contains(name))
            {
                Resources.Remove(name);
                Resources.Add(name, AnimationSmart);
            }
            else
            {
                Resources.Add(name, AnimationSmart);
            }

        }

        private void addRotateEffectToObject(string name, DependencyObject objectValue, double from, double to, int milliseconds)
        {
            Storyboard AnimationSmart = new Storyboard();
            QuinticEase quinticEase = new QuinticEase();
            quinticEase.EasingMode = EasingMode.EaseOut;
            DoubleAnimation ShowingAnimation = new DoubleAnimation();
            ShowingAnimation.From = from;
            ShowingAnimation.To = to;
            ShowingAnimation.Duration = new TimeSpan(0, 0, 0, 0, milliseconds);
            ShowingAnimation.EasingFunction = quinticEase;
            Storyboard.SetTarget(ShowingAnimation, objectValue);
            DependencyProperty[] propertyChain = new DependencyProperty[] { RenderTransformProperty, RotateTransform.AngleProperty };
            PropertyPath myPropertyPath = new PropertyPath("(0).(1)", propertyChain);
            Storyboard.SetTargetProperty(ShowingAnimation, myPropertyPath);
            AnimationSmart.Children.Add(ShowingAnimation);

            if (Resources.Contains(name))
            {
                Resources.Remove(name);
                Resources.Add(name, AnimationSmart);
            }
            else
            {
                Resources.Add(name, AnimationSmart);
            }
        }

        private Card EpisodeButton(int SeasonNum, int EpisodeNum, string LinkSteaming)
        {
            Card EpisodeFinalCard = new Card();
            Grid SeasonInnerGrid = new Grid();

            ColumnDefinition colDef1 = new ColumnDefinition();
            ColumnDefinition colDef2 = new ColumnDefinition();
            ColumnDefinition colDef3 = new ColumnDefinition();
            colDef1.Width = new GridLength(1, GridUnitType.Star);
            colDef2.Width = new GridLength(1, GridUnitType.Auto);
            colDef3.Width = new GridLength(1, GridUnitType.Auto);
            SeasonInnerGrid.ColumnDefinitions.Add(colDef1);
            SeasonInnerGrid.ColumnDefinitions.Add(colDef2);
            SeasonInnerGrid.ColumnDefinitions.Add(colDef3);

            TextBlock EpisodeTxt = generateTextBlock(EpisodeNum + ".   Episode", FontStyles.Normal, (Style)FindResource("MaterialDesignHeadline2TextBlock"), 13, VerticalAlignment.Center, HorizontalAlignment.Left, new Thickness(7, 0, 0, 0));
            Button Play = generateCirleButton(PackIconKind.Play, 30, 15);
            Play.Name = "PlayBtn_" + SeasonNum + "_" + EpisodeNum;
            Play.Tag = SeasonNum + "," + EpisodeNum + "," + LinkSteaming;
            Button Copy = generateCirleButton(PackIconKind.ContentCopy, 30, 15);
            Copy.Tag = SeasonNum + "," + EpisodeNum + "," + LinkSteaming;
            Grid.SetColumn(EpisodeTxt, 0);
            Grid.SetColumn(Play, 1);
            Grid.SetColumn(Copy, 2);

            SeasonInnerGrid.Children.Add(EpisodeTxt);
            SeasonInnerGrid.Children.Add(Play);
            SeasonInnerGrid.Children.Add(Copy);

            Copy.MouseEnter += Copy_MouseEnter;
            Copy.MouseLeave += Copy_MouseLeave;
            Copy.Click += Copy_Click;
            Play.MouseEnter += Expand_MouseEnter;
            Play.MouseLeave += Expand_MouseLeave;
            Play.Click += Play_Click;

            EpisodeFinalCard.Padding = new Thickness(0);
            ShadowAssist.SetShadowDepth(EpisodeFinalCard, ShadowDepth.Depth2);
            EpisodeFinalCard.UniformCornerRadius = 1;
            EpisodeFinalCard.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0d151f"));
            EpisodeFinalCard.BorderThickness = new Thickness(0);
            EpisodeFinalCard.VerticalAlignment = VerticalAlignment.Center;
            EpisodeFinalCard.HorizontalAlignment = HorizontalAlignment.Stretch;
            EpisodeFinalCard.Margin = new Thickness(15, 2, 5, 2);

            EpisodeFinalCard.Content = SeasonInnerGrid;

            return EpisodeFinalCard;
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Button CopyBtn = (Button)sender;
            var SplitLink = CopyBtn.Tag.ToString().Split(',');
            Clipboard.SetText("Season " + SplitLink[0] + " Ep. " + SplitLink[1] + " --> " + SplitLink[2]);
            MessageBox.Show("Link copied in clipboard!");
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            Button PlayBtn = (Button)sender;
            var SplitLink = PlayBtn.Tag.ToString().Split(',');

            if (OnlyMe)
            {
                ActionOnPlay(SplitLink[0], SplitLink[1]);
                CloseLinks.Click -= CloseLinks_Click;
                CloseLinks.Click += SeriesMiniBanner;
                streamtape.DownloadStringAsync(new Uri(SplitLink[2]));
            }
            else
            {
                MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, MqttConnection.NickName + "_SERIEPLAYER_PLAYURL:" + SplitLink[0] + ":" + SplitLink[1] + ":" + SplitLink[2]);
            }
        }

        private void ActionOnPlay(string SeasonNum, string EpisodeNum)
        {
            if (!VideoView.MediaPlayer.IsPlaying)
            {
                SeasonEpisondeTxt.Text = "S. " + SeasonNum + " Ep. " + EpisodeNum;
            }
            if (LinkStreamContainer.Height != 30)
            {
                if (ProgressFilmLink.IsIndeterminate)
                {
                    Storyboard CollectingLabelHide = (Storyboard)TryFindResource("CollectingLabelHide");
                    CollectingLabelHide.Begin();
                }
                Storyboard ExpandRotateAntClock = (Storyboard)TryFindResource("RotateAntClock_" + SeasonNum);
                ExpandRotateAntClock.Begin();
                Storyboard EpisodeLinkListUp = (Storyboard)TryFindResource("ScrollUpEpList_" + SeasonNum);
                EpisodeLinkListUp.Begin();
                Storyboard SeriesMiniDown = (Storyboard)TryFindResource("SeriesMiniDown");
                SeriesMiniDown.Begin();
                IconCloseLinks.Kind = PackIconKind.KeyboardArrowUp;
                addRotateEffectToObject("SeriesMiniRotateOnClock", CloseLinks, 0, -180, 800);
                addRotateEffectToObject("SeriesMiniRotateAntClock", CloseLinks, -180, 0, 800);
                RotateTransform rotateTransform = new RotateTransform();
                CloseLinks.RenderTransform = rotateTransform;
                MiniControllPannelMultipleAnimation(true);
            }
        }

        private void MiniControllPannelMultipleAnimation(bool Showing)
        {
            if (Showing)
            {
                ProgressFilmLink.Visibility = Visibility.Collapsed;
                Storyboard TitleOpacityShow = (Storyboard)TryFindResource("TitleOpacityShow");
                TitleOpacityShow.Begin();
                Storyboard TitleGridHeightShow = (Storyboard)TryFindResource("TitleGridHeightShow");
                TitleGridHeightShow.Begin();
                Storyboard InfoTxtOpacityHide = (Storyboard)TryFindResource("InfoTxtOpacityHide");
                InfoTxtOpacityHide.Begin();
                Storyboard InfoTxtGridHeightHide = (Storyboard)TryFindResource("InfoTxtGridHeightHide");
                InfoTxtGridHeightHide.Begin();
                Storyboard SeasonEpTxtGridWidthShow = (Storyboard)TryFindResource("SeasonEpTxtGridWidthShow");
                SeasonEpTxtGridWidthShow.Begin();
                Storyboard MiniControllPanellGridWidthShow = (Storyboard)TryFindResource("MiniControllPanellGridWidthShow");
                MiniControllPanellGridWidthShow.Begin();
            }
            else
            {
                ProgressFilmLink.Visibility = Visibility.Visible;
                Storyboard TitleOpacityHide = (Storyboard)TryFindResource("TitleOpacityHide");
                TitleOpacityHide.Begin();
                Storyboard TitleGridHeightHide = (Storyboard)TryFindResource("TitleGridHeightHide");
                TitleGridHeightHide.Begin();
                Storyboard InfoTxtOpacityShow = (Storyboard)TryFindResource("InfoTxtOpacityShow");
                InfoTxtOpacityShow.Begin();
                Storyboard InfoTxtGridHeightShow = (Storyboard)TryFindResource("InfoTxtGridHeightShow");
                InfoTxtGridHeightShow.Begin();
                Storyboard SeasonEpTxtGridWidthHide = (Storyboard)TryFindResource("SeasonEpTxtGridWidthHide");
                SeasonEpTxtGridWidthHide.Begin();
                Storyboard MiniControllPanellGridWidthHide = (Storyboard)TryFindResource("MiniControllPanellGridWidthHide");
                MiniControllPanellGridWidthHide.Begin();
            }
        }

        private void SeriesMiniBanner(object sender, RoutedEventArgs e)
        {
            RotateTransform getRotation = CloseLinks.RenderTransform as RotateTransform;
            if (getRotation.Angle == -180)
            {
                if (ProgressFilmLink.IsIndeterminate)
                {
                    ProgressFilmLink.Visibility = Visibility.Collapsed;
                    Storyboard CollectingLabelHide = (Storyboard)TryFindResource("CollectingLabelHide");
                    CollectingLabelHide.Begin();
                }
                Storyboard SeriesMiniRotateAntClock = (Storyboard)TryFindResource("SeriesMiniRotateAntClock");
                SeriesMiniRotateAntClock.Begin();
                Storyboard SeriesMiniDown = (Storyboard)TryFindResource("SeriesMiniDown");
                SeriesMiniDown.Begin();
                MiniControllPannelMultipleAnimation(true);
            }
            else if (getRotation.Angle == 0)
            {
                if (ProgressFilmLink.IsIndeterminate)
                {
                    ProgressFilmLink.Visibility = Visibility.Visible;
                    Storyboard CollectingLabelShow = (Storyboard)TryFindResource("CollectingLabelShow");
                    CollectingLabelShow.Begin();
                }
                Storyboard SeriesMiniRotateOnClock = (Storyboard)TryFindResource("SeriesMiniRotateOnClock");
                SeriesMiniRotateOnClock.Begin();
                Storyboard SeriesMiniUp = (Storyboard)TryFindResource("SeriesMiniUp");
                SeriesMiniUp.Begin();
                MiniControllPannelMultipleAnimation(false);
            }
        }

        private void Expand_Click(object sender, RoutedEventArgs e)
        {
            Button ExpandBtn = (Button)sender;
            RotateTransform getRotation = ExpandBtn.RenderTransform as RotateTransform;

            if (getRotation.Angle == 90)
            {
                Storyboard ExpandRotateAntClock = (Storyboard)TryFindResource("RotateAntClock_" + ExpandBtn.Tag.ToString());
                ExpandRotateAntClock.Begin();
                Storyboard EpisodeLinkListUp = (Storyboard)TryFindResource("ScrollUpEpList_" + ExpandBtn.Tag.ToString());
                EpisodeLinkListUp.Begin();
            }
            else
            {
                Storyboard ExpandRotateOnClock = (Storyboard)TryFindResource("RotateOnClock_" + ExpandBtn.Tag.ToString());
                ExpandRotateOnClock.Begin();
                Storyboard EpisodeLinkListDown = (Storyboard)TryFindResource("ScrollDownEpList_" + ExpandBtn.Tag.ToString());
                EpisodeLinkListDown.Begin();
            }
        }

        private void Expand_MouseLeave(object sender, MouseEventArgs e)
        {
            Button ExpandBtn = (Button)sender;
            ExpandBtn.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#737E87"));
        }

        private void Expand_MouseEnter(object sender, MouseEventArgs e)
        {
            Button ExpandBtn = (Button)sender;
            ExpandBtn.Foreground = Brushes.White;
        }

        private void Copy_MouseLeave(object sender, MouseEventArgs e)
        {
            Button CopyBtn = (Button)sender;
            CopyBtn.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#737E87"));
        }

        private void Copy_MouseEnter(object sender, MouseEventArgs e)
        {
            Button CopyBtn = (Button)sender;
            CopyBtn.Foreground = Brushes.White;
        }

        private TextBlock generateTextBlock(string Text, FontStyle fontStyle, Style style, double fontSize, VerticalAlignment alignmentVe, HorizontalAlignment alignmentHo, Thickness Margin)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.FontSize = fontSize;
            textBlock.VerticalAlignment = alignmentVe;
            textBlock.HorizontalAlignment = alignmentHo;
            textBlock.FontStyle = fontStyle;
            textBlock.Style = style;
            textBlock.Text = Text;
            textBlock.Margin = Margin;

            return textBlock;
        }

        private Button generateCirleButton(PackIconKind KindIcon, double largeBtn, double largeIcon)
        {
            Button FinalButton = new Button();
            FinalButton.RenderTransformOrigin = new Point(0.5, 0.5);
            RotateTransform rotateTransform = new RotateTransform();
            FinalButton.RenderTransform = rotateTransform;
            FinalButton.Padding = new Thickness(0);
            FinalButton.ToolTip = "MaterialDesignFlatButton";
            FinalButton.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#737E87"));
            FinalButton.Style = (Style)FindResource("MyButton");
            RippleAssist.SetFeedback(FinalButton, (SolidColorBrush)(new BrushConverter().ConvertFrom("#232e3c")));
            ButtonAssist.SetCornerRadius(FinalButton, new CornerRadius(20));
            FinalButton.VerticalAlignment = VerticalAlignment.Center;
            FinalButton.HorizontalAlignment = HorizontalAlignment.Center;
            FinalButton.Height = largeBtn;
            FinalButton.Width = largeBtn;

            PackIcon IconButton = new PackIcon();
            IconButton.Height = largeIcon;
            IconButton.Width = largeIcon;
            IconButton.VerticalAlignment = VerticalAlignment.Center;
            IconButton.HorizontalAlignment = HorizontalAlignment.Center;
            IconButton.Kind = KindIcon;

            FinalButton.Content = IconButton;

            return FinalButton;
        }











        string linkDNS = "";
        string linkSeriesDNS = "";
        private void DownloadNewLink(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                var splitLinkDNS = e.Result.Split(new string[] { "elementor-heading-title elementor-size-default\"><a href=\"" }, StringSplitOptions.None);
                if (search)
                {
                    linkDNS = splitLinkDNS[1].Substring(0, splitLinkDNS[1].IndexOf("\""));
                    //MessageBox.Show(linkDNS);
                    SearchLinkAltaDef(linkDNS);
                }
                else if (SerieSearch)
                {
                    linkSeriesDNS = splitLinkDNS[1].Substring(0, splitLinkDNS[1].IndexOf("\""));
                    //MessageBox.Show(linkSeriesDNS);
                    SeriesSearch(linkSeriesDNS + "?s=" + SendText.Text);
                }
            }
            catch (Exception)
            {
                EndTackTimer.Start();
                HintAssist.SetHint(SendText, "Type a message");
                SendText.Text = "";
                SendText.Focus();
                Storyboard s = (Storyboard)TryFindResource("FilmLinkListDown");
                s.Begin();
                MessageAllertCardBox("Could not find data for stream :(", PackIconKind.DatabaseRemove, "#FFFFFF", "#9B0000");
            }
        }

        private void SearchLinkAltaDef(string DNS)
        {
            Web1.Navigate(new Uri(DNS + "?s=" + SendText.Text));
            Web1.LoadCompleted += new LoadCompletedEventHandler(completeLoadWeb1);
            HintAssist.SetHint(SendText, "Type a message");
            search = false;
            SendText.Text = "";
            TextLink.Text = "Select a movie";
            LinkStream.RowDefinitions.Clear();
            LinkStream.ColumnDefinitions.Clear();
            LinkStream.Children.Clear();
            ProgressFilmLink.IsIndeterminate = true;
            Storyboard FilmLinkListUp = (Storyboard)TryFindResource("FilmLinkListUp");
            FilmLinkListUp.Begin();
        }

        private void completeLoadWeb1(object sender, NavigationEventArgs na)
        {
            var document = Web1.Document as mshtml.HTMLDocument;
            string page = document.body.outerHTML;
            //Clipboard.SetText(page);
            try
            {
                if (!page.Contains("<form class=\"challenge-form\" id=\"challenge-form\" action="))
                {
                    string subPage = page.Substring(page.IndexOf("<section id=\"lastUpdate\">"));
                    var subPageSplit = subPage.Split(new string[] { "</section>" }, StringSplitOptions.None);
                    string subPageRow = subPageSplit[0]; //.Remove(subPageSplit[0].IndexOf("<div class=\"row ismobile\">")) + "</section>";
                    var LinkSplit = subPageRow.Split(new string[] { "<a href=\"" }, StringSplitOptions.None);
                    var nameSplit = subPageRow.Split(new string[] { "class=\"titleFilm\">" }, StringSplitOptions.None);
                    var LinkImageSplit = subPageRow.Split(new string[] { "src=\"" }, StringSplitOptions.None);
                    var ValueStartSplit = subPageRow.Split(new string[] { "IMDB: " }, StringSplitOptions.None);
                    int cont = 0;
                    for (int i = 1; i < LinkSplit.Length; i++)
                    {
                        string Title = nameSplit[i].Substring(0, nameSplit[i].IndexOf("<"));
                        string link = LinkSplit[i].Substring(0, LinkSplit[i].IndexOf("\""));
                        string Image = "";
                        string valueStar;
                        try
                        {
                            Image = LinkImageSplit[i + cont].Substring(0, LinkImageSplit[i + cont].IndexOf("\""));
                            valueStar = ValueStartSplit[i].Substring(0, ValueStartSplit[i].IndexOf("<")).Replace(',', '.');
                        }
                        catch (Exception)
                        {
                            valueStar = "0";
                        }
                        cont = cont + 2;
                        double NumStar = double.Parse(valueStar);
                        Button FilmButton = new Button();
                        string ReduceLink = link;
                        if (link.Length > 40)
                        {
                            ReduceLink = link.Substring(0, 40) + "...";
                        }
                        FilmButton = FilmBlockCreator(Title, "Link: " + ReduceLink, Image, NumStar, (SolidColorBrush)(new BrushConverter().ConvertFrom("#0D151F")), (SolidColorBrush)(FindResource("SecondaryColorMessage")), "");
                        FilmButton.Tag = link;
                        RowDefinition row = new RowDefinition();
                        row.Height = new GridLength(1, GridUnitType.Auto);
                        Grid.SetRow(FilmButton, LinkStream.RowDefinitions.Count);
                        LinkStream.RowDefinitions.Add(row);
                        LinkStream.Children.Add(FilmButton);
                        FilmButton.Click += new RoutedEventHandler(FilmButtonLinkClick);
                    }

                    Web1.LoadCompleted -= completeLoadWeb1;
                    Web1.Navigate("about:blank");
                    ProgressFilmLink.IsIndeterminate = false;
                }
            }
            catch (Exception)
            {
                HintAssist.SetHint(SendText, "Type a message");
                BtnStop.IsEnabled = true;
                BtnUpdateUrl.IsEnabled = false;
                BtnNetflix.IsEnabled = false;
                sendUrl = false;
                SendText.Text = "";
                SendText.Focus();
                Storyboard s = (Storyboard)TryFindResource("FilmLinkListDown");
                s.Begin();
                MessageAllertCardBox("Sorry, you need to be more precise in your search", PackIconKind.EmoticonSad, "#FFFFFF", "#9B0000");
                EndTackTimer.Start();
            }
        }

        private void FilmButtonLinkClick(object sender, RoutedEventArgs e)
        {
            Button LinkBtn = (Button)sender;
            if (!OnlyMe)
            {
                MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, MqttConnection.NickName + "_VIDEOPLAYER_URL:" + LinkBtn.Tag.ToString());
            }
            else
            {
                MessageAllertCardBox("Searching link video in the AltaDefinizione database...", PackIconKind.DatabaseSearch, "#FFFFFF", "#123B82");
                GetHdPassHtml.DownloadStringAsync(new Uri(LinkBtn.Tag.ToString()));
            }
            HintAssist.SetHint(SendText, "Type a message");
            BtnStop.IsEnabled = true;
            BtnUpdateUrl.IsEnabled = false;
            BtnNetflix.IsEnabled = false;
            sendUrl = false;
            SendText.Text = "";
            SendText.Focus();
            Storyboard s = (Storyboard)TryFindResource("FilmLinkListDown");
            s.Begin();
        }

        public bool controllUserOnlineYet(string nickname)
        {
            for (int i = 0; i < 20; i++)
            {
                if (Onlineuser[i] == nickname)
                {
                    return false;
                }
            }
            return true;
        }

        public void MessageAllertCardBox(string Message, PackIconKind IconKind, string ColorText, string ColorBack)
        {
            if (lastUserSendedMessage != "")
                insertSpace(ChatBoxContainer, 5);

            PackIcon AllertIcon = new PackIcon();
            AllertIcon.Height = 30;
            AllertIcon.Width = 30;
            AllertIcon.Kind = IconKind;
            AllertIcon.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorText));
            AllertIcon.VerticalAlignment = VerticalAlignment.Center;
            AllertIcon.HorizontalAlignment = HorizontalAlignment.Center;

            TextBlock AllertText = new TextBlock();
            AllertText.TextWrapping = TextWrapping.Wrap;
            AllertText.FontSize = AllertTextSize;
            AllertText.VerticalAlignment = VerticalAlignment.Center;
            AllertText.HorizontalAlignment = HorizontalAlignment.Left;
            AllertText.FontStyle = FontStyles.Normal;
            AllertText.FontFamily = new FontFamily("Calibri Light");
            AllertText.Margin = new Thickness(5, 0, 10, 0);
            AllertText.Text = Message;

            TextBlock TimeText = new TextBlock();
            TimeText.TextWrapping = TextWrapping.NoWrap;
            TimeText.FontSize = TimeTextSize;
            TimeText.HorizontalAlignment = HorizontalAlignment.Center;
            TimeText.VerticalAlignment = VerticalAlignment.Bottom;
            TimeText.FontStyle = FontStyles.Normal;
            TimeText.Style = (Style)FindResource("MaterialDesignHeadline2TextBlock");
            int minute = DateTime.Now.Minute;
            string goodminute = "";
            if (minute < 10)
            {
                goodminute = "0" + minute.ToString();
            }
            else
            {
                goodminute = minute.ToString();
            }
            TimeText.Text = DateTime.Now.Hour + ":" + goodminute;

            Grid MainMessageGrid = new Grid();

            MainMessageGrid.Margin = new Thickness(5, 5, 8, 5);
            ColumnDefinition colDef1 = new ColumnDefinition();
            ColumnDefinition colDef2 = new ColumnDefinition();
            ColumnDefinition colDef3 = new ColumnDefinition();

            colDef1.Width = new GridLength(1, GridUnitType.Auto);
            colDef2.Width = new GridLength(1, GridUnitType.Auto);
            colDef3.Width = new GridLength(1, GridUnitType.Auto);

            MainMessageGrid.ColumnDefinitions.Add(colDef1);
            MainMessageGrid.ColumnDefinitions.Add(colDef2);
            MainMessageGrid.ColumnDefinitions.Add(colDef3);

            Grid AllertTextGrid = new Grid();
            AllertTextGrid.MaxWidth = 200;
            AllertTextGrid.Children.Add(AllertText);

            Grid.SetColumn(AllertIcon, 0);
            Grid.SetColumn(AllertTextGrid, 1);
            Grid.SetColumn(TimeText, 2);

            MainMessageGrid.Children.Add(AllertIcon);
            MainMessageGrid.Children.Add(AllertTextGrid);
            MainMessageGrid.Children.Add(TimeText);

            Card MessageContainer = new Card();
            MessageContainer.Padding = new Thickness(0);
            ShadowAssist.SetShadowDepth(MessageContainer, ShadowDepth.Depth0);
            MessageContainer.UniformCornerRadius = 8;
            MessageContainer.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorText));
            MessageContainer.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorBack));
            MessageContainer.BorderThickness = new Thickness(0);
            MessageContainer.VerticalAlignment = VerticalAlignment.Center;
            MessageContainer.HorizontalAlignment = HorizontalAlignment.Center;

            MessageContainer.Content = MainMessageGrid;

            RowDefinition rowDef = new RowDefinition();
            rowDef.Height = new GridLength(1, GridUnitType.Auto);
            Grid.SetRow(MessageContainer, ChatBoxContainer.RowDefinitions.Count);
            ChatBoxContainer.RowDefinitions.Add(rowDef);

            ChatBoxContainer.Children.Add(MessageContainer);

            lastUserSendedMessage = "AllertMessage-----1234567890";

            ScrollChatBox.ScrollToEnd();
        }

        private string lastUserSendedMessage = "";
        private void AddMessage(string nickname, string message, bool isReplay, string nicknameReplay, string messageReplay, string messageReplayedPosition)
        {
            if (lastUserSendedMessage != nickname && lastUserSendedMessage != "")
            {
                insertSpace(ChatBoxContainer, 5);
            }
            else
            {
                insertSpace(ChatBoxContainer, 2.5);
            }

            TextBlock NickNameText = new TextBlock();
            NickNameText.TextWrapping = TextWrapping.Wrap;
            NickNameText.FontSize = NickNameTextSize;
            NickNameText.HorizontalAlignment = HorizontalAlignment.Left;
            NickNameText.VerticalAlignment = VerticalAlignment.Center;
            NickNameText.Style = (Style)FindResource("MaterialDesignSubtitle2TextBlock");
            NickNameText.Margin = new Thickness(0, 0, 0, 5);
            NickNameText.Text = nickname;

            TextBlock MessageText = new TextBlock();
            MessageText.Name = "message";
            MessageText.TextWrapping = TextWrapping.Wrap;
            MessageText.FontSize = MessageTextSize;
            MessageText.HorizontalAlignment = HorizontalAlignment.Left;
            MessageText.VerticalAlignment = VerticalAlignment.Center;
            MessageText.FontFamily = new FontFamily("Calibri Light");
            MessageText.Text = message;

            TextBlock TimeText = new TextBlock();
            TimeText.Margin = new Thickness(0, 0, 10, 0);
            TimeText.TextWrapping = TextWrapping.NoWrap;
            TimeText.FontSize = TimeTextSize;
            TimeText.HorizontalAlignment = HorizontalAlignment.Center;
            TimeText.VerticalAlignment = VerticalAlignment.Bottom;
            TimeText.FontStyle = FontStyles.Normal;
            TimeText.Style = (Style)FindResource("MaterialDesignCaptionTextBlock");
            int minute = DateTime.Now.Minute;
            string goodminute = "";
            if (minute < 10)
            {
                goodminute = "0" + minute.ToString();
            }
            else
            {
                goodminute = minute.ToString();
            }
            TimeText.Text = DateTime.Now.Hour + ":" + goodminute;

            Grid MainContentGrid = new Grid();
            ColumnDefinition colDef1 = new ColumnDefinition();
            ColumnDefinition colDef2 = new ColumnDefinition();
            colDef1.Width = new GridLength(1, GridUnitType.Auto);
            colDef2.Width = new GridLength(1, GridUnitType.Auto);
            MainContentGrid.ColumnDefinitions.Add(colDef1);
            MainContentGrid.ColumnDefinitions.Add(colDef2);

            Grid SubMessageGrid = new Grid();
            SubMessageGrid.MaxWidth = 235;
            SubMessageGrid.VerticalAlignment = VerticalAlignment.Stretch;
            SubMessageGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            SubMessageGrid.Margin = new Thickness(10);
            RowDefinition rowDef1 = new RowDefinition();
            RowDefinition rowDef2 = new RowDefinition();
            RowDefinition rowDef3 = new RowDefinition();
            rowDef1.Height = new GridLength(1, GridUnitType.Auto);
            rowDef2.Height = new GridLength(1, GridUnitType.Auto);
            rowDef3.Height = new GridLength(1, GridUnitType.Auto);

            SubMessageGrid.RowDefinitions.Add(rowDef1);
            SubMessageGrid.RowDefinitions.Add(rowDef2);
            Grid.SetRow(MessageText, 2);
            SubMessageGrid.RowDefinitions.Add(rowDef3);
            SubMessageGrid.Children.Add(MessageText);

            Grid.SetColumn(SubMessageGrid, 0);
            Grid.SetColumn(TimeText, 1);
            MainContentGrid.Children.Add(SubMessageGrid);
            MainContentGrid.Children.Add(TimeText);

            Button MessageContainer = new Button();
            MessageContainer.Tag = ChatBoxContainer.RowDefinitions.Count.ToString() + "*" + ScrollChatBox.VerticalOffset.ToString() + "}£(&{$)(" + nickname;
            MessageContainer.Padding = new Thickness(0);
            ShadowAssist.SetShadowDepth(MessageContainer, ShadowDepth.Depth0);
            MessageContainer.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
            MessageContainer.Style = (Style)FindResource("MessageAllert");
            MessageContainer.VerticalAlignment = VerticalAlignment.Center;

            if (nickname == MqttConnection.NickName)
            {
                SubMessageGrid.RowDefinitions.Remove(rowDef1);
                ButtonAssist.SetCornerRadius(MessageContainer, new CornerRadius(8, 8, 0, 8));
                MessageContainer.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#2B5278"));
                MessageContainer.HorizontalAlignment = HorizontalAlignment.Right;
                TimeText.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#5B7EA7"));
                MessageContainer.Margin = new Thickness(0, 0, 7, 0);

                if (lastUserSendedMessage == NickNameText.Text)
                {
                    ButtonAssist.SetCornerRadius(MessageContainer, new CornerRadius(8, 0, 0, 8));
                }

                if (isReplay)
                {
                    MessageText.Margin = new Thickness(0, 2, 0, 0);
                    SubMessageGrid.RowDefinitions.Add(rowDef1);
                    Button ReplayMessage = ReplayCreator(nicknameReplay, messageReplay, (SolidColorBrush)(new BrushConverter().ConvertFrom("#2B5278")), (SolidColorBrush)FindResource("SecondaryColorMessage"), messageReplayedPosition, false);
                    //ReplayMessage.Click += MessageReplayed_Click;
                    Grid.SetRow(ReplayMessage, 0);
                    SubMessageGrid.Children.Add(ReplayMessage);
                }
            }
            else
            {
                NickNameText.Foreground = new SolidColorBrush(ChangeColorBrightness(Color.FromArgb(255, (byte)(NickNameText.Text.Length * 3), (byte)(NickNameText.Text.Length * 8.5), (byte)(NickNameText.Text.Length * 5)), 20));
                Grid.SetRow(NickNameText, 0);
                SubMessageGrid.Children.Add(NickNameText);
                ButtonAssist.SetCornerRadius(MessageContainer, new CornerRadius(8, 8, 8, 0));
                MessageContainer.HorizontalAlignment = HorizontalAlignment.Left;
                MessageContainer.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#182533"));
                TimeText.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#404D5D"));
                MessageContainer.Margin = new Thickness(5, 0, 0, 0);

                if (lastUserSendedMessage == NickNameText.Text)
                {
                    SubMessageGrid.Children.Remove(NickNameText);
                    ButtonAssist.SetCornerRadius(MessageContainer, new CornerRadius(0, 8, 8, 0));
                }

                if (isReplay)
                {
                    MessageText.Margin = new Thickness(0, 2, 0, 0);
                    Button ReplayMessage = ReplayCreator(nicknameReplay, messageReplay, (SolidColorBrush)(new BrushConverter().ConvertFrom("#182533")), new SolidColorBrush(ChangeColorBrightness(Color.FromArgb(255, (byte)(nicknameReplay.Length * 3), (byte)(nicknameReplay.Length * 8.5), (byte)(nicknameReplay.Length * 5)), 20)), messageReplayedPosition, false);
                    //ReplayMessage.Click += MessageReplayed_Click;
                    Grid.SetRow(ReplayMessage, 1);
                    SubMessageGrid.Children.Add(ReplayMessage);
                }
            }

            MessageContainer.Content = MainContentGrid;
            RowDefinition rowDef = new RowDefinition();
            rowDef.Height = new GridLength(1, GridUnitType.Auto);
            Grid.SetRow(MessageContainer, ChatBoxContainer.RowDefinitions.Count);
            ChatBoxContainer.RowDefinitions.Add(rowDef);
            ChatBoxContainer.Children.Add(MessageContainer);

            ScrollChatBox.ScrollToEnd();
            lastUserSendedMessage = NickNameText.Text;

            MessageContainer.MouseDoubleClick += MessageContainer_MouseDoubleClick;
            //MessageContainer.Click += MessageContainer_Click;
        }

        private void MessageContainer_Click(object sender, RoutedEventArgs e)
        {
            //ScrollChatBox.ScrollToVerticalOffset(0);

            Storyboard s = (Storyboard)TryFindResource("Scroll");
            s.Begin();
        }

        private Button ReplayCreator(string nicknameReplay, string messageReplay, SolidColorBrush BackgroundColor, SolidColorBrush ReplayColor, string ReplayedPosition, bool insertSpaceGrid)
        {
            Grid mainReplayGrid = new Grid();
            StackPanel rectangle = new StackPanel();
            Grid dataTextGrid = new Grid();
            Grid spaceGrid = new Grid();
            TextBlock nicknameReplayTxt = new TextBlock();
            TextBlock messsageReplayTxt = new TextBlock();

            TextBlock NickNameText = new TextBlock();
            NickNameText.Margin = new Thickness(3);
            NickNameText.TextWrapping = TextWrapping.Wrap;
            NickNameText.FontSize = NickNameTextSize - 2;
            NickNameText.HorizontalAlignment = HorizontalAlignment.Left;
            NickNameText.VerticalAlignment = VerticalAlignment.Center;
            NickNameText.Style = (Style)FindResource("MaterialDesignSubtitle2TextBlock");
            NickNameText.Foreground = ReplayColor;
            NickNameText.Text = nicknameReplay;

            TextBlock MessageText = new TextBlock();
            MessageText.Margin = new Thickness(3);
            MessageText.TextWrapping = TextWrapping.NoWrap;
            MessageText.FontSize = MessageTextSize - 2;
            MessageText.HorizontalAlignment = HorizontalAlignment.Left;
            MessageText.VerticalAlignment = VerticalAlignment.Center;
            MessageText.FontFamily = new FontFamily("Calibri Light");
            MessageText.Text = messageReplay;

            RowDefinition rowDef1 = new RowDefinition();
            RowDefinition rowDef2 = new RowDefinition();
            rowDef1.Height = new GridLength(1, GridUnitType.Auto);
            rowDef2.Height = new GridLength(1, GridUnitType.Auto);
            dataTextGrid.RowDefinitions.Add(rowDef1);
            dataTextGrid.RowDefinitions.Add(rowDef2);
            Grid.SetRow(NickNameText, 0);
            Grid.SetRow(MessageText, 1);
            dataTextGrid.Children.Add(NickNameText);
            dataTextGrid.Children.Add(MessageText);
            spaceGrid.Width = 350;

            ColumnDefinition colDef1 = new ColumnDefinition();
            ColumnDefinition colDef2 = new ColumnDefinition();
            ColumnDefinition colDef3 = new ColumnDefinition();
            colDef1.Width = new GridLength(1, GridUnitType.Auto);
            colDef2.Width = new GridLength(1, GridUnitType.Auto);
            colDef3.Width = new GridLength(1, GridUnitType.Auto);

            rectangle.Height = 36;
            rectangle.Width = 2;
            rectangle.VerticalAlignment = VerticalAlignment.Center;
            rectangle.HorizontalAlignment = HorizontalAlignment.Left;
            rectangle.Background = ReplayColor;
            rectangle.Margin = new Thickness(2);

            if (nicknameReplay == MqttConnection.NickName)
            {
                rectangle.Background = (SolidColorBrush)FindResource("SecondaryColorMessage");
                NickNameText.Foreground = (SolidColorBrush)FindResource("SecondaryColorMessage");
                NickNameText.Text = "Tu";
            }

            Grid.SetColumn(rectangle, 0);
            Grid.SetColumn(dataTextGrid, 1);
            Grid.SetColumn(spaceGrid, 2);
            mainReplayGrid.ColumnDefinitions.Add(colDef1);
            mainReplayGrid.ColumnDefinitions.Add(colDef2);
            mainReplayGrid.ColumnDefinitions.Add(colDef3);
            mainReplayGrid.Children.Add(rectangle);
            mainReplayGrid.Children.Add(dataTextGrid);
            if (insertSpaceGrid)
                mainReplayGrid.Children.Add(spaceGrid);

            Button ReplayContainer = new Button();
            ReplayContainer.Tag = ReplayedPosition;
            ReplayContainer.Padding = new Thickness(0);
            //RippleAssist.SetFeedback(ReplayContainer, (SolidColorBrush)(new BrushConverter().ConvertFrom("#BDBDBD")));
            ButtonAssist.SetCornerRadius(ReplayContainer, new CornerRadius(3));
            ShadowAssist.SetShadowDepth(ReplayContainer, ShadowDepth.Depth0);
            ReplayContainer.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
            ReplayContainer.Background = BackgroundColor;
            ReplayContainer.Style = (Style)FindResource("MyButton");
            ReplayContainer.VerticalAlignment = VerticalAlignment.Center;
            ReplayContainer.HorizontalAlignment = HorizontalAlignment.Left;
            ReplayContainer.Content = mainReplayGrid;

            return ReplayContainer;
        }

        private void Hyperlink_RequestNavigate(object sender, RoutedEventArgs e)
        {
            Hyperlink TrailerLink = (Hyperlink)sender;
            MessageBox.Show(TrailerLink.NavigateUri.AbsoluteUri);
            Process.Start(TrailerLink.NavigateUri.AbsoluteUri);
        }        

        private Button FilmBlockCreator(string Title, string Link, string LinkSkin, double numStar, SolidColorBrush BackgroundColor, SolidColorBrush ReplayColor, string ReplayedPosition)
        {
            Grid mainReplayGrid = new Grid();
            Image ImageSkin = new Image();
            Grid dataTextGrid = new Grid();
            TextBlock nicknameReplayTxt = new TextBlock();
            TextBlock messsageReplayTxt = new TextBlock();

            TextBlock FilmTitle = new TextBlock();
            FilmTitle.Margin = new Thickness(3);
            FilmTitle.TextWrapping = TextWrapping.Wrap;
            FilmTitle.FontSize = NickNameTextSize - 2;
            FilmTitle.HorizontalAlignment = HorizontalAlignment.Left;
            FilmTitle.VerticalAlignment = VerticalAlignment.Center;
            FilmTitle.Style = (Style)FindResource("MaterialDesignSubtitle2TextBlock");
            FilmTitle.Foreground = ReplayColor;
            FilmTitle.Text = Title;

            TextBlock FilmLink = new TextBlock();
            FilmLink.Margin = new Thickness(3);
            FilmLink.TextWrapping = TextWrapping.NoWrap;
            FilmLink.FontSize = MessageTextSize - 2;
            FilmLink.HorizontalAlignment = HorizontalAlignment.Left;
            FilmLink.VerticalAlignment = VerticalAlignment.Center;
            FilmLink.FontFamily = new FontFamily("Calibri Light");
            FilmLink.Text = Link;

            TextBlock Star = new TextBlock();
            Star.Margin = new Thickness(3);
            Star.HorizontalAlignment = HorizontalAlignment.Left;
            Star.VerticalAlignment = VerticalAlignment.Center;
            Star.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#F9A825"));
            Star.FontSize = 20;
            for (int i = 0; i < Math.Round(numStar); i++)
            {
                Star.Text = Star.Text + "★";
            }

            RowDefinition rowDef1 = new RowDefinition();
            RowDefinition rowDef2 = new RowDefinition();
            RowDefinition rowDef3 = new RowDefinition();
            rowDef1.Height = new GridLength(1, GridUnitType.Auto);
            rowDef2.Height = new GridLength(1, GridUnitType.Auto);
            rowDef3.Height = new GridLength(1, GridUnitType.Auto);
            dataTextGrid.RowDefinitions.Add(rowDef1);
            dataTextGrid.RowDefinitions.Add(rowDef2);
            dataTextGrid.RowDefinitions.Add(rowDef3);
            Grid.SetRow(FilmTitle, 0);
            Grid.SetRow(FilmLink, 1);
            Grid.SetRow(Star, 2);
            dataTextGrid.Children.Add(FilmTitle);
            dataTextGrid.Children.Add(FilmLink);
            dataTextGrid.Children.Add(Star);

            ColumnDefinition colDef1 = new ColumnDefinition();
            ColumnDefinition colDef2 = new ColumnDefinition();
            colDef1.Width = new GridLength(1, GridUnitType.Auto);
            colDef2.Width = new GridLength(1, GridUnitType.Auto);

            var image = new Image();
            var fullFilePath = LinkSkin;
            BitmapImage bitmap = new BitmapImage();
            try
            {
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(fullFilePath, UriKind.Absolute);
                bitmap.EndInit();
            }
            catch (Exception)
            {

            }
            image.Source = bitmap;
            ImageSkin.Height = 69.75;
            ImageSkin.Width = 47.5;
            ImageSkin.Source = image.Source;
            ImageSkin.VerticalAlignment = VerticalAlignment.Center;
            ImageSkin.HorizontalAlignment = HorizontalAlignment.Left;
            ImageSkin.Margin = new Thickness(2);

            Grid.SetColumn(ImageSkin, 0);
            Grid.SetColumn(dataTextGrid, 1);
            mainReplayGrid.ColumnDefinitions.Add(colDef1);
            mainReplayGrid.ColumnDefinitions.Add(colDef2);
            mainReplayGrid.Children.Add(ImageSkin);
            mainReplayGrid.Children.Add(dataTextGrid);

            Button ReplayContainer = new Button();
            ReplayContainer.Tag = ReplayedPosition;
            ReplayContainer.Padding = new Thickness(0);
            RippleAssist.SetFeedback(ReplayContainer, (SolidColorBrush)(new BrushConverter().ConvertFrom("#BDBDBD")));
            ButtonAssist.SetCornerRadius(ReplayContainer, new CornerRadius(3));
            ShadowAssist.SetShadowDepth(ReplayContainer, ShadowDepth.Depth0);
            ReplayContainer.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
            ReplayContainer.Background = BackgroundColor;
            ReplayContainer.Style = (Style)FindResource("MyButton");
            ReplayContainer.VerticalAlignment = VerticalAlignment.Center;
            ReplayContainer.HorizontalAlignment = HorizontalAlignment.Left;
            ReplayContainer.Content = mainReplayGrid;

            return ReplayContainer;
        }

        private Button ButtonQuality(string Title, string Link, string LinkSkin, double numStar, SolidColorBrush BackgroundColor, SolidColorBrush ReplayColor, string ReplayedPosition)
        {
            Grid mainReplayGrid = new Grid();
            Grid dataTextGrid = new Grid();
            TextBlock nicknameReplayTxt = new TextBlock();
            TextBlock messsageReplayTxt = new TextBlock();

            TextBlock FilmTitle = new TextBlock();
            FilmTitle.Margin = new Thickness(3);
            FilmTitle.TextWrapping = TextWrapping.Wrap;
            FilmTitle.FontSize = NickNameTextSize - 2;
            FilmTitle.HorizontalAlignment = HorizontalAlignment.Left;
            FilmTitle.VerticalAlignment = VerticalAlignment.Center;
            FilmTitle.Style = (Style)FindResource("MaterialDesignSubtitle2TextBlock");
            FilmTitle.Foreground = ReplayColor;
            FilmTitle.Text = Title;

            TextBlock FilmLink = new TextBlock();
            FilmLink.Margin = new Thickness(3);
            FilmLink.TextWrapping = TextWrapping.NoWrap;
            FilmLink.FontSize = MessageTextSize - 2;
            FilmLink.HorizontalAlignment = HorizontalAlignment.Left;
            FilmLink.VerticalAlignment = VerticalAlignment.Center;
            FilmLink.FontFamily = new FontFamily("Calibri Light");
            FilmLink.Text = Link;

            TextBlock Star = new TextBlock();
            Star.Margin = new Thickness(3);
            Star.HorizontalAlignment = HorizontalAlignment.Left;
            Star.VerticalAlignment = VerticalAlignment.Center;
            Star.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#F9A825"));
            Star.FontSize = 20;
            for (int i = 0; i < Math.Round(numStar); i++)
            {
                Star.Text = Star.Text + "★";
            }

            RowDefinition rowDef1 = new RowDefinition();
            RowDefinition rowDef2 = new RowDefinition();
            RowDefinition rowDef3 = new RowDefinition();
            rowDef1.Height = new GridLength(1, GridUnitType.Auto);
            rowDef2.Height = new GridLength(1, GridUnitType.Auto);
            rowDef3.Height = new GridLength(1, GridUnitType.Auto);
            dataTextGrid.RowDefinitions.Add(rowDef1);
            dataTextGrid.RowDefinitions.Add(rowDef2);
            dataTextGrid.RowDefinitions.Add(rowDef3);
            Grid.SetRow(FilmTitle, 0);
            Grid.SetRow(FilmLink, 1);
            Grid.SetRow(Star, 2);
            dataTextGrid.Children.Add(FilmTitle);
            dataTextGrid.Children.Add(FilmLink);
            dataTextGrid.Children.Add(Star);

            ColumnDefinition colDef1 = new ColumnDefinition();
            ColumnDefinition colDef2 = new ColumnDefinition();
            colDef1.Width = new GridLength(1, GridUnitType.Auto);
            colDef2.Width = new GridLength(1, GridUnitType.Auto);

            Grid.SetColumn(dataTextGrid, 1);
            mainReplayGrid.ColumnDefinitions.Add(colDef1);
            mainReplayGrid.ColumnDefinitions.Add(colDef2);
            mainReplayGrid.Children.Add(dataTextGrid);

            Button Quality = new Button();
            Quality.Tag = ReplayedPosition;
            Quality.Padding = new Thickness(0);
            RippleAssist.SetFeedback(Quality, (SolidColorBrush)(new BrushConverter().ConvertFrom("#BDBDBD")));
            ButtonAssist.SetCornerRadius(Quality, new CornerRadius(3));
            ShadowAssist.SetShadowDepth(Quality, ShadowDepth.Depth0);
            Quality.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
            Quality.Background = BackgroundColor;
            Quality.Style = (Style)FindResource("MyButton");
            Quality.VerticalAlignment = VerticalAlignment.Center;
            Quality.HorizontalAlignment = HorizontalAlignment.Left;
            Quality.Content = mainReplayGrid;

            return Quality;
        }

        private void insertSpace(Grid container, double spaceHeight)
        {
            RowDefinition Space = new RowDefinition();
            Space.Height = new GridLength(spaceHeight);
            container.RowDefinitions.Add(Space);
        }

        public static Color ChangeColorBrightness(Color color, float correctionFactor)
        {
            float red = (float)color.R;
            float green = (float)color.G;
            float blue = (float)color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return Color.FromArgb(color.A, (byte)red, (byte)green, (byte)blue);
        }

        private void MessageContainer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SendText.Focus();

            Button MessageToResponse = (Button)sender;
            Grid GridMessageContainer = (Grid)(UIElement)MessageToResponse.Content;

            IEnumerable<Grid> Grid = GridMessageContainer.Children.OfType<Grid>();
            foreach (var GridData in Grid)
            {
                IEnumerable<TextBlock> Text = GridData.Children.OfType<TextBlock>();
                foreach (var TextData in Text)
                {
                    if (TextData.Name == "message")
                    {
                        ResponseMessage.Text = TextData.Text;
                    }
                }
            }

            var Tag_Message = MessageToResponse.Tag.ToString().Split(new string[] { "}£(&{$)(" }, StringSplitOptions.None);
            ResponseNickName.Text = Tag_Message[1];
            ResponseNickName.Tag = Tag_Message[0];
            if (ResponseNickName.Text == MqttConnection.NickName)
            {
                ResponseNickName.Foreground = (SolidColorBrush)(FindResource("SecondaryColorMessage"));
                Arrow.Foreground = (SolidColorBrush)(FindResource("SecondaryColorMessage"));
            }
            else
            {
                ResponseNickName.Foreground = new SolidColorBrush(ChangeColorBrightness(Color.FromArgb(255, (byte)(ResponseNickName.Text.Length * 3), (byte)(ResponseNickName.Text.Length * 8.5), (byte)(ResponseNickName.Text.Length * 5)), 20));
                Arrow.Foreground = new SolidColorBrush(ChangeColorBrightness(Color.FromArgb(255, (byte)(ResponseNickName.Text.Length * 3), (byte)(ResponseNickName.Text.Length * 8.5), (byte)(ResponseNickName.Text.Length * 5)), 20));
            }

            Storyboard WritingUp = (Storyboard)TryFindResource("ReplayUp");
            WritingUp.Begin();
        }

        private void MessageReplayed_Click(object sender, RoutedEventArgs e)
        {
            Button MessageReplayed = (Button)sender;
            MessageBox.Show(MessageReplayed.Tag.ToString());
            var offset_RowNum = MessageReplayed.Tag.ToString().Split('*');
            ScrollChatBox.ScrollToVerticalOffset(double.Parse(offset_RowNum[0]));
            ScrollChatBox.UpdateLayout();
            MessageBox.Show(ChatBoxContainer.RowDefinitions[int.Parse(offset_RowNum[1].ToString())].ActualHeight.ToString());
        }

        public bool CheckVectOnlineUser(string[] usersOnline, string nickname)
        {
            for (int i = 0; i < 20; i++)
            {
                if (MqttConnection.NickName != nickname)
                {
                    if (usersOnline[i] == nickname)
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        public void addVectOnlineUser(string nickname)
        {
            for (int i = 0; i < 20; i++)
            {
                if (Onlineuser[i] == null)
                {
                    Onlineuser[i] = nickname;
                    break;
                }
            }
        }

        public void removeOfflineUser(string nickname)
        {
            for (int i = 0; i < 20; i++)
            {
                if (Onlineuser[i] == nickname)
                {
                    Onlineuser[i] = null;
                    break;
                }
            }

            ChipUserContainer.Children.Clear();
            ChipBoxUsersOnline = 0;

            for (int i = 0; i < 20; i++)
            {
                if (Onlineuser[i] != null)
                {
                    addUserOnline(Onlineuser[i]);
                }
            }
        }

        double ChipBoxUsersOnline = 0;
        public void addUserOnline(string nickname)
        {
            Chip userOnline = new Chip();
            userOnline.Foreground = Brushes.White;
            userOnline.IconBackground = new SolidColorBrush(ChangeColorBrightness(Color.FromArgb(255, (byte)(nickname.Length * 3), (byte)(nickname.Length * 8.5), (byte)(nickname.Length * 5)), 20));
            userOnline.Content = nickname;
            userOnline.Icon = nickname.Substring(0, 1).ToUpper();
            userOnline.Margin = new Thickness(8, ChipBoxUsersOnline, 0, 0);
            ChipUserContainer.Children.Add(userOnline);
            ChipBoxUsersOnline = ChipBoxUsersOnline + 40;
        }

        public void removeUserOnline(string nickname)
        {
            IEnumerable<Chip> ChipUser = ChipUserContainer.Children.OfType<Chip>();
            foreach (var ChipBox in ChipUser)
            {
                if (ChipBox.Content.ToString() == nickname)
                {
                    ChipUserContainer.Children.Remove(ChipBox);
                    break;
                }
            }
        }

        private double getlastHeightMessageBox()
        {
            double lastHeight = 0;
            IEnumerable<Card> ChatBox1 = ChatBoxContainer.Children.OfType<Card>();
            foreach (var CardBox in ChatBox1)
            {
                lastHeight = CardBox.ActualHeight;
            }
            return lastHeight;
        }

        private double setPositionMessageBox(Canvas canvas)
        {
            double MessageContainerPosition = 0;
            IEnumerable<Card> ChatBox1 = canvas.Children.OfType<Card>();
            int i = 0;
            foreach (var CardBox in ChatBox1)
            {
                i++;
                MessageContainerPosition = MessageContainerPosition + CardBox.ActualHeight;
            }
            MessageContainerPosition = MessageContainerPosition + (10.00 * i);
            return MessageContainerPosition;
        }

        private void LogOutBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MqttConnection.IsSolace)
            {
                MqttConnection.MqttUnsecurePublish("solace-cloud-client/clients", "{\"status\":\"disconnected\",\"at\":\"2020-10-14T00:23:28.481Z\",\"client\":{\"id\":\"dzbpiydeyvrngch_" + MqttConnection.NickName + "\",\"ip\":\"78.74.65.35\"}}");
            }
            EndTackTimer.Start();
            CheckBorker.Stop();
            MqttConnection.MqttDisconnect();
            MqttConnection.setMqttServer("io.adafruit.com");
            MqttConnection.setMqttPort("8883");
            MqttConnection.setNickname("");
            MqttConnection.setChatRoomNameKey("");
            MqttConnection.setAioKey("");
            MqttConnection.setUser("");
            MqttConnection.IsSolace = false;
            CloseAllWindows();
            MainWindow m = new MainWindow();
            m.Show();
            Hide();
        }

        private void CloseAllWindows()
        {
            for (int intCounter = App.Current.Windows.Count - 1; intCounter > 0; intCounter--)
                App.Current.Windows[intCounter].Hide();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ChatBoxContainer.Width = ScrollChatBox.Width;

            ChipUserContainer.Height = ChipUser.Height;
            ChipUserContainer.Width = ChipUser.Width;

            LogOutBtn.Margin = new Thickness(0, 0, 0, 10);
        }

        private void ToggleButtonOpenMenu1_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButtonOpenMenu1.IsChecked = false;
        }
























        public void BufferingProgressCirle_Tick(object sender, EventArgs e)
        {
            BufferingCircle.IsIndeterminate = true;
            BtnPlay.IsEnabled = false;
            BtnTenMinus.IsEnabled = false;
            BtnTenPlus.IsEnabled = false;
            BtnSubtitle.IsEnabled = false;
            BtnLanguage.IsEnabled = false;
            BtnSync.IsEnabled = false;
            ProgressTimeline.IsEnabled = false;
            BufferingProgressCirle.Stop();
        }

        bool getDuration = false;
        bool stopStreaming = false;
        public void PlayingVideo_Tick(object sender, EventArgs e)
        {
            if (!getDuration)
            {
                if (HdMarioMovie)
                {
                    ProgressTimeline.Maximum = MovieTime;
                    TotalTime.Text = getCurrentVideoTime(MovieTime);
                }
                else
                {
                    ProgressTimeline.Maximum = VideoView.MediaPlayer.Length;
                    TotalTime.Text = getCurrentVideoTime(VideoView.MediaPlayer.Length);
                }
                
                getDuration = true;
                VideoView.MediaPlayer.Pause();
                IcnBtnPlay.Kind = PackIconKind.Play;
                BtnPlay.IsEnabled = true;
                BtnStop.IsEnabled = true;
                BtnTenMinus.IsEnabled = true;
                BtnTenPlus.IsEnabled = true;
                BtnSubtitle.IsEnabled = true;
                BtnLanguage.IsEnabled = true;
                ProgressTimeline.IsEnabled = true;
                BtnUpdateUrl.IsEnabled = false;
                if (!OnlyMe)
                    MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, MqttConnection.NickName + "_VIDEOPLAYER_READYSTART");
            }

            if (stopStreaming)
            {
                stopStreaming = false;
                VideoView.MediaPlayer.Pause();
                IcnBtnPlay.Kind = PackIconKind.Play;
                if (!OnlyMe)
                    MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, MqttConnection.NickName + "_VIDEOPLAYER_READYCONTINUE");
            }

            ProgressTimeline.IsEnabled = true;
            BufferingCircle.IsIndeterminate = false;
            BtnPlay.IsEnabled = true;
            BtnTenMinus.IsEnabled = true;
            BtnTenPlus.IsEnabled = true;
            BtnSubtitle.IsEnabled = true;
            BtnLanguage.IsEnabled = true;
            if (!OnlyMe)
                BtnSync.IsEnabled = true;
            long VideoTime = VideoView.MediaPlayer.Time;
            ProgressTimeline.Value = VideoTime + offsetHdMarioTimeline;
            CurrentTime.Text = getCurrentVideoTime((long)(VideoTime + offsetHdMarioTimeline));


            if (VideoView.MediaPlayer.State == VLCState.Ended)
            {
                EndTackTimer.Start();
            }

            PlayingVideo.Stop();
        }

        private void PausedStatus_Tick(object sender, EventArgs e)
        {
            PausedStatus.Stop();
        }

        private void MediaPlayer_Buffering(object sender, MediaPlayerBufferingEventArgs e)
        {
            BufferingProgressCirle.Start();
        }

        private void MediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            PlayingVideo.Start();
        }

        public string getCurrentVideoTime(long currentime)
        {
            string Time = "";

            int Seconds = ((int)(currentime / 1000));
            int Minute = ((int)(currentime / 1000 / 60));
            int Hour = ((int)(currentime / 1000 / 60 / 60));

            if (Seconds > 59)
            {
                while (Seconds > 59)
                {
                    Seconds = Seconds - 60;
                }
            }

            if (Minute > 59)
            {
                while (Minute > 59)
                {
                    Minute = Minute - 60;
                }
            }

            string seconds;
            string minute;
            string hour = Hour.ToString();

            if (Seconds < 10)
            {
                seconds = "0" + Seconds.ToString();
            }
            else
            {
                seconds = Seconds.ToString();
            }

            if (Minute < 10)
            {
                minute = "0" + Minute.ToString();
            }
            else
            {
                minute = Minute.ToString();
            }


            Time = hour + ":" + minute + ":" + seconds;
            return Time;
        }

        private void ChosePlayerMood(string NickName, string Url)
        {
            if (Url.Contains("https://supervideo.tv/"))
            {
                MessageAllertCardBox("Searching link stream in the Supervideo database...", PackIconKind.DatabaseSearch, "#FFFFFF", "#123B82");
                NickUpload = NickName;
                supervideo.DownloadStringAsync(new Uri(Url.Substring(4)));
            }
            else if (Url.Contains("https://streamtape.com/"))
            {
                MessageAllertCardBox("Searching link stream in the StreamTape database...", PackIconKind.DatabaseSearch, "#FFFFFF", "#123B82");
                NickUpload = NickName;
                streamtape.DownloadStringAsync(new Uri(Url.Substring(4)));
            }
            else if (Url.Contains("https://altadefinizione."))
            {
                MessageAllertCardBox("Searching link video in the AltaDefinizione database...", PackIconKind.DatabaseSearch, "#FFFFFF", "#123B82");
                NickUpload = NickName;
                GetHdPassHtml.DownloadStringAsync(new Uri(Url.Substring(4)));
            }
            else if (Url.Contains("https://hdmario."))
            {
                MessageAllertCardBox("Searching link video in the HD Mario database...", PackIconKind.DatabaseSearch, "#FFFFFF", "#123B82");
                NickUpload = NickName;
                getHdMarioM3u8(Url.Substring(4));
            }
            else
            {
                //MessageBox.Show(Url);
                setVideoPlayer(NickName, Url);
            }
        }

        public void setVideoPlayer(string nickname, string command)
        {
            if (command == "PLAY")
            {
                try
                {
                    VideoView.MediaPlayer.Play();
                    BtnStop.IsEnabled = true;
                    MessageAllertCardBox(nickname + " played the video", PackIconKind.Play, "#FFFFFF", "#BF360C");
                    IcnBtnPlay.Kind = PackIconKind.Pause;
                }
                catch (Exception)
                {
                    MessageAllertCardBox("Error playing video!", PackIconKind.LinkVariantPlus, "#FFFFFF", "#BF360C");
                }

            }
            else if (command == "PAUSE")
            {
                try
                {
                    VideoView.MediaPlayer.Pause();
                    MessageAllertCardBox(nickname + " paused the video", PackIconKind.Pause, "#FFFFFF", "#BF360C");
                    IcnBtnPlay.Kind = PackIconKind.Play;
                }
                catch (Exception)
                {
                    MessageAllertCardBox("Error pause video!", PackIconKind.LinkVariantPlus, "#FFFFFF", "#BF360C");
                }
            }
            else if (command == "STOP")
            {
                MessageAllertCardBox(nickname + " stopped the video", PackIconKind.Stop, "#FFFFFF", "#BF360C");
                EndTackTimer.Start();
            }
            else if (command == "READYSTART")
            {
                if (MqttConnection.NickName != nickname)
                {
                    MessageAllertCardBox(nickname + " is ready to start video!", PackIconKind.AccountTick, "#FFFFFF", "#BF360C");
                }
            }
            else if (command == "READYCONTINUE")
            {
                if (MqttConnection.NickName != nickname)
                {
                    MessageAllertCardBox(nickname + " is ready to continue video!", PackIconKind.AccountTick, "#FFFFFF", "#BF360C");
                }
            }
            else if (command.Contains("URL"))
            {
                string link = command.Substring(4);
                ChangeStreamingUrl(link, true);
            }
            else if (command.Contains("JUMP") || command.Contains("SYNC"))
            {
                var TimeJump = command.Split(':');
                if (command.Contains("JUMP"))
                {
                    MessageAllertCardBox(nickname + " jumped to: " + getCurrentVideoTime(Convert.ToInt64(TimeJump[1])), PackIconKind.Jump, "#FFFFFF", "#BF360C");
                }
                else if (command.Contains("SYNC"))
                {
                    MessageAllertCardBox(nickname + " sync movie to: " + getCurrentVideoTime(Convert.ToInt64(TimeJump[1])), PackIconKind.Jump, "#FFFFFF", "#BF360C");
                }
                JumpTimeLine(Convert.ToInt64(TimeJump[1]), nickname, false);
            }

        }

        string LinkStreamUrlVideo = "";
        public void ChangeStreamingUrl(string URL, bool Allert)
        {
            //MessageBox.Show(URL);
            if (!VideoView.MediaPlayer.IsPlaying)
            {
                try
                {
                    BufferingCircle.IsIndeterminate = true;
                    VideoView.MediaPlayer.FileCaching = 100;
                    VideoView.MediaPlayer.NetworkCaching = 300;
                    VideoView.MediaPlayer.Play(new Media(_libVLC, URL, FromType.FromLocation));
                    LinkStreamUrlVideo = URL;
                    VideoView.MediaPlayer.Buffering += new EventHandler<MediaPlayerBufferingEventArgs>(MediaPlayer_Buffering);
                    VideoView.MediaPlayer.TimeChanged += new EventHandler<MediaPlayerTimeChangedEventArgs>(MediaPlayer_TimeChanged);
                    var splitUrl = URL.Split('/');
                    ServerAddress = splitUrl[2];
                    StreamingDataDownload.Start();
                    PingServer.Start();
                    PingServer.Start();
                    VideoView.MediaPlayer.Volume = (int)ProgVolume.Value;
                    if (Allert)
                        if (NickUpload != "")
                            MessageAllertCardBox(NickUpload + " update video stream, please wait and trust in us!", PackIconKind.LinkVariantPlus, "#FFFFFF", "#BF360C");
                        else
                            MessageAllertCardBox(MqttConnection.NickName + " update video stream, please wait and trust in us!", PackIconKind.LinkVariantPlus, "#FFFFFF", "#BF360C");
                    getDuration = false;
                    stopStreaming = false;
                    NickUpload = "";
                }
                catch (Exception)
                {
                    BufferingCircle.IsIndeterminate = false;
                    if (Allert)
                        MessageAllertCardBox("Error update Url!", PackIconKind.LinkVariantPlus, "#FFFFFF", "#BF360C");
                    VideoView.MediaPlayer.Stop();
                }
            }
            else
            {
                MessageAllertCardBox("Please, at first pause / stop video", PackIconKind.ArrowLeft, "#FFFFFF", "#212121");
            }

        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (VideoView.MediaPlayer.IsPlaying)
            {
                if (!OnlyMe)
                    MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, MqttConnection.NickName + "_VIDEOPLAYER_PAUSE");
                else
                {
                    try
                    {
                        VideoView.MediaPlayer.Pause();
                        MessageAllertCardBox("You paused the video", PackIconKind.Pause, "#FFFFFF", "#BF360C");
                        IcnBtnPlay.Kind = PackIconKind.Play;
                    }
                    catch (Exception)
                    {
                        MessageAllertCardBox("Error pause video!", PackIconKind.LinkVariantPlus, "#FFFFFF", "#BF360C");
                    }
                }
            }
            else
            {
                if (SuppressChat.Height == 38)
                {
                    SuppressChat.Background = Brushes.Black;
                }
                if (!OnlyMe)
                    MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, MqttConnection.NickName + "_VIDEOPLAYER_PLAY");
                else
                {
                    try
                    {
                        VideoView.MediaPlayer.Play();
                        BtnStop.IsEnabled = true;
                        MessageAllertCardBox("You played the video", PackIconKind.Play, "#FFFFFF", "#BF360C");
                        IcnBtnPlay.Kind = PackIconKind.Pause;
                    }
                    catch (Exception)
                    {
                        MessageAllertCardBox("Error playing video!", PackIconKind.LinkVariantPlus, "#FFFFFF", "#BF360C");
                    }
                }
            }
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (!OnlyMe)
                MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, MqttConnection.NickName + "_VIDEOPLAYER_STOP");
            else
            {
                MessageAllertCardBox("You stopped the video", PackIconKind.Stop, "#FFFFFF", "#BF360C");
                EndTackTimer.Start();
            }
        }

        private void BtnUpdateUrl_Click(object sender, RoutedEventArgs e)
        {
            SerieSearch = false;
            sendUrl = true;
            search = false;
            BtnSearch.IsEnabled = true;
            BtnUpdateUrl.IsEnabled = false;
            BtnNetflix.IsEnabled = true;
            HintAssist.SetHint(SendText, "PASTE URL HERE AND PRESS ENTER OR PRESS ESC TO ABORT");
            SendText.Focus();
        }

        private void BtnExpandReduce_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                IcnExpandReduce.Kind = PackIconKind.ArrowCollapseAll;
                if (SuppressChat.Height == 38 && ControlPannel.Height == 110)
                {
                    SuppressChat.Height = 0;
                    ControlPannel.Height = 0;
                }
            }
            else if (WindowState == WindowState.Maximized)
            {
                if (IcnExpandReduce.Kind == PackIconKind.ArrowExpandAll)
                {
                    WindowStyle = WindowStyle.None;
                    WindowState = WindowState.Maximized;
                    IcnExpandReduce.Kind = PackIconKind.ArrowCollapseAll;
                }
                else
                {
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    WindowState = WindowState.Normal;
                    IcnExpandReduce.Kind = PackIconKind.ArrowExpandAll;
                }
            }
            VideoViewGrid.Focus();
        }

        public async void EndTrack_Tick(object sender, EventArgs e)
        {
            if (VideoView.MediaPlayer.IsPlaying)
            {
                VideoView.MediaPlayer.Pause();
                await Task.Run(() => Thread.Sleep(500));
            }

            CurrentTime.Text = "00:00:00";
            TotalTime.Text = "00:00:00";

            LinkStream.RowDefinitions.Clear();
            LinkStream.ColumnDefinitions.Clear();
            LinkStream.Children.Clear();

            selected_Video_Res = "";
            selected_Video_Bit = "";
            MovieTime = 0;
            HdMarioMovie = false;
            Web1.LoadCompleted -= completeLoadWebHdMario;
            Web1.Navigate("about:blank");
            try { ffmpeg_process.Kill(); } catch (Exception) { }

            BtnPlay.IsEnabled = false;
            BtnStop.IsEnabled = false;
            BtnTenMinus.IsEnabled = false;
            BtnSync.IsEnabled = false;
            if (LinkStreamContainer.Height != 30)
            {
                BtnSearch.IsEnabled = true;
                BtnNetflix.IsEnabled = true;
            }
            BtnTenPlus.IsEnabled = false;
            BtnSubtitle.IsEnabled = false;
            BtnLanguage.IsEnabled = false;
            ProgressTimeline.IsEnabled = false;
            offsetHdMarioTimeline = 0;
            ProgressTimeline.Value = 0;
            BufferingCircle.IsIndeterminate = false;
            if (LinkStreamContainer.Height != 30)
                BtnUpdateUrl.IsEnabled = true;
            getDuration = false;
            stopStreaming = false;
            IcnBtnPlay.Kind = PackIconKind.Play;
            VideoView.MediaPlayer.Stop();
            DownloadData.Text = "Streaming speed: 0 Bps 0 ms";            
            ControlPannel.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0E1621"));
            SuppressChat.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0E1621"));
            StreamingDataDownload.Stop();
            NetworkCardViewer = false;
            PingServer.Stop();
            EndTackTimer.Stop();
        }

        double offsetHdMarioTimeline = 0;
        private async void JumpTimeLine(long time, string nickname, bool allert)
        {
            if (allert)
                MessageAllertCardBox(nickname +" jumped to: " + getCurrentVideoTime(Convert.ToInt64(time)), PackIconKind.Jump, "#FFFFFF", "#BF360C");
            BtnPlay.IsEnabled = false;
            BtnTenMinus.IsEnabled = false;
            BtnTenPlus.IsEnabled = false;
            BtnSubtitle.IsEnabled = false;
            BtnLanguage.IsEnabled = false;

            if (VideoView.MediaPlayer.IsPlaying)
            {
                IcnBtnPlay.Kind = PackIconKind.Play;
                VideoView.MediaPlayer.Pause();
            }
            await Task.Run(() => Thread.Sleep(500));

            VideoView.MediaPlayer.TimeChanged -= new EventHandler<MediaPlayerTimeChangedEventArgs>(MediaPlayer_TimeChanged);
            if (HdMarioMovie)
            {
                offsetHdMarioTimeline = time;
                MessageAllertCardBox("Please wait, I'm trying to find the corrispective streaming segment... (is very hard with a resolution 2K or 4K)", PackIconKind.WarningDecagram, "#FFFFFF", "#B0003A");
                try { ffmpeg_process.Kill(); } catch (Exception) { }
                await Task.Run(() => Thread.Sleep(1000));
                VideoView.MediaPlayer.Stop();
                BufferingCircle.IsIndeterminate = true;
                startTcpLocalStreaming("https://hdmario.live/pl/" + Movie_Id + ".m3u8?s=" + AccessKeyMovie, ((int)(time / 1000)).ToString(), selected_Video_Res, selected_Video_Bit);
                await Task.Run(() => Thread.Sleep(2500));
                VideoView.MediaPlayer.Play(new Media(_libVLC, "tcp://127.0.0.1:8080", FromType.FromLocation));
            }
            else
            {
                VideoView.MediaPlayer.Time = Convert.ToInt64(time);
            }

            VideoView.MediaPlayer.TimeChanged += new EventHandler<MediaPlayerTimeChangedEventArgs>(MediaPlayer_TimeChanged);
            IcnBtnPlay.Kind = PackIconKind.Pause;
            VideoView.MediaPlayer.Play();
            stopStreaming = true;
        }

        private void ProgressTimeline_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            double TimeLineValue = ProgressTimeline.Value;
            if (!OnlyMe)
            {
                MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, MqttConnection.NickName + "_VIDEOPLAYER_JUMP:" + ((long)TimeLineValue).ToString());
            }
            else
            {
                JumpTimeLine(Convert.ToInt64(TimeLineValue), MqttConnection.NickName, true);
            }
        }

        private void ProgressTimeline_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (VideoView.MediaPlayer.IsPlaying)
            {
                VideoView.MediaPlayer.Pause();
            }
        }

        private void BtnPlay_MouseEnter(object sender, MouseEventArgs e)
        {
            BtnPlay.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
        }

        private void BtnPlay_MouseLeave(object sender, MouseEventArgs e)
        {
            BtnPlay.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
        }

        private void BtnStop_MouseEnter(object sender, MouseEventArgs e)
        {
            BtnStop.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
        }

        private void BtnStop_MouseLeave(object sender, MouseEventArgs e)
        {
            BtnStop.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
        }

        private void BtnMute_MouseEnter(object sender, MouseEventArgs e)
        {
            BtnMute.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
        }

        private void BtnMute_MouseLeave(object sender, MouseEventArgs e)
        {
            BtnMute.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
        }

        private void BtnUpdateUrl_MouseEnter(object sender, MouseEventArgs e)
        {
            BtnUpdateUrl.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
        }

        private void BtnUpdateUrl_MouseLeave(object sender, MouseEventArgs e)
        {
            BtnUpdateUrl.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
        }

        private void BtnExpandReduce_MouseEnter(object sender, MouseEventArgs e)
        {
            BtnExpandReduce.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
        }

        private void BtnExpandReduce_MouseLeave(object sender, MouseEventArgs e)
        {
            BtnExpandReduce.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
        }

        private void BtnSubtitle_MouseEnter(object sender, MouseEventArgs e)
        {
            BtnSubtitle.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
        }

        private void BtnSubtitle_MouseLeave(object sender, MouseEventArgs e)
        {
            BtnSubtitle.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
        }

        private void BtnLanguage_MouseEnter(object sender, MouseEventArgs e)
        {
            BtnLanguage.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
        }

        private void BtnLanguage_MouseLeave(object sender, MouseEventArgs e)
        {
            BtnLanguage.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
        }

        private void BtnMute_Click(object sender, RoutedEventArgs e)
        {
            if (!VideoView.MediaPlayer.Mute)
            {
                IcnBtnMute.Kind = PackIconKind.VolumeMute;
                VideoView.MediaPlayer.Mute = true;
            }
            else
            {
                IcnBtnMute.Kind = PackIconKind.Audio;
                VideoView.MediaPlayer.Mute = false;
            }
        }

        private void BtnTenMinus_MouseEnter(object sender, MouseEventArgs e)
        {
            BtnTenMinus.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
        }

        private void BtnTenMinus_MouseLeave(object sender, MouseEventArgs e)
        {
            BtnTenMinus.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
        }

        private void BtnTenPlus_MouseEnter(object sender, MouseEventArgs e)
        {
            BtnTenPlus.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
        }

        private void BtnTenPlus_MouseLeave(object sender, MouseEventArgs e)
        {
            BtnTenPlus.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
        }

        private void ProgVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (VideoView.IsLoaded)
            {
                VideoView.MediaPlayer.Volume = (int)ProgVolume.Value;
            }
        }

        private void BtnTenMinus_Click(object sender, RoutedEventArgs e)
        {
            long Time = VideoView.MediaPlayer.Time - 10000;
            if(!OnlyMe)
                MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, MqttConnection.NickName + "_VIDEOPLAYER_JUMP:" + (Time).ToString());
            else
                JumpTimeLine(Convert.ToInt64(Time), MqttConnection.NickName, true);
        }

        private void BtnTenPlus_Click(object sender, RoutedEventArgs e)
        {
            long Time = VideoView.MediaPlayer.Time + 10000;
            if(!OnlyMe)
                MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, MqttConnection.NickName + "_VIDEOPLAYER_JUMP:" + (Time).ToString());
            else
                JumpTimeLine(Convert.ToInt64(Time), MqttConnection.NickName, true);
        }

        private void ProgressTimeline_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!VideoView.MediaPlayer.IsPlaying)
            {
                CurrentTime.Text = getCurrentVideoTime((int)ProgressTimeline.Value);
            }
        }

        private void BtnSubtitle_Click(object sender, RoutedEventArgs e)
        {
            if (VideoView.MediaPlayer.SpuCount > 0)
            {
                LibVLCSharp.Shared.Structures.TrackDescription[] Subtitles = VideoView.MediaPlayer.SpuDescription;
                int index = getIndex(Subtitles, VideoView.MediaPlayer.Spu, VideoView.MediaPlayer.SpuCount);
                if (index < VideoView.MediaPlayer.SpuCount - 1)
                {
                    VideoView.MediaPlayer.SetSpu(Subtitles[index + 1].Id);
                }
                else
                {
                    VideoView.MediaPlayer.SetSpu(Subtitles[0].Id);
                }
                MessageAllertCardBox("Subtitle setted: " + Subtitles[getIndex(Subtitles, VideoView.MediaPlayer.Spu, VideoView.MediaPlayer.SpuCount)].Name, PackIconKind.Subtitles, "#FFFFFF", "#212121");
            }
            else
            {
                MessageAllertCardBox("Subtitles not available!", PackIconKind.Subtitles, "#FFFFFF", "#B71C1C");
            }
        }

        private void BtnLanguage_Click(object sender, RoutedEventArgs e)
        {
            if (VideoView.MediaPlayer.AudioTrackCount > 0)
            {
                LibVLCSharp.Shared.Structures.TrackDescription[] Audio = VideoView.MediaPlayer.AudioTrackDescription;
                int index = getIndex(Audio, VideoView.MediaPlayer.AudioTrack, VideoView.MediaPlayer.AudioTrackCount);
                if (index < VideoView.MediaPlayer.AudioTrackCount - 1 && index > 0)
                {
                    VideoView.MediaPlayer.SetAudioTrack(Audio[index + 1].Id);
                }
                else
                {
                    VideoView.MediaPlayer.SetAudioTrack(Audio[1].Id);
                }
                MessageAllertCardBox("Language setted: " + Audio[getIndex(Audio, VideoView.MediaPlayer.AudioTrack, VideoView.MediaPlayer.AudioTrackCount)].Name, PackIconKind.Language, "#FFFFFF", "#212121");
            }
            else
            {
                MessageAllertCardBox("Audio track unavailable", PackIconKind.Language, "#FFFFFF", "#B71C1C");
            }
        }

        private int getIndex(LibVLCSharp.Shared.Structures.TrackDescription[] Track, int TrackId, int TotalTrack)
        {
            for (int i = 0; i < TotalTrack; i++)
            {
                if (Track[i].Id == TrackId)
                {
                    return i;
                }
            }
            return 0;
        }

        long previewDownloadData = 0;
        long currentDowloadData = 0;
        private void StreamingDataDownload_Tick(object sender, EventArgs e)
        {
            if (previewDownloadData == 0 && currentDowloadData == 0)
            {
                previewDownloadData = getCurrentDownloadData();
                return;
            }
            else if (previewDownloadData > 0)
            {
                currentDowloadData = getCurrentDownloadData();
                double DataByte = (int)(currentDowloadData - previewDownloadData);

                if (DataByte < 1024)
                {
                    DownloadData.Text = "Streaming speed: " + DataByte.ToString() + " Bps " + ServerPingResult + " ms";
                }
                else if (DataByte > 1023 && DataByte < 1048576)
                {
                    DownloadData.Text = "Streaming speed: " + Math.Round(DataByte / 1024, 2).ToString() + " Kbps " + ServerPingResult + " ms";
                }
                else if (DataByte > 1048575 && DataByte < 1073741824)
                {
                    DownloadData.Text = "Streaming speed: " + Math.Round(DataByte / 1024 / 1024, 2).ToString() + " Mbps " + ServerPingResult + " ms";
                }
                else if (DataByte > 1073741823 && DataByte > 1099511627776)
                {
                    DownloadData.Text = "Streaming speed: " + Math.Round(DataByte / 1024 / 1024 / 1024, 2).ToString() + " Gbps " + ServerPingResult + " ms";
                }

                previewDownloadData = currentDowloadData;
            }

        }

        int NetworkInterfaceIndex = 0;
        private long getCurrentDownloadData()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return 0;
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            return interfaces[NetworkInterfaceIndex].GetIPv4Statistics().BytesReceived;
        }

        /*private Grid generateGridMessageContainer(PackIcon Icon, TextBlock textBlock)
        {
            Grid messageGrid = new Grid();
            ColumnDefinition colDef1 = new ColumnDefinition();
            ColumnDefinition colDef2 = new ColumnDefinition();
            colDef2.Width = new GridLength(200);
            messageGrid.ColumnDefinitions.Add(colDef1);
            messageGrid.ColumnDefinitions.Add(colDef2);
            Grid.SetColumn(Icon, 0);
            Grid.SetColumn(textBlock, 1);
            messageGrid.Children.Add(Icon);
            messageGrid.Children.Add(textBlock);

            return messageGrid;
        }

        

        private PackIcon generatePackIcon(PackIconKind iconKind, string colorIcon)
        {
            PackIcon packIcon = new PackIcon();
            packIcon.Height = 30;
            packIcon.Width = 30;
            packIcon.Kind = iconKind;
            packIcon.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom(colorIcon));

            return packIcon;
        }

        private void BtnInfo_Click(object sender, RoutedEventArgs e)
        {
            PackIconKind[] packIcons = { PackIconKind.Play, PackIconKind.Stop, PackIconKind.Rewind10, PackIconKind.FastForward10, PackIconKind.Subtitles, PackIconKind.Language, PackIconKind.Audio, PackIconKind.LinkVariantPlus, PackIconKind.Sync, PackIconKind.ArrowExpandAll, PackIconKind.Menu };
            string[] messageText = { "Start and pause video for all user", "Stop video for all user", "Back 10 sec in video for all user", "Next 10 sec in video for all user", "Chage subtitles, if available", "Change language, if available", "Audio on / Mute", "Add link video stream for all user", "Sync video for all user", "Full screen video", "Menu, you can see users online, update software, make log out" };

            Grid MainGrid = new Grid();
            RowDefinition row1 = new RowDefinition();
            RowDefinition row2 = new RowDefinition();
            RowDefinition row3 = new RowDefinition();
            RowDefinition row4 = new RowDefinition();
            MainGrid.RowDefinitions.Add(row1);
            MainGrid.RowDefinitions.Add(row2);
            MainGrid.RowDefinitions.Add(row3);
            MainGrid.RowDefinitions.Add(row4);

            Grid TopMessageGrid = new Grid();
            Grid BodyMessageGrid = new Grid();
            Grid SubText = new Grid();
            Grid BottomMessageGrid = new Grid();
            TopMessageGrid.Height = 35;
            SubText.Height = 125;
            SubText.Width = 260;
            BottomMessageGrid.Height = 45;

            TopMessageGrid.Children.Add(generateTextBlock("Info point", FontStyles.Normal, (Style)FindResource("MaterialDesignHeadline2TextBlock"), 20, VerticalAlignment.Top));
            for (int i = 0; i < messageText.Length; i++)
            {
                PackIcon packIcon = generatePackIcon(packIcons[i], "#FFFFFF");
                TextBlock textBlock = generateTextBlock(messageText[i], FontStyles.Italic, (Style)FindResource("MaterialDesignCaptionTextBlock"), 12, VerticalAlignment.Center);
                Grid messageContainer = generateGridMessageContainer(packIcon, textBlock);

                RowDefinition row = new RowDefinition();
                BodyMessageGrid.RowDefinitions.Add(row);
                Grid.SetRow(messageContainer, i);
                BodyMessageGrid.Children.Add(messageContainer);
            }
            SubText.Children.Add(generateTextBlock('\n' + "For a good streaming speed we recommended to use a cable connection (like ethernet) or a good WiFi connection." + '\n' + "You can check your streaming speed on top of chat, if is 0 you can click on the text and look in the chat box the internet network selected for reed the stream speed." + '\n' + "A good streaming speed is over 1.5 Mbps", FontStyles.Italic, (Style)FindResource("MaterialDesignBody1TextBlock"), 11, VerticalAlignment.Top));
            BottomMessageGrid.Children.Add(generateTextBlock("Software version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + '\n' + "Created by: PeppeRomano98", FontStyles.Normal, (Style)FindResource("MaterialDesignBody1TextBlock"), 10, VerticalAlignment.Bottom));

            Grid.SetRow(TopMessageGrid, 0);
            Grid.SetRow(BodyMessageGrid, 1);
            Grid.SetRow(SubText, 2);
            Grid.SetRow(BottomMessageGrid, 3);
            MainGrid.Children.Add(TopMessageGrid);
            MainGrid.Children.Add(BodyMessageGrid);
            MainGrid.Children.Add(SubText);
            MainGrid.Children.Add(BottomMessageGrid);

            Card MessageContainer = new Card();
            MessageContainer.Padding = new Thickness(8);
            ShadowAssist.SetShadowDepth(MessageContainer, ShadowDepth.Depth0);
            MessageContainer.UniformCornerRadius = 8;
            MessageContainer.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
            MessageContainer.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#182533"));
            MessageContainer.MaxWidth = 285;
            MessageContainer.Content = MainGrid;

            RowDefinition rowDefinition = new RowDefinition();
            rowDefinition.Height = new GridLength(MessageContainer.ActualHeight + 10);
            Grid.SetRow(MessageContainer, ChatBoxContainer.RowDefinitions.Count);
            ChatBoxContainer.RowDefinitions.Add(rowDefinition);
            ChatBoxContainer.Children.Add(MessageContainer);

            ScrollChatBox.ScrollToBottom();
        }*/

        private void LogOutBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            LogOutBtn.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
        }

        private void LogOutBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            LogOutBtn.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
        }

        bool NetworkCardViewer = false;
        private void DownloadData_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (LinkStreamContainer.Height == 0)
            {
                NetworkCardViewer = true;
                StreamingDataDownload.Stop();
                PingServer.Stop();
                previewDownloadData = 0;
                currentDowloadData = 0;
                DownloadData.Text = "Streaming speed: 0 Bps 0 ms";
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                    LinkStream.RowDefinitions.Clear();
                    LinkStream.ColumnDefinitions.Clear();
                    LinkStream.Children.Clear();
                    for (int i = 0; i < interfaces.Length; i++)
                    {
                        CreateBtnLinkHost(interfaces[i].Name, "CARD " + (i + 1).ToString(), handlerNeworkCardClick);
                    }
                    HintAssist.SetHint(SendText, "Type a message");
                    search = false;
                    SendText.Text = "";
                    TextLink.Text = "Select your network card";
                    ProgressFilmLink.IsIndeterminate = false;
                    Storyboard NetworkCardListUp = (Storyboard)TryFindResource("FilmLinkListUp");
                    NetworkCardListUp.Begin();
                }
            }
            else
            {
                MessageAllertCardBox("Please at first close any other viewer", PackIconKind.WarningDecagram, "#FFFFFF", "#B0003A");
            }
        }

        private void handlerNeworkCardClick(object sender, RoutedEventArgs e)
        {
            Storyboard NeworkCardListDown = (Storyboard)TryFindResource("FilmLinkListDown");
            NeworkCardListDown.Begin();
            Button NeworkCardBtn = (Button)sender;
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            var tagBtn = NeworkCardBtn.Tag.ToString().Split(' ');
            NetworkInterfaceIndex = int.Parse(tagBtn[1]) - 1;
            MessageAllertCardBox("You have selected: " + interfaces[NetworkInterfaceIndex].Name, PackIconKind.Network, "#FFFFFF", "#424242");
            NetworkCardViewer = false;
            if (VideoView.IsLoaded)
            {
                StreamingDataDownload.Start();
                PingServer.Start();
            }
        }

        string ServerPingResult = "0";
        private void PingServer_Tick(object sender, EventArgs e)
        {
            if (ServerAddress != "" && !ServerAddress.Contains("127.0.0.1"))
                ServerPingResult = getPing(ServerAddress, 3000);
        }

        private string getPing(string server, int timeout)
        {
            Ping ping = new Ping();
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);

            PingOptions options = new PingOptions(64, true);
            PingReply reply = ping.Send(server, timeout, buffer, options);

            if (reply.Status == IPStatus.Success)
            {
                return reply.RoundtripTime.ToString();
            }
            else
            {
                return "fail";
            }
        }

        private void BtnSync_Click(object sender, RoutedEventArgs e)
        {
            long Time = VideoView.MediaPlayer.Time;
            MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, MqttConnection.NickName + "_VIDEOPLAYER_SYNC:" + (Time).ToString());
            BtnSync.IsEnabled = false;
        }

        private void BtnSync_MouseEnter(object sender, MouseEventArgs e)
        {
            BtnSync.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
        }

        private void BtnSync_MouseLeave(object sender, MouseEventArgs e)
        {
            BtnSync.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
        }

        private void UpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            DrawerHost.CloseDrawerCommand.Execute(DrawerHostContent, UpdateBtn);
            try
            {
                Process.Start(Environment.CurrentDirectory + "\\update\\Update.exe");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            UpdateBtn.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
        }

        private void UpdateBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            UpdateBtn.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
        }

        private void RoomMakerBtn_Click(object sender, RoutedEventArgs e)
        {
            DrawerHost.CloseDrawerCommand.Execute(DrawerHostContent, UpdateBtn);
            CheckBorker.Stop();
            RoomMaker roomMaker = new RoomMaker();
            roomMaker.Closed += new EventHandler(RoomMaker_Closed);
            roomMaker.Show();
        }

        private void RoomMaker_Closed(object sender, EventArgs e)
        {
            CheckBorker.Start();
        }

        private void RoomMakerBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            RoomMakerBtn.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
        }

        private void RoomMakerBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            RoomMakerBtn.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
        }

        private void MyRoomBtn_Click(object sender, RoutedEventArgs e)
        {
            MyRoomBtn.IsEnabled = false;
            DrawerHost.CloseDrawerCommand.Execute(DrawerHostContent, UpdateBtn);
            MyRoom myRoom = new MyRoom();
            myRoom.Closed += new EventHandler(MyRoomBtn_Closed);
            myRoom.Show();
        }

        private void MyRoomBtn_Closed(object sender, EventArgs e)
        {
            MyRoomBtn.IsEnabled = true;
        }

        private void MyRoomBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            MyRoomBtn.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
        }

        private void MyRoomBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            MyRoomBtn.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
        }

        bool writting = false;
        private async void SendText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!OnlyMe)
            {
                if (writting && string.IsNullOrWhiteSpace(SendText.Text) && !search && !sendUrl)
                {
                    writting = false;
                    await Task.Run(() => MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, MqttConnection.NickName + "_STOP-TYPING"));
                }
                else if (!writting && !string.IsNullOrWhiteSpace(SendText.Text) && !search && !sendUrl)
                {
                    writting = true;
                    await Task.Run(() => MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, MqttConnection.NickName + "_START-TYPING"));
                }
            }
        }

        private void CloseResponse_MouseEnter(object sender, MouseEventArgs e)
        {
            CloseResponse.Foreground = Brushes.White;
        }

        private void CloseResponse_MouseLeave(object sender, MouseEventArgs e)
        {
            CloseResponse.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#232E3C"));
        }

        private void CloseResponse_Click(object sender, RoutedEventArgs e)
        {
            Storyboard WritingDown = (Storyboard)TryFindResource("ReplayDown");
            WritingDown.Begin();
            SendText.Focus();
        }

        private void ToggleResizeControll_MouseEnter(object sender, MouseEventArgs e)
        {
            ToggleResizeControll.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
        }

        private void ToggleResizeControll_MouseLeave(object sender, MouseEventArgs e)
        {
            ToggleResizeControll.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
        }

        private void ToggleResizeControll_Click(object sender, RoutedEventArgs e)
        {
            if (ControlPannel.Height == 14)
            {
                ToggleArrowControl.Kind = PackIconKind.KeyboardArrowDown;
                SubControlPannel.IsEnabled = true;
                ControlPannel.Height = 110;
                ControlPannel.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0E1621"));
            }
            else
            {
                ToggleArrowControl.Kind = PackIconKind.KeyboardArrowUp;
                if (WindowState != WindowState.Maximized)
                {
                    ControlPannel.Height = 14;
                }
                else
                {
                    if (ChatRoom.Width == 320) { ControlPannel.Height = 14; }
                    else { ControlPannel.Height = 0; }
                }
                SubControlPannel.IsEnabled = false;
                if (VideoView.MediaPlayer.IsPlaying)
                    ControlPannel.Background = Brushes.Black;
            }
            VideoViewGrid.Focus();
        }

        private void ToggleResizeChat_MouseEnter(object sender, MouseEventArgs e)
        {
            ResizeChat.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
        }

        private void ToggleResizeChat_MouseLeave(object sender, MouseEventArgs e)
        {
            ResizeChat.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
        }

        private void ToggleResizeChat_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState != WindowState.Maximized)
            {
                SuppressChat.Height = 38;
            }
            else
            {
                SuppressChat.Height = 0;
                if (ControlPannel.Height == 14)
                {
                    ControlPannel.Height = 0;
                }
            }
            ChatRoom.Width = 0;
            SubChatRoom.Visibility = Visibility.Hidden;
            if (VideoView.MediaPlayer.IsPlaying)
                SuppressChat.Background = Brushes.Black;

            VideoViewGrid.Focus();
        }

        private void ToggleResizeChatSuppress_MouseEnter(object sender, MouseEventArgs e)
        {
            ToggleResizeChatSuppress.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
        }

        private void ToggleResizeChatSuppress_MouseLeave(object sender, MouseEventArgs e)
        {
            ToggleResizeChatSuppress.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
        }

        private void ToggleResizeChatSuppress_Click(object sender, RoutedEventArgs e)
        {
            SuppressChat.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0E1621"));
            SuppressChat.Height = 0;
            ChatRoom.Width = 320;
            SubChatRoom.Visibility = Visibility.Visible;
        }

        private void CloseLinks_Click(object sender, RoutedEventArgs e)
        {
            if (OnlyMe)
            {
                CloseSeriesViewer("You");
            }
            else
            {
                MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, MqttConnection.NickName + "_SERIEPLAYER_CLOSEVIEWER");
            }
        }

        private void CloseSeriesViewer(string nickname)
        {
            if (LinkStreamContainer.ActualHeight == 250 && !NetworkCardViewer)
            {
                EndTackTimer.Start();
                SerieHD_Client.CancelAsync();
                BtnSearch.IsEnabled = true;
                BtnNetflix.IsEnabled = true;
                ProgressFilmLink.IsIndeterminate = false;
                CollectingLabelContainer.Width = 0;
                contSeasons = 0;
                NumbersOfSeasons = 0;
                NumberOfEpisodies = 0;
                contEpisodies = 0;
                if (LinkStreamContainer.ActualHeight == 250)
                {
                    Storyboard s = (Storyboard)TryFindResource("FilmLinkListDown");
                    s.Begin();
                }
                else
                {
                    Storyboard s = (Storyboard)TryFindResource("LinkListDown");
                    s.Begin();
                }
                LinkStream.Children.Clear();
                MessageAllertCardBox(nickname + " close viewer", PackIconKind.WindowClose, "#FFFFFF", "#BF360C");
            }
            if (NetworkCardViewer)
            {
                if (LinkStreamContainer.ActualHeight == 250)
                {
                    Storyboard s = (Storyboard)TryFindResource("FilmLinkListDown");
                    s.Begin();
                }
                else
                {
                    Storyboard s = (Storyboard)TryFindResource("LinkListDown");
                    s.Begin();
                }
                LinkStream.Children.Clear();
                NetworkCardViewer = false;
            }
        }

        private void CloseLinks_MouseEnter(object sender, MouseEventArgs e)
        {
            CloseLinks.Foreground = Brushes.White;
        }

        private void CloseLinks_MouseLeave(object sender, MouseEventArgs e)
        {
            CloseLinks.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#737e87"));
        }

        private void BtnSearch_MouseEnter(object sender, MouseEventArgs e)
        {
            BtnSearch.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
        }

        private void BtnSearch_MouseLeave(object sender, MouseEventArgs e)
        {
            BtnSearch.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            SerieSearch = false;
            search = true;
            sendUrl = false;
            BtnNetflix.IsEnabled = true;
            BtnSearch.IsEnabled = false;
            //if (!OnlyMe)
                BtnUpdateUrl.IsEnabled = true;
            HintAssist.SetHint(SendText, "TYPE A NAME OF MOVIE, PRESS ENTER OR PRESS ESC TO ABORT");
            SendText.Focus();
        }

        private void MySolaceRoomBtn_Click(object sender, RoutedEventArgs e)
        {
            DrawerHost.CloseDrawerCommand.Execute(DrawerHostContent, MySolaceRoomBtn);
            try
            {
                KeyEndToEnd.DownloadStringTaskAsync("https://passwordsgenerator.net/calc.php?Length=50&Symbols=0&Lowercase=1&Uppercase=1&Numbers=1&Nosimilar=1&Last=1");
            }
            catch (Exception)
            {
                MessageBox.Show("Error generating key End-to-End");
            }            
        }

        private void getKEY(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                string NewKeyEndToEnd = e.Result.Substring(0, 50);
                MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, MqttConnection.NickName + "_KEY_" + NewKeyEndToEnd);
            }
            catch (Exception)
            {
                MessageBox.Show("Error room-key");
            }
        }

        private void MySolaceRoomBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            MySolaceRoomBtn.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
        }

        private void MySolaceRoomBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            MySolaceRoomBtn.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
        }

        private void ShareSolaceRoomBtn_Click(object sender, RoutedEventArgs e)
        {
            DrawerHost.CloseDrawerCommand.Execute(DrawerHostContent, ShareSolaceRoomBtn);
            string KeyRoomDec = "SolaceRoom|" + MqttConnection.UserName + "|" + MqttConnection.AioKey + "|" + MqttConnection.Key_EndToEnd + "|" + MqttConnection.NickName + "|" + LinkStreamUrlVideo + "|false|" + MqttConnection.MqttServer + "|" + MqttConnection.MqttPort.ToString();
            string KeyRoomEnc = StringCipher.Encrypt(KeyRoomDec, "");
            Clipboard.SetText(KeyRoomEnc);
            MessageAllertCardBox("Ok, now you can paste and share link-room, enjoy!", PackIconKind.ContentCopy, "#FFFFFF", "#B0003A");
        }

        private void ShareSolaceRoomBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            ShareSolaceRoomBtn.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
        }

        private void ShareSolaceRoomBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            ShareSolaceRoomBtn.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
        }

        public void SetOnlyMe(bool Variable)
        {
            OnlyMe = Variable;
        }

        private void BtnNetflix_Click(object sender, RoutedEventArgs e)
        {
            SerieSearch = true;
            search = false;
            sendUrl = false;
            BtnNetflix.IsEnabled = false;
            BtnSearch.IsEnabled = true;
            //if (!OnlyMe)
                BtnUpdateUrl.IsEnabled = true;
            HintAssist.SetHint(SendText, "TYPE A NAME OF SERIES, PRESS ENTER OR PRESS ESC TO ABORT");
            SendText.Focus();
        }

        private void BtnNetflix_MouseEnter(object sender, MouseEventArgs e)
        {
            BtnNetflix.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
        }

        private void BtnNetflix_MouseLeave(object sender, MouseEventArgs e)
        {
            BtnNetflix.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
        }

        private void StopSeries_MouseEnter(object sender, MouseEventArgs e)
        {
            StopSeries.Foreground = Brushes.White;
        }

        private void StopSeries_MouseLeave(object sender, MouseEventArgs e)
        {
            StopSeries.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#737e87"));
        }

        private void SkippEpisode_MouseEnter(object sender, MouseEventArgs e)
        {
            SkippEpisode.Foreground = Brushes.White;
        }

        private void SkippEpisode_MouseLeave(object sender, MouseEventArgs e)
        {
            SkippEpisode.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#737e87"));
        }

        private void StopSeries_Click(object sender, RoutedEventArgs e)
        {
            if (!OnlyMe)
                MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, MqttConnection.NickName + "_SERIEPLAYER_EXIT");
            else
            {
                stopSeries("You ");
            }
        }

        private void stopSeries(string nickname)
        {
            MiniControllPannelMultipleAnimation(false);
            BtnSearch.IsEnabled = true;
            BtnNetflix.IsEnabled = true;
            CloseLinks.Click -= SeriesMiniBanner;
            CloseLinks.Click += CloseLinks_Click;
            IconCloseLinks.Kind = PackIconKind.WindowClose;
            Storyboard SeriesMiniHide = (Storyboard)TryFindResource("SeriesMiniHide");
            SeriesMiniHide.Begin();
            MessageAllertCardBox(nickname + "exit from viewer", PackIconKind.WindowClose, "#FFFFFF", "#BF360C");
            EndTackTimer.Start();
        }

        private void SkippEpisode_Click(object sender, RoutedEventArgs e)
        {
            if (OnlyMe)
            {
                SkipperEpisode("You");
            }
            else
            {
                MqttConnection.MqttPublish(MqttConnection.UserName + "/feeds/" + MqttConnection.ChatRoomNameKey, MqttConnection.NickName + "_SERIEPLAYER_SKIPP");
            }
        }

        private void SkipperEpisode(string nickname)
        {
            if (nickname == MqttConnection.NickName)
            {
                string seasonEp = SeasonEpisondeTxt.Text;
                var splitMySeaEp = seasonEp.Split(' ');
                int currentSeason = int.Parse(splitMySeaEp[1]);
                int currentEpisode = int.Parse(splitMySeaEp[3]);
                //MessageBox.Show(currentSeason.ToString() + " -- " + currentEpisode.ToString());


                Button LinkPlayBtn = PlayEpisodeBtn(currentSeason, currentEpisode + 1);
                if (LinkPlayBtn != null)
                {
                    MessageAllertCardBox("You moved on to the next episode", PackIconKind.SkipForward, "#FFFFFF", "#BF360C");
                    ButtonAutomationPeer peer = new ButtonAutomationPeer(LinkPlayBtn);
                    IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                    invokeProv.Invoke();
                }
                else
                {
                    LinkPlayBtn = PlayEpisodeBtn(currentSeason + 1, 1);
                    if (LinkPlayBtn != null)
                    {
                        MessageAllertCardBox("You moved on to the next episode", PackIconKind.SkipForward, "#FFFFFF", "#BF360C");
                        ButtonAutomationPeer peer = new ButtonAutomationPeer(LinkPlayBtn);
                        IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                        invokeProv.Invoke();
                    }
                    else
                    {
                        ScrollLinkList.ScrollToTop();
                        ButtonAutomationPeer peer = new ButtonAutomationPeer(CloseLinks);
                        IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                        invokeProv.Invoke();
                        MessageAllertCardBox("You finished the series!", PackIconKind.VideoOff, "#FFFFFF", "#212121");
                    }
                }
            }
            else
            {
                MessageAllertCardBox(nickname + " moved on to the next episode", PackIconKind.SkipForward, "#FFFFFF", "#BF360C");
            }
        }

        private Button PlayEpisodeBtn(int currentSeason, int nextEpisode)
        {
            IEnumerable<Grid> SeasonContainer = LinkStream.Children.OfType<Grid>();
            foreach (var Season in SeasonContainer)
            {
                if (Season.Name == "SeasonContainer_" + currentSeason)
                {
                    IEnumerable<Grid> EpisodeContainer = Season.Children.OfType<Grid>();
                    foreach (var Episode in EpisodeContainer)
                    {
                        IEnumerable<Card> EpisodeCard = Episode.Children.OfType<Card>();
                        foreach (var EpisodeBtnCard in EpisodeCard)
                        {
                            Grid InnerGrid = EpisodeBtnCard.Content as Grid;
                            IEnumerable<Button> ButtonPlay = InnerGrid.Children.OfType<Button>();
                            foreach (var LinkPlayBtn in ButtonPlay)
                            {
                                if (LinkPlayBtn.Name == "PlayBtn_" + currentSeason + "_" + nextEpisode)
                                {
                                    return LinkPlayBtn;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try { ffmpeg_process.Kill(); } catch (Exception) { }
        }
    }
}