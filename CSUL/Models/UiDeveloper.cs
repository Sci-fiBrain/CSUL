using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSUL.Models
{
    /// <summary>
    /// UI操作类
    /// </summary>
    public class UiDeveloper : IDisposable
    {
        #region ---私有字段---
        private const string uri = "ws://127.0.0.1:9444/devtools/page/0";
        private readonly ClientWebSocket socket = new();
        private readonly System.Threading.CancellationTokenSource cancellation = new();
        private readonly JsonSerializerOptions jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        private bool connected = false;
        private int _messageId = 0;
        #endregion

        #region ---公共属性---
        //
        #endregion

        #region ---构造函数---
        /// <summary>
        /// 实例化<see cref="UiDeveloper"/>对象
        /// </summary>
        public UiDeveloper() { }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            connected = false;
            receiveInfoTasks.Clear();
            cancellation.Cancel(true);
            socket.Abort();
            socket.Dispose();
            cancellation.Dispose();
        }
        #endregion

        #region ---消息接收方法---
        private readonly Dictionary<int, UiDeveloperReceiveInfoTask> receiveInfoTasks = new();

        /// <summary>
        /// 消息接收方法
        /// </summary>
        private async Task ReceiveMessage()
        {
            while (connected)
            {
                using MemoryStream stream = new();
                await Task.Delay(500);
                try
                {   //循环接收消息
                    while (connected && receiveInfoTasks.Count > 0)
                    {
                        byte[] buffer = new byte[1024];
                        WebSocketReceiveResult receive = await socket.ReceiveAsync(buffer, cancellation.Token);
                        stream.Write(buffer, 0, receive.Count);
                        if (receive.EndOfMessage) break;
                    }
                    UiDeveloperReceiveInfo receiveInfo = await JsonSerializer.DeserializeAsync<UiDeveloperReceiveInfo>(stream, jsonOptions, cancellation.Token);
                    if(receiveInfoTasks.TryGetValue(receiveInfo.Id, out UiDeveloperReceiveInfoTask? task))
                    {
                        task.Set(receiveInfo);
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception) { }
            }
        }

        /// <summary>
        /// 等待指定id的返回值
        /// </summary>
        private async Task<UiDeveloperReceiveInfo?> WaitReceiveMeg(int id, TimeSpan span)
        {
            UiDeveloperReceiveInfoTask task = new();
            receiveInfoTasks[id] = task;
            bool flag = await task.WaitAsync(span);
            receiveInfoTasks.Remove(id);
            if (flag) return task.ReceiveInfo;
            else return null;
        }
        #endregion

        #region ---公共方法---
        /// <summary>
        /// 连接服务器
        /// </summary>
        public async Task<bool> Connect()
        {
            if (connected) throw new Exception("服务器已连接");
            await socket.ConnectAsync(new Uri(uri), cancellation.Token).WaitAsync(TimeSpan.FromSeconds(5));
            connected = socket.State == WebSocketState.Open;
            _ = Task.Run(ReceiveMessage, cancellation.Token);
            await SendMessage(new UiDeveloperSendInfo("DOM.enable"));
            await SendMessage(new UiDeveloperSendInfo("CSS.enable"));
            return connected;
        }
        #endregion

        #region ---私有方法---
        private async Task<UiDeveloperReceiveInfo> SendMessage(UiDeveloperSendInfo sendInfo)
        {
            if (!connected) throw new Exception("服务器未连接");
            sendInfo.Id = _messageId++;
            using MemoryStream stream = new();
            await JsonSerializer.SerializeAsync(stream, sendInfo, jsonOptions);
            await socket.SendAsync(stream.ToArray(), WebSocketMessageType.Binary, true, cancellation.Token);
            UiDeveloperReceiveInfo receiveInfo = await WaitReceiveMeg(sendInfo.Id, TimeSpan.FromSeconds(5))
                ?? throw new TimeoutException();
            return receiveInfo;
        }
        #endregion
    }
}
