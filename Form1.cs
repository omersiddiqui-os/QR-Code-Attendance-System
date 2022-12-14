using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MessagingToolkit.Barcode;
using BasselTech_CamCapture;
using System.Data.SqlClient;

namespace WebCam_Barcode_Scanner
{
    public partial class Form1 : Form
    {
        Camera cam;
        Timer t;
        BackgroundWorker worker;
        Bitmap CapImage;
        string con_string = "Data Source=OMER;Initial Catalog=qr_codes;Integrated Security=True";

        public Form1()
        {
            InitializeComponent();

            t = new Timer();
            cam = new Camera(pictureBox1);
            worker = new BackgroundWorker();

            worker.DoWork += Worker_DoWork;
            t.Tick += T_Tick;
            t.Interval = 1;
        }

        private void T_Tick(object sender, EventArgs e)
        {
            CapImage = cam.GetBitmap();
            if (CapImage != null && !worker.IsBusy)
                worker.RunWorkerAsync();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BarcodeDecoder decoder = new BarcodeDecoder();
            try
            {
                string decoded_text = decoder.Decode(CapImage).Text;
                SqlConnection connection = new SqlConnection(con_string);
                SqlCommand command = connection.CreateCommand();

                connection.Open();
                command.CommandText = "INSERT INTO dbo.log (employee_id) VALUES(@employee_id);";
                command.Parameters.AddWithValue("@employee_id", decoded_text);
                if (command.ExecuteNonQuery() > 0)
                {
                    MessageBox.Show("Attendance was successfully recorded for ID: " + decoded_text);
                }

                connection.Close();
            }
            catch (SqlException)
            {
                MessageBox.Show("Please make sure you scanned a correct QR Code!");
            }
            catch (Exception)
            {

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                cam.Start();
                t.Start();
                button2.Enabled = true;
                button1.Enabled = false;
            }
            catch(Exception ex)
            {
                cam.Stop();
                MessageBox.Show(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            t.Stop();
            cam.Stop();
            button2.Enabled = false;
            button1.Enabled = true;
        }
    }
}
