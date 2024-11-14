using System;
using System.Collections.Generic;
using LabWork3.Framework.Components.Tasks.Common;
using LabWork3.Framework.Components.Schemes.Common;
using LabWork3.Framework.Components.Modules.Common;

namespace LabWork3.Framework.Components.Schemes.Concrete
{
    internal sealed class ProbabilityScheme : IScheme
    {
        private const float MAX_TOTAL_PROBABILITIES_SUM = 1.0f;

        private readonly Random random;
        private readonly IDictionary<Module, (float probability, Action<Task>? customLogicHandler)> neighbours;

        private float totalProbabilitiesSum;

        internal ProbabilityScheme(Module fallback)
        {
            this.Fallback = fallback;
            this.random = new Random();
            this.neighbours = new Dictionary<Module, (float probability, Action<Task>? customLogicHandler)>();
        }

        public Module? Fallback { get; init; }

        public Module? GetNextModule(Task task)
        {
            float intervalUpperBound = 0.0f;
            float intervalLowerBound = 0.0f;
            float randomNumber = this.random.NextSingle();

            foreach (KeyValuePair<Module, (float probability, Action<Task>? customLogicHandler)> neighbour in this.neighbours)
            {
                intervalUpperBound += neighbour.Value.probability;

                if (randomNumber >= intervalLowerBound && randomNumber <= intervalUpperBound)
                {
                    neighbour.Value.customLogicHandler?.Invoke(task);
                    return neighbour.Key;
                }

                intervalLowerBound = intervalUpperBound;
            }

            return this.Fallback;
        }

        internal void Attach(Module module, float probability)
        {
            this.Attach(module, probability, null);
        }

        internal void Attach(Module module, float probability, Action<Task>? customLogicHandler)
        {
            if (module == null)
                throw new ArgumentNullException($"{nameof(module)} cannot be null.");

            if (probability <= 0)
                throw new ArgumentException($"{nameof(probability)} cannot be less or equals to 0.");

            if (probability + this.totalProbabilitiesSum > ProbabilityScheme.MAX_TOTAL_PROBABILITIES_SUM)
                throw new ArgumentException($"Total sum of {nameof(probability)} cannot exceed the {ProbabilityScheme.MAX_TOTAL_PROBABILITIES_SUM} threshold.");

            this.totalProbabilitiesSum += probability;
            this.neighbours.Add(module, (probability, customLogicHandler));
        }
    }
}
