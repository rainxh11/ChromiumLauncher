using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Win32;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;

namespace ChromiumLauncher
{
    class Program
    {
		///-------------------------------------------------------------------------------------------------------------///
         /* method to launch "Google Chrome / Chromium" executable
         * iterate through every "chrome.exe" available in the current launcher directory & sub-directories
         * execute one each time and waiting for the process to exit before executing the next one
         */
        static void LaunchChrome(string currentDir, string commandLineArguments, string cacheDir)
        {
			string cacheDirCommandLineArgument = $" --disk-cache-dir={cacheDir}";
            Process chrome = new Process();
            chrome.StartInfo.Arguments = commandLineArguments + cacheDirCommandLineArgument;

            DirectoryInfo dir = new DirectoryInfo(currentDir);
            try
            {
                foreach (FileInfo file in dir.GetFiles("chrome.exe", SearchOption.AllDirectories))
                    {
                        chrome.StartInfo.FileName = file.FullName;
                        chrome.Start();
                        chrome.WaitForExit();
                        break;
                    }
            }
            catch{}
        }
		///-------------------------------------------------------------------------------------------------------------///
        /*
         * method that create a temporary directory in the %TEMP% directory
         * and return the path of the created directory
         * 
         * - isFolderNameRandom : toggle whether the temporary folder created is randomely named
         */
		private static string GetTemporaryDirectory(bool isFolderNameRandom)
        {
            string randomFileName = "";
            if (isFolderNameRandom)
            {
                randomFileName = Path.GetRandomFileName();
            }
            string path = Path.Combine(Path.GetTempPath(), "XHCH_Chromium" + randomFileName + "\\"); 
            try
			{
				Directory.CreateDirectory(path);
				return path;
			}
			catch
			{
				return null;
			}
        }
		///-------------------------------------------------------------------------------------------------------------///
        private static void CleanupCache(string profileDir, string cacheDir)
        {
            try
            {
                Directory.Delete(cacheDir, true);
                Directory.Delete(profileDir + $@"\ShaderCache", true);
                Directory.Delete(profileDir + $@"\BrowserMetrics", true);
                Directory.Delete(profileDir + $@"\Default\blob_storage", true);
                Directory.Delete(profileDir + $@"\Default\Cache", true);
                Directory.Delete(profileDir + $@"\Default\Code Cache", true);
                Directory.Delete(profileDir + $@"\Default\GPUCache", true);
            }
            catch { }
        }
		///-------------------------------------------------------------------------------------------------------------///
        static void Cleanup(string profileDir)
        {
            try
            {
                Directory.Delete(profileDir, true);
            }
            catch { }
        }
        ///-------------------------------------------------------------------------------------------------------------///
        static void SetGoogleAPIs(string apiKey, string clientId, string clientSecret)
        {
            /*
             GOOGLE_API_KEY=your_api_key
             GOOGLE_DEFAULT_CLIENT_ID=your_client_id
             GOOGLE_DEFAULT_CLIENT_SECRET=your_client_secret
             */
            try
            {
                Environment.SetEnvironmentVariable("GOOGLE_API_KEY"
                                                    , apiKey, EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("GOOGLE_DEFAULT_CLIENT_ID"
                                                    , clientId, EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("GOOGLE_DEFAULT_CLIENT_SECRET"
                                                    , clientSecret, EnvironmentVariableTarget.Process);
            }
            catch { }
        }
        ///-------------------------------------------------------------------------------------------------------------///
        static void Main(string[] args)
        {
			string tempDir = null;
            string currentDir = AppContext.BaseDirectory;
            string profileDir = currentDir + "Profile";
            string cacheDir = currentDir + "ProfileCache";
            //profileDir += "Incognito";
			bool keepCache = false;
            bool tempCache = false;

            string chromeExe = currentDir + @"\Bin\chrome.exe";
            string exeArguments = $"--user-data-dir=\"{profileDir}\" --disable-smooth-scrolling --enable-overlay-scrollbar --disable-notifications --enable-quic --no-default-browser-check --disable-crash-reporter --disable-plugins --allow-insecure-localhost --enable-parallel-downloading";

            exeArguments += " --enable-features=OverlayScrollbar,OverlayScrollbarFlashAfterAnyScrollUpdate,OverlayScrollbarFlashWhenMouseEnter";
            ///---------- Some GPU Acceleration bullshit:
            exeArguments += " --force-gpu-rasterization "; // --gpu-rasterization-msaa-sample-count=4";
            ///---------- Enabling Incongnito:
            //exeArguments += " --incognito";

            ///--------------- Set Google's API Keys (for no-sync chromium builds)-//
            SetGoogleAPIs("AIzaSyDmI4dtlAJ8m9GVKhgAneFZZ9eJfgU9SYw",
                          "574734135153-9t0gvq8s4o4g7np765b0069gbevq6n91.apps.googleusercontent.com",
                          "46fUpJcn9hQ62n5rDOZBM2Am");
            ///------------------------------------------------------------------////
            
            if (args.Length != 0)
            {
                foreach(string arg in args)
                {
					if(arg != "--tempcache" || arg != "--keepcache")
                    {
						exeArguments += " " + arg;
					}
					if(arg == "--tempcache")
					{
                        tempCache = true;
					}  
					if(arg == "--keepcache")
					{
						keepCache = true;
					}
                }
            }
            if(tempCache)
            {
                tempDir = GetTemporaryDirectory(!keepCache);
                if (tempDir != null)
                {
                    cacheDir = tempDir + "ProfileCache";
                }
            }
			///------------------------------------------------------------------////
            if (!Directory.Exists(profileDir))
            {
                try
                {
                    Directory.CreateDirectory(profileDir);
                }
                catch { }
            }

            //LaunchChrome(chromeExe, exeArguments
			if(keepCache == false)
            { 
                CleanupCache(profileDir, cacheDir);
            }
            LaunchChrome(currentDir, exeArguments, cacheDir);
            if(keepCache == false)
            {
                CleanupCache(profileDir, cacheDir);
				Cleanup(tempDir);
			}
            //Cleanup(profileDir);

        }
    }
}
