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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для RMcontrol.xaml
    /// </summary>
    public partial class RMcontrol : Page
    {
        public RMcontrol()
        {
            InitializeComponent();
            RmIssues rmi = new RmIssues();
            ProdList.ItemsSource = rmi.Issuess;
        }

        private void AddRmControl_Click(object sender, RoutedEventArgs e)
        {
            new ControlAdd(1).ShowDialog();

            RmIssues rmi = new RmIssues();
            ProdList.ItemsSource = rmi.Issuess;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;
            int uid = Convert.ToInt32(btn.Uid);

            RmIssues rmi = new RmIssues();
            System.IO.StreamWriter file = new System.IO.StreamWriter(@"rm.csv", false);

            string str1 = "";
            foreach (var i in rmi.Issuess)
            {
                if (i.Id == uid)
                { }
                else
                {
                    str1 += i.Id + "," + i.what + "\n";
                }
            }
            using (file)
            {
                file.WriteLine(str1);
            }
            rmi = new RmIssues();
            ProdList.ItemsSource = rmi.Issuess;
            MessageBox.Show("Запись удалена", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
