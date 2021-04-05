using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    public class SendBufferHelper
    {
        // 전역이지만, 쓰레드 별로 지역적으로 동작
        public static ThreadLocal<SendBuffer> CurrentBuffer =
           new ThreadLocal<SendBuffer>(() => { return null; });

        // 기본적인 buffer size 설정
        public static int ChunSize { get; set; } = 4096 * 100;

        // SendBuffer를 Open 할때 만약 할당된 SendBuffer의 공간이 부족하거나
        // 할당되기 이전이면 신규 할당해주고, 그 값을 반환해준다.
        // 공간이 있으면 그 값을 준다.
        public static ArraySegment<byte> Open(int reserveSize)
        {
            if(CurrentBuffer.Value == null)
            {
                CurrentBuffer.Value = new SendBuffer(ChunSize);
            }

            if (CurrentBuffer.Value.FreeSize < reserveSize)
                CurrentBuffer.Value = new SendBuffer(ChunSize);

            return CurrentBuffer.Value.Open(reserveSize);
        }

        // 사용한 버퍼를 알리고 반환한다.
        public static ArraySegment<byte> Close(int usedSize)
        {
            return CurrentBuffer.Value.Close(usedSize);
        }
    }



    public class SendBuffer
    {
        // [u][][][][][][][][][][][]...
        // u: used size
        byte[] _buffer;
        int _usedSize = 0;

        public SendBuffer(int chunkSize)
        {
            _buffer = new byte[chunkSize];
        }

        public int FreeSize { get { return _buffer.Length - _usedSize; } }

        // 사용 할 공간을 예약한다.
        // 반환값: 예약해서 가져온 공간
        public ArraySegment<byte> Open(int reserveSize)
        {
            if(reserveSize > FreeSize)
            {
                // return null;
                // 공간 부족일 경우 null을 반환해야 하는데 안되니까 빈 공간을 반환
                return new ArraySegment<byte>(_buffer, 0, 0);
            }

            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);

        }

        // 사용 한 공간을 알린다.
        // 반환값: 실제 사용한 공간
        public ArraySegment<byte> Close(int usedSize)
        {
            _usedSize += usedSize;
            return new ArraySegment<byte>(_buffer, _usedSize, usedSize);
        }

    }
}
