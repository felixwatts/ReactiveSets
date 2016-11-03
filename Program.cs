using System;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var subject = new ReactiveSets.FastSubject<int>();

            var x = Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(y => Console.WriteLine(y));

            Console.ReadLine();
        }
    }
}
