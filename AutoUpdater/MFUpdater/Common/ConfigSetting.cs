using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace MFUpdater
{
    public class ConfigSetting
    {
        /// <summary>
        /// 读取App.config中的配置信息
        /// </summary>
        /// <param name="appKey">键值</param>
        /// <returns>参数值</returns>
        public static string GetConfigValue(string appKey)
        {
            string appValue = ConfigurationManager.AppSettings[appKey];
            return appValue;
        }
    }
}
