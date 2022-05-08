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
        public List<mailstruct> MList;
        public struct mailstruct
        {
            public string MailAdress { get; set; }
        }


        public Settings()
        {
            InitializeComponent();

            loadmail();

            DataContext = MList;
        }

        public void loadmail()
        {

            using (TextFieldParser parser = new TextFieldParser(@"mails.csv"))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {
                    //Process row
                    string[] fields = parser.ReadFields();

                    foreach (string field in fields)
                    {



                    }
                }
            }
        }
    }



}
