using System;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace MovingObjectClient
{
    public partial class Form1 : Form
    {
        TcpClient client;
        NetworkStream stream;

        int kotakX = 50; // posisi default
        int kotakY = 50;

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ConnectToServer();
        }

        private void ConnectToServer()
        {
            try
            {
                client = new TcpClient("127.0.0.1", 5000); // alamat server lokal
                stream = client.GetStream();
                BeginRead();
                this.Text = "Client Connected";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal konek ke server: " + ex.Message);
            }
        }

        private void BeginRead()
        {
            byte[] buffer = new byte[1024];
            stream.BeginRead(buffer, 0, buffer.Length, OnDataReceived, buffer);
        }

        private void OnDataReceived(IAsyncResult ar)
        {
            int bytesRead;
            try
            {
                bytesRead = stream.EndRead(ar);
            }
            catch
            {
                return; // koneksi terputus
            }

            if (bytesRead <= 0) return;

            byte[] buffer = (byte[])ar.AsyncState;
            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            // parsing koordinat "x,y"
            var parts = message.Split(',');
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out int x) &&
                int.TryParse(parts[1], out int y))
            {
                this.Invoke((MethodInvoker)delegate {
                    kotakX = x;
                    kotakY = y;
                    this.Invalidate(); // repaint form
                });
            }

            BeginRead(); // baca lagi
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.FillRectangle(Brushes.Blue, kotakX, kotakY, 30, 30);
        }
    }
}
