using System;
using System.Collections.Generic;
using LabWork3.Framework.Core.Controllers;
using LabWork3.Framework.Components.Tasks.Common;
using LabWork3.Framework.Components.Tasks.Concrete;
using LabWork3.Framework.Components.Queues.Concrete;
using LabWork3.Framework.Components.Modules.Common;
using LabWork3.Framework.Components.Modules.Concrete;
using LabWork3.Framework.Components.Workers.Common;
using LabWork3.Framework.Components.Workers.Concrete;
using LabWork3.Framework.Components.Schemes.Concrete;
using LabWork3.Framework.Components.Tasks.Utilities.Factories.Concrete;

namespace LabWork3.Application;

file sealed class Program
{
    private static void Main()
    {
        Program.RunCarBankModel();
        // Program.RunHospitalModel();
    }

    private static void RunCarBankModel()
    {
        const int MAX_QUEUE_LENGTH = 3;
        int injectionsCount = 0;

        CreateModule create;
        DisposeModule dispose;
        ProcessorModule cashier1;
        ProcessorModule cashier2;

        dispose = new DisposeModule("dispose");

        DefaultQueue cashier1Queue = new DefaultQueue(MAX_QUEUE_LENGTH);
        cashier1 = new ProcessorModule("cashier_1", new SingleTransitionScheme(dispose), new MockExponentialWorker(0.3f), cashier1Queue);

        DefaultQueue cashier2Queue = new DefaultQueue(MAX_QUEUE_LENGTH);
        cashier2 = new ProcessorModule("cashier_2", new SingleTransitionScheme(dispose), new MockExponentialWorker(0.3f), cashier2Queue);

        QueuePriorityScheme createScheme = new QueuePriorityScheme(dispose, InjectCustomLogic);
        createScheme.Attach(cashier1);
        createScheme.Attach(cashier2);
        create = new CreateModule("create", createScheme, new MockExponentialWorker(0.5f), new CarTaskFactory(), 0.1f);

        cashier1Queue.AddLast(new CarTask(0.0f));
        cashier1Queue.AddLast(new CarTask(0.0f));
        cashier1.AcceptInitialTask(new CarTask(0.0f), new MockNormalWorker(1, 0.3f).DelayPayload);

        cashier2Queue.AddLast(new CarTask(0.0f));
        cashier2Queue.AddLast(new CarTask(0.0f));
        cashier2.AcceptInitialTask(new CarTask(0.0f), new MockNormalWorker(1, 0.3f).DelayPayload);

        cashier1Queue.TaskAdded += OnTaskAdded;
        cashier1Queue.TaskRemoved += OnTaskRemoved;
        cashier2Queue.TaskAdded += OnTaskAdded;
        cashier2Queue.TaskRemoved += OnTaskRemoved;

        new SimulationModelController(new Module[] { create, cashier1, cashier2, dispose }).RunSimulation(1000.0f);

        Console.WriteLine($"|REPORT| [CUSTOM] Injections: {injectionsCount}");

        cashier1Queue.TaskAdded -= OnTaskAdded;
        cashier1Queue.TaskRemoved -= OnTaskRemoved;
        cashier2Queue.TaskAdded -= OnTaskAdded;
        cashier2Queue.TaskRemoved -= OnTaskRemoved;

        ProcessorModule? InjectCustomLogic()
        {
            if (cashier1.Queue.Count == 0)
                return cashier1;

            if ((cashier1.Queue.Count != MAX_QUEUE_LENGTH) && (cashier1.Queue.Count == cashier2.Queue.Count))
                return cashier1;

            return null;
        }

        void OnTaskAdded(object? sender, EventArgs eventArgs)
        {
            BalanceQueues(cashier1, cashier2);
        }

        void OnTaskRemoved(object? sender, EventArgs eventArgs)
        {
            BalanceQueues(cashier1, cashier2);
        }

        void BalanceQueues(ProcessorModule source, ProcessorModule target)
        {
            const int TARGET_DELTA_QUEUES_LENGTH = 2;

            int deltaQueueLength = source.Queue.Count - target.Queue.Count;

            if (Math.Abs(deltaQueueLength) >= TARGET_DELTA_QUEUES_LENGTH)
            {
                if (deltaQueueLength > 0)
                {
                    Task task = source.Queue.RemoveLast();
                    target.Queue.AddLast(task);
                }
                else if (deltaQueueLength < 0)
                {
                    Task task = target.Queue.RemoveLast();
                    source.Queue.AddLast(task);
                }

                ++injectionsCount;
            }
        }
    }

    private static void RunHospitalModel()
    {
        const int PATIENT_TYPE_1 = 1;
        const int PATIENT_TYPE_2 = 2;
        const int PATIENT_TYPE_3 = 3;

        int injectionsCount = 0;

        CreateModule create;
        DisposeModule dispose;
        CustomMultiProcessorModule reception;
        ProcessorModule laboratoryPathForward;
        ProcessorModule laboratoryPathBackwards;
        ProcessorModule laboratoryRegistry;
        MultiProcessorModule hospitalWardsPath;
        MultiProcessorModule laboratoryExamination;

        dispose = new DisposeModule("dispose");

        TypeScheme receptionScheme = new TypeScheme(dispose);
        Dictionary<int, IMockWorker> receptionWorkers = new Dictionary<int, IMockWorker>();
        receptionWorkers[PATIENT_TYPE_1] = new MockExponentialWorker(15);
        receptionWorkers[PATIENT_TYPE_2] = new MockExponentialWorker(40);
        receptionWorkers[PATIENT_TYPE_3] = new MockExponentialWorker(30);
        reception = new CustomMultiProcessorModule("reception", receptionScheme, receptionWorkers, new DefaultQueue(Int32.MaxValue), 2);

        hospitalWardsPath = new MultiProcessorModule("hospital_wards_path", new SingleTransitionScheme(dispose), new MockUniformWorker(3, 8), new DefaultQueue(Int32.MaxValue), 3);

        ProbabilityScheme laboratoryExaminationScheme = new ProbabilityScheme(dispose);
        laboratoryExamination = new MultiProcessorModule("laboratory_examination", laboratoryExaminationScheme, new MockErlangWorker(4.0f, 2), new DefaultQueue(Int32.MaxValue), 2);

        laboratoryRegistry = new ProcessorModule("laboratory_registry", new SingleTransitionScheme(laboratoryExamination, dispose), new MockErlangWorker(4.5f, 3), new DefaultQueue(Int32.MaxValue));

        laboratoryPathForward = new ProcessorModule("laboratory_path_forward", new SingleTransitionScheme(laboratoryRegistry, dispose), new MockUniformWorker(2, 5), new DefaultQueue(Int32.MaxValue));

        laboratoryPathBackwards = new ProcessorModule("laboratory_path_backwards", new SingleTransitionScheme(reception, dispose), new MockUniformWorker(2, 5), new DefaultQueue(Int32.MaxValue));

        laboratoryExaminationScheme.Attach(dispose, 0.5f);
        laboratoryExaminationScheme.Attach(laboratoryPathBackwards, 0.5f, InjectCustomLogic);

        receptionScheme.Attach(hospitalWardsPath, PATIENT_TYPE_1);
        receptionScheme.Attach(laboratoryPathForward, PATIENT_TYPE_2);
        receptionScheme.Attach(laboratoryPathForward, PATIENT_TYPE_3);

        create = new CreateModule("create", new SingleTransitionScheme(reception, dispose), new MockExponentialWorker(15), new PatientTaskFactory());

        new SimulationModelController(new Module[] { create, reception, laboratoryPathForward, laboratoryRegistry, laboratoryExamination, laboratoryPathBackwards, hospitalWardsPath, dispose }).RunSimulation(1000.0f);

        Console.WriteLine($"|REPORT| [CUSTOM] Injections: {injectionsCount}");

        float totalVisitorsCount = laboratoryExamination.Queue.Count + laboratoryExamination.SuccessesCount + laboratoryExamination.BusySubProcessors.Count;
        Console.WriteLine($"|REPORT| [CUSTOM] Laboratory average arrival time: {dispose.TimeCurrent / totalVisitorsCount}");

        void InjectCustomLogic(Task task)
        {
            ++injectionsCount;
            task.CurrentType = PATIENT_TYPE_1;
        }
    }
}
