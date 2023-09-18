using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System;
using freeRSS.View;
using System.Collections.Generic;
using Windows.System;
using System.Linq;
using Windows.ApplicationModel.UserDataAccounts;
using freeRSS.ViewModels;
using System.ComponentModel;
using System.Threading.Tasks;
using freeRSS.Models;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;
using freeRSS.Common;
using System.IO;
using Windows.UI.Xaml.Documents;
using System.Text;
using Windows.Foundation;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media;
using Windows.Media.Devices;
using Windows.UI.WebUI;
using System.Reflection.Metadata;
using System.Xml.Linq;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Controls.Primitives;
using System.Runtime.CompilerServices;
using freeRSS.Helper;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace freeRSS
{

    public sealed partial class MainPage : Page
    {
        public static MainPage Current = null;

        public MainViewModel ViewModel { get; } = new MainViewModel();
        public ObservableCollection<FeedsListItemViewModel> Items { get; set; } = new ObservableCollection<FeedsListItemViewModel>();
        public ObservableCollection<FeedsListItemViewModel> FooterItems { get; set; } = new ObservableCollection<FeedsListItemViewModel>();

        private bool IsSeted = false;

        private FeedsListItemViewModel _addbutton = new FeedsListItemViewModel();
        private FeedsListItemViewModel _editbutton = new FeedsListItemViewModel();

        private FeedsListItemViewModel _settingbutton = new FeedsListItemViewModel();

        private FeedsListItemViewModel _oldselected = new FeedsListItemViewModel();

        //一些私有字段
        string Font_color = "black";

        //

        public MainPage()
        {
            Current = this;
            // get view model
            this.Loaded += async (sender, args) =>
            {
                try
                {
                    //viewModel 初始化
                    await ViewModel.InitializeFeedsAsync();
                    /*if (FeedsList.MenuItems.Count == 0)
                    {
                        F.SelectedIndex = 0;
                    }*/
                    FeedsList.SelectedItem = FeedsList.MenuItems.Count > 0 ? FeedsList.MenuItems[0] : null;
                    RSS_ArticleListView.SelectedIndex = RSS_ArticleListView.Items.Count > 0 ? 0 : -1;

                    _addbutton.IconElement = new SymbolIcon(Symbol.Add);
                    _addbutton.Title = "Add Subscription";

                    _editbutton.IconElement = new SymbolIcon(Symbol.Edit);
                    _editbutton.Title = "Edit Subscription";

                    _settingbutton.IconElement = new SymbolIcon(Symbol.Setting);
                    _settingbutton.Title = "设置";

                    FooterItems.Add(_editbutton);
                    FooterItems.Add(_addbutton);
                    FooterItems.Add(_settingbutton);


                    foreach (var v in ViewModel.Feeds)
                    {
                        Items.Add(v);
                    }
                }
                catch { }


                //检测系统主题
                switch (App.Current.RequestedTheme)
                {
                    case ApplicationTheme.Light:
                        Font_color = "black";
                        break;
                    case ApplicationTheme.Dark:
                        Font_color = "white";
                        break;
                    default:
                        break;
                }
            };
            
            this.InitializeComponent();
         


            //获取主题颜色
        }




        /// <summary>
        /// 点击FeedListView里的Item
        /// </summary>
        private async void FeedsList_ItemClick(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs e)
        {

            if (e.SelectedItem == _addbutton)
            {
                FeedSetDialog AddFeedDialog = new FeedSetDialog();
                await AddFeedDialog.ShowAsync();
                FeedsList.SelectedItem = _oldselected;
            }
            else if(e.SelectedItem == _editbutton)
            {
                EditDialog editDialog = new EditDialog();
                await editDialog.ShowAsync();
                FeedsList.SelectedItem = _oldselected;
            }
            else if (e.SelectedItem == _settingbutton)
            {

                Frame.Navigate(typeof(SettingPage),null);
            }
            else
            {
                _oldselected = (FeedsListItemViewModel)FeedsList.SelectedItem;
                try
                {
                    //FeedsList.SelectedItem = null;
                    ViewModel.CurrentFeed = (FeedViewModel)((FeedsListItemViewModel)e.SelectedItem).InnerObject;
                    RSS_ArticleListView.SelectedIndex = RSS_ArticleListView.Items.Count > 0 ? 0 : -1;
                }
                catch { }
            }
            
        }

        /// <summary>
        /// 点击全部Unread或者点击查看喜欢的文章
        /// </summary>
        private void FeedTotalList_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.CurrentFeed = ViewModel.StarredFeed;
            FeedsList.SelectedItem = null;
            RSS_ArticleListView.SelectedIndex = RSS_ArticleListView.Items.Count > 0 ? 0 : -1;
        }
        private IAsyncAction awaitvoid(Action action)
        {
            action();
            return default;
        }

        private void SelectionChangedAsync()
        {
            if (RSS_ArticleListView.SelectedIndex >= 0)
            {
                //ArticleWebView.IsReadOnly = false;
                LoadingProgressBar.Visibility = Visibility.Visible;
                LoadingProgressBar.IsActive = true;
                ViewModel.CurrentArticle = (ArticleModel)RSS_ArticleListView.SelectedItem;
                ViewModel.CurrentArticle.UnRead = false;
                //
                string css_string = "";
                //double DefaultFontSize = 12;
                string Background_color = "transparent";// (App.Current.RequestedTheme == ApplicationTheme.Dark) ? "white" : "black";
               
                string Font_family = "Microsoft Yahei,PingFang SC,HanHei SC,Arial";
                string html = $@"<!DOCTYPE html>
<html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta charset=""utf-8"" />
    <title>

</title>
    <style>
        p{{
           line-height:{LineHeight}px;
        }}
        .content {{
            word - wrap: break-word;
            white-space: pre-wrap;
            padding:0 20px;line-height:1.15;font-size:{ContentFontSize}px;
            background-color: {Background_color};
            color: {Font_color};
            font-family: {Font_family};
            max-width: 720px;
            margin-left: auto;
    margin-right: auto;
        }}

        img {{
width:100%;
            max-width: 720px;
        }}

    </style>
    <style>
{css_string}
    </style>
</head>
<body id=""_body"" class=""content"">
</style>
<h1>{ViewModel.CurrentArticle.Title}</h1>
    {ViewModel.CurrentArticle.Description}
</body>
</html>";
                ArticleWebView.NavigateToString(html);
                
                
                ArticleWebView.Visibility = Visibility.Visible;

                    
                LoadingProgressBar.IsActive = false;
                LoadingProgressBar.Visibility = Visibility.Collapsed;
      
                if (!IsSeted)
                {
                    ActualThemeChanged += (a, b) =>
                    {
                        //ArticleWebView.IsReadOnly = false;
                        LoadingProgressBar.Visibility = Visibility.Visible;
                        LoadingProgressBar.IsActive = true;
                        ViewModel.CurrentArticle = (ArticleModel)RSS_ArticleListView.SelectedItem;
                        ViewModel.CurrentArticle.UnRead = false;
                        

                        //ArticleWebView.TextDocument.SetText(Windows.UI.Text.TextSetOptions.FormatRtf, content1);
                        ArticleWebView.Visibility = Visibility.Visible;
                        //ArticleWebView.IsReadOnly = true;
                        LoadingProgressBar.IsActive = false;
                        LoadingProgressBar.Visibility = Visibility.Collapsed;
                    };
                    IsSeted = true;
                }
            }
        }

        private void RSS_ArticleListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionChangedAsync();
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.GetForCurrentView().DataRequested += OnShareDataRequested;
            DataTransferManager.ShowShareUI();
        }

        private void OnShareDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var toShare = ViewModel.CurrentArticle;
            var deferral = args.Request.GetDeferral();
            DataRequest request = args.Request;
            request.Data.Properties.Title = toShare.Title;
            request.Data.Properties.Description = toShare.PubDate;
            request.Data.SetText(toShare.Summary + "\n" + toShare.SourceAsString);
            deferral.Complete();
        }

        private async void buttonSync_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentFeed != null)
            {
                await ViewModel.CurrentFeed.RefreshAsync();
                UpdateTile.UpDateTile(ViewModel.CurrentFeed.NewestArticles);
            }
        }



        private async void LineHeightBtnClick(object sender, RoutedEventArgs e)
        {
            var flag= (sender as RepeatButton).Tag.ToString();
            switch (flag)
            {
                case "bigger":
                    LineHeight += .5;
                    LineHeight = Math.Clamp(LineHeight, 10, 200);
                    //await ArticleWebView?.InvokeScriptAsync("eval", new string[] { $@"document.styleSheets[0].insertRule(""p{{line-height: {lineHeight}px;}}"")" });

                    await ArticleWebView?.InvokeScriptAsync("eval", new string[] { $"document.styleSheets[0].cssRules.item(0).style.lineHeight=\"{LineHeight}px\"" });
                    break;
                case "smaller":
                    LineHeight -= .5;
                    LineHeight = Math.Clamp(LineHeight, 10, 200);
                    await ArticleWebView?.InvokeScriptAsync("eval", new string[] { $"document.styleSheets[0].cssRules.item(0).style.lineHeight=\"{LineHeight}px\"" });
               
                    break;
                default:
                    break;
            }



        }

        private async void FeedsList_ActualThemeChanged(FrameworkElement sender, object args)
        {
            switch (App.Current.RequestedTheme)
            {
                case ApplicationTheme.Light:


                    await ArticleWebView?.InvokeScriptAsync("eval", new string[] { $@"document.body.style.color = 'black';" });
                    Font_color = "black";

                    break;
                case ApplicationTheme.Dark:


                    await ArticleWebView?.InvokeScriptAsync("eval", new string[] { $@"document.body.style.color = 'white';" });
                    Font_color = "white";

                    break;
                default:
                    break;
            }

        }
    }






    public sealed partial class MainPage : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public double LineHeight
        {
            get { 
                return UserSetting.ReadSetting<double>(UserSetting.GetCallerPropertyName(),40);
            }
            set {
                UserSetting.WriteSetting(UserSetting.GetCallerPropertyName(), value);
                OnPropertyChanged(); 
            
            }
        }
        public double ContentFontSize
        {
            get
            {
                return UserSetting.ReadSetting<double>(UserSetting.GetCallerPropertyName(), 18);
            }
            set
            {
                UserSetting.WriteSetting(UserSetting.GetCallerPropertyName(), value);
                OnPropertyChanged();
            }
        }





    }
}
