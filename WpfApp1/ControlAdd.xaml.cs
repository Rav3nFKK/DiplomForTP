using Microsoft.VisualBasic.FileIO;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    /// Логика взаимодействия для ControlAdd.xaml
    /// </summary>
    public partial class ControlAdd : Window
    {
        int identifical = 0;
        public ControlAdd(int i)
        {
            InitializeComponent();
            identifical = i;


        }




        private void Add_Click(object sender, RoutedEventArgs e)
        {
            //запись
            if (identifical == 1)
                try
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"rm.csv", true))
                    {
                        file.WriteLine(Nomer.Text + "," + WhatTxt.Text);
                    }
                    MessageBox.Show("Закладка добавлена!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch { }
            else
            {
                try
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"fr.csv", true))
                    {
                        file.WriteLine(Nomer.Text + "," + WhatTxt.Text);
                    }
                    MessageBox.Show("Закладка добавлена!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch { }
            }

        }

        private void Nomer_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (identifical == 1)
            {
                try
                {
                    if (Convert.ToInt32(Nomer.Text) > 50)
                    {
                        ss(Convert.ToInt32(Nomer.Text));
                    }
                    else
                    {
                        TemaTxt.Visibility = Visibility.Visible;

                        TemaTxt.Text = "";
                        DesTxt.Document.Blocks.Clear();
                    }
                }
                catch { }
            }
            else
            {
                try
                {
                    if (Convert.ToInt32(Nomer.Text) > 99000)
                    {
                        ff(Convert.ToInt32(Nomer.Text));
                    }
                    else
                    {
                        TemaTxt.Visibility = Visibility.Collapsed;
                        TemaTxt.Text = "";
                        DesTxt.Document.Blocks.Clear();
                    }
                }
                catch { }

            }
        }


        /// <summary>
        /// Модуль для задач
        /// </summary>
        /// <param name="i"></param>
        public void ss(int i)
        {

            var host = "http://testred.ru/";
            var login = "User1";
            var password = "12345678";
            var a = new RedmineManager(host, login, password);
            User currentUser = a.GetCurrentUser();
            var parameters = new NameValueCollection();
            RedmineManager manager = new RedmineManager(host, login, password);

            IList<Issue> issues = a.GetObjects<Issue>(parameters);

            foreach (var issue in issues)
            {
                if (issue.Id == i)
                {

                    TemaTxt.Text = issue.Subject;
                    DesTxt.AppendText(issue.Description);

                    break;
                }
                else
                {
                    TemaTxt.Text = "";
                    DesTxt.Document.Blocks.Clear();
                }
            }
        }


        DbProviderFactory factory;
        DbConnection connection;

        /// <summary>
        /// модуль для ЛРП
        /// </summary>
        /// <param name="i"></param>
        public void ff(int i)
        {
            #region подключение к базе
            factory = DbProviderFactories.GetFactory("System.Data.OracleClient");
            connection = factory.CreateConnection();
            connection.ConnectionString = "Data Source=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.111.197)(PORT = 1521))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = NIS)));Password=dtrnjh340;User ID=ais_disp2";
            connection.Open();
            #endregion
            DbCommand command = factory.CreateCommand(); //create a new command

            command.Connection = connection; //connect the command
            DbDataAdapter adapter = factory.CreateDataAdapter();//create new adapter
            DataSet ds = new DataSet(); //create a new dataset




            command.CommandText = "select lrp_mini_inf as SUB from fault_data where lrp_id = " + i;

            adapter.SelectCommand = command;
            adapter.Fill(ds); //fill the data set with the query result

            DataTable dt;
            dt = ds.Tables[0]; //copy the table from the dataset to the dataTable
            connection.Close(); //dont forget to close the connection
            if (dt.Rows[0].ItemArray[0].ToString() == "")
            {
                DesTxt.AppendText("Описание отсутствует");
            }
            else
            {
                DesTxt.AppendText(dt.Rows[0].ItemArray[0].ToString());
            }

        }
    }
}



