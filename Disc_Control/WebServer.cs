using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using dotless.Core;

namespace Disc_Control
{
    internal class WebServer
    {
        public static HttpListener Listener;
        public static string Url;
        public static int PageViews = 0;
        private static StringWriter consoleOutput = new StringWriter();
        private static DualWriter dualWriter;

        public static string PageData =
            "<!DOCTYPE html>" +
            "<html>" +
            "  <head>" +
            "    <title>Drive Information</title>" +
            "    <meta http-equiv=\"refresh\" content=\"1\">" +
            "    <style>{3}</style>" +
            "  </head>" +
            "  <body>" +
            "    <h1>Drive Information</h1>" +
            "    <h3>Configuration</h3>" +
            "    <pre class=\"config\">{2}</pre>" +
            "    <p>Number of refreshes: {0}</p>" +
            "    <h3>Drive information</h3>" +
            "    <div class=\"container\">{1}</div>" + 
            "  </body>" +
            "</html>";

        public static async Task HandleIncomingConnections()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetParent(baseDirectory).FullName;
            string configPath = Path.Combine(projectDirectory, "config.json");
            string lessFilePath = Path.Combine(projectDirectory, "main.less");

            bool runServer = true;
            var config = new Config();

            while (runServer)
            {
                HttpListenerContext ctx = await Listener.GetContextAsync();

                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                if (req.HttpMethod == "POST" && req.Url.AbsolutePath == "/shutdown")
                {
                    runServer = false;
                }
                else
                {
                    var drives = Drive.GetDrives();
                    StringBuilder htmlBuilder = new StringBuilder();

                    foreach (var drive in drives.Values)
                    {
                        htmlBuilder.Append(drive.ToHTML());
                    }

                    string configContent = File.ReadAllText(configPath);
                    string cssContent = GetCssFromLess(lessFilePath);

                    byte[] data;
                    if (req.Url.AbsolutePath == "/")
                    {
                        data = Encoding.UTF8.GetBytes(string.Format(PageData, ++PageViews, htmlBuilder.ToString(), configContent, cssContent));
                    }
                    else if (req.Url.AbsolutePath == "/drive-info")
                    {
                        data = Encoding.UTF8.GetBytes(htmlBuilder.ToString());
                    }
                    else
                    {
                        resp.StatusCode = (int)HttpStatusCode.NotFound;
                        resp.Close();
                        continue;
                    }

                    resp.ContentType = "text/html";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = data.LongLength;
                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                    resp.Close();
                }
            }
        }

        public static async Task StartServerAsync()
        {
            var config = new Config();
            Url = $"http://localhost:{config.Port}/";
            Listener = new HttpListener();
            Listener.Prefixes.Add(Url);
            Listener.Start();
            Console.WriteLine("Listening for connections on {0}", Url);

            dualWriter = new DualWriter(Console.Out, consoleOutput);
            Console.SetOut(dualWriter);
            Console.SetError(dualWriter);

            await HandleIncomingConnections();

            Listener.Close();
            Console.WriteLine("Server has shut down.");
        }

        public static void RestoreConsoleOutput()
        {
            var standardOutput = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            Console.SetOut(standardOutput);
            var standardError = new StreamWriter(Console.OpenStandardError()) { AutoFlush = true };
            Console.SetError(standardError);
        }

        private static string GetCssFromLess(string lessFilePath)
        {
            string lessContent = File.ReadAllText(lessFilePath);
            return Less.Parse(lessContent);
        }
    }
}
