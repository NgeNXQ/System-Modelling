using System;
using Coursework.Framework.Core.Services;
using Coursework.Framework.Core.Controllers;
using Coursework.Framework.Components.Modules.Common;
using Coursework.Framework.Components.Modules.Concrete;
using Coursework.Framework.Components.Queues.Concrete;
using Coursework.Framework.Components.Workers.Concrete;
using Coursework.Framework.Components.Blueprints.Schemes.Concrete;
using Coursework.Framework.Components.Blueprints.Transitions.Concrete;
using Coursework.Framework.Components.Tasks.Utilities.Factories.Concrete;

namespace Coursework.Application;

file sealed class Program
{
    private static void Main()
    {
        Program.RunSimpleModel();
        // Program.RunVerificationModel(1000);

        // Benchmark benchmark = new Benchmark();
        // benchmark.GenerateGeneralSamples(20, "docs/papers/samples.csv");
        // benchmark.GenerateStabilizationSamples(1_000.0f, 100_000.0f, 500.0f, 20, "docs/papers/stabilization.csv");
        // benchmark.GenerateVerboseRegressionSamples(20, "docs/papers/regression.csv");
        // benchmark.GenerateCochranRegressionSamples(20, "docs/papers/cochran.csv");
    }

    private static void RunSimpleModel()
    {
        const float SIMULATION_TIME = 1000.0f;
        const float MAX_PACKET_LIFETIME = 10.0f;
        const float PIPELINE_BALANCE_THRESHOLD = 0.7f;

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

        SystemLoggerService.Instance.Initialize();

        SystemLoggerService.Instance.RegisterSender(create);
        SystemLoggerService.Instance.RegisterSender(processorD5);
        SystemLoggerService.Instance.RegisterSender(processorK1);
        SystemLoggerService.Instance.RegisterSender(processorK2);
        SystemLoggerService.Instance.RegisterSender(processorK3);
        SystemLoggerService.Instance.RegisterSender(processorK4);
        SystemLoggerService.Instance.RegisterSender(defaultPipeline);
        SystemLoggerService.Instance.RegisterSender(reservePipeline);
        SystemLoggerService.Instance.RegisterSender(processorK2Scheme);
        SystemLoggerService.Instance.RegisterSender(processorK4Scheme);

        SystemModelController.Instance.Build(new Module[] { create, processorK1, processorK2, processorK3, processorK4, processorD5, dispose });
        SystemModelController.Instance.RunSimulation(SIMULATION_TIME);

        int totalFailuresCount = processorK2Scheme.FailedTasksCount + processorK4Scheme.FailedTasksCount;
        int totalProcessedCount = processorK2Scheme.ProcessedTasksCount + processorK4Scheme.ProcessedTasksCount;
        Console.WriteLine($"|REPORT| (RESULTS) Packets loss frequency: {(float)totalFailuresCount / totalProcessedCount}");
        Console.WriteLine($"|REPORT| (RESULTS) Reserve pipeline connection frequency: {reservePipeline.ActivationsCount / reservePipeline.TimeUsageTotal}");
    }

    private static void RunVerificationModel(int iterationsCount)
    {
        const int PROCESSORS_COUNT = 4;
        const float SIMULATION_TIME = 1000.0f;
        const float DISTRIBUTION_MEAN = 6.0f;
        const float DISTRIBUTION_DEVIATION = 3.0f;
        const float MAX_PACKET_LIFETIME = 10.0f;
        const float CHANNEL_DEFAULT_PAYLOAD = 5.0f;
        const float CHANNEL_RESERVE_PAYLOAD = 4.0f;
        const float PIPELINE_BALANCE_THRESHOLD = 0.7f;

        int failedTasksCountTotal = 0;
        int createdTasksCountTotal = 0;
        int succeededTasksCountTotal = 0;
        int processedTasksCountTotal = 0;
        float processorsBusynessTotal = 0.0f;
        float processorsQueueMeanTotal = 0.0f;
        float reservePipleineTimeUsageTotal = 0.0f;
        int reservePipleineConnectionsCountTotal = 0;
        int defaultPipelineProcessedTasksCountTotal = 0;
        int reservePipelineProcessedTasksCountTotal = 0;
        float defaultPipelineTasksLifetimeSumTotal = 0.0f;
        float reservePipelineTasksLifetimeSumTotal = 0.0f;

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

            createdTasksCountTotal += create.CreatedTasksCount;
            succeededTasksCountTotal += processorD5.SucceededTasksCount;
            reservePipleineTimeUsageTotal += reservePipeline.TimeUsageTotal;
            reservePipleineConnectionsCountTotal += reservePipeline.ActivationsCount;
            defaultPipelineTasksLifetimeSumTotal += processorK2Scheme.TasksLifetimeSum;
            reservePipelineTasksLifetimeSumTotal += processorK4Scheme.TasksLifetimeSum;
            defaultPipelineProcessedTasksCountTotal += processorK2Scheme.ProcessedTasksCount;
            reservePipelineProcessedTasksCountTotal += processorK4Scheme.ProcessedTasksCount;
            failedTasksCountTotal += processorK2Scheme.FailedTasksCount + processorK4Scheme.FailedTasksCount;
            processedTasksCountTotal += processorK2Scheme.ProcessedTasksCount + processorK4Scheme.ProcessedTasksCount;
            processorsBusynessTotal += processorK1.Busyness + processorK2.Busyness + processorK3.Busyness + processorK4.Busyness;
            processorsQueueMeanTotal += processorK1.QueueMean + processorK2.QueueMean + processorK3.QueueMean + processorK4.QueueMean;
        }

        Console.WriteLine($"Created tasks: {(float)createdTasksCountTotal / iterationsCount}");
        Console.WriteLine($"Succeeded tasks: {(float)succeededTasksCountTotal / iterationsCount}");
        Console.WriteLine($"Failed tasks: {(float)failedTasksCountTotal / iterationsCount}");
        Console.WriteLine($"Tasks' lifetime mean in default pipeline: {defaultPipelineTasksLifetimeSumTotal / defaultPipelineProcessedTasksCountTotal}");
        Console.WriteLine($"Tasks' lifetime mean in reserve pipeline: {reservePipelineTasksLifetimeSumTotal / reservePipelineProcessedTasksCountTotal}");
        Console.WriteLine($"Processors' busyness mean: {processorsBusynessTotal / PROCESSORS_COUNT / iterationsCount}");
        Console.WriteLine($"Processors' queue mean: {processorsQueueMeanTotal / PROCESSORS_COUNT / iterationsCount}");
        Console.WriteLine($"Reserve pipeline connections: {(float)reservePipleineConnectionsCountTotal / iterationsCount}");
        Console.WriteLine($"Reserve pipeline time usage: {reservePipleineTimeUsageTotal / iterationsCount}");
        Console.WriteLine($"Packets loss frequency: {(float)failedTasksCountTotal / processedTasksCountTotal}");
        Console.WriteLine($"Reserve pipeline connection frequency: {reservePipleineConnectionsCountTotal / reservePipleineTimeUsageTotal}");
    }
}
