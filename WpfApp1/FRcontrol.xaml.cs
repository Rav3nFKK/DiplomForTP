using Microsoft.VisualBasic.FileIO;
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
    /// Логика взаимодействия для FRcontrol.xaml
    /// </summary>
    public partial class FRcontrol : Page
    {

        public FRcontrol()
        {
            InitializeComponent();
            ff();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        DbProviderFactory factory;
        DbConnection connection;
        DataTable dt;
        public void ff()
        {
            try
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

                using (TextFieldParser parser = new TextFieldParser(@"fr.csv"))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    while (!parser.EndOfData)
                    {
                        //Process row
                        string[] fields = parser.ReadFields();
                        int z = 1, iss = 0;
                        foreach (string field in fields)
                        {
                            if (z == 1)
                            {
                                iss = Convert.ToInt32(field);
                                z = 2;
                            }
                            else
                            {

                                command.CommandText = "select lrp_id as LI, lrp_prioritet_id as PI, '" + field + "' as T,  lrp_mini_inf as MI from fault_data where lrp_id = " + iss;

                                adapter.SelectCommand = command;
                                adapter.Fill(ds); //fill the data set with the query result

                            }
                        }
                    }


                }




                dt = ds.Tables[0]; //copy the table from the dataset to the dataTable
                connection.Close(); //dont forget to close the connection

                LRPlist.ItemsSource = dt.DefaultView;

            }
            catch { }



        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {


            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;
            int uid = Convert.ToInt32(btn.Uid);


            System.IO.StreamWriter file = new System.IO.StreamWriter(@"fr.csv", false);


            string str1 = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (Convert.ToInt32(dt.Rows[i].ItemArray[0]) == uid)
                {

                }
                else
                {
                    str1 += dt.Rows[i].ItemArray[0] + "," + dt.Rows[i].ItemArray[2] + "\n";
                }
            }

            using (file)
            {
                file.WriteLine(str1);
            }

            MessageBox.Show("Запись удалена", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            ff();
        }

        private void AddRmControl_Click(object sender, RoutedEventArgs e)
        {
            new ControlAdd(2).ShowDialog();
            ff();
        }
    }
}
