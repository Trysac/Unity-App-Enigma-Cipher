using System;
using System.Collections.Generic;

namespace EnigmaCipher.Core
{
    /// <summary>
    /// Steckerbrett: cables that swap pairs of letters before and after the rotors.
    /// </summary>
    public sealed class Plugboard
    {
        readonly Alphabet alphabet;
        readonly int[] table;

        /// <summary>Maximum number of cables (one per pair of letters).</summary>
        public int MaxPairs => alphabet.Size / 2;

        public Plugboard(Alphabet alphabet)
        {
            this.alphabet = alphabet;
            table = new int[alphabet.Size];
            Clear();
        }

        public void Clear()
        {
            for (int i = 0; i < table.Length; i++)
            {
                table[i] = i;
            }
        }

        /// <summary>Connects two free letters with a cable.</summary>
        public void Connect(char a, char b)
        {
            int first = RequireLetter(a);
            int second = RequireLetter(b);
            if (first == second)
            {
                throw new ArgumentException($"A cable needs two different letters, got '{a}' twice.");
            }
            if (table[first] != first || table[second] != second)
            {
                throw new ArgumentException($"'{alphabet[first]}' or '{alphabet[second]}' already has a cable.");
            }
            table[first] = second;
            table[second] = first;
        }

        /// <summary>Removes the cable plugged into `letter`, if any.</summary>
        public void Disconnect(char letter)
        {
            int index = RequireLetter(letter);
            int other = table[index];
            table[index] = index;
            table[other] = other;
        }

        /// <summary>
        /// Replaces every cable with the pairs in `spec`, e.g. "AB CD EF".
        /// Blank input clears the board.
        /// </summary>
        public void SetPairs(string spec)
        {
            Clear();
            if (string.IsNullOrWhiteSpace(spec))
            {
                return;
            }
            foreach (string pair in spec.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (pair.Length != 2)
                {
                    Clear();
                    throw new ArgumentException($"'{pair}' is not a pair of letters.");
                }
                try
                {
                    Connect(pair[0], pair[1]);
                }
                catch (ArgumentException)
                {
                    Clear();
                    throw;
                }
            }
        }

        /// <summary>Connected pairs, each written once, e.g. ["AB", "CD"].</summary>
        public IReadOnlyList<string> Pairs
        {
            get
            {
                var pairs = new List<string>();
                for (int i = 0; i < table.Length; i++)
                {
                    if (table[i] > i)
                    {
                        pairs.Add($"{alphabet[i]}{alphabet[table[i]]}");
                    }
                }
                return pairs;
            }
        }

        public int Swap(int contact) => table[contact];

        int RequireLetter(char letter)
        {
            int index = alphabet.IndexOf(letter);
            if (index < 0)
            {
                throw new ArgumentException($"'{letter}' is not a letter of this machine.");
            }
            return index;
        }
    }
}
