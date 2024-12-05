using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace antras_praktinis
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = textBox1.Text.Trim();
            string password = textBox2.Text.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Įveskite prisijungimo vardą ir slaptažodį.");
                return;
            }

            using (MySqlConnection connection = new MySqlConnection(Program.ConnectionString))
            {
                connection.Open();

                string query = "SELECT ID, role FROM naudotojai WHERE username = @username AND password = @password";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int userId = Convert.ToInt32(reader["ID"]);
                        string role = reader["role"].ToString();

                        // Patikriname rolę ir atidarome atitinkamą langą
                        if (role == "studentas")
                        {
                            this.Hide();
                            StudentForm studentForm = new StudentForm(userId);
                            studentForm.Show();
                        }
                        else if (role == "destytojas")
                        {
                            this.Hide();
                            TeacherForm teacherForm = new TeacherForm(userId);
                            teacherForm.Show();
                        }
                        else if (role == "admin")
                        {
                            this.Hide();
                            AdminForm adminForm = new AdminForm(userId);
                            adminForm.Show();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Neteisingas prisijungimo vardas arba slaptažodis.");
                    }
                }
            }
        }
    }
}
