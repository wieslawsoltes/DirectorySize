using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DirectorySize
{
    class Program
    {
        static void Main(string[] args)
        {
            // The root directory to scan
            string root = args[0];

            // Get the tree of directories and files
            DirectoryTree tree = BuildDirectoryTree(root);

            // Sort the tree nodes by size
            // tree.SortTree();
            
            // Flatten the tree into a list
            List<DirectoryTree> flatList = new List<DirectoryTree>();
            FlattenTree(tree, flatList);

            // Sort the list by size
            flatList.Sort((x, y) => y.Size.CompareTo(x.Size));

            // Determine the number of rows that will fit on the screen
            int rows = Console.WindowHeight - 2;

            // Paging loop
            int page = 0;
            while (true)
            {
                // Clear the console
                Console.Clear();

                // Display the paging information
                Console.WriteLine($"Page {page + 1} of {flatList.Count / rows + 1}");

                // Determine the width of the Directory column
                int directoryWidth = 50;

                // Display the table header
                Console.WriteLine("Directory".PadRight(directoryWidth) + "Size".PadLeft(20));
                Console.WriteLine(new string('-', directoryWidth + 20));

                // Display the top N directories by size
                for (int i = page * rows; i < Math.Min((page + 1) * rows, flatList.Count); i++)
                {
                    DirectoryTree node = flatList[i];
                    string shortenedPath = ShortenPath(node.Path, directoryWidth);
                    Console.WriteLine(shortenedPath.PadRight(directoryWidth) + node.Size.ToString("N0").PadLeft(20));
                }

                // Wait for the user to press a key
                ConsoleKeyInfo key = Console.ReadKey();

                // Go to the next or previous page
                if (key.Key == ConsoleKey.RightArrow)
                {
                    page++;
                }
                else if (key.Key == ConsoleKey.LeftArrow)
                {
                    page--;
                }

                // Clamp the page index to the valid range
                page = Math.Max(0, Math.Min(page, flatList.Count / rows));
            }
        }

        static string ShortenPath(string path, int maxLength)
        {
            string fileName = Path.GetFileName(path);
            string directoryName = Path.GetDirectoryName(path);

            if (directoryName.Length + fileName.Length + 1 > maxLength)
            {
                // The path is too long, so shorten it by removing the middle part
                int remainingLength = maxLength - fileName.Length - 3;
                int startIndex = directoryName.Length - remainingLength / 2;
                return Path.Combine(directoryName.Substring(0, startIndex) + "...", fileName);
            }
            else
            {
                // The path fits within the maximum length, so return the full path
                return path;
            }
        }

        static void FlattenTree(DirectoryTree node, List<DirectoryTree> list)
        {
            list.Add(node);
            foreach (DirectoryTree child in node)
            {
                FlattenTree(child, list);
            }
        }
        
        static DirectoryTree BuildDirectoryTree(string path)
        {
            // Create the root node for the directory
            DirectoryTree root = new DirectoryTree { Path = path, Size = 0 };

            // Get the subdirectories and files in the directory
            string[] directories = Directory.GetDirectories(path);
            string[] files = Directory.GetFiles(path);

            // Add the size of the files to the root node
            foreach (string file in files)
            {
                try
                {
                    FileInfo info = new FileInfo(file);
                    root.Size += info.Length;
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine($"Access denied: {file}");
                }
                catch (IOException)
                {
                    Console.WriteLine($"IO error: {file}");
                }
            }

            // Recursively add the subdirectories and their sizes
            foreach (string directory in directories)
            {
                try
                {
                    DirectoryTree child = BuildDirectoryTree(directory);
                    root.Size += child.Size;
                    root.Add(child);
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine($"Access denied: {directory}");
                }
                catch (IOException)
                {
                    Console.WriteLine($"IO error: {directory}");
                }
            }

            return root;
        }

    }

    class DirectoryTree : List<DirectoryTree>
    {
        public string Path { get; set; }
        public long Size { get; set; }

        public void SortTree()
        {
            // Sort the children of this node
            Sort((x, y) => y.Size.CompareTo(x.Size));

            // Recursively sort the children of the children
            foreach (DirectoryTree child in this)
            {
                child.SortTree();
            }
        }
    }
}
