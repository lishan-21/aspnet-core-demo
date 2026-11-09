using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Hosting;

namespace PerformanceMonitor
{
    public class PerformanceMetrics
    {
        private static readonly Random _random = new Random();
        /// <summary>
        /// 处理器使用率（百分比）
        /// CPU Usage (Percentage)
        /// </summary>
        public int Processor { get; set; }
        /// <summary>
        /// 内存使用量
        /// Memory Usage
        /// </summary>
        public long Memory { get; set; }
        /// <summary>
        /// 网络使用量
        /// Network throughput
        /// </summary>
        public long Network { get; set; }
        override public string ToString()
        {
            return $"CPU: {Processor * 100}%, Memory: {Memory / (1024 * 1024)} MB, Network: {Network / (1024 * 1024)} MB";
        }

        public static PerformanceMetrics Create() => new PerformanceMetrics()
        {
            Processor = _random.Next(1, 8),
            Memory = _random.Next(10, 100) * 1024 * 1024,
            Network = _random.Next(10, 100) * 1024 * 1024
        };

    }



    public interface IProcessMetricsCollector
    {
        int GetUsage();
    }
    public interface INetworkMetricsCollector
    {
        long GetThroughput();
    }

    public interface IMemoryMetricsCollector
    {
        long GetUsage();
    }

    public interface IMetricsDelivery
    {
        Task DeliveryAsync(PerformanceMetrics metrics);
    }

    public class FakeMetricsCollector : IProcessMetricsCollector, INetworkMetricsCollector, IMemoryMetricsCollector
    {
        public FakeMetricsCollector()
        {
            Console.WriteLine("FakeMetricsCollector created.");
        }
        int IProcessMetricsCollector.GetUsage() => PerformanceMetrics.Create().Processor;

        long INetworkMetricsCollector.GetThroughput() => PerformanceMetrics.Create().Network;

        long IMemoryMetricsCollector.GetUsage() => PerformanceMetrics.Create().Memory;
    }
    public class FakeMetricsDelivery : IMetricsDelivery
    {
        public FakeMetricsDelivery()
        {
            Console.WriteLine("FakeMetricsDelivery created.");
        }
        public Task DeliveryAsync(PerformanceMetrics metrics)
        {
            Console.WriteLine($"[{DateTimeOffset.Now}] Delivered Metrics: {metrics}");
            return Task.CompletedTask;
        }

    }

    public class FakeMetricsDeliveryNew : IMetricsDelivery
    {
        public FakeMetricsDeliveryNew()
        {
            Console.WriteLine("FakeMetricsDeliveryNew created.");
        }
        public Task DeliveryAsync(PerformanceMetrics metrics)
        {
            Console.WriteLine($"[{DateTimeOffset.Now}] Delivered Metrics(New): {metrics}");
            return Task.CompletedTask;
        }

    }


    public sealed class PerformanceMetricsCollector : IHostedService
    {
        private IDisposable _scheduler;
        public Task StartAsync(CancellationToken cancellationToken)
        {
            //传入的两个TimeSpan参数分别表示首次执行回调的延迟时间和后续执行的间隔时间。首次回调的延迟时间是指从调用StartAsync方法开始到第一次执行回调的时间间隔，而后续执行的间隔时间是指每次回调之间的时间间隔。
            _scheduler = new Timer(Callback, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
            return Task.CompletedTask;
            // 使用static 关键字，以便在回调中不捕获外部状态，从而避免潜在的内存泄漏。
            // 在StartAsync方法中定义该回调方法，其他方法将无法访问它，从而确保了封装性和安全性。
            static void Callback(object state)
            {
                Console.WriteLine($"[{DateTimeOffset.Now}] Performance Metrics: {PerformanceMetrics.Create()}");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _scheduler?.Dispose();
            return Task.CompletedTask;
        }
    }

    public sealed class PerformanceMetricsCollectorWithDI : IHostedService
    {
        private readonly IProcessMetricsCollector _processMetricsCollector;
        private readonly INetworkMetricsCollector _networkMetricsCollector;
        private readonly IMemoryMetricsCollector _memoryMetricsCollector;
        private readonly IMetricsDelivery _metricsDelivery;
        private IDisposable _scheduler;
        public PerformanceMetricsCollectorWithDI(
            IProcessMetricsCollector processMetricsCollector,
            INetworkMetricsCollector networkMetricsCollector,
            IMemoryMetricsCollector memoryMetricsCollector,
            IMetricsDelivery metricsDelivery)
        {
            _processMetricsCollector = processMetricsCollector;
            _networkMetricsCollector = networkMetricsCollector;
            _memoryMetricsCollector = memoryMetricsCollector;
            _metricsDelivery = metricsDelivery;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _scheduler = new Timer(Callback, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
            return Task.CompletedTask;
            async void Callback(object state)
            {
                var metrics = new PerformanceMetrics
                {
                    Processor = _processMetricsCollector.GetUsage(),
                    Network = _networkMetricsCollector.GetThroughput(),
                    Memory = _memoryMetricsCollector.GetUsage()
                };
                await _metricsDelivery.DeliveryAsync(metrics);
            }
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _scheduler?.Dispose();
            return Task.CompletedTask;
        }
    }
}
