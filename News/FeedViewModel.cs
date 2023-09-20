using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using freeRSS.Common;
using freeRSS.Models;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using freeRSS.Services;
using freeRSS.Schema;

namespace freeRSS.ViewModels
{
    public class FeedViewModel : BindableBase
    {
        // 这个类里面可以涉及到数据库的用户可从界面修改的属性应该只有Name和Description而已
        private int? _id;
        public int? Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        // 从get到的xml里面拿默认设置，用户可以自定义
        private string _name;
        public string Name
        {
            get { return _name; }
            set {
                SetProperty(ref _name, value);
                if (this.Id != null) SQLiteService._db.UpdateAsync(this.AbstractInfo());
            }
        }

        private Uri _source;
        public Uri Source
        {
            get { return _source; }
            set
            {
                if (SetProperty(ref _source, value))
                    OnPropertyChanged(SourceAsString);
            }
        }

        // 图标应该是不能够再给用户设置的，所以这里就没有有SourceAsString来给用户方便自己输入string型的URI
        private string _iconSrc = string.Empty;
        public string IconSrc
        {
            get { return _iconSrc; }
            set {
                SetProperty(ref _iconSrc, value);
                setShortcutIcon(_iconSrc);
            }
        }

        // 用户按一次刷新键之后它就会有可能进行更新
        private string _lastBuildedTime;
        public string LastBuildedTime
        {
            get { return _lastBuildedTime; }
            set {
                SetProperty(ref _lastBuildedTime, value);
                if (this.Id != null) SQLiteService._db.UpdateAsync(this.AbstractInfo());
            }
        }

        // 从get到的xml里面拿默认设置，用户可以自定义
        private string _description;
        public string Description
        {
            get { return _description; }
            set {
                SetProperty(ref _description, value);
                if (this.Id != null) SQLiteService._db.UpdateAsync(this.AbstractInfo());
            }
        }

        // 存放属于这个Feed的最新的Articles
        public ObservableCollection<ArticleModel> Articles { get; set; }

        private void SetListeningPropertyChanged()
        {
            // 一旦有改变就更新视图
            Articles.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(IsEmpty));
                OnPropertyChanged(nameof(IsNotEmpty));
                OnPropertyChanged(nameof(UnreadNum));
                //OnPropertyChanged(nameof(IsInErrorAndEmpty));
                //OnPropertyChanged(nameof(IsInErrorAndNotEmpty));
                //OnPropertyChanged(nameof(IsLoadingAndNotEmpty));
            };
        }

        // 特殊的FeedViewModel：StarredFeed的构建，就使用默认的吧
        // 如果需要添加unread的Viewmodel的话，需要传一个int来标识就好
        // 然后要把isStarredFeedViewModel这个bool量换成一个int值用三个值来标识是普通的，还是喜欢的还是未读的
        public FeedViewModel()
        {
            // 实际上把它作为了StarredFeedViewModel来构建
            // StarredFeedViewModel的id就用数据库里面不可能出现的0吧，如果加上unread栏的话，unread栏的Id使用-1
            // 诶这样子其实可以通过id是否大于0来判断是不是普通的FeedviewModel，美滋滋
            _id = 0;
            _name = "My Favourites";
            _description = "There are all my favourite articles.";
            _source = null;
            //改成public
            IconSrc = string.Empty;
            _lastBuildedTime = DateTimeOffset.Now.ToString();

            Articles = new ObservableCollection<ArticleModel>();

            SetListeningPropertyChanged();
        }

        // 初始化函数, 通过在mainviewmodel里面获得的feeds列表构建所要的FeedViewModel
        public FeedViewModel (FeedInfo f)
        {
            _shortcutIcon = new BitmapImage(new Uri("ms-appx:///Assets/default/FeedHead.png"));

            // 通过给进来的FeedInfo来设置各种与数据库相关的属性的值
            // 只要这里的Id是空的话，那么就可以表明这个是一个数据库中没有的feed了
            _id = f.Id;
            _name = f.Name;
            // 可能这里的source会有两次重复设置，注意一下避免冗余
            _source = new Uri(f.Source);
            SourceAsString = f.Source;
            //改成公有
            IconSrc = (f.Id == null) ? string.Empty : f.IconSrc;
            _lastBuildedTime = f.LastBuildedTime;
            _description = f.Description;

            Articles = new ObservableCollection<ArticleModel>();

            SetListeningPropertyChanged();
        }

        public FeedInfo AbstractInfo()
        {
            return new FeedInfo()
            {
                Id = this.Id, 
                Name = this.Name,
                Source = this.SourceAsString,
                IconSrc = this.IconSrc.ToString(),
                LastBuildedTime = this.LastBuildedTime,
                Description = this.Description
            };
        }

        // 类中的相关属性
        public string SourceAsString
        {
            get { return Source?.OriginalString ?? string.Empty; }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;

                if (!value.Trim().StartsWith("http://") && !value.Trim().StartsWith("https://"))
                {
                    IsInError = true;
                    ErrorMessage = NOT_HTTP_MESSAGE;
                } else
                {
                    //  Uri.TryCreate Method (String, UriKind, Uri)
                    //  Usage: Creates a new Uri using the specified String instance and a UriKind.
                    //  public static bool TryCreate(
                    //         string uriString,
                    //         UriKind uriKind,
                    //         out Uri result
                    //  )
                    Uri uri = null;
                    if (Uri.TryCreate(value.Trim(), UriKind.Absolute, out uri))
                    {
                        Source = uri;
                    }
                    else
                    {
                        IsInError = true;
                        ErrorMessage = INVALID_URL_MESSAGE;
                    }
                }
            }
        }

        // 7. shortcutIcon
        private ImageSource _shortcutIcon;
        public ImageSource ShortcutIcon
        {
            set
            {
                SetProperty(ref _shortcutIcon, value);
            }
            get
            {
                return _shortcutIcon;
            }
        }

        /// <summary>
        /// total Unread Num.
        /// </summary>
        private int _unreadNum = -1;
        public int UnreadNum
        {
            get { return _unreadNum > 0 ? _unreadNum : this.Articles.ToList().Where(x => x.UnRead == true).Count() ; }
            set { SetProperty(ref _unreadNum, value); }
        }

        public Array NewestArticles
        {
            get
            {
                var a = new List<ArticleModel>();
                if (this.Id == 0) return a.ToArray();

                int count = 5;
                foreach (var item in this.Articles)
                {
                    if (item.UnRead)
                    {
                        a.Add(item);
                        if (--count < 0) break;
                    }
                }
                return a.ToArray();
            }
        }

        /// <summary>
        /// set the shourcut img's source async.
        /// </summary>
        private async void setShortcutIcon(string IconName)
        {
            if (IconName == "")
            {
                //若名字为空用默认ICON
                ShortcutIcon = new BitmapImage(new Uri("ms-appx:///Assets/default/FeedHead.png"));
            }
            else
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(IconName);
                IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
                BitmapImage bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(fileStream);
                ShortcutIcon = bitmapImage;
            }
        }



        /// <summary>
        /// Gets the articles collection as an instance of type Object.
        /// </summary>
        //  public object ArticlesAsObject => Articles as object;

        // 判断Articles是否为空
        public bool IsEmpty => Articles.Count == 0;
        public bool IsNotEmpty => !IsEmpty;

        // judge if the selected bar is the favouritefeed(column)
        public bool IsStarredFeed { get; set; }

        public bool IsNotFavouritesOrInError => !IsStarredFeed && !IsInError;

        //  下面的属性是用来辅助操作的

        // 判断是否是当前被选择的feed
        private bool _isSelectedInNavList;
        public bool IsSelectedInNavList
        {
            get { return _isSelectedInNavList; }
            set { SetProperty(ref _isSelectedInNavList, value); }
        }

        // 判断当前是否是在loading
        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    OnPropertyChanged(nameof(IsInError));
                    OnPropertyChanged(nameof(IsNotFavouritesOrInError));
                }
            }
        }

        //private bool _isInEdit;
        //public bool IsInEdit { get { return _isInEdit; } set { SetProperty(ref _isInEdit, value); } }

        private bool _isInError;
        public bool IsInError
        {
            get { return _isInError && !IsLoading; }
            set
            {
                if (SetProperty(ref _isInError, value))
                {
                    //OnPropertyChanged(nameof(IsNotFavouritesOrInError));
                }
            } 
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { SetProperty(ref _errorMessage, value);}
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object. 
        /// </summary>
        //public override bool Equals(object obj) =>
        //    obj is FeedViewModel ? (obj as FeedViewModel).GetHashCode() == GetHashCode() : false;

        ///// <summary>
        ///// Returns the hash code of the FeedViewModel, which is based on 
        ///// a string representation the Link value, using only the host and path.  
        ///// 即系通过link来进行一次哈希就可以拿到一个唯一标识的hash_id
        ///// </summary>
        //public override int GetHashCode() =>
        //    Source?.GetComponents(UriComponents.Host | UriComponents.Path, UriFormat.Unescaped).GetHashCode() ?? 0;


        private const string NOT_HTTP_MESSAGE = "Sorry. The URL must begin with http:// or https://";
        private const string INVALID_URL_MESSAGE = "Sorry. That is not a valid URL";
    }
}
