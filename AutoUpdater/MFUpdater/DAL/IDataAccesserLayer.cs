using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFUpdater
{
    interface IDataAccesserLayer
    {
        VersionInfo VersionInfos { get; set; }
        /// <summary>
        /// 获取更新所需的版本信息
        /// </summary>
        /// <returns></returns>
        VersionInfo GetUpdateInfo();
        /// <summary>
        /// 将要更新的文件保存在临时文件夹中
        /// </summary>
        /// <param name="tempFolder"></param>
        /// <param name="fileInfo"></param>
        bool SaveUpdateFile(string tempFolder, VersionFileInfo fileInfo);
    }
}
  