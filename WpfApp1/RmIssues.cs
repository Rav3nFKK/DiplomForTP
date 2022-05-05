using Microsoft.VisualBasic.FileIO;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class RmIssues
    {
        public List<Issues> Issuess;
        public struct Issues
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string DateR { get; set; }
            public string NLRP { get; set; }
            public string what { get; set; }
        }

        public RmIssues()
        {
            Issuess = new List<Issues>();
            var host = "http://testred.ru/";
            var login = "User1";
            var password = "12345678";
            var a = new RedmineManager(host, login, password);
            User currentUser = a.GetCurrentUser();
            var parameters = new NameValueCollection();
            RedmineManager manager = new RedmineManager(host, login, password);
            DateTime dd;

            IList<Issue> issues = a.GetObjects<Issue>(parameters);

            using (TextFieldParser parser = new TextFieldParser(@"rm.csv"))
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
                            foreach (var issue in issues)
                            {
                                if (issue.Id == iss)
                                {
                                    Issues issue2 = new Issues();
                                    issue2.Id = issue.Id;
                                    issue2.Name = issue.Subject;
                                    dd = Convert.ToDateTime(issue.DueDate);
                                    if (dd.Year > 2000)
                                        issue2.DateR = dd.ToString("dd/MM/yyyy");
                                    else
                                        issue2.DateR = "None";
                                    issue2.Description = issue.Description;

                                    foreach (var c in issue.CustomFields)
                                    {
                                        if (c.Values != null)
                                            if (c.Id == 10)
                                                if (c.Value != null)
                                                    issue2.NLRP = c.Values[0].Info;
                                                else { issue2.NLRP = " "; }
                                            else { issue2.NLRP = " "; }
                                        else { issue2.NLRP = " "; }
                                    }

                                    issue2.what = field;

                                    Issuess.Add(issue2);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
