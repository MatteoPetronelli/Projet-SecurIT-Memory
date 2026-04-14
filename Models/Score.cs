using System;

namespace SecurIT_Memory.Models
{
    public class Score
    {
        public string PlayerName { get; set; }
        public TimeSpan Time { get; set; }
        public int Attempts { get; set; }
    }
}