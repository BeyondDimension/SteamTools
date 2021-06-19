using System;
using System.Application.Models;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System.Application
{
    public static class SteamAppPropertyExtensions
    {
        public static string ReadAppInfoString(this BinaryReader reader)
        {
            byte[] buffer;
            int count;
            using (MemoryStream memoryStream = new())
            {
                byte value;
                while ((value = reader.ReadByte()) != 0)
                {
                    memoryStream.WriteByte(value);
                }
                buffer = memoryStream.GetBuffer();
                count = (int)memoryStream.Length;
            }
            return Encoding.UTF8.GetString(buffer, 0, count);
        }

        public static string ReadAppInfoWideString(this BinaryReader reader)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (char value = (char)reader.ReadUInt16(); value != '\0'; value = (char)reader.ReadUInt16())
            {
                stringBuilder.Append(value);
            }
            return stringBuilder.ToString();
        }

        public static void WriteAppInfoString(this BinaryWriter writer, string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            writer.Write(bytes);
            writer.Write(0);
        }

        public static void WriteAppInfoWideString(this BinaryWriter writer, string str)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(str);
            writer.Write(bytes);
            writer.Write(0);
        }

        public static Color ReadColor(this BinaryReader reader)
        {
            int red = (int)reader.ReadByte();
            byte green = reader.ReadByte();
            byte blue = reader.ReadByte();
            return Color.FromArgb(red, (int)green, (int)blue);
        }

        public static void Write(this BinaryWriter writer, Color color)
        {
            writer.Write(color.R);
            writer.Write(color.G);
            writer.Write(color.B);
        }


        internal static void Write(this BinaryWriter writer, SteamAppPropertyTable table)
        {
            foreach (SteamAppProperty property in table.Properties)
            {
                writer.Write((byte)property.PropertyType);
                writer.WriteAppInfoString(property.Name);
                switch (property.PropertyType)
                {
                    case SteamAppPropertyType.Table:
                        writer.Write((SteamAppPropertyTable)property.Value);
                        continue;
                    case SteamAppPropertyType.String:
                        writer.WriteAppInfoString((string)property.Value);
                        continue;
                    case SteamAppPropertyType.Int32:
                        writer.Write((int)property.Value);
                        continue;
                    case SteamAppPropertyType.Float:
                        writer.Write((float)property.Value);
                        continue;
                    case SteamAppPropertyType.WString:
                        writer.WriteAppInfoWideString((string)property.Value);
                        continue;
                    case SteamAppPropertyType.Color:
                        writer.Write((Color)property.Value);
                        continue;
                    case SteamAppPropertyType.Uint64:
                        writer.Write((ulong)property.Value);
                        continue;
                }
                throw new NotImplementedException("The value type " + property.PropertyType.ToString() + " has not been implemented.");
            }
            writer.Write(8);
        }

        internal static SteamAppPropertyTable ReadPropertyTable(this BinaryReader reader)
        {
            SteamAppPropertyTable propertyTable = new();
            SteamAppPropertyType type;
            while ((type = (SteamAppPropertyType)reader.ReadByte()) != SteamAppPropertyType._EndOfTable_)
            {
                string name = reader.ReadAppInfoString();
                object value;
                switch (type)
                {
                    case SteamAppPropertyType.Table:
                        value = reader.ReadPropertyTable();
                        break;
                    case SteamAppPropertyType.String:
                        value = reader.ReadAppInfoString();
                        break;
                    case SteamAppPropertyType.Int32:
                        value = reader.ReadInt32();
                        break;
                    case SteamAppPropertyType.Float:
                        value = reader.ReadSingle();
                        break;
                    case SteamAppPropertyType.WString:
                        value = reader.ReadAppInfoWideString();
                        break;
                    case SteamAppPropertyType.Color:
                        value = reader.ReadColor();
                        break;
                    case SteamAppPropertyType.Uint64:
                        value = reader.ReadUInt64();
                        break;
                    default:
                        Log.Error(nameof(SteamAppPropertyTable), "The property type " + type.ToString() + " has not been implemented.");
                        return null;
                }
                propertyTable.AddPropertyValue(name, type, value);
                continue;
            }
            return propertyTable;
        }
    }
}
