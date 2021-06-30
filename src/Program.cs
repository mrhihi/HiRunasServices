using System;
using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace HiStartSrv
{
    class Program
    {
        static Process p;

        public static Process StartProcess(string exePath, string args)
        {
            ProcessStartInfo sinfo = new ProcessStartInfo
            {
                Arguments = args,
                FileName = exePath,
                WorkingDirectory = Path.GetDirectoryName(exePath)
            };
            return Process.Start(sinfo);
        }

        public static void StopProcess()
        {
            if (p != null) {
                if (!p.HasExited) {
                    p.CloseMainWindow();
                    p.Kill(true);
                }
            }
        }
        ~Program()
        {
            StopProcess();            
        }

        public static void ShowException(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        public static void Main(string[] args)
        {
            try {
                if (args.Length == 0) throw new Exception("請輸入 .ini 路徑。");
                if (!File.Exists(args[0])) throw new Exception($"請確認 {args[0]} 檔案是否存在。");
                Console.WriteLine($"{args[0]} 載入中 ...");
                var iniReader = new IniFile(args[0]);
                string exePath =  iniReader.Read("ExePath", "Service")??"";
                string arguments = iniReader.Read("Arguments")??"";
                if (exePath == null || !File.Exists(exePath)) throw new Exception($"執行檔 {exePath??"?"} 不存在。");

                AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

                try
                {
                    var host = CreateHostBuilder(args).Build();
                    p = StartProcess(exePath, arguments);
                    Console.WriteLine($"Process run: {p.Id}");

                    host.Run();
                }
                catch (Exception)
                {
                    StopProcess();
                }
            }
            catch(Exception ex)
            {
                ShowException(ex);
            }
        }

        public static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            StopProcess();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService();
    }
}
