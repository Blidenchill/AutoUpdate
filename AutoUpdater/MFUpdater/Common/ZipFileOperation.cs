using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shell32;
using Ionic.Zip;

namespace MFUpdater
{
    public class ZipFileOperation
    {
        /// <summary>
        /// 功能：解压zip格式的文件。
        /// </summary>
        /// <param name="zipFilePath">压缩文件路径</param>
        /// <param name="unZipDir">解压文件存放路径,为空时默认与压缩文件同一级目录下，跟压缩文件同名的文件夹</param>
        /// <param name="err">出错信息</param>
        /// <returns>解压是否成功</returns>
        public static bool UnZipFile(string zipFilePath, string unZipDir, out string err)
        {
            err = "";
            if (zipFilePath.Length == 0)
            {
                err = "压缩文件不能为空！";
                return false;
            }
            else if (!zipFilePath.EndsWith(".zip"))
            {
                err = "文件格式不正确！";
                return false;
            }
            else if (!System.IO.File.Exists(zipFilePath))
            {
                err = "压缩文件不存在！";
                return false;
            }
            //解压文件夹为空时默认与压缩文件同一级目录下，跟压缩文件同名的文件夹
            if (unZipDir.Length == 0)
                unZipDir = zipFilePath.Replace(System.IO.Path.GetFileName(zipFilePath), System.IO.Path.GetFileNameWithoutExtension(zipFilePath));
            if (!unZipDir.EndsWith("\\"))
                unZipDir += "\\";
            if (!System.IO.Directory.Exists(unZipDir))
                System.IO.Directory.CreateDirectory(unZipDir);
            try
            {
                Shell32.ShellClass sc = new Shell32.ShellClass();
                Shell32.Folder SrcFolder = sc.NameSpace(zipFilePath);
                Shell32.Folder DestFolder = sc.NameSpace(unZipDir);
                Shell32.FolderItems items = SrcFolder.Items();
                DestFolder.CopyHere(items, 20);
            }
            catch (Exception ex)
            {
                err = ex.Message;
                return false;
            }
            return true;
        }//解压结束

        public static bool UnZipFile(string zipFilePath, string unZipDir)
        {
            try
            {
                using (ZipFile zip = ZipFile.Read(zipFilePath))
                {
                    foreach (var z in zip)
                    {
                        z.Extract(unZipDir, ExtractExistingFileAction.OverwriteSilently);
                    }

                }
                return true;
            }
            catch
            {
                return false;
            }


        }
    }
}
