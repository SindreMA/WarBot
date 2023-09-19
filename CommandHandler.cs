using System;
using Discord;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using System.Threading.Tasks;
using WarBot;
using TemplateBot.Modules;
using System.IO;
using Newtonsoft.Json;
using System.Threading;
using OfficeOpenXml;

namespace TemplateBot
{
    class CommandHandler
    {
        public System.Threading.Timer _timer;

        private DiscordSocketClient _client;
        private CommandService _service;
        public static List<WarEventDTO> WarEvents = new List<WarEventDTO>();
        public CommandHandler(DiscordSocketClient client)
        {
            _timer = new System.Threading.Timer(Callback, true, 60000, System.Threading.Timeout.Infinite);

            _client = client;
            _service = new CommandService();
            _service.AddModulesAsync(Assembly.GetEntryAssembly());
            _client.MessageReceived += _client_MessageReceived;
            _client.ReactionAdded += _client_ReactionAdded;
            try
            {

                string json = File.ReadAllText("WarEvents.json");
                WarEvents = JsonConvert.DeserializeObject<List<WarEventDTO>>(json);
            }
            catch (Exception)
            {
            }
            TimerEvent();
        }
        private void Callback(Object state)
        {
            TimerEvent();
            _timer.Change(60000, Timeout.Infinite);
        }
        private void TimerEvent()
        {
            foreach (var item in WarEvents)
            {
                if (item.Active)
                {
                    int s = DateTime.Compare(item.EventEnded, DateTime.Now);
                    if (s.ToString().Contains("-"))
                    {

                        var warevent = item;
                        var result = warevent.EventEnded.Subtract(DateTime.Now).TotalHours;
                        SendEmbed(warevent.ChannelCreatedIn, "War event have been removed!", "*There where " + result + "remaining when it was ended!*");
                        //warevent.ChannelCreatedIn.SendFileAsync(Commands.ToExcel(item.GuildID, warevent.ChannelCreatedIn));
                        warevent.Active = false;
                    }
                }
            }
        }
        private async Task _client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (arg3.Emote.Name == "✅" && !arg3.User.Value.IsBot)
            {


                if (CommandHandler.WarEvents.Exists(x => x.GuildID == (arg3.Channel as SocketGuildChannel).Guild.Id && x.Active))
                {
                    var warevent = CommandHandler.WarEvents.Find(x => x.GuildID == (arg3.Channel as SocketGuildChannel).Guild.Id && x.Active);
                    if (!warevent.Users.Exists(x=> x.UserID == arg3.User.Value.Id ))
                    {
                        WarUsersDTO waruser = new WarUsersDTO();
                        waruser.UserID = arg3.User.Value.Id;
                        List<string> bases = new List<string>();
                        waruser.Bases = bases;
                        waruser.JoinedAt = DateTime.Now;
                        warevent.Users.Add(waruser);
                        await SendEmbed(arg3.Channel, arg3.User.Value.Username + " have been added!", "*Use .addbase 'name' to add a base.*");
                    }
                    else
                    {
                        await SendEmbed(arg3.Channel, "User already exist in current war!");
                    }

                }
                else
                {
                    await SendEmbed(arg3.Channel, "There is no current war active!");
                }
            }
        }

        private async Task _client_MessageReceived(SocketMessage arg)
        {
            var msg = arg as SocketUserMessage;
            if (msg == null) return;

            var context = new SocketCommandContext(_client, msg);
            int argPost = 0;
            if (msg.HasCharPrefix('.', ref argPost))
            {
                var result = _service.ExecuteAsync(context, argPost);
                if (!result.Result.IsSuccess && result.Result.Error != CommandError.UnknownCommand)
                {
                    await context.Channel.SendMessageAsync(result.Result.ErrorReason);

                }
                if (result.Result.IsSuccess)
                {
                    await Save();
                }
                await Program.Log("Invoked " + msg + " in " + context.Channel + " with " + result.Result, ConsoleColor.Magenta);

            }
            else
            {
                await Program.Log(context.Channel + "-" + context.User.Username + " : " + msg, ConsoleColor.White);
            }

        }
        public async Task SendEmbed(ISocketMessageChannel Channel, string title, string text)
        {
            await Channel.SendMessageAsync("", false, SimpleEmbed(Color.Green, title, text));
        }
        public static Embed SimpleEmbed(Color c, string title, string description)
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithColor(c);
            eb.Title = title;
            eb.WithDescription(description);
            return eb.Build();
        }
        public async Task SendEmbed(ISocketMessageChannel Channel, string title)
        {
            await Channel.SendMessageAsync("", false, SimpleEmbed(Color.Green, title));
        }
        public static Embed SimpleEmbed(Color c, string title)
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithColor(c);
            eb.Title = title;
            return eb.Build();
        }
        public async Task Save()
        {
            string jsonfile = "WarEvents.json";
            if (File.Exists(jsonfile))
            {
                File.Delete(jsonfile);
            }
            File.WriteAllText(jsonfile, JsonConvert.SerializeObject(CommandHandler.WarEvents));
        }
        /*
        public void ToExcel(ulong GuildID, IMessageChannel channel)
        {
            WarEventDTO warevent = null;
            if (CommandHandler.WarEvents.Exists(x => x.GuildID == GuildID && x.Active))
            {
                warevent = CommandHandler.WarEvents.Find(x => x.GuildID == GuildID && x.Active);

                ExcelPackage ExcelPkg = new ExcelPackage();
                ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add("WarStats");

                int RowStart = 6;
                int RowStart2 = 3;
                wsSheet1.Column(2).Width = 40;
                wsSheet1.Column(3).Width = 40;
                wsSheet1.Column(4).Width = 40;
                wsSheet1.Column(5).Width = 40;
                using (ExcelRange Rng = wsSheet1.Cells[RowStart2, 2])
                {
                    Rng.Value = "War started:";

                    Rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkOrange);

                }
                using (ExcelRange Rng = wsSheet1.Cells[RowStart2, 3])
                {
                    Rng.Value = warevent.EventStarted.ToString();
                    Rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkOrange);
                }
                using (ExcelRange Rng = wsSheet1.Cells[RowStart2, 4])
                {
                    Rng.Value = "War ended:";
                    Rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkOrange);
                }
                using (ExcelRange Rng = wsSheet1.Cells[RowStart2, 5])
                {
                    Rng.Value = warevent.EventEnded.ToString();
                    Rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkOrange);
                }

                using (ExcelRange Rng = wsSheet1.Cells[RowStart, 2])
                {
                    Rng.Value = "Users";
                    Rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Orange);
                }
                using (ExcelRange Rng = wsSheet1.Cells[RowStart, 3])
                {
                    Rng.Value = "Base 1";
                    Rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Orange);
                }
                using (ExcelRange Rng = wsSheet1.Cells[RowStart, 4])
                {
                    Rng.Value = "Base 2";
                    Rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Orange);
                }
                using (ExcelRange Rng = wsSheet1.Cells[RowStart, 5])
                {
                    Rng.Value = "Time joined";
                    Rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Orange);
                }
                int i = 7;
                foreach (var item in warevent.Users)
                {
                    System.Drawing.Color color;
                    if (IsOdd(i))
                    {
                        color = (System.Drawing.Color.LightGray);
                    }
                    else
                    {
                        color = (System.Drawing.Color.Gray);
                    }
                    var user = GetUser(item.UserID.ToString());
                    if (user != null)
                    {
                        using (ExcelRange Rng = wsSheet1.Cells[i, 2])
                        {
                            Rng.Value = user.Username;
                            Rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            Rng.Style.Fill.BackgroundColor.SetColor(color);

                        }
                    }
                    else
                    {
                        using (ExcelRange Rng = wsSheet1.Cells[i, 2])
                        {
                            Rng.Value = item.UserID;
                            Rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            Rng.Style.Fill.BackgroundColor.SetColor(color);
                        }
                    }
                    using (ExcelRange Rng = wsSheet1.Cells[i, 3])
                    {
                        if (item.Bases.Count != 0)
                        {
                            Rng.Value = item.Bases[0];
                        }

                        Rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        Rng.Style.Fill.BackgroundColor.SetColor(color);
                    }
                    using (ExcelRange Rng = wsSheet1.Cells[i, 4])
                    {
                        if (item.Bases.Count == 2)
                        {
                            Rng.Value = item.Bases[1];
                        }
                        Rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        Rng.Style.Fill.BackgroundColor.SetColor(color);
                    }
                    using (ExcelRange Rng = wsSheet1.Cells[i, 5])
                    {
                        Rng.Value = item.JoinedAt.ToString();
                        Rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        Rng.Style.Fill.BackgroundColor.SetColor(color);

                    }
                    i++;
                }
                for (int o = 1; o < 100; o++)
                {
                    using (ExcelRange Rng = wsSheet1.Cells[o, 1])
                    {
                        Rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Black);

                    }
                }
                for (int o = 1; o < 6; o++)
                {
                    using (ExcelRange Rng = wsSheet1.Cells[1, o])
                    {
                        Rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Black);

                    }
                }
                for (int o = 1; o < 6; o++)
                {
                    using (ExcelRange Rng = wsSheet1.Cells[2, o])
                    {
                        Rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Black);

                    }
                }
                for (int o = 1; o < 6; o++)
                {
                    using (ExcelRange Rng = wsSheet1.Cells[4, o])
                    {
                        Rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Black);

                    }
                }
                for (int o = 1; o < 6; o++)
                {
                    using (ExcelRange Rng = wsSheet1.Cells[5, o])
                    {
                        Rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Black);

                    }
                }
                for (int o = 1; o < 200; o++)
                {

                    using (ExcelRange Rng = wsSheet1.Cells[o, 6])
                    {
                        Rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Black);

                    }


                }

                wsSheet1.Protection.IsProtected = false;
                wsSheet1.Protection.AllowSelectLockedCells = false;
                string savelocation = @"D:\Report_" + warevent.EventStarted.Date.ToString().Replace("00.00.00", "").Replace("_", "") + "-" + warevent.EventEnded.Date.ToString().Replace("00.00.00", "").Replace("_", "") + ".xlsx";
                ExcelPkg.SaveAs(new FileInfo(savelocation));
                return savelocation;

            }


        }
        public SocketGuildUser GetUser(string nameorID)
        {
            SocketGuildUser Founduser = null;

            var warevent = CommandHandler.WarEvents.Find(x => x.GuildID == Context.Guild.Id && x.Active);

            Int64 n;
            bool isNumeric = Int64.TryParse(nameorID, out n);
            if (isNumeric)
            {
                Founduser = Context.Guild.Users.Single(x => x.Id == ulong.Parse(nameorID));
            }
            else
            {
                Founduser = Context.Guild.Users.Single(x => x.Username.ToLower() == nameorID.ToLower());
            }
            if (Founduser != null)
            {
                return Founduser;
            }
            else
            {
                return null;
            }

        }
        */
        public static bool IsOdd(int value)
        {
            return value % 2 != 0;
        }
    }
}
