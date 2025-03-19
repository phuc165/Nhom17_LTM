using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;


namespace Server
{
    public class CaroServer : Form
    {
        private TcpListener server;
        private List<Room> rooms = new List<Room>();
        private Button startButton;
        private Label statusLabel;

        public CaroServer()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Caro Game Server";
            this.Size = new Size(400, 300);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            statusLabel = new Label
            {
                Text = "Server Status: Not Running",
                Location = new Point(20, 20),
                AutoSize = true
            };

            startButton = new Button
            {
                Text = "Start Server",
                Location = new Point(20, 50),
                Size = new Size(100, 30)
            };
            startButton.Click += StartServer_Click;

            this.Controls.Add(statusLabel);
            this.Controls.Add(startButton);

            this.FormClosing += (s, e) => StopServer();
        }

        private void StartServer_Click(object sender, EventArgs e)
        {
            if (server == null)
            {
                StartServer();
                startButton.Text = "Stop Server";
            }
            else
            {
                StopServer();
                startButton.Text = "Start Server";
            }
        }
        private void StartServer()
        {
            try
            {
                server = new TcpListener(IPAddress.Any, 8888);
                server.Start();

                statusLabel.Text = "Server Status: Running on port 8888";

                Thread listenerThread = new Thread(ListenForClients);
                listenerThread.IsBackground = true;
                listenerThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting server: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StopServer()
        {
            if (server != null)
            {
                server.Stop();
                server = null;

                foreach (var room in rooms)
                {
                    foreach (var client in room.Players)
                    {
                        try { client.Close(); } catch { }
                    }
                }

                rooms.Clear();
                statusLabel.Text = "Server Status: Not Running";
            }
        }
    }
}