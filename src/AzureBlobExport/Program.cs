using System;
using System.Diagnostics;
using System.Globalization;

namespace AzureBlobExport
{
    class Program
    {
        public static string inputFrom;
        public static string inputTo;

        static void Main(params string[] args)
        {
            var from = new DateTime();
            var to = new DateTime();

            if(args.Length == 0)
            {
                InputVariables();

                if(!DateTime.TryParse(inputFrom, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out from))
                {
                    Console.WriteLine("DateTime conversion failed. Please try following formats dd.MM.YYYY or dd/MM/YYYY");
                    Main();
                }

                if(!DateTime.TryParse(inputTo, out to))
                {
                    Console.WriteLine("DateTime conversion failed. Please try following formats dd.MM.YYYY or dd/MM/YYYY");
                }
            }
            else
            {
                if(!DateTime.TryParse(args[0], out from) || !DateTime.TryParse(args[1], out to))
                {
                    Console.WriteLine("DateTime conversion failed. Please try following formats dd.MM.YYYY or dd/MM/YYYY");
                    Environment.Exit(0);
                }
            }

            var copyFromAzure = new CopyFromAzure();
            copyFromAzure.Copy(from, to).GetAwaiter().GetResult();

            Console.WriteLine("Done.");
            Console.ReadLine();
        }

        private static void InputVariables()
        {
            Console.Write("Please enter period start datetime in following format dd.MM.YYYY: ");
            inputFrom = Console.ReadLine();

            Console.Write("Please enter period end datetime in following format dd.MM.YYYY: ");
            inputTo = Console.ReadLine();
        }
    }
}
