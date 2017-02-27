using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace ClusterPack
{
    public class SafeRandom
    {
        private static readonly RandomNumberGenerator seedProvider = RandomNumberGenerator.Create();

        [ThreadStatic]
        private static Random random;

        private static Random Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (random == null)
                {
                    var bytes = new byte[4];
                    seedProvider.GetBytes(bytes);
                    random = new Random(BitConverter.ToInt32(bytes, 0));
                }
                return random;
            }
        }

        public static int NextInt32() => Current.Next();
        public static int NextInt32(int max) => Current.Next(max);
        public static int NextInt32(int min, int max) => Current.Next(min, max);

        public static uint NexUInt32() => unchecked((uint)NextInt32());

        public static void NextBytes(byte[] bytes) => Current.NextBytes(bytes);
        
        public static IEnumerable<T> ChooseRandom<T>(T[] array, int n)
        {
            if (array.Length > n) throw new ArgumentException("Cannot pick more unique elements than array contains.", nameof(n));

            throw new NotImplementedException();
        }
    }
}