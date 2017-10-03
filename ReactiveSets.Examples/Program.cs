﻿using System;
using System.Linq;
using System.Reactive.Linq;

namespace ReactiveSets.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            Example1();
        }

        private static void Example1()
        {
            var source = new Set<string>();                             // a set of stock names that can change over time

            source
                .Select(n => new Stock(n))                              // map from stock names to stock objects
                .WhereDynamic(s => s.Price, p => p > 100.1)             // filter for stocks who's price has moved a lot
                .Select(s => $"{s.Name}:{s.Price.Current}")             // map each stock to just its name
                .Aggregate(ns => string.Join(",", ns.OrderBy(n => n)))  // reduce the collection to a single string of stock names
                .Subscribe(str =>                                       // print the result each time it changes
                {
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine(str);
                });

            source.SetItem("GOOG");                                     // any manipulations to the source set will be taken
            source.SetItem("AAPL");                                     // into account            

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
            Price = Observable
                .Interval(TimeSpan.FromSeconds(1))  
                .Scan(100d, (current, _) => current + _random.NextDouble() - 0.5d)              
                .ToValue();
        }

        private static Random _random = new Random();
    }
}