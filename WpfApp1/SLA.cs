using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{


    class SLA
    {
        DbProviderFactory factory;
        DbConnection connection;
        static DataTable dtSLAStatus, dtSLAFirstorPros, dtSLANotOtv, dtSLABLZProsr;

        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static readonly string ApplicationName = "SLA";
        static readonly string SpreadsheetId = "1WcKopoNUbCz_nDzxcl7Knki9R9iVEGFWyTQ15fx_GwY";

        #region имена листов
        static readonly string sheet1 = "Нарушения SLAи 1 приоритеты";
        static readonly string sheet2 = "Приближаются к просрочке";
        static readonly string sheet3 = "Ожидание ответа более трех дней";
        #endregion

        static SheetsService service;

        public static string login = "";

        public SLA(string l)
        {
            login = l;
            ff();
            GooglePush();

        }


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

                DbCommand commandSLAStatus = factory.CreateCommand(); //create a new command
                DbCommand commandSLAFirstorPros = factory.CreateCommand(); //create a new command
                DbCommand commandSLANotOtv = factory.CreateCommand(); //create a new command
                DbCommand commandSLABLZProsr = factory.CreateCommand(); //create a new command

                #region статус
                commandSLAStatus.CommandText = "select count(*) AS CCO from(select s.lrp_status_name, t.* from fault_data t,  lrp_status s where  t.lrp_status_id not in (3, 7)  and    t.lrp_respondent_id not in (select t.id from SPD.WWSEC_PERSON t, Z_GROUP_USER g, SPD.t_Users u where  g.usernameid = t.id and t.id = u.user_id and g.group_col_id = 1 and t.office_addr2 not in(0) and t.email not like '%transset.ru' and u.status = 1) and s.lrp_status_id = t.lrp_status_id minus select s.lrp_status_name, t.* from fault_data t,  spd.xterritorylist ter, lrp_status s where  t.lrp_status_id not in (3, 7) and s.lrp_status_id = t.lrp_status_id and t.zo = ter.territoryid and(t.system_id in (135, 137) and t.zo != 101) )group by lrp_status_name "; //make the query
                commandSLAStatus.Connection = connection; //connect the command

                DbDataAdapter adapter = factory.CreateDataAdapter();//create new adapter
                adapter.SelectCommand = commandSLAStatus;


                DataSet dsSLAStatus = new DataSet(); //create a new dataset

                adapter.Fill(dsSLAStatus); //fill the data set with the query result
                dtSLAStatus = dsSLAStatus.Tables[0]; //copy the table from the dataset to the dataTable

                #endregion

                #region первый и просрочка
                commandSLAFirstorPros.CommandText = "select fd.lrp_id,  max(ld.secret_comments), fd.lrp_prioritet_id, st.lrp_status_name, p.last_name || ' ' || p.first_name || ' ' || p.middle_name, time_trs(fd.lrp_id), ais_disp2.time_trs_timeout(fd.lrp_id), sl.system_name_short, trs.type_problem_inf, fd.lrp_mini_inf from lrp_drp ld, (select * from ais_disp2.fault_data t where t.lrp_status_id not in (3, 7)) fd, ais_disp2.lrp_status st, spd.wwsec_person p, ais_disp2.lrp_system_list sl, spd.xusername xuser, ais_disp2.LRP_TYPE_PROBLEM trs where (((ld.secret_comments like '%http://red.transset.ru/issues/%') or ld.secret_comments is null or ld.secret_comments like '#%') and ld.lrp_id = fd.lrp_id )  and fd.lrp_status_id = st.lrp_status_id and fd.type_problem_id = trs.type_problem_id and p.user_name = xuser.username and fd.lrp_respondent_id = xuser.usernameid and sl.system_id = fd.system_id and fd.lrp_prioritet_id != 4 and p.email like '%transset%' and (((fd.lrp_prioritet_id = 3) AND ((lrp_func.get_lrp_time(fd.lrp_id, 1) - lrp_func.get_lrp_time_not_in_work(fd.lrp_id, 1)) >= 20)) OR ((fd.lrp_prioritet_id = 2) AND ((lrp_func.get_lrp_time(fd.lrp_id, 1) - lrp_func.get_lrp_time_not_in_work(fd.lrp_id, 1)) >= 2)) OR (fd.lrp_prioritet_id = 1)) group by fd.lrp_id,  fd.lrp_prioritet_id, st.lrp_status_name, p.last_name || ' ' || p.first_name || ' ' || p.middle_name, time_trs(fd.lrp_id), ais_disp2.time_trs_timeout(fd.lrp_id), sl.system_name_short, trs.type_problem_inf, fd.lrp_mini_inf order by fd.lrp_prioritet_id";
                commandSLAFirstorPros.Connection = connection;

                adapter.SelectCommand = commandSLAFirstorPros;


                DataSet dsSLAFirstorPros = new DataSet(); //create a new dataset

                adapter.Fill(dsSLAFirstorPros);
                dtSLAFirstorPros = dsSLAFirstorPros.Tables[0];

                #endregion

                #region без ответа более 3х дней
                commandSLANotOtv.CommandText = "select sub.\"Номер ЛРП\", max(sub.\"Номер задачи в RM\") as \"Задача в Redmine\", sub.\"Приоритет\",sub.\"Модуль\", sub.\"Краткое описание\", sub.\"Затраченное TRS время\", sub.\"Время ожидания ответа\" from (select distinct fd.lrp_id as \"Номер ЛРП\", get_diff(sysdate - fd.last_update_time) as \"Время ожидания ответа\", fd.lrp_prioritet_id as \"Приоритет\", sl.system_name_short as \"Модуль\", fd.lrp_mini_inf as \"Краткое описание\", time_trs(fd.lrp_id) as \"Затраченное TRS время\", ld.secret_comments as \"Номер задачи в RM\" from   fault_data fd, lrp_system_list sl, lrp_drp ld where  fd.lrp_status_id = 2 and    last_update_time <= sysdate-3 and    fd.system_id = sl.system_id  and    fd.lrp_id not in (115764, 115376) and    fd.lrp_respondent_id not in (select t.id from SPD.WWSEC_PERSON t, Z_GROUP_USER g, SPD.t_Users u where  g.usernameid = t.id and t.id = u.user_id and    g.group_col_id = 1 and t.office_addr2 not in(0) and t.email not like '%transset.ru' and u.status = 1) and ld.lrp_id = fd.lrp_id and ((fd.lrp_prioritet_id = 4 and time_trs_date(fd.lrp_id) < 20) or fd.lrp_prioritet_id !=4) and ((ld.secret_comments like '%http://red.transset.ru/issues/%') or ld.secret_comments is null or ld.secret_comments like '#%')) sub group by sub.\"Номер ЛРП\", sub.\"Приоритет\", sub.\"Модуль\",sub.\"Краткое описание\", sub.\"Затраченное TRS время\", sub.\"Время ожидания ответа\"";
                commandSLANotOtv.Connection = connection;

                adapter.SelectCommand = commandSLANotOtv;

                DataSet dsSLANotOtv = new DataSet(); //create a new dataset

                adapter.Fill(dsSLANotOtv);
                dtSLANotOtv = dsSLANotOtv.Tables[0];

                #endregion

                #region близкие к просрочке  
                commandSLABLZProsr.CommandText = "SELECT DISTINCT fd.lrp_id,fd.lrp_prioritet_id, max(ld.secret_comments), ls.lrp_status_name, tu1.lastname || ' ' || tu1.firstname || ' ' || tu1.secondname, lrp_func.get_lrp_time_work(fd.lrp_id), time_trs(fd.lrp_id),get_diff(SYSDATE - fd.last_update_time), sl.system_name_short, fd.lrp_mini_inf, tu2.lastname || ' ' || tu2.firstname || ' ' || tu2.secondname FROM fault_data fd, lrp_status ls,lrp_system_list sl, spd.t_users tu1,spd.t_users tu2, spd.xuname xu1,spd.xuname xu2,       lrp_drp ld WHERE (fd.lrp_status_id = ls.lrp_status_id)and (((ld.secret_comments like '%http://red.transset.ru/issues/%') or ld.secret_comments is null or ld.secret_comments like '#%') and ld.lrp_id = fd.lrp_id ) AND (fd.system_id = sl.system_id) AND (fd.lrp_respondent_id = xu1.unameid) AND (xu1.uname = tu1.login) AND (fd.lrp_opener = xu2.unameid) AND (xu2.uname = tu2.login) AND fd.lrp_respondent_id not in (select t.id from SPD.WWSEC_PERSON t, Z_GROUP_USER g, SPD.t_Users u where  g.usernameid = t.id and t.id = u.user_id and    g.group_col_id = 1 and t.office_addr2 not in(0) and    t.email not like '%transset.ru' and u.status = 1) AND (((fd.lrp_prioritet_id = 3) AND ((lrp_func.get_lrp_time(fd.lrp_id, 1) - lrp_func.get_lrp_time_not_in_work(fd.lrp_id, 1)) BETWEEN 16 AND 20) AND (fd.lrp_status_id = 2)) OR ((fd.lrp_prioritet_id = 2) AND ((lrp_func.get_lrp_time(fd.lrp_id, 1) - lrp_func.get_lrp_time_not_in_work(fd.lrp_id, 1)) BETWEEN 1   AND 2) AND (fd.lrp_status_id = 2)) OR ((fd.lrp_prioritet_id = 1) AND ((lrp_func.get_lrp_time(fd.lrp_id, 1) - lrp_func.get_lrp_time_not_in_work(fd.lrp_id, 1)) BETWEEN 0.3 AND 1) AND (fd.lrp_status_id IN (2,5,8))))group by fd.lrp_id,fd.lrp_prioritet_id, ls.lrp_status_name, tu1.lastname || ' ' || tu1.firstname || ' ' || tu1.secondname, fd.lrp_id, fd.lrp_id,fd.last_update_time, sl.system_name_short, fd.lrp_mini_inf, tu2.lastname || ' ' || tu2.firstname || ' ' || tu2.secondname order by fd.lrp_prioritet_id";
                commandSLABLZProsr.Connection = connection;

                adapter.SelectCommand = commandSLABLZProsr;


                DataSet dsSLABLZProsr = new DataSet(); //create a new dataset

                adapter.Fill(dsSLABLZProsr);
                dtSLABLZProsr = dsSLABLZProsr.Tables[0];



                #endregion


            }
            catch
            { }
            finally
            {
                string letter1 = "Здравствуйте!\n" +
                    "Приложена ссылка на отчет с обращениями с нарушением SLA и ЛРП, приближающимся к просрочке, отчет по ЛРП без ответа более трёх дней\n\n" +
                    "https://docs.google.com/spreadsheets/d/10HW4HAmJe48qpSZQMMSAag78KDW3z0g1ASkRO97HzKs/edit#gid=1689407417 \n\n";
                string letter2 = "";
                if (dtSLAFirstorPros.Rows.Count < 1)
                {
                    letter2 = "Нарушения SLA нет.";
                }
                else
                {
                    letter2 = "Обращаем внимание, что на текущий момент есть просрочки по ЛРП:";
                    for (int i = 0; i < dtSLAFirstorPros.Rows.Count; i++)
                    {
                        letter2 += "\nhttp://192.168.111.197:7780/pls/portal30/ais_disp2.p_create_denials.p_recovery_sets?v_id_denial=" + dtSLAFirstorPros.Rows[i].ItemArray[0];
                    }
                }
                connection.Close();
                mail(letter1, letter2);
            }

        }


        public void GooglePush()
        {
            GoogleCredential credential;
            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }

            service = new SheetsService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,

            });
            CreateEtry();
        }
        static void CreateEtry()
        {
            var rangeStatus = $"{sheet1}!B6";
            var rangeDateToday = $"{sheet1}!B2";
            var rangeUser = $"{sheet1}!B3";
            var rangeDateFirst = $"{sheet1}!B14";
            var regionFirstClear = $"{sheet1}!B14:M100";

            var regionProsrClear = $"{sheet2}!B3:N100";
            var regionBlZProsr = $"{sheet2}!B3";


            var rangeSLANototv = $"{sheet3}!B2";
            var regionNotvClear = $"{sheet3}!B2:N100";




            var valueRangeStatus = new ValueRange();  //регион для статуса
            var valueRangeDay = new ValueRange();  // регион для дня сбора
            var valueRangeUser = new ValueRange();  // регион для пользователя
            var valueRangeFirstorPros = new ValueRange();  // регион для просрочки
            var valueRangeBLZPros = new ValueRange();  // регион для близких к просрочке
            var valueRangeThred = new ValueRange(); // регион для без ответа 3+


            #region данные SLA
            double statusOpNe, statusZDI, statusStop;

            if (dtSLAStatus.Rows.Count == 4)
            {

                double statusNew = Convert.ToDouble(dtSLAStatus.Rows[0].ItemArray[0]);
                double statusOpen = Convert.ToDouble(dtSLAStatus.Rows[1].ItemArray[0]);
                statusOpNe = statusNew + statusOpen;
                statusZDI = Convert.ToDouble(dtSLAStatus.Rows[2].ItemArray[0]);
                statusStop = Convert.ToDouble(dtSLAStatus.Rows[3].ItemArray[0]);
            }
            else
            {
                statusOpNe = Convert.ToDouble(dtSLAStatus.Rows[0].ItemArray[0]);
                statusZDI = Convert.ToDouble(dtSLAStatus.Rows[2].ItemArray[0]);
                statusStop = Convert.ToDouble(dtSLAStatus.Rows[3].ItemArray[0]);
            }


            var ListStatusOpNe = new List<object>() { statusOpNe };
            var ListStatusZDI = new List<object>() { statusZDI };
            var ListStatusStop = new List<object>() { statusStop };
            var ListFirstorPros = new List<object>();
            var ListNotOtv = new List<object>();
            var ListBLZProsr = new List<object>();
            valueRangeFirstorPros.Values = new List<IList<object>>();
            valueRangeThred.Values = new List<IList<object>>();
            valueRangeBLZPros.Values = new List<IList<object>>();

            for (int i = 0; i < dtSLAFirstorPros.Rows.Count; i++)
            {
                ListFirstorPros = new List<object>() { dtSLAFirstorPros.Rows[i].ItemArray[0], dtSLAFirstorPros.Rows[i].ItemArray[1], dtSLAFirstorPros.Rows[i].ItemArray[2], dtSLAFirstorPros.Rows[i].ItemArray[3], dtSLAFirstorPros.Rows[i].ItemArray[4], dtSLAFirstorPros.Rows[i].ItemArray[5], dtSLAFirstorPros.Rows[i].ItemArray[6], dtSLAFirstorPros.Rows[i].ItemArray[7], dtSLAFirstorPros.Rows[i].ItemArray[8], dtSLAFirstorPros.Rows[i].ItemArray[9] };
                valueRangeFirstorPros.Values.Add(ListFirstorPros);
            }

            for (int i = 0; i < dtSLANotOtv.Rows.Count; i++)
            {
                ListNotOtv = new List<object> { dtSLANotOtv.Rows[i].ItemArray[0], dtSLANotOtv.Rows[i].ItemArray[1], dtSLANotOtv.Rows[i].ItemArray[2], dtSLANotOtv.Rows[i].ItemArray[3], dtSLANotOtv.Rows[i].ItemArray[4], dtSLANotOtv.Rows[i].ItemArray[5], dtSLANotOtv.Rows[i].ItemArray[6], };
                valueRangeThred.Values.Add(ListNotOtv);
            }

            for (int i = 0; i < dtSLABLZProsr.Rows.Count; i++)
            {
                ListBLZProsr = new List<object> { dtSLABLZProsr.Rows[i].ItemArray[0], dtSLABLZProsr.Rows[i].ItemArray[1], dtSLABLZProsr.Rows[i].ItemArray[2], dtSLABLZProsr.Rows[i].ItemArray[3], dtSLABLZProsr.Rows[i].ItemArray[4], dtSLABLZProsr.Rows[i].ItemArray[5], dtSLABLZProsr.Rows[i].ItemArray[6], dtSLABLZProsr.Rows[i].ItemArray[7], dtSLABLZProsr.Rows[i].ItemArray[8], dtSLABLZProsr.Rows[i].ItemArray[9], dtSLABLZProsr.Rows[i].ItemArray[10] };
                valueRangeBLZPros.Values.Add(ListBLZProsr);
            }

            var DateToday = new List<object>() { DateTime.Now.ToString("dd.MM.yyyy") };
            var UserT = new List<object>() { login };


            #endregion

            #region запросы
            valueRangeDay.Values = new List<IList<object>> { DateToday };
            var appendRequestDay = service.Spreadsheets.Values.Update(valueRangeDay, SpreadsheetId, rangeDateToday);
            appendRequestDay.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;         //наполнение запроса дня

            valueRangeUser.Values = new List<IList<object>> { UserT };
            var appendRequestUser = service.Spreadsheets.Values.Update(valueRangeUser, SpreadsheetId, rangeUser);
            appendRequestUser.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;


            valueRangeStatus.Values = new List<IList<object>> { ListStatusOpNe, ListStatusZDI, ListStatusStop };
            var appendRequestStatus = service.Spreadsheets.Values.Update(valueRangeStatus, SpreadsheetId, rangeStatus);
            appendRequestStatus.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;    //наполнение запроса для статусов



            var appendRequestFirstPros = service.Spreadsheets.Values.Update(valueRangeFirstorPros, SpreadsheetId, rangeDateFirst);
            appendRequestFirstPros.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED; //наполнение запроса для первых и просрочки




            var appendRequestNototv = service.Spreadsheets.Values.Update(valueRangeThred, SpreadsheetId, rangeSLANototv);
            appendRequestNototv.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;    //наполнение запроса для статусов


            var appendRequestBLZProsr = service.Spreadsheets.Values.Update(valueRangeBLZPros, SpreadsheetId, regionBlZProsr);
            appendRequestBLZProsr.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;    //наполнение запроса для статусов




            var rr = new ClearValuesRequest();

            var ClearFirstorPros = service.Spreadsheets.Values.Clear(rr, SpreadsheetId, regionFirstClear); //очищение первых и просрочки
            var ClearNototv = service.Spreadsheets.Values.Clear(rr, SpreadsheetId, regionNotvClear); //очищение без ответа
            var ClearProsr = service.Spreadsheets.Values.Clear(rr, SpreadsheetId, regionProsrClear); //очищение к просрочке

            #endregion




            #region выполнение запроса к таблице

            ClearFirstorPros.Execute();
            ClearNototv.Execute();
            ClearProsr.Execute(); //очищение

            appendRequestStatus.Execute();
            appendRequestDay.Execute();
            appendRequestFirstPros.Execute();
            appendRequestNototv.Execute();
            appendRequestBLZProsr.Execute();
            appendRequestUser.Execute();  //наполнение
            #endregion

        }

        public void mail(string l1, string l2)
        {
            try
            {
                string from = @"shabanin.serzh@mail.ru"; // адреса отправителя
                string pass = "4GXvrzxn4VPVQNX9tv0T"; // пароль отправителя
                MailMessage mess = new MailMessage();
                mess.To.Add(@"shabanin.serzh@mail.ru"); // адрес получателя
                mess.From = new MailAddress(from);
                mess.Subject = "Форум ЦСС: соблюдение SLA"; // тема
                mess.Body = l1 + l2; // текст сообщения
                SmtpClient client = new SmtpClient();
                client.Host = "smtp.mail.ru"; //smtp-сервер отправителя
                client.Port = 587;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(from.Split('@')[0], pass);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Send(mess); // отправка пользователю
                mess.To.Remove(mess.To[0]);
                mess.To.Add(from); //для сообщения на свой адрес
                mess.Subject = "Отправлено сообщение";
                mess.Body = "Пользователю отправлено сообщение";
                mess.Dispose();
            }
            catch (Exception e)
            {
                throw new Exception("Mail.Send: " + e.Message);
            }
        }


    }
}
