using System;

namespace AzureBlobExport
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Please enter period start datetime in following format dd.MM.YYYY: ");
            var startDate = Console.ReadLine();
            Console.Write("Please enter period end datetime in following format dd.MM.YYYY: ");
            var endDate = Console.ReadLine();

            DateTime DATE_FROM = new DateTime();
            DateTime DATE_TO = new DateTime();

            try
            {
                DATE_FROM = DateTime.Parse(startDate);
                DATE_TO = DateTime.Parse(endDate);
            }
            catch (Exception ex)
            {
                Console.WriteLine("DateTime conversion failed. Please try following formats dd.MM.YYYY or dd/MM/YYYY");
            }

            var copyFromAzure = new CopyFromAzure();
            copyFromAzure.Copy(DATE_FROM, DATE_TO);

            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }
}
