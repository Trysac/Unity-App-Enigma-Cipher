using System;
using NUnit.Framework;

namespace EnigmaCipher.Core.Tests
{
    public class EnigmaMachineTests
    {
        // Enigma I, reflector B, rotors I-II-III (left to right).
        static EnigmaMachine EnigmaI_B_I_II_III() => Presets.EnigmaI.Build(0, 0, 1, 2);

        [Test]
        public void KnownVector_AAAAA_EncryptsToBDZGO()
        {
            EnigmaMachine machine = EnigmaI_B_I_II_III();

            Assert.AreEqual("BDZGO", machine.Process("AAAAA"));
        }

        [Test]
        public void KnownVector_WithRingSettings()
        {
            // Rotors I-II-III, reflector B, rings BBB, start AAA.
            EnigmaMachine machine = EnigmaI_B_I_II_III();
            machine.RingLetters = "BBB";

            Assert.AreEqual("EWTYX", machine.Process("AAAAA"));
        }

        [Test]
        public void SameSettings_DecryptTheCiphertext()
        {
            const string message = "THE QUICK BROWN FOX JUMPS OVER THE LAZY DOG";
            EnigmaMachine sender = EnigmaI_B_I_II_III();
            sender.WindowLetters = "MCK";
            sender.RingLetters = "DOG";
            sender.Plugboard.SetPairs("AV BS CG DL FU HZ IN KM OW RX");
            string cipher = sender.Process(message);

            EnigmaMachine receiver = EnigmaI_B_I_II_III();
            receiver.WindowLetters = "MCK";
            receiver.RingLetters = "DOG";
            receiver.Plugboard.SetPairs("AV BS CG DL FU HZ IN KM OW RX");

            Assert.AreNotEqual(message, cipher);
            Assert.AreEqual(message, receiver.Process(cipher));
        }

        [Test]
        public void NoLetter_EverEncryptsToItself()
        {
            EnigmaMachine machine = EnigmaI_B_I_II_III();
            string input = new string('E', 500);

            string output = machine.Process(input);

            StringAssert.DoesNotContain("E", output);
        }

        [Test]
        public void MiddleRotor_DoubleSteps()
        {
            // Classic sequence for rotors I-II-III: ADU -> ADV -> AEW -> BFX.
            EnigmaMachine machine = EnigmaI_B_I_II_III();
            machine.WindowLetters = "ADU";

            machine.Press('A');
            Assert.AreEqual("ADV", machine.WindowLetters);
            machine.Press('A');
            Assert.AreEqual("AEW", machine.WindowLetters);
            machine.Press('A');
            Assert.AreEqual("BFX", machine.WindowLetters);
        }

        [Test]
        public void CharactersWithoutKeys_ArePassedThroughWithoutStepping()
        {
            EnigmaMachine machine = EnigmaI_B_I_II_III();

            string output = machine.Process("A, A!");

            Assert.AreEqual("B, D!", output);
            Assert.AreEqual("AAC", machine.WindowLetters);
        }

        [Test]
        public void LowerCase_IsTreatedAsUpperCase()
        {
            Assert.AreEqual("BDZGO", EnigmaI_B_I_II_III().Process("aaaaa"));
        }

        [Test]
        public void Trace_FollowsTheSignalThroughEveryWheel()
        {
            EnigmaMachine machine = EnigmaI_B_I_II_III();

            KeyTrace trace = machine.Press('A').Value;

            // plugboard in, 3 rotors forward, reflector, 3 rotors back, plugboard out
            Assert.AreEqual(9, trace.Path.Count);
            Assert.AreEqual('B', trace.Output);
            Assert.AreEqual(machine.Alphabet.IndexOf('B'), trace.Path[trace.Path.Count - 1]);
        }

        [Test]
        public void Spanish_RoundTripsLettersWithÑAndSpaces()
        {
            const string message = "EL NIÑO ENSEÑA ESPAÑOL";
            EnigmaMachine sender = Presets.Spanish.Build(0, 2, 0, 4);
            sender.WindowLetters = "PÑA";
            string cipher = sender.Process(message);

            EnigmaMachine receiver = Presets.Spanish.Build(0, 2, 0, 4);
            receiver.WindowLetters = "PÑA";

            Assert.AreEqual(message, receiver.Process(cipher));
        }

        [Test]
        public void EveryPreset_HasValidWirings()
        {
            foreach (MachineModel model in Presets.All)
            {
                for (int i = 0; i < model.Rotors.Count; i++)
                {
                    Assert.DoesNotThrow(() => model.CreateRotor(i), $"{model.Name} rotor {model.Rotors[i].Name}");
                }
                for (int i = 0; i < model.Reflectors.Count; i++)
                {
                    Assert.DoesNotThrow(() => model.CreateReflector(i), $"{model.Name} reflector {model.Reflectors[i].Key}");
                }
            }
        }

        [Test]
        public void WindowLetters_RejectsWrongLengthOrUnknownLetters()
        {
            EnigmaMachine machine = EnigmaI_B_I_II_III();

            Assert.Throws<ArgumentException>(() => machine.WindowLetters = "AA");
            Assert.Throws<ArgumentException>(() => machine.WindowLetters = "A1A");
            Assert.AreEqual("AAA", machine.WindowLetters);
        }
    }

    public class PlugboardTests
    {
        [Test]
        public void SetPairs_SwapsBothWays()
        {
            var board = new Plugboard(Alphabet.Latin);
            board.SetPairs("AB, CD");

            Assert.AreEqual(1, board.Swap(0));
            Assert.AreEqual(0, board.Swap(1));
            Assert.AreEqual(4, board.Swap(4));
            CollectionAssert.AreEqual(new[] { "AB", "CD" }, board.Pairs);
        }

        [Test]
        public void SetPairs_RejectsReusedLetterAndLeavesBoardEmpty()
        {
            var board = new Plugboard(Alphabet.Latin);

            Assert.Throws<ArgumentException>(() => board.SetPairs("AB AC"));
            Assert.IsEmpty(board.Pairs);
        }

        [Test]
        public void SetPairs_RejectsBrokenPairs()
        {
            var board = new Plugboard(Alphabet.Latin);

            Assert.Throws<ArgumentException>(() => board.SetPairs("ABC"));
            Assert.Throws<ArgumentException>(() => board.SetPairs("AA"));
            Assert.Throws<ArgumentException>(() => board.SetPairs("A1"));
        }

        [Test]
        public void Disconnect_FreesBothLetters()
        {
            var board = new Plugboard(Alphabet.Latin);
            board.Connect('Q', 'W');

            board.Disconnect('W');

            Assert.IsEmpty(board.Pairs);
            Assert.DoesNotThrow(() => board.Connect('Q', 'E'));
        }
    }

    public class WiringValidationTests
    {
        [Test]
        public void Reflector_RejectsSelfWiredOrUnpairedLetters()
        {
            Assert.Throws<ArgumentException>(() => new Reflector("X", Alphabet.Latin, Alphabet.Latin.Letters));
            // A rotation is a permutation but does not swap in pairs.
            Assert.Throws<ArgumentException>(() => new Reflector("X", Alphabet.Latin, "BCDEFGHIJKLMNOPQRSTUVWXYZA"));
        }

        [Test]
        public void Rotor_RejectsIncompleteWiringAndUnknownNotch()
        {
            Assert.Throws<ArgumentException>(() => new Rotor("X", Alphabet.Latin, "ABC", "A"));
            Assert.Throws<ArgumentException>(() => new Rotor("X", Alphabet.Latin, Alphabet.Latin.Letters, "1"));
        }

        [Test]
        public void Alphabet_RejectsDuplicates()
        {
            Assert.Throws<ArgumentException>(() => new Alphabet("ABCA"));
        }
    }
}
