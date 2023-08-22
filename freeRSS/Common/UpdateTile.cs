using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace freeRSS.Common
{
    public static class UpdateTile
    {
        public static void UpDateTile(Array articleArray)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(File.ReadAllText("tile.xml", Encoding.UTF8));
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);

            string pattern = "(\\d+)/(\\d+)/(\\d+) ";
            //正则匹配
            var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            XmlNodeList texts = xmlDocument.GetElementsByTagName("text");

            int newArticlNum = (articleArray.Length < 5) ? articleArray.Length : 5;
            for (int j = 0; j < newArticlNum; j++)
            {
                Models.ArticleModel article = (Models.ArticleModel)articleArray.GetValue(j);
                for (int i = 0; i < texts.Length - 2; i += 2)
                {
                    texts[i].InnerText = article.Title.Substring(0, article.Title.Length > 50 ? 50 : article.Title.Length);

                    var matchStrings = regex.Matches(article.PubDate.ToString());

                    texts[i + 1].InnerText = matchStrings[0].Value;
                }
                texts[6].InnerText = article.Summary.Substring(0, 200);
                var notification = new TileNotification(xmlDocument);
                TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
            }
        }
    }
}
