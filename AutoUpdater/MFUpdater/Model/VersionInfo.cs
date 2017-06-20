using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFUpdater
{
 
    [Serializable]
    public class VersionInfo
    {
        public VersionInfo()
        {
        }
        /// <summary>
        /// 版本更新信息描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 版本信息
        /// </summary>
        public string VersionName { get; set; }

        public bool IsForceUpdate { get; set; }
        public bool IsForceDownLoadSetupPackage { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
        private List<VersionFileInfo> updateFileList = new List<VersionFileInfo>();
        /// <summary>
        /// 需要更新的文件信息
        /// </summary>
        public List<VersionFileInfo> UpdateFileList
        {
            get
            {
                return updateFileList;
            }
            set
            {
                updateFileList = value;
            }
        }

        private long totalFileSize;
        public long TotalFileSize { get { return totalFileSize; } set { totalFileSize = value; } }


        private string lowestVersion = string.Empty;
        public string LowestVersion
        {
            get { return lowestVersion; }
            set
            {
                lowestVersion = value;
            }
        }


        private HttpUpdateInfo httpGetInfo = new HttpUpdateInfo();
        public HttpUpdateInfo HttpGetInfo
        {
            get { return httpGetInfo; }
            set { httpGetInfo = value;}
        }


    }
    [Serializable]
    public class VersionFileInfo
    {
        private string fileName;
        /// <summary>
        /// 文件名称，不含路径
        /// </summary>
        public string FileName { get { return fileName; } set { fileName = value; } }

        private string description;
        /// <summary>
        /// 文件描述
        /// </summary>
        public string Description { get { return description; } set { description = value; } }

        private string relativePath;
        /// <summary>
        /// 相对于要升级的主程序的路径
        /// </summary>
        public string RelativePath { get { return relativePath; } set { relativePath = value; } }

        private string realPath;
        /// <summary>
        /// 文件的实际路径
        /// </summary>
        public string RealPath { get { return realPath; } set { realPath = value; } }

        private OperateType fileOperateType;
        /// <summary>
        /// 升级方式
        /// </summary>
        public OperateType FileOperateType { get { return fileOperateType; } set { fileOperateType = value; } }

        private long fileSize;
        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileSize { get { return fileSize; } set { fileSize = value; } }
        /// <summary>
        /// 文件下载的url路径
        /// </summary>
        public string DownloadUrl { get; set; }
    }
    [Serializable]
    public enum OperateType
    {
        /// <summary>
        /// 新添加的文件
        /// </summary>
        Add,
        /// <summary>
        /// 需要替换的文件
        /// </summary>
        Update,
        /// <summary>
        /// 需要删除的文件
        /// </summary>
        Del,
        /// <summary>
        /// 需要运行的文件
        /// </summary>
        Run
    }
}
