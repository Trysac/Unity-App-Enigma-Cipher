using System;

namespace EnigmaCipher.Core
{
    /// <summary>
    /// Ordered set of letters a machine works with. Every wiring is a
    /// permutation of these letters.
    /// </summary>
    public sealed class Alphabet
    {
        /// <summary>The 26 letters of the historical machines.</summary>
        public static readonly Alphabet Latin = new Alphabet("ABCDEFGHIJKLMNOPQRSTUVWXYZ");

        /// <summary>
        /// The 27 letters of the Spanish alphabet (Ñ after N) plus the space.
        /// A reflector pairs symbols, so it needs an even count; encrypting
        /// spaces also hides where words end.
        /// </summary>
        public static readonly Alphabet Spanish = new Alphabet("ABCDEFGHIJKLMNÑOPQRSTUVWXYZ ");

        public string Letters { get; }
        public int Size => Letters.Length;

        public Alphabet(string letters)
        {
            if (string.IsNullOrEmpty(letters))
            {
                throw new ArgumentException("An alphabet needs at least one letter.", nameof(letters));
            }
            for (int i = 0; i < letters.Length; i++)
            {
                if (letters.IndexOf(letters[i], i + 1) >= 0)
                {
                    throw new ArgumentException($"Letter '{letters[i]}' appears twice.", nameof(letters));
                }
            }
            Letters = letters;
        }

        /// <summary>Index of a letter (case-insensitive), or -1 when it is not part of the alphabet.</summary>
        public int IndexOf(char letter)
        {
            return Letters.IndexOf(char.ToUpperInvariant(letter));
        }

        public bool Contains(char letter) => IndexOf(letter) >= 0;

        public char this[int index] => Letters[Mod(index)];

        /// <summary>Wraps any integer into the range 0..Size-1.</summary>
        public int Mod(int value)
        {
            int result = value % Size;
            return result < 0 ? result + Size : result;
        }

        /// <summary>True when `wiring` uses every letter of the alphabet exactly once.</summary>
        public bool IsPermutation(string wiring)
        {
            if (wiring == null || wiring.Length != Size)
            {
                return false;
            }
            foreach (char letter in Letters)
            {
                if (wiring.IndexOf(letter) < 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
