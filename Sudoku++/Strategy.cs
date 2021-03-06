﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sudoku.Strategies;

namespace Sudoku
{
    abstract class Strategy
    {
        abstract public Step FindStep(Game game, bool generateHints);

        public const int MaxDifficulty = 7;

        public static Strategy[][] AllStrategies = new Strategy[][]
        {
            // difficulty 1
            new Strategy[] {
                new Validity(),
                new NakedSetStrategy(1),
                new HiddenSetStrategy(1)
            },

            // difficulty 2
            new Strategy[] {
                new NakedSetStrategy(2),
                new HiddenSetStrategy(2),
                new NakedSetStrategy(3),
                new HiddenSetStrategy(3)
            },

            // difficulty 3
            new Strategy[] {
                new NakedSetStrategy(4),
                new NakedSetStrategy(5),
                new HiddenSetStrategy(4),
                new NakedSetStrategy(6),
                new HiddenSetStrategy(5),
                new HiddenSetStrategy(6)
            },

            // difficulty 4
            new Strategy[] { },

            // difficulty 5
            new Strategy[] { },

            // difficulty 6
            new Strategy[] { },

            // difficulty 7
            new Strategy[] { }
        };

        public static Step FindStep(Game game, int difficulty, bool generateHints)
        {
            for (int i = 0; i < MaxDifficulty && i < difficulty; i++)
            {
                foreach (var strategy in AllStrategies[i])
                {
                    var step = strategy.FindStep(game, generateHints);
                    if (step != null)
                        return step;
                }
            }
            return null;
        }

        public static Game Solve(Game game, int difficulty)
        {
            bool flag = false;

            while (true)
            {
                var step = FindStep(game, difficulty, false);

                if (step == Step.Invalid || step == null)
                {
                    if (flag)
                        return game;
                    else
                        return null;
                }

                flag = true;
                game = step.NewGame;
            }
        }
    }
}
