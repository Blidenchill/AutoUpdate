using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFUpdater
{
    public class DownFileEventArgs : EventArgs
    {
        public DownFileEventArgs(string info)
        {
            this.info = info;
        }

        private readonly string info;

        public string Info
        {
            get { return info; }
        }
    }

    public class UpdateFileEventArgs : DownFileEventArgs
    {
        public UpdateFileEventArgs(string info, string updateFileName)
            : base(info)
        {
            this.updateFileName = updateFileName;
        }

        private readonly string updateFileName;

        public string UpdateFileName
        {
            get { return updateFileName; }
        }
    }

    public class DownFileChangedEventArgs : DownFileEventArgs
    {
        public DownFileChangedEventArgs(string info, long totleSize, long updateSize, string updateFileName)
            : base(info)
        {
            this.totleSize = totleSize;
            this.updateSize = updateSize;
            this.updateFileName = updateFileName;
            if (totleSize == 0)
                totleSize = 1;
        }

        private readonly long totleSize;
        private readonly long updateSize;
        private readonly string updateFileName;
    

        public long TotleSize
        {
            get { return totleSize; }
        }

        public long UpdateSize
        {
            get { return updateSize; }
        }

        public string UpdateFileName
        {
            get { return updateFileName; }
        }
    }

    public class AutoUpdateEventArgs : EventArgs
    {
        public long TotleSize { get; set; }
        public long UpdateSize { get; set; }
        public string Info { get; set; }
        public ProgressBarStateEnum ProgressBarStyle { get; set; }
        public AutoUpdateEventArgs(string info, long totleSize, long updateSize,  ProgressBarStateEnum progressbarStyle)
        {
            this.TotleSize = totleSize;
            this.UpdateSize = updateSize;
            this.Info = info;
            this.ProgressBarStyle = progressbarStyle;
        }
    }

    public class RunTimeStateArgs : EventArgs
    {
        public RuntimeStateEnum RuntimeState { get; set; }
        public string Info { get; set; }
        public RunTimeStateArgs(string info, RuntimeStateEnum runtimeState)
        {
            this.RuntimeState = runtimeState;
            this.Info = info;
        }
    }


    public enum ProgressBarStateEnum
    {
        /// <summary>
        /// 连续滚动一个块来指示进度
        /// </summary>
        Marquee,
        /// <summary>
        /// 连续的栏的大小来指示进度
        /// </summary>
        Continue,
        /// <summary>
        /// 不可用
        /// </summary>
        UnEnable,

    }

    public enum RuntimeStateEnum
    {
        Error,
        Finish,
    }

}
