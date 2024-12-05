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
        const float MAX_PACKET_LIFETIME = 10.0f;
        const float TARGET_PIPELINE_DELTA = 0.7f;

        DisposeModule dispose = new DisposeModule("DISPOSE");

        SimpleScheme decoderScheme = new SimpleScheme("DECODER_SCHEME", new DisposeTransition("DECODER => DISPOSE", dispose));
        ProcessorModule decoder = new ProcessorModule("DECODER", decoderScheme, new StubConstantWorker(0), SimpleQueueFIFO.Infinite);

        SimpleScheme processorK12Scheme = new SimpleScheme("K12_SCHEME", new ProcessorBlockingTransition("K12 => DECODER", decoder, (task) => task.Lifetime > MAX_PACKET_LIFETIME));
        ProcessorModule processorK12 = new ProcessorModule("PROCESSOR_K12", processorK12Scheme, new StubConstantWorker(5), SimpleQueueFIFO.Infinite);

        SimpleScheme processorK11Scheme = new SimpleScheme("K11_SCHEME", new ProcessorSimpleTransition("K11 => K12", processorK12));
        ProcessorModule processorK11 = new ProcessorModule("PROCESSOR_K11", processorK11Scheme, new StubConstantWorker(5), SimpleQueueFIFO.Infinite);

        SimpleScheme processorK22Scheme = new SimpleScheme("K22_SCHEME", new ProcessorBlockingTransition("K22 => DECODER", decoder, (task) => task.Lifetime > MAX_PACKET_LIFETIME));
        ProcessorModule processorK22 = new ProcessorModule("PROCESSOR_K22", processorK22Scheme, new StubConstantWorker(4), SimpleQueueFIFO.Infinite);

        SimpleScheme processorK21Scheme = new SimpleScheme("K21_SCHEME", new ProcessorSimpleTransition("K21 => K22", processorK22));
        ProcessorModule processorK21 = new ProcessorModule("PROCESSOR_K21", processorK21Scheme, new StubConstantWorker(4), SimpleQueueFIFO.Infinite);

        SimpleScheme createScheme = new SimpleScheme("CRETE_SCHEME");
        CreateModule create = new CreateModule("CREATE", createScheme, new MockNormalWorker(6.0f, 3.0f), new PacketDummyTaskFactory());
        var defaultPipeline = new ProcessorBlockingTransition("CREATE => K11", processorK11, (_) => ((float)decoder.SucceededTasksCount / create.CreatedTasksCount) < TARGET_PIPELINE_DELTA);
        var reservePipeline = new ProcessorBlockingTransition("CREATE => K21", processorK21, (_) => ((float)decoder.SucceededTasksCount / create.CreatedTasksCount) >= TARGET_PIPELINE_DELTA);
        createScheme.Attach(defaultPipeline);
        createScheme.Attach(reservePipeline);

        SystemLoggerService.Instance.Initialize();

        SystemLoggerService.Instance.RegisterSender(create);
        SystemLoggerService.Instance.RegisterSender(decoder);
        SystemLoggerService.Instance.RegisterSender(processorK11);
        SystemLoggerService.Instance.RegisterSender(processorK12);
        SystemLoggerService.Instance.RegisterSender(processorK21);
        SystemLoggerService.Instance.RegisterSender(processorK22);
        SystemLoggerService.Instance.RegisterSender(defaultPipeline);
        SystemLoggerService.Instance.RegisterSender(reservePipeline);
        SystemLoggerService.Instance.RegisterSender(processorK12Scheme);
        SystemLoggerService.Instance.RegisterSender(processorK22Scheme);

        SystemModelController.Instance.Build(new Module[] { create, processorK11, processorK12, processorK21, processorK22, decoder, dispose });
        SystemModelController.Instance.RunSimulation(1000.0f);

        int totalFailuresCount = processorK12Scheme.FailuresCount + processorK22Scheme.FailuresCount;
        int totalProcessedCount = processorK12Scheme.ProcessedCount + processorK22Scheme.ProcessedCount;
        Console.WriteLine($"|REPORT| (STATS) Packets loss: {(float)totalFailuresCount / totalProcessedCount}");
    }
}
