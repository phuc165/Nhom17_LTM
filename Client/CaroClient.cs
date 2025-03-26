using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Client
{
    public class CaroClient : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        private Thread listenThread;
        private Panel boardPanel;
        private Label statusLabel;
        private Label roomLabel;
        private TextBox chatInput;
        private Button sendButton;
        private ListBox chatBox;
        private Button connectButton;
        private TextBox serverInput;
        private Common.CellState[,] board = new Common.CellState[Common.BOARD_SIZE, Common.BOARD_SIZE];
        private Common.GameStatus gameStatus = Common.GameStatus.Waiting;
        private Common.CellState playerRole = Common.CellState.Empty;
        private bool isMyTurn = false;
        private int roomId = -1;

    }