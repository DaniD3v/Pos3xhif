﻿using FlightMonitor.Domain;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using System;

namespace FlightMonitor
{
    public class FlightContext
    {
        public IEnumerable<Departure> Departures { get; private set; }
        public IEnumerable<Destination> Destinations => Departures.SelectMany(d => d.Destinations).Distinct(g => g.IataCode);
        public IEnumerable<City> Cities => Destinations.Select(d => d.City).Distinct(g => g.NameEn);
        public IEnumerable<Country> Countries => Cities.Select(d => d.Country).Distinct(g => g.NameDe);
        public IEnumerable<Aircraft> Aircrafts => Departures.Select(d => d.Aircraft).Distinct(g => g.Description);
        public IEnumerable<Airline> Airlines => Departures.Select(d => d.Airline).Distinct(g => g.IataCode);

        private FlightContext() { }
        public static FlightContext FromFile(string filename)
        {
            FlightContext flightContext = new FlightContext();

            using FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            string content = JsonDocument.Parse(fs)
                .RootElement
                .GetProperty("monitor")
                .GetProperty("departure")
                .GetRawText();
            IEnumerable<Departure> departures = JsonSerializer
                .Deserialize<IEnumerable<Departure>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            flightContext.Departures = departures;
            return flightContext;
        }
    }

    /// <summary>
    /// Extensions für IEnumerable
    /// </summary>
    public static class FlightMonitorExtensions
    {
        public static IEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> collection, Func<TSource, TKey> comparer) =>
            collection.GroupBy(comparer).Select(g => g.FirstOrDefault());
    }
}
