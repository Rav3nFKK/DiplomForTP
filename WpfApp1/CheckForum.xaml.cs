using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
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
    /// Логика взаимодействия для CheckForum.xaml
    /// </summary>
    public partial class CheckForum : Window
    {
        DbProviderFactory factory;
        DbConnection connection;
        DataTable dtLrpForAnalys, dtLrpTrs, dtLrpCss, dtLrpForClose;

        public void opCon()
        {
            #region подключение к базе
            factory = DbProviderFactories.GetFactory("System.Data.OracleClient");
            connection = factory.CreateConnection();
            connection.ConnectionString = "Data Source=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.111.197)(PORT = 1521))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = NIS)));Password=dtrnjh340;User ID=ais_disp2";
            connection.Open();
            #endregion
        }
        int checker = 0;
        public CheckForum()
        {
            InitializeComponent();

        }

        public void clearList()
        {
            ListfForum.ItemsSource = "";
            ListfForum2.ItemsSource = "";
        }

        private void waitTRS_Click(object sender, RoutedEventArgs e)
        {
            ListfForum2.Visibility = Visibility.Visible;
            ListfForum.Visibility = Visibility.Collapsed;
            clearList();
            checker = 0;
            try
            {
                string s = System.IO.File.ReadAllText(@"sqlCur_TRS.txt", Encoding.UTF8).Replace("\n", " ");
                opCon(); //открытие подключения к бд

                DbCommand commandWaitTrs = factory.CreateCommand(); //create a new command

                #region Анализ ЛРП
                commandWaitTrs.CommandText = s;
                commandWaitTrs.Connection = connection; //connect the command

                DbDataAdapter adapter = factory.CreateDataAdapter();//create new adapter
                adapter.SelectCommand = commandWaitTrs;


                DataSet dsWaitTrs = new DataSet(); //create a new dataset

                adapter.Fill(dsWaitTrs); //fill the data set with the query result
                dtLrpTrs = dsWaitTrs.Tables[0]; //copy the table from the dataset to the dataTable

                #endregion
                ListfForum2.ItemsSource = dtLrpTrs.DefaultView;
                connection.Close();

            }
            catch (Exception em)
            {
                MessageBox.Show(em.Message);
            }
        }

        private void AnalistBTN_Click(object sender, RoutedEventArgs e)
        {
            ListfForum.Visibility = Visibility.Visible;
            ListfForum2.Visibility = Visibility.Collapsed;
            clearList();
            checker = 0;
            try
            {
                string s = System.IO.File.ReadAllText(@"sqlCur_Analyst.txt", Encoding.UTF8).Replace("\n", " ");
                opCon(); //открытие подключения к бд

                DbCommand commandLrpForAnalys = factory.CreateCommand(); //create a new command

                #region Анализ ЛРП
                commandLrpForAnalys.CommandText = s;
                commandLrpForAnalys.Connection = connection; //connect the command

                DbDataAdapter adapter = factory.CreateDataAdapter();//create new adapter
                adapter.SelectCommand = commandLrpForAnalys;


                DataSet dsLrpForAnalys = new DataSet(); //create a new dataset

                adapter.Fill(dsLrpForAnalys); //fill the data set with the query result
                dtLrpForAnalys = dsLrpForAnalys.Tables[0]; //copy the table from the dataset to the dataTable

                #endregion
                ListfForum.ItemsSource = dtLrpForAnalys.DefaultView;
                connection.Close();

            }
            catch (Exception em)
            {
                MessageBox.Show(em.Message);
            }
        }

        private void waitCSS_Click(object sender, RoutedEventArgs e)
        {
            ListfForum.Visibility = Visibility.Collapsed;
            ListfForum2.Visibility = Visibility.Visible;
            opCon(); //открытие подключения к бд
            checker = 0;
            clearList();
            try
            {
                string s = System.IO.File.ReadAllText(@"sqlCur_CSS.txt", Encoding.UTF8).Replace("\n", " ");
                opCon(); //открытие подключения к бд

                DbCommand commandWaitCss = factory.CreateCommand(); //create a new command

                #region Анализ ЛРП
                commandWaitCss.CommandText = s;
                commandWaitCss.Connection = connection; //connect the command

                DbDataAdapter adapter = factory.CreateDataAdapter();//create new adapter
                adapter.SelectCommand = commandWaitCss;


                DataSet dsWaitCss = new DataSet(); //create a new dataset

                adapter.Fill(dsWaitCss); //fill the data set with the query result
                dtLrpCss = dsWaitCss.Tables[0]; //copy the table from the dataset to the dataTable

                #endregion
                ListfForum2.ItemsSource = dtLrpCss.DefaultView;
                connection.Close();

            }
            catch (Exception em)
            {
                MessageBox.Show(em.Message);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (checker < dtLrpForAnalys.Rows.Count)
                {
                    checker++;

                }
                else
                {
                    opCon();
                    int i = ComboBox.SelectedIndexProperty.GlobalIndex;
                    ComboBox cmb = (ComboBox)sender;
                    int idLRP = Convert.ToInt32(cmb.Uid);
                    int idStat = Convert.ToInt32(cmb.SelectedIndex);

                    DbCommand commandNewLrpForDay = factory.CreateCommand(); //create a new command

                    commandNewLrpForDay.CommandText = "update fault_data set lrp_actual_type_id =" + idStat + " where lrp_id =" + idLRP;
                    commandNewLrpForDay.Connection = connection; //connect the command

                    commandNewLrpForDay.ExecuteNonQuery();



                }
            }
            catch
            {

            }
            finally
            {
                connection.Close();
            }
        }

        private void closeLRP_Click(object sender, RoutedEventArgs e)
        {
            ListfForum.Visibility = Visibility.Visible;
            ListfForum2.Visibility = Visibility.Collapsed;
            clearList();
            checker = 0;
            try
            {
                string s = System.IO.File.ReadAllText(@"sqlCur_CloseThis.txt", Encoding.UTF8).Replace("\n", " ");
                opCon(); //открытие подключения к бд

                DbCommand commandLrpForClose = factory.CreateCommand(); //create a new command

                #region Анализ ЛРП
                commandLrpForClose.CommandText = s;
                commandLrpForClose.Connection = connection; //connect the command

                DbDataAdapter adapter = factory.CreateDataAdapter();//create new adapter
                adapter.SelectCommand = commandLrpForClose;


                DataSet dsLrpForCloses = new DataSet(); //create a new dataset

                adapter.Fill(dsLrpForCloses); //fill the data set with the query result
                dtLrpForAnalys = dsLrpForCloses.Tables[0]; //copy the table from the dataset to the dataTable

                #endregion
                ListfForum.ItemsSource = dtLrpForAnalys.DefaultView;
                connection.Close();

            }
            catch (Exception em)
            {
                MessageBox.Show(em.Message);
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            var urlPart = ((Hyperlink)sender).NavigateUri;
            var fullUrl = string.Format("{0}", urlPart);
            Process.Start(new ProcessStartInfo(fullUrl));
            e.Handled = true;
        }



        private void Browser_Click(object sender, RoutedEventArgs e)
        {
            Button url = (Button)sender;
            int urlPart = Convert.ToInt32(url.Uid);

            var fullUrl = string.Format("http://192.168.111.197:7780/pls/portal30/!ais_disp2.p_create_denials.p_new_denials_main?v_denials_id={0}&event_save=&v_recovery=&v_id_out=&v_str_alert=&v_id_uniq=3679514&v_select_padge=", urlPart);
            Process.Start(new ProcessStartInfo(fullUrl));
            e.Handled = true;
        }
    }
}
