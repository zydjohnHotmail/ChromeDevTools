﻿
namespace MasterDevs.ChromeDevTools
{


    public class ChromeProcessFactory : IChromeProcessFactory
    {
        public IDirectoryCleaner DirectoryCleaner { get; set; }
        public string ChromePath { get; }


        public ChromeProcessFactory(IDirectoryCleaner directoryCleaner, string chromePath)
        {
            DirectoryCleaner = directoryCleaner;
            ChromePath = chromePath;
        }


        public ChromeProcessFactory(IDirectoryCleaner directoryCleaner)
            :this(directoryCleaner, ChromeProcessFactoryHelper.DefaultChromePath)
        { }


        public IChromeProcess Create(int port, bool headless)
        {
            string path = System.IO.Path.GetRandomFileName();
            System.IO.DirectoryInfo directoryInfo = System.IO.Directory.CreateDirectory(
                System.IO.Path.Combine(
                    System.IO.Path.GetTempPath(), path)
            );

            string remoteDebuggingArg = $"--remote-debugging-port={port}";
            string userDirectoryArg = $"--user-data-dir=\"{directoryInfo.FullName}\"";
            const string headlessArg = "--headless --disable-gpu";

            // https://peter.sh/experiments/chromium-command-line-switches/
            System.Collections.Generic.List<string> chromeProcessArgs = 
                new System.Collections.Generic.List<string>
            {
                remoteDebuggingArg,
                userDirectoryArg,
                // Indicates that the browser is in "browse without sign-in" (Guest session) mode. 
                // Should completely disable extensions, sync and bookmarks.
                "--bwsi", 
                "--no-first-run"
            };

            if (headless)
                chromeProcessArgs.Add(headlessArg);

            System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo(ChromePath, string.Join(" ", chromeProcessArgs));
            System.Diagnostics.Process chromeProcess = System.Diagnostics.Process.Start(processStartInfo);

            string remoteDebuggingUrl = "http://localhost:" + port;
            return new LocalChromeProcess(new System.Uri(remoteDebuggingUrl), () => DirectoryCleaner.Delete(directoryInfo), chromeProcess);
        }


    }


}