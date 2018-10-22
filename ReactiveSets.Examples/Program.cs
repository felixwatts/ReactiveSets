using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace ReactiveSets.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            //Example1();
            //Example2();
            //Example3();
            Example4();
        }

        private static void Example1()
        {
            // declare a set of integers
            var source = new Set<int>(); 

            source
                // reduce the set to the sum of its contents
                .Aggregate(ints => ints.Sum())
                // print the sum each time it changes
                .Subscribe(Console.WriteLine);

            // modifications to the set update
            // and print the sum accordingly        
            source.SetItem(2);
            source.SetItem(3);
            source.SetItem(37);
            source.DeleteItem(3);

            // prints:
            // 0
            // 2
            // 5
            // 42
            // 39
        }

        private static void Example2()
        {
            // declare a set of ints keyed by string
            var source = new Set<string, int>();

            source
                // filter out even numbers
                .Where(i => i % 2 == 1)
                // reduce to a comma separated string
                .Aggregate(ints => string.Join(",", ints))
                // print the string each time it changes
                .Subscribe(Console.WriteLine);

            source.SetItem("A", 1);
            source.SetItem("B", 2);
            source.SetItem("C", 3);
            // keys are unique but values need not be
            source.SetItem("C", 1);
            source.DeleteItem("C");
        }

        private static void Example3()
        {
            // a set mapping stock ticker to current price
            var stockPrices = new Set<string, double>();
            // a set mapping stock ticker to number of stocks owned
            var positions = new Set<string, int>();

            positions                
                // join the two sets by stock ticker (the Id), multiplying the values
                // to give us a set of stock ticker to current value of the owned stocks
                .Join(stockPrices, (position, price) => position * price)
                // sum value over all stocks owned
                .Aggregate(vs => vs.Sum())
                // print the total value to the console each time it changes
                .Subscribe(Console.WriteLine);

            // changes to stock prices and stocks owned
            // automatically update the total value
            stockPrices.SetItem("GOOG", 500);
            positions.SetItem("GOOG", 10);
            stockPrices.SetItem("AAPL", 1000);
            positions.SetItem("AAPL", 10);
            stockPrices.SetItem("AAPL", 1001);
            positions.DeleteItem("AAPL");

            // prints:
            // 0
            // 0
            // 5000
            // 15000
            // 15010
            // 5000
        }

        private static void Example4()
        {
            // a set of stock names that can change over time
            var source = new Set<string>();                                 

            source
                // map from stock names to stock objects, the stock object has observable properties that change over time
                .Select(n => new Stock(n))
                // filter for stocks whose price is over 100, note that we are filtering on a property that changes over time
                .WhereDynamic(s => s.Price, p => p > 100)
                // map each stock to a string summarizing its name and price, note that we are mapping a property that changes over time            
                .SelectDynamic(s => s.Price.Select(p => $"{s.Name}:{p:f2}"))
                // reduce the collection to a single string summarizing the filtered stocks and their prices
                .Aggregate(ns => string.Join(Environment.NewLine, ns.OrderBy(n => n)))
                // print the result each time it changes   
                .Subscribe(str =>                                           
                {
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine(str);
                });

            // any manipulations to the source set will be taken into account 
            source.SetItem("GOOG");                                 
            source.SetItem("AAPL");          

            Console.Read();
        }
    }

    public class Stock
    {
        public string Name { get; }
        public IValue<double> Price { get; }

        public Stock(string name)
        {
            Name = name;

            // stock price does a random walk starting at 100
            Price = _ticker
                .Scan(100d, (current, _) => current + SmallDelta())                  
                .ToValue();
        }

        private double SmallDelta()
        {
            return (_random.NextDouble() - 0.5d) * 0.1d;
        }

        private static Random _random = new Random();
        private static IObservable<long> _ticker = Observable
            .Interval(TimeSpan.FromSeconds(0.2))
            // Since the ReactiveSets is not thread safe, its important to avoid concurrent updates
            .ObserveOn(new SequentialScheduler());
    }
}
