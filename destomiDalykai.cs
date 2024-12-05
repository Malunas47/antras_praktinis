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
    public partial class destomiDalykai : Form
    {
        private string connectionString = Program.ConnectionString; // Centralizuotas ryšys
        public destomiDalykai()
        {
            InitializeComponent();
            LoadPrograms();
            LoadSubjects();
            LoadSemesters();
            dataGridView1.SelectionChanged += DataGridView1_SelectionChanged;
        }

        private void LoadSemesters()
        {
            comboBox2.Items.Clear();
            for (int i = 1; i <= 12; i++) // Jei reikia daugiau semestrų, galite pakeisti skaičių
            {
                comboBox2.Items.Add(i);
            }
            comboBox2.SelectedIndex = -1; // Numatyta reikšmė – pirmasis semestras
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

        private void LoadSubjects()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
        SELECT d.ID, d.pavadinimas, sp.pavadinimas AS studiju_programa, d.semestras
        FROM destomi_dalykai d
        JOIN studiju_programos sp ON d.studiju_programa_ID = sp.ID";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                DataTable table = new DataTable();
                adapter.Fill(table);

                dataGridView1.DataSource = table;
                dataGridView1.Columns["semestras"].HeaderText = "Semestras";
            }
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue == null || comboBox2.SelectedItem == null || string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Pasirinkite studijų programą, semestrą ir įrašykite dėstomo dalyko pavadinimą.");
                return;
            }

            int programId = Convert.ToInt32(comboBox1.SelectedValue);
            int semester = Convert.ToInt32(comboBox2.SelectedItem);
            string subjectName = textBox1.Text.Trim();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO destomi_dalykai (pavadinimas, studiju_programa_ID, semestras) VALUES (@name, @programId, @semester)";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@name", subjectName);
                command.Parameters.AddWithValue("@programId", programId);
                command.Parameters.AddWithValue("@semester", semester);
                command.ExecuteNonQuery();
            }

            MessageBox.Show("Dalykas sėkmingai pridėtas.");
            LoadSubjects(); // Atnaujina sąrašą
            textBox1.Clear();
            comboBox2.SelectedIndex = -1; // Grąžina į pirmą semestrą
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0 && comboBox1.SelectedValue != null && comboBox2.SelectedItem != null && !string.IsNullOrWhiteSpace(textBox1.Text))
            {
                int subjectId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);
                string subjectName = textBox1.Text.Trim();
                int programId = Convert.ToInt32(comboBox1.SelectedValue);
                int semester = Convert.ToInt32(comboBox2.SelectedItem);

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE destomi_dalykai SET pavadinimas = @subjectName, studiju_programa_ID = @programId, semestras = @semester WHERE ID = @subjectId";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@subjectName", subjectName);
                    command.Parameters.AddWithValue("@programId", programId);
                    command.Parameters.AddWithValue("@semester", semester);
                    command.Parameters.AddWithValue("@subjectId", subjectId);
                    command.ExecuteNonQuery();
                }

                MessageBox.Show("Dalykas sėkmingai atnaujintas.");
                LoadSubjects();
            }
            else
            {
                MessageBox.Show("Pasirinkite dėstomą dalyką redagavimui.");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int subjectId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);

                DialogResult result = MessageBox.Show("Ar tikrai norite pašalinti šį dėstomą dalyką?", "Patvirtinimas", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        string query = "DELETE FROM destomi_dalykai WHERE ID = @subjectId";
                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@subjectId", subjectId);
                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Dalykas sėkmingai pašalintas.");
                    LoadSubjects();
                }
            }
            else
            {
                MessageBox.Show("Pasirinkite dėstomą dalyką šalinimui.");
            }
        }

        private void comboBox2_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            
        }

        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // Gauname pasirinktą eilutę
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];

                // Užpildome dalyko pavadinimą
                if (selectedRow.Cells["pavadinimas"].Value != DBNull.Value)
                {
                    textBox1.Text = selectedRow.Cells["pavadinimas"].Value.ToString();
                }
                else
                {
                    textBox1.Clear();
                }

                // Užpildome studijų programą
                if (selectedRow.Cells["studiju_programa"].Value != DBNull.Value)
                {
                    string programName = selectedRow.Cells["studiju_programa"].Value.ToString();
                    comboBox1.Text = programName; // Naudojame tiesioginį tekstinį priskyrimą
                }
                else
                {
                    comboBox1.SelectedIndex = -1;
                }

                // Užpildome semestrą
                if (selectedRow.Cells["semestras"].Value != DBNull.Value)
                {
                    int semester = Convert.ToInt32(selectedRow.Cells["semestras"].Value);
                    comboBox2.Text = semester.ToString(); // Naudojame tekstinį priskyrimą
                }
                else
                {
                    comboBox2.SelectedIndex = -1;
                }
            }
            else
            {
                // Jei niekas nepasirinkta, išvalome laukus
                textBox1.Clear();
                comboBox1.SelectedIndex = -1;
                comboBox2.SelectedIndex = -1;
            }

        }
    }
}
