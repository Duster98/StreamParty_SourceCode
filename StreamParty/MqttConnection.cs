using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace FilmParty
{
    public static class MqttConnection
    {
        public static MqttClient client;
        public static string UserName;
        public static string AioKey;
        public static string NickName;
        public static string ChatRoomNameKey = "";
        public static string Key_EndToEnd = "";
        private static string PrimaryKey = "secret key";
        public static string MqttServer = "io.adafruit.com";
        public static int MqttPort = 8883;

        public static bool IsSolace = false;
        public static bool IsGroup = false;
        public static bool IamAdmin = true;

        public static async Task<bool> MqttTryToConnect()
        {
            try
            {
                client = await Task.Run(() => new MqttClient(MqttServer, MqttPort, true, new X509Certificate(), new X509Certificate(), MqttSslProtocols.TLSv1_2));
                string parNickname = await Task.Run(() => RandomString(15, true));
                await Task.Run(() => client.Connect(parNickname + "_" + NickName, UserName, AioKey, true, 1000));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static async Task<bool> MqttFastConnect()
        {
            try
            {
                string parNickname = await Task.Run(() => RandomString(15, true));
                await Task.Run(() => client.Connect(parNickname + "_" + NickName, UserName, AioKey, true, 1000));
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static async void MqttPublish(string Topic, string DataMessage)
        {
            DataMessage = StringCipher.Encrypt(DataMessage, Key_EndToEnd);
            await Task.Run(() => client.Publish(Topic, Encoding.UTF8.GetBytes(DataMessage)));
        }

        public static async void MqttUnsecurePublish(string Topic, string DataMessage)
        {
            await Task.Run(() => client.Publish(Topic, Encoding.UTF8.GetBytes(DataMessage)));
        }

        public static void MqttSubscribe(string Topic)
        {
            client.Subscribe(new string[] { Topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }

        public static void MqttUnsubscribe(string Topic)
        {
            client.Unsubscribe(new string[] { Topic });
        }

        public static void MqttDisconnect()
        {
            try
            {
                if (client.IsConnected)
                {
                    client.Disconnect();
                }
            }
            catch (Exception)
            {

            }
        }

        public static async Task<string> RandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = await Task.Run(() => Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65))));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }

        public static void setUser(string user)
        {
            UserName = user;
        }

        public static void setAioKey(string aiokey)
        {
            AioKey = aiokey;
        }

        public static void setNickname(string nickname)
        {
            NickName = nickname;
        }

        public static void setChatRoomNameKey(string RoomName)
        {
            ChatRoomNameKey = RoomName;
        }

        public static void setMqttServer(string Server)
        {
            MqttServer = Server;
        }

        public static void setMqttPort(string Port)
        {
            MqttPort = int.Parse(Port);
        }

        public static void setKeyEndToEnd(string key)
        {
            if (key == "")
            {
                Key_EndToEnd = PrimaryKey;
            }
            else
            {
                Key_EndToEnd = key;
            }
        }
    }
}
