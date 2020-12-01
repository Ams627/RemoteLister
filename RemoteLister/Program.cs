using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace RemoteLister
{
    class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var dir = GetDirectories(Directory.GetCurrentDirectory(), "*");
                foreach (var d in dir)
                {
                    var name = new DirectoryInfo(d).Name;
                    if (name == ".git")
                    {
                        PrintRemote(d);
                    }
                }
            }
            catch (Exception ex)
            {
                var fullname = System.Reflection.Assembly.GetEntryAssembly().Location;
                var progname = Path.GetFileNameWithoutExtension(fullname);
                Console.Error.WriteLine($"{progname} Error: {ex.Message}");
            }

        }

        private static void PrintRemote(string d)
        {
            var configPath = Path.Combine(d, "config");
            var pattern = @"^\s*\[(.+)\]\s*$";
            var remotePattern = @"^\s*url\s*=\s*([^\s]+)\s*$";
            bool inRemote = false;
            foreach (var line in File.ReadLines(configPath))
            {
                if (inRemote)
                {
                    var rMatch = Regex.Match(line, remotePattern);
                    if (rMatch.Success)
                    {
                        if (rMatch.Groups.Count > 1)
                        {
                            var parent = new DirectoryInfo(d).Parent.FullName;
                            Console.WriteLine($"{parent} | {rMatch.Groups[1].Value}");
                        }
                    }
                }
                else
                {
                    var match = Regex.Match(line, pattern);
                    if (match.Success)
                    {
                        if (match.Groups.Count > 1 && match.Groups[1].Value == "remote \"origin\"")
                        {
                            inRemote = true;
                        }
                        else
                        {
                            inRemote = false;
                        }
                    }
                }
            }
        }

        private static List<string> GetDirectories(string startdir, string pattern)
        {
            var result = new List<string>();

            var fstack = new Stack<string>();
            fstack.Push(startdir);

            while (fstack.Any())
            {
                var dir = fstack.Pop();
                foreach (var subdir in Directory.GetDirectories(dir, pattern))
                {
                    var lastComponent = new DirectoryInfo(subdir).Name;
                    if (lastComponent.Equals("bin", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    if (lastComponent.Equals("obj", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    result.Add(subdir);
                    fstack.Push(subdir);
                }
            }

            return result;
        }


        private static List<string> GetFiles(string startdir, string pattern)
        {
            var result = new List<string>();

            var fstack = new Stack<string>();
            fstack.Push(startdir);

            while (fstack.Any())
            {
                var dir = fstack.Pop();
                var files = Directory.GetFiles(startdir);
                foreach (var file in files)
                {
                    result.Add(file);
                }
                foreach (var subdir in Directory.GetDirectories(dir))
                {
                    var lastComponent = new DirectoryInfo(subdir).Name;
                    if (lastComponent.Equals("bin", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    if (lastComponent.Equals("obj", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    fstack.Push(subdir);
                }
            }

            return result;
        }
    }
}
