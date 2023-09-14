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
using freeRSS.Pages;

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

        private FeedsListItemViewModel _addbutton;

        /// <summary>
        /// The initial content of <see cref="FeedsList"/>.
        /// <para>The content of <see cref="FeedsList"/> will be replaced with the Settings page
        /// when switching to the Settings page, and restored to this cached content when switching to another page</para>
        /// </summary>
        private readonly object cachedContent;

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

                    _addbutton = new FeedsListItemViewModel();
                    _addbutton.IconElement = new SymbolIcon(Symbol.Add);
                    _addbutton.Title = "Add Subscription";
                    var editbutton = new NavigationViewItem();
                    editbutton.Icon = new SymbolIcon(Symbol.Edit);
                    editbutton.Tapped += EditFeedButton_Tapped;
                    editbutton.Content = new TextBlock() { Text = "Edit Subscription" };
                    FooterItems.Add((FeedsListItemViewModel)editbutton);
                    FooterItems.Add(_addbutton);

                    foreach(var v in ViewModel.Feeds)
                    {
                        Items.Add(v);
                    }
                }
                catch { }
            };
            
            this.InitializeComponent();
            //设置顶部UI
            setWebView();
            //自适应监控窗口变化
            //this.SizeChanged += MainPage_SizeChanged;

            cachedContent = FeedsList.Content;
        }

        private void setWebView()
        {/*
            ArticleWebView.ContentLoading += (s, e) =>
             {
                 LoadingProgressBar.Visibility = Visibility.Visible;
             };
            ArticleWebView.LoadCompleted += (s, e) =>
            {
                ArticleWebView.Visibility = Visibility.Visible;
                LoadingProgressBar.Visibility = Visibility.Collapsed;
            };*/
        }

        /// <summary>
        /// 设置自定义标题栏控件
        /// </summary>
        private void setTitleUI()
        {
            /*var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            //Window.Current.SetTitleBar(GridTitleBar); 
            /*var view = ApplicationView.GetForCurrentView();
            view.TitleBar.BackgroundColor = Color.FromArgb(255, 37, 37, 37);
            view.TitleBar.ButtonBackgroundColor = Color.FromArgb(255, 37, 37, 37);
            view.TitleBar.ButtonForegroundColor = Colors.White;

            // inactive
            view.TitleBar.InactiveBackgroundColor = Color.FromArgb(255, 37, 37, 37);
            view.TitleBar.InactiveForegroundColor = Colors.Gray;
            view.TitleBar.ButtonInactiveForegroundColor = Colors.Gray;
            view.TitleBar.ButtonInactiveBackgroundColor = Color.FromArgb(255, 37, 37, 37);*/

        }

        /// <summary>
        /// 控制导航栏的开闭
        /// </summary>
        /*
        private void PaneOpenTrigger_Click(object sender, RoutedEventArgs e)
        {
            RootSplitView.IsPaneOpen = RootSplitView.IsPaneOpen ? false : true;
        }*/


        /// <summary>
        /// 新建一个Subscribtion
        /// </summary>
        private async void AddFeedButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            FeedSetDialog AddFeedDialog = new FeedSetDialog();
            await AddFeedDialog.ShowAsync();
            FeedsList.SelectedItem = null;
        }

        /// <summary>
        /// 新建一个Subscribtion
        /// </summary>
        private async void EditFeedButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            EditDialog EditFeedDialog = new EditDialog();
            await EditFeedDialog.ShowAsync();
            FeedsList.SelectedItem = null;   
        }

        /// <summary>
        /// 点击FeedListView里的Item
        /// </summary>
        private async void FeedsList_ItemClick(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs e)
        {
            if (e.IsSettingsSelected)
            {
                sender.Content = new SettingsPage();
                return;
            }
            else
            {
                sender.Content = cachedContent;
            }

            if (e.SelectedItem == _addbutton)
            {
                FeedSetDialog AddFeedDialog = new FeedSetDialog();
                await AddFeedDialog.ShowAsync();
                FeedsList.SelectedItem = null;
            }

            try
            {
                FeedsList.SelectedItem = null;
                ViewModel.CurrentFeed = (FeedViewModel)e.SelectedItem;
                RSS_ArticleListView.SelectedIndex = RSS_ArticleListView.Items.Count > 0 ? 0 : -1;
            }
            catch { }
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

        private async void SelectionChangedAsync()
        {
            if (RSS_ArticleListView.SelectedIndex >= 0)
            {
                //ArticleWebView.IsReadOnly = false;
                LoadingProgressBar.Visibility = Visibility.Visible;
                LoadingProgressBar.IsActive = true;
                ViewModel.CurrentArticle = (ArticleModel)RSS_ArticleListView.SelectedItem;
                ViewModel.CurrentArticle.UnRead = false;
                ArticleWebView.Source = ViewModel.CurrentArticle.Description;
                // 得在这里改改
                //
                // by Elipese
                // 2023/7/16
                    //string content = "";
                    //if (ViewModel.CurrentArticle.Rtf == null)
                    //{
                    //    Spire.Doc.Document doc = new Spire.Doc.Document();
                    //    StringReader sr = new StringReader(ViewModel.CurrentArticle.Description + ((App.Current.RequestedTheme == ApplicationTheme.Dark) ? "<style>*{color:white;}</style>" : ""));
                    //    doc.LoadHTML(sr, Spire.Doc.Documents.XHTMLValidationType.None);
                    //    MemoryStream ms = new MemoryStream();
                    //    doc.SaveToStream(ms, Spire.Doc.FileFormat.Rtf);
                    //    byte[] data = ms.ToArray();
                    //    sr.Close();
                    //    ms.Close();
                    //    doc.Close();
                    //    content = Encoding.Default.GetString(data);
                    //    ViewModel.CurrentArticle.Rtf = content;
                    //}
                    //else
                    //{
                    //    content = ViewModel.CurrentArticle.Rtf;
                    //}

                   // ArticleWebView.TextDocument.SetText(Windows.UI.Text.TextSetOptions.FormatRtf, content);
                    ArticleWebView.Visibility = Visibility.Visible;
                    //ArticleWebView.IsReadOnly = true;
                    LoadingProgressBar.IsActive = false;
                    LoadingProgressBar.Visibility = Visibility.Collapsed;
                //ArticleWebView.NavigateToString(ViewModel.CurrentArticle.Description + ((App.Current.RequestedTheme == ApplicationTheme.Dark)? "<style>*{color:white;}</style>":""));

                if (!IsSeted)
                {
                    ActualThemeChanged += (a, b) =>
                    {
                        //ArticleWebView.IsReadOnly = false;
                        LoadingProgressBar.Visibility = Visibility.Visible;
                        LoadingProgressBar.IsActive = true;
                        ViewModel.CurrentArticle = (ArticleModel)RSS_ArticleListView.SelectedItem;
                        ViewModel.CurrentArticle.UnRead = false;
                        // 得在这里改改
                        //
                        // by Elipese
                        // 2023/7/16
                        //string content1 = "";
                        //Spire.Doc.Document doc1 = new Spire.Doc.Document();
                        //StringReader sr1 = new StringReader(ViewModel.CurrentArticle.Description + ((App.Current.RequestedTheme == ApplicationTheme.Dark) ? "<style>*{color:white;}</style>" : ""));
                        //doc1.LoadHTML(sr1, Spire.Doc.Documents.XHTMLValidationType.None);
                        //MemoryStream ms1 = new MemoryStream();
                        //doc1.SaveToStream(ms1, Spire.Doc.FileFormat.Rtf);
                        //byte[] data1 = ms1.ToArray();
                        //sr1.Close();
                        //ms1.Close();
                        //doc1.Close();
                        //content1 = Encoding.Default.GetString(data1);
                        //ViewModel.CurrentArticle.Rtf = content1;

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
    }
}
