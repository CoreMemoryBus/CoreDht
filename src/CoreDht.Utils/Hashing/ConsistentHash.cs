using System;
using System.Linq;
using System.Text;

namespace CoreDht.Utils.Hashing
{
    public class ConsistentHash : IComparable<ConsistentHash>
    {
        public class InconsistentRankException : Exception
        {
            public InconsistentRankException(string message) : base(message)
            { }
        }

        public byte[] Bytes { get; private set; }

        public ConsistentHash(byte[] bytes)
        {
            Bytes = bytes.ToArray();
        }

        public ConsistentHash Zero()
        {
            return Zero(Bytes.Length);
        }
        public ConsistentHash One()
        {
            return One(Bytes.Length);
        }

        public static ConsistentHash Zero(int rank)
        {
            var bytes = new byte[rank];
            return new ConsistentHash(bytes);
        }

        public static ConsistentHash One(int rank)
        {
            var bytes = new byte[rank];
            bytes[0] = 1;
            return new ConsistentHash(bytes);
        }

        private const int Hexadecimal = 16;

        public static ConsistentHash NewFromHex(string hex)
        {
            int charCount = hex.Length;
            byte[] bytes = new byte[charCount / 2];
            for (int i = 0; i < charCount; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), Hexadecimal);
            }
            return new ConsistentHash(bytes);
        }

        public int Rank => Bytes.Length;

        public int BitCount => Rank * 8;

        public int CompareTo(ConsistentHash other)
        {
            VerifyRank(other);

            for (int i = 0; i < Bytes.Length; ++i)
            {
                if (Bytes[i] < other.Bytes[i]) return -1;
                if (Bytes[i] > other.Bytes[i]) return 1;
            }

            return 0;
        }

        private void VerifyRank(ConsistentHash other)
        {
            if (other.Rank != Rank)
            {
                throw new InconsistentRankException($"Other ConsistentHash rank ({other.Rank}) different from this ({Rank})");
            }
        }

        public static bool operator <(ConsistentHash lhs, ConsistentHash rhs)
        {
            return lhs.CompareTo(rhs) < 0;
        }

        public static bool operator >=(ConsistentHash lhs, ConsistentHash rhs)
        {
            return !(lhs < rhs);
        }

        public static bool operator <=(ConsistentHash lhs, ConsistentHash rhs)
        {
            return !(lhs > rhs);
        }

        public static bool operator >(ConsistentHash lhs, ConsistentHash rhs)
        {
            return lhs.CompareTo(rhs) > 0;
        }

        public static bool operator ==(ConsistentHash lhs, ConsistentHash rhs)
        {
            return lhs.CompareTo(rhs) == 0;
        }

        public static bool operator !=(ConsistentHash lhs, ConsistentHash rhs)
        {
            return !(lhs == rhs);
        }

        public override string ToString()
        {
            return ToBase58();
        }

        public string ToHex()
        {
            var hexString = new StringBuilder(Bytes.Length * 2);
            foreach (var b in Bytes.Reverse())
            {
                hexString.AppendFormat("{0:x2}", b);
            }
            return hexString.ToString();
        }

        public string ToBase58()
        {
            return Base58Check.Base58CheckEncoding.EncodePlain(Bytes);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as ConsistentHash;
            if (!ReferenceEquals(other, null))
            {
                VerifyRank(other);

                for (int i = 0; i < Bytes.Length; ++i)
                {
                    if (Bytes[i] != other.Bytes[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        private byte[] ModuloAdd(byte[] other)
        {
            var result = new byte[Rank];
            int carry = 0;
            for (int i = other.Length - 1; i >= 0; --i)
            {
                int sum = Bytes[i] + other[i] + carry;
                result[i] = (byte)(sum & 0xFF);
                carry = sum >> 8;
            } // Ignoring overflow implements modulo for us
            return result;
        }

        /// <summary>
        /// Shift the bits left (doubling the value). Does not accomodate a wraparound case.
        /// </summary>
        /// <param name="bitcount"></param>
        /// <returns></returns>
        public byte[] ShiftLeft(int bitcount)
        {
            byte[] temp = new byte[Bytes.Length];
            if (bitcount >= 8)
            {
                Array.Copy(Bytes, bitcount / 8, temp, 0, temp.Length - (bitcount / 8));
            }
            else
            {
                Array.Copy(Bytes, temp, temp.Length);
            }

            if (bitcount % 8 != 0)
            {
                for (int i = 0; i < temp.Length; i++)
                {
                    temp[i] <<= bitcount % 8;
                    if (i < temp.Length - 1)
                    {
                        temp[i] |= (byte)(temp[i + 1] >> 8 - bitcount % 8);
                    }
                }
            }
            return temp;
        }

        public static ConsistentHash operator +(ConsistentHash lhs, ConsistentHash rhs)
        {
            lhs.VerifyRank(rhs);
            return new ConsistentHash(lhs.ModuloAdd(rhs.Bytes));
        }

        public static ConsistentHash operator <<(ConsistentHash lhs, int shift)
        {
            return new ConsistentHash(lhs.ShiftLeft(shift));
        }
    }
}