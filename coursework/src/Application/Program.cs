using System;
using CourseWork.Framework.Core.Controllers;
using CourseWork.Framework.Components.Modules.Common;
using CourseWork.Framework.Components.Modules.Concrete;
using CourseWork.Framework.Components.Queues.Concrete;
using CourseWork.Framework.Components.Workers.Concrete;
using CourseWork.Framework.Components.Blueprints.Schemes.Common;
using CourseWork.Framework.Components.Blueprints.Schemes.Concrete;
using CourseWork.Framework.Components.Blueprints.Transitions.Concrete;
using CourseWork.Framework.Components.Tasks.Utilities.Factories.Concrete;

namespace CourseWork.Application;

file sealed class Program
{
    private static void Main()
    {
        const float TARGET_PACKET_LIFETIME = 10.0f;
        const float TARGET_FLOW_REDIRECTION_PROBABILITY = 0.3f;

        DisposeModule dispose = new DisposeModule("DISPOSE");

        IScheme decoderScheme = new SimpleScheme(new DisposeTransition(dispose));
        ProcessorModule decoder = new ProcessorModule("DECODER", decoderScheme, new StubConstantWorker(0), new SimpleQueue(Int32.MaxValue));

        SimpleScheme processorToDecoderScheme = new SimpleScheme(new AdvancedProcessorTransition(decoder, (task) => (dispose.TimeCurrent - task.TimestampCreation) > TARGET_PACKET_LIFETIME));

        #region TOP

        ProcessorModule processorK12 = new ProcessorModule("PROCESSOR_K12", processorToDecoderScheme, new StubConstantWorker(5), new SimpleQueue(Int32.MaxValue));

        IScheme processorK11Scheme = new SimpleScheme(new SimpleProcessorTransition(processorK12));
        ProcessorModule processorK11 = new ProcessorModule("PROCESSOR_K11", processorK11Scheme, new StubConstantWorker(5), new SimpleQueue(Int32.MaxValue));

        #endregion

        #region BOTTOM

        ProcessorModule processorK22 = new ProcessorModule("PROCESSOR_K12", processorToDecoderScheme, new StubConstantWorker(4), new SimpleQueue(Int32.MaxValue));

        IScheme processorK21Scheme = new SimpleScheme(new SimpleProcessorTransition(processorK22));
        ProcessorModule processorK21 = new ProcessorModule("PROCESSOR_K11", processorK11Scheme, new StubConstantWorker(4), new SimpleQueue(Int32.MaxValue));

        #endregion

        SimpleScheme createScheme = new SimpleScheme();
        createScheme.Attach(new AdvancedProcessorTransition(processorK11, (_) => processorToDecoderScheme.FailureProbability > TARGET_FLOW_REDIRECTION_PROBABILITY));
        createScheme.Attach(new AdvancedProcessorTransition(processorK21, (_) => processorToDecoderScheme.FailureProbability <= TARGET_FLOW_REDIRECTION_PROBABILITY));
        CreateModule create = new CreateModule("CREATE", createScheme, new MockNormalWorker(6.0f, 3.0f), new PacketDummyTaskFactory());

        new SimulationModelController(new Module[] { create, processorK11, processorK12, processorK21, processorK22, dispose }).RunSimulation(1000.0f);
    }
}
