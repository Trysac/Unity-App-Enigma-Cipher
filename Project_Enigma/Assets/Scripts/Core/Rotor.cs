using System;

namespace EnigmaCipher.Core
{
    /// <summary>
    /// A wired wheel. The current signal enters at a contact, goes through the
    /// internal wiring and leaves at another contact; turning the wheel shifts
    /// which wire each contact meets.
    /// </summary>
    public sealed class Rotor
    {
        readonly Alphabet alphabet;
        readonly int[] forward;
        readonly int[] backward;
        readonly string notches;

        public string Name { get; }
        public string Wiring { get; }

        /// <summary>Letter shown in the machine's window (0-based index).</summary>
        public int Position { get; set; }

        /// <summary>Ringstellung: offset of the wiring against the letter ring (0-based).</summary>
        public int RingSetting { get; set; }

        public Rotor(string name, Alphabet alphabet, string wiring, string notches)
        {
            if (!alphabet.IsPermutation(wiring))
            {
                throw new ArgumentException($"Rotor {name}: wiring must use every letter once.", nameof(wiring));
            }
            foreach (char notch in notches ?? string.Empty)
            {
                if (!alphabet.Contains(notch))
                {
                    throw new ArgumentException($"Rotor {name}: notch '{notch}' is not a letter.", nameof(notches));
                }
            }

            Name = name;
            Wiring = wiring;
            this.alphabet = alphabet;
            this.notches = notches ?? string.Empty;
            forward = new int[alphabet.Size];
            backward = new int[alphabet.Size];
            for (int i = 0; i < alphabet.Size; i++)
            {
                int target = alphabet.IndexOf(wiring[i]);
                forward[i] = target;
                backward[target] = i;
            }
        }

        /// <summary>True when the window shows a turnover letter: the next step also turns the rotor on the left.</summary>
        public bool AtNotch => notches.IndexOf(alphabet[Position]) >= 0;

        public void Step()
        {
            Position = alphabet.Mod(Position + 1);
        }

        /// <summary>Signal travelling from the entry wheel towards the reflector.</summary>
        public int Forward(int contact) => Pass(contact, forward);

        /// <summary>Signal coming back from the reflector.</summary>
        public int Backward(int contact) => Pass(contact, backward);

        int Pass(int contact, int[] table)
        {
            int shift = Position - RingSetting;
            int wired = table[alphabet.Mod(contact + shift)];
            return alphabet.Mod(wired - shift);
        }

        /// <summary>Copy with the same wiring and settings, so a machine can be reset.</summary>
        public Rotor Clone()
        {
            return new Rotor(Name, alphabet, Wiring, notches) { Position = Position, RingSetting = RingSetting };
        }
    }
}
