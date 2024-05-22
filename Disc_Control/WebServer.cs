using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Disc_Control
{
    internal class WebServer
    {
        public static HttpListener listener;
        public static string url = "http://localhost:8000/";
        public static int pageViews = 0;
        private static StringWriter consoleOutput = new StringWriter();
        private static DualWriter dualWriter;

        public static string pageData =
            "<!DOCTYPE html>" +
            "<html>" +
            "  <head>" +
            "    <title>Drive Information</title>" +
            "    <meta http-equiv=\"refresh\" content=\"1\">" +
            "  </head>" +
            "  <body>" +
            "    <h1>Drive Information</h1>" +
            "    <p>Page Views: {0}</p>" +
            "    <pre>{1}</pre>" + 
            "  </body>" +
            "</html>";

        public static async Task HandleIncomingConnections()
        {
            bool runServer = true;

            while (runServer)
            {
                HttpListenerContext ctx = await listener.GetContextAsync();

                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                if (req.HttpMethod == "POST" && req.Url.AbsolutePath == "/shutdown")
                {
                    runServer = false;
                }
                else
                {
             
                    string driveInfo = Program.Drives();

                  
                    string disableSubmit = !runServer ? "disabled" : "";
                    byte[] data;
                    if (req.Url.AbsolutePath == "/")
                    {
                        data = Encoding.UTF8.GetBytes(string.Format(pageData, ++pageViews, WebUtility.HtmlEncode(driveInfo)));
                    }
                   
                    else if (req.Url.AbsolutePath == "/drive-info")
                    {
                        data = Encoding.UTF8.GetBytes(driveInfo);
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
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine("Listening for connections on {0}", url);

    
            dualWriter = new DualWriter(Console.Out, consoleOutput);
            Console.SetOut(dualWriter);
            Console.SetError(dualWriter);

            await HandleIncomingConnections();

            listener.Close();
            Console.WriteLine("Server has shut down.");
        }

        public static void RestoreConsoleOutput()
        {
            var standardOutput = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            Console.SetOut(standardOutput);
            var standardError = new StreamWriter(Console.OpenStandardError()) { AutoFlush = true };
            Console.SetError(standardError);
        }
    }
}
