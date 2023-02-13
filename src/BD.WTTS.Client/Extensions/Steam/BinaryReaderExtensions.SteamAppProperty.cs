#if WINDOWS || MACCATALYST || MACOS || LINUX

using Color = System.Drawing.Color;

// ReSharper disable once CheckNamespace
namespace System;

public static partial class BinaryReaderExtensions
{
    internal static string ReadAppInfoString(this BinaryReader reader)
    {
        byte[] buffer;
        int count;
        using (MemoryStream memoryStream = new MemoryStream())
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

    internal static string ReadAppInfoWideString(this BinaryReader reader)
    {
        StringBuilder stringBuilder = new StringBuilder();
        for (char c = (char)reader.ReadUInt16(); c != 0; c = (char)reader.ReadUInt16())
        {
            stringBuilder.Append(c);
        }
        return stringBuilder.ToString();
    }

    internal static void WriteAppInfoString(this BinaryWriter writer, string str)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(str);
        writer.Write(bytes);
        writer.Write((byte)0);
    }

    internal static void WriteAppInfoWideString(this BinaryWriter writer, string str)
    {
        byte[] bytes = Encoding.Unicode.GetBytes(str);
        writer.Write(bytes);
        writer.Write((byte)0);
    }

    internal static Color ReadColor(this BinaryReader reader)
    {
        byte red = reader.ReadByte();
        byte green = reader.ReadByte();
        byte blue = reader.ReadByte();
        return Color.FromArgb(red, green, blue);
    }

    internal static void Write(this BinaryWriter writer, Color color)
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
                    break;
                case SteamAppPropertyType.String:
                    writer.WriteAppInfoString((string)property.Value);
                    break;
                case SteamAppPropertyType.WString:
                    writer.WriteAppInfoWideString((string)property.Value);
                    break;
                case SteamAppPropertyType.Int32:
                    writer.Write((int)property.Value);
                    break;
                case SteamAppPropertyType.Uint64:
                    writer.Write((ulong)property.Value);
                    break;
                case SteamAppPropertyType.Float:
                    writer.Write((float)property.Value);
                    break;
                case SteamAppPropertyType.Color:
                    writer.Write((Color)property.Value);
                    break;
                default:
                    throw new NotImplementedException("The value type " + property.PropertyType.ToString() + " has not been implemented.");
            }
        }
        writer.Write((byte)8);
    }

    internal static SteamAppPropertyTable ReadPropertyTable(this BinaryReader reader)
    {
        SteamAppPropertyTable propertyTable = new SteamAppPropertyTable();
        SteamAppPropertyType propertyType;
        while ((propertyType = (SteamAppPropertyType)reader.ReadByte()) != SteamAppPropertyType._EndOfTable_)
        {
            string name = reader.ReadAppInfoString();
            propertyTable.AddPropertyValue(value: propertyType switch
            {
                SteamAppPropertyType.Table => reader.ReadPropertyTable(),
                SteamAppPropertyType.String => reader.ReadAppInfoString(),
                SteamAppPropertyType.WString => reader.ReadAppInfoWideString(),
                SteamAppPropertyType.Int32 => reader.ReadInt32(),
                SteamAppPropertyType.Uint64 => reader.ReadUInt64(),
                SteamAppPropertyType.Float => reader.ReadSingle(),
                SteamAppPropertyType.Color => reader.ReadColor(),
                _ => throw new NotImplementedException("The property type " + propertyType.ToString() + " has not been implemented."),
            }, name: name, type: propertyType);
        }
        return propertyTable;
    }
}

#endif