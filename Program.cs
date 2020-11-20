using System;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Windows;

namespace Entry_Builder
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 2 && args[0] == "download")
            {
                downloadMethod(args[1]);
                return;
            }
            else if (args.Length == 1 && args[0] == "build")
            {
                return;
            }

            downloadMethod("5efc61012dc8ef002be36378");
            //buildMethod();

            Console.WriteLine(
                "enbuild download <projectID> : 엔트리 서버에서 프로젝트를 다운로드합니다. \n" +
                "enbuild build : 다운로드된 프로젝트를 빌드합니다.\n"
            );
        }

        static void buildMethod()
        {

        }

        static void downloadMethod(string projectID)
        {
            Console.Write("메인 프로젝트 다운로드 중 - ");

            WebRequest request = WebRequest.Create($"https://playentry.org/api/project/{projectID}");
            request.Credentials = CredentialCache.DefaultCredentials;

            WebResponse response;
            try
            {
                response = request.GetResponse();
            }
            catch (WebException)
            {
                Console.WriteLine("찾을 수 없는 프로젝트이거나 다운로드할 수 없습니다.");
                return;
            }

            string responseFromServer;
            using (Stream dataStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(dataStream);
                responseFromServer = reader.ReadToEnd();
            }
            response.Close();

            string sDirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\resources";
            DirectoryInfo di = new DirectoryInfo(sDirPath);
            if (di.Exists == false)
            {
                di.Create();
            }

            if (!File.Exists("resources\\project.entbuild"))
            {
                File.Create("resources\\project.entbuild");
            }
            File.WriteAllText("resources\\project.entbuild", responseFromServer, System.Text.Encoding.UTF8);

            JObject mainProject = JObject.Parse(responseFromServer);
            Console.WriteLine("완료");


            List<string> images = new List<string>();
            List<string> sounds = new List<string>();
            Console.Write("필요한 이미지, 소리 찾는 중 - ");

            foreach (var i in mainProject["objects"])
            {
                foreach (var j in i["sprite"]["pictures"])
                {
                    try { images.Add(j["filename"].ToString()); }
                    catch { images.Add($";{j["fileurl"]}"); }
                }
                foreach (var j in i["sprite"]["sounds"])
                {
                    try { sounds.Add(j["filename"].ToString()); }
                    catch { sounds.Add($";{j["fileurl"]}"); }
                }
            }
            Console.WriteLine($"이미지 {images.Count}개, 소리 {sounds.Count}개 발견됨");

            foreach (var item in images)
            {
                using (WebClient client = new WebClient())
                {
                    if (item[0] == ';')
                    {
                        Console.Write($"{item.Substring(1)} 다운로드 시작 - ");
                        client.DownloadFile(new Uri($"https://playentry.org{item.Substring(1)}"), $"resources\\{item.Substring(1).Replace('/', ' ')}");
                        Console.WriteLine("완료");
                    }
                    else
                    {
                        Console.Write($"{item}.png 다운로드 시작 - ");
                        client.DownloadFile(new Uri($"https://playentry.org/uploads/{item.Substring(0, 2)}/{item.Substring(2, 2)}/image/{item}.png"), $"resources\\{item}.png");
                        Console.WriteLine("완료");
                    }
                }
            }
            foreach (var item in sounds)
            {
                using (WebClient client = new WebClient())
                {
                    Console.Write($"{item}.mp3 다운로드 시작 - ");
                    client.DownloadFile(new Uri($"https://playentry.org/uploads/{item.Substring(0, 2)}/{item.Substring(2, 2)}/{item}.mp3"), $"resources\\{item}.mp3");
                    Console.WriteLine("완료");
                }
            }
        }
    }
}