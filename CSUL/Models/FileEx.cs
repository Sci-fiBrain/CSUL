/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: FileEx.cs
 *  创建时间: 2024年4月17日 0:30
 *  创建开发: ScifiBain
 *  文件介绍: 文件扩展方法
 *  --------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSUL.Models
{
    internal static partial class FileEx
    {
        /// <summary>
        /// 文件是否正在被使用
        /// </summary>
        public static bool IsInUse(this FileInfo file) => IsInUse(file.FullName);

        /// <summary>
        /// 文件是否正在被使用
        /// </summary>
        /// <param name="path">文件路径</param>
        public static bool IsInUse(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException();
            try
            {
                using FileStream stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                return false;
            }
            catch (IOException)
            {
                return true;
            }
        }

        /// <summary>
        /// 获取正在使用某文件的进程对象
        /// </summary>
        public static async Task<Process[]> GetFileProcesses(this FileInfo file) => await GetFileProcesses(file.FullName);

        /// <summary>
        /// 获取正在使用某文件的进程对象
        /// </summary>
        public static async Task<Process[]> GetFileProcesses(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException();
            string handlePath = @"Dependences\handle.exe";
            if (!File.Exists(handlePath)) return Array.Empty<Process>();
            ProcessStartInfo startInfo = new()
            {
                FileName = handlePath,
                Arguments = $"\"{path}\"",
                Verb = "runas",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            };
            using Process handleProcess = new() { StartInfo = startInfo };
            handleProcess.Start();
            await handleProcess.WaitForExitAsync();
            HashSet<int> pids = new();
            while (handleProcess.StandardOutput.ReadLine() is string line)
            {
                Regex regex = HandleRegex();
                Match match = regex.Match(line);
                if (match.Success)
                {
                    string data = match.Groups["PID"].Value;
                    if (!int.TryParse(data, out int pid)) continue;
                    pids.Add(pid);
                }
            }
            List<Process> processes = new();
            foreach (int pid in pids)
            {
                try
                {
                    processes.Add(Process.GetProcessById(pid));
                }
                catch { }
            }
            return processes.ToArray();
        }

        //匹配handle.exe输出信息
        [GeneratedRegex("(?<ProcessName>\\S+)\\s+pid:\\s+(?<PID>\\d+)\\s+type:\\s+(?<Type>\\w+)\\s+(?<Path>.+)")]
        private static partial Regex HandleRegex();

        /// <summary>
        /// 释放某个文件
        /// </summary>
        /// <returns>该文件是否需要释放</returns>
        public static async Task<bool> Release(this FileInfo fileInfo) => await Release(fileInfo.FullName);

        /// <summary>
        /// 释放某个文件
        /// </summary>
        /// <returns>该文件是否需要释放</returns>
        public static async Task<bool> Release(string filePath)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException();
            Process[] processes = await GetFileProcesses(filePath);
            if(processes.Length == 0) return true;
            return processes.TaskKills();
        }
    }
}