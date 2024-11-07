using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LabWork2.Framework.Common;

namespace LabWork2.Framework.Components.Modules.Common;

internal abstract class Module : IStatisticsPrinter
{
    private readonly Random random;
    private readonly IDictionary<Module, float> neighbours;

    protected Module(string identifier)
    {
        if (String.IsNullOrWhiteSpace(identifier))
            throw new ArgumentNullException($"Invalid value of the {nameof(identifier)}.");

        this.random = new Random();
        this.Identifier = identifier;
        this.TimeNext = Single.MaxValue;
        this.neighbours = new Dictionary<Module, float>();
    }

    internal string Identifier { get; }

    internal float TimeNext { get; set; }

    private protected Module? NextModule
    {
        get
        {
            if (this.neighbours.Count == 0)
            {
                Console.WriteLine($"|LOG| (TRACE) {this.Identifier} disposes task.");
                return null;
            }

            float intervalUpperBound = 0.0f;
            float intervalLowerBound = 0.0f;
            float randomNumber = this.random.NextSingle();

            foreach (KeyValuePair<Module, float> neighbour in this.neighbours)
            {
                intervalUpperBound += neighbour.Value;

                if (randomNumber >= intervalLowerBound && randomNumber <= intervalUpperBound)
                {
                    Console.WriteLine($"|LOG| (TRACE) {this.Identifier} sends task to the {neighbour.Key.Identifier}.");
                    return neighbour.Key;
                }

                intervalLowerBound = intervalUpperBound;
            }

            return null;

            // int currentWeight = 0;
            // int randomValue = this.random.Next(this.totalNeighboursWeights);

            // foreach (KeyValuePair<Module, int> neighbour in this.neighbours)
            // {
            //     if (randomValue <= currentWeight)
            //     {
            //         Console.Write($"|LOG| (TRACE) {this.Identifier} ");
            //         Console.WriteLine($"sends task to the {neighbour.Key.Identifier}.");
            //         return neighbour.Key;
            //     }

            //     currentWeight += neighbour.Value;
            // }

            // Console.WriteLine($"|LOG| (TRACE) {this.Identifier} disposes task.");
            // return null;
        }
    }

    internal abstract float TimeCurrent { get; set; }

    internal abstract void AcceptTask();
    internal abstract void CompleteTask();
    private protected abstract void MoveTimeline();

    public abstract void PrintFinalStatistics();
    public abstract void PrintIntermediateStatistics();

    internal void AttachModule(Module module, float flowProbability)
    {
        if (module == null)
            throw new ArgumentNullException($"{nameof(module)} cannot be null.");

        if (flowProbability <= 0)
            throw new ArgumentException($"{nameof(flowProbability)} cannot be less or equals to 0.");

        this.neighbours.Add(module, flowProbability);
    }
}
