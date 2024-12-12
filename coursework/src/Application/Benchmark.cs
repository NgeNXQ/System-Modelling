using System;
using System.IO;
using Coursework.Framework.Core.Controllers;
using Coursework.Framework.Components.Modules.Common;
using Coursework.Framework.Components.Modules.Concrete;
using Coursework.Framework.Components.Queues.Concrete;
using Coursework.Framework.Components.Workers.Concrete;
using Coursework.Framework.Components.Blueprints.Schemes.Concrete;
using Coursework.Framework.Components.Blueprints.Transitions.Concrete;
using Coursework.Framework.Components.Tasks.Utilities.Factories.Concrete;

namespace Coursework.Application;

internal sealed class Benchmark
{
    internal void GenerateStabilizationSamples(float timeLowerBound, float timeUpperBound, float step, int iterationsCount, string outputPath)
    {
        const float MAX_PACKET_LIFETIME = 10.0f;
        const float PIPELINE_BALANCE_THRESHOLD = 0.7f;

        using (StreamWriter writer = new StreamWriter(outputPath, false))
            writer.WriteLine("Time,Loss,Connection,Lifetime");

        for (float timeModelling = timeLowerBound; timeModelling <= timeUpperBound; timeModelling += step)
        {
            float lossFrequencyTotal = 0.0f;
            float packetsLifetimeTotal = 0.0f;
            float connectionFrequencyTotal = 0.0f;

            Console.Clear();
            Console.WriteLine($"\r{timeModelling / timeUpperBound * 100}%");

            for (int i = 0; i < iterationsCount; ++i)
            {
                DisposeModule dispose = new DisposeModule("DISPOSE");

                SimpleScheme processorD5Scheme = new SimpleScheme("D5_SCHEME", new DisposeTransition("D5 => DISPOSE", dispose));
                ProcessorModule processorD5 = new ProcessorModule("PROCESSOR_D5", processorD5Scheme, new StubConstantWorker(0), SimpleQueueFIFO.Infinite);

                SimpleScheme processorK2Scheme = new SimpleScheme("K2_SCHEME", new ProcessorBlockingTransition("K2 => D5", processorD5, (task) => task.Lifetime > MAX_PACKET_LIFETIME));
                ProcessorModule processorK2 = new ProcessorModule("PROCESSOR_K2", processorK2Scheme, new StubConstantWorker(5), SimpleQueueFIFO.Infinite);

                SimpleScheme processorK1Scheme = new SimpleScheme("K1_SCHEME", new ProcessorSimpleTransition("K1 => K2", processorK2));
                ProcessorModule processorK1 = new ProcessorModule("PROCESSOR_K1", processorK1Scheme, new StubConstantWorker(5), SimpleQueueFIFO.Infinite);

                SimpleScheme processorK4Scheme = new SimpleScheme("K4_SCHEME", new ProcessorBlockingTransition("K4 => D5", processorD5, (task) => task.Lifetime > MAX_PACKET_LIFETIME));
                ProcessorModule processorK4 = new ProcessorModule("PROCESSOR_K4", processorK4Scheme, new StubConstantWorker(4), SimpleQueueFIFO.Infinite);

                SimpleScheme processorK3Scheme = new SimpleScheme("K3_SCHEME", new ProcessorSimpleTransition("K3 => K4", processorK4));
                ProcessorModule processorK3 = new ProcessorModule("PROCESSOR_K3", processorK3Scheme, new StubConstantWorker(4), SimpleQueueFIFO.Infinite);

                SimpleScheme createScheme = new SimpleScheme("CRETE_SCHEME");
                CreateModule create = new CreateModule("CREATE", createScheme, new MockNormalWorker(6.0f, 3.0f), new PacketDummyTaskFactory());
                var defaultPipeline = new ProcessorBlockingTransition("CREATE => K1", processorK1, (_) => ((float)processorD5.ProcessedTasksCount / create.CreatedTasksCount) < PIPELINE_BALANCE_THRESHOLD);
                var reservePipeline = new ProcessorBlockingTransition("CREATE => K1", processorK3, (_) => ((float)processorD5.ProcessedTasksCount / create.CreatedTasksCount) >= PIPELINE_BALANCE_THRESHOLD);
                createScheme.Attach(defaultPipeline);
                createScheme.Attach(reservePipeline);

                SystemModelController.Instance.Build(new Module[] { create, processorK1, processorK2, processorK3, processorK4, processorD5, dispose });
                SystemModelController.Instance.RunSimulation(timeModelling);

                int totalFailuresCount = processorK2Scheme.FailedTasksCount + processorK4Scheme.FailedTasksCount;
                int totalProcessedCount = processorK2Scheme.ProcessedTasksCount + processorK4Scheme.ProcessedTasksCount;

                float lossFrequency = (float)totalFailuresCount / totalProcessedCount;
                float connectionFrequency = reservePipeline.ActivationsCount / reservePipeline.TimeUsageTotal;

                lossFrequencyTotal += lossFrequency;
                connectionFrequencyTotal += connectionFrequency;
                packetsLifetimeTotal += (processorK2Scheme.TasksLifetimeSum + processorK4Scheme.TasksLifetimeSum) / (processorK2Scheme.ProcessedTasksCount + processorK4Scheme.ProcessedTasksCount);
            }

            float lossFrequencyMean = lossFrequencyTotal / iterationsCount;
            float packetsLifetimeMean = packetsLifetimeTotal / iterationsCount;
            float connectionFrequencyMean = connectionFrequencyTotal / iterationsCount;

            using (StreamWriter writer = new StreamWriter(outputPath, true))
                writer.WriteLine($"{timeModelling},{MathF.Round(lossFrequencyMean, 5)},{MathF.Round(connectionFrequencyMean, 5)},{MathF.Round(packetsLifetimeMean, 5)}");
        }

        Console.Clear();
    }

    internal void GenerateGeneralSamples(int iterationsCount, string outputPath)
    {
        const float SIMULATION_TIME = 1000.0f;
        const float DISTRIBUTION_MEAN = 6.0f;
        const float DISTRIBUTION_DEVIATION = 3.0f;
        const float MAX_PACKET_LIFETIME = 10.0f;
        const float CHANNEL_DEFAULT_PAYLOAD = 5.0f;
        const float CHANNEL_RESERVE_PAYLOAD = 4.0f;
        const float PIPELINE_BALANCE_THRESHOLD = 0.7f;

        using (StreamWriter writer = new StreamWriter(outputPath, false))
            writer.WriteLine("Loss,Connection");

        for (int i = 0; i < iterationsCount; ++i)
        {
            DisposeModule dispose = new DisposeModule("DISPOSE");

            SimpleScheme processorD5Scheme = new SimpleScheme("D5_SCHEME", new DisposeTransition("D5 => DISPOSE", dispose));
            ProcessorModule processorD5 = new ProcessorModule("PROCESSOR_D5", processorD5Scheme, new StubConstantWorker(0), SimpleQueueFIFO.Infinite);

            SimpleScheme processorK2Scheme = new SimpleScheme("K2_SCHEME", new ProcessorBlockingTransition("K2 => D5", processorD5, (task) => task.Lifetime > MAX_PACKET_LIFETIME));
            ProcessorModule processorK2 = new ProcessorModule("PROCESSOR_K2", processorK2Scheme, new StubConstantWorker(CHANNEL_DEFAULT_PAYLOAD), SimpleQueueFIFO.Infinite);

            SimpleScheme processorK1Scheme = new SimpleScheme("K1_SCHEME", new ProcessorSimpleTransition("K1 => K2", processorK2));
            ProcessorModule processorK1 = new ProcessorModule("PROCESSOR_K1", processorK1Scheme, new StubConstantWorker(CHANNEL_DEFAULT_PAYLOAD), SimpleQueueFIFO.Infinite);

            SimpleScheme processorK4Scheme = new SimpleScheme("K4_SCHEME", new ProcessorBlockingTransition("K4 => D5", processorD5, (task) => task.Lifetime > MAX_PACKET_LIFETIME));
            ProcessorModule processorK4 = new ProcessorModule("PROCESSOR_K4", processorK4Scheme, new StubConstantWorker(CHANNEL_RESERVE_PAYLOAD), SimpleQueueFIFO.Infinite);

            SimpleScheme processorK3Scheme = new SimpleScheme("K3_SCHEME", new ProcessorSimpleTransition("K3 => K4", processorK4));
            ProcessorModule processorK3 = new ProcessorModule("PROCESSOR_K3", processorK3Scheme, new StubConstantWorker(CHANNEL_RESERVE_PAYLOAD), SimpleQueueFIFO.Infinite);

            SimpleScheme createScheme = new SimpleScheme("CRETE_SCHEME");
            CreateModule create = new CreateModule("CREATE", createScheme, new MockNormalWorker(DISTRIBUTION_MEAN, DISTRIBUTION_DEVIATION), new PacketDummyTaskFactory());
            var defaultPipeline = new ProcessorBlockingTransition("CREATE => K1", processorK1, (_) => ((float)processorD5.ProcessedTasksCount / create.CreatedTasksCount) < PIPELINE_BALANCE_THRESHOLD);
            var reservePipeline = new ProcessorBlockingTransition("CREATE => K1", processorK3, (_) => ((float)processorD5.ProcessedTasksCount / create.CreatedTasksCount) >= PIPELINE_BALANCE_THRESHOLD);
            createScheme.Attach(defaultPipeline);
            createScheme.Attach(reservePipeline);

            SystemModelController.Instance.Build(new Module[] { create, processorK1, processorK2, processorK3, processorK4, processorD5, dispose });
            SystemModelController.Instance.RunSimulation(SIMULATION_TIME);

            int failedTasksCountTotal = processorK2Scheme.FailedTasksCount + processorK4Scheme.FailedTasksCount;
            int processedTasksCountTotal = processorK2Scheme.ProcessedTasksCount + processorK4Scheme.ProcessedTasksCount;

            float packetsLossFrequency = MathF.Round((float)failedTasksCountTotal / processedTasksCountTotal, 5);
            float connectionFrequency = MathF.Round(reservePipeline.ActivationsCount / reservePipeline.TimeUsageTotal, 5);

            using (StreamWriter writer = new StreamWriter(outputPath, true))
                writer.WriteLine($"{packetsLossFrequency},{connectionFrequency}");
        }
    }

    internal void GenerateVerboseRegressionSamples(int iterationsCount, string outputPath)
    {
        int MIN_PACKET_LIFETIME = 10;
        int MAX_PACKET_LIFETIME = 20;

        float MIN_DEFAULT_PIPELINE_DELAY = 5;
        float MAX_DEFAULT_PIPELINE_DELAY = 10;

        float MIN_RESERVE_PIPELINE_DELAY = 1;
        float MAX_RESERVE_PIPELINE_DELAY = 4;

        using (StreamWriter writer = new StreamWriter(outputPath, false))
            writer.WriteLine("Default,Reserve,Lifetime,Succeeded,Failed,Loss");

        this.GenerateVerboseSamples(MAX_DEFAULT_PIPELINE_DELAY, MAX_RESERVE_PIPELINE_DELAY, MAX_PACKET_LIFETIME, iterationsCount, outputPath);
        this.GenerateVerboseSamples(MIN_DEFAULT_PIPELINE_DELAY, MAX_RESERVE_PIPELINE_DELAY, MAX_PACKET_LIFETIME, iterationsCount, outputPath);
        this.GenerateVerboseSamples(MAX_DEFAULT_PIPELINE_DELAY, MIN_RESERVE_PIPELINE_DELAY, MAX_PACKET_LIFETIME, iterationsCount, outputPath);
        this.GenerateVerboseSamples(MIN_DEFAULT_PIPELINE_DELAY, MIN_RESERVE_PIPELINE_DELAY, MAX_PACKET_LIFETIME, iterationsCount, outputPath);
        this.GenerateVerboseSamples(MAX_DEFAULT_PIPELINE_DELAY, MAX_RESERVE_PIPELINE_DELAY, MIN_PACKET_LIFETIME, iterationsCount, outputPath);
        this.GenerateVerboseSamples(MIN_DEFAULT_PIPELINE_DELAY, MAX_RESERVE_PIPELINE_DELAY, MIN_PACKET_LIFETIME, iterationsCount, outputPath);
        this.GenerateVerboseSamples(MAX_DEFAULT_PIPELINE_DELAY, MIN_RESERVE_PIPELINE_DELAY, MIN_PACKET_LIFETIME, iterationsCount, outputPath);
        this.GenerateVerboseSamples(MIN_DEFAULT_PIPELINE_DELAY, MIN_RESERVE_PIPELINE_DELAY, MIN_PACKET_LIFETIME, iterationsCount, outputPath);
    }

    private void GenerateVerboseSamples(float defaultPipelineDelay, float reservePipelineDelay, int packetLifetime, int iterationsCount, string outputPath)
    {
        const float SIMULATION_TIME = 100_000.0f;

        const float DISTRIBUTION_MEAN = 6.0f;
        const float DISTRIBUTION_DEVIATION = 3.0f;
        const float PIPELINE_BALANCE_THRESHOLD = 0.7f;

        int failedTasksCountTotal = 0;
        int succeededTasksCountTotal = 0;
        int processedTasksCountTotal = 0;

        for (int iteration = 0; iteration < iterationsCount; ++iteration)
        {
            DisposeModule dispose = new DisposeModule("DISPOSE");

            SimpleScheme processorD5Scheme = new SimpleScheme("D5_SCHEME", new DisposeTransition("D5 => DISPOSE", dispose));
            ProcessorModule processorD5 = new ProcessorModule("PROCESSOR_D5", processorD5Scheme, new StubConstantWorker(0), SimpleQueueFIFO.Infinite);

            SimpleScheme processorK2Scheme = new SimpleScheme("K2_SCHEME", new ProcessorBlockingTransition("K2 => D5", processorD5, (task) => task.Lifetime > packetLifetime));
            ProcessorModule processorK2 = new ProcessorModule("PROCESSOR_K2", processorK2Scheme, new StubConstantWorker(defaultPipelineDelay), SimpleQueueFIFO.Infinite);

            SimpleScheme processorK1Scheme = new SimpleScheme("K1_SCHEME", new ProcessorSimpleTransition("K1 => K2", processorK2));
            ProcessorModule processorK1 = new ProcessorModule("PROCESSOR_K1", processorK1Scheme, new StubConstantWorker(defaultPipelineDelay), SimpleQueueFIFO.Infinite);

            SimpleScheme processorK4Scheme = new SimpleScheme("K4_SCHEME", new ProcessorBlockingTransition("K4 => D5", processorD5, (task) => task.Lifetime > packetLifetime));
            ProcessorModule processorK4 = new ProcessorModule("PROCESSOR_K4", processorK4Scheme, new StubConstantWorker(reservePipelineDelay), SimpleQueueFIFO.Infinite);

            SimpleScheme processorK3Scheme = new SimpleScheme("K3_SCHEME", new ProcessorSimpleTransition("K3 => K4", processorK4));
            ProcessorModule processorK3 = new ProcessorModule("PROCESSOR_K3", processorK3Scheme, new StubConstantWorker(reservePipelineDelay), SimpleQueueFIFO.Infinite);

            SimpleScheme createScheme = new SimpleScheme("CRETE_SCHEME");
            CreateModule create = new CreateModule("CREATE", createScheme, new MockNormalWorker(DISTRIBUTION_MEAN, DISTRIBUTION_DEVIATION), new PacketDummyTaskFactory());
            var defaultPipeline = new ProcessorBlockingTransition("CREATE => K1", processorK1, (_) => ((float)processorD5.ProcessedTasksCount / create.CreatedTasksCount) < PIPELINE_BALANCE_THRESHOLD);
            var reservePipeline = new ProcessorBlockingTransition("CREATE => K1", processorK3, (_) => ((float)processorD5.ProcessedTasksCount / create.CreatedTasksCount) >= PIPELINE_BALANCE_THRESHOLD);
            createScheme.Attach(defaultPipeline);
            createScheme.Attach(reservePipeline);

            SystemModelController.Instance.Build(new Module[] { create, processorK1, processorK2, processorK3, processorK4, processorD5, dispose });
            SystemModelController.Instance.RunSimulation(SIMULATION_TIME);

            succeededTasksCountTotal += processorD5.SucceededTasksCount;
            failedTasksCountTotal += processorK2Scheme.FailedTasksCount + processorK4Scheme.FailedTasksCount;
            processedTasksCountTotal += processorK2Scheme.ProcessedTasksCount + processorK4Scheme.ProcessedTasksCount;
        }

        int failedTasksCountMean = failedTasksCountTotal / iterationsCount;
        int succeededTasksCountMean = succeededTasksCountTotal / iterationsCount;
        float packetsLossFrequency = MathF.Round((float)failedTasksCountTotal / processedTasksCountTotal, 5);

        using (StreamWriter writer = new StreamWriter(outputPath, true))
            writer.WriteLine($"{defaultPipelineDelay},{reservePipelineDelay},{packetLifetime},{succeededTasksCountMean},{failedTasksCountMean},{packetsLossFrequency}");
    }

    internal void GenerateCochranRegressionSamples(int iterationsCount, string outputPath)
    {
        int MIN_PACKET_LIFETIME = 10;
        int MAX_PACKET_LIFETIME = 20;

        float MIN_DEFAULT_PIPELINE_DELAY = 5;
        float MAX_DEFAULT_PIPELINE_DELAY = 10;

        float MIN_RESERVE_PIPELINE_DELAY = 1;
        float MAX_RESERVE_PIPELINE_DELAY = 4;

        for (int i = 0; i < iterationsCount; ++i)
        {
            string path = $"{outputPath.Substring(0, outputPath.Length - 4)}_{i}{outputPath[^4..]}";

            using (StreamWriter writer = new StreamWriter(path, false))
                writer.WriteLine("Default,Reserve,Lifetime,Loss");

            this.GenerateCochranSamples(MAX_DEFAULT_PIPELINE_DELAY, MAX_RESERVE_PIPELINE_DELAY, MAX_PACKET_LIFETIME, path);
            this.GenerateCochranSamples(MIN_DEFAULT_PIPELINE_DELAY, MAX_RESERVE_PIPELINE_DELAY, MAX_PACKET_LIFETIME, path);
            this.GenerateCochranSamples(MAX_DEFAULT_PIPELINE_DELAY, MIN_RESERVE_PIPELINE_DELAY, MAX_PACKET_LIFETIME, path);
            this.GenerateCochranSamples(MIN_DEFAULT_PIPELINE_DELAY, MIN_RESERVE_PIPELINE_DELAY, MAX_PACKET_LIFETIME, path);
            this.GenerateCochranSamples(MAX_DEFAULT_PIPELINE_DELAY, MAX_RESERVE_PIPELINE_DELAY, MIN_PACKET_LIFETIME, path);
            this.GenerateCochranSamples(MIN_DEFAULT_PIPELINE_DELAY, MAX_RESERVE_PIPELINE_DELAY, MIN_PACKET_LIFETIME, path);
            this.GenerateCochranSamples(MAX_DEFAULT_PIPELINE_DELAY, MIN_RESERVE_PIPELINE_DELAY, MIN_PACKET_LIFETIME, path);
            this.GenerateCochranSamples(MIN_DEFAULT_PIPELINE_DELAY, MIN_RESERVE_PIPELINE_DELAY, MIN_PACKET_LIFETIME, path);
        }
    }

    private void GenerateCochranSamples(float defaultPipelineDelay, float reservePipelineDelay, int packetLifetime, string outputPath)
    {
        const float SIMULATION_TIME = 200_000.0f;
        const float STATISTICS_AGGREGATION_TIMESTAMP = 100_000.0f;

        const float DISTRIBUTION_MEAN = 6.0f;
        const float DISTRIBUTION_DEVIATION = 3.0f;
        const float PIPELINE_BALANCE_THRESHOLD = 0.7f;

        int failedTasksCountTotal = 0;
        int processedTasksCountTotal = 0;
        bool hasStatisticsDropOccurred = false;

        DisposeModule dispose = new DisposeModule("DISPOSE");

        SimpleScheme processorD5Scheme = new SimpleScheme("D5_SCHEME", new DisposeTransition("D5 => DISPOSE", dispose));
        ProcessorModule processorD5 = new ProcessorModule("PROCESSOR_D5", processorD5Scheme, new StubConstantWorker(0), SimpleQueueFIFO.Infinite);

        SimpleScheme processorK2Scheme = new SimpleScheme("K2_SCHEME", new ProcessorBlockingTransition("K2 => D5", processorD5, (task) => task.Lifetime > packetLifetime));
        ProcessorModule processorK2 = new ProcessorModule("PROCESSOR_K2", processorK2Scheme, new StubConstantWorker(defaultPipelineDelay), SimpleQueueFIFO.Infinite);

        SimpleScheme processorK1Scheme = new SimpleScheme("K1_SCHEME", new ProcessorSimpleTransition("K1 => K2", processorK2));
        ProcessorModule processorK1 = new ProcessorModule("PROCESSOR_K1", processorK1Scheme, new StubConstantWorker(defaultPipelineDelay), SimpleQueueFIFO.Infinite);

        SimpleScheme processorK4Scheme = new SimpleScheme("K4_SCHEME", new ProcessorBlockingTransition("K4 => D5", processorD5, (task) => task.Lifetime > packetLifetime));
        ProcessorModule processorK4 = new ProcessorModule("PROCESSOR_K4", processorK4Scheme, new StubConstantWorker(reservePipelineDelay), SimpleQueueFIFO.Infinite);

        SimpleScheme processorK3Scheme = new SimpleScheme("K3_SCHEME", new ProcessorSimpleTransition("K3 => K4", processorK4));
        ProcessorModule processorK3 = new ProcessorModule("PROCESSOR_K3", processorK3Scheme, new StubConstantWorker(reservePipelineDelay), SimpleQueueFIFO.Infinite);

        SimpleScheme createScheme = new SimpleScheme("CRETE_SCHEME");
        CreateModule create = new CreateModule("CREATE", createScheme, new MockNormalWorker(DISTRIBUTION_MEAN, DISTRIBUTION_DEVIATION), new PacketDummyTaskFactory());
        var defaultPipeline = new ProcessorBlockingTransition("CREATE => K1", processorK1, (_) => ((float)processorD5.ProcessedTasksCount / create.CreatedTasksCount) < PIPELINE_BALANCE_THRESHOLD);
        var reservePipeline = new ProcessorBlockingTransition("CREATE => K1", processorK3, (_) => ((float)processorD5.ProcessedTasksCount / create.CreatedTasksCount) >= PIPELINE_BALANCE_THRESHOLD);
        createScheme.Attach(defaultPipeline);
        createScheme.Attach(reservePipeline);

        SystemModelController.Instance.Build(new Module[] { create, processorK1, processorK2, processorK3, processorK4, processorD5, dispose });
        SystemModelController.Instance.RunBenchmarkSimulation(SIMULATION_TIME, OnDropStatistics);

        failedTasksCountTotal += processorK2Scheme.FailedTasksCount + processorK4Scheme.FailedTasksCount;
        processedTasksCountTotal += processorK2Scheme.ProcessedTasksCount + processorK4Scheme.ProcessedTasksCount;
        float packetsLossFrequency = (float)failedTasksCountTotal / processedTasksCountTotal;

        using (StreamWriter writer = new StreamWriter(outputPath, true))
            writer.WriteLine($"{defaultPipelineDelay},{reservePipelineDelay},{packetLifetime},{MathF.Round(packetsLossFrequency, 5)}");

        void OnDropStatistics(float timeCurrent)
        {
            if (hasStatisticsDropOccurred)
                return;

            if (timeCurrent <= STATISTICS_AGGREGATION_TIMESTAMP)
            {
                hasStatisticsDropOccurred = true;
                processorK2Scheme.ResetStatistics();
                processorK4Scheme.ResetStatistics();
            }
        }
    }
}
