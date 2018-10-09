using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku
{
    static class Algorithms
    {
        public static int[] RandomPermutation(int size)
        {
            int[] perm = new int[size];
            var random = new Random();

            for (int i = 0; i < size - 1; i++)
            {
                int l = random.Next(size - i), k = 0;
                for (int j = 0; j < size; j++)
                    if (perm[j] == 0 && k++ == l)
                    {
                        perm[j] = i + 1;
                        break;
                    }
            }
            return perm;
        }

        public static bool NextCombination(int[] comb, int max, int size)
        {
            for(int i = 0; i < size; i++)
            {
                if (comb[size - 1 - i] < max - 1 - i)
                {
                    comb[size - 1 - i]++;
                    for(int j = size - i; j < size; j++)
                    {
                        comb[j] = comb[size - 1 - i] + (j + 1 + i - size);
                    }
                    return true;
                }
            }
            return false;
        }

        public static bool NextSubset(bool[] set, int size)
        {
            for(int i = 0; i < size; i++)
            {
                if (set[i]) set[i] = false;
                else
                {
                    set[i] = true;
                    return true;
                }
            }
            return false;
        }

        public static string TupleName(int size)
        {
            if (size <= 0) return "???-tuple";
            if (size == 1) return "Single";
            if (size == 2) return "Pair";
            if (size == 3) return "Triple";
            if (size == 4) return "Quadruple";
            if (size == 5) return "Quintuple";
            if (size == 6) return "Sextuple";
            if (size == 7) return "Septuple";
            if (size == 8) return "Octuple";
            if (size == 9) return "Nontuple";
            return $"{size}-tuple";
        }
    }
}
