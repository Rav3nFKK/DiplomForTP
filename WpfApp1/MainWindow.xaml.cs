using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


        }

        private void SignInBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var host = "http://testred.ru/";

                var a = new RedmineManager(host, LoginTxt.Text, PassTxt.Password);
                User currentUser = a.GetCurrentUser();
                Menu menu = new Menu(LoginTxt.Text, PassTxt.Password);
                menu.Show();
                this.Hide();
            }
            catch
            {
                MessageBox.Show("Неправильный логин или пароль.");
            }
        }
    }
}
