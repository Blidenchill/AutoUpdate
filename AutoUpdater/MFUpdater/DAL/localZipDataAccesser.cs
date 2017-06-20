using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MFUpdater
{
    public class localZipDataAccesser : IDataAccesserLayer
    {
        public VersionInfo VersionInfos { get; set; }
        public VersionInfo GetUpdateInfo()
        {
            VersionInfo info = new VersionInfo();
            info.VersionName = "2.0.0.0";
            info.Description = "更新内容：\r\n 1. test1.dll,修复了启动闪屏的bug。\r\n 2. 优化了界面显示。\r\n 3. 增加了音效功能。";
            info.TotalFileSize = 29;
            info.UpdateFileList.Add(new VersionFileInfo() { FileName = "test.zip", FileOperateType = OperateType.Add,  FileSize = 29 });
            VersionInfos = info;
            return info;
        }

        public bool SaveUpdateFile(string tempFolder, VersionFileInfo fileInfo)
        {
            string tempFileName = LocalFilesOperation.PathCombine(tempFolder, fileInfo.FileName);
            File.Copy(@"E:\" + fileInfo.FileName, tempFileName, true);
            string err;
            if (!ZipFileOperation.UnZipFile(tempFileName, tempFolder, out err))
                return false;
            string versionFilePath = LocalFilesOperation.PathCombine(tempFolder, "VersionInfo.txt");
            if (!File.Exists(versionFilePath))
                return false;
            using(FileStream fs = new FileStream(versionFilePath, FileMode.Open, FileAccess.Read))
            {
                using(StreamReader sr = new StreamReader(fs))
                {
                    VersionInfos.TotalFileSize = long.Parse(sr.ReadLine());
                    VersionInfos.UpdateFileList.Clear();
                    List<string> infoList = new List<string>();
                    while(!sr.EndOfStream)
                    {
                        infoList.Add(sr.ReadLine());
                    }
                    foreach(string item in infoList)
                    {
                        string[] paramList = item.Split(new char[]{','});
                        VersionInfos.UpdateFileList.Add(new VersionFileInfo() { FileName = paramList[0], RelativePath = paramList[1], FileOperateType = (OperateType)Enum.Parse(typeof(OperateType), paramList[2]), FileSize = long.Parse(paramList[3]) });
                    }
                    

                }
            }
            return false;
        }
    }
}
