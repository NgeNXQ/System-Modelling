using System;
using System.Linq;
using System.Collections.Generic;
using CourseWork.Framework.Common;
using CourseWork.Framework.Components.Tasks.Common;
using CourseWork.Framework.Components.Modules.Common;
using CourseWork.Framework.Components.Modules.Concrete;

namespace CourseWork.Framework.Core.Controllers;

internal sealed class SimulationModelController //: IStatisticsPrinter
{
    private readonly IList<Module> modules;

    private float timeNext;
    private float timeCurrent;

    internal SimulationModelController(IList<Module> modules)
    {
        if (modules == null)
            throw new ArgumentNullException($"{nameof(modules)} cannot be null.");

        this.timeNext = 0.0f;
        this.modules = modules;
        this.timeCurrent = 0.0f;
    }

    internal void RunSimulation(float simulationTime)
    {
        IList<Module> nextModules;
        this.timeNext = this.modules.Min(module => module.TimeNext);

        while (timeNext < simulationTime)
        {
            this.timeCurrent = this.timeNext;

            foreach (Module module in this.modules)
                module.TimeCurrent = this.timeCurrent;

            nextModules = this.modules.Where(module => module.TimeNext == this.timeCurrent).ToList();

            foreach (Module module in nextModules)
                module.CompleteTask();

            this.timeNext = this.modules.Min(module => module.TimeNext);

            // this.PrintIntermediateStatistics();
            // this.EvaluateIntermediateStatistics();
        }

        // this.PrintFinalStatistics();
    }

    // private void EvaluateIntermediateStatistics()
    // {
    //     int totalCurrentTasksCount = (this.create.CreatedTasksCount + this.startTasksCount) - dispose.DisposedTasksCountTotal;
    //     this.tasksInsideCountMeanSum += totalCurrentTasksCount * this.deltaTime;
    // }

    public void PrintIntermediateStatistics()
    {
        foreach (Module module in this.modules)
            module.PrintIntermediateStatistics();
    }

    // public void PrintFinalStatistics()
    // {
    //     foreach (Module module in this.modules)
    //         module.PrintFinalStatistics();

    //     Console.WriteLine($"|REPORT| [SYSTEM] Task lifetime (mean): {this.dispose.DisposedTasksLifetimeMean}");
    //     Console.WriteLine($"|REPORT| [SYSTEM] Active tasks inside (mean): {this.tasksInsideCountMeanSum / this.timeCurrent}");
    //     Console.WriteLine($"|REPORT| [SYSTEM] Time between disposes (mean): {this.timeCurrent / this.dispose.DisposedTasksCountTotal}");
    // }
}