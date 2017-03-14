using System;

namespace AzureBlobExport
{
    class Program
    {
        public static string inputFrom;
        public static string inputTo;
        static void Main(string[] args)
        {
            DateTime DATE_FROM = new DateTime();
            DateTime DATE_TO = new DateTime();
            if (args.Length == 0)
            {
                InputVariables();
                try
                {
                    DATE_FROM = DateTime.Parse(inputFrom);
                    DATE_TO = DateTime.Parse(inputTo);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        "DateTime conversion failed. Please try following formats dd.MM.YYYY or dd/MM/YYYY");
                    Main(new string[] {});
                }

            }
            else
            {
                try
                {
                    DATE_FROM = DateTime.Parse(args[0]);
                    DATE_TO = DateTime.Parse(args[1]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        "DateTime conversion failed. Please try following formats dd.MM.YYYY or dd/MM/YYYY");
                    Environment.Exit(0);
                }
            }

            var copyFromAzure = new CopyFromAzure();
            copyFromAzure.Copy(DATE_FROM, DATE_TO);

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
