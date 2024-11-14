using System;
using LabWork3.Framework.Components.Tasks.Common;

namespace LabWork3.Framework.Components.Tasks.Concrete;

internal sealed class PatientTask : Task
{
    private static readonly Random random;

    static PatientTask()
    {
        PatientTask.random = new Random();
    }

    public PatientTask(float timeCreation) : base(timeCreation)
    {
        this.InitialType = PatientTask.random.NextSingle() switch
        {
            <= 0.5f => 1,
            <= 0.6f => 2,
            _ => 3
        };

        this.CurrentType = this.InitialType;
    }
}
