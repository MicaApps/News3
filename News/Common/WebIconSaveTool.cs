using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace News.Common
{
    // 格式1：<link rel="shortcut icon" href="/favicon.ico" type="image/x-icon">
    // 格式2：大多数网站直接不明确写明icon,但可通过在url上加上"/favicon.ico"访问获取

    public static class WebIconDownloadTool
    {
        /// <summary>
        /// 下载给定URI网址对应的icon, 例入传入"http://www.126.com/"
        /// </summary>
        public static async Task DownLoadIconFrom_WebUri(string Uri, string IconName)
        {
            // 大多数网站直接不明确写明icon,但可通过在url上加上"/favicon.ico"访问获取
            try
            {
                // 格式1
                await getIconByHtmlSearch(Uri, IconName);
                if (IsPicture(ApplicationData.Current.LocalFolder.Path + "\\" + IconName + ".png"))
                {
                    //若得到的是图片
                    Debug.WriteLine("格式一从html文件获取icon成功");
                }
                else
                {
                    Debug.WriteLine("格式一从html文件获取icon失败");
                    // 尝试格式2
                    Uri = getHostFromAnyUri(Uri);
                    string IconUri = Uri + "favicon.ico";
                    await DownLoadIconFrom_IconUri(IconUri, IconName);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// 传入任意网址，返回尝试其主页
        /// 例：传入 "http://daily.zhihu.com/story/9418288"，返回 "http://daily.zhihu.com/"， 若正则匹配为空范围空字符串。
        /// </summary>
        public static string getHostFromAnyUri(string Uri) // 这里不new Uri(Uri).Host完事的事情么（
        {
            string hostUri = "";
            string pattern = "[a-zA-Z]+://[^\\s]*?/";
            // 正则匹配
            var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matchStrings = regex.Matches(Uri);
            if (matchStrings.Count == 0)
            {
                Debug.WriteLine("Can not get Host Uri!");
            } else
            {
                hostUri = matchStrings[0].Value;
            }

            return hostUri;
        }

        /// <summary>
        /// HttpContent异步读取响应流并写入本地文件方法扩展
        /// </summary>
        public static async Task DownloadAsFileAsync(this HttpContent content, string fileName, bool overwrite)
        {
            string filePath = Path.GetFullPath(ApplicationData.Current.LocalFolder.Path + "\\" + fileName + ".png");
            Debug.WriteLine(filePath);

            if (!overwrite && File.Exists(filePath))
            {
                throw new InvalidOperationException(string.Format("文件 {0} 已经存在！", filePath));
            }

            try
            {
                await content.ReadAsByteArrayAsync().ContinueWith(
                    (readBytestTask) =>
                    {
                        byte[] data = readBytestTask.Result;
                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            fs.Write(data, 0, data.Length);   
                            //防止文件被独占的。
                            fs.Dispose();
                        }
                    }
                    );
            }
            catch (Exception e)
            {
                Debug.WriteLine("发生异常： {0}", e.Message);
            }
        }

        /// <summary>
        /// 判断是不是图片文件
        /// </summary>
        private static bool IsPicture(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    BinaryReader reader = new BinaryReader(fs);
                    string fileClass;
                    byte buffer;
                    buffer = reader.ReadByte();
                    fileClass = buffer.ToString();
                    buffer = reader.ReadByte();
                    fileClass += buffer.ToString();
                    reader.Dispose();
                    fs.Dispose();
                    if (fileClass == "255216" || fileClass == "7173" || fileClass == "13780" || fileClass == "6677")

                    //255216是jpg;7173是gif;6677是BMP,13780是PNG;7790是exe,8297是rar 
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 直接给出icon的URI网址进行下载
        /// 使用Feed的ID来作为Icon的名字
        /// 
        /// 草 :)
        /// by Elipese 2023/7/16
        /// </summary>
        public static async Task<bool> DownLoadIconFrom_IconUri(string Icon_Uri, string IconName)
        {
            const string GOOGLE_ICON_SEARCH_PRE = @"http://www.google.com/s2/favicons?domain_url=";

            string reqUrl = GOOGLE_ICON_SEARCH_PRE + Icon_Uri;
            HttpClient httpClient = new HttpClient();
            var cancellationTokenSource = new CancellationTokenSource(2000);
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(reqUrl, cancellationTokenSource.Token);
                response.EnsureSuccessStatusCode();
                await response.Content.DownloadAsFileAsync(IconName, true).ContinueWith(
                            (readTask) =>
                            {
                                Debug.WriteLine("文件下载完成！");
                            });
                return true;
            }
            catch (Exception)
            {
                Debug.WriteLine("Try to DownLoad Favicon Failed! Check Your Internet Or Google May be Blocked!");
                return false;
            }
        }

        /// <summary>
        /// 解决少部分网站没有"/favicon.ico"的问题，通过html内容获取
        /// </summary>
        private static async Task getIconByHtmlSearch(string Uri, string IconName)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                // 创建一个异步GET请求，当请求返回时继续处理（Continue-With模式）

                HttpResponseMessage response = await httpClient.GetAsync(Uri);
                string resultStr = await response.Content.ReadAsStringAsync();
                string patternOfShortCut = "<link +rel=\"shortcut icon\" +href=\"([^\"]*)\"";

                // 正则匹配
                var regex = new Regex(patternOfShortCut, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var matchStrings = regex.Matches(resultStr);

                // 取子表达式
                string IconUri;
                if (matchStrings.Count > 0)
                {
                    IconUri = matchStrings[0].Groups[1].Value;
                    Debug.WriteLine("Get Icon Uri: " + IconUri);
                    await DownLoadIconFrom_IconUri(IconUri, IconName);
                }
                else
                {
                    Debug.WriteLine("No icon uri!");
                    IconUri = "";
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}
