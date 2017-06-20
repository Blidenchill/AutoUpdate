using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MFUpdater
{
    class LocalDataTest : IDataAccesserLayer
    {
        public VersionInfo VersionInfos { get; set; }
        public VersionInfo GetUpdateInfo()
        {
            VersionInfo info = new VersionInfo();
            using(FileStream fs = new FileStream(@"D:\updateInfo.txt", FileMode.Open, FileAccess.Read))
            {
                using(StreamReader sr = new StreamReader(fs))
                {
                    info.VersionName = sr.ReadLine();
                    info.UpdateTime = DateTime.ParseExact(sr.ReadLine(), LocalFilesOperation.TimeFoldFormat, null);
                    info.TotalFileSize = 29 + 19 + 302;
                    info.Description = "更新内容：\r\n 1. test1.dll,修复了启动闪屏的bug。\r\n 2. 优化了界面显示。\r\n 3. 增加了音效功能。";

                    info.UpdateFileList.Add(new VersionFileInfo() { FileName = "Git-2.7.2-64-bit.exe", FileOperateType = OperateType.Add, RelativePath = "Git-2.7.2-64-bit.exe", FileSize = 29});
                    info.UpdateFileList.Add(new VersionFileInfo(){FileName = "TortoiseGit-2.0.0.0-64bit.msi", FileOperateType = OperateType.Add, RelativePath = "TortoiseGit-2.0.0.0-64bit.msi", FileSize = 19});
                    info.UpdateFileList.Add(new VersionFileInfo() { FileName = "VMware-workstation-full-11.1.2.61471.1437365244.exe", FileOperateType = OperateType.Add, RelativePath = "VMware-workstation-full-11.1.2.61471.1437365244.exe", FileSize = 302 });
                }
            }
            VersionInfos = info;
            return info;
        }
        public bool SaveUpdateFile(string tempFolder, VersionFileInfo fileInfo)
        {
            //string newPath = Path.Combine(tempFold, localFileName);
            byte[] temp = new byte[1024];
            List<byte> tempList = new List<byte>();
            //using (FileStream fs = new FileStream(@"E:\软件安装包\" + fileInfo.FileName, FileMode.Open, FileAccess.Read))
            //{
            //    using(BinaryReader br = new BinaryReader(fs))
            //    {
            //        int len = br.Read(temp, 0, temp.Length);
            //        while(len == 1024)
            //        {
            //            tempList.AddRange(temp);
            //            len = br.Read(temp, 0, temp.Length);
            //        }
            //        for(int i = 0; i < len; i++)
            //        {
            //            tempList.Add(temp[i]);
            //        }
            //    }
            //}
            //using(FileStream fs = new FileStream(tempFileName, FileMode.Create, FileAccess.Write))
            //{
            //    using(BinaryWriter bw = new BinaryWriter(fs))
            //    {
            //        bw.Write(tempList.ToArray());
            //    }
            //}
            string tempFileName = Path.Combine(tempFolder, fileInfo.FileName);
            File.Copy(@"E:\软件安装包\" + fileInfo.FileName, tempFileName, true);
            return true;
        }

    }
}
