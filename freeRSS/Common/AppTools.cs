using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace freeRSS.Common
{
    public class AppTools
    {
        /// <summary>
        /// 写入本地设置
        /// </summary>
        /// <param name="key">设置名</param>
        /// <param name="value">设置值</param>
        public static void WriteLocalSetting(string key, string value)
        {
            var localSetting = ApplicationData.Current.LocalSettings;
            var localcontainer = localSetting.CreateContainer("RSS", ApplicationDataCreateDisposition.Always);
            localcontainer.Values[key] = value;
        }
        /// <summary>
        /// 读取本地设置
        /// </summary>
        /// <param name="key">设置名</param>
        /// <returns></returns>
        public static string GetLocalSetting(string key, string defaultValue)
        {
            var localSetting = ApplicationData.Current.LocalSettings;
            var localcontainer = localSetting.CreateContainer("RSS", ApplicationDataCreateDisposition.Always);
            bool isKeyExist = localcontainer.Values.ContainsKey(key);
            if (isKeyExist)
            {
                return localcontainer.Values[key].ToString();
            }
            else
            {
                WriteLocalSetting(key, defaultValue);
                return defaultValue;
            }
        }
        /// <summary>
        /// 写入漫游设置
        /// </summary>
        /// <param name="key">设置名</param>
        /// <param name="value">设置值</param>
        public static void WriteRoamingSetting(string key, string value)
        {
            var roamingSetting = ApplicationData.Current.RoamingSettings;
            var roamingcontainer = roamingSetting.CreateContainer("RSS", ApplicationDataCreateDisposition.Always);
            roamingcontainer.Values[key] = value;
        }
        /// <summary>
        /// 读取漫游设置
        /// </summary>
        /// <param name="key">设置名</param>
        /// <returns></returns>
        public static string GetRoamingSetting(string key, string defaultValue)
        {
            var roamingSetting = ApplicationData.Current.RoamingSettings;
            var roamingcontainer = roamingSetting.CreateContainer("RSS", ApplicationDataCreateDisposition.Always);
            bool isKeyExist = roamingcontainer.Values.ContainsKey(key);
            if (isKeyExist)
            {
                return roamingcontainer.Values[key].ToString();
            }
            else
            {
                WriteRoamingSetting(key, defaultValue);
                return defaultValue;
            }
        }
    }
}
