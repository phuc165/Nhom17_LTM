using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Timers; // Thêm namespace này

namespace Server
{
    public class Room
    {
        public List<TcpClient> Players { get; set; } = new List<TcpClient>();
        public Common.CellState[,] Board { get; set; } = new Common.CellState[Common.BOARD_SIZE, Common.BOARD_SIZE];
        public Common.GameStatus GameStatus { get; set; } = Common.GameStatus.Waiting;
        public int CurrentPlayerIndex { get; set; } = 0;
        public int RoomId { get; set; }
        private System.Timers.Timer inactivityTimer; // Sử dụng rõ ràng namespace System.Timers.Timer

        public event Action<int> OnRoomClosed; // Sự kiện khi phòng bị đóng

        public Room(int id)
        {
            RoomId = id;
            InitializeBoard();
            StartInactivityTimer();
        }

        public void InitializeBoard()
        {
            for (int i = 0; i < Common.BOARD_SIZE; i++)
            {
                for (int j = 0; j < Common.BOARD_SIZE; j++)
                {
                    Board[i, j] = Common.CellState.Empty;
                }
            }
        }

        private void StartInactivityTimer()
        {
            inactivityTimer = new System.Timers.Timer(30 * 60 * 1000); // 30 phút (1800000 ms)
            inactivityTimer.Elapsed += (sender, e) => CloseRoom();
            inactivityTimer.AutoReset = false;
            inactivityTimer.Start();
        }

        public void ResetInactivityTimer()
        {
            inactivityTimer.Stop();
            inactivityTimer.Start();
        }

        private void CloseRoom()
        {
            Console.WriteLine($"Room {RoomId} is closing due to inactivity.");
            OnRoomClosed?.Invoke(RoomId);
            Dispose();
        }

        public void Dispose()
        {
            inactivityTimer?.Stop();
            inactivityTimer?.Dispose();
            Players.Clear();
            Board = null;
        }

        public void PlayerJoined(TcpClient player)
        {
            Players.Add(player);
            ResetInactivityTimer();
        }

        public void PlayerMoved()
        {
            ResetInactivityTimer();
        }
    }
}
