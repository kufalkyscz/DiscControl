using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Display;

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

                    // Split the driveInfo string into lines
                    string[] lines = SplitIntoLines(driveInfo);

                    StringBuilder htmlBuilder = new StringBuilder();

                    // Add the header and separator lines
                    htmlBuilder.Append("<pre>");
                    htmlBuilder.Append(WebUtility.HtmlEncode(lines[0]));
                    htmlBuilder.Append("<br>");
                    htmlBuilder.Append(WebUtility.HtmlEncode(lines[1]));
                    htmlBuilder.Append("</pre><br>");

                    // Process the data lines
                    for (int i = 2; i < lines.Length; i++)
                    {
                        // Encode the line
                        string encodedLine = WebUtility.HtmlEncode(lines[i]);

                        // Check if the third column has a number
                        string[] columns = encodedLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (columns.Length > 2 && double.TryParse(columns[2].TrimEnd('%'), out double percentage))
                        {
                            if (percentage <= 10)
                            {
                                htmlBuilder.Append("<span style=\"background-color:red;\"><strong>");
                                htmlBuilder.Append(encodedLine);
                                htmlBuilder.Append("</strong></span>");
                            }
                            else if (percentage > 10 && percentage <= 25)
                            {
                                htmlBuilder.Append("<span style=\"background-color:orange;\"><strong>");
                                htmlBuilder.Append(encodedLine);
                                htmlBuilder.Append("</strong></span>");
                            }
                            else if (percentage > 25)
                            {
                                htmlBuilder.Append("<span style=\"background-color:lightgreen;\"><strong>");
                                htmlBuilder.Append(encodedLine);
                                htmlBuilder.Append("</strong></span>");
                            }
                        }
                        else
                        {
                            htmlBuilder.Append("<span style=\"color:black;\">");
                            htmlBuilder.Append(encodedLine);
                            htmlBuilder.Append("</span>");
                        }
                        htmlBuilder.Append("<br>");
                    }

                    byte[] data;
                    if (req.Url.AbsolutePath == "/")
                    {
                        data = Encoding.UTF8.GetBytes(string.Format(pageData, ++pageViews, htmlBuilder.ToString()));
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

        private static string[] SplitIntoLines(string text)
        {
            var lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length < 2) return lines;

            var header = lines[0];
            var separator = lines[1];
            var dataLines = lines.Skip(2).ToArray();

            var result = new List<string> { header, separator };
            result.AddRange(dataLines);

            return result.ToArray();
        }


        private static bool ContainsNumber10OrLess(string line)
        {
            foreach (char c in line)
            {
                if (char.IsDigit(c))
                {
                    int number = int.Parse(c.ToString());
                    if (number <= 10)
                    {
                        return true;
                    }
                }
            }
            return false;
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
