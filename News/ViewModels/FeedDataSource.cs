using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using System.Runtime.InteropServices.WindowsRuntime;

using News.Models;
using News.Common;
using News.Schema;
using News.Services;
using Windows.Web.Syndication;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace News.ViewModels
{

    public static class FeedDataSource
    {

        public static async Task<List<FeedViewModel>> GetFeedsAsync()
        {
            var feeds = (await SQLiteService.QueryAllFeeds()).Select(feedinfo => new FeedViewModel(feedinfo)).ToList();
            foreach (var feedviewmodel in feeds)
            {
                await feedviewmodel.LoadArticlesFromDb();
                // 如果需要加载就刷新一次，就在这里加多一个函数
            }
            return feeds;
        }

        /// <summary>
        /// Try to get the old articles in the database
        /// 这是一个可扩展的方法。
        /// </summary>
        private static async Task LoadArticlesFromDb(this FeedViewModel feedViewModel)
        {
            if (feedViewModel.Id != null)
            {
                feedViewModel.Articles.Clear();
                // working here 
                feedViewModel.IsLoading = true;
                (await SQLiteService.QueryAritclesByFeedIdAsync((int)feedViewModel.Id)).Select(item => new ArticleModel(item))
                .ToList().ForEach(article =>
                {
                    article.InitialOnlyBindingProperty(feedViewModel);
                    feedViewModel.Articles.Insert(0, article);
                    // 在这里在开始的时候添加Favourite的文章
                    if (article.IsStarred) MainPage.Current.ViewModel.StarredFeed.Articles.Insert(0, article);
                });
                feedViewModel.IsLoading = false;
            }
            return;
        }

        // 添加扩展方法来方便MainViewModel来从上面控制refresh
        /// <summary>
        /// Attempts to update the feed with new data from the server.
        /// 想要在原来的基础上加入新的文章
        /// 这是一个可扩展的方法。
        /// </summary>
        //// Cancellation 还是不是太懂，先保留一下
        public static async Task RefreshAsync(this FeedViewModel feedViewModel, CancellationToken? cancellationToken = null)
        {
            if (feedViewModel.Source.Host == "localhost" ||
                (feedViewModel.Source.Scheme != "http" && feedViewModel.Source.Scheme != "https")) return;

            feedViewModel.IsLoading = true;

            int numberOfAttempts = 5;
            bool success = false;
            // 在这里进行了一波阻塞           
            // 这个do-while循环的判断条件贼强，判断了是否成功，总共是第几次尝试，还有是否取消，一并给判断了，强
            // 5 次都没有成功， 就跳出来
            do { success = await TryGetFeedFromServerAsync(feedViewModel, cancellationToken); }
            while (!success && numberOfAttempts-- > 0 &&
                (!cancellationToken.HasValue || !cancellationToken.Value.IsCancellationRequested));

            feedViewModel.IsLoading = false;
        }

        /// <summary>
        /// Retrieves feed data from the server and updates the appropriate FeedViewModel properties.
        /// There may be two condition, One is for the Have ReFreshbuild and the orther is not.
        /// </summary>
        /// 已经修改了变量名使之可以编译，还没有全体check
        private static async Task<bool> TryGetFeedFromServerAsync(FeedViewModel feedViewModel, CancellationToken? cancellationToken = null)
        {
            var originalArticleNum = feedViewModel.Articles.Count;
            try
            {
                var feed = await new SyndicationClient().RetrieveFeedAsync(feedViewModel.Source);
                
                //if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested) return false;

                // If it is a new feed without in the database, we need to get the default basic info
                bool isHaveNewArticles = false;

                if (feedViewModel.Id == null)
                {
                    // 第一次添加新的feed
                    feedViewModel.Name = string.IsNullOrEmpty(feedViewModel.Name) ? feed.Title.Text : feedViewModel.Name;
                    feedViewModel.Description = feed.Subtitle?.Text ?? feed.Title.Text;
                    feedViewModel.IconSrc = string.Empty;
                    feedViewModel.LastBuildedTime = feed.LastUpdatedTime.ToString();
                    feedViewModel.Id = await SQLiteService.InsertOrReplaceFeedAsync(feedViewModel.AbstractInfo());

                    var homePageLinkString = (feed.IconUri == null) ? feed.Links.Select(l => l.Uri).FirstOrDefault().ToString() : feed.IconUri.ToString();
                    await feedViewModel.TryUpdateIconSource(homePageLinkString);

                    // 现在UI上添加，缩短响应时间
                    MainPage.Current.ViewModel.Feeds.Add(feedViewModel);
                    isHaveNewArticles = true;
                }

                if (isHaveNewArticles || DateTimeOffset.Parse(feedViewModel.LastBuildedTime) < feed.LastUpdatedTime)
                {
                    feedViewModel.LastBuildedTime = feed.LastUpdatedTime.ToString();
                    isHaveNewArticles = true;
                }

                if (isHaveNewArticles || feedViewModel.Articles.Count() == 0)
                {
                    var articleInfoList = new List<ArticleInfo>();
                    // Get Article from the newly getted feed And sync
                    foreach (var item in feed.Items)
                    {
                        var newArticle = new ArticleModel(new ArticleInfo()
                        {
                            FeedId = (int)feedViewModel.Id,
                            Title = item.Title.Text,
                            PubDate = item.PublishedDate.ToString(),
                            // 借鉴了官方的获取本篇文章source的方法
                            // 找到初始化的方法了
                            Source = (item.ItemUri ?? item.Links.Select(l => l.Uri).FirstOrDefault()).ToString(),
                            Description = item.Summary == null ? "The article doesn't have description!" : item.Summary.Text,
                            Unread = true,
                            Isstarred = false,
                        });

                        newArticle.Id = await SQLiteService.InsertOrReplaceArticleAsync(newArticle.AbstractInfo());
                        // 初始化那些不存在数据库里面的用于绑定的属性
                        newArticle.InitialOnlyBindingProperty(feedViewModel);
                        feedViewModel.Articles.Insert(0, newArticle);
                    }
                } else
                {
                    Debug.Write("The feed is already the newest.");
                }
                feedViewModel.IsInError = false;
                feedViewModel.ErrorMessage = null;
                return true;
            }
            catch (Exception)
            {
                if (!cancellationToken.HasValue || !cancellationToken.Value.IsCancellationRequested)
                {
                    feedViewModel.IsInError = true;
                    feedViewModel.ErrorMessage = feedViewModel.Articles.Count == originalArticleNum ? BAD_URL_MESSAGE : NO_REFRESH_MESSAGE;
                }
                return false;
            }
        }

        /// <summary>
        /// Try to Get The Favicon From the homepage Server
        /// Use Google tool, So please ensure your computer unblocked  Google
        /// </summary>
        /// 

        public static async Task  TryUpdateIconSource(this FeedViewModel feedViewModel, string HomePageUrl)
        {
            if (feedViewModel.IconSrc.ToString() != string.Empty) return;
            if (HomePageUrl == string.Empty) return;

            //一般这个不会发生
            var feedId = feedViewModel.Id ?? -1;

            int numberOfAttempts = 2;
            bool success = false;

            // 尝试下载2次
            do
            {
                success = await WebIconDownloadTool.DownLoadIconFrom_IconUri(HomePageUrl, feedId.ToString());
            } while (!success && numberOfAttempts-- > 0);


            // 如果成功,更新IconSrc, iconsrc为下载的图片文件的文件名，否则IconSrc还会是默认的那个图标路径
            if (success)
            {
                feedViewModel.IconSrc = feedId.ToString() + ".png";
                await SQLiteService.UpdateFeedInfoAsync(feedViewModel.AbstractInfo());
            }
        }

        public static async Task ClearOutTimeArticlesAsync(this FeedViewModel feedViewModel)
        {
            var original = feedViewModel.Articles;

            // 更新视图集合
            var temp = feedViewModel.Articles.ToList();
            temp.RemoveAll(x => x.UnRead == false && x.IsStarred == false);
            foreach (var item in temp)
            {
                feedViewModel.Articles.Remove(item);
            }

            var ClearArticles = from x in original
                                where x.UnRead == false && x.IsStarred == false
                                select x.AbstractInfo();
            await SQLiteService.RemoveArticlesAsync(ClearArticles);
        }

        /// <summary>
        /// Delete All the Related Articles in DataBases. 
        /// </summary>
        public static async Task RemoveRelatedArticlesAsync(this FeedViewModel feedViewModel)
        {
            await SQLiteService.RemoveArticlesAsync(feedViewModel.Articles.Select(article => article.AbstractInfo()));
            feedViewModel.Articles.Clear();
        }

        public static async Task RemoveAFeedAsync(this FeedViewModel feedViewModel)
        {
            await SQLiteService.RemoveAFeedAsync(feedViewModel.AbstractInfo());
        }

        /// <summary>
        /// Saves the feed data (not including the Favorites feed) to DataBases. 
        /// </summary>
        public static async Task SaveAsync(this IEnumerable<FeedViewModel> feeds) 
        {
            await SQLiteService.SaveFeedsInfoAsync(feeds.Select(feed => feed.AbstractInfo()));
        }

        /// <summary>
        /// Saves the favorites feed (the first feed of the feeds list) to local storage. 
        /// </summary>
        public static async Task SaveArticlesAsync(this FeedViewModel feedviewmodel)
        {
            await SQLiteService.SaveArticlesInfoAsync(feedviewmodel.Articles.Select(article => article.AbstractInfo()));
        }

        private const string BAD_URL_MESSAGE = "Hmm... Are you sure this is an RSS URL?";
        private const string NO_REFRESH_MESSAGE = "Sorry. We can't get more articles right now.";
        private const string DEFAULT_HEAD_PATH = "ms-appx:///Assets/default/DefaultHead.png";

        public static String RegexRemove(this string input, string pattern) => Regex.Replace(input, pattern, string.Empty);

        public static void InitialOnlyBindingProperty(this ArticleModel a, FeedViewModel feedViewModel)
        {
            a.FeedName = feedViewModel.Name;
            a.FeedIconSource = feedViewModel.ShortcutIcon;
            a.Summary = a.Description.RegexRemove("\\&.{0,4}\\;").RegexRemove("<.*?>");
            a.FeedIconSourceAsString = feedViewModel.IconSrc.ToString();
        }

        public static void UpdateArticlesFeedName(this FeedViewModel feedViewModel)
        {
            // 不知道如何改变它本来的内容
            var li = feedViewModel.Articles.ToList();
            var newFeedName = feedViewModel.Name;
            feedViewModel.Articles.Clear();

            foreach (var x in li) {
                x.FeedName = newFeedName;
                feedViewModel.Articles.Add(x);
            }
        }
    }
}
