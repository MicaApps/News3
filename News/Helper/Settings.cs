using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace News.Helper
{
    public partial class UserSetting
    {

        static string setting_filename = "app_settings";
        /// <summary>
        /// 读取json文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static async Task<T> ReadObjectAsync<T>(string filename)
        {
            var file_item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(filename);
            if (file_item == null)
            {
                return default(T);
            }
            try
            {
                var file = (StorageFile)file_item;
                string _string = await FileIO.ReadTextAsync(file);
                var _object = JsonConvert.DeserializeObject<T>(_string);
                return _object;
            }
            catch (Exception)
            {

                return default(T);
            }

        }
        /// <summary>
        /// 保存json文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename"></param>
        /// <param name="_object"></param>
        /// <returns></returns>
        public static async Task WriteObjectAsync<T>(string filename, T _object)
        {
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            string _string = JsonConvert.SerializeObject(_object);
            await FileIO.WriteTextAsync(file, _string);
        }


        // 读取设置文件
        public async static Task<UserSetting> ReadSetting()
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            var file = await folder.TryGetItemAsync(setting_filename);
            if (file != null)
            {
                try
                {

                    string settting_string = await FileIO.ReadTextAsync((StorageFile)file);
                    UserSetting setting = JsonConvert.DeserializeObject<UserSetting>(settting_string);
                    return setting;

                }
                catch (Exception)
                {
                    var default_setting = new UserSetting();
                    return default_setting;
                }



            }
            else
            {

                var default_setting = new UserSetting();
                return default_setting;
            }

        }





        // 保存设置文件
        public async static void SaveSetting(UserSetting setting)
        {
            string settting_string = JsonConvert.SerializeObject(setting);

            StorageFolder folder = ApplicationData.Current.LocalFolder;
            var file = await folder.CreateFileAsync(setting_filename, CreationCollisionOption.ReplaceExisting);

            await FileIO.WriteTextAsync(file, settting_string);

        }





        public static void WriteSetting<T>(string key, T Tvalue)// where T : struct
        {
            ApplicationDataContainer root = ApplicationData.Current.LocalSettings;
            if (root.Values.TryGetValue(key, out object oldkey))
            {

                root.Values[key] = Tvalue;
            }
            else
            {
                root.Values.Add(key, Tvalue);
            }
        }





        public static T ReadSetting<T>(string key)
        {
            ApplicationDataContainer root = ApplicationData.Current.LocalSettings;

            if (root.Values.TryGetValue(key, out object value))
            {
                return (T)value;
            }
            else
            {
                return default(T);
            }
        }

        public static T ReadSetting<T>(string key, T default_value)
        {
            ApplicationDataContainer root = ApplicationData.Current.LocalSettings;

            if (root.Values.TryGetValue(key, out object value))
            {
                return (T)value;
            }
            else
            {
                return default_value;
            }
        }

        public static string GetCallerPropertyName([CallerMemberName] string propertyName = "")
        {
            return propertyName;
        }
    }



}
