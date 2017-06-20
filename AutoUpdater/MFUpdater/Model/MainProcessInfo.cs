using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFUpdater
{
    public class MainProcessInfo
    {
        /// <summary>
        /// 主进程名
        /// </summary>
        public string MainProcessName { get; set; }
        /// <summary>
        /// 更新软件的运行程序集名（含路径）
        /// </summary>
        public string UpdateApplicationEntryAssembly { get; set; }
        /// <summary>
        /// 更新软件的运行程序集父目录
        /// </summary>
        public string UpdateApplicationStartupFolder { get; set; }
        
    }
}
