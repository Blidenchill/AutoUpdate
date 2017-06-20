using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MFUpdater
{
    public class HttpDataAccess : IDataAccesserLayer
    {
        public VersionInfo VersionInfos { get; set; }
        /// <summary>
        /// 获取更新所需的版本信息
        /// </summary>
        /// <returns></returns>
        public VersionInfo GetUpdateInfo()
        {
            string tempVersionPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\mofanghr\updateVersion";
            string tempInfoPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\mofanghr\updateInfo";
            VersionInfo versionInfo = new VersionInfo();
            versionInfo.HttpGetInfo = Common.XmlClassData.ReadDataFromXml<HttpUpdateInfo>(tempInfoPath);
            //string urlSend = HttpHelper.ServerGetVersionJson;

            ////上线版本
            //string temp = HttpHelper.HttpGet(urlSend);
            //if (temp.StartsWith("连接失败"))
            //    return null;
            //versionInfo.HttpGetInfo = SerializableOperation.JsonParse<HttpUpdateInfo>(temp);
            //////每日构建
            ////string temp = string.Empty;
            ////Stream streamJson = HttpHelper.GetResponseStream(urlSend);
            ////if (streamJson != null)
            ////{
            ////    using (StreamReader sr = new StreamReader(streamJson))
            ////    {
            ////        temp = sr.ReadToEnd();
            ////        versionInfo.HttpGetInfo = SerializableOperation.JsonParse<HttpUpdateInfo>(temp);
            ////    }
            ////}



            //if (versionInfo.HttpGetInfo != null)
            //{
            //    //下载更新说明文件
            //    string path = Path.GetTempPath();
            //    string tempFile = Path.Combine(path, "version");
            //    HttpHelper.Download(versionInfo.HttpGetInfo.versionUrl, tempFile);


            //    if(File.Exists(tempFile))
            //        File.Delete(tempFile);
            //}
           
            if (File.Exists(tempVersionPath))
            {
                using (FileStream fs = new FileStream(tempVersionPath, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string tempLines = sr.ReadToEnd();
                        string[] tempList = tempLines.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        if (tempList.Length > 4)
                        {
                            versionInfo.VersionName = tempList[0];
                            versionInfo.IsForceUpdate = Convert.ToBoolean(tempList[1]);
                            versionInfo.IsForceDownLoadSetupPackage = Convert.ToBoolean(tempList[2]);
                            versionInfo.LowestVersion = tempList[3];
                            versionInfo.Description = tempList[4];
                            versionInfo.TotalFileSize = long.Parse(tempList[5]);
                            for (int i = 6; i < tempList.Length; i++)
                            {
                                string[] items = tempList[i].Split(new char[] { ',' });
                                if (items.Length == 4)
                                    versionInfo.UpdateFileList.Add(new VersionFileInfo() { FileName = items[0], RelativePath = items[1], FileOperateType = (OperateType)Enum.Parse(typeof(OperateType), items[2]), FileSize = long.Parse(items[3]) });
                            }
                        }
                    }
                }
            }

            return versionInfo;
        }
        /// <summary>
        /// 将要更新的文件保存在临时文件夹中
        /// </summary>
        /// <param name="tempFolder"></param>
        /// <param name="fileInfo"></param>
        public bool SaveUpdateFile(string tempFolder, VersionFileInfo fileInfo)
        {
            string tempFileName = LocalFilesOperation.PathCombine(tempFolder, fileInfo.FileName);
            HttpHelper.Download(fileInfo.DownloadUrl, tempFileName);
            //压缩包
            if(fileInfo.FileName.EndsWith(".zip"))
            {
                string err;
                if (!ZipFileOperation.UnZipFile(tempFileName, tempFolder, out err))
                    return false;
                
                //string versionFilePath = LocalFilesOperation.PathCombine(tempFolder, "VersionInfo.txt");
                //if (!File.Exists(versionFilePath))
                //    return false;
                //using (FileStream fs = new FileStream(versionFilePath, FileMode.Open, FileAccess.Read))
                //{
                //    using (StreamReader sr = new StreamReader(fs))
                //    {
                //        VersionInfos.TotalFileSize = long.Parse(sr.ReadLine());
                //        VersionInfos.UpdateFileList.Clear();
                //        List<string> infoList = new List<string>();
                //        while (!sr.EndOfStream)
                //        {
                //            infoList.Add(sr.ReadLine());
                //        }
                //        foreach (string item in infoList)
                //        {
                //            string[] paramList = item.Split(new char[] { ',' });
                //            VersionInfos.UpdateFileList.Add(new VersionFileInfo() { FileName = paramList[0], RelativePath = paramList[1], FileOperateType = (OperateType)Enum.Parse(typeof(OperateType), paramList[2]), FileSize = long.Parse(paramList[3]) });
                //        }
                //    }
                //}
            }
            else if(fileInfo.FileName.EndsWith(".exe"))
            {
                fileInfo.FileOperateType = OperateType.Run;
            }
            return true;
        }
    }
}
