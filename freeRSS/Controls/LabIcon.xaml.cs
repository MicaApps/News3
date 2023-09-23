// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using Microsoft.Toolkit.Uwp.Helpers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace freeRSS.Controls
{
    public sealed partial class LabIcon : UserControl
    {
        public LabIcon()
        {
            this.InitializeComponent();
        }

        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        { 
            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
        }

        private void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Border s = sender as Border;
            s.Background = new SolidColorBrush("#FF363636".ToColor());
        }

        private void Border_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Border s = sender as Border;
            s.Background = new SolidColorBrush(Colors.Transparent);
        }
    }
}
