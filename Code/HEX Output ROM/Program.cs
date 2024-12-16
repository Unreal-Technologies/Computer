using System;
using System.IO;
using System.Text;

namespace HEX_Output_ROM
{
    public class Rom
    {
        #region Members
        private int pointer = 0;
        private readonly int[] data;
        private readonly string filename;
        private readonly int roms;
        #endregion //Members

        #region Properties
        public int[] Data { get { return data; } }
        public int Pointer { get { return pointer; } }
        #endregion //Properties

        #region Constructors
        private Rom(string filename, int addressCount, int byteLength = 8)
        {
            int size = 1 << addressCount;

            this.pointer = 0;
            this.data = new int[size];
            this.filename = filename;
            this.roms = byteLength / 8;
        }
        #endregion //Constructors

        #region Public Methods
        #region Static
        public static Rom AT28C256(string filename, int count)
        {
            return new Rom(filename, 15, 8 * count);
        }
        #endregion //Static

        public void PointAt(int location)
        {
            if(location < 0 || location >= data.Length)
            {
                throw new NotImplementedException();
            }

            this.pointer = location;
        }

        #region Write
        public void Write(int data)
        {
            this.data[this.pointer] = data;
            this.pointer++;
        }

        public void Write(int[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                this.data[i + this.pointer] = data[i];
                this.pointer++;
            }
        }
        #endregion //Write

        public void Create(bool createPlainText = false)
        {
            DirectoryInfo di = new DirectoryInfo(this.filename);
            if(!di.Exists)
            {
                DirectoryInfo parent = di.Parent;
                if(!parent.Exists)
                {
                    parent.Create();
                }
            }

            string path = di.Parent.FullName;
            int mask = 0xff;
            int shift = 0;
            for(int i=0; i<this.roms; i++)
            {
                string file = path + "\\" + di.Name.Replace(di.Extension, "") + "." + (this.roms - 1 - i) + di.Extension;
                byte[] rom = new byte[this.data.Length];

                int p = 0;
                foreach(int val in this.data)
                {
                    int rVal = (val >> shift) & mask;
                    rom[p++] = (byte)rVal;
                }

                shift += 8;

                FileStream fs = File.Open(file, FileMode.Create, FileAccess.Write);
                fs.Write(rom, 0, rom.Length);
                fs.Close();

                if (createPlainText)
                {
                    FileStream fs2 = File.Open(file.Replace(di.Extension, ".txt"), FileMode.Create, FileAccess.Write);

                    bool space = false;
                    string output = "";
                    foreach(byte val in rom)
                    {
                        string hex = Convert.ToString(val, 16).PadLeft(2, '0');

                        output += (space ? " " : "") + hex;

                        space = true;
                    }

                    byte[] textData = Encoding.UTF8.GetBytes(output);
                    fs2.Write(textData, 0, textData.Length);

                    fs2.Close();
                }
            }
        }
        #endregion //Public Methods
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Rom rom = Rom.AT28C256("Output/HEX Output Control.bin", 2);

            for(int i=0;i<256; i++)
            {
                Program.WriteValue(i, rom);
            }

            rom.Create(true);
        }

        private static void WriteValue(int val, Rom rom)
        {
            int lower = val & 0xf;
            int upper = (val >> 4) & 0xf;

            byte bWriteLower = Program.Get7SegmentValue(lower);
            byte bWriteUpper = Program.Get7SegmentValue(upper);

            rom.Write((bWriteUpper << 8) | bWriteLower);

            Console.WriteLine(
                val.ToString().PadLeft(3, '0') +
                " (" +
                Convert.ToString(val, 2).PadLeft(8, '0') +
                " | " +
                Convert.ToString(val, 16).PadLeft(2, '0') +
                "):  = " +
                Convert.ToString(bWriteUpper, 16).PadLeft(2, '0') +
                "-" +
                Convert.ToString(bWriteLower, 16).PadLeft(2, '0')
            );
        }

        private static byte Get7SegmentValue(int value)
        {
            switch(value)
            {
                case 0:
                    return 0x7e;
                case 1:
                    return 0x30;
                case 2:
                    return 0x6d;
                case 3:
                    return 0x79;
                case 4:
                    return 0x33;
                case 5:
                    return 0x5b;
                case 6:
                    return 0x5f;
                case 7:
                    return 0x70;
                case 8:
                    return 0x7f;
                case 9:
                    return 0x7b;
                case 10:
                    return 0x77;
                case 11:
                    return 0x1f;
                case 12:
                    return 0x4e;
                case 13:
                    return 0x3d;
                case 14:
                    return 0x4f;
                case 15:
                    return 0x47;
                default:
                    break;
            }
            return 0;
        }
    }
}
