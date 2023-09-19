using Discord;
using Discord.Addons.EmojiTools;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WarBot;


namespace TemplateBot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("startwar")]
        public async Task warstart([Optional]string time1)
        {
            if (!CommandHandler.WarEvents.Exists(x => x.GuildID == Context.Guild.Id && x.Active))
            {


                string time = time1;
                WarEventDTO item1 = new WarEventDTO();
                if (time == null || time == "")
                {
                    item1.EventStarted = DateTime.Now;
                    item1.EventEnded = item1.EventStarted.AddHours(48);
                }
                else
                {
                    item1.EventStarted = DateTime.Parse(time);
                    item1.EventEnded = item1.EventStarted.AddHours(48);
                }
                List<WarUsersDTO> users = new List<WarUsersDTO>();
                item1.Users = users;
                item1.GuildID = Context.Guild.Id;
                item1.Active = true;
                var result = item1.EventEnded.Subtract(item1.EventStarted).TotalHours;
                item1.ChannelCreatedIn = Context.Channel;

                CommandHandler.WarEvents.Add(item1);
                var message = await mSendEmbed(Context, "War started " + item1.EventStarted + "!",
                    result + "hours remaining of the war!" + Environment.NewLine +
                    "Click the reaction to join the war or use the .joinwar command!"
                    );
                await message.AddReactionAsync(EmojiExtensions.FromText("::white_check_mark::"));
            }

            else
            {
                await SendEmbed(Context, "There is already a war ongoing!");

            }

        }
        [Command("joinwar")]
        public async Task joinwar()
        {
            if (CommandHandler.WarEvents.Exists(x => x.GuildID == Context.Guild.Id && x.Active))
            {
                var warevent = CommandHandler.WarEvents.Find(x => x.GuildID == Context.Guild.Id && x.Active);

                if (!warevent.Users.Exists(x => x.UserID == Context.User.Id))
                {
                    WarUsersDTO waruser = new WarUsersDTO();
                    waruser.UserID = Context.User.Id;
                    List<string> bases = new List<string>();
                    waruser.Bases = bases;
                    waruser.JoinedAt = DateTime.Now;
                    warevent.Users.Add(waruser);
                    await SendEmbed(Context, Context.User.Username + " have been added!", "*Use .addbase 'name' to add a base.*");


                }
                else
                {
                    await SendEmbed(Context.Channel, "User already exist in current war!");
                }
            }
            else
            {
                await SendEmbed(Context, "There is no current war!");

            }
        }
        [Command("addbase")]
        public async Task addbase([Remainder]string basename)
        {
            if (CommandHandler.WarEvents.Exists(x => x.GuildID == Context.Guild.Id && x.Active))
            {
                var warevent = CommandHandler.WarEvents.Find(x => x.GuildID == Context.Guild.Id && x.Active);
                if (warevent.Users.Exists(x => x.UserID == Context.User.Id))
                {
                    var waruser = warevent.Users.Find(x => x.UserID == Context.User.Id);
                    if (waruser.Bases.Count < 2)
                    {
                        waruser.Bases.Add(basename);
                        await SendEmbed(Context, basename + " have been added!", "*Use .showbases to view your bases.*");
                    }
                    else
                    {
                        await SendEmbed(Context, "You already have 2 bases!", "*You can delete one of your beses with .delbase \"basename\"*.");
                    }
                }
                else
                {
                    await SendEmbed(Context, Context.User + " is not in a war!");
                }
            }
            else
            {
                await SendEmbed(Context, "There is no current war!");
            }
        }
        [Command("delbase")]
        public async Task delbase([Remainder]string basename)
        {
            if (CommandHandler.WarEvents.Exists(x => x.GuildID == Context.Guild.Id && x.Active))
            {
                var warevent = CommandHandler.WarEvents.Find(x => x.GuildID == Context.Guild.Id && x.Active);
                if (warevent.Users.Exists(x => x.UserID == Context.User.Id))
                {
                    var waruser = warevent.Users.Find(x => x.UserID == Context.User.Id);
                    if (waruser.Bases.Exists(x => x == basename))
                    {
                        waruser.Bases.Remove(basename);
                        await SendEmbed(Context, "Base have been removed!");
                    }
                    else
                    {
                        await SendEmbed(Context, "Base does'nt exist!");
                    }
                }

                else
                {
                    await SendEmbed(Context, Context.User + " is not in a war!");
                }
            }
            else
            {
                await SendEmbed(Context, "There is no current war!");
            }
        }

        [Command("delbase")]
        public async Task delbase(string username, [Remainder]string basename)
        {
            SocketGuildUser Founduser = null;

            if (CommandHandler.WarEvents.Exists(x => x.GuildID == Context.Guild.Id && x.Active))
            {
                var warevent = CommandHandler.WarEvents.Find(x => x.GuildID == Context.Guild.Id && x.Active);

                Int64 n;
                bool isNumeric = Int64.TryParse(username, out n);
                if (isNumeric)
                {
                    Founduser = Context.Guild.Users.Single(x => x.Id == ulong.Parse(username));
                }
                else
                {
                    Founduser = Context.Guild.Users.Single(x => x.Username.ToLower() == username.ToLower());
                }
                if (Founduser != null)
                {
                    if (warevent.Users.Exists(x => x.UserID == Founduser.Id))
                    {
                        var waruser = warevent.Users.Find(x => x.UserID == Context.User.Id);
                        if (waruser.Bases.Exists(x => x == basename))
                        {
                            waruser.Bases.Remove(basename);
                            await SendEmbed(Context, "Base have been removed!");
                        }
                        else
                        {
                            await SendEmbed(Context, "Base does'nt exist!");
                        }
                    }
                    else
                    {
                        await SendEmbed(Context, Context.User + " is not in a war!");
                    }
                }
                else
                {
                    await SendEmbed(Context, "Could'nt find user");
                }
            }
            else
            {
                await SendEmbed(Context, "There is no current war!");
            }
        }
        [Command("Showbases")]
        public async Task Showbases()
        {

            if (CommandHandler.WarEvents.Exists(x => x.GuildID == Context.Guild.Id && x.Active))
            {
                var warevent = CommandHandler.WarEvents.Find(x => x.GuildID == Context.Guild.Id && x.Active);
                if (warevent.Users.Exists(x => x.UserID == Context.User.Id))
                {
                    string msg = "";
                    var waruser = warevent.Users.Find(x => x.UserID == Context.User.Id);
                    foreach (var item in waruser.Bases)
                    {
                        msg = msg + item + Environment.NewLine;
                    }
                    await SendEmbed(Context, "Bases for " + Context.User.Username, msg);
                }
                else
                {
                    await SendEmbed(Context, Context.User + " is not in a war!");
                }
            }
            else
            {
                await SendEmbed(Context, "There is no current war!");
            }
        }
        [Command("Showallbases")]
        public async Task Showallbases()
        {
            string msg = "";
            if (CommandHandler.WarEvents.Exists(x => x.GuildID == Context.Guild.Id && x.Active))
            {
                var warevent = CommandHandler.WarEvents.Find(x => x.GuildID == Context.Guild.Id && x.Active);
                bool over1800 = false;
                foreach (var users in warevent.Users)
                {
                    string stringuser = "(Cant find user)";
                    try
                    {
                        stringuser = Context.Guild.GetUser(users.UserID).Username;
                    }
                    catch (Exception) { }
                    foreach (var bases in users.Bases)
                    {

                        if (msg.Length > 1800)
                        {
                            msg = msg + "Basename : " + bases + "  - User : " + stringuser + Environment.NewLine;
                            await SendEmbed(Context, "All bases : ", msg);
                            over1800 = true;
                        }
                        msg = msg + "Basename : " + bases + "  - User : " + stringuser + Environment.NewLine;
                    }
                }
                if (over1800)
                {
                    await SendEmbed(Context, "", msg);
                }
                else
                {
                    await SendEmbed(Context, "All bases", msg);
                }

            }
            else
            {
                await SendEmbed(Context, "There is no current war!");
            }


        }

        [Command("Showbases")]
        public async Task Showbases([Remainder]string user)
        {
            if (isnumber(user))
            {
                var i = int.Parse(user);
                string msg = "";
                if (CommandHandler.WarEvents.Exists(x => x.GuildID == Context.Guild.Id && x.Active))
                {
                    var warevent = CommandHandler.WarEvents.Find(x => x.GuildID == Context.Guild.Id && x.Active);
                    bool over1800 = false;
                    foreach (var users in warevent.Users.FindAll(x => x.Bases.Count == i))
                    {
                        string stringuser = "(Cant find user)";
                        try
                        {
                            stringuser = Context.Guild.GetUser(users.UserID).Username;
                        }

                        catch (Exception) { }
                        foreach (var bases in users.Bases)
                        {

                            if (msg.Length > 1800)
                            {
                                msg = msg + "Basename : " + bases + "  - User : " + stringuser + Environment.NewLine;
                                await SendEmbed(Context, "All users with only " + i + " base(s)!", msg);
                                over1800 = true;
                            }
                            msg = msg + "Basename : " + bases + "  - User : " + stringuser + Environment.NewLine;


                        }
                    }
                    if (msg == "")
                    {
                        await SendEmbed(Context, "There are no users with only " + i + " base(s)!");
                    }
                    else
                    {


                        if (over1800)
                        {
                            await SendEmbed(Context, "", msg);
                        }
                        else
                        {
                            await SendEmbed(Context, "All users with only " + i + " base(s)!", msg);
                        }
                    }
                }
                else
                {
                    await SendEmbed(Context, "There is no current war!");
                }
            }
            else
            {


                string msg = "";
                if (CommandHandler.WarEvents.Exists(x => x.GuildID == Context.Guild.Id && x.Active))
                {
                    var warevent = CommandHandler.WarEvents.Find(x => x.GuildID == Context.Guild.Id && x.Active);
                    if (DoesExist(user.ToString()))
                    {
                        var user1 = GetUser(user);
                        if (warevent.Users.Exists(x => x.UserID == user1.Id))
                        {
                            var user2 = warevent.Users.Find(x => x.UserID == user1.Id);
                            bool over1800 = false;
                            foreach (var bases in user2.Bases)
                            {

                                if (msg.Length > 1800)
                                {
                                    over1800 = true;
                                    msg = msg + "Basename : " + bases + "  - User : " + user1.Username + Environment.NewLine;
                                    await SendEmbed(Context, "All bases : ", msg);
                                }
                                msg = msg + "Basename : " + bases + "  - User : " + user1.Username + Environment.NewLine;
                            }
                            if (over1800)
                            {
                                await SendEmbed(Context, "", msg);
                            }
                            else
                            {
                                await SendEmbed(Context, "All bases", msg);
                            }

                        }
                        else
                        {
                            await SendEmbed(Context, "User is could not be found!");

                        }

                    }


                }
                else
                {
                    await SendEmbed(Context, "There is no current war!");
                }
            }
        }
        [Command("ShowUsers")]
        public async Task ShowUsers()
        {
            if (CommandHandler.WarEvents.Exists(x => x.GuildID == Context.Guild.Id && x.Active))
            {
                string msg = "";
                bool over1800 = false;
                var warevent = CommandHandler.WarEvents.Find(x => x.GuildID == Context.Guild.Id && x.Active);
                foreach (var item in warevent.Users)
                {
                    if (DoesExist(item.UserID))
                    {
                        string userstring = item.UserID.ToString();
                        var user = GetUser(item.UserID.ToString());
                        if (user != null)
                        {
                            userstring = user.Username;
                        }
                        if (msg.Length > 1800)
                        {
                            msg = msg + userstring + Environment.NewLine;
                            await SendEmbed(Context, "Users", msg);
                            over1800 = true;
                        }
                        else
                        {
                            msg = msg + userstring + Environment.NewLine;
                        }

                    }
                }
                if (over1800)
                {
                    await SendEmbed(Context, "", msg);

                }
                else
                {
                    await SendEmbed(Context, "Users", msg);

                }
            }
            else
            {
                await SendEmbed(Context, "There is no current war!");
            }
        }

        [Command("endwar")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task endwar()
        {
            if (CommandHandler.WarEvents.Exists(x => x.GuildID == Context.Guild.Id && x.Active))
            {
                var warevent = CommandHandler.WarEvents.Find(x => x.GuildID == Context.Guild.Id && x.Active);
                var result = warevent.EventEnded.Subtract(DateTime.Now).TotalHours;
                await SendEmbed(Context, "War event have been removed!", "*There where " + result + "remaining when it was ended!*");
                await Context.Channel.SendFileAsync(ToExcel(Context.Guild.Id, Context.Channel));
                warevent.Active = false;

            }
            else
            {
                await SendEmbed(Context, "There is no war ongoing!");
            }

        }

        [Command("Getreport")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task test()
        {
            var file = ToExcel(Context.Guild.Id, Context.Channel);
            if (file != null)
            {
                await SendEmbed(Context, "Report have been generated!");
                await Context.Channel.SendFileAsync(file);
            }
            else
            {

                await SendEmbed(Context, "Something went wrong with the excel creation!");
            }

        }
        [Command("warstatus")]
        public async Task warstart()
        {

            if (CommandHandler.WarEvents.Exists(x => x.GuildID == Context.Guild.Id && x.Active))
            {
                var warevent = CommandHandler.WarEvents.Find(x => x.GuildID == Context.Guild.Id && x.Active);
                var result = warevent.EventEnded.Subtract(DateTime.Now).TotalHours;
                int bases = 0;
                foreach (var item in warevent.Users)
                {
                    foreach (var bases1 in item.Bases)
                    {
                        bases++;
                    }
                }
                var message = await mSendEmbed(Context, "War status :  ",

                result + " hours remaining of the war!" + Environment.NewLine +
                "Users : " + warevent.Users.Count + Environment.NewLine +
                "Bases : " + bases + Environment.NewLine +
                "Event started : " + warevent.EventStarted + Environment.NewLine +
                "Event ends : " + warevent.EventEnded

          );

            }
            else
            {
                await SendEmbed(Context, "There is no war ongoing!");
            }



        }
        public async Task<RestUserMessage> mSendEmbed(SocketCommandContext Context, string title, string text)
        {
            return await Context.Channel.SendMessageAsync("", false, SimpleEmbed(Color.Green, title, text)) as RestUserMessage;
        }
        public string ToExcel(ulong GuildID, IMessageChannel channel)
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
            else
            {
                SendEmbed(channel, "There is no war ongoing!");
                return null;
            }
        }
        public static bool IsOdd(int value)
        {
            return value % 2 != 0;
        }
        public async Task SendEmbed(SocketCommandContext Context, string title, string text)
        {
            await Context.Channel.SendMessageAsync("", false, SimpleEmbed(Color.Green, title, text));
        }
        public static Embed SimpleEmbed(Color c, string title, string description)
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithColor(c);
            eb.Title = title;
            eb.WithDescription(description);
            return eb.Build();
        }
        public async Task SendEmbed(SocketCommandContext Context, string title)
        {
            await Context.Channel.SendMessageAsync("", false, SimpleEmbed(Color.Green, title));
        }
        public static async Task SendEmbed(IMessageChannel channel, string title)
        {
            channel.SendMessageAsync("", false, SimpleEmbed(Color.Green, title));
        }
        public static Embed SimpleEmbed(Color c, string title)
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithColor(c);
            eb.Title = title;
            return eb.Build();
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
        public bool DoesExist(ulong idoruser)
        {
            ulong foruser = 0;
            try
            {
                foruser = GetUser(idoruser.ToString()).Id;
            }
            catch (Exception) { }

            if (foruser != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool DoesExist(string idoruser)
        {
            ulong foruser = 0;
            try
            {
                foruser = GetUser(idoruser).Id;
            }
            catch (Exception) { }

            if (foruser != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool isnumber(string i)
        {
            Int64 n;
            bool isNumeric = Int64.TryParse(i, out n);
            return isNumeric;
        }

    }

}
