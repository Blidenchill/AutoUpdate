using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MFUpdater
{
    public class LocalFilesOperation
    {
        public static readonly string TimeFoldFormat = "yyyyMMdd";
        /// <summary>
        /// 创建新版本的临时文件夹
        /// </summary>
        /// <param name="path"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string CreateNewVersionFold(string path, DateTime time)
        {
            
            if (File.Exists(path))
            {
                path = GetParentDirectory(path);
            }
            string fold = Path.Combine(path, time.ToString(TimeFoldFormat));
            try
            {
                Directory.CreateDirectory(fold);
            }
            catch { }
            return fold;
    
            
        }
        /// <summary>
        /// 获取父目录
        /// </summary>
        /// <param name="fileOrfolder"></param>
        /// <returns></returns>
        public static string GetParentDirectory(string fileOrfolder)
        {
            DirectoryInfo info = Directory.GetParent(fileOrfolder);
            if (info == null)
            {
                return string.Empty;
            }
            return info.FullName;
        }

        /// <summary>
        /// 去掉path2中以路径分隔符开头时返回path2的问题
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <returns></returns>
        public static string PathCombine(string path1, string path2)
        {
            if (path2.StartsWith(Path.DirectorySeparatorChar.ToString()))
            {
                path2 = path2.Substring(1);
            }
            return Path.Combine(path1, path2);
        }
        /// <summary>
        /// 删除文件夹及其下面的所有文件
        /// </summary>
        /// <param name="fold"></param>
        public static void DelFold(string fold)
        {
            try
            {
                if (Directory.Exists(fold))
                    Directory.Delete(fold, true);
            }
            catch { }

        }
    }

}
