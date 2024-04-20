/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: ProcessEx.cs
 *  创建时间: 2024年4月19日 23:22
 *  创建开发: ScifiBrain
 *  文件介绍: Process扩展类
 *  --------------------------------------
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace CSUL.Models
{
    /// <summary>
    /// Process扩展类
    /// </summary>
    internal static class ProcessEx
    {
        /// <summary>
        /// 强制结束进程
        /// </summary>
        public static bool TaskKills(this IEnumerable<Process> processes)
        {
            try
            {
                foreach (Process process in processes)
                {
                    if (process.HasExited) continue;
                    process.Kill();
                }
                return true;
            }
            catch (Win32Exception) { }
            catch (UnauthorizedAccessException) { }
            catch { throw; }

            StringBuilder builder = new();
            builder.Append("/C ");
            foreach (Process process in processes)
            {
                builder.Append($"taskkill /F /PID {process.Id} & ");
            }

            ProcessStartInfo startInfo = new()
            {   //管理员模式启动cmd 强制终止进程
                FileName = "cmd.exe",
                Verb = "runas",
                Arguments = builder.ToString(),
            };
            try
            {
                Process.Start(startInfo)?.WaitForExit(1000);
                return true;
            }
            catch (Win32Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 强制结束进程
        /// </summary>
        public static bool TaskKill(this Process process)
        {
            if (process.HasExited) return true;
            try
            {
                process.Kill();
                return true;
            }
            catch (Win32Exception) { }
            catch (UnauthorizedAccessException) { }
            catch { throw; }

            ProcessStartInfo startInfo = new()
            {   //管理员模式启动cmd 强制终止进程
                FileName = "cmd.exe",
                Verb = "runas",
                Arguments = $"/C taskkill /F /PID {process.Id} & ",
            };
            try
            {
                Process.Start(startInfo)?.WaitForExit(1000);
                return true;
            }
            catch (Win32Exception)
            {
                return false;
            }
        }
    }
}
