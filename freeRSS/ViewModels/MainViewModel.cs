using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using freeRSS.Common;
using System.Diagnostics;
using freeRSS.Models;

namespace freeRSS.ViewModels
{
    public class MainViewModel : BindableBase
    {
        public FeedViewModel StarredFeed { get; private set; }
        public FeedViewModel CurrentFeed
        {
            get { return _currentFeed; }
            set
            {
                if (SetProperty(ref _currentFeed, value))
                {
                    if ((_currentFeed??(_currentFeed = new FeedViewModel())).Articles.Count > 0)
                    {
                        CurrentArticle = _currentFeed.Articles.First();
                    }
                    else
                    {
                        // If the articles have not yet been loaded, clear CurrentArticle then
                        // wait until the articles are loaded before selecting the first one. 
                        CurrentArticle = null;
                        NotifyCollectionChangedEventHandler handler = null;
                        handler = (s, e) =>
                        {
                            if (e.Action == NotifyCollectionChangedAction.Add)
                            {
                                _currentFeed.Articles.CollectionChanged -= handler;

                                // Use the dispatcher to update CurrentArticle. This fixes a timing issue that happens 
                                // on app launch where the first article does not appear selected in the UI because 
                                // change notification occurs for CurrentArticle before the UI has finished loading.
                                var withoutAwait = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                                    Windows.UI.Core.CoreDispatcherPriority.Normal,
                                    () => CurrentArticle = _currentFeed.Articles.First());
                            }
                        };
                        _currentFeed.Articles.CollectionChanged += handler;
                    }
                }
            }
        }
        private FeedViewModel _currentFeed = new FeedViewModel();


        public ArticleModel CurrentArticle
        {
            get { return _currentArticle; }
            set
            {
                // CurrentArticle is a special case, so it doesn't use SetProperty 
                // to update the backing field, raising the PropertyChanged event
                // only when the field value changes. Instead, CurrentArticle raises
                // PropertyChanged every time the setter is called. This ensures
                // that the ListView selection is updated when changing feeds, even 
                // if the first article is the same in both feeds. It also ensures
                // that clicking an article in the narrow view will always navigate
                // to the details view, even if the article is already the current one.
                _currentArticle = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentArticleAsObject));
            }
        }
        private ArticleModel _currentArticle;
        /// <summary>
        /// Gets the current article as an instance of type Object. 
        /// </summary>
        public object CurrentArticleAsObject => CurrentArticle as object;

        // Collection of Rss feeds
        public ObservableCollection<FeedViewModel> Feeds { get; set; }

        // 初始化一个新的MainViewModel
        public MainViewModel()
        {
            this.Feeds = new ObservableCollection<FeedViewModel>();

            this.Feeds.CollectionChanged += (s, e) =>
            {
                //OnPropertyChanged(nameof(FeedsWithFavorites));
                //OnPropertyChanged(nameof(HasNoFeeds));

                // Save the Feeds collection here only for additions, not including 看着难受 ：）
                // the bulks additions that occur during initialization. This approach
                // is necessary to handle list reorderings in the Edit Feeds view. Each 
                // position change results in a Remove action followed by an Add action, 
                // so removal is ignored here. For removals that don't involve reordering,
                // SaveFeedsAsync is called by the methods that handle the removals. 
                //if (suppressSave || e.Action != NotifyCollectionChangedAction.Add) return;
                //var withoutAwait = SaveFeedsAsync();
            };
        }

        /// <summary>
        /// Populates the Feeds list and initializes the CurrentFeed property. 
        /// </summary>
        public async Task InitializeFeedsAsync()
        {
            // FeedDataSource.GetStarredFeedAsync()这个异步方法已经弃用，为了同步starredFeed的内容
            // 逻辑改成了在构建普通Feed的时候同时插入构建starredFeed，可以work
            //StarredFeed = await FeedDataSource.GetStarredFeedAsync();
            StarredFeed = new FeedViewModel();

            var res = await FeedDataSource.GetFeedsAsync();
            foreach (var feed in res)
            {
                Feeds.Add(feed);
            }

            CurrentFeed = Feeds.Count == 0 ? StarredFeed : Feeds[0];
            if (StarredFeed.Articles.Count == 0) StarredFeed.ErrorMessage = NO_ARTICLES_MESSAGE;

            StarredFeed.Articles.CollectionChanged += async (s, e) =>
            {
                // This handles list saving for both newly-starred items and for 
                // reordering of the Favorites list (which causes a Remove followed by an Add). 
                // List saving for removals due to an unstarring are handled in FeedView.xaml.cs.
                if (e.Action == NotifyCollectionChangedAction.Add) await SaveArticlesAsync();
                StarredFeed.ErrorMessage = StarredFeed.Articles.Count > 0 ? null : NO_ARTICLES_MESSAGE;
            };
        }

        public async Task SaveArticlesAsync() => await StarredFeed.SaveArticlesAsync();

        public async void getIconTest()
        {
            await WebIconDownloadTool.DownLoadIconFrom_WebUri("https://blogs.msdn.microsoft.com/", "111");
        }

        private const string NO_ARTICLES_MESSAGE = "There are no starred articles.";
        private const string ALREADY_ADDED_MESSAGE = "This feed has already been added.";
    }
}
