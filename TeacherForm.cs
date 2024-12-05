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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
namespace antras_praktinis
{
    public partial class TeacherForm : Form
    {
        private int teacherId;

        public TeacherForm(int userId)
        {
            InitializeComponent();
            this.teacherId = GetTeacherId(userId);
            LoadGroups(); // Pradinis grupių užkrovimas
            LoadTeacherInfo(); // Užkrauna dėstytojo vardą ir pavardę
            LoadGradeTypes(); // Užkrauti `pazymio_tipas`
        }

        private void LoadGradeTypes()
        {
            using (MySqlConnection connection = new MySqlConnection(Program.ConnectionString))
            {
                connection.Open();

                // Užklausą, kad gautų ENUM reikšmes iš `pazymio_tipas`
                string query = @"
            SELECT COLUMN_TYPE 
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = 'pazymiai' AND COLUMN_NAME = 'pazymio_tipas'";

                MySqlCommand command = new MySqlCommand(query, connection);
                object result = command.ExecuteScalar();

                if (result != null)
                {
                    // Pašalina ENUM reikšmių formatą iš duomenų bazės ('kontrolinis','praktinis','testas')
                    string enumValues = result.ToString();
                    enumValues = enumValues.Replace("enum(", "").Replace(")", "").Replace("'", "");

                    // Padalina reikšmes į sąrašą ir prideda į ComboBox
                    string[] types = enumValues.Split(',');
                    comboBox2.Items.Clear();
                    comboBox2.Items.AddRange(types);
                }

                comboBox2.SelectedIndex = -1; // Nėra pasirinkimo
            }
        }

        private int GetTeacherId(int userId)
        {
            using (MySqlConnection connection = new MySqlConnection(Program.ConnectionString))
            {
                connection.Open();
                string query = "SELECT ID FROM destytojai WHERE naudotojai_ID = @userId";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@userId", userId);

                object result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        private void LoadTeacherInfo()
        {
            using (MySqlConnection connection = new MySqlConnection(Program.ConnectionString))
            {
                connection.Open();
                string query = "SELECT vardas, pavarde FROM destytojai WHERE ID = @teacherId";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@teacherId", teacherId);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        textBox1.Text = reader["vardas"].ToString();
                        textBox2.Text = reader["pavarde"].ToString();
                    }
                }
            }
        }

        private void LoadGroups()
        {
            using (MySqlConnection connection = new MySqlConnection(Program.ConnectionString))
            {
                connection.Open();
                string query = @"
            SELECT DISTINCT g.ID, g.pavadinimas
            FROM grupes g
            INNER JOIN destytojo_dalykai dd ON g.ID = dd.grupes_ID
            WHERE dd.destytojas_ID = @teacherId";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@teacherId", teacherId);

                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable groupsTable = new DataTable();
                adapter.Fill(groupsTable);

                comboBox1.DataSource = groupsTable;
                comboBox1.DisplayMember = "pavadinimas";
                comboBox1.ValueMember = "ID";
                comboBox1.SelectedIndex = -1;
            }
        }

        private void LoadStudents(int groupId)
        {
            using (MySqlConnection connection = new MySqlConnection(Program.ConnectionString))
            {
                connection.Open();
                string query = @"
        SELECT s.ID, s.vardas AS 'Vardas', s.pavarde AS 'Pavardė'
        FROM studentai s
        WHERE s.grupes_ID = @groupId";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@groupId", groupId);

                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable studentsTable = new DataTable();
                adapter.Fill(studentsTable);

                dataGridView1.DataSource = studentsTable;
            }
        }

        private void LoadGrades(int studentId)
        {
            using (MySqlConnection connection = new MySqlConnection(Program.ConnectionString))
            {
                connection.Open();
                string query = @"
        SELECT 
            p.pazymys AS 'Pažymys', 
            p.date AS 'Data', 
            p.pazymio_tipas AS 'Tipas' 
        FROM pazymiai p
        WHERE p.studentai_ID = @studentId";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@studentId", studentId);

                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);

                dataGridView2.DataSource = table;
            }
        }

        private void LoadSubjectForGroup(int groupId)
        {
            using (MySqlConnection connection = new MySqlConnection(Program.ConnectionString))
            {
                connection.Open();
                string query = @"
        SELECT dal.pavadinimas 
        FROM destomi_dalykai dal
        INNER JOIN destytojo_dalykai dd ON dal.ID = dd.dalykas_ID
        WHERE dd.grupes_ID = @groupId AND dd.destytojas_ID = @teacherId";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@groupId", groupId);
                command.Parameters.AddWithValue("@teacherId", teacherId);

                object result = command.ExecuteScalar();
                textBox4.Text = result != null ? result.ToString() : "Nėra dėstomo dalyko";
            }
        }

        private void LoadStudentGrades(int studentId)
        {
            using (MySqlConnection connection = new MySqlConnection(Program.ConnectionString))
            {
                connection.Open();
                string query = @"
            SELECT 
                p.ID AS 'GradeID', 
                p.pazymys AS 'Pazymys', 
                p.date AS 'Date', 
                p.pazymio_tipas AS 'Type' 
            FROM pazymiai p
            WHERE p.studentai_ID = @studentId";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@studentId", studentId);

                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);

                dataGridView2.DataSource = table; // Priskiriame duomenis DataGridView
            }
        }



        private void button1_Click(object sender, EventArgs e)
        {
            Application.Restart(); // Iš naujo paleidžia programą
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue != null)
            {
                int groupId = Convert.ToInt32(comboBox1.SelectedValue);
                LoadStudents(groupId); // Užkrauna studentus
                LoadSubjectForGroup(groupId); // Užkrauna dėstomą dalyką
                dataGridView2.DataSource = null; // Išvalo pažymius
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0 && comboBox2.SelectedItem != null && !string.IsNullOrWhiteSpace(textBox3.Text))
            {
                try
                {
                    int studentId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);
                    decimal grade;
                    if (!decimal.TryParse(textBox3.Text.Trim(), out grade))
                    {
                        MessageBox.Show("Įveskite tinkamą pažymį.");
                        return;
                    }

                    string gradeType = comboBox2.SelectedItem.ToString(); // Pažymio tipas
                    int groupId = Convert.ToInt32(comboBox1.SelectedValue);

                    using (MySqlConnection connection = new MySqlConnection(Program.ConnectionString))
                    {
                        connection.Open();
                        string query = @"
                INSERT INTO pazymiai (pazymys, date, studentai_ID, destomi_dalykai_ID, pazymio_tipas)
                VALUES (
                    @grade, NOW(), @studentId,
                    (SELECT dd.dalykas_ID 
                     FROM destytojo_dalykai dd
                     WHERE dd.grupes_ID = @groupId AND dd.destytojas_ID = @teacherId),
                    @gradeType)";
                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@grade", grade);
                        command.Parameters.AddWithValue("@studentId", studentId);
                        command.Parameters.AddWithValue("@groupId", groupId);
                        command.Parameters.AddWithValue("@teacherId", teacherId);
                        command.Parameters.AddWithValue("@gradeType", gradeType);
                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Pažymys pridėtas.");

                    // Atnaujiname pažymių lentelę
                    LoadStudentGrades(studentId);

                    // Išvalome laukus
                    comboBox2.SelectedIndex = -1; // Pažymio tipas
                    textBox3.Clear(); // Pažymio vertė
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Įvyko klaida: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Pasirinkite pažymio tipą ir įveskite pažymį.");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0 && comboBox2.SelectedItem != null && !string.IsNullOrWhiteSpace(textBox3.Text))
            {
                try
                {
                    // Pažymio ID paėmimas
                    int gradeId = Convert.ToInt32(dataGridView2.SelectedRows[0].Cells["GradeID"].Value);

                    // Naujo pažymio ir tipo tikrinimas
                    decimal grade;
                    if (!decimal.TryParse(textBox3.Text.Trim(), out grade))
                    {
                        MessageBox.Show("Įveskite tinkamą pažymį.");
                        return;
                    }

                    string gradeType = comboBox2.SelectedItem.ToString(); // Pažymio tipas iš ComboBox

                    // Atnaujiname pažymį duomenų bazėje
                    using (MySqlConnection connection = new MySqlConnection(Program.ConnectionString))
                    {
                        connection.Open();
                        string query = "UPDATE pazymiai SET pazymys = @grade, pazymio_tipas = @gradeType, date = NOW() WHERE ID = @gradeId";
                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@grade", grade);
                        command.Parameters.AddWithValue("@gradeType", gradeType);
                        command.Parameters.AddWithValue("@gradeId", gradeId);
                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Pažymys atnaujintas.");

                    // Atnaujiname pažymių lentelę
                    if (dataGridView1.SelectedRows.Count > 0)
                    {
                        int studentId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);
                        LoadStudentGrades(studentId); // Užkrauname pažymius
                    }

                    // Išvalome laukus
                    comboBox2.SelectedIndex = -1; // Pažymio tipas
                    textBox3.Clear(); // Pažymio vertė
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Įvyko klaida: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Pasirinkite pažymį ir įveskite naują informaciją.");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0)
            {
                try
                {
                    // Paėmimas konkretaus pažymio ID
                    int gradeId = Convert.ToInt32(dataGridView2.SelectedRows[0].Cells["GradeID"].Value);

                    using (MySqlConnection connection = new MySqlConnection(Program.ConnectionString))
                    {
                        connection.Open();

                        // Pašaliname pažymį pagal ID
                        string query = "DELETE FROM pazymiai WHERE ID = @gradeId";
                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@gradeId", gradeId);
                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Pažymys pašalintas.");

                    // Atnaujiname pažymių lentelę
                    if (dataGridView1.SelectedRows.Count > 0)
                    {
                        int studentId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);
                        LoadStudentGrades(studentId); // Atnaujina pažymių lentelę
                    }

                    // Išvalome laukus
                    comboBox2.SelectedIndex = -1; // Pažymio tipas
                    textBox3.Clear(); // Pažymio vertė
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Įvyko klaida: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Pasirinkite pažymį, kurį norite pašalinti.");
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];

                // Užpildome studento vardą
                textBox5.Text = selectedRow.Cells["vardas"].Value != DBNull.Value
                    ? selectedRow.Cells["vardas"].Value.ToString()
                    : string.Empty;

                // Užpildome studento pavardę
                textBox6.Text = selectedRow.Cells["pavarde"].Value != DBNull.Value
                    ? selectedRow.Cells["pavarde"].Value.ToString()
                    : string.Empty;

                // Užkrauname pažymius
                int studentId = Convert.ToInt32(selectedRow.Cells["ID"].Value); // Patikrinkite, ar "ID" yra teisingas stulpelio pavadinimas
                LoadStudentGrades(studentId); // Kvieskite metodą, kuris užkrauna pažymius į DataGridView2
            }
            else
            {
                // Jei nėra pasirinktos eilutės, išvalome laukus
                textBox5.Clear();
                textBox6.Clear();
                dataGridView2.DataSource = null; // Išvalome pažymių lentelę
            }
        }

        private void dataGridView2_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView2.SelectedRows[0];

                // Užpildome pažymio vertę į textBox3
                if (selectedRow.Cells["Pazymys"].Value != DBNull.Value)
                {
                    textBox3.Text = selectedRow.Cells["Pazymys"].Value.ToString();
                }
                else
                {
                    textBox3.Clear();
                }

                // Užpildome pažymio tipą į comboBox2
                if (selectedRow.Cells["Type"].Value != DBNull.Value)
                {
                    string gradeType = selectedRow.Cells["Type"].Value.ToString();
                    comboBox2.SelectedItem = gradeType;
                }
                else
                {
                    comboBox2.SelectedIndex = -1;
                }
            }
            else
            {
                // Jei nėra pasirinktos eilutės, išvalome laukus
                textBox3.Clear();
                comboBox2.SelectedIndex = -1;
            }
        }



    }
}
