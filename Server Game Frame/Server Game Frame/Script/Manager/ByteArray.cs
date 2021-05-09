using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Game_Frame.Script.Manager
{
    public class ByteArray
    {

        /// 默认字节数组大小
        /// 
        /// read index从哪个位置读数据
        /// 
        /// write index从哪个位置写数据
        /// 
        /// capacity 数组的容量是多少
        /// 
        /// initsize 初始化字节数组的长度
        /// 
        /// length 有效数据的长度
        /// 
        /// remain 剩余字节数组的长度
        /// 
        /// </summary>
        const int Defaute_Size = 1024;

        public byte[] bytes;

        public int readIndex = 0;

        public int writeIndex = 0;

        public int capacity = 0;

        public int initSize = 0;

        public int length { get { return writeIndex - readIndex; } }

        public int remain { get { return capacity - writeIndex; } }

        public ByteArray(int size = Defaute_Size)
        {
            bytes = new byte[size];

            writeIndex = 0;

            capacity = size;

            initSize = size;

            readIndex = 0;
        }

        public ByteArray(byte[] defauteByte)
        {
            bytes = defauteByte;

            capacity = defauteByte.Length;

            initSize = defauteByte.Length;

            writeIndex = defauteByte.Length;

            readIndex = 0;
        }

        public void ReSize(int size)
        {
            if (size < length)
                return;

            if (size < initSize)
            {
                return;
            }

            int n = 0;

            while (n < size)
            {
                n *= n;
            }

            capacity = n;

            byte[] newByte = new byte[size];

            Array.Copy(bytes, readIndex, newByte, 0, writeIndex - readIndex);

            bytes = newByte;

            writeIndex = bytes.Length;

            readIndex = 0;
        }

        public override string ToString()
        {
            return BitConverter.ToString(bytes, readIndex, length);
        }

        public string Debug()
        {
            return string.Format("readindex({0})writeindex({1})byte({2})", readIndex, writeIndex, BitConverter.ToString(bytes, 0, length));
        }

        //如果长度小于8
        public void CheckAndMoveBytes()
        {
            if (remain < 8)
            {
                MoveBytes();
            }
        }

        public void MoveBytes()
        {
            Array.Copy(bytes, readIndex, bytes, 0, length);

            writeIndex = length;

            readIndex = 0;

        }

        public int Write(byte[] bs, int offset, int count)
        {
            if (count > remain)
            {
                ReSize(count + length);
            }

            Array.Copy(bs, offset, bytes, writeIndex, count);

            writeIndex += count;

            return count;
        }

        public int Read(byte[] bs, int offset, int count)
        {
            count = Math.Min(count, length);

            Array.Copy(bytes, 0, bs, offset, count);

            readIndex += count;

            CheckAndMoveBytes();

            return count;
        }

        //计算消息的长度
        public Int16 ReadInt16()
        {
            if (length < 2)
            {
                return 0;
            }

            Int16 ret = (Int16)((bytes[1] << 8) | bytes[0]);

            readIndex += 2;

            CheckAndMoveBytes();

            return ret;
        }

        //计算32位的信息的长度
        public Int32 ReadInt32(byte[] bs, int offset, int count)
        {
            if (length < 4)
            {
                return 0;
            }

            Int32 ret = (Int32)((bytes[3] << 32) | bytes[2] << 16 | bytes[1] << 8 | bytes[0]);

            readIndex += 4;

            CheckAndMoveBytes();

            return ret;
        }
    }
}
