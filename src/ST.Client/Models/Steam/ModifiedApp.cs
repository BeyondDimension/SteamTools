using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using MPObj = MessagePack.MessagePackObjectAttribute;
using MPKey = MessagePack.KeyAttribute;
using MPIgnore = MessagePack.IgnoreMemberAttribute;
using System.IO;

namespace System.Application.Models
{
    [MPObj]
    public class ModifiedApp
    {
        public ModifiedApp()
        {
            ReadChanges();
        }

        public ModifiedApp(SteamApp app)
        {
            if (app.ChangesData == null)
            {
                throw new ArgumentException("New ModifiedApp Failed. SteamApp.ChangesData is null.");
            }
            AppId = app.AppId;
            OriginalData = (byte[])app.OriginalData.Clone();

            using BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream());
            binaryWriter.Write(app.ChangesData);

            ChangesData = binaryWriter.BaseStream.ToByteArray();
        }

        [MPKey(0)]
        public uint AppId { get; set; }

        private byte[]? originalData;

        [MPKey(1)]
        public byte[]? OriginalData { get => originalData; set => originalData = value; }

        private byte[]? changesData;

        [MPKey(2)]
        public byte[]? ChangesData { get => changesData; set => changesData = value; }

        [MPIgnore]
        public SteamAppPropertyTable? Changes { get; set; }

        public SteamAppPropertyTable? ReadChanges()
        {
            if (ChangesData != null)
            {
                using BinaryReader reader = new BinaryReader(new MemoryStream(ChangesData));
                return Changes = reader.ReadPropertyTable();
            }
            return null;
        }

        public SteamAppPropertyTable? ReadOriginalData()
        {
            if (OriginalData != null)
            {
                using BinaryReader reader = new BinaryReader(new MemoryStream(OriginalData));
                return reader.ReadPropertyTable();
            }
            return null;
        }
    }
}
