using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using freeRSS.Common;
using freeRSS.Services;
using freeRSS.Schema;
using Windows.UI.Xaml.Media;

namespace freeRSS.Models
{
    public class ArticleModel : BindableBase
    {
        public int? Id { get; set; }

        public string Title { get; set; }

        public string FeedName { get; set; }

        public string Description { get; set; } // WebView.NavigateTo(Description);就行了 不过可以再做点处理
                                                // byd html 要转成rtf 性能大下降
        public string Rtf { get; set; }
        public string OldRtf { get; set; }
        public ImageSource FeedIconSource { get; set; }

        // 只需要用来绑定的属性
        public string Summary { get; set; }
        public int FeedId { get; set; }
        public string FeedIconSourceAsString { get; set; }

        // 这里不需要SourceAsString,因为这个是一构建好之后就不可以改变的了，
        // 只是在存数据库的时候需要转换成string，在构建的时候直接在构造函数里面写好new Uri即可
        public Uri Source { get; set; }
        public string SourceAsString
        {
            get { return Source?.OriginalString ?? string.Empty; }
        }

        // 因为实际上这个pubDate其实是确定的，而且也不会有Datetime上面的更新
        // 所以这里直接把它存为string即可，也方便前端的绑定
        public string PubDate { get; set; }

        private bool _isStarred = false;
        public bool IsStarred
        {
            get { return _isStarred; }
            set {
                if (_isStarred == value) return;

                SetProperty(ref _isStarred, value);
                if (this.Id != null)
                {
                    if (IsStarred)
                    {
                        var a = this;
                        if (MainPage.Current.ViewModel.StarredFeed.Articles.Where(x => x==a).Count() == 0)
                            MainPage.Current.ViewModel.StarredFeed.Articles.Add(a);
                    }
                    else
                    {
                        MainPage.Current.ViewModel.StarredFeed.Articles.Remove(this);
                    }
                    SQLiteService._db.UpdateAsync(this.AbstractInfo());
                }
            }
        }

        private bool _unread = true;
        public bool UnRead
        {
            get { return _unread; }
            set {
                SetProperty(ref _unread, value);
                if (this.Id != null)
                {
                    MainPage.Current.ViewModel.CurrentFeed.UnreadNum = 
                        MainPage.Current.ViewModel.CurrentFeed.Articles.ToList().Where(x => x.UnRead == true).Count();
                    SQLiteService._db.UpdateAsync(this.AbstractInfo());
                }
            }
        }

        // Construction From ArticleInfo
        public ArticleModel(ArticleInfo a)
        {
            Id = a.Id;
            Title = a.Title;
            FeedId = a.FeedId;
            Description = a.Description;
            Source = new Uri(a.Source);
            PubDate = a.PubDate;
            Rtf = a.Rtf;
            OldRtf = a.OldRtf;
            _isStarred = a.Isstarred;
            _unread = a.Unread;
        }

        // 获得一个关于这个ArticleModel的ArticleInfo实例
        public ArticleInfo AbstractInfo()
        {
            return new ArticleInfo()
            {
                Id = this.Id,
                FeedId = this.FeedId,
                Title = this.Title,
                PubDate = this.PubDate,
                Source = this.Source.ToString(),
                Description = this.Description,
                Unread = this.UnRead,
                Isstarred = this.IsStarred,
                Rtf = this.Rtf,
                OldRtf = this.OldRtf
            };
        }
    }
}
