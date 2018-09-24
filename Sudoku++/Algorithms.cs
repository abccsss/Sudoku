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
    }
}
