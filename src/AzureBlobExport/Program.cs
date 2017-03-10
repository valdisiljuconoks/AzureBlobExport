using System;

namespace AzureBlobExport
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime DATE_FROM = new DateTime(2017,01,01,00,00,00);
            DateTime DATE_TO = new DateTime(2017,01,08,00,00,00);
            var copyFromAzure = new CopyFromAzure();
            copyFromAzure.Copy(DATE_FROM,DATE_TO);
        }

    }
}
