﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Metadata
{
    public class StandAloneSignatureTable : MetadataTable<StandAloneSignature>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.StandAloneSig; }
        }

        public override uint GetElementByteCount()
        {
            return (uint)TableStream.BlobIndexSize;
        }

        protected override StandAloneSignature ReadMember(MetadataToken token, ReadingContext context)
        {
            return new StandAloneSignature(Header, token, new MetadataRow<uint>()
            {
                Column1 = context.Reader.ReadIndex(TableStream.BlobIndexSize)
            });
        }

        protected override void UpdateMember(NetBuildingContext context, StandAloneSignature member)
        {
            var buffer = context.GetStreamBuffer<BlobStreamBuffer>();
            member.MetadataRow.Column1 = buffer.GetBlobOffset(member.Signature);
        }

        protected override void WriteMember(WritingContext context, StandAloneSignature member)
        {
            context.Writer.WriteIndex(TableStream.BlobIndexSize, member.MetadataRow.Column1);
        }
    }

    public class StandAloneSignature : MetadataMember<MetadataRow<uint>>, IHasCustomAttribute
    {
        private CustomAttributeCollection _customAttributes;
        private readonly LazyValue<CallingConventionSignature> _signature;

        public StandAloneSignature(CallingConventionSignature signature)
            : base(null, new MetadataToken(MetadataTokenType.StandAloneSig), new MetadataRow<uint>())
        {
            _signature = new LazyValue<CallingConventionSignature>(signature);
        }

        internal StandAloneSignature(MetadataHeader header, MetadataToken token, MetadataRow<uint> row)
            : base(header, token, row)
        {

            _signature = new LazyValue<CallingConventionSignature>(() =>
            {
                IBinaryStreamReader reader;
                return header.GetStream<BlobStream>().TryCreateBlobReader(row.Column1, out reader)
                    ? CallingConventionSignature.FromReader(header, reader)
                    : null;
            });
        }

        public CallingConventionSignature Signature
        {
            get { return _signature.Value; }
            set { _signature.Value = value; }
        }

        public CustomAttributeCollection CustomAttributes
        {
            get
            {
                if (_customAttributes != null)
                    return _customAttributes;
                return _customAttributes = new CustomAttributeCollection(this);
            }
        }
    }
}
