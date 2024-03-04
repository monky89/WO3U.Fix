using System.Diagnostics;
using Vanara.PInvoke;

namespace WO3U.Fix
{
    class Program
    {
        private const string STEAM_RUN_CMD = @"steam://rungameid/1879330";
        private const string PROCESS_NAME = @"WO3U";

        private static void StartSteamGame()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = STEAM_RUN_CMD,
                UseShellExecute = true
            };

            using (var process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();

                process.WaitForExit();
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Trying to set RefreshRate to 60Hz...");

            var devMode = new DEVMODE();
            User32.EnumDisplaySettings(null, User32.ENUM_CURRENT_SETTINGS, ref devMode);

            uint defaultFrequency = devMode.dmDisplayFrequency;
            devMode.dmDisplayFrequency = 60; // Max FPS for Warriors Orochi 3 Ultimate

            int result = User32.ChangeDisplaySettings(in devMode, User32.ChangeDisplaySettingsFlags.CDS_DEFAULT);
            if (result != 0)
            {
                Console.WriteLine("Oh no! :(" + Environment.NewLine + "Couldn't set DisplayFrequency.");
                return;
            }

            Console.WriteLine("RefreshRate got set. Starting Warriors Orochi 3 Ultimate...");

            bool cancel = false;
            Console.CancelKeyPress += (sender, e) => {
                e.Cancel = true;
                cancel = true;
            };

            try
            {
                StartSteamGame();
                Console.WriteLine("Waiting for the Game to run. Press Ctrl+C to abort.");

                Process? wo3uProcess = null;
                while (wo3uProcess is null)
                {
                    if (cancel) return;

                    Console.WriteLine("Wait for WO3U.exe...");
                    Thread.Sleep(1000);
                    wo3uProcess = Process.GetProcessesByName(PROCESS_NAME).FirstOrDefault();
                }

                Console.WriteLine($"Found WO3U.exe with ProcessID {wo3uProcess.Id}");

                wo3uProcess.WaitForExit();
                Console.WriteLine("Game exited. Reset RefreshRate...");
            }
            finally
            {
                // Reset RefreshRate
                devMode.dmDisplayFrequency = defaultFrequency;
                User32.ChangeDisplaySettings(in devMode, User32.ChangeDisplaySettingsFlags.CDS_DEFAULT);
            }
        }
    }
}