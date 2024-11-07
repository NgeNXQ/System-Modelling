using System;
using System.Linq;
using System.Collections.Generic;
using LabWork2.Framework.Common;
using LabWork2.Framework.Components.Modules.Common;

namespace LabWork2.Framework.Core.Controllers;

internal sealed class SystemModelController : IStatisticsPrinter
{
    private readonly IList<Module> modules;

    private float timeNext;
    private float timeCurrent;

    internal SystemModelController(IList<Module> modules)
    {
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

            this.PrintIntermediateStatistics();
        }

        this.PrintFinalStatistics();
    }

    public void PrintFinalStatistics()
    {
        foreach (Module module in this.modules)
            module.PrintFinalStatistics();

        Console.Write('\n');
    }

    public void PrintIntermediateStatistics()
    {
        foreach (Module module in this.modules)
            module.PrintIntermediateStatistics();
    }
}