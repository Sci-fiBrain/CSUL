/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: PlayerLogParser.cs
 *  创建时间: 2024年3月16日 0:21
 *  创建开发: ScifiBrain
 *  文件介绍: 玩家日志解析器
 *  --------------------------------------
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace CSUL.Models.Local.Game
{
    /// <summary>
    /// 玩家日志解析器
    /// </summary>
    internal partial class PlayerLogParser : IDisposable
    {
        private readonly StreamReader reader;
        private readonly CancellationTokenSource cancellation = new();
        private Task? readingTask;

        /// <summary>
        /// 实例化<see cref="PlayerLogParser"/>对象
        /// </summary>
        /// <param name="file">日志文件对象</param>
        public PlayerLogParser(FileInfo file)
        {
            if(file.Exists) file.Delete();
            file.Create().Dispose();
            Stream stream = new FileStream(file.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            reader = new StreamReader(stream, Encoding.UTF8);
        }

        ~PlayerLogParser() => Dispose();

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            cancellation.Cancel();
            readingTask?.Wait();
            reader.Dispose();
        }

        /// <summary>
        /// 启动日志监听
        /// </summary>
        public void StartListening()
        {
            readingTask = Task.Run(ListeningHandler, cancellation.Token);
        }

        private async Task ListeningHandler()
        {
            HashSet<string> logs = new();
            while (!cancellation.IsCancellationRequested)
            {
                if (reader.EndOfStream)
                {
                    if(logs.Count > 0)
                    {
                        StringBuilder builder = new();
                        builder.Append("以下消息由CSUL解析游戏日志自动生成，和CSUL本身无关").AppendLine();
                        builder.Append('-', 60).AppendLine();
                        builder.AppendJoin('\n', logs);
                        MessageBox.Show(builder.ToString(), "CSUL 游戏日志解析器", MessageBoxButton.OK, MessageBoxImage.Warning);
                        logs.Clear();
                    }
                    await Task.Delay(10);
                    continue;
                }
                if (await reader.ReadLineAsync() is string line && !string.IsNullOrWhiteSpace(line))
                {
                    if (MatchSuccess(AssemblyNameRegex, line, out Match match))
                    {
                        logs.Add($"有模组依赖[{match.Groups["name"].Value}]运行，但您并没有安装该模组");
                    }
                    else if (MatchSuccess(DependencyRegex, line, out match))
                    {
                        logs.Add($"模组[{match.Groups["name"].Value}]依赖[{match.Groups["dependency"].Value}]运行，但您并没有安装该模组");
                    }
                    else if (line.Contains("A platform service integration failed to initialize"))
                    {
                        logs.Add("Steam正版验证未通过");
                    }
                    else if (line.Contains("Out of memory"))
                    {
                        logs.Add("游戏内存溢出");
                    }
                }
            }
        }

        private static bool MatchSuccess(Func<Regex> getRegex, string input, out Match match)
        {
            match = getRegex().Match(input);
            return match.Success;
        }

        [GeneratedRegex("Could not load file or assembly '(?<name>[\\w]+),")]
        private static partial Regex AssemblyNameRegex();

        [GeneratedRegex("Could not load \\[(?<name>.*?)\\] because it has missing dependencies: (?<dependency>.*?)$")]
        private static partial Regex DependencyRegex();
    }
}
