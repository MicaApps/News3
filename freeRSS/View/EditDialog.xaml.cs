using freeRSS.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace freeRSS.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class EditDialog : ContentDialog
    {
        MainViewModel ViewModel = MainPage.Current.ViewModel;

        public enum EditFeedResult
        {
            Cancel,
            Nothing
        }

        public EditFeedResult Result { get; private set; }

        public EditDialog()
        {
            this.InitializeComponent();
            this.Opened += EditDialog_Opened;
        }

        void EditDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            this.Result = EditFeedResult.Nothing;
        }

        private void Save_ButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Ensure the Feed or Site URL fields isn't empty. If a required field
            // is empty, set args.Cancel = true to keep the dialog open.
            if (MainPage.Current.ViewModel.CurrentFeed.Id == 0)
            {
                args.Cancel = true;
                errorTextBlock.Text = "You can not Edit Favourites";
                return;
            }

            if (string.IsNullOrEmpty(feedTextBox.Text))
            {
                args.Cancel = true;
                errorTextBlock.Text = "Feed should have a name.";
                return;
            } else
            {
                try
                {
                    MainPage.Current.ViewModel.CurrentFeed.Name = feedTextBox.Text;
                    MainPage.Current.ViewModel.CurrentFeed.UpdateArticlesFeedName();
                } catch (Exception e)
                {
                    args.Cancel = true;
                    errorTextBlock.Text = e.ToString();
                    return;
                }
                return;
            }
        }

        /// <summary>
        /// 直接跳出
        /// </summary>
        private void Cancel_ButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            return;
        }

        /// <summary>
        /// 删除文章
        /// </summary>
        private async void DeleteArticlesButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (MainPage.Current.ViewModel.CurrentFeed.Id == 0)
            {
                errorTextBlock.Text = "You can not delete Article in Favourites";
                return;
            }
            await MainPage.Current.ViewModel.CurrentFeed.ClearOutTimeArticlesAsync();
            this.Hide();
        }

        /// <summary>
        /// 删除Feed
        /// </summary>
        private async void DeleteFeedButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (MainPage.Current.ViewModel.CurrentFeed.Id == 0)
            {
                errorTextBlock.Text = "You can not delete Favourites";
                return;
            }
            else
            {
                // 删除
                await MainPage.Current.ViewModel.CurrentFeed.RemoveRelatedArticlesAsync();
                await MainPage.Current.ViewModel.CurrentFeed.RemoveAFeedAsync();
                MainPage.Current.ViewModel.Feeds.Remove(MainPage.Current.ViewModel.CurrentFeed);

                // 重新初始化CurrentFeed 和 CurrentArticle
                MainPage.Current.ViewModel.CurrentFeed = MainPage.Current.ViewModel.Feeds.Count == 0 ?
                    MainPage.Current.ViewModel.StarredFeed :
                    MainPage.Current.ViewModel.Feeds[0];

                if(MainPage.Current.ViewModel.CurrentFeed.Articles.Count > 0)
                {
                    MainPage.Current.ViewModel.CurrentArticle = MainPage.Current.ViewModel.CurrentFeed.Articles[0] ?? null;
                } else
                {
                    MainPage.Current.ViewModel.CurrentArticle = null;
                }
                this.Hide();
            }
        }
    }
}
