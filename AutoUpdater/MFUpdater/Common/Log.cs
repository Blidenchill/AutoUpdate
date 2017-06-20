using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace MFUpdater
{
    [Serializable]
    public class Log : MarshalByRefObject, IDisposable
    {
        protected static string logfilename = LocalFilesOperation.PathCombine(Path.GetDirectoryName(Assembly.GetEntryAssembly().GetModules()[0].FullyQualifiedName), "update.log");

        public static string LogFileName
        {
            get { return logfilename; }
            set { logfilename = value; }
        }

        protected static StreamWriter writer;

        public static void Init(string path) { }

        public static void Open()
        {
            if (logfilename != String.Empty)
            {
                if (writer != null)
                {
                    return;
                }
                writer = new StreamWriter(logfilename, true, System.Text.Encoding.Default);
                writer.BaseStream.Seek(0, SeekOrigin.End);
                writer.AutoFlush = true;
            }
        }

        protected static string GetLogInfoByType(LogType t)
        {
            string[] msgs = { "Warn:", "Error:", "Info:", "Exception", "DataBase" };
            int nIndex = (int)t;
            return msgs[nIndex];
        }

        protected Log()
        {
            if (logfilename != String.Empty)
            {
                if (writer != null)
                {
                    return;
                }

                writer = new StreamWriter(logfilename, true, System.Text.Encoding.Default);
                writer.BaseStream.Seek(0, SeekOrigin.End);
                writer.AutoFlush = true;
            }
        }

        public void Dispose()
        {
            if (writer == null)
            {
                return;
            }
            writer.Flush();
            writer.Close();
            writer = null;
        }

        public static void Flush()
        {
            if (writer == null)
            {
                return;
            }
            lock (writer)
            {
                writer.Flush();
            }
        }

        public static void Close()
        {
            if (writer == null)
            {
                return;
            }
            Flush();
            writer.Close();
            writer = null;
        }

        protected static void WriteMsg(string info, string msg)
        {
            Open();
            //do real write
            if (null != writer)
            {
                lock (writer)
                {
                    writer.WriteLine(DateTime.Now + "  " + info + msg);
                }
            }
            Close();
        }

        public static void Write(LogType lt, string msg)
        {
            string info = GetLogInfoByType(lt);
            WriteMsg(info, msg);
        }

        public static void Write(string msg)
        {
            string t = GetLogInfoByType(LogType.LmtInfo);
            WriteMsg(t, msg);
        }

        public static void Write(string msg, Exception exception)
        {
            string t = GetLogInfoByType(LogType.LmtInfo);
            WriteMsg(t, msg);
            LogException(exception);
        }

        public static void Write(Exception exception)
        {
            LogException(exception);
        }

        public static void LogException(Exception exception)
        {
            Write(LogType.LmtException, "异常类型：" + exception.GetType().AssemblyQualifiedName);
            Write(LogType.LmtException, "异常信息：" + exception.Message);
            Write(LogType.LmtException, "导致错误的应用程序或对象的名称：" + exception.Source);
            Write(LogType.LmtException, "引发当前异常的方法：" + exception.TargetSite.Name);
            Write(LogType.LmtException, "当前异常发生时调用堆栈上的帧:\r\n" + exception.StackTrace);
            Write(LogType.LmtException, "异常相关帮助文件：" + exception.HelpLink);
            if (exception.InnerException != null)
            {
                Write(LogType.LmtException, "内部异常");
                LogException(exception.InnerException);
            }
        }

        public static void LogProcessStart(string process)
        {
            string logStartProcess = "";
            for (int i = 1; i <= 10; i++)
            {
                logStartProcess += "======";
            }
            logStartProcess += "\r\n\t" + DateTime.Now;
            logStartProcess += ":\tStart process" + process + "\r\n";
            for (int i = 1; i <= 10; i++)
            {
                logStartProcess += ">>>>>>";
            }
            Write(logStartProcess);
        }

        public static void LogProcessEnd(string process)
        {
            string logEndProcess = "";
            for (int i = 1; i <= 10; i++)
            {
                logEndProcess += "<<<<<<";
            }
            logEndProcess += "\r\n\t" + DateTime.Now;
            logEndProcess += ":\tEnd process" + process + "\r\n";
            for (int i = 1; i <= 10; i++)
            {
                logEndProcess += "======";
            }
            Write(logEndProcess);
        }
    }

    public enum LogType
    {
        LmtWarn = 0,
        LmtError = 1,
        LmtInfo = 2,
        LmtException = 3,
        LmtDb = 4
    } ;
}
