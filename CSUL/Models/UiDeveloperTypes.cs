using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

//UiDeveloper所用的类型
namespace CSUL.Models
{
    /// <summary>
    /// UiDeveloper发送消息
    /// </summary>
    public struct UiDeveloperSendInfo
    {
        public UiDeveloperSendInfo(string methodName, JsonElement? methodParams = null)
            => (Method, Params) = (methodName,  methodParams);

        /// <summary>
        /// 消息Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 执行方法名称
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// 方法参数
        /// </summary>
        public JsonElement? Params { get; set; }
    }

    /// <summary>
    /// UiDeveloper接收消息
    /// </summary>
    public struct UiDeveloperReceiveInfo
    {
        /// <summary>
        /// 消息Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 错误内容
        /// </summary>
        public UiDeveloperReceiveInfoError? Error { get; set; }

        /// <summary>
        /// 返回值
        /// </summary>
        public JsonElement? Result { get; set; }
    }

    /// <summary>
    /// UiDeveloper接收消息中的错误类型
    /// </summary>
    public struct UiDeveloperReceiveInfoError
    {
        /// <summary>
        /// 错误代码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 错误内容
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 更多数据
        /// </summary>
        public JsonElement? Data { get; set; }
    }

    /// <summary>
    /// UiDeveloper接收消息任务
    /// </summary>
    public class UiDeveloperReceiveInfoTask
    {
        /// <summary>
        /// 接收到的消息
        /// </summary>
        public UiDeveloperReceiveInfo? ReceiveInfo { get; private set; }

        /// <summary>
        /// 信号
        /// </summary>
        private readonly ManualResetEvent manual = new(false);

        /// <summary>
        /// 设定内容
        /// </summary>
        public void Set(UiDeveloperReceiveInfo info)
        {
            ReceiveInfo = info;
            manual.Set();
        }

        /// <summary>
        /// 等待消息接收
        /// </summary>
        /// <returns>是否接收到消息</returns>
        public async Task<bool> WaitAsync(TimeSpan timeout)
        {
            return await Task.Run(() => manual.WaitOne(timeout));
        }
    }
}
