using antras_praktinis;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace antras_praktinis
{
    static class Program
    {
        public static string ConnectionString = "SERVER=localhost;DATABASE=akademinesistema;UID=root;PASSWORD=;";

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1()); // Paleidžia prisijungimo langą
        }
    }
}