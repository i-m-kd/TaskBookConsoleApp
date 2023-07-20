using System;

namespace TaskBookConsoleApp
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            DatabaseConfig DBConfig = new DatabaseConfig();

            DBConfig.TaskbookTable();

            ListMenu();
            while (true)
            {
                Console.WriteLine("1 : Add Task");
                Console.WriteLine("2 : View Task");
                Console.WriteLine("3 : Modify Task");
                Console.WriteLine("4 : Delete Task");
                Console.WriteLine("5 : Exit");

                Console.Write("Enter Your Choice : ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        DBConfig.UserInput();
                        break;
                    case "2":
                        DBConfig.ViewData();
                        break;
                    case "3":
                        DBConfig.ModifyData();
                        break;
                    case "4":
                        DBConfig.DeleteData();
                        break;
                    case "5":
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Invallid Input");
                        break;
                }
            }

        }
        static void ListMenu()
        {
            string heading = "The Task Book";
            char[] headingChar = heading.ToCharArray();
            Console.WriteLine(heading);
            for (int i = 0; i < headingChar.Length; i++)
            {
                if (headingChar[i] == ' ')
                {
                    Console.Write(" ");
                }
                else
                {
                    Console.Write("=");
                }
            }
            Console.WriteLine();
        }
    }
}
