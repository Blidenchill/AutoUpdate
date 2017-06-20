using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFUpdater
{
    /// <summary>
    /// 从服务器获取更新下载信息
    /// </summary>
    public class HttpUpdateInfo
    {
        /// <summary>
        /// 是否强制更新
        /// </summary>
        public bool forcedUpgrade { get; set; }
        /// <summary>
        /// 安装包下载路径
        /// </summary>
        public string downloadUrl { get; set; }
        /// <summary>
        /// 最新版本号文件
        /// </summary>
        public string versionUrl { get; set; }
        /// <summary>
        /// 是否需要更新
        /// </summary>
        public bool upgrade { get; set; }
        /// <summary>
        /// 更新zip压缩包下载路径
        /// </summary>
        public string updateUrl { get; set; }
        /// <summary>
        /// 是否为最近三个版本（是：true，否：false）
        /// </summary>
        public bool latestThreeVersion { get; set; }

    }
}
