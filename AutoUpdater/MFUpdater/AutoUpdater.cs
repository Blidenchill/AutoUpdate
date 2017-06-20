using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;
using Ionic.Zip;


namespace MFUpdater
{
    public class AutoUpdater
    {
        #region "构造函数"
        private static AutoUpdater instance;
        private static object lockerInstance = new object();
        public static AutoUpdater Instance
        {
            get
            {
                if (instance == null)
                {
                    lock(lockerInstance)
                    {
                        if(instance == null)
                            instance = new AutoUpdater();
                    }
                }
                   
                return instance;
            }
        }
        private AutoUpdater()
        {
            dataAccesser = new HttpDataAccess();
            //获取主程序信息
            RegistryKey key = Registry.LocalMachine;
            RegistryKey temp;
            if (Environment.Is64BitOperatingSystem)
                temp = key.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Mofanghr");
            else
                temp = key.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Mofanghr");
            string tempName = ConfigSetting.GetConfigValue("UpdateApplicationEntryAssembly");
            Console.WriteLine(tempName);
            mainProcessInfo.UpdateApplicationEntryAssembly = temp.GetValue(tempName).ToString();
            mainProcessInfo.UpdateApplicationStartupFolder = LocalFilesOperation.GetParentDirectory(mainProcessInfo.UpdateApplicationEntryAssembly);
            string exeName = Path.GetFileName(mainProcessInfo.UpdateApplicationEntryAssembly);
            mainProcessInfo.MainProcessName = exeName.Remove(exeName.LastIndexOf(".exe"));
             
        }
        #endregion

        #region "变量"
        private IDataAccesserLayer dataAccesser;
        public VersionInfo versionInfo;
        public MainProcessInfo mainProcessInfo = new MainProcessInfo();
        public HttpUpdateInfo updateInfo = new HttpUpdateInfo();
        /// <summary>
        /// 临时文件夹
        /// </summary>
        private string tempFold = "";
        /// <summary>
        /// 是否取消更新
        /// </summary>
        private bool isCancel = false;
        private Object lockObj = new object();

        /// <summary>
        /// 备份文件夹，备份旧文件
        /// </summary>
        private string backupFold = string.Empty;
        public bool IsCancel
        {
            get { lock (lockObj)return isCancel; }
            set { lock (lockObj) isCancel = value; }
        }
        #endregion

        #region "事件&委托"

        public EventHandler<AutoUpdateEventArgs> AutoUpdateEvent;

        public EventHandler<RunTimeStateArgs> RunTimeInfoEvent;
        #endregion

        #region "公共方法"
        /// <summary>
        /// 客户端判断版本号
        /// </summary>
        /// <returns>true为需要更新，false为不需要更新</returns>
        public bool VersionCompare()
        {
            try
            {
                VersionInfo info = AutoUpdater.Instance.GetUpdateInfo();
                var ass = Assembly.LoadFrom(AutoUpdater.Instance.mainProcessInfo.UpdateApplicationEntryAssembly);
                var attributes = ass.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
                if (attributes.Length == 0)
                {
                    return false;
                }
                var version = (AssemblyFileVersionAttribute)attributes[0];
                if (version.Version == info.VersionName)
                    return false;
                else
                    return true;
            }
            catch { }
            return false;
           
        }
        /// <summary>
        /// 服务器判断版本号
        /// </summary>
        /// <returns>true为需要更新，false为不需要更新</returns>
        public bool ServerVersionCompare()
        {
            try
            {
                var ass = Assembly.LoadFrom(AutoUpdater.Instance.mainProcessInfo.UpdateApplicationEntryAssembly);
                var attributes = ass.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
                if (attributes.Length == 0)
                {
                    return false;
                }
                var version = (AssemblyFileVersionAttribute)attributes[0];
                string urlSend = HttpHelper.ServerDomain + "face/version";
                string temp = HttpHelper.HttpGet(urlSend);
                if (temp.StartsWith("连接失败"))
                    return false;
                updateInfo = SerializableOperation.JsonParse<HttpUpdateInfo>(temp);
                //填写更新信息岛versionInfo中
                this.versionInfo = new VersionInfo();
                this.versionInfo.TotalFileSize = 10;
                this.versionInfo.VersionName = updateInfo.versionUrl;
                this.versionInfo.Description = string.Format( "您当前版本为v={0} 已过期，您需要更新到最新版本v={1}。", version.Version,updateInfo.versionUrl);
                this.versionInfo.UpdateTime = DateTime.Now;
                this.versionInfo.UpdateFileList.Add(new VersionFileInfo() { FileName = HttpHelper.GetFileNameFromUrl(updateInfo.updateUrl), FileSize = 10, DownloadUrl = updateInfo.updateUrl });
                //讲versionInfo 传给HttpDataAccess中
                dataAccesser.VersionInfos = this.versionInfo;
                return updateInfo.upgrade;
            }
            catch { }
            return false;
           
        }
        /// <summary>
        /// 获取更新信息
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public VersionInfo GetUpdateInfo()
        {

            //var s = new Service1SoapClient();
            //s.Endpoint.ListenUri = new Uri(GetUrl());
            //Type 
            //VersionInfo info = (VersionInfo)SerializableObject.DeserializeToObject(temp);
            VersionInfo temp = dataAccesser.GetUpdateInfo();
            this.versionInfo = temp;
            //VersionFileInfo info = 
            //return info;
            return temp;
        }
        public void StartUpdate()
        {
            if (!this.Update(this.versionInfo))
            {
                RunTimeInfoEvent(this, new RunTimeStateArgs("更新失败", RuntimeStateEnum.Error));
                return;
            }
            this.Install(this.versionInfo);
            RunTimeInfoEvent(this, new RunTimeStateArgs("更新完成！", RuntimeStateEnum.Finish));
        }

        public void StartZipUpdate()
        {
            if(!this.DownLoadZip(this.versionInfo))
            {
                RunTimeInfoEvent(this, new RunTimeStateArgs("更新失败", RuntimeStateEnum.Error));
                return;
            }
            this.Install(this.versionInfo);
            RunTimeInfoEvent(this, new RunTimeStateArgs("更新完成！", RuntimeStateEnum.Finish));
        }
        #endregion

        #region "内部方法"

        private bool DownLoadZip(VersionInfo info)
        {
            AutoUpdateEvent(this, new AutoUpdateEventArgs("准备更新" +  "...", (long)(info.TotalFileSize * 1.1), 1, ProgressBarStateEnum.Continue));

            tempFold = LocalFilesOperation.CreateNewVersionFold(Path.GetTempPath(), DateTime.Now);
            string tempFilename = Path.Combine(tempFold, "update.zip");

            HttpHelper.DonwloadSizeAction += DownloasSizeCallback;

            HttpHelper.Download(info.HttpGetInfo.updateUrl, tempFilename);
            //AutoUpdateEvent(this, new AutoUpdateEventArgs("解压缩中...", (long)(info.TotalFileSize * 1.25), (long)(info.TotalFileSize * 1.25), ProgressBarStateEnum.Continue));
            if (!ZipFileOperation.UnZipFile(tempFilename, tempFold))
            {
                AutoUpdateEvent(this, new AutoUpdateEventArgs(string.Format("文件下载异常，更新失败"), 0, 0, ProgressBarStateEnum.UnEnable));
                return false;
            }
            AutoUpdateEvent(this, new AutoUpdateEventArgs("文件下载完成", 0, 0, ProgressBarStateEnum.UnEnable));
            return true;
        }
        /// <summary>
        /// 讲更新文件下载到本地临时文件夹（未使用）
        /// </summary>
        /// <param name="info"></param>
        private bool Update(VersionInfo info)
        {
            if (info.UpdateFileList.Count == 0)
            {
                return false;
            }
            string path = Path.GetTempPath();
            long downSize = 0;
            long totalSize = info.TotalFileSize;
            tempFold = LocalFilesOperation.CreateNewVersionFold(path, info.UpdateTime);
            AutoUpdateEvent(this, new AutoUpdateEventArgs("开始更新文件", 0, 0, ProgressBarStateEnum.Continue));
            //更新压缩包单文件代码如下
            if(info.UpdateFileList.Count == 1 && info.UpdateFileList[0].FileName.EndsWith(".zip"))
            {
                VersionFileInfo c = info.UpdateFileList[0];
                string filename = Path.GetFileName(c.FileName);
                downSize += c.FileSize;
                //AutoUpdateEvent(this, new AutoUpdateEventArgs("下载文件" + filename+"...", totalSize, downSize, ProgressBarStateEnum.Continue));
                string tempFilename = Path.Combine(tempFold, filename);
                AutoUpdateEvent(this, new AutoUpdateEventArgs("解压缩中...", 0, 0, ProgressBarStateEnum.Marquee));
                if(!dataAccesser.SaveUpdateFile(tempFold, c))
                {
                    AutoUpdateEvent(this, new AutoUpdateEventArgs(string.Format("文件{0}更新失败", filename), 0, 0, ProgressBarStateEnum.UnEnable));
                    return false;
                }
                AutoUpdateEvent(this, new AutoUpdateEventArgs("文件" + filename + "更新完成", 0, 0, ProgressBarStateEnum.UnEnable));
                return true ;
            }


            foreach (VersionFileInfo c in info.UpdateFileList)
            {
                  if (IsCancel)
                {
                    return false;
                }
                if (c.FileOperateType != OperateType.Del)
                {
                    string filename = Path.GetFileName(c.FileName);
                    downSize += c.FileSize;
                    AutoUpdateEvent(this, new AutoUpdateEventArgs("开始更新文件...", 0, 0, ProgressBarStateEnum.Continue));
                    // filename = Path.Combine(info.UpdateTime.ToString(PathAndFolder.TimeFoldFormat), filename);
                    //string Downfilename = Path.Combine(info.UpdateTime.ToString(LocalFilesOperation.TimeFoldFormat), filename);
                    //GetFile(tempFold, Downfilename, c.FileSize);
                    string tempFilename = Path.Combine(tempFold, filename);
                    if (!dataAccesser.SaveUpdateFile(tempFold, c))
                    {
                        AutoUpdateEvent(this, new AutoUpdateEventArgs(string.Format("文件{0}更新失败", filename), 0, 0, ProgressBarStateEnum.UnEnable));
                        return false;
                    }
                    AutoUpdateEvent(this, new AutoUpdateEventArgs("文件" + filename + "更新完成", 0, 0, ProgressBarStateEnum.UnEnable));
                }
            }
            return true;
        }

        /// <summary>
        /// 本地更新文件覆盖安装目录
        /// </summary>
        /// <param name="info"></param>
        private void Install(VersionInfo info)
        {
            AutoUpdateEvent(this, new AutoUpdateEventArgs("开始安装文件", (long)(info.TotalFileSize * 1.1), info.TotalFileSize, ProgressBarStateEnum.Continue));
            backupFold = LocalFilesOperation.CreateNewVersionFold(this.mainProcessInfo.UpdateApplicationStartupFolder, info.UpdateTime);
            long totalSize = info.TotalFileSize;
            long downSize = 0;
            double cupple = info.TotalFileSize * 0.1 / info.UpdateFileList.Count;
            int countIndex = 1;
            try
            {
                foreach (VersionFileInfo c in info.UpdateFileList)
                {
                    if (IsCancel)
                    {
                        return;
                    }
                    string filename = Path.GetFileName(c.FileName);
                    downSize += c.FileSize;
                    AutoUpdateEvent(this, new AutoUpdateEventArgs("正在安装文件" + filename +"...", (long)(info.TotalFileSize * 1.1), info.TotalFileSize + (long)cupple * countIndex, ProgressBarStateEnum.Continue));
                    System.Threading.Thread.Sleep(100);
                    countIndex++;
                    InstallFile(filename, c, tempFold);
                }
                AutoUpdateEvent(this, new AutoUpdateEventArgs("安装完成", 0, 0, ProgressBarStateEnum.UnEnable));
            }
            catch (Exception e)
            {
                Log.LogException(e);
                AutoUpdateEvent(this, new AutoUpdateEventArgs("安装失败!", 0, 0, ProgressBarStateEnum.UnEnable));
                RollBack();
            }
            LocalFilesOperation.DelFold(tempFold);
            if (backupFold != null)
            {
                LocalFilesOperation.DelFold(backupFold);
            }
        }
        private void InstallFile(string localFileName, VersionFileInfo fileInfo, string tempFold)
        {
            string path = this.mainProcessInfo.UpdateApplicationStartupFolder;
            string newFileName;
            if(fileInfo.RelativePath != null)
            {
                newFileName = LocalFilesOperation.PathCombine(path, fileInfo.RelativePath);
            }
            else
            {
                newFileName = LocalFilesOperation.PathCombine(path, localFileName);
            }

            if (File.Exists(newFileName))
            {
                BackUpFile(newFileName);
                File.Copy(newFileName, Path.Combine(backupFold, localFileName), true);
            }
            localFileName = Path.Combine(tempFold, localFileName);
            switch (fileInfo.FileOperateType)
            {
                case OperateType.Add:
                    CreateFold(newFileName);
                    File.Copy(localFileName, newFileName, true);
                    break;
                case OperateType.Update:
                    CreateFold(newFileName);
                    File.Copy(localFileName, newFileName, true);
                    break;
                case OperateType.Del:
                    if (File.Exists(newFileName))
                    {
                        File.Delete(newFileName);
                    }
                    break;
                case OperateType.Run:
                    File.Copy(localFileName, newFileName, true);
                    string dir = path;
                    RunFile(dir, fileInfo.FileName);
                    break;
            }
        }

        private void RollBack()
        {
            try
            {
                using( StreamReader reader = new StreamReader(Path.Combine(backupFold, "backUpFile.txt"), System.Text.Encoding.Default))
                {
                    string line = reader.ReadLine();
                    while (!string.IsNullOrEmpty(line))
                    {
                        string localFileName = Path.GetFileName(line);
                        File.Copy(Path.Combine(backupFold, localFileName), line, true);
                    }
                }
            }
            catch { }
            
        }

        private static void CreateFold(string fullFileName)
        {
            if (!File.Exists(fullFileName))
            {
                string directoryname = LocalFilesOperation.GetParentDirectory(fullFileName);
                if (!Directory.Exists(directoryname))
                {
                    Directory.CreateDirectory(directoryname);
                }
            }
        }

        private static void DelFold(string fold)
        {
            Directory.Delete(fold, true);
        }

        private void BackUpFile(string backUpFile)
        {
            StreamWriter writer = new StreamWriter(Path.Combine(backupFold, "backUpFile.txt"), true, Encoding.Default);
            writer.WriteLine(backUpFile);
            writer.Close();
        }

        private void RunFile(string dir, string fileName)
        {
            string info = "运行程序" + fileName;
            try
            {
                if (File.Exists(Path.Combine(dir, fileName)))
                {
                    //Process myProcess = new Process();
                    //ProcessStartInfo psi = new ProcessStartInfo();
                    //psi.FileName = fileName;
                    //psi.WorkingDirectory = @"D:\WebSite\";
                    //psi.UseShellExecute = false;
                    //psi.RedirectStandardError = true;
                    //psi.CreateNoWindow = true;
                    //psi.RedirectStandardOutput = true;
                    //psi.WindowStyle = ProcessWindowStyle.Hidden;
                    //myProcess.StartInfo = psi;
                    //myProcess.Start();
                   Process pro =  Process.Start(Path.Combine(dir, fileName));
                   pro.WaitForExit();

                    //string error = myProcess.StandardError.ReadToEnd();
                    //string output = myProcess.StandardOutput.ReadToEnd();
                    //myProcess.WaitForExit();
                    //myProcess.Close();
                    //if (error != string.Empty)
                    //{
                    //    Log.Write("StandardError：" + error);
                    //}
                    //if (output != string.Empty)
                    //{
                    //    Log.Write("StandardOutput：" + output);
                    //}
                    //Log.LogProcessEnd(info);
                }
            }
            catch (Exception ex)
            {
                Log.Write(info + "出错");
                Log.LogException(ex);
                throw ex;
            }
        }


        #endregion

        #region "回调函数"
        private void DownloasSizeCallback(int size)
        {
            if(this.versionInfo.TotalFileSize >= size)
                AutoUpdateEvent(this, new AutoUpdateEventArgs("下载文件" + "...", this.versionInfo.TotalFileSize, size, ProgressBarStateEnum.Continue));
        }
        #endregion
    }
        
}
