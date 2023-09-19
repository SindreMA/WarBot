using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace WarBot
{
    public class WarEventDTO
    {
        public ulong GuildID { get; set; }
        public DateTime EventStarted { get; set; }
        public DateTime EventEnded { get; set; }
        public List<WarUsersDTO> Users { get; set; }
        public ISocketMessageChannel ChannelCreatedIn { get; set; }
        public bool Active { get; set; }
    }
}
