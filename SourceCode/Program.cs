using System;

namespace PlaymentProject
{
    class Program
    {
        static void Main(string [] args)
        {
            Console.WriteLine("<Starting your application...>");
            FileSystemCommands fs = new FileSystemCommands();
            while(true)
            {
                string input = Console.ReadLine();
                args = input.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                fs.Execute(args);
            }
        }
    }
}
