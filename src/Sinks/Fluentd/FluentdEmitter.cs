﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MsgPack;
using MsgPack.Serialization;

namespace Nlog.Targets.Fluentd.Core.Sinks.Fluentd
{
    internal class FluentdEmitter
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly Packer _packer;
        private readonly SerializationContext _serializationContext;

        public FluentdEmitter(Stream stream)
        {
            _serializationContext =InstantiateSerializationContext();
            
            _serializationContext.Serializers.Register(new OrdinaryDictionarySerializer());
            _packer = Packer.Create(stream);
        }

        public void Emit(DateTime timestamp, string tag, IDictionary<string, object> data)
        {
            var unixTimestamp = timestamp.ToUniversalTime().Subtract(UnixEpoch).Ticks / 10000000;
            _packer.PackArrayHeader(3);
            _packer.PackString(tag, Encoding.UTF8);
            _packer.Pack((ulong)unixTimestamp);
            _packer.Pack(data, _serializationContext);
        }
        private static SerializationContext InstantiateSerializationContext()
        {
            var serializationContext = new SerializationContext(PackerCompatibilityOptions.PackBinaryAsRaw)
            {
                DefaultDateTimeConversionMethod = DateTimeConversionMethod.Native,
                SerializationMethod = SerializationMethod.Map
            };

            return serializationContext;
        }
    }
}

