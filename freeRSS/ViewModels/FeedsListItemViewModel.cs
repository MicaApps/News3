using freeRSS.Common;
using Windows.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace freeRSS.ViewModels
{
    public class FeedsListItemViewModel : BindableBase
    {
        public FeedsListItemViewModel() { }

        private Windows.UI.Xaml.Controls.IconElement _iconElement;
        public Windows.UI.Xaml.Controls.IconElement IconElement
        {
            get { return _iconElement; }
            set
            {
                SetProperty(ref _iconElement, value);
            }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                SetProperty(ref _title, value);
            }
        }

        private string _subtitle;
        public string SubTitle
        {
            get { return _subtitle; }
            set
            {
                SetProperty(ref _subtitle, value);
            }
        }

        public static explicit operator FeedsListItemViewModel(Microsoft.UI.Xaml.Controls.NavigationViewItem item)
        {
            return new FeedsListItemViewModel()
            {
                Title = (item.Content as TextBlock).Text,
                SubTitle = "",
                IconElement = item.Icon
            };
        }

        public static explicit operator FeedsListItemViewModel(NavigationViewItem v)
        {
            return new FeedsListItemViewModel()
            {
                Title = (v.Content as TextBlock).Text,
                SubTitle = "",
                IconElement = v.Icon
            };
        }
    }
}
