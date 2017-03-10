using System;

namespace AzureBlobExport
{
    class Program
    {
        static void Main(string[] args)
        {
            var DATE_FROM = new DateTime(2017, 01, 01, 00, 00, 00);
            var DATE_TO = new DateTime(2017, 01, 02, 00, 00, 00);
            var copyFromAzure = new CopyFromAzure();
            copyFromAzure.Copy(DATE_FROM, DATE_TO);

            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }
}
