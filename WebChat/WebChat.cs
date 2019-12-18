using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SuperWebSocket;

namespace Twitch
{
    public class WebChat
    {
        List<WebSocketSession> sessions = new List<WebSocketSession>();
        private bool master = true;

        public void PushMessage(string username, string message)
        {
            string buffer = $"{username},{message}";

            foreach (WebSocketSession ui in sessions)
                ui.Send(buffer);
        }

        private void WebThread()
        {
            using (HttpListener listen = new HttpListener())
            {
                var webSocket = new SuperWebSocket.WebSocketServer();
                webSocket.Setup(22199);
                webSocket.NewSessionConnected += Add;
                webSocket.Start();
            }

            master = true;
        }

        private void Add(WebSocketSession session)
        {
            sessions.Add(session);
        }

        public void Stop()
        {
            master = false;
        }

        public void Run()
        {
            if (!File.Exists("WebChat//ui.html"))
                throw new Exception("WebChatFiles not found");
            else
            {
                Task.Run(() => WebThread());
            }
        }
    }
}
