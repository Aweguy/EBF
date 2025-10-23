using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace EBF.Systems
{
    /// <summary>
    /// Manages a set of attacks with weighted random selection. 
    /// After an attack is chosen, its weight is reset to its initial value, 
    /// while all other attacks gain weight, making them more likely to be picked next time.
    /// </summary>
    public class AttackManager
    {
        private readonly Dictionary<int, float> initialWeights = [];
        private readonly Dictionary<int, float> currentWeights = [];

        /// <summary>
        /// Selects an attack at random based on current weights. 
        /// The chosen attack’s weight is reset, while all others are rewarded.
        /// </summary>
        /// <returns>The ID of the chosen attack.</returns>
        public int Next()
        {
            if (currentWeights.Count < 1)
                throw new Exception("Error: AttackManager must have at least one attack stored before attempting to roll an attack.");

            float totalWeight = currentWeights.Values.Sum();
            float roll = Main.rand.NextFloat(totalWeight);

            float cumulative = 0f;
            foreach (var kvp in currentWeights)
            {
                cumulative += kvp.Value;
                if (roll < cumulative)
                {
                    UpdateWeights(kvp.Key);
                    return kvp.Key;
                }
            }

            return currentWeights.Keys.First(); // fallback
        }

        private void UpdateWeights(int usedAttack)
        {
            currentWeights[usedAttack] = initialWeights[usedAttack];

            // Reward all other attacks
            foreach (var key in currentWeights.Keys.ToList())
                if (key != usedAttack)
                    currentWeights[key] = Math.Min(initialWeights[key] * 10f, currentWeights[key] + 2f);
        }

        /// <summary>
        /// Adds a single attack with a specified starting weight.
        /// </summary>
        /// <param name="attackID">The unique ID for the attack.</param>
        /// <param name="weight">The initial weight for the attack.</param>
        /// <returns>This manager, so calls can be chained fluently.</returns>
        public AttackManager Add(int attackID, float weight)
        {
            initialWeights[attackID] = weight;
            currentWeights[attackID] = weight;
            return this;
        }
    }
}
