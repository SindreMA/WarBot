using System;
using System.Collections.Generic;
using System.Text;

namespace WarBot
{
    public class WarUsersDTO
    {
        public ulong UserID { get; set; }
        public DateTime JoinedAt { get; set; }
        public List<string> Bases { get; set; }
    }
}
