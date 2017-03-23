using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using System.ComponentModel;
using DropboxTest;

namespace PDWDropboxClient
{
    /// <summary>
    /// Логика взаимодействия для UserInfoPage.xaml
    /// </summary>
    public partial class UserInfoPage : Window
    {
        UserInfo Userdata;
        public UserInfoPage(UserInfo userdata)
        {
            InitializeComponent();
            Userdata = userdata;
            Username.Content=userdata.Name+" "+userdata.Surname;
            Email.Content=userdata.Email;
            RefLink.Text=userdata.RefLink;
            Lang.Content=userdata.Locale;
            Country.Content = userdata.Country;
            ulong Coef = 1073741824;
            double currentSpace = Math.Round((Convert.ToDouble(userdata.spaceInfo.UsedSpace) / Coef), 3);
            double fullSpace = Math.Round(Convert.ToDouble(userdata.spaceInfo.FullSpace) / Coef,3);
            DriveSpaceValue.Content =  currentSpace+ "/" + fullSpace +" ГБ";
            DriveSpaceIndicator.Value = userdata.spaceInfo.Procent;
            Avatar.Source = userdata.LoadAvatar();

        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Border)sender).Background = new SolidColorBrush(Color.FromRgb(196,30,58));
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            ((Border)sender).Background = new SolidColorBrush(Colors.Firebrick);
        }
    }
}
