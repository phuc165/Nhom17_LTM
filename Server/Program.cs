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
    public class Common
    {
        public const int BOARD_SIZE = 15;
        public const int CELL_SIZE = 40;
        public const int WINNING_COUNT = 5;

        public enum CellState { Empty, X, O }
        public enum GameStatus { Waiting, Playing, GameOver }

        public static string FormatMessage(string command, string data)
        {
            return $"{command}|{data}";
        }

        public static void ParseMessage(string message, out string command, out string data)
        {
            string[] parts = message.Split('|');
            command = parts[0];
            data = parts.Length > 1 ? parts[1] : "";
        }
    }
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
}