using System;
using System.Collections.Generic;
using LabWork4.Framework.Components.Tasks.Common;
using LabWork4.Framework.Components.Schemes.Common;
using LabWork4.Framework.Components.Modules.Common;

namespace LabWork4.Framework.Components.Schemes.Concrete
{
    internal sealed class ProbabilityScheme : IScheme
    {
        private const float TOLERANCE = 0.001f;
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
            int index = 0;
            float intervalUpperBound = 0.0f;
            float intervalLowerBound = 0.0f;
            float randomNumber = this.random.NextSingle();

            foreach (KeyValuePair<Module, (float probability, Action<Task>? customLogicHandler)> neighbour in this.neighbours)
            {
                ++index;
                intervalUpperBound += neighbour.Value.probability;

                if (randomNumber >= intervalLowerBound - TOLERANCE && randomNumber <= intervalUpperBound + TOLERANCE)
                {
                    neighbour.Value.customLogicHandler?.Invoke(task);
                    return neighbour.Key;
                }

                if (index == this.neighbours.Count)
                    return neighbour.Key;

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

            if (probability + this.totalProbabilitiesSum > MAX_TOTAL_PROBABILITIES_SUM + TOLERANCE)
                throw new ArgumentException($"Total sum of {nameof(probability)} cannot exceed the {ProbabilityScheme.MAX_TOTAL_PROBABILITIES_SUM} threshold.");

            this.totalProbabilitiesSum += probability;
            this.neighbours.Add(module, (probability, customLogicHandler));
        }
    }
}