using System.Collections.Generic;
using LabWork2.Framework.Core.Controllers;
using LabWork2.Framework.Components.Modules.Common;
using LabWork2.Framework.Components.Modules.Concrete;
using LabWork2.Framework.Components.Workers.Concrete;

namespace LabWork2.Application;

file sealed class Program
{
    private static void Main()
    {
        Program.RunExampleModel();
        // Program.RunAdvancedModel();
    }

    private static void RunExampleModel()
    {
        CreateModule create = new CreateModule("create", new MockExponentialWorker(5.0f));
        ProcessorModule processor1 = new ProcessorModule("processor1", new MockExponentialWorker(5.0f), 5);
        ProcessorModule processor2 = new ProcessorModule("processor2", new MockExponentialWorker(5.0f), 4);
        ProcessorModule processor3 = new ProcessorModule("processor3", new MockExponentialWorker(5.0f), 3);

        create.AttachModule(processor1, 1.0f);
        processor1.AttachModule(processor2, 1.0f);
        processor2.AttachModule(processor3, 1.0f);

        new SystemModelController(new List<Module>() { create, processor1, processor2, processor3 }).RunSimulation(1000);
    }

    private static void RunAdvancedModel()
    {
        CreateModule create = new CreateModule("create", new MockUniformWorker(1.0f, 5.0f));
        ProcessorModule processor1 = new ProcessorModule("processor1", new MockExponentialWorker(2.5f), 3);
        ProcessorModule processor2 = new ProcessorModule("processor2", new MockNormalWorker(2.5f, 0.5f), 2);
        MultiProcessorModule multiProcessor3 = new MultiProcessorModule("multiProcessor3", new MockExponentialWorker(2.25f), 0, 2);

        create.AttachModule(processor1, 1.0f);
        processor1.AttachModule(processor2, 0.5f);
        processor1.AttachModule(multiProcessor3, 0.5f);
        processor2.AttachModule(processor1, 0.5f);
        processor2.AttachModule(multiProcessor3, 0.5f);

        new SystemModelController(new List<Module>() { create, processor1, processor2, multiProcessor3 }).RunSimulation(1000);
    }

}