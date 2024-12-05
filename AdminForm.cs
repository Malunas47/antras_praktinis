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
    public partial class AdminForm : Form
    {
        private int adminId;
        public AdminForm(int userId)
        {
            InitializeComponent();
            this.adminId = userId;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            Application.Restart(); // Iš naujo paleidžia programą
        }

        private void button1_Click(object sender, EventArgs e)
        {
            studentuGrupes groupForm = new studentuGrupes();
            groupForm.ShowDialog(); // Atidaro grupių valdymo formą
        }

        private void button4_Click(object sender, EventArgs e)
        {
            studentai studentForm = new studentai();
            studentForm.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            destytojai addTeacherForm = new destytojai();
            addTeacherForm.ShowDialog(); // Atidarome formą
        }

        private void button7_Click(object sender, EventArgs e)
        {
            studijuProgramos programForm = new studijuProgramos();
            programForm.ShowDialog(); // Atidaro studijų programų formą kaip dialogą
        }

        private void button2_Click(object sender, EventArgs e)
        {
            destomiDalykai destomiDalykaiForm = new destomiDalykai();
            destomiDalykaiForm.ShowDialog(); // Atidarome kaip modalinę formą
        }

        private void button5_Click(object sender, EventArgs e)
        {
            priskirtiDestytoja form = new priskirtiDestytoja();
            form.ShowDialog();
        }
    }
}
