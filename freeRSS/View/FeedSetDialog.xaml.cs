using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

using freeRSS.ViewModels;
using freeRSS.Schema;

namespace freeRSS.View
{
    // More about ContentDialog: https://docs.microsoft.com/en-us/uwp/api/Windows.UI.Xaml.Controls.ContentDialog

    public sealed partial class FeedSetDialog : ContentDialog
    {
        public enum FeedGetResult
        {
            getFeedSuccess,
            getFeedFail,
            Cancel,
            Nothing
        }

        public FeedGetResult Result { get; private set; }

        public FeedSetDialog()
        {
            this.InitializeComponent();
            this.Opened += FeedSetDialog_Opened;
        }

        void FeedSetDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            this.Result = FeedGetResult.Nothing;
        }

        private async void AddFeed_ButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Ensure the Feed or Site URL fields isn't empty. If a required field
            // is empty, set args.Cancel = true to keep the dialog open.
            if (string.IsNullOrEmpty(feedTextBox.Text))
            {
                args.Cancel = true;
                errorTextBlock.Text = "Feed or Site URL is required.";
                return;
            }

            // 检查URI是否非法，是否重复
            try
            {
                Uri check_uri = new Uri(this.feedTextBox.Text);
                foreach (var item in MainPage.Current.ViewModel.Feeds)
                {
                    if (item.Source.Equals(check_uri))
                    {
                        args.Cancel = true;
                        errorTextBlock.Text = "You already add this Feed!";
                        return;
                    }
                }
            }
            catch (UriFormatException)
            {
                args.Cancel = true;
                errorTextBlock.Text = "FreeRSS could not find a feed at the specified location.";
                return;
            }       

            // 开始尝试添加新的feed
            var newfeed = new FeedViewModel(new FeedInfo {
                Source = this.feedTextBox.Text,
            });

            // 在refresh里面内嵌了添加的逻辑，如果Id为空的话，就会往MainPage.Current.ViewModel.Feeds.里面加新的feed
            await newfeed.RefreshAsync();

            if (newfeed.ErrorMessage != null)
            {
                args.Cancel = true;
                errorTextBlock.Text = newfeed.ErrorMessage;
            }
        }

        private void Cancel_ButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            return;
        }

    }
}
