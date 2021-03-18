using SolaceSystems.Solclient.Messaging;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;


namespace StreamParty
{

    public static class SolaceWebClient
    {

        private static ISession session = null;

        private static void Connect()
        {
            ContextFactoryProperties cfp = new ContextFactoryProperties();
            cfp.SolClientLogLevel = SolLogLevel.Warning;
            cfp.LogToConsoleError();
            ContextFactory.Instance.Init(cfp);

            SessionProperties sessionProps = new SessionProperties();
            sessionProps.Host = "tcp://mr4yqbkp31ewl.messaging.solace.cloud:20992";
            sessionProps.VPNName = "msgvpn-9xboqhaaj7p";
            sessionProps.UserName = "solace-cloud-client";
            sessionProps.Password = "41lt4btduge6r9695snciedj6o";
            sessionProps.ReconnectRetries = 3;

            IContext context = ContextFactory.Instance.CreateContext(new ContextProperties(), null);
            session = context.CreateSession(sessionProps, HandleMessage, HandleSession);

            try
            {
                ReturnCode returnCode = session.Connect();
                if (returnCode == ReturnCode.SOLCLIENT_OK)
                {
                    MyNickName = "Peppe";
                    session.Subscribe(ContextFactory.Instance.CreateTopic("test"), true);
                    MessageBox.Show("Yes");
                }
            }
            catch (OperationErrorException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        static string MyNickName = "";
        static string text = "";
        static string UserWriting = "";
        private static void HandleMessage(object source, MessageEventArgs args)
        {
            IMessage message = args.Message;
            string messageTxt = Encoding.ASCII.GetString(message.BinaryAttachment);
            MessageBox.Show(messageTxt);
            var splitData = messageTxt.Split('_');
            if (splitData[0] != MyNickName && splitData[1] == "writing")
            {
                UserWriting = splitData[0];
                //this.Dispatcher.Invoke(new nomedelegate(TextWritting));
            }
            else if (splitData[0] != MyNickName && splitData[1] == "stopWriting")
            {
                //this.Dispatcher.Invoke(new nomedelegate(TextStopWritting));
            }
            else if (splitData[1] != "stopWriting" && splitData[1] != "writing")
            {
                text = splitData[0] + ": " + splitData[1] + Environment.NewLine;
                //this.Dispatcher.Invoke(new nomedelegate(AppenedText));
            }
        }

        private static void HandleSession(object source, SessionEventArgs argsSession)
        {
            MessageBox.Show(argsSession.Info);
        }
    }
}
