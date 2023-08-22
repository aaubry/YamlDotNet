using System.Globalization;
using BenchmarkDotNet.Running;
using YamlDotNet.Benchmark;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

var dateTimeOffset = new DateTimeOffset(new DateTime(2017, 1, 2, 3, 4, 5), new TimeSpan(-6, 0, 0));
Console.WriteLine(dateTimeOffset.ToString("MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture));
Console.WriteLine(dateTimeOffset.ToString("O", CultureInfo.InvariantCulture));
