using System;

namespace PlaymentProject
{
    public static class FileSystemUtilities
    {
        public static bool ValidateArgs(int argsCount, int expectedCount) // validates input arguements count against expected arguements count
        {
            if(argsCount != expectedCount)
            {
                Console.WriteLine("<INVALID ARGS>");
                return false;
            }
            return true;
        }
        public static string RemoveLastSlash(string path) // sanitizing path as trailing slash in input is valid but will cause problem in identifying last node via splitting
        {
            return path[path.Length - 1] == '/' ? path.Substring(0, path.Length - 1) : path;
        }
    }
}