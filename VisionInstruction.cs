using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GantryPointGrey
{
    public class VisionInstruction
    {


        public string Cmd { get; set; }
        public string Data { get; set; }

        public VisionInstruction(string _Cmd, string _Data)
        {


            Cmd = _Cmd;
            Data = _Data;
        }

        public VisionInstruction(byte[] data)
        {
            //IsMale = BitConverter.ToBoolean(data, 0);
            //Age = BitConverter.ToUInt16(data, 1);
            int dataLength = BitConverter.ToInt32(data, 4);
            Cmd = Encoding.ASCII.GetString(data, 0, 4);
            Data = Encoding.ASCII.GetString(data, 8, dataLength);
        }

        /// <summary>
        ///  Serializes this package to a byte array.
        /// </summary>
        /// <remarks>
        /// Use the <see cref="Buffer"/> or <see cref="Array"/> class for better performance.
        /// </remarks>
        public byte[] ToByteArray()
        {
            List<byte> byteList = new List<byte>();

            byteList.AddRange(Encoding.ASCII.GetBytes(Cmd));
            byteList.AddRange(BitConverter.GetBytes(Data.Length));
            byteList.AddRange(Encoding.ASCII.GetBytes(Data));
            return byteList.ToArray();
        }
    }
}
