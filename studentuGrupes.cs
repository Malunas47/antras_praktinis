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
    public partial class studentuGrupes : Form
    {
        public studentuGrupes()
        {
            InitializeComponent();
            LoadGroups(); // Užkrauna grupių sąrašą, kai forma atidaroma
            LoadPrograms();
            dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
        }

        // Užkrauna grupių sąrašą į DataGridView
        private void LoadGroups()
        {
            using (MySqlConnection connection = new MySqlConnection(Program.ConnectionString))
            {
                string query = @"
            SELECT g.id, g.pavadinimas AS Grupė, sp.pavadinimas AS 'Studijų programa'
            FROM grupes g
            JOIN studiju_programos sp ON g.studiju_programa_ID = sp.ID";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                DataTable table = new DataTable();
                adapter.Fill(table);
                dataGridView1.DataSource = table;
            }
        }

        private void LoadPrograms()
        {
            using (MySqlConnection connection = new MySqlConnection(Program.ConnectionString))
            {
                connection.Open();
                string query = "SELECT ID, pavadinimas FROM studiju_programos";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                DataTable programsTable = new DataTable();
                adapter.Fill(programsTable);

                comboBox1.DataSource = programsTable;
                comboBox1.DisplayMember = "pavadinimas";
                comboBox1.ValueMember = "ID";

                // Nustatome, kad niekas nebūtų pasirinkta
                comboBox1.SelectedIndex = -1;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        // Pridėti naują grupę
        private void button1_Click(object sender, EventArgs e)
        {
            string groupName = textBox1.Text.Trim();
            int programId = comboBox1.SelectedValue != null ? Convert.ToInt32(comboBox1.SelectedValue) : 0;

            if (!string.IsNullOrWhiteSpace(groupName) && programId > 0)
            {
                using (MySqlConnection connection = new MySqlConnection(Program.ConnectionString))
                {
                    string query = "INSERT INTO grupes (pavadinimas, studiju_programa_ID) VALUES (@groupName, @programId)";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@groupName", groupName);
                    command.Parameters.AddWithValue("@programId", programId);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                LoadGroups(); // Atnaujina lentelę
                textBox1.Clear(); // Išvalo įvesties lauką
                comboBox1.SelectedIndex = -1; // Pašalina pasirinkimą
            }
            else
            {
                MessageBox.Show("Įveskite grupės pavadinimą ir pasirinkite studijų programą.");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int groupId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["id"].Value);
                string newGroupName = textBox1.Text.Trim();
                int newProgramId = comboBox1.SelectedValue != null ? Convert.ToInt32(comboBox1.SelectedValue) : 0;

                if (!string.IsNullOrWhiteSpace(newGroupName) && newProgramId > 0)
                {
                    using (MySqlConnection connection = new MySqlConnection(Program.ConnectionString))
                    {
                        string query = "UPDATE grupes SET pavadinimas = @newGroupName, studiju_programa_ID = @newProgramId WHERE id = @groupId";
                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@newGroupName", newGroupName);
                        command.Parameters.AddWithValue("@newProgramId", newProgramId);
                        command.Parameters.AddWithValue("@groupId", groupId);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    LoadGroups(); // Atnaujina lentelę
                    textBox1.Clear();
                    comboBox1.SelectedIndex = -1;
                }
                else
                {
                    MessageBox.Show("Įveskite naują grupės pavadinimą ir pasirinkite studijų programą.");
                }
            }
            else
            {
                MessageBox.Show("Pasirinkite grupę redagavimui.");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["id"].Value);

                DialogResult result = MessageBox.Show("Ar tikrai norite pašalinti šią grupę?", "Patvirtinimas", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    using (MySqlConnection connection = new MySqlConnection(Program.ConnectionString))
                    {
                        string query = "DELETE FROM grupes WHERE id = @id";
                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@id", id);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    LoadGroups(); // Atnaujina lentelę
                }
            }
            else
            {
                MessageBox.Show("Pasirinkite grupę iš sąrašo.");
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // Gauname pasirinktą eilutę
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];

                // Užpildome ComboBox ir TextBox reikšmes
                textBox1.Text = selectedRow.Cells["Grupe"].Value.ToString();
                comboBox1.Text = selectedRow.Cells["Studiju programa"].Value.ToString();
            }
            else
            {
                // Išvalome, jei niekas nepasirinkta
                textBox1.Clear();
                comboBox1.SelectedIndex = -1;
            }
        }

    }
}
