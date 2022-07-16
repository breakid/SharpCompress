// Sources:
//   - https://icsharpcode.github.io/SharpZipLib/help/api/ICSharpCode.SharpZipLib.Zip.ZipOutputStream.html
//   - https://stackoverflow.com/questions/1395205/better-way-to-check-if-a-path-is-a-file-or-a-directory
//   - https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/file-system/how-to-iterate-through-a-directory-tree
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace SharpCompress
{
    class Program
    {
        public static void PrintUsage()
        {
            Console.WriteLine(@"Compress all specified files and directories and write the output to the specified output filepath
        
USAGE:
    SharpCompress [/F] [/R] [/V] <input_filepath> [<input_filepath>] /O <output_filepath>
    
    /O  Used to specify path to output file; required
    /F  Forcibly overwrite output file, if it exists
    /R  Specifies that directories should be compressed recursively
    /V  Verbose; print the name of each file as it's compressed
    /?  Display this help message (-h and /h also work)

    One or more input filepaths can be provided; paths can be individual files or directories. 
    By default, if a directory is provided, all files immediately in the specified directory will be compressed.
    Use the /R flag to process directories recursively.

    Flags are case insensitive and can be specified using '/' or '-' (e.g., '/v', '-F'), depending on your preference.
            ");

            Console.WriteLine("DONE");
        }

        public static List<string> WalkDirectoryTree(System.IO.DirectoryInfo root)
        {
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;
            List<string> input_filepaths = new List<string>();

            // First, process all the files directly under this folder
            try
            {
                files = root.GetFiles("*.*");
            }
            // This is thrown if even one of the files require permissions greater
            // than the application provides.
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (System.IO.DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            if (files != null)
            {
                foreach (System.IO.FileInfo fi in files)
                {
                    // In this example, we only access the existing FileInfo object. If we
                    // want to open, delete or modify the file, then
                    // a try-catch block is required here to handle the case
                    // where the file has been deleted since the call to TraverseTree().
                    input_filepaths.Add(fi.FullName);
                }

                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories();

                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    // Resursively call for each subdirectory.
                    input_filepaths.AddRange(WalkDirectoryTree(dirInfo));
                }
            }

            return input_filepaths;
        }

        public static void Main(string[] args)
        {
            Boolean force = false;
            Boolean recursive = false;
            Boolean verbose = false;
            List<string> input_filepaths = new List<string>();
            string output_filepath = "";

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                switch (arg.ToUpper())
                {
                    case "-F":
                    case "/F":
                        force = true;
                        break;
                    case "-O":
                    case "/O":
                        i++;
                        output_filepath = args[i];
                        break;
                    case "-R":
                    case "/R":
                        recursive = true;
                        break;
                    case "-V":
                    case "/V":
                        verbose = true;
                        break;
                    case "-H":
                    case "/H":
                    case "/?":
                        PrintUsage();
                        return;
                    default:
                        input_filepaths.Add(arg);
                        break;
                }
            }

            if (output_filepath == "")
            {
                Console.WriteLine("[-] ERROR: No output filepath specified\n\nDONE");
                System.Environment.Exit(1);
            }
            else if (File.Exists(output_filepath) && !force)
            {
                Console.WriteLine("[-] ERROR: Output filepath already exists\n\nDONE");
                System.Environment.Exit(1);
            }

            byte[] buffer = new byte[4096];

            try
            {
                using (ZipOutputStream s = new ZipOutputStream(File.Create(output_filepath)))
                {
                    s.SetLevel(9); // 0 - store only to 9 - means best compression

                    foreach (string path in input_filepaths)
                    {
                        if (File.Exists(path))
                        {
                            // Compress individual file
                            if (verbose)
                            {
                                Console.WriteLine(path);
                            }

                            s.PutNextEntry(new ZipEntry(path));

                            using (FileStream fs = File.OpenRead(path))
                            {
                                StreamUtils.Copy(fs, s, buffer);
                            }
                        }
                        else if (Directory.Exists(path))
                        {
                            if (recursive)
                            {
                                // Compress all files in and below the specified folder
                                foreach (string p in WalkDirectoryTree(new DirectoryInfo(path)))
                                {
                                    if (verbose)
                                    {
                                        Console.WriteLine(p);
                                    }

                                    s.PutNextEntry(new ZipEntry(p));

                                    using (FileStream fs = File.OpenRead(p))
                                    {
                                        StreamUtils.Copy(fs, s, buffer);
                                    }
                                }
                            }
                            else
                            {
                                // Compress only files directly in the specified folder
                                foreach (string p in Directory.GetFiles(path))
                                {
                                    if (verbose)
                                    {
                                        Console.WriteLine(p);
                                    }

                                    s.PutNextEntry(new ZipEntry(p));

                                    using (FileStream fs = File.OpenRead(p))
                                    {
                                        StreamUtils.Copy(fs, s, buffer);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Output an error if the file was deleted between when it was added to the input_filepaths list and when it was read for compression
                            Console.WriteLine("[-] ERROR 2: Path Not Found (" + path + ")");
                        }
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("[-] ERROR: " + e.Message);
            }

             Console.WriteLine("\nDONE");
        }
    }
}