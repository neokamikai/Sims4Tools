﻿/***************************************************************************
 *  Copyright (C) 2009, 2010 by Peter L Jones                              *
 *  pljones@users.sf.net                                                   *
 *                                                                         *
 *  This file is part of the Sims 3 Package Interface (s3pi)               *
 *                                                                         *
 *  s3pi is free software: you can redistribute it and/or modify           *
 *  it under the terms of the GNU General Public License as published by   *
 *  the Free Software Foundation, either version 3 of the License, or      *
 *  (at your option) any later version.                                    *
 *                                                                         *
 *  s3pi is distributed in the hope that it will be useful,                *
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of         *
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
 *  GNU General Public License for more details.                           *
 *                                                                         *
 *  You should have received a copy of the GNU General Public License      *
 *  along with s3pi.  If not, see <http://www.gnu.org/licenses/>.          *
 ***************************************************************************/
using s4pi.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace CASPartResource
{
    public class CASPartResourceTS4 : AResource
    {
        const int recommendedApiVersion = 1;
        public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
        public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }

        static bool checking = s4pi.Settings.Settings.Checking;

        #region Attributes
        public uint version;
        uint tgiOffset;
        uint presetCount;
        string name;
        float sortPriority;
        ushort secondarySortIndex;
        private uint propertyID;
        uint auralMaterialHash;
        PramFlag parmFlags;
        ulong excludePartFlags;
        uint excludeModifierRegionFlags;
        FlagList flagList;
        uint simlolencePrice;
        uint partTitleKey;
        uint partDesptionKey;
        byte uniqueTextureSpace;
        int bodyType;
        int unused1;
        uint ageGender;
        byte unused2;
        byte unused3;
        SwatchColorList swatchColorCode;
        byte buffResKey;
        byte varientThumbnailKey;
        byte nakedKey;
        byte parentKey;
        int sortLayer;
        LODBlockList lodBlockList;
        SimpleList<byte> slotKey;
        byte difussShadowKey;
        byte shadowKey;
        byte compositionMethod;
        byte regionMapKey;
        byte overrides;
        byte normalMapKey;
        byte specularMapKey;
        private CountedTGIBlockList tgiList;
        #endregion

        public CASPartResourceTS4(int APIversion, Stream s) : base(APIversion, s) { if (stream == null) { stream = UnParse(); OnResourceChanged(this, EventArgs.Empty); } stream.Position = 0; Parse(stream); }

        #region Data I/O
        void Parse(Stream s)
        {
            s.Position = 0;
            BinaryReader r = new BinaryReader(s);
            version = r.ReadUInt32();
            tgiOffset = r.ReadUInt32() + 8;
            presetCount = r.ReadUInt32();
            if (presetCount != 0) throw new Exception("Found non-zero one");
            name = BigEndianUnicodeString.Read(s);

            sortPriority = r.ReadSingle();
            this.secondarySortIndex = r.ReadUInt16();
            propertyID = r.ReadUInt32();
            this.auralMaterialHash = r.ReadUInt32();
            this.parmFlags = (PramFlag)r.ReadByte();
            this.excludePartFlags = r.ReadUInt64();
            this.excludeModifierRegionFlags = r.ReadUInt32();
            
            flagList = new FlagList(OnResourceChanged, s);

            this.simlolencePrice = r.ReadUInt32();
            this.partTitleKey = r.ReadUInt32();
            this.partDesptionKey = r.ReadUInt32();
            this.uniqueTextureSpace = r.ReadByte();
            this.bodyType = r.ReadInt32();
            this.unused1 = r.ReadInt32();
            this.ageGender = r.ReadUInt32();
            this.unused2 = r.ReadByte();
            this.unused3 = r.ReadByte();

            swatchColorCode = new SwatchColorList(OnResourceChanged, s);

            this.buffResKey = r.ReadByte();
            this.varientThumbnailKey = r.ReadByte();
            this.nakedKey = r.ReadByte();
            this.parentKey = r.ReadByte();
            this.sortLayer = r.ReadInt32();


            // TGI block list
            long currentPosition = r.BaseStream.Position;
            r.BaseStream.Position = tgiOffset;            
            byte count4 = r.ReadByte();
            tgiList = new CountedTGIBlockList(OnResourceChanged, "IGT", count4, s);
            r.BaseStream.Position = currentPosition;

            lodBlockList = new LODBlockList(null, s, tgiList);

            byte count = r.ReadByte();
            this.slotKey = new SimpleList<byte>(null);
            for (byte i = 0; i < count; i++) this.slotKey.Add(r.ReadByte());

            this.difussShadowKey = r.ReadByte();
            this.shadowKey = r.ReadByte();
            this.compositionMethod = r.ReadByte();
            this.regionMapKey = r.ReadByte();
            this.overrides = r.ReadByte();
            this.normalMapKey = r.ReadByte();
            this.specularMapKey = r.ReadByte();
        }

        protected override Stream UnParse()
        {
            MemoryStream s = new MemoryStream();
            BinaryWriter w = new BinaryWriter(s);

            w.Write(this.version);
            w.Write(0); // tgi offset
            w.Write(presetCount);
            BigEndianUnicodeString.Write(s, name);
            w.Write(sortPriority);
            w.Write(secondarySortIndex);
            w.Write(propertyID);
            w.Write(auralMaterialHash);
            w.Write((byte)parmFlags);
            w.Write(excludePartFlags);
            w.Write(excludeModifierRegionFlags);
            flagList.UnParse(s);
            w.Write(simlolencePrice);
            w.Write(partTitleKey);
            w.Write(partDesptionKey);
            w.Write(uniqueTextureSpace);
            w.Write(bodyType);
            w.Write(unused1);
            w.Write(ageGender);
            w.Write(unused2);
            w.Write(unused3);
            swatchColorCode.UnParse(s);
            w.Write(buffResKey);
            w.Write(varientThumbnailKey);
            w.Write(nakedKey);
            w.Write(parentKey);
            w.Write(sortLayer);
            lodBlockList.UnParse(s);

            w.Write((byte)this.slotKey.Count);
            foreach (var b in this.slotKey) w.Write(b);
            w.Write(difussShadowKey);
            w.Write(shadowKey);
            w.Write(compositionMethod);
            w.Write(regionMapKey);
            w.Write(overrides);
            w.Write(normalMapKey);
            w.Write(specularMapKey);

            long tgiPosition = w.BaseStream.Position;
            w.BaseStream.Position = 4;
            w.Write(tgiPosition - 8);
            w.BaseStream.Position = tgiPosition;

            w.Write((byte)tgiList.Count);
            foreach (var tgi in tgiList) tgi.UnParse(s);

            return s;
        }
        #endregion
        
        #region Content Fields
        [ElementPriority(0)]
        public uint Version { get { return version; } set { if (value != version)version = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(1)]
        public uint TGIoffset { get { return tgiOffset; } }
        [ElementPriority(2)]
        public uint PresetCount { get { return presetCount; } set { if (value != presetCount) presetCount = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(3)]
        public string Name { get { return name; } set { if (!value.Equals(name)) name = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(4)]
        public float SortPriority { get { return sortPriority; } set { if (!value.Equals(sortPriority)) sortPriority = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(5)]
        public UInt16 SecondarySortIndex { get { return secondarySortIndex; } set { if (!value.Equals(secondarySortIndex)) sortPriority = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(6)]
        public uint PropertyID { get { return propertyID; } set { if (!value.Equals(propertyID)) propertyID = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(7)]
        public uint AuralMaterialHash { get { return auralMaterialHash; } set { if (!value.Equals(this.auralMaterialHash)) { this.auralMaterialHash = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(8)]
        public PramFlag ParmFlags { get { return parmFlags; } set { if (!value.Equals(parmFlags)) parmFlags = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(9)]
        public ulong ExcludePartFlags { get { return excludePartFlags; } set { if (!value.Equals(excludePartFlags)) excludePartFlags = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(10)]
        public uint ExcludeModifierRegionFlags { get { return excludeModifierRegionFlags; } set { if (!value.Equals(excludeModifierRegionFlags)) excludeModifierRegionFlags = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(11)]
        public FlagList CASFlagList { get { return flagList; } set { if (!value.Equals(flagList)) flagList = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(12)]
        public uint SimlolencePrice { get { return simlolencePrice; } set { if (!value.Equals(simlolencePrice)) SimlolencePrice = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(13)]
        public uint PartTitleKey { get { return partTitleKey; } set { if (!value.Equals(partTitleKey)) partTitleKey = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(14)]
        public uint PartDesptionKey { get { return partDesptionKey; } set { if (!value.Equals(partDesptionKey)) partDesptionKey = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(15)]
        public byte UniqueTextureSpace { get { return uniqueTextureSpace; } set { if (!value.Equals(uniqueTextureSpace)) uniqueTextureSpace = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(16)]
        public int BodyType { get { return bodyType; } set { if (!value.Equals(bodyType)) bodyType = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(17)]
        public int Unused1 { get { return unused1; } set { if (!value.Equals(unused1)) unused1 = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(18)]
        public uint AgeGender { get { return ageGender; } set { if (!value.Equals(ageGender)) ageGender = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(19)]
        public byte Unused2 { get { return unused2; } set { if (!value.Equals(unused2)) unused2 = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(20)]
        public byte Unused3 { get { return unused3; } set { if (!value.Equals(unused3)) unused3 = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(21)]
        public SwatchColorList SwatchColorCode { get { return swatchColorCode; } set { if (!swatchColorCode.Equals(value)) swatchColorCode = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(22), TGIBlockListContentField("TGIList")]
        public byte BuffResKey { get { return buffResKey; } set { if (!value.Equals(buffResKey)) buffResKey = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(23), TGIBlockListContentField("TGIList")]
        public byte VarientThumbnailKey { get { return varientThumbnailKey; } set { if (!value.Equals(varientThumbnailKey)) varientThumbnailKey = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(24), TGIBlockListContentField("TGIList")]
        public byte NakedKey { get { return nakedKey; } set { if (!value.Equals(nakedKey)) nakedKey = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(25), TGIBlockListContentField("TGIList")]
        public byte ParentKey { get { return parentKey; } set { if (!value.Equals(parentKey)) parentKey = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(26)]
        public int SortLayer { get { return sortLayer; } set { if (!value.Equals(sortLayer)) sortLayer = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(27)]
        public LODBlockList LodBlockList { get { return lodBlockList; } set { if (!lodBlockList.Equals(value)) lodBlockList = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(28), TGIBlockListContentField("TGIList")]
        public SimpleList<byte> SlotKey { get { return slotKey; } set { if (!value.Equals(slotKey)) slotKey = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(29), TGIBlockListContentField("TGIList")]
        public byte DifussShadowKey { get { return difussShadowKey; } set { if (!value.Equals(difussShadowKey)) difussShadowKey = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(30), TGIBlockListContentField("TGIList")]
        public byte ShadowKey { get { return shadowKey; } set { if (!value.Equals(shadowKey)) shadowKey = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(31), TGIBlockListContentField("TGIList")]
        public byte CompositionMethod { get { return compositionMethod; } set { if (!value.Equals(compositionMethod)) compositionMethod = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(32), TGIBlockListContentField("TGIList")]
        public byte RegionMapKey { get { return regionMapKey; } set { if (!value.Equals(regionMapKey)) regionMapKey = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(33), TGIBlockListContentField("TGIList")]
        public byte Overrides { get { return overrides; } set { if (!value.Equals(overrides)) overrides = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(34), TGIBlockListContentField("TGIList")]
        public byte NormalMapKey { get { return normalMapKey; } set { if (!value.Equals(normalMapKey)) normalMapKey = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(35), TGIBlockListContentField("TGIList")]
        public byte SpecularMapKey { get { return specularMapKey; } set { if (!value.Equals(specularMapKey)) specularMapKey = value; OnResourceChanged(this, EventArgs.Empty); } }
        [ElementPriority(36)]
        public CountedTGIBlockList TGIList { get { return tgiList; } set { if (!value.Equals(tgiList)) { OnResourceChanged(this, EventArgs.Empty); this.tgiList = value; } } }
        public String Value { get { return ValueBuilder; } }
        #endregion

        #region Sub-Class
        public class LODInfoEntry : AHandlerElement, IEquatable<LODInfoEntry>
        {
            const int recommendedApiVersion = 1;

            private CountedTGIBlockList tgiList;

            byte level;
            UInt32 unused;
            LODAssetList lodAssetList;
            ByteIndexList lodKeyList;


            public LODInfoEntry(int APIversion, EventHandler handler, CountedTGIBlockList tgiList) : base(APIversion, handler) { this.tgiList = tgiList; }
            public LODInfoEntry(int APIversion, EventHandler handler, Stream s, CountedTGIBlockList tgiList) : base(APIversion, handler) { this.tgiList = tgiList; Parse(s); }
            public void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                this.level = r.ReadByte();

                this.unused = r.ReadUInt32();

                lodAssetList = new LODAssetList(null, s);

                byte[] byteList = new byte[r.ReadByte()];
                for (int i = 0; i < byteList.Length; i++)
                    byteList[i] = r.ReadByte();
                lodKeyList = new ByteIndexList(handler, byteList, tgiList);


            }

            public void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(this.level);
                w.Write(this.unused);

                this.lodAssetList.UnParse(s);

                w.Write((byte)lodKeyList.Count);
                foreach (byte b in lodKeyList)
                    w.Write(b);
            }

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region Content Fields
            [ElementPriority(0)]
            public byte Level { get { return level; } set { if (!value.Equals(level)) { level = value; OnElementChanged(); } } }
            public uint Unused { get { return unused; } set { if (!value.Equals(this.unused)) { this.unused = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public LODAssetList LodAssetList { get { return this.lodAssetList; } set { if (!value.Equals(this.lodAssetList)) { this.lodAssetList = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public ByteIndexList LODKeyList { get { return lodKeyList; } set { if (!value.Equals(lodKeyList)) value = lodKeyList; OnElementChanged(); } }
            public string Value { get { return ValueBuilder; } }

            #endregion

            #region IEquatable
            public bool Equals(LODInfoEntry other)
            {
                return this.level == other.level && this.unused == other.unused && this.lodKeyList.Equals(other.lodKeyList);
            }
            #endregion

            #region Sub-class
            public class LodAssets : AHandlerElement, IEquatable<LodAssets>
            {
                #region Attribute
                const int recommendedApiVersion = 1;

                int sorting;
                int specLevel;
                int castShadow;

                #endregion

                #region Constructor
                public LodAssets(int APIversion, EventHandler handler) : base(APIversion, handler) { }
                public LodAssets(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }

                #endregion

                #region AHandlerElement Members
                public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
                public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
                #endregion

                #region Data I/O
                protected void Parse(Stream s)
                {
                    BinaryReader r = new BinaryReader(s);
                    this.sorting = r.ReadInt32();
                    this.specLevel = r.ReadInt32();
                    this.castShadow = r.ReadInt32();
                }

                public void UnParse(Stream s)
                {
                    BinaryWriter w = new BinaryWriter(s);
                    w.Write(this.sorting);
                    w.Write(this.specLevel);
                    w.Write(this.castShadow);
                }
                #endregion

                #region IEquatable
                public bool Equals(LodAssets other)
                {
                    return this.sorting == other.sorting && this.specLevel == other.specLevel && this.castShadow == other.castShadow;
                }
                #endregion


                #region Content Fields
                public int Sorting { get { return this.sorting; } set { if (!value.Equals(this.sorting)) { OnElementChanged(); this.sorting = value; } } }
                public int SpecLevel { get { return this.specLevel; } set { if (!value.Equals(this.specLevel)) { OnElementChanged(); this.specLevel = value; } } }
                public int CastShadow { get { return this.castShadow; } set { if (!value.Equals(this.castShadow)) { OnElementChanged(); this.castShadow = value; } } }

                public string Value { get { return ValueBuilder; } }
                #endregion
            }

            public class LODAssetList : DependentList<LodAssets>
            {
                public LODAssetList(EventHandler handler) : base(handler) { }
                public LODAssetList(EventHandler handler, Stream s) : base(handler) { Parse(s); }

                #region Data I/O
                protected override void Parse(Stream s)
                {
                    BinaryReader r = new BinaryReader(s);
                    byte count = r.ReadByte();
                    for (int i = 0; i < count; i++)
                        base.Add(new LodAssets(1, handler, s));
                }

                public override void UnParse(Stream s)
                {
                    BinaryWriter w = new BinaryWriter(s);
                    w.Write((byte)base.Count);
                    foreach (var asset in this)
                        asset.UnParse(s);
                }

                protected override LodAssets CreateElement(Stream s) { return new LodAssets(1, handler, s); }
                protected override void WriteElement(Stream s, LodAssets element) { element.UnParse(s); }
                #endregion
            }
            #endregion
        }

        public class LODBlockList : DependentList<LODInfoEntry>
        {
            #region Attributes
            CountedTGIBlockList tgiList;
            #endregion

            #region Constructors
            public LODBlockList(EventHandler handler, CountedTGIBlockList tgiList) : base(handler) { this.tgiList = tgiList; }
            public LODBlockList(EventHandler handler, Stream s, CountedTGIBlockList tgiList) : base(handler) { this.tgiList = tgiList; Parse(s); }
            #endregion


            #region Data I/O
            protected override void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                byte count = r.ReadByte();
                for (int i = 0; i < count; i++)
                {
                    base.Add(new LODInfoEntry(1, handler, s, tgiList));
                }
            }

            public override void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write((byte)base.Count);
                foreach(var unknownClass in this)
                {
                    unknownClass.UnParse(s);
                }
            }

            protected override LODInfoEntry CreateElement(Stream s) { return new LODInfoEntry(1, handler, tgiList); }
            protected override void WriteElement(Stream s, LODInfoEntry element) { element.UnParse(s); }
            #endregion
            
        }


        public class SwatchColor : AHandlerElement, IEquatable<SwatchColor>
        {
            private Color color;
            public SwatchColor(int APIversion, EventHandler handler, Stream s) :base(APIversion, handler)
            {
                BinaryReader r = new BinaryReader(s);
                this.color = Color.FromArgb(r.ReadInt32());
            }
            public SwatchColor(int APIversion, EventHandler handler, Color color) : base(APIversion, handler) { this.color = color; }
            public void UnParse(Stream s) { BinaryWriter w = new BinaryWriter(s); w.Write(this.color.ToArgb()); }

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            public bool Equals(SwatchColor other) { return other.Equals(this.color); }

            public Color Color { get { return this.color; } set { if (!color.Equals(value)) { this.color = value; OnElementChanged(); } } }
            public string Value { get { { return this.color.IsKnownColor? this.color.ToKnownColor().ToString(): this.color.Name; } } }
        }

        public class SwatchColorList : DependentList<SwatchColor>
        {
            public SwatchColorList(EventHandler handler) : base(handler) { }
            public SwatchColorList(EventHandler handler, Stream s) : base(handler) { Parse(s); }

            #region Data I/O
            protected override void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                byte count = r.ReadByte();
                for (int i = 0; i < count; i++)
                    base.Add(new SwatchColor(1, handler, s));
            }

            public override void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write((byte)base.Count);
                foreach (var color in this)
                    color.UnParse(s);
            }

            protected override SwatchColor CreateElement(Stream s) { return new SwatchColor(1, handler, Color.Black); }
            protected override void WriteElement(Stream s, SwatchColor element) { element.UnParse(s); }
            #endregion
        }

        public class Flag : AHandlerElement, IEquatable<Flag>
        {
            CASPFlags flagCatagory;
            ushort flagValue;

            public Flag(int APIversion, EventHandler handler, Stream s) :base(APIversion, handler)
            {
                BinaryReader r = new BinaryReader(s);
                this.flagCatagory = (CASPFlags)r.ReadUInt16();
                this.flagValue = r.ReadUInt16();
            }

            public void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write((ushort)this.flagCatagory);
                w.Write(this.flagValue);
            }

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            public bool Equals(Flag other)
            {
                return this.flagValue == other.flagValue && this.flagCatagory == other.flagCatagory;
            }

            [ElementPriority(0)]
            public CASPFlags FlagCatagory { get { return this.flagCatagory; } set { if (value != this.flagCatagory) { OnElementChanged(); this.flagCatagory = value; } } }
            [ElementPriority(1)]
            public ushort FlagValue { get { return this.flagValue; } set { if (value != this.flagValue) { OnElementChanged(); this.flagValue = value; } } }

            public string Value { get { return ValueBuilder; } }
        }

        public class FlagList : DependentList<Flag> 
        {
            public FlagList(EventHandler handler, Stream s) : base(handler, s) { }

            #region Data I/O
            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                uint count = r.ReadUInt32();
                for (int i = 0; i < count; i++)
                    base.Add(new Flag(recommendedApiVersion, handler, s));
            }

            public void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write((uint)base.Count);
                foreach (var flag in this)
                    flag.UnParse(s);
            }
            #endregion

            protected override Flag CreateElement(Stream s) { return new Flag(recommendedApiVersion, null, s); }
            protected override void WriteElement(Stream s, Flag element) { element.UnParse(s); }
        }

        #region Flags
        public enum CASPFlags : ushort
        {
            Mood = 0x0040,
            Color = 0x0041,
            Style = 0x0042,
            Theme = 0x0043,
            AgeAppropriate = 0x0044,
            Archetype = 0x0045,
            OutfitCategory = 0x0046,
            Skill = 0x0047,
            EyeColor = 0x0048,
            Persona = 0x0049,
            Special = 0x004A,
            HairColor = 0x004B,
            ColorPalette = 0x004C,
            Hair = 0x004D,
            FacialHair = 0x004E,
            Hat = 0x004F,
            FaceMakeup = 0x0050,
            Top = 0x0051,
            Bottom = 0x0052,
            Body = 0x0053,
            Shoes = 0x0054,
            BottomAccessory = 0x0055,
            BuyCatEE = 0x0056,
            BuyCatPA = 0x0057,
            BuyCatLD = 0x0058,
            BuyCatSS = 0x0059,
            BuyCatVO = 0x005A,
            Uniform = 0x005B,
            Accessories = 0x005C,
            BuyCatMAG = 0x005D,
            FloorPattern = 0x005E,
            WallPattern = 0x005F,
            Fabric = 0x0060,
            Build = 0x0061,
            Pattern = 0x0062,
            HairLength = 0x0063,
            HairTexture = 0x0064,
            TraitGroup = 0x0065,
            SkinHue = 0x0066,
            Reward = 0x0067,
            TerrainPaint = 0x0068,
            EyebrowThickness = 0x0069,
            EyebrowShape = 0x006A,
        }

        [Flags]
        public enum PramFlag : byte
        {
            ShowInCASDemo = 1 << 5,
            ShowInSimInfoDemo = 1 << 4,
            ShowInUI = 1 << 3,
            AllowForRandom = 1 << 2,
            DefaultThumbnailPart = 1 << 1,
            DefaultForBodyType = 1
        }
        #endregion


        #endregion
    }

    public class CASPartResourceTS4Handler : AResourceHandler
    {
        public CASPartResourceTS4Handler()
        {
            if(s4pi.Settings.Settings.IsTS4)
                this.Add(typeof(CASPartResourceTS4), new List<string>(new string[] { "0x034AEECB", }));
        }
    }

}
