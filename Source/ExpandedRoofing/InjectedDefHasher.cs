using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace ExpandedRoofing
{
    // took code from the following repository to fix hasher:
    // https://github.com/UnlimitedHugs/RimworldHugsLib/blob/55d2232c3accab908439ac52c3d676c3a2bd0a0b/Source/Utils/InjectedDefHasher.cs
    public static class InjectedDefHasher
    {
        private delegate void GiveShortHashTakenHashes(Def def, Type defType, HashSet<ushort> takenHashes);
        private delegate void GiveShortHash(Def def, Type defType);
        private static GiveShortHash giveShortHashDelegate;

        internal static void PrepareReflection()
        {
            var takenHashesField = typeof(ShortHashGiver).GetField(
                    "takenHashesPerDeftype", BindingFlags.Static | BindingFlags.NonPublic);
            var takenHashesDictionary = takenHashesField?.GetValue(null) as Dictionary<Type, HashSet<ushort>>;
            if (takenHashesDictionary == null) throw new Exception("taken hashes");

            var methodInfo = typeof(ShortHashGiver).GetMethod(
                "GiveShortHash", BindingFlags.NonPublic | BindingFlags.Static,
                null, new[] { typeof(Def), typeof(Type), typeof(HashSet<ushort>) }, null);
            if (methodInfo == null) throw new Exception("hashing method");

            var hashDelegate = (GiveShortHashTakenHashes)Delegate.CreateDelegate(
                typeof(GiveShortHashTakenHashes), methodInfo);
            giveShortHashDelegate = (def, defType) => {
                var takenHashes = takenHashesDictionary.TryGetValue(defType);
                if (takenHashes == null)
                {
                    takenHashes = new HashSet<ushort>();
                    takenHashesDictionary.Add(defType, takenHashes);
                }
                hashDelegate(def, defType, takenHashes);
            };
        }

        public static void GiveShortHasToDef(Def newDef, Type defType)
        {
            if (giveShortHashDelegate == null) throw new Exception("Hasher not initalized");
            giveShortHashDelegate(newDef, defType);
        }
    }
}
