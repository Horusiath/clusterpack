using System;
using System.Collections.Generic;
using System.Linq;
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
        
        /// <summary>
        /// Chooses a single element of an input <paramref name="array"/> at random.
        /// </summary>
        public static T Choose<T>(T[] array) => array[NextInt32(0, array.Length)];
        
        /// <summary>
        /// Returns (potentially infinite) enumerable collection of elements from input <paramref name="array"/>,
        /// potentially duplicating its contents.
        /// </summary>
        public static IEnumerable<T> InfiniteFrom<T>(T[] array)
        {
            while (true)
            {
                yield return Choose(array);
            }
        }

        /// <summary>
        /// Shuffles elements of provided enumerable sequence in random order,
        /// returning new shuffled sequence of element.
        /// This method does NOT duplicate elements of input sequence.
        /// </summary>
        public static IEnumerable<T> Shuffle<T>(IEnumerable<T> sequence) => sequence.OrderBy(x => NextInt32());
    }
}