using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace antras_praktinis
{
    public partial class priskirtiDestytoja : Form
    {
        private string connectionString = Program.ConnectionString; // Centralizuotas ryšys
        public priskirtiDestytoja()
        {
            InitializeComponent();
            LoadPrograms();
            LoadTeachers();
            LoadAssignments();
            LoadSemesters(); // Užpildome semestrus
            dataGridView1.SelectionChanged += DataGridView1_SelectionChanged;
        }

        private void LoadGroups(int programId)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID, pavadinimas FROM grupes WHERE studiju_programa_ID = @programId";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@programId", programId);

                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);

                comboBox4.DataSource = table;
                comboBox4.DisplayMember = "pavadinimas";
                comboBox4.ValueMember = "ID";
                comboBox4.SelectedIndex = -1;
            }
        }

        private void LoadSemesters()
        {
            comboBox5.Items.Clear();
            for (int i = 1; i <= 12; i++) // Jei norite daugiau semestrų, pakeiskite diapazoną
            {
                comboBox5.Items.Add(i);
            }
            comboBox5.SelectedIndex = -1; // Numatyta reikšmė – pirmasis semestras
        }

        private void LoadPrograms()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID, pavadinimas FROM studiju_programos";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                DataTable table = new DataTable();
                adapter.Fill(table);

                comboBox1.DataSource = table;
                comboBox1.DisplayMember = "pavadinimas";
                comboBox1.ValueMember = "ID";
                comboBox1.SelectedIndex = -1;
            }
        }

        private void LoadTeachers()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID, CONCAT(vardas, ' ', pavarde) AS vardas FROM destytojai";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                DataTable table = new DataTable();
                adapter.Fill(table);

                comboBox3.DataSource = table;
                comboBox3.DisplayMember = "vardas";
                comboBox3.ValueMember = "ID";
                comboBox3.SelectedIndex = -1;
            }
        }


        private void LoadSubjects(int programId, int semester)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                SELECT d.ID, d.pavadinimas 
                FROM destomi_dalykai d
                WHERE d.studiju_programa_ID = @programId AND d.semestras = @semester";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@programId", programId);
                command.Parameters.AddWithValue("@semester", semester);

                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);

                comboBox2.DataSource = table;
                comboBox2.DisplayMember = "pavadinimas";
                comboBox2.ValueMember = "ID";
                comboBox2.SelectedIndex = -1;
            }
        }

        private void LoadAssignments()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                SELECT dd.ID,
                       sp.pavadinimas AS StudijuPrograma,
                       dal.pavadinimas AS Dalykas,
                       dal.semestras AS Semestras,
                       CONCAT(dest.vardas, ' ', dest.pavarde) AS Destytojas,
                       gr.pavadinimas AS Grupe
                FROM destytojo_dalykai dd
                JOIN destomi_dalykai dal ON dd.dalykas_ID = dal.ID
                JOIN destytojai dest ON dd.destytojas_ID = dest.ID
                JOIN grupes gr ON dd.grupes_ID = gr.ID
                JOIN studiju_programos sp ON gr.studiju_programa_ID = sp.ID";

                MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                DataTable table = new DataTable();
                adapter.Fill(table);

                dataGridView1.DataSource = table;
                dataGridView1.Columns["ID"].Visible = false; // Paslėpti ID stulpelį
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue != null && comboBox5.SelectedItem != null)
            {
                int programId = Convert.ToInt32(comboBox1.SelectedValue);
                int semester = Convert.ToInt32(comboBox5.SelectedItem);
                LoadSubjects(programId, semester); // Užkrauname dalykus pagal programą ir semestrą
                LoadGroups(programId); // Užkrauname grupes
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue == null || comboBox2.SelectedValue == null || comboBox3.SelectedValue == null || comboBox4.SelectedValue == null)
            {
                MessageBox.Show("Pasirinkite studijų programą, dėstomą dalyką, dėstytoją ir grupę.");
                return;
            }

            int teacherId = Convert.ToInt32(comboBox3.SelectedValue);
            int subjectId = Convert.ToInt32(comboBox2.SelectedValue);
            int groupId = Convert.ToInt32(comboBox4.SelectedValue);

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO destytojo_dalykai (destytojas_ID, dalykas_ID, grupes_ID) VALUES (@teacherId, @subjectId, @groupId)";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@teacherId", teacherId);
                command.Parameters.AddWithValue("@subjectId", subjectId);
                command.Parameters.AddWithValue("@groupId", groupId);
                command.ExecuteNonQuery();
            }

            MessageBox.Show("Dėstytojas sėkmingai priskirtas.");
            LoadAssignments();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0 && comboBox1.SelectedValue != null && comboBox2.SelectedValue != null && comboBox3.SelectedValue != null && comboBox4.SelectedValue != null && comboBox5.SelectedItem != null)
            {
                int assignmentId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);
                int teacherId = Convert.ToInt32(comboBox3.SelectedValue);
                int subjectId = Convert.ToInt32(comboBox2.SelectedValue);
                int groupId = Convert.ToInt32(comboBox4.SelectedValue);
                int semester = Convert.ToInt32(comboBox5.SelectedItem);

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE destytojo_dalykai SET destytojas_ID = @teacherId, dalykas_ID = @subjectId, grupes_ID = @groupId WHERE ID = @assignmentId";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@teacherId", teacherId);
                    command.Parameters.AddWithValue("@subjectId", subjectId);
                    command.Parameters.AddWithValue("@groupId", groupId);
                    command.Parameters.AddWithValue("@semester", semester);
                    command.Parameters.AddWithValue("@assignmentId", assignmentId);
                    command.ExecuteNonQuery();
                }

                MessageBox.Show("Priskyrimas atnaujintas.");
                LoadAssignments();
            }
            else
            {
                MessageBox.Show("Pasirinkite priskyrimą ir užpildykite visus laukus.");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int assignmentId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM destytojo_dalykai WHERE ID = @assignmentId";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@assignmentId", assignmentId);
                    command.ExecuteNonQuery();
                }

                MessageBox.Show("Priskyrimas pašalintas.");
                LoadAssignments();
            }
            else
            {
                MessageBox.Show("Pasirinkite priskyrimą pašalinimui.");
            }
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue != null && comboBox5.SelectedItem != null)
            {
                int programId = Convert.ToInt32(comboBox1.SelectedValue);
                int semester = Convert.ToInt32(comboBox5.SelectedItem);

                // Užkrauname dalykus pagal programą ir semestrą
                LoadSubjects(programId, semester);

                // Užkrauname grupes pagal studijų programą
                LoadGroups(programId);
            }
            else
            {
                comboBox4.DataSource = null; // Išvalome grupes
            }
        }

        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];

                // Užpildome ComboBox1 su studijų programa
                if (selectedRow.Cells["StudijuPrograma"].Value != DBNull.Value)
                {
                    string programName = selectedRow.Cells["StudijuPrograma"].Value.ToString();
                    comboBox1.SelectedIndex = comboBox1.FindStringExact(programName);
                }
                else
                {
                    comboBox1.SelectedIndex = -1;
                }

                // Užpildome ComboBox5 su semestru
                if (selectedRow.Cells["Semestras"].Value != DBNull.Value)
                {
                    int semester = Convert.ToInt32(selectedRow.Cells["Semestras"].Value);
                    comboBox5.SelectedItem = semester;
                }
                else
                {
                    comboBox5.SelectedIndex = -1;
                }

                // Užpildome ComboBox2 su dėstomu dalyku
                if (selectedRow.Cells["Dalykas"].Value != DBNull.Value)
                {
                    string subjectName = selectedRow.Cells["Dalykas"].Value.ToString();
                    comboBox2.SelectedIndex = comboBox2.FindStringExact(subjectName);
                }
                else
                {
                    comboBox2.SelectedIndex = -1;
                }

                // Užpildome ComboBox3 su dėstytoju
                if (selectedRow.Cells["Destytojas"].Value != DBNull.Value)
                {
                    string teacherName = selectedRow.Cells["Destytojas"].Value.ToString();
                    comboBox3.SelectedIndex = comboBox3.FindStringExact(teacherName);
                }
                else
                {
                    comboBox3.SelectedIndex = -1;
                }

                // Užpildome ComboBox4 su grupe
                if (selectedRow.Cells["Grupe"].Value != DBNull.Value)
                {
                    string groupName = selectedRow.Cells["Grupe"].Value.ToString();
                    comboBox4.SelectedIndex = comboBox4.FindStringExact(groupName);
                }
                else
                {
                    comboBox4.SelectedIndex = -1;
                }
            }
            else
            {
                // Jei niekas nepasirinkta, išvalome ComboBox laukus
                comboBox1.SelectedIndex = -1;
                comboBox2.SelectedIndex = -1;
                comboBox3.SelectedIndex = -1;
                comboBox4.SelectedIndex = -1;
                comboBox5.SelectedIndex = -1;
            }
        }

    }
}
