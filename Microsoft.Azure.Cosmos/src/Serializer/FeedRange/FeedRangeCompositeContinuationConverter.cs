﻿// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

namespace Microsoft.Azure.Cosmos
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.Cosmos.Query.Core.ContinuationTokens;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal sealed class FeedRangeCompositeContinuationConverter : JsonConverter
    {
        private const string TypePropertyName = "T";
        private const string VersionPropertyName = "V";
        private const string RidPropertyName = "Rid";
        private const string ContinuationPropertyName = "Continuation";

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(FeedRangeCompositeContinuation);
        }

        public override object ReadJson(
           JsonReader reader,
           Type objectType,
           object existingValue,
           JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new JsonReaderException();
            }

            JObject jObject = JObject.Load(reader);

            if (!jObject.TryGetValue(FeedRangeCompositeContinuationConverter.TypePropertyName, out JToken typeJtoken)
                || !Enum.TryParse(typeJtoken.Value<int>().ToString(), ignoreCase: true, out FeedRangeContinuationType tokenType)
                || !FeedRangeContinuationType.Composite.Equals(tokenType))
            {
                throw new JsonReaderException();
            }

            if (!jObject.TryGetValue(FeedRangeCompositeContinuationConverter.RidPropertyName, out JToken ridJToken)
                || string.IsNullOrEmpty(ridJToken.Value<string>()))
            {
                throw new JsonReaderException();
            }

            if (!jObject.TryGetValue(FeedRangeCompositeContinuationConverter.ContinuationPropertyName, out JToken continuationJToken))
            {
                throw new JsonReaderException();
            }

            List<CompositeContinuationToken> ranges = serializer.Deserialize<List<CompositeContinuationToken>>(continuationJToken.CreateReader());
            if (!FeedRangeInternal.TryParse(jObject, serializer, out FeedRangeInternal feedRangeInternal))
            {
                throw new JsonReaderException();
            }

            return new FeedRangeCompositeContinuation(
                containerRid: ridJToken.Value<string>(),
                feedRange: feedRangeInternal,
                deserializedTokens: ranges);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is FeedRangeCompositeContinuation feedRangeCompositeContinuation)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(FeedRangeCompositeContinuationConverter.TypePropertyName);
                writer.WriteValue(FeedRangeContinuationType.Composite);
                writer.WritePropertyName(FeedRangeCompositeContinuationConverter.VersionPropertyName);
                writer.WriteValue(FeedRangeContinuationVersion.V1);
                writer.WritePropertyName(FeedRangeCompositeContinuationConverter.RidPropertyName);
                writer.WriteValue(feedRangeCompositeContinuation.ContainerRid);
                writer.WritePropertyName(FeedRangeCompositeContinuationConverter.ContinuationPropertyName);
                serializer.Serialize(writer, feedRangeCompositeContinuation.CompositeContinuationTokens.ToArray());
                serializer.Serialize(writer, feedRangeCompositeContinuation.FeedRange);
                writer.WriteEndObject();
                return;
            }

            throw new JsonSerializationException(ClientResources.FeedToken_UnrecognizedFeedToken);
        }
    }
}