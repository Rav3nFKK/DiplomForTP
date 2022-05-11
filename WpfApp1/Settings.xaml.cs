using Microsoft.VisualBasic.FileIO;
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

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            mailList.ItemsSource = BaseConnect.BaseModel.MailsTable.ToList();

        }

        int Flag = 0;
        private void AddMail_Click(object sender, RoutedEventArgs e)
        {
            if (Flag == 0)
            {
                AddMail.Content = "Сохранить";
                EnteredEmail.Visibility = Visibility.Visible;
                borderr.BorderThickness = new Thickness(1);
                Flag = 1;
                row.Height = new GridLength(95);
            }
            else
            {
                MailsTable mailsadd = new MailsTable { Mail = EnteredEmail.Text };
                BaseConnect.BaseModel.MailsTable.Add(mailsadd);
                BaseConnect.BaseModel.SaveChanges();
                mailList.ItemsSource = BaseConnect.BaseModel.MailsTable.ToList();
            }
        }

        private void del_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int uid = Convert.ToInt32(button.Uid);
            MessageBoxResult result = MessageBox.Show("Удалить данный e-mail из рассылки?", "Удаление e-mail", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (result == MessageBoxResult.Yes)
            {
                MailsTable delmail = BaseConnect.BaseModel.MailsTable.FirstOrDefault(x => x.Id == uid);
                BaseConnect.BaseModel.MailsTable.Remove(delmail);
                BaseConnect.BaseModel.SaveChanges();
                mailList.ItemsSource = BaseConnect.BaseModel.MailsTable.ToList();
            }

        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
