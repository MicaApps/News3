using freeRSS.Helper;
using freeRSS.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace freeRSS.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class WebViewPage : Page,INotifyPropertyChanged
    {


        public static WebViewPage Instance;
        //一些私有字段
        string Font_color = "black";

        //

        public WebViewPage()
        {
            this.InitializeComponent();
            Instance = this;
            this.ActualThemeChanged += WebViewPage_ActualThemeChanged;
            this.Loaded += (s, e) => {
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
        }



        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public double LineHeight
        {
            get
            {
                return UserSetting.ReadSetting<double>(UserSetting.GetCallerPropertyName(), 40);
            }
            set
            {
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


        private ArticleModel _CurrentArticle;

        public ArticleModel CurrentArticle
        {
            get 
            { 
                return _CurrentArticle;
            }
            set 
            {
                _CurrentArticle = value; 
                LoadHtml(value); 
            }
        }




        private void LoadHtml(ArticleModel article) 
        {
            string css_string = "";
            //double DefaultFontSize = 12;
            string Background_color = "transparent";// (App.Current.RequestedTheme == ApplicationTheme.Dark) ? "white" : "black";

            string Font_family = "Microsoft Yahei,PingFang SC,HanHei SC,Arial";
            string html = $@"<!DOCTYPE html>
<html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta charset=""utf-8"" />
    <title></title>
    <style>
        p{{
           line-height:{LineHeight}px;
        }}
        .content {{
            word - wrap: break-word;
            white-space: pre-wrap;
            padding:0 20px;text-indent:2em;line-height:1.15;font-size:{ContentFontSize}px;
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
<h1>{article.Title}</h1>
    {article.Description}
</body>
</html>";
            ArticleWebView.NavigateToString(html);

        }







        private async void LineHeightBtnClick(object sender, RoutedEventArgs e)
        {
            var flag = (sender as RepeatButton).Tag.ToString();
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


        private async void WebViewPage_ActualThemeChanged(FrameworkElement sender, object args)
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
}
