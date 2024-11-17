using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using LabWork4.Framework.Common;
using LabWork4.Framework.Components.Modules.Common;
using LabWork4.Framework.Components.Modules.Concrete;

namespace LabWork4.Framework.Core.Controllers;

internal sealed class SimulationModelController : IStatisticsPrinter
{
    // private readonly int startTasksCount;
    private readonly Stopwatch stopwatch;
    private readonly IList<Module> modules;

    private float timeNext;
    private float timeCurrent;

    internal SimulationModelController(IList<Module> modules)
    {
        if (modules == null)
            throw new ArgumentNullException($"{nameof(modules)} cannot be null.");

        this.stopwatch = new Stopwatch();

        this.timeNext = 0.0f;
        this.modules = modules;
        this.timeCurrent = 0.0f;
    }

    internal int SimulationDurationMilliseconds => this.stopwatch.Elapsed.Milliseconds;

    internal void RunSimulation(float simulationTime)
    {
        IList<Module> nextModules;
        this.timeNext = this.modules.Min(module => module.TimeNext);

        this.stopwatch.Restart();

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
        }

        this.stopwatch.Stop();

        // this.PrintFinalStatistics();
    }

    public void PrintIntermediateStatistics()
    {
        foreach (Module module in this.modules)
            module.PrintIntermediateStatistics();
    }

    public void PrintFinalStatistics()
    {
        // foreach (Module module in this.modules)
        //     module.PrintFinalStatistics();

        // Console.WriteLine($"|LOG| [SYSTEM] Duration: {this.stopwatch.Elapsed}");
    }
}