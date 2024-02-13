using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Keyence.IV3.Sdk;

namespace Camera_Prueba
{
    public partial class Form1 : Form
    {
        Size image_size = new Size(320, 240);
        VisionSensorStore store = new VisionSensorStore();
        IVisionSensor camera;
        public Form1()
        {
            InitializeComponent();

            try
            {
                byte[] ipAddressLocal = { 192, 168, 0, 11 };
                IPAddress ipLocal = new IPAddress(ipAddressLocal);
                VisionSensorStore.StartPoint = ipLocal;
                
                byte[] ipAddressCamera = { 192, 168, 0, 178 };
                IPAddress ipCamera = new IPAddress(ipAddressCamera);
                camera = store.Create(ipCamera, 63000);
                
                camera.EventEnable = true;
                camera.ResultUpdated += EventoResultadoCamera;
                camera.ImageAcquired += EventoImagenCamera;

                timer_camera.Start();
            }
            catch 
            { 
            }

            
        }

        private void EventoResultadoCamera(object sender, ToolResultUpdatedEventArgs e )
        {
            if (e.TotalStatusResult)
            {
                label1.Text = "OK";
                label1.ForeColor = Color.Lime;
            }
            else
            {
                label1.Text = "NOK";
                label1.ForeColor = Color.Red;
            }
        }

        private void EventoImagenCamera(object sender, ImageAcquiredEventArgs e)
        {
            var foto = new Bitmap(image_size.Width, image_size.Height, PixelFormat.Format24bppRgb);
            BitmapData bitData = foto.LockBits(new Rectangle(Point.Empty, image_size), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            Marshal.Copy(e.LiveImage.ByteData, 0, bitData.Scan0, e.LiveImage.ByteData.Length);
            foto.UnlockBits(bitData);

            using (Graphics Marcador = Graphics.FromImage(foto))
            {
                Marcador.SmoothingMode = SmoothingMode.AntiAlias;
                for (byte i = 0; i < 16; i++)
                {
                    camera.DrawWindow(Marcador,Color.Green,Color.Red,i);
                }
            }

            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
            }

            pictureBox1.Image = foto;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            camera.TickTack();
            label2.Text = camera.ActiveProgram.ProgramName;
            if(camera.Errors.Length > 0)
            {
                label3.Text = camera.Errors[0].Description;
                label3.Visible = true;

                camera.ClearError(camera.Errors[0]);
            }
            else
            {
                if(label3.Visible)
                {
                    label3.Text = "";
                    label3.Visible = false;
                }
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            camera.Trigger();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ProgramHeader[] programaDisponibles = camera.Programs;
            if ((int)numeric_programa.Value < programaDisponibles.Length && !camera.ExternalProgramSwitch) 
            {
                try
                {
                    camera.SwitchProgramTo(programaDisponibles[(int)numeric_programa.Value]);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
