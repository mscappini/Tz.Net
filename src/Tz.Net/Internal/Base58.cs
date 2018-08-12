using System;
using Tz.Net.Extensions;

namespace Tz.Net.Internal
{
    /**
     * Base58 is a way to encode Bitcoin addresses (or arbitrary data) as alphanumeric strings.
     * Note that this is not the same base58 as used by Flickr, which you may find referenced around the Internet.
     * You may want to consider working with  VersionedChecksummedBytes instead, which
     * adds support for testing the prefix and suffix bytes commonly found in addresses.
     * Satoshi explains: why base-58 instead of standard base-64 encoding?
     * Don't want 0OIl characters that look the same in some fonts and
     * could be used to create visually identical looking account numbers.
     * A string with non-alphanumeric characters is not as easily accepted as an account number.
     * E-mail usually won't line-break if there's no punctuation to break at.
     * Doubleclicking selects the whole number as one word if it's all alphanumeric.
     * However, note that the encoding/decoding runs in O(n sup2) time, so it is not useful for large data.
     * The basic idea of the encoding is to treat the data bytes as a large number represented using
     * base-256 digits, convert the number to be represented using base-58 digits, preserve the exact
     * number of leading zeros (which are otherwise lost during the mathematical operations on the
     * numbers), and finally represent the resulting base-58 digits as alphanumeric ASCII characters.
     */
    internal static class Base58
    {
        public static readonly char[] ALPHABET = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz".ToCharArray();

        private static readonly char ENCODED_ZERO = ALPHABET[0];
        private static readonly int[] INDEXES = new int[128];

        static Base58()
        {
            for (int i = 0; i < INDEXES.Length; i++)
            {
                INDEXES[i] = -1;
            }

            for (int i = 0; i < ALPHABET.Length; i++)
            {
                INDEXES[ALPHABET[i]] = i;
            }
        }

        /**
         * Encodes the given bytes as a base58 string (no checksum is appended).
         *
         * param input the bytes to encode
         * return the base58-encoded string
         */
        public static string Encode(byte[] input)
        {
            if (input.Length == 0)
            {
                return "";
            }
            // Count leading zeros.
            int zeros = 0;
            while (zeros < input.Length && input[zeros] == 0)
            {
                ++zeros;
            }
            // Convert base-256 digits to base-58 digits (plus conversion to ASCII characters)
            input = (byte[])input.Clone(); // since we modify it in-place
            char[] encoded = new char[input.Length * 2]; // upper bound
            int outputStart = encoded.Length;
            for (int inputStart = zeros; inputStart < input.Length;)
            {
                encoded[--outputStart] = ALPHABET[divmod(input, inputStart, 256, 58)];
                if (input[inputStart] == 0)
                {
                    ++inputStart; // optimization - skip leading zeros
                }
            }
            // Preserve exactly as many leading encoded zeros in output as there were leading zeros in input.
            while (outputStart < encoded.Length && encoded[outputStart] == ENCODED_ZERO)
            {
                ++outputStart;
            }
            while (--zeros >= 0)
            {
                encoded[--outputStart] = ENCODED_ZERO;
            }
            // Return encoded string (including encoded leading zeros).
            return new string(encoded, outputStart, encoded.Length - outputStart);
        }

        /**
         * Decodes the given base58 string into the original data bytes.
         *
         *  param input the base58-encoded string to decode
         *  return the decoded data bytes
         * throws AddressFormatException if the given string is not a valid base58 string
         */
        public static byte[] Decode(string input)
        {
            if (input.Length == 0)
            {
                return new byte[0];
            }
            // Convert the base58-encoded ASCII chars to a base58 byte sequence (base58 digits).
            byte[] input58 = new byte[input.Length];
            for (int i = 0; i < input.Length; ++i)
            {
                char c = input[i];
                int digit = c < 128 ? INDEXES[c] : -1;
                if (digit < 0)
                {
                    throw new Exception("Illegal character " + c + " at position " + i);
                }
                input58[i] = (byte)digit;
            }
            // Count leading zeros.
            int zeros = 0;
            while (zeros < input58.Length && input58[zeros] == 0)
            {
                ++zeros;
            }
            // Convert base-58 digits to base-256 digits.
            byte[] decoded = new byte[input.Length];
            int outputStart = decoded.Length;
            for (int inputStart = zeros; inputStart < input58.Length;)
            {
                decoded[--outputStart] = divmod(input58, inputStart, 58, 256);
                if (input58[inputStart] == 0)
                {
                    ++inputStart; // optimization - skip leading zeros
                }
            }
            // Ignore extra leading zeroes that were added during the calculation.
            while (outputStart < decoded.Length && decoded[outputStart] == 0)
            {
                ++outputStart;
            }

            // Return decoded data (including original number of leading zeros).
            //return Arrays.copyOfRange(decoded, outputStart - zeros, decoded.length);
            return decoded.CopyOfRange(outputStart - zeros, decoded.Length);
        }

        /**
         * Divides a number, represented as an array of bytes each containing a single digit
         * in the specified base, by the given divisor. The given number is modified in-place
         * to contain the quotient, and the return value is the remainder.
         *
         *  param number     the number to divide
         *  param firstDigit the index within the array of the first non-zero digit
         *                   (this is used for optimization by skipping the leading zeros)
         *  param base       the base in which the number's digits are represented (up to 256)
         * param divisor    the number to divide by (up to 256)
         * return the remainder of the division operation
         */
        private static byte divmod(byte[] number, int firstDigit, int baze, int divisor)
        {
            // this is just long division which accounts for the base of the input digits
            int remainder = 0;
            for (int i = firstDigit; i < number.Length; i++)
            {
                int digit = (int)number[i] & 0xFF;
                int temp = remainder * baze + digit;
                number[i] = (byte)(temp / divisor);
                remainder = temp % divisor;
            }
            return (byte)remainder;
        }
    }
}