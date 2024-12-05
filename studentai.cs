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
    public partial class studentai : Form
    {
        private string connectionString = Program.ConnectionString; // Centralizuotas ryšys

        public studentai()
        {
            InitializeComponent();
            LoadPrograms();  // Užkrauna studijų programas į comboBox2
            LoadStudents();  // Užkrauna studentus į DataGridView
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

                comboBox2.DataSource = table;
                comboBox2.DisplayMember = "pavadinimas"; // Studijų programos pavadinimas
                comboBox2.ValueMember = "ID";           // Studijų programos ID
                comboBox2.SelectedIndex = -1;           // Nieko nepasirinkta pagal nutylėjimą
            }
        }

        // Užkrauna grupių sąrašą į ComboBox
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

                comboBox1.DataSource = table;
                comboBox1.DisplayMember = "pavadinimas";
                comboBox1.ValueMember = "ID";
                comboBox1.SelectedIndex = -1;
            }
        }


        // Užkrauna studentų sąrašą į DataGridView
        private void LoadStudents()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
            SELECT 
                s.ID, 
                s.vardas, 
                s.pavarde, 
                s.grupes_ID, 
                g.pavadinimas AS grupes_pavadinimas, 
                g.studiju_programa_ID, 
                sp.pavadinimas AS studiju_programa_pavadinimas
            FROM studentai s
            JOIN grupes g ON s.grupes_ID = g.ID
            JOIN studiju_programos sp ON g.studiju_programa_ID = sp.ID";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                DataTable table = new DataTable();
                adapter.Fill(table);
                dataGridView1.DataSource = table;
            }
        }




        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];

                // Užpildome vardą ir pavardę
                textBox1.Text = selectedRow.Cells["vardas"].Value != DBNull.Value
                    ? selectedRow.Cells["vardas"].Value.ToString()
                    : string.Empty;

                textBox2.Text = selectedRow.Cells["pavarde"].Value != DBNull.Value
                    ? selectedRow.Cells["pavarde"].Value.ToString()
                    : string.Empty;

                // Pasirenkame studijų programos ID
                if (selectedRow.Cells["studiju_programa_ID"].Value != DBNull.Value)
                {
                    int programId = Convert.ToInt32(selectedRow.Cells["studiju_programa_ID"].Value);
                    comboBox2.SelectedValue = programId;
                    LoadGroups(programId); // Užkrauna grupes pagal studijų programą
                }
                else
                {
                    comboBox2.SelectedIndex = -1;
                    comboBox1.DataSource = null; // Išvalome grupių pasirinkimą
                }

                // Pasirenkame grupės ID
                if (selectedRow.Cells["grupes_ID"].Value != DBNull.Value)
                {
                    comboBox1.SelectedValue = Convert.ToInt32(selectedRow.Cells["grupes_ID"].Value);
                }
                else
                {
                    comboBox1.SelectedIndex = -1;
                }
            }
            else
            {
                textBox1.Clear();
                textBox2.Clear();
                comboBox1.SelectedIndex = -1;
                comboBox2.SelectedIndex = -1;
            }
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        // Pridėti naują studentą
        private void button1_Click(object sender, EventArgs e)
        {
            string firstName = textBox1.Text.Trim(); // Vardas originaliu formatu
            string lastName = textBox2.Text.Trim();  // Pavardė originaliu formatu
            int groupId = Convert.ToInt32(comboBox1.SelectedValue);

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                MessageBox.Show("Įveskite vardą ir pavardę.");
                return;
            }

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // 1. Sukuriame prisijungimo vardą ir slaptažodį (mažosiomis raidėmis)
                string username = firstName.ToLower();  // Prisijungimo vardas – vardas mažosiomis raidėmis
                string password = lastName.ToLower();  // Slaptažodis – pavardė mažosiomis raidėmis
                string role = "studentas";             // Rolė – "studentas"

                // 2. Įrašome naudotojo informaciją į lentelę "naudotojai"
                string userQuery = @"INSERT INTO naudotojai (username, password, role) VALUES (@username, @password, @role)";
                MySqlCommand userCommand = new MySqlCommand(userQuery, connection);
                userCommand.Parameters.AddWithValue("@username", username);
                userCommand.Parameters.AddWithValue("@password", password);
                userCommand.Parameters.AddWithValue("@role", role);
                userCommand.ExecuteNonQuery();

                // 3. Gauti įrašyto naudotojo ID
                long userId = userCommand.LastInsertedId;

                // 4. Įrašome studentą į lentelę "studentai" su gautu naudotojai_ID
                string studentQuery = "INSERT INTO studentai (vardas, pavarde, grupes_ID, naudotojai_ID) VALUES (@firstName, @lastName, @groupId, @userId)";
                MySqlCommand studentCommand = new MySqlCommand(studentQuery, connection);
                studentCommand.Parameters.AddWithValue("@firstName", firstName);
                studentCommand.Parameters.AddWithValue("@lastName", lastName);
                studentCommand.Parameters.AddWithValue("@groupId", groupId);
                studentCommand.Parameters.AddWithValue("@userId", userId);
                studentCommand.ExecuteNonQuery();
            }

            LoadStudents(); // Atnaujina studentų lentelę
            textBox1.Clear();
            textBox2.Clear();
        }

        // Redaguoti pasirinktą studentą
        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int studentId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);
                string newFirstName = textBox1.Text.Trim(); // Naujas vardas (originalus formatas)
                string newLastName = textBox2.Text.Trim();  // Nauja pavardė (originalus formatas)
                int newGroupId = Convert.ToInt32(comboBox1.SelectedValue);

                if (string.IsNullOrWhiteSpace(newFirstName) || string.IsNullOrWhiteSpace(newLastName))
                {
                    MessageBox.Show("Įveskite vardą ir pavardę.");
                    return;
                }

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // 1. Atnaujiname studento informaciją lentelėje "studentai"
                    string updateStudentQuery = "UPDATE studentai SET vardas = @newFirstName, pavarde = @newLastName, grupes_ID = @newGroupId WHERE ID = @studentId";
                    MySqlCommand updateStudentCommand = new MySqlCommand(updateStudentQuery, connection);
                    updateStudentCommand.Parameters.AddWithValue("@newFirstName", newFirstName);
                    updateStudentCommand.Parameters.AddWithValue("@newLastName", newLastName);
                    updateStudentCommand.Parameters.AddWithValue("@newGroupId", newGroupId);
                    updateStudentCommand.Parameters.AddWithValue("@studentId", studentId);
                    updateStudentCommand.ExecuteNonQuery();

                    // 2. Gauti naudotojai_ID iš lentelės "studentai"
                    string getUserIdQuery = "SELECT naudotojai_ID FROM studentai WHERE ID = @studentId";
                    MySqlCommand getUserIdCommand = new MySqlCommand(getUserIdQuery, connection);
                    getUserIdCommand.Parameters.AddWithValue("@studentId", studentId);
                    object userIdObj = getUserIdCommand.ExecuteScalar();

                    if (userIdObj != null)
                    {
                        int userId = Convert.ToInt32(userIdObj);

                        // 3. Sugeneruojame prisijungimo vardą ir slaptažodį (mažosiomis raidėmis)
                        string newUsername = newFirstName.ToLower();  // Prisijungimo vardas mažosiomis raidėmis
                        string newPassword = newLastName.ToLower();  // Slaptažodis mažosiomis raidėmis

                        // 4. Atnaujiname prisijungimo duomenis lentelėje "naudotojai"
                        string updateUserQuery = @"UPDATE naudotojai SET username = @newUsername, password = @newPassword WHERE ID = @userId";
                        MySqlCommand updateUserCommand = new MySqlCommand(updateUserQuery, connection);
                        updateUserCommand.Parameters.AddWithValue("@newUsername", newUsername);
                        updateUserCommand.Parameters.AddWithValue("@newPassword", newPassword);
                        updateUserCommand.Parameters.AddWithValue("@userId", userId);
                        updateUserCommand.ExecuteNonQuery();
                    }
                }

                LoadStudents(); // Atnaujina lentelę
                textBox1.Clear();
                textBox2.Clear();
            }
            else
            {
                MessageBox.Show("Pasirinkite studentą iš sąrašo.");
            }
        }


        // Pašalinti pasirinktą studentą
        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int studentId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);

                DialogResult result = MessageBox.Show("Ar tikrai norite pašalinti šį studentą ir jo prisijungimo duomenis?", "Patvirtinimas", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();

                        // 1. Gauti naudotojai_ID iš lentelės studentai
                        string getUserIdQuery = "SELECT naudotojai_ID FROM studentai WHERE ID = @studentId";
                        MySqlCommand getUserIdCommand = new MySqlCommand(getUserIdQuery, connection);
                        getUserIdCommand.Parameters.AddWithValue("@studentId", studentId);
                        object userIdObj = getUserIdCommand.ExecuteScalar();

                        if (userIdObj != null)
                        {
                            int userId = Convert.ToInt32(userIdObj);

                            // 2. Ištriname naudotoją iš lentelės naudotojai
                            string deleteUserQuery = "DELETE FROM naudotojai WHERE ID = @userId";
                            MySqlCommand deleteUserCommand = new MySqlCommand(deleteUserQuery, connection);
                            deleteUserCommand.Parameters.AddWithValue("@userId", userId);
                            deleteUserCommand.ExecuteNonQuery();
                        }

                        // 3. Ištriname studentą iš lentelės studentai
                        string deleteStudentQuery = "DELETE FROM studentai WHERE ID = @studentId";
                        MySqlCommand deleteStudentCommand = new MySqlCommand(deleteStudentQuery, connection);
                        deleteStudentCommand.Parameters.AddWithValue("@studentId", studentId);
                        deleteStudentCommand.ExecuteNonQuery();
                    }

                    LoadStudents(); // Atnaujina lentelę
                }
            }
            else
            {
                MessageBox.Show("Pasirinkite studentą iš sąrašo.");
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedValue != null && int.TryParse(comboBox2.SelectedValue.ToString(), out int programId))
            {
                LoadGroups(programId);
            }
            else
            {
                comboBox1.DataSource = null;
            }
        }
    }
}
