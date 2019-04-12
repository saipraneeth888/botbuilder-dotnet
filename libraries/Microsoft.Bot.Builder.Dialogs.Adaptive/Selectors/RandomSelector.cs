﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors
{
    /// <summary>
    /// Select a random true rule implementation of IRuleSelector.
    /// </summary>
    public class RandomSelector : IRuleSelector
    {
        private List<IRule> _rules;
        private bool _evaluate;
        private Random _rand;
        private int _seed = -1;
        private IExpressionParser _parser = new ExpressionEngine();

        /// <summary>
        /// Optional seed for random number generator.
        /// </summary>
        /// <remarks>If not specified a random seed will be used.</remarks>
        public int Seed
        {
            get => _seed;
            set
            {
                _seed = value;
                _rand = new Random(_seed);
            }
        }

        public Task Initialize(PlanningContext context, IEnumerable<IRule> rules, bool evaluate, CancellationToken cancel = default(CancellationToken))
        {
            _rules = rules.ToList();
            _evaluate = evaluate;
            if (_rand == null)
            {
                _rand = _seed == -1 ? new Random() : new Random(_seed);
            }
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<int>> Select(PlanningContext context, CancellationToken cancel = default(CancellationToken))
        {
            var candidates = new List<int>();
            for (var i = 0; i < _rules.Count; ++i)
            {
                if (_evaluate)
                {
                    var rule = _rules[i];
                    var expression = rule.GetExpression(_parser);
                    var (value, error) = expression.TryEvaluate(context.State);
                    var eval = error == null && (bool)value;
                    if (eval == true)
                    {
                        candidates.Add(i);
                    }
                }
                else
                {
                    candidates.Add(i);
                }
            }
            var result = new List<int>();
            if (candidates.Count > 0)
            {
                var selection = _rand.Next(candidates.Count);
                result.Add(candidates[selection]);
            }
            return Task.FromResult((IReadOnlyList<int>)result);
        }
    }
}
