using System;
using System.Collections.Generic;
using LabWork4.Framework.Core.Controllers;
using LabWork4.Framework.Components.Queues.Concrete;
using LabWork4.Framework.Components.Modules.Common;
using LabWork4.Framework.Components.Modules.Concrete;
using LabWork4.Framework.Components.Workers.Concrete;
using LabWork4.Framework.Components.Schemes.Concrete;
using LabWork4.Framework.Components.Tasks.Utilities.Factories.Concrete;

namespace LabWork4.Application;

file sealed class Program
{
    private static void Main()
    {
        const float SIMULATION_TIME = 500.0f;
        const int BENCHMARK_MILLISECONDS = 1000;

        const int ITERATIONS_COUNT = 10;

        const int LINES_COUNT = 10;
        const int MODULES_COUNT_START = 100;
        const int MODULES_COUNT_DELTA = 100;
        const int MODULES_COUNT_FINISH = 1000;

        // Program.CreateBenchmarkModel().RunSimulation(SIMULATION_TIME, BENCHMARK_MILLISECONDS);

        IDictionary<int, IList<int>> samples = new Dictionary<int, IList<int>>();

        for (int modulesCount = MODULES_COUNT_START; modulesCount <= MODULES_COUNT_FINISH; modulesCount += MODULES_COUNT_DELTA)
            samples[modulesCount] = new int[ITERATIONS_COUNT];

        for (int iteration = 0; iteration < ITERATIONS_COUNT; ++iteration)
        {
            Console.WriteLine($"\n|LOG| [BENCHMARK] Iteration #{iteration + 1}\n");

            for (int modulesCount = MODULES_COUNT_START; modulesCount <= MODULES_COUNT_FINISH; modulesCount += MODULES_COUNT_DELTA)
            {
                SimulationModelController model = Program.CreateSequentialModel(modulesCount);
                // SimulationModelController model = Program.CreateParallelModel(modulesCount, LINES_COUNT);

                model.RunSimulation(SIMULATION_TIME);
                samples[modulesCount][iteration] = model.SimulationDurationMilliseconds;
                Console.WriteLine($"|LOG| [BENCHMARK] n: {modulesCount}; Duration: {model.SimulationDurationMilliseconds}ms");
            }
        }

        for (int modulesCount = MODULES_COUNT_START; modulesCount <= MODULES_COUNT_FINISH; modulesCount += MODULES_COUNT_DELTA)
            Console.Write($"\n|REPORT| [BENCHMARK] n: {modulesCount}; Duration (mean): {Program.CalculateDurationMean(samples[modulesCount])}ms");
    }

    private static BenchmarkModelController CreateBenchmarkModel()
    {
        DisposeModule dispose = new DisposeModule("dispose");
        ProcessorModule processor = new ProcessorModule("processor", new SingleTransitionScheme(dispose), new MockExponentialWorker(1.0f), new DefaultQueue(Int32.MaxValue));
        CreateModule create = new CreateModule("create", new SingleTransitionScheme(processor), new MockExponentialWorker(1.0f), new MockTaskFactory());
        return new BenchmarkModelController(new Module[] { create, processor, dispose });

    }

    private static SimulationModelController CreateSequentialModel(int modelsCount)
    {
        ProcessorModule nextProcessor;
        ProcessorModule previousProcessor;
        IList<Module> modules = new List<Module>();

        DisposeModule dispose = new DisposeModule("dispose");
        modules.Add((Module)dispose);

        previousProcessor = new ProcessorModule($"processor_{modelsCount}", new SingleTransitionScheme(dispose), new MockExponentialWorker(1.0f), new DefaultQueue(Int32.MaxValue));
        modules.Add((Module)previousProcessor);

        for (int i = modelsCount - 1; i > 0; --i)
        {
            nextProcessor = new ProcessorModule($"processor_{i}", new SingleTransitionScheme(previousProcessor), new MockExponentialWorker(1.0f), new DefaultQueue(Int32.MaxValue));
            modules.Add((Module)nextProcessor);
            previousProcessor = nextProcessor;
        }

        modules.Add(new CreateModule("create", new SingleTransitionScheme(previousProcessor), new MockExponentialWorker(1.0f), new MockTaskFactory()));

        return new SimulationModelController(modules);
    }

    private static SimulationModelController CreateParallelModel(int modelsCount, int linesCount)
    {
        const float MAX_PROBABILITY = 1.0f;
        const int DIRECT_CREATE_CONNECTIONS_COUNT = 1;

        float flowProbability = MAX_PROBABILITY / linesCount;
        int modelsPerLineCount = (modelsCount / linesCount) - DIRECT_CREATE_CONNECTIONS_COUNT;

        ProcessorModule nextProcessor;
        ProcessorModule previousProcessor;
        IList<Module> modules = new List<Module>();

        DisposeModule dispose = new DisposeModule("dispose");
        modules.Add((Module)dispose);

        ProbabilityScheme createScheme = new ProbabilityScheme(dispose);

        for (int i = 0; i < linesCount; ++i)
        {
            previousProcessor = new ProcessorModule($"processor_{i}_{0}", new SingleTransitionScheme(dispose), new MockExponentialWorker(1.0f), new DefaultQueue(Int32.MaxValue));
            modules.Add((Module)previousProcessor);

            for (int j = modelsPerLineCount - 1; j >= 0; --j)
            {
                nextProcessor = new ProcessorModule($"processor_{i}_{j}", new SingleTransitionScheme(previousProcessor), new MockExponentialWorker(1.0f), new DefaultQueue(Int32.MaxValue));
                modules.Add((Module)nextProcessor);
                previousProcessor = nextProcessor;
            }

            createScheme.Attach(previousProcessor, flowProbability);
        }

        modules.Add(new CreateModule("create", createScheme, new MockExponentialWorker(1.0f), new MockTaskFactory()));

        return new SimulationModelController(modules);
    }

    private static float CalculateDurationMean(IList<int> samples)
    {
        float durationTotal = 0.0f;

        for (int i = 0; i < samples.Count; ++i)
            durationTotal += samples[i];

        return durationTotal / samples.Count;
    }
}
