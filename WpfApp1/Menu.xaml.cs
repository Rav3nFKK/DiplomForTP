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
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для Menu.xaml
    /// </summary>

    public partial class Menu : Window
    {
        DbProviderFactory factory;
        DbConnection connection;
        public static string host = "http://testred.ru";
        public static string login = "";
        public static string password = "";
        public int projectId = 1;

        public Menu(string l, string p)
        {
            InitializeComponent();
           
            login = l;
            password = p;
            load();
         
        }

        public void load()
        {
            LoadPage.FRLFrame = FRFrame;
            LoadPage.RMLFrame = RMFrame;
            FRFrame.Navigate(new FRcontrol());
            RMFrame.Navigate(new RMcontrol());
            Name.Text = login;
            Date.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private void CheckFbtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Модуль в разработке!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void NewLRPbtn_Click(object sender, RoutedEventArgs e)
        {
            new NewIssuesLRP(login, password).ShowDialog();
          
        }

        private void SLAbtn_Click(object sender, RoutedEventArgs e)
        {
            new SLA(login);
            MessageBox.Show("SLA собран! \nСообщения отправлены на электронные почты ответственным.", "Обработка запроса!", MessageBoxButton.OK, MessageBoxImage.Information);

        }


    }
}
