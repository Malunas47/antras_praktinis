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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace antras_praktinis
{
    public partial class studijuProgramos : Form
    {
        private string connectionString = Program.ConnectionString;
        public studijuProgramos()
        {
            InitializeComponent();
            LoadPrograms();
            dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
        }

        private void LoadPrograms()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID, pavadinimas FROM studiju_programos";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                DataTable programsTable = new DataTable();
                adapter.Fill(programsTable);

                dataGridView1.DataSource = programsTable;
                dataGridView1.Columns["ID"].Visible = false; // Paslepia ID stulpelį
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string programName = textBox1.Text.Trim();
            if (string.IsNullOrWhiteSpace(programName))
            {
                MessageBox.Show("Įveskite studijų programos pavadinimą.");
                return;
            }

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO studiju_programos (pavadinimas) VALUES (@programName)";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@programName", programName);
                command.ExecuteNonQuery();
            }

            MessageBox.Show("Studijų programa pridėta.");
            LoadPrograms();
            textBox1.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int programId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);
                string newProgramName = textBox1.Text.Trim();

                if (string.IsNullOrWhiteSpace(newProgramName))
                {
                    MessageBox.Show("Įveskite naują studijų programos pavadinimą.");
                    return;
                }

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE studiju_programos SET pavadinimas = @newProgramName WHERE ID = @programId";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@newProgramName", newProgramName);
                    command.Parameters.AddWithValue("@programId", programId);
                    command.ExecuteNonQuery();
                }

                MessageBox.Show("Studijų programa atnaujinta.");
                LoadPrograms();
                textBox1.Clear();
            }
            else
            {
                MessageBox.Show("Pasirinkite studijų programą redagavimui.");
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int programId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);

                DialogResult result = MessageBox.Show("Ar tikrai norite pašalinti šią studijų programą?", "Patvirtinimas", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        string query = "DELETE FROM studiju_programos WHERE ID = @programId";
                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@programId", programId);
                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Studijų programa pašalinta.");
                    LoadPrograms();
                }
            }
            else
            {
                MessageBox.Show("Pasirinkite studijų programą šalinimui.");
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // Užpildome pasirinktos studijų programos pavadinimą į tekstinį lauką
                string programName = dataGridView1.SelectedRows[0].Cells["pavadinimas"].Value.ToString();
                textBox1.Text = programName;
            }
            else
            {
                // Jei nieko nepasirinkta, išvalome tekstinį lauką
                textBox1.Clear();
            }

        }
    }
}
