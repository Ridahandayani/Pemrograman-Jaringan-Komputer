using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace MovingObject
{
    public partial class Form1 : Form
    {
        Pen red = new Pen(Color.Red);
        Rectangle rect = new Rectangle(20, 20, 30, 30);
        SolidBrush fillBlue = new SolidBrush(Color.Blue);
        int slide = 10;

        // --- Networking ---
        TcpListener server;
        List<TcpClient> clients = new List<TcpClient>();

        public Form1()
        {
            InitializeComponent();
            timer1.Interval = 50;
            timer1.Enabled = true;
            this.Load += Form1_Load; // pastikan server start saat form load
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            StartServer();
        }

        // ---- SERVER FUNCTIONS ----
        private void StartServer()
        {
            try
            {
                server = new TcpListener(IPAddress.Any, 5000);
                server.Start();
                server.BeginAcceptTcpClient(OnClientConnected, null);
                this.Text = "Server Running on port 5000";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error start server: " + ex.Message);
            }
        }

        private void OnClientConnected(IAsyncResult ar)
        {
            try
            {
                TcpClient client = server.EndAcceptTcpClient(ar);
                clients.Add(client);

                // siap terima client berikutnya
                server.BeginAcceptTcpClient(OnClientConnected, null);
            }
            catch { }
        }

        private void Broadcast(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            foreach (var client in clients.ToList())
            {
                try
                {
                    client.GetStream().Write(data, 0, data.Length);
                }
                catch
                {
                    clients.Remove(client); // hapus kalau client putus
                }
            }
        }

        // ---- ANIMASI KOTAK ----
        private void timer1_Tick(object sender, EventArgs e)
        {
            back();

            rect.X += slide;
            Invalidate();

            // kirim posisi ke semua client
            Broadcast($"{rect.X},{rect.Y}");
        }

        private void back()
        {
            if (rect.X >= this.Width - rect.Width * 2)
                slide = -10;
            else if (rect.X <= rect.Width / 2)
                slide = 10;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.DrawRectangle(red, rect);
            g.FillRectangle(fillBlue, rect);
        }
    }
}
