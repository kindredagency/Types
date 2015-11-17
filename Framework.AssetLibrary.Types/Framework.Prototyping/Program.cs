using System;
using System.Globalization;
using Framework.Sample.Mapping.Samples.Repositories;

namespace Framework.Sample.Mapping
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            int numberOfObjects = 100000;

            SampleRepository sample = new SampleRepository(numberOfObjects);

            //5 is the number of lists
            //2 is the number of composite objects

            Console.WriteLine("Mapping a " + numberOfObjects * 5 * 2 + " objects");

            var startTime = DateTime.Now;
            Console.WriteLine("Start time : " + startTime.ToString(CultureInfo.InvariantCulture));

            sample.ImplicitMapping();

            var endTime = DateTime.Now;
            Console.WriteLine("End time : " + endTime.ToString(CultureInfo.InvariantCulture));

            Console.WriteLine("Time taken : " + (endTime - startTime).Seconds + " secs");

            Console.ReadLine();           
        }
    }
}