using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    //class RecvBuffer
    //{
    //    /* 기본상태

    //     r/w
    //     [ ][ ][ ][ ][ ][ ][ ][ ][ ][ ][ ][ ][ ][ ][ ]

    //    5byte "abcde" 전송

    //      r              w                
    //     [a][b][c][d][e][ ][ ][ ][ ][ ][ ][ ][ ][ ][ ]

    //    */
    //    ArraySegment<byte> _buffer;

    //    // 커서
    //    int readPos;
    //    int writePos;

    //    public RecvBuffer (int bufferSize)
    //    {
    //        _buffer = new ArraySegment<byte>(new byte[bufferSize],0,bufferSize);

    //    }

    //    public int DataSize { get { return writePos - readPos; } }
    //    public int FreeSize { get { return _buffer.Count - writePos; } }

    //    public ArraySegment<byte> ReadSegment
    //    {
    //        get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset+ readPos, DataSize); }
    //    }

    //    public ArraySegment<byte> WriteSegment
    //    {
    //        get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + writePos, FreeSize); }
    //    }

    //    // 커서 앞으로
    //    public void Clean()
    //    {
    //        // 남은 데이터가 없으면, 커서 위치만 맨 앞으로
    //        if (DataSize == 0)
    //        {
    //            readPos = writePos = 0;

    //        }
    //        else
    //        {
    //            // 남은 데이터가 있으면 그것을 앞당겨 줘야 한다.
    //            Array.Copy(_buffer.Array, _buffer.Offset +readPos, _buffer.Array, _buffer.Offset, DataSize);
    //            readPos = 0;
    //            writePos = DataSize;
    //        }
    //    }

    //    // Read 완료 시 호출 해서 read 커서 이동
    //    public bool OnRead(int numOfBytes)
    //    {
    //        if(numOfBytes > DataSize)
    //        {
    //            return false;
    //        }

    //        readPos += numOfBytes;
    //        return true;
    //    }

    //    // Wrtie 완료 시 호출 해서 write 커서 이동
    //    public bool OnWrite(int numOfBytes)
    //    {
    //        if(numOfBytes > FreeSize)
    //        {
    //            return false;
    //        }

    //        writePos += numOfBytes;
    //        return true;
    //    }
    //}
    public class RecvBuffer
    {
        // [r][][w][][][][][][][]
        ArraySegment<byte> _buffer;
        int _readPos;
        int _writePos;

        public RecvBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        public int DataSize { get { return _writePos - _readPos; } }
        public int FreeSize { get { return _buffer.Count - _writePos; } }

        public ArraySegment<byte> ReadSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
        }

        public ArraySegment<byte> WriteSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
        }

        public void Clean()
        {
            int dataSize = DataSize;
            if (dataSize == 0)
            {
                // 남은 데이터가 없으면 복사하지 않고 커서 위치만 리셋
                _readPos = _writePos = 0;
            }
            else
            {
                // 남은 찌끄레기가 있으면 시작 위치로 복사
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;
            }
        }

        public bool OnRead(int numOfBytes)
        {
            if (numOfBytes > DataSize)
                return false;

            _readPos += numOfBytes;
            return true;
        }

        public bool OnWrite(int numOfBytes)
        {
            if (numOfBytes > FreeSize)
                return false;

            _writePos += numOfBytes;
            return true;
        }
    }
}
