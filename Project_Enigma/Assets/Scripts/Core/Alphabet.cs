using System;
using System.Globalization;
using System.Text;

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

        /// <summary>
        /// Index of a letter, or -1 when it is not part of the alphabet. Case and
        /// accents are ignored (Á counts as A) unless the accented letter is itself
        /// part of the alphabet, like Ñ in Spanish.
        /// </summary>
        public int IndexOf(char letter)
        {
            char upper = char.ToUpperInvariant(letter);
            int index = Letters.IndexOf(upper);
            return index >= 0 ? index : Letters.IndexOf(StripAccent(upper));
        }

        // Base letter of an accented one (É -> E), or the letter itself.
        static char StripAccent(char letter)
        {
            string decomposed = letter.ToString().Normalize(NormalizationForm.FormD);
            foreach (char c in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    return c;
                }
            }
            return letter;
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
