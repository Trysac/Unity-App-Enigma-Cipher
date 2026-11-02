using System;

namespace EnigmaCipher.Core
{
    /// <summary>
    /// Fixed wheel that sends the signal back through the rotors. Its wiring
    /// swaps letters in pairs and never maps a letter to itself, which is what
    /// makes encryption and decryption the same operation.
    /// </summary>
    public sealed class Reflector
    {
        readonly int[] table;

        public string Name { get; }
        public string Wiring { get; }

        public Reflector(string name, Alphabet alphabet, string wiring)
        {
            if (!alphabet.IsPermutation(wiring))
            {
                throw new ArgumentException($"Reflector {name}: wiring must use every letter once.", nameof(wiring));
            }
            if (alphabet.Size % 2 != 0)
            {
                throw new ArgumentException($"Reflector {name}: an odd alphabet cannot be paired completely.", nameof(alphabet));
            }

            table = new int[alphabet.Size];
            for (int i = 0; i < alphabet.Size; i++)
            {
                int target = alphabet.IndexOf(wiring[i]);
                if (target == i)
                {
                    throw new ArgumentException($"Reflector {name}: '{wiring[i]}' is wired to itself.", nameof(wiring));
                }
                table[i] = target;
            }
            for (int i = 0; i < table.Length; i++)
            {
                if (table[table[i]] != i)
                {
                    throw new ArgumentException($"Reflector {name}: wiring must swap letters in pairs.", nameof(wiring));
                }
            }

            Name = name;
            Wiring = wiring;
        }

        public int Reflect(int contact) => table[contact];
    }
}
