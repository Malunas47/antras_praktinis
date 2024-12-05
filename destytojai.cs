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
    public partial class destytojai : Form
    {
        public destytojai()
        {
            InitializeComponent();
            LoadTeachers(); // Užkrauname dėstytojų sąrašą
        }

        private void LoadTeachers()
        {
            using (MySqlConnection connection = new MySqlConnection(Program.ConnectionString))
            {
                connection.Open();
                string query = @"
            SELECT d.ID, d.vardas, d.pavarde, n.username 
            FROM destytojai d
            JOIN naudotojai n ON d.naudotojai_ID = n.ID";

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

                textBox1.Text = selectedRow.Cells["vardas"].Value != DBNull.Value
                    ? selectedRow.Cells["vardas"].Value.ToString()
                    : string.Empty;

                textBox2.Text = selectedRow.Cells["pavarde"].Value != DBNull.Value
                    ? selectedRow.Cells["pavarde"].Value.ToString()
                    : string.Empty;
            }
            else
            {
                textBox1.Clear();
                textBox2.Clear();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string firstName = textBox1.Text.Trim(); // Dėstytojo vardas
            string lastName = textBox2.Text.Trim();  // Dėstytojo pavardė

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                MessageBox.Show("Įveskite dėstytojo vardą ir pavardę.");
                return;
            }

            using (MySqlConnection connection = new MySqlConnection(Program.ConnectionString))
            {
                connection.Open();

                // 1. Sukuriame prisijungimo vardą ir slaptažodį (mažosiomis raidėmis)
                string username = firstName.ToLower();
                string password = lastName.ToLower();
                string role = "destytojas";

                // 2. Įrašome naudotoją į lentelę "naudotojai"
                string userQuery = @"
            INSERT INTO naudotojai (username, password, role) 
            VALUES (@username, @password, @role)";
                MySqlCommand userCommand = new MySqlCommand(userQuery, connection);
                userCommand.Parameters.AddWithValue("@username", username);
                userCommand.Parameters.AddWithValue("@password", password);
                userCommand.Parameters.AddWithValue("@role", role);
                userCommand.ExecuteNonQuery();

                // 3. Gauti naudotojai_ID iš lentelės "naudotojai"
                long userId = userCommand.LastInsertedId;

                // 4. Įrašome dėstytojo duomenis į lentelę "destytojai"
                string teacherQuery = @"
            INSERT INTO destytojai (vardas, pavarde, naudotojai_ID) 
            VALUES (@firstName, @lastName, @userId)";
                MySqlCommand teacherCommand = new MySqlCommand(teacherQuery, connection);
                teacherCommand.Parameters.AddWithValue("@firstName", firstName);
                teacherCommand.Parameters.AddWithValue("@lastName", lastName);
                teacherCommand.Parameters.AddWithValue("@userId", userId);
                teacherCommand.ExecuteNonQuery();

                MessageBox.Show("Dėstytojas sėkmingai pridėtas.");
            }

            // Išvalome įvesties laukus
            textBox1.Clear();
            textBox2.Clear();

            // Atnaujiname dėstytojų sąrašą
            LoadTeachers();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int teacherId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);
                string newFirstName = textBox1.Text.Trim();
                string newLastName = textBox2.Text.Trim();

                if (string.IsNullOrWhiteSpace(newFirstName) || string.IsNullOrWhiteSpace(newLastName))
                {
                    MessageBox.Show("Įveskite vardą ir pavardę.");
                    return;
                }

                using (MySqlConnection connection = new MySqlConnection(Program.ConnectionString))
                {
                    connection.Open();

                    // Atnaujiname dėstytojo duomenis lentelėje "destytojai"
                    string updateTeacherQuery = "UPDATE destytojai SET vardas = @newFirstName, pavarde = @newLastName WHERE ID = @teacherId";
                    MySqlCommand updateTeacherCommand = new MySqlCommand(updateTeacherQuery, connection);
                    updateTeacherCommand.Parameters.AddWithValue("@newFirstName", newFirstName);
                    updateTeacherCommand.Parameters.AddWithValue("@newLastName", newLastName);
                    updateTeacherCommand.Parameters.AddWithValue("@teacherId", teacherId);
                    updateTeacherCommand.ExecuteNonQuery();

                    // Sugeneruojame naują prisijungimo vardą ir slaptažodį
                    string newUsername = newFirstName.ToLower();
                    string newPassword = newLastName.ToLower();

                    // Atnaujiname prisijungimo vardą ir slaptažodį lentelėje "naudotojai"
                    string updateUserQuery = @"
                UPDATE naudotojai 
                SET username = @newUsername, password = @newPassword 
                WHERE ID = (SELECT naudotojai_ID FROM destytojai WHERE ID = @teacherId)";
                    MySqlCommand updateUserCommand = new MySqlCommand(updateUserQuery, connection);
                    updateUserCommand.Parameters.AddWithValue("@newUsername", newUsername);
                    updateUserCommand.Parameters.AddWithValue("@newPassword", newPassword);
                    updateUserCommand.Parameters.AddWithValue("@teacherId", teacherId);
                    updateUserCommand.ExecuteNonQuery();
                }

                MessageBox.Show("Dėstytojo duomenys ir prisijungimo informacija atnaujinti.");
                LoadTeachers(); // Atnaujina dėstytojų sąrašą
            }
            else
            {
                MessageBox.Show("Pasirinkite dėstytoją iš sąrašo.");
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int teacherId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);

                DialogResult result = MessageBox.Show("Ar tikrai norite pašalinti šį dėstytoją ir jo prisijungimo duomenis?", "Patvirtinimas", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    using (MySqlConnection connection = new MySqlConnection(Program.ConnectionString))
                    {
                        connection.Open();

                        // Gauti naudotojai_ID
                        string getUserIdQuery = "SELECT naudotojai_ID FROM destytojai WHERE ID = @teacherId";
                        MySqlCommand getUserIdCommand = new MySqlCommand(getUserIdQuery, connection);
                        getUserIdCommand.Parameters.AddWithValue("@teacherId", teacherId);
                        int userId = Convert.ToInt32(getUserIdCommand.ExecuteScalar());

                        // Pašaliname naudotoją
                        string deleteUserQuery = "DELETE FROM naudotojai WHERE ID = @userId";
                        MySqlCommand deleteUserCommand = new MySqlCommand(deleteUserQuery, connection);
                        deleteUserCommand.Parameters.AddWithValue("@userId", userId);
                        deleteUserCommand.ExecuteNonQuery();

                        // Pašaliname dėstytoją
                        string deleteTeacherQuery = "DELETE FROM destytojai WHERE ID = @teacherId";
                        MySqlCommand deleteTeacherCommand = new MySqlCommand(deleteTeacherQuery, connection);
                        deleteTeacherCommand.Parameters.AddWithValue("@teacherId", teacherId);
                        deleteTeacherCommand.ExecuteNonQuery();
                    }

                    MessageBox.Show("Dėstytojas pašalintas.");
                    LoadTeachers();
                }
            }
            else
            {
                MessageBox.Show("Pasirinkite dėstytoją iš sąrašo.");
            }
        }
    }
}
