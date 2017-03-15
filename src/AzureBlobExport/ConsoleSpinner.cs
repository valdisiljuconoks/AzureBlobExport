using System;

namespace AzureBlobExport
{
    public class ConsoleSpinner
    {
        int counter;

        public void Turn()
        {
            counter++;
            switch (counter % 4)
            {
                case 0:
                    Console.Write("/");
                    counter = 0;
                    break;
                case 1:
                    Console.Write("-");
                    break;
                case 2:
                    Console.Write("\\");
                    break;
                case 3:
                    Console.Write("|");
                    break;
            }

            Console.CursorLeft = 0;
        }
    }
}
