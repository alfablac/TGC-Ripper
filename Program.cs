using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace TGCPripper
{
    class Program
    {
        private static double GetFileSize(string uriPath)
        {
            var webRequest = HttpWebRequest.Create(uriPath);
            webRequest.Method = "HEAD";

            using (var webResponse = webRequest.GetResponse())
            {
                double fileSize = Convert.ToDouble(webResponse.Headers.Get("Content-Length"));
                double fileSizeInMegaByte = Math.Round(Convert.ToDouble(fileSize) / 1024 / 1024, 2);
                return fileSizeInMegaByte;
            }
        }

        static void Main(string[] args)
        {
            new Program().Ripper();
        }

        private void Ripper()
        {
            StringBuilder path = new StringBuilder();
            StringBuilder cookie_file = new StringBuilder();
            StringBuilder output_file = new StringBuilder();

            if (File.Exists(@"cookies.txt"))
            {
                Console.WriteLine("Cookies found!");
            }
            else
            {
                Console.WriteLine("Import your cookies.txt file and place in the same folder containing the executable");

                return;
            }

            try
            {
                using (StreamReader sr = new StreamReader(@"cookies.txt"))
                {
                    String line;
                    int contador = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        contador++;
                        if (contador > 7)
                        {
                            string[] split = line.Split('\t');
                            cookie_file.Append(split[5]).Append("=").Append(split[6]).Append("; ");
                        }
                    }
                }
            }
            catch (Exception e)
            {

                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);

            }

            ChromeOptions options = new ChromeOptions();
            options.AddArguments("--headless", "--silent", "--log-level=3", "--allow-file-access-from-files", "--disable-gpu");
            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            IWebDriver driver = new ChromeDriver(service, options);

            try
            {
                using (StreamReader sr = new StreamReader(@"list.txt"))
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string path_to_courseURL = line;

                        Console.WriteLine("\n\n Scrapping page " + path_to_courseURL + "\n\n");
                        driver.Navigate().GoToUrl(path_to_courseURL);
                        IList<IWebElement> elements = driver.FindElements(By.TagName("a"));
                        string course_name = driver.FindElement(By.XPath("//*[@class='course-info']/h1")).Text;
                        int counter = 0, pos_start = 1;
                        foreach (IWebElement element in elements)
                        {
                            if (element.GetAttribute("data-film-id") != null)
                            {
                                WebClient webClient = new WebClient();
                                webClient.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36";
                                webClient.Headers["cookie"] = cookie_file.ToString();
                                webClient.Headers["dnt"] = "1";
                                string video_url = webClient.DownloadString("https://www.thegreatcoursesplus.com/embed/player?filmId=" + element.GetAttribute("data-film-id"));
                                var file_regex = new Regex(@"\b(?:https?://vt)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                                List<string> links_to_video = new List<string>();
                                foreach (Match m in file_regex.Matches(video_url))
                                    links_to_video.Add(m.Value);
                                String download_video_url = "";
                                double maior = 0;
                                foreach (string item in links_to_video)
                                {
                                    if (GetFileSize(item) > maior)
                                    {
                                        maior = GetFileSize(item);
                                        download_video_url = item;
                                    }
                                }

                                string title = element.GetAttribute("data-title");
                                title = title.Replace(":", "").Replace("?", "");
                                course_name = course_name.Replace(":", "");
                                course_name = course_name.Replace("?", "");
                                Console.WriteLine("\n\n-----Downloading Lecture " + pos_start.ToString() + " of Course " + course_name);
                                string args = "-s16 -j16 -x16 --file-allocation=none --console-log-level=error -o \"\\" + course_name + "\\Lecture " + pos_start.ToString() + " - " + title + ".mp4\" " + download_video_url;
                                Console.WriteLine(args);
                                ProcessStartInfo start_aria = new ProcessStartInfo(@"aria2c", args);
                                start_aria.UseShellExecute = false;
                                var proc = Process.Start(start_aria);
                                proc.WaitForExit();

     
                                pos_start++;

                            }
                            counter++;
                        }
                        driver.Close();
                        driver.Quit();

                    }
                }
            }
            catch (Exception e)
            {

                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);

            }

        }
     }
}
