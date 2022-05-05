using Oracle.ManagedDataAccess.Client;
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
    /// Логика взаимодействия для NewIssuesLRP.xaml
    /// </summary>
    public partial class NewIssuesLRP : Window
    {
        public static string login, password;
        public NewIssuesLRP(string l, string p)
        {
            login = l;
            password = p;

            InitializeComponent();
            search();

        }

        DbProviderFactory factory;
        DbConnection connection;
        DataTable dtNewLrpForDay;

        public void search()
        {

            #region подключение к базе
            factory = DbProviderFactories.GetFactory("System.Data.OracleClient");
            connection = factory.CreateConnection();
            connection.ConnectionString = "Data Source=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.111.197)(PORT = 1521))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = NIS)));Password=dtrnjh340;User ID=ais_disp2";
            connection.Open();
            #endregion

            DbCommand commandNewLrpForDay = factory.CreateCommand(); //create a new command

            commandNewLrpForDay.CommandText = "select  f.lrp_id as lid, f.lrp_mini_inf as minf, f.lrp_inf as inf, sl.system_name_short, f.lrp_prioritet_id, l.lrp_status_name, f.lrp_time_open from   ais_disp2.lrp_system_list sl,  fault_data f, lrp_status l, spd.wwsec_person p where  f.lrp_status_id = l.lrp_status_id and     f.LRP_RESPONDENT_ID = p.id and     f.lrp_status_id not in (3,7) and     f.lrp_respondent_id not in (3884)  and     not exists (select * from lrp_drp d where d.secret_comments like '%red.transset.ru/issues%' and f.lrp_id = d.lrp_id) and (((to_date(sysdate, 'dd.mm.yy') = to_date(f.lrp_time_open, 'dd.mm.yy')) and to_char(f.lrp_time_open, 'HH24') in (00,01,02,03,04,05,06,07,08,09,10,11,12,13,14,15,16,17)) or ((to_date(sysdate-1, 'dd.mm.yy') = to_date(f.lrp_time_open, 'dd.mm.yy')) and to_char(f.lrp_time_open, 'HH24') in (17,18,19,20,21,22,23))) and (p.email like '%transset%' or (p.email not like '%transset%' and f.EMAIL_ADD like '%transset%')) and  sl.system_id = f.system_id order by f.lrp_time_open, l.lrp_status_name";
            commandNewLrpForDay.Connection = connection; //connect the command

            DbDataAdapter adapter = factory.CreateDataAdapter();//create new adapter
            adapter.SelectCommand = commandNewLrpForDay;


            DataSet dsLrpForDay = new DataSet(); //create a new dataset

            adapter.Fill(dsLrpForDay); //fill the data set with the query result
            dtNewLrpForDay = dsLrpForDay.Tables[0]; //copy the table from the dataset to the dataTable

            ListNewLrp.ItemsSource = dtNewLrpForDay.DefaultView;
            connection.Close();

        }


        private void res_Click(object sender, RoutedEventArgs e)
        {

        }


        private void create_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;
            int uid = Convert.ToInt32(btn.Uid);
            opis.Document.Blocks.Clear();
            Lrp.Text = uid.ToString();
            infoIs.IsEnabled = true;
            for (int i = 0; i < dtNewLrpForDay.Rows.Count; i++)
            {
                if (Convert.ToInt32(dtNewLrpForDay.Rows[i].ItemArray[0]) == uid)
                {
                    if ("" != dtNewLrpForDay.Rows[i].ItemArray[2].ToString())
                        opis.AppendText(dtNewLrpForDay.Rows[i].ItemArray[2].ToString());
                    else { opis.AppendText("Отсутствует"); }

                    opis.AppendText("\n*Источник:*\n http://192.168.111.197:7780/pls/portal30/ais_disp2.p_create_denials.p_recovery_sets?v_id_denial=" + uid);
                    Tema.Text = "ЛРП " + uid + ". " + dtNewLrpForDay.Rows[i].ItemArray[3].ToString() + ". ";

                    inDate.SelectedDate = DateTime.Now;
                }
            }


        }

        private void createIss_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var host = "http://testred.ru/";
                var redmine = new RedmineManager(host, login, password);

                redmine.ImpersonateUser = login;
                User user = redmine.GetCurrentUser();
                IList<IssueCustomField> fields = new List<IssueCustomField>();
                IList<CustomFieldValue> fieldss = new List<CustomFieldValue>();
                IssueCustomField cs = new IssueCustomField();
                CustomFieldValue cf = new CustomFieldValue();

                cf.Info = Lrp.Text;
                cs.Id = 10;
                fieldss.Add(cf);
                cs.Values = fieldss;
                fields.Add(cs);
                var opistext = new TextRange(opis.Document.ContentStart, opis.Document.ContentEnd);
                Issue redminetask = new Issue()
                {

                    AssignedTo = new IdentifiableName() { Id = user.Id },
                    Author = new IdentifiableName() { Id = user.Id },
                    Subject = Tema.Text,
                    Description = opistext.Text,
                    Project = new IdentifiableName { Id = 5 },
                    StartDate = inDate.SelectedDate,                         // Дата создание
                    DueDate = outDate.SelectedDate,                   // Дата окончания
                    Tracker = new IdentifiableName { Id = 1 },        // Трекер
                    Status = new IdentifiableName { Id = 1 },
                    Priority = new IdentifiableName { Id = 1 },
                    CustomFields = fields,
                    IsPrivate = false,
                    Category = new IdentifiableName { Id = 1 },
                };


                Issue savedIssues = redmine.CreateObject(redminetask);
                MessageBox.Show("Задача " + savedIssues.Id.ToString() + " успешно создана");



                #region подключение к базе
                factory = DbProviderFactories.GetFactory("System.Data.OracleClient");
                connection = factory.CreateConnection();
                connection.ConnectionString = "Data Source=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.111.197)(PORT = 1521))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = NIS)));Password=dtrnjh340;User ID=ais_disp2";
                connection.Open();
                #endregion


                DbCommand commandNewLrpForDay = factory.CreateCommand(); //create a new command

                commandNewLrpForDay.CommandText = "insert into lrp_drp t (t.drp_id, t.drp_avtor, t.lrp_id, t.secret_comments) values ((select max(drp_id) + 1 from lrp_drp), 'HELPDESK_SA'," +
                    Lrp.Text + ", 'http://red.transset.ru/issues/" + savedIssues.Id + "')";
                commandNewLrpForDay.Connection = connection; //connect the command

                commandNewLrpForDay.ExecuteNonQuery();
                search();

                connection.Close();

            }
            catch { }


        }
    }
}
