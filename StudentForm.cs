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
    public partial class StudentForm : Form
    {
        private string connectionString = Program.ConnectionString; // Duomenų bazės ryšys
        private int studentId; // Student ID, perduodamas iš prisijungimo

        // Konstruktoriumi perduodamas studento ID
        // Konstruktoriumi perduodamas naudotojo ID
        public StudentForm(int userId)
        {
            InitializeComponent();
            // Gaukime studento ID pagal perduotą naudotojo ID
            studentId = GetStudentId(userId);
            LoadStudentData();
            LoadSubjects();
        }

        private int GetStudentId(int userId)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT ID FROM studentai WHERE naudotojai_ID = @userId";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@userId", userId);

                    object result = command.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Klaida gaunant studento ID: {ex.Message}");
                return 0;
            }
        }

        private void LoadSubjects()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Užklausa parenka studento grupės paskaitas per destytojo_dalykai lentelę
                    string query = @"
                    SELECT dd.ID AS SubjectID, dd.pavadinimas AS SubjectName
                    FROM destomi_dalykai dd
                    INNER JOIN destytojo_dalykai ddd ON dd.ID = ddd.dalykas_ID
                    INNER JOIN grupes g ON ddd.grupes_ID = g.ID
                    WHERE g.ID = (SELECT grupes_ID FROM studentai WHERE ID = @studentId)";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@studentId", studentId); // Naudojame studento ID

                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    dataGridView1.DataSource = table;

                    dataGridView1.Columns["SubjectID"].Visible = false; // Paslepia ID stulpelį
                    dataGridView1.Columns["SubjectName"].HeaderText = "Paskaitos pavadinimas";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Klaida užkraunant paskaitas: " + ex.Message);
            }
        }

        private void LoadGrades(int subjectId)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                    SELECT 
                        p.pazymys AS Grade, 
                        p.date AS Date, 
                        p.pazymio_tipas AS GradeType
                    FROM pazymiai p
                    WHERE p.studentai_ID = @studentId AND p.destomi_dalykai_ID = @subjectId";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@studentId", studentId); // Naudojame studento ID
                    command.Parameters.AddWithValue("@subjectId", subjectId);

                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    dataGridView2.DataSource = table;

                    dataGridView2.Columns["Grade"].HeaderText = "Pažymys";
                    dataGridView2.Columns["Date"].HeaderText = "Data";
                    dataGridView2.Columns["GradeType"].HeaderText = "Tipas";

                    if (table.Rows.Count == 0)
                    {
                        MessageBox.Show("Pažymių nerasta šiai paskaitai.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Klaida užkraunant pažymius: " + ex.Message);
            }
        }

        private void LoadStudentData()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    string query = @"
                    SELECT s.vardas, s.pavarde, g.pavadinimas AS grupes_pavadinimas 
                    FROM studentai s 
                    LEFT JOIN grupes g ON s.grupes_ID = g.ID 
                    WHERE s.ID = @studentId";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@studentId", studentId); // Dabar perduodamas studento ID
                    connection.Open();

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            textBox1.Text = reader["vardas"].ToString();       // Studentas vardas
                            textBox2.Text = reader["pavarde"].ToString();      // Studentas pavardė
                            textBox3.Text = reader["grupes_pavadinimas"].ToString(); // Grupė
                        }
                        else
                        {
                            MessageBox.Show("Studento duomenys nerasti.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Klaida užkraunant studento duomenis: {ex.Message}");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Restart(); // Iš naujo paleidžia programą
        }

        private void StudentForm_Load(object sender, EventArgs e)
        {

        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int subjectId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["SubjectID"].Value);
                LoadGrades(subjectId);
            }
            else
            {
                dataGridView2.DataSource = null;
            }
        }
    }
}
