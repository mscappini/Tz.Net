using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Tz.Net.Security
{
    public class Bip39
    {
        private List<string> wordList;

        private static readonly string BIP39_ENGLISH_RESOURCE_NAME = "mnemonic.wordlist.english.txt";
        private static readonly string BIP39_ENGLISH_SHA256 = "cad18e21a5354694610e49b147526db1b2c32f63dc4aff6cb0bc03690f757db1";
        //private static readonly String BIP39_ENGLISH_SHA256 = "ad90bf3beb7b0eb7e5acd74727dc0da96e0a280a258354e7293fb7e211ac03db";

        // UNIX time for when the BIP39 standard was finalised. This can be used as a default seed birthday.
        public static long BIP39_STANDARDISATION_TIME_SECS = 1381276800;

        private static readonly int PBKDF2_ROUNDS = 2048;

        public static Bip39 Instance;

        static Bip39()
        {
            Instance = new Bip39();
        }

        /// <summary>
        /// Initialise from the included word list.
        /// </summary>
        public Bip39()
            : this(OpenDefaultWords(), BIP39_ENGLISH_SHA256)
        { }

        /// <summary>
        /// Creates an MnemonicCode object, initializing with words read from the supplied input stream.  If a wordListDigest
        /// is supplied the digest of the words will be checked.
        /// <summary>
        public Bip39(Stream wordStream, string wordListDigest)
        {
            byte[] wordBytes = null;

            SHA256 md = SHA256.Create();
            md.Initialize();

            using (BufferedStream bs = new BufferedStream(wordStream))
            using (StreamReader br = new StreamReader(bs, Encoding.UTF8))
            {
                wordList = new List<string>(2048);

                string word;
                while ((word = br.ReadLine()) != null)
                {
                    wordList.Add(word);

                    wordBytes = Encoding.UTF8.GetBytes(word);

                    md.TransformBlock(wordBytes, 0, wordBytes.Length, wordBytes, 0);
                }

                if (wordBytes == null)
                {
                    throw new ArgumentException("input stream was empty");
                }

                md.TransformFinalBlock(wordBytes, 0, wordBytes.Length);
            }

            if (wordList.Count != 2048)
            {
                throw new ArgumentException("input stream did not contain 2048 words");
            }

            // If a wordListDigest is supplied check to make sure it matches.
            if (wordListDigest != null)
            {
                byte[] digest = md.Hash;

                string hexdigest = BytesToHex(digest);

                if (hexdigest != wordListDigest)
                {
                    throw new ArgumentException("wordlist digest mismatch");
                }
            }
        }

        /// <summary>
        /// Convert mnemonic word list to original entropy value.
        /// </summary>
        public byte[] ToEntropy(IEnumerable<string> words)
        {
            if (!words.Any())
            {
                throw new ArgumentException("Word list is empty.");
            }

            int wordCount = words.Count();

            if (wordCount % 3 > 0)
            {
                throw new ArgumentException("Word list size must be multiple of three words.");
            }

            // Look up all the words in the list and construct the
            // concatenation of the original entropy and the checksum.
            //
            int concatLenBits = wordCount * 11;
            bool[] concatBits = new bool[concatLenBits];
            int wordindex = 0;
            foreach (string word in words)
            {
                // Find the words index in the wordlist.
                int ndx = wordList.BinarySearch(word);
                if (ndx < 0)
                    throw new KeyNotFoundException(word);

                // Set the next 11 bits to the value of the index.
                for (int ii = 0; ii < 11; ++ii)
                    concatBits[(wordindex * 11) + ii] = (ndx & (1 << (10 - ii))) != 0;
                ++wordindex;
            }

            int checksumLengthBits = concatLenBits / 33;
            int entropyLengthBits = concatLenBits - checksumLengthBits;

            // Extract original entropy as bytes.
            byte[] entropy = new byte[entropyLengthBits / 8];
            for (int ii = 0; ii < entropy.Length; ++ii)
                for (int jj = 0; jj < 8; ++jj)
                    if (concatBits[(ii * 8) + jj])
                        entropy[ii] |= (byte)(1 << (7 - jj));

            // Take the digest of the entropy.
            byte[] hash = SHA256.Create().ComputeHash(entropy);
            bool[] hashBits = BytesToBits(hash);

            // Check all the checksum bits.
            for (int i = 0; i < checksumLengthBits; ++i)
                if (concatBits[entropyLengthBits + i] != hashBits[i])
                    throw new Exception("Checksum error");

            return entropy;
        }

        /// <summary>
        /// Convert mnemonic word list to seed.
        /// </summary>
        public byte[] ToSeed(IEnumerable<string> words, string passphrase)
        {
            if (string.IsNullOrWhiteSpace(passphrase))
            {
                throw new ArgumentException("A null passphrase is not allowed.", nameof(passphrase));
            }

            // To create binary seed from mnemonic, we use PBKDF2 function
            // with mnemonic sentence (in UTF-8) used as a password and
            // string "mnemonic" + passphrase (again in UTF-8) used as a
            // salt. Iteration count is set to 4096 and HMAC-SHA512 is
            // used as a pseudo-random function. Desired length of the
            // derived key is 512 bits (= 64 bytes).

            string pass = string.Join(" ", words);
            string saltString = "mnemonic" + passphrase;

            byte[] salt = Encoding.UTF8.GetBytes(saltString);

            return KeyDerivation.Pbkdf2(pass, salt, KeyDerivationPrf.HMACSHA512, PBKDF2_ROUNDS, 64);
        }

        /// <summary>
        /// Convert entropy data to mnemonic word list.
        /// </summary>
        public List<String> ToMnemonic(byte[] entropy)
        {
            if (entropy.Length == 0)
            {
                throw new ArgumentException("Entropy is empty.");
            }
            else if (entropy.Length % 4 > 0)
            {
                throw new ArgumentException("Entropy length not multiple of 32 bits.");
            }

            // We take initial entropy of ENT bits and compute its
            // checksum by taking first ENT / 32 bits of its SHA256 hash.

            byte[] hash = SHA256.Create().ComputeHash(entropy);
            bool[] hashBits = BytesToBits(hash);

            bool[] entropyBits = BytesToBits(entropy);
            int checksumLengthBits = entropyBits.Length / 32;

            // We append these bits to the end of the initial entropy. 
            bool[] concatBits = new bool[entropyBits.Length + checksumLengthBits];
            Array.Copy(entropyBits, 0, concatBits, 0, entropyBits.Length);
            Array.Copy(hashBits, 0, concatBits, entropyBits.Length, checksumLengthBits);

            // Next we take these concatenated bits and split them into
            // groups of 11 bits. Each group encodes number from 0-2047
            // which is a position in a wordlist.  We convert numbers into
            // words and use joined words as mnemonic sentence.

            List<string> words = new List<string>();

            int nwords = concatBits.Length / 11;

            for (int i = 0; i < nwords; ++i)
            {
                int index = 0;

                for (int j = 0; j < 11; ++j)
                {
                    index <<= 1;
                    if (concatBits[(i * 11) + j])
                        index |= 0x1;
                }

                words.Add(wordList[index]);
            }

            return words;
        }

        private static Stream OpenDefaultWords()
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Bip39), BIP39_ENGLISH_RESOURCE_NAME);

            if (stream == null)
            {
                throw new FileNotFoundException(BIP39_ENGLISH_RESOURCE_NAME);
            }

            return stream;
        }

        private static string BytesToHex(byte[] bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length * 2);

            foreach (byte b in bytes)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }

        private static bool[] BytesToBits(byte[] data)
        {
            bool[] bits = new bool[data.Length * 8];
            for (int i = 0; i < data.Length; ++i)
                for (int j = 0; j < 8; ++j)
                    bits[(i * 8) + j] = (data[i] & (1 << (7 - j))) != 0;
            return bits;
        }
    }
}
