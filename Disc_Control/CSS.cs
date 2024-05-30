using dotless.Core.Parser.Tree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disc_Control
{
    internal class CSS
    {
        public CSS()
        {
            CheckCSS();
        }

        internal static void CheckCSS()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetParent(baseDirectory).FullName;
            string lessFilePath = Path.Combine(projectDirectory, "main.less");

            if (!File.Exists(lessFilePath))
            {
                string lessContent = 
                 @"
body{
    font-family: Arial, sans-serif;
    margin: 0;
    padding: 20px;
    }

.container {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
    gap: 20px;
    }

.container > div {
    padding: 10px;
    border: 1px solid black;
    }

.critical_threshold {
    background-color: red;
    }

.warning_threshold {
    background-color: orangered;
    }

.normal {
    background-color: lightgreen;
    }

.config {
    font-size: 15px;
    display: block;
    }
                 ";

                try
                {
                    File.WriteAllText(lessFilePath, lessContent);
                    Console.WriteLine($"File '{lessFilePath}' has been created successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while creating the file: {ex.Message}");
                }
            }
        }
    }
}
       
    

