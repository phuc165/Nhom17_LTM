using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
        private void ListenForClients()
        {
            try
            {
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    AssignClientToRoom(client);
                }
            }
            catch (SocketException)
            {
                // Server stopped
            }
            catch (Exception ex)
            {
                UpdateStatus($"Server error: {ex.Message}");
            }
        }
        private void AssignClientToRoom(TcpClient client)
        {
            Room availableRoom = null;

            // Find an available room or create a new one
            foreach (var room in rooms)
            {
                if (room.Players.Count < 2 && room.GameStatus == Common.GameStatus.Waiting)
                {
                    availableRoom = room;
                    break;
                }
            }

            if (availableRoom == null)
            {
                availableRoom = new Room(rooms.Count);
                rooms.Add(availableRoom);
            }

            int playerIndex = availableRoom.Players.Count;
            availableRoom.Players.Add(client);

            UpdateStatus($"Player joined Room {availableRoom.RoomId}. Players: {availableRoom.Players.Count}/2");

            //Thread clientThread = new Thread(() => HandleClient(client, availableRoom, playerIndex));
            //clientThread.IsBackground = true;
            //clientThread.Start();

            NetworkStream stream = client.GetStream();
            string role = (playerIndex == 0) ? "X" : "O";
            byte[] roleMsg = Encoding.ASCII.GetBytes(Common.FormatMessage("ROLE", $"{role},{availableRoom.RoomId}"));
            stream.Write(roleMsg, 0, roleMsg.Length);

            if (availableRoom.Players.Count == 2)
            {
                availableRoom.GameStatus = Common.GameStatus.Playing;
               // BroadcastToRoom(availableRoom, Common.FormatMessage("START", ""));
                UpdateStatus($"Room {availableRoom.RoomId}: Game started. Player X's turn.");
            }
        }

        private void UpdateStatus(string status)
        {
            if (statusLabel.InvokeRequired)
            {
                statusLabel.Invoke(new Action<string>(UpdateStatus), status);
            }
            else
            {
                statusLabel.Text = status;
            }
        }

        private bool CheckWin(Room room, int row, int col)
        {
            Common.CellState player = room.Board[row, col];
            int count;

            // Horizontal
            count = 0;
            for (int c = Math.Max(0, col - 4); c <= Math.Min(Common.BOARD_SIZE - 1, col + 4); c++)
            {
                if (room.Board[row, c] == player) { count++; if (count == Common.WINNING_COUNT) return true; } else { count = 0; }
            }

            // Vertical
            count = 0;
            for (int r = Math.Max(0, row - 4); r <= Math.Min(Common.BOARD_SIZE - 1, row + 4); r++)
            {
                if (room.Board[r, col] == player) { count++; if (count == Common.WINNING_COUNT) return true; } else { count = 0; }
            }

            // Diagonal (top-left to bottom-right)
            count = 0;
            for (int i = -4; i <= 4; i++)
            {
                int r = row + i;
                int c = col + i;
                if (r >= 0 && r < Common.BOARD_SIZE && c >= 0 && c < Common.BOARD_SIZE)
                {
                    if (room.Board[r, c] == player) { count++; if (count == Common.WINNING_COUNT) return true; } else { count = 0; }
                }
            }

            // Diagonal (top-right to bottom-left)
            count = 0;
            for (int i = -4; i <= 4; i++)
            {
                int r = row + i;
                int c = col - i;
                if (r >= 0 && r < Common.BOARD_SIZE && c >= 0 && c < Common.BOARD_SIZE)
                {
                    if (room.Board[r, c] == player) { count++; if (count == Common.WINNING_COUNT) return true; } else { count = 0; }
                }
            }

            return false;
        }
    }
}
