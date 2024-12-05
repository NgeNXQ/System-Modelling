using System;
using System.Linq;
using System.Collections.Generic;
using Coursework.Framework.Common;
using Coursework.Framework.Components.Modules.Common;

namespace Coursework.Framework.Core.Controllers;

internal sealed class SystemModelController : IStatisticsPrinter
{
    private static readonly SystemModelController instance;

    private readonly IList<Module> modules;

    static SystemModelController()
    {
        SystemModelController.instance = new SystemModelController();
    }

    private SystemModelController()
    {
        this.modules = new List<Module>();
    }

    internal static SystemModelController Instance => SystemModelController.instance;

    internal float TimeNext { get; private set; }
    internal float TimeCurrent { get; private set; }
    internal float IterationsCount { get; private set; }

    internal void Build(IEnumerable<Module> modules)
    {
        this.modules.Clear();

        foreach (Module module in modules)
            this.modules.Add(module ?? throw new ArgumentNullException($"{nameof(module)} cannot be null."));
    }

    internal void RunSimulation(float simulationTime)
    {
        this.TimeNext = 0.0f;
        this.TimeCurrent = 0.0f;
        IList<Module> nextModules;

        this.TimeNext = this.modules.Min(module => module.TimeNext);

        while (TimeNext < simulationTime)
        {
            ++this.IterationsCount;

            foreach (Module module in this.modules)
                module.UpdateStatistics(this.TimeNext - this.TimeCurrent);

            this.TimeCurrent = this.TimeNext;

            foreach (Module module in this.modules)
                module.UpdateTimeline(this.TimeCurrent);

            nextModules = this.modules.Where(module => module.TimeNext == this.TimeCurrent).ToList();

            foreach (Module module in nextModules)
                module.CompleteTask();

            this.TimeNext = this.modules.Min(module => module.TimeNext);
        }

        this.PrintStatistics();
    }

    public void PrintStatistics()
    {
        foreach (Module module in this.modules)
            module.PrintStatistics();
    }
}
