using System;
using System.Configuration;
using System.Globalization;

namespace AzureBlobExport
{
    internal class Program
    {
        public static string inputFrom;
        public static string inputTo;

        private static void Main(params string[] args)
        {
            DateTime? from;
            DateTime? to;

            var copyFromAzure = new CopyFromAzure(ConfigurationManager.AppSettings["AccountName"],
                                                  ConfigurationManager.AppSettings["AccountAccessKey"],
                                                  ConfigurationManager.AppSettings["SASToken"],
                                                  ConfigurationManager.AppSettings["TableName"],
                                                  ConfigurationManager.AppSettings["BlobContainernName"],
                                                  ConfigurationManager.AppSettings["SaveDirectory"],
                                                  ConfigurationManager.AppSettings["TablePartionKey"]);

            if(args.Length < 1)
            {
                InputVariables();

                from = ParseDateAndTime(inputFrom);
                if(!from.HasValue)
                {
                    Console.WriteLine("DateTime conversion failed. Please try format dd.MM.yyyy");
                    Main();
                }

                to = ParseDateAndTime(inputTo);
                if(!to.HasValue)
                {
                    Console.WriteLine("DateTime conversion failed. Please try format dd.MM.yyyy");
                    Main();
                }
            }
            else
            {
                from = ParseDateAndTime(inputFrom);
                to = ParseDateAndTime(inputTo);

                if(!from.HasValue || !to.HasValue)
                {
                    Console.WriteLine("DateTime conversion failed. Please try format dd.MM.yyyy");
                    Environment.Exit(0);
                }
            }

            copyFromAzure.Copy(from.Value, to.Value).GetAwaiter().GetResult();

            Console.WriteLine("Done.");
            Console.ReadLine();
        }

        private static void InputVariables()
        {
            Console.Write("Please enter period start datetime in dd.MM.yyyy format: ");
            inputFrom = Console.ReadLine();

            Console.Write("Please enter period end datetime in dd.MM.yyyy format: ");
            inputTo = Console.ReadLine();
        }

        private static DateTime? ParseDateAndTime(string value)
        {
            DateTime? date = null;
            if(DateTime.TryParseExact(value, new[] { "dd.MM.yyyy", "dd.MM.yyyy HH:mm:ss" }, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out DateTime parsedValue))
                date = parsedValue;

            return date;
        }
    }
}
