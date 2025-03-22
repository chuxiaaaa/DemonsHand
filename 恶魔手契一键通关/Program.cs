using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Management;
using System.Threading;
using System.Diagnostics;
using System.Security.Principal;
using System.IO;
using Newtonsoft.Json.Linq;
using System.IO.IsolatedStorage;
using System.Runtime.InteropServices;

namespace 恶魔手契一键通关
{
    internal class Program
    {

        static void Main(string[] args)
        {
            bool isAdmin = IsRunAsAdmin();
            Console.Title = $"{(isAdmin ? "管理员: " : "")}恶魔手契一键通关";
            string cmd = string.Empty;
            if (isAdmin)
            {
                List<Tuple<string, uint>> items = GetCommandLines("LeagueClientUx.exe");
                foreach (var item in items)
                {
                    if (!string.IsNullOrEmpty(item.Item1))
                    {
                        cmd = item.Item1;
                        break;
                    }
                }
                if (items.Count == 0)
                {
                    Info("请启动英雄联盟！");
                    return;
                }
            }
            else
            {
                try
                {
                    Process[] items = Process.GetProcessesByName("LeagueClientUx");
                    foreach (var item in items)
                    {
                        var tmp = GetCommandLine1((uint)item.Id);
                        if (!string.IsNullOrEmpty(tmp))
                        {
                            cmd = tmp;
                            break;
                        }
                    }
                    if (items.Length == 0)
                    {
                        Info("请启动英雄联盟！");
                        return;
                    }
                }
                catch (Exception)
                {
                    ProcessStartInfo procInfo = new ProcessStartInfo();
                    procInfo.UseShellExecute = true;
                    procInfo.FileName = System.Reflection.Assembly.GetEntryAssembly().Location;
                    procInfo.Verb = "runas";
                    try
                    {
                        Process.Start(procInfo);

                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                        Error("用户取消了 UAC 提示或未授予管理员权限0！");
                    }
                    return;
                }
            }
            bool find = !string.IsNullOrWhiteSpace(cmd);
            string lcutoken = string.Empty;
            string port = string.Empty;
            using (var web = new WebClient())
            {
                try
                {
                    string value = Regex.Match(cmd, "--remoting-auth-token=(.*?)\"").Groups[1].Value;
                    string value2 = Regex.Match(cmd, "--app-port=(.*?)\"").Groups[1].Value;
                    port = value2;
                    web.Encoding = Encoding.UTF8;
                    lcutoken = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes("riot:" + value))}";
                    web.Headers.Add("authorization", lcutoken);
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12);
                    ServicePointManager.ServerCertificateValidationCallback = ((object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true);

                ms:
                    Info($"读取任务列表...");
                    var missions = web.DownloadString($"https://127.0.0.1:{value2}/lol-missions/v1/missions");
                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MissionsData>>(missions);
                    List<int> donum = new List<int>();
                    foreach (var item3 in data.Where(x => x.internalName.StartsWith("DemonsHand_Auto")).OrderBy(x => x.internalName))
                    {
                        var num = item3.internalName.Substring(item3.internalName.Length - 1, 1);
                        donum.Add(int.Parse(num));
                        if (item3.viewed)
                        {
                            Info($"任务：恶魔手契 {num}/3 已通关");
                        }
                        else
                        {
                            Info($"执行通关 任务：恶魔手契 {num}/3");
                            Post($"https://127.0.0.1:{value2}/lol-missions/v1/player/{item3.id}", "{\"rewardGroups\":[\"" + item3.rewards[0].rewardGroup + "\"]}", lcutoken, "PUT");
                            Info($"执行完毕 任务：恶魔手契 {num}/3");
                            Thread.Sleep(100);
                            goto ms;
                        }
                    }
                    var DemonsHand_SigilTracker = data.Where(x => x.internalName == "DemonsHand_SigilTracker").FirstOrDefault();
                    if (!donum.Contains(2))
                    {
                        Info($"任务：恶魔手契 {2}/3 ，需要收集 {DemonsHand_SigilTracker.objectives[0].progress.currentProgress}/8 个印记");
                    }
                    if (!donum.Contains(3))
                    {
                        Info($"任务：恶魔手契 {3}/3 ，需要收集 {DemonsHand_SigilTracker.objectives[0].progress.currentProgress}/16 个印记");
                    }

                }
                catch (Exception ex)
                {
                    Error(ex.ToString());
                }
                finally
                {

                    Console.WriteLine();
                    Info("一键通关功能已经执行完毕");
                    Info("如还有任务没完成，请收集完印记再执行本工具！");
                    Console.WriteLine();
                select:
                    Info("如果想游玩 恶魔手契，按 G 进入作弊模式");
                    Info("如果想领取账号未领取奖励，按 J 进行领奖");
                    Info("按其他键退出程序");
                    var g = Console.ReadKey(true);
                    Console.Clear();
                    if (g.Key == ConsoleKey.G)
                    {
                        while (true)
                        {
                            Info($"按 H 将生命值恢复值满血");
                            Info($"按 J 设置基础伤害加成");
                            Info($"按 K 设置暴击几率");
                            Info($"按 M 设置金币数量");
                            Info($"按 ESC 退出程序");
                            Console.WriteLine();
                            Info("请先点击右上角 X 按钮，再进行修改，修改完毕之后选择 继续 进行读档！");
                            g = Console.ReadKey();
                            if (g.Key == ConsoleKey.Escape)
                            {
                                break;
                            }
                            else if (g.Key == ConsoleKey.H)
                            {
                                Post($"https://127.0.0.1:{port}/lol-settings/v2/account/LCUPreferences/lol-home-hubs", @"{
                                            ""data"": {
                                                ""boba-minigame"": {
                                                    ""Version"": 1,
                                                     ""hp"":100
                                                }
                                            },
                                            ""schemaVersion"": 1
                                        }", lcutoken, "PATCH");
                                Console.Clear();
                                Info("生命值已恢复");
                                Console.WriteLine();
                            }
                            else if (g.Key == ConsoleKey.J)
                            {
                                Console.WriteLine();
                                Console.Write("基础伤害加成：");
                                if (int.TryParse(Console.ReadLine(), out var result))
                                {
                                    Post($"https://127.0.0.1:{port}/lol-settings/v2/account/LCUPreferences/lol-home-hubs", @"{
                                                ""data"": {
                                                    ""boba-minigame"": {
                                                        ""Version"": 1,
                                                         ""runBonusStats"":{ ""BonusBaseCardDamage"":" + result + @" }
                                                    }
                                                },
                                                ""schemaVersion"": 1
                                            }", lcutoken, "PATCH");
                                    Console.Clear();
                                    Info($"基础伤害加成已设置为 {result}");
                                    Console.WriteLine();
                                }
                                else
                                {
                                    Info("输入的数值不正确，请输入整数！");
                                }
                            }
                            else if (g.Key == ConsoleKey.K)
                            {
                                Console.WriteLine();
                                Console.Write("请输入暴击几率：");
                                if (int.TryParse(Console.ReadLine(), out var result))
                                {
                                    Post($"https://127.0.0.1:{port}/lol-settings/v2/account/LCUPreferences/lol-home-hubs", @"{
                                                ""data"": {
                                                    ""boba-minigame"": {
                                                        ""Version"": 1,
                                                         ""runBonusStats"":{ ""BonusCritChance"":" + result + @" }
                                                    }
                                                },
                                                ""schemaVersion"": 1
                                            }", lcutoken, "PATCH");
                                    Console.Clear();
                                    Info($"暴击几率已设置为 {result}");
                                    Console.WriteLine();
                                }
                                else
                                {
                                    Info("输入的数值不正确，请输入整数！");
                                }
                            }
                            else if (g.Key == ConsoleKey.M)
                            {
                                Console.WriteLine();
                                Console.Write("请输入金币：");
                                if (int.TryParse(Console.ReadLine(), out var result))
                                {
                                    Post($"https://127.0.0.1:{port}/lol-settings/v2/account/LCUPreferences/lol-home-hubs", @"{
                                                ""data"": {
                                                    ""boba-minigame"": {
                                                        ""Version"": 1,
                                                         ""currency"":" + result + @"
                                                    }
                                                },
                                                ""schemaVersion"": 1
                                            }", lcutoken, "PATCH");
                                    Console.Clear();
                                    Info($"金币已设置为 {result}");
                                    Console.WriteLine();
                                }
                                else
                                {
                                    Info("输入的数值不正确，请输入整数！");
                                }
                            }
                        }
                    }
                    else if (g.Key == ConsoleKey.J)
                    {
                        Console.Clear();
                        var json = web.DownloadString($"https://127.0.0.1:{port}/lol-rewards/v1/grants");
                        var root = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Class1>>(json);
                        if (!root.Any())
                        {
                            Info("当前账号没有可领取的奖励！");
                        }
                        else
                        {
                            foreach (var item in root.OrderBy(x => x.info.dateCreated))
                            {
                                var ids = new List<string>();
                                var names = new List<string>();
                                foreach (var item2 in item.rewardGroup.rewards)
                                {
                                    if (!string.IsNullOrWhiteSpace(item2.localizations.details) || item.info.status == "FULFILLED")
                                    {
                                        Error("奖励:" + item2.localizations.title + " -> " + (string.IsNullOrWhiteSpace(item2.localizations.details) ? "已被领取或无法领取" : item2.localizations.details));
                                    }
                                    else
                                    {
                                        ids.Add(item2.id);
                                        names.Add(item2.localizations.title);
                                    }
                                }
                                if (ids.Count > 0)
                                {
                                    var post = Post($"https://127.0.0.1:{port}/lol-rewards/v1/grants/" + item.info.id + "/select", Newtonsoft.Json.JsonConvert.SerializeObject(new
                                    {
                                        grantId = item.info.id,
                                        rewardGroupId = item.info.rewardGroupId,
                                        selections = ids
                                    }), lcutoken);
                                    if (post.Contains("errorCode"))
                                    {
                                        Console.WriteLine(post);
                                    }
                                    else
                                    {
                                        foreach (var item3 in names)
                                        {
                                            Info("奖励:" + item3 + " -> 执行领取！");
                                        }
                                    }
                                }
                            }
                            Console.WriteLine();
                            Info("所有奖励领取完毕！");
                            Console.WriteLine();
                        }
                        Console.WriteLine();
                        goto select;
                    }

                }
            }
            Info("按任意键退出！");
            Console.ReadKey();
        }

        public class Class1
        {
            public Info2 info { get; set; }
            public Rewardgroup rewardGroup { get; set; }
        }

        public class Info2
        {
            public DateTime dateCreated { get; set; }
            public object[] grantElements { get; set; }
            public string granteeId { get; set; }
            public Grantordescription grantorDescription { get; set; }
            public string id { get; set; }
            public Messageparameters messageParameters { get; set; }
            public string rewardGroupId { get; set; }
            public object[] selectedIds { get; set; }
            public string status { get; set; }
            public bool viewed { get; set; }
        }

        public class Grantordescription
        {
            public string appName { get; set; }
            public string entityId { get; set; }
        }

        public class Messageparameters
        {
        }

        public class Rewardgroup
        {
            public bool active { get; set; }
            public string celebrationType { get; set; }
            public object[] childRewardGroupIds { get; set; }
            public string id { get; set; }
            public Localizations localizations { get; set; }
            public Media media { get; set; }
            public string productId { get; set; }
            public string rewardStrategy { get; set; }
            public Reward[] rewards { get; set; }
            public Selectionstrategyconfig selectionStrategyConfig { get; set; }
            public object[] types { get; set; }
        }

        public class Localizations
        {
            public string description { get; set; }
            public string title { get; set; }
        }

        public class Media
        {
        }

        public class Selectionstrategyconfig
        {
            public int maxSelectionsAllowed { get; set; }
            public int minSelectionsAllowed { get; set; }
        }

        public class Reward
        {
            public string fulfillmentSource { get; set; }
            public string id { get; set; }
            public string itemId { get; set; }
            public string itemType { get; set; }
            public Localizations1 localizations { get; set; }
            public Media1 media { get; set; }
            public int quantity { get; set; }
        }

        public class Localizations1
        {
            public string details { get; set; }
            public string title { get; set; }
        }

        public class Media1
        {
            public string iconUrl { get; set; }
        }


        private static bool IsRunAsAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public class MissionsData
        {



            public long completedDate { get; set; }
            public string completionExpression { get; set; }
            public int cooldownTimeMillis { get; set; }
            public string description { get; set; }

            public string displayType { get; set; }
            public long earnedDate { get; set; }
            public long endTime { get; set; }

            public string helperText { get; set; }
            public string iconImageUrl { get; set; }
            public string id { get; set; }
            public string internalName { get; set; }
            public bool isNew { get; set; }
            public long lastUpdatedTimestamp { get; set; }
            public string locale { get; set; }

            public string missionLineText { get; set; }
            public string missionType { get; set; }
            public Objective[] objectives { get; set; }


            public Reward[] rewards { get; set; }
            public int sequence { get; set; }
            public string seriesName { get; set; }
            public long startTime { get; set; }
            public string status { get; set; }
            public string title { get; set; }
            public bool viewed { get; set; }









            public class Objective
            {
                public string description { get; set; }
                public bool hasObjectiveBasedReward { get; set; }
                public Progress progress { get; set; }
                public string[] requirements { get; set; }
                public string[] rewardGroups { get; set; }
                public int sequence { get; set; }
                public string status { get; set; }
                public string type { get; set; }
            }

            public class Progress
            {
                public int currentProgress { get; set; }
                public int lastViewedProgress { get; set; }
                public int totalCount { get; set; }
            }

            public class Reward
            {
                public string description { get; set; }
                public bool iconNeedsFrame { get; set; }
                public string iconUrl { get; set; }
                public bool isObjectiveBasedReward { get; set; }
                public string itemId { get; set; }
                public int quantity { get; set; }
                public bool rewardFulfilled { get; set; }
                public string rewardGroup { get; set; }
                public bool rewardGroupSelected { get; set; }
                public string rewardType { get; set; }
                public int sequence { get; set; }
                public string smallIconUrl { get; set; }
                public string uniqueName { get; set; }
            }


        }



        private static string Post(string uri, string param, string token, string method = "POST")
        {
            try
            {


                using (var web2 = new WebClient())
                {
                    web2.Encoding = Encoding.UTF8;
                    web2.Headers.Add("User-Agent: Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) RiotClient/81.0.0 (CEF 74) Safari/537.36");
                    web2.Headers.Add("Accept-Language: en-US,en;q=0.9");
                    web2.Headers.Add("Accept: */*");
                    web2.Headers.Add($"Referer: https://{new Uri(uri).Host}/index.html");
                    web2.Headers.Add("Authorization", token);
                    web2.Headers.Add("Content-Type", "application/json");
                    return web2.UploadString(uri, method, param);
                }
            }
            catch (WebException webex)
            {
                if (webex.Response != null)
                {
                    using (StreamReader sr = new StreamReader(webex.Response.GetResponseStream()))
                    {
                        return sr.ReadToEnd();
                    }
                }
                else
                {
                    throw webex;
                }
            }
        }



        public static void Info(string log, bool newLine = true)
        {
            Log(log, "I", newLine);
        }

        public static void Error(string log, bool newLine = true)
        {
            var bf = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Log(log, "E", newLine);
            Console.ForegroundColor = bf;
        }


        public static void Log(string log, string level, bool newLine)
        {
            Console.Write(string.Format("{0} [{1}] {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), level, log));
            if (newLine)
            {
                Console.WriteLine();
            }
        }

        #region 方法1

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, uint processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [StructLayout(LayoutKind.Sequential)]
        public struct UNICODE_STRING
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr Buffer;
        }


        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern uint NtQueryInformationProcess(
               IntPtr processHandle,
               int processInformationClass,
               IntPtr pbi,
               uint processInformationLength,
               out IntPtr returnLength
           );

        public static string GetCommandLine1(uint pid)
        {
            IntPtr hProcess = OpenProcess(0x1000, false, pid);
            if (hProcess == IntPtr.Zero)
                throw new Exception("Failed to open process.");
            try
            {
                IntPtr returnLength = IntPtr.Zero;
                uint status = NtQueryInformationProcess(hProcess, 60, IntPtr.Zero, 0, out returnLength);
                if (status != 0xC0000004 && status != 0xC0000023 && status != 0xC0000046) // 检查是否是需要缓冲区的错误
                {
                    throw new Exception($"NtQueryInformationProcess failed with status: 0x{status:X}");
                }
                int bufferSize = returnLength.ToInt32();
                byte[] buffer = new byte[bufferSize];
                IntPtr bufferPtr = Marshal.AllocHGlobal(bufferSize);
                try
                {
                    status = NtQueryInformationProcess(hProcess, 60, bufferPtr, (uint)bufferSize, out returnLength);
                    if (status != 0)
                    {
                        throw new Exception($"NtQueryInformationProcess failed with status: 0x{status:X}");
                    }
                    Marshal.Copy(bufferPtr, buffer, 0, bufferSize);
                    UNICODE_STRING us = Marshal.PtrToStructure<UNICODE_STRING>(bufferPtr);
                    byte[] commandLineBytes = new byte[us.Length];
                    Marshal.Copy(us.Buffer, commandLineBytes, 0, us.Length);
                    return Encoding.Unicode.GetString(commandLineBytes);
                }
                finally
                {
                    Marshal.FreeHGlobal(bufferPtr);
                }
            }
            finally
            {
                CloseHandle(hProcess);
            }
        }

        #endregion

        private static List<Tuple<string, uint>> GetCommandLines(string processName = "nopad++.exe")
        {
            List<Tuple<string, uint>> list = new List<Tuple<string, uint>>();
            string queryString = "select CommandLine,ProcessId from Win32_Process where Name='" + processName + "'";
            using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(queryString))
            {
                using (ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get())
                {
                    foreach (ManagementBaseObject managementBaseObject in managementObjectCollection)
                    {
                        ManagementObject managementObject = (ManagementObject)managementBaseObject;
                        list.Add(new Tuple<string, uint>((string)managementObject["CommandLine"], (uint)managementObject["ProcessId"]));
                    }
                }
            }
            return list;
        }
    }
}
