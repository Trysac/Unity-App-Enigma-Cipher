using System.Linq;
using EnigmaCipher.Core;
using NUnit.Framework;

namespace EnigmaCipher.App.Tests
{
    public class MachineSessionTests
    {
        [Test]
        public void Defaults_AreEnigmaI_ReflectorB_RotorsI_II_III_AtAAA()
        {
            var session = new MachineSession();

            Assert.AreEqual("Enigma I", session.Model.Name);
            Assert.AreEqual("B", session.Machine.Reflector.Name);
            CollectionAssert.AreEqual(new[] { "I", "II", "III" }, session.Machine.Rotors.Select(r => r.Name));
            Assert.AreEqual("AAA", session.Machine.WindowLetters);
            // Known test vector for this configuration.
            Assert.AreEqual("BDZGO", session.ProcessText("AAAAA"));
        }

        [Test]
        public void SelectRotor_AlreadyInAnotherSlot_SwapsTheSlots()
        {
            var session = new MachineSession();

            session.SelectRotor(0, 2); // III goes left, I takes its old slot.

            Assert.AreEqual(2, session.RotorChoice(0));
            Assert.AreEqual(1, session.RotorChoice(1));
            Assert.AreEqual(0, session.RotorChoice(2));
            CollectionAssert.AreEqual(new[] { "III", "II", "I" }, session.Machine.Rotors.Select(r => r.Name));
        }

        [Test]
        public void SelectRotor_UnusedRotor_ReplacesOnlyThatSlot()
        {
            var session = new MachineSession();

            session.SelectRotor(1, 4);

            CollectionAssert.AreEqual(new[] { "I", "V", "III" }, session.Machine.Rotors.Select(r => r.Name));
        }

        [Test]
        public void TrySetPlugboard_Valid_AppliesNormalisedPairsAndCountsCables()
        {
            var session = new MachineSession();

            bool ok = session.TrySetPlugboard("ba  dc,EF", out string error);

            Assert.IsTrue(ok);
            Assert.IsNull(error);
            Assert.AreEqual("AB CD EF", session.PlugboardPairs);
            Assert.AreEqual(3, session.CableCount);
            Assert.AreEqual(13, session.MaxCables);
        }

        [Test]
        public void TrySetPlugboard_Invalid_KeepsThePreviousCablesAndReportsTheCoreMessage()
        {
            var session = new MachineSession();
            session.TrySetPlugboard("AB", out _);

            bool ok = session.TrySetPlugboard("CD AC", out string error);

            Assert.IsFalse(ok);
            StringAssert.Contains("already has a cable", error);
            Assert.AreEqual("AB", session.PlugboardPairs);
            Assert.AreEqual(1, session.CableCount);
        }

        [Test]
        public void ChangingASetting_ResetsTheLiveMachineToTheStartPositions()
        {
            var session = new MachineSession();
            session.StepStart(2, 1); // AAB
            session.Press('A');
            session.Press('A');
            Assert.AreEqual("AAD", session.Machine.WindowLetters);

            session.StepRing(0, 1);

            Assert.AreEqual("AAB", session.Machine.WindowLetters);
            Assert.AreEqual(string.Empty, session.TypedText);
            Assert.IsNull(session.LastTrace);
        }

        [Test]
        public void StepStart_WrapsAroundTheAlphabet()
        {
            var session = new MachineSession();

            session.StepStart(0, -1);

            Assert.AreEqual('Z', session.StartLetter(0));
        }

        [Test]
        public void ProcessText_RoundTrip_DoesNotMoveTheLiveMachine()
        {
            var session = new MachineSession();
            session.SetStartLetters("MCK");
            session.SetRingLetters("DOG");
            session.TrySetPlugboard("AV BS CG DL FU HZ IN KM OW RX", out _);
            const string message = "HELLO WORLD";

            string cipher = session.ProcessText(message);
            string plain = session.ProcessText(cipher);

            Assert.AreNotEqual(message, cipher);
            Assert.AreEqual(message, plain);
            Assert.AreEqual("MCK", session.Machine.WindowLetters);
        }

        [Test]
        public void Press_MatchesProcessTextAndRecordsTheTape()
        {
            var session = new MachineSession();
            string expected = session.ProcessText("HELLO WORLD");

            foreach (char c in "HELLO WORLD")
            {
                session.Press(c);
            }

            Assert.AreEqual("HELLO WORLD", session.TypedText);
            Assert.AreEqual(expected, session.LitText);
            Assert.IsTrue(session.LastTrace.HasValue);
        }

        [Test]
        public void DescribeTrace_ListsEveryStageInSignalOrder()
        {
            var session = new MachineSession();

            KeyTrace trace = session.Press('A').Value;
            string path = session.DescribeTrace(trace);

            // First key of the AAAAA vector lights B.
            StringAssert.StartsWith("key A  →  plug A  →  III: ", path);
            StringAssert.Contains("→  reflector B: ", path);
            StringAssert.EndsWith("plug B  →  lamp B", path);
            Assert.AreEqual(11, path.Split('→').Length); // 11 stages, 10 arrows
        }

        [Test]
        public void SelectModel_Spanish_UsesItsAlphabetAndClearsCablesItCannotKeep()
        {
            var session = new MachineSession();
            session.TrySetPlugboard("AB", out _);

            session.SelectModel(1);

            Assert.AreEqual("Enigma ES", session.Model.Name);
            Assert.AreEqual(28, session.Alphabet.Size);
            Assert.AreEqual("AB", session.PlugboardPairs);
            Assert.IsTrue(session.TrySetPlugboard("ÑZ", out _));

            session.SelectModel(0);

            Assert.AreEqual(string.Empty, session.PlugboardPairs);
        }

        [Test]
        public void KeyboardLayouts_CoverEachAlphabetExactlyOnce()
        {
            foreach (MachineModel model in Presets.All)
            {
                string keys = string.Concat(KeyboardLayouts.For(model));

                Assert.AreEqual(model.Alphabet.Size, keys.Length, model.Name);
                Assert.IsTrue(model.Alphabet.IsPermutation(keys), model.Name);
            }
        }
    }
}
