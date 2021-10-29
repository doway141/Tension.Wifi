using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tension.Model
{
    public enum FuncCode { ReadCoil = 0x01, ReadHoldingData = 0x03, WriteData = 0x06, WriteMultipleData = 0x10 };
    public enum TypeData { byteData,ushortData,shortData,uintData,intData,floatData,doubleData}
    public class ModbusInfo
    {
        public byte AddrStation { set; get; }
        public FuncCode FunCode { set; get; }
        public ushort AddrData { set; get; }            //数据地址为2个byte
        public ushort CntData { set; get; }               //数据个数为2个byte

        public TypeData TypData { set; get; }
        public object[] Data { set; get; }
    }
}
