﻿//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.Cosmos.Tests
{
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CosmosThroughputTests
    {
        [TestMethod]
        public async Task AutoscaleThroughputSerializationTest()
        {
            ThroughputProperties autoscaleThroughputProperties = ThroughputProperties.CreateAutoScaleProvionedThroughput(1000);
            Assert.AreEqual(1000, autoscaleThroughputProperties.MaxThroughput);
            Assert.IsNull(autoscaleThroughputProperties.Throughput);

            using (Stream stream = MockCosmosUtil.Serializer.ToStream<ThroughputProperties>(autoscaleThroughputProperties))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string output = await reader.ReadToEndAsync();
                    Assert.IsNotNull(output);
                    Assert.AreEqual("{\"content\":{\"offerAutopilotSettings\":{\"maxThroughput\":1000}},\"offerVersion\":\"V2\"}", output);
                }
            }

            OfferAutoscaleProperties autoscaleProperties = new OfferAutoscaleProperties(1000);
            using (Stream stream = MockCosmosUtil.Serializer.ToStream<OfferAutoscaleProperties>(autoscaleProperties))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string output = await reader.ReadToEndAsync();
                    Assert.IsNotNull(output);
                    Assert.AreEqual("{\"maxThroughput\":1000}", output);
                }
            }

            using (Stream stream = MockCosmosUtil.Serializer.ToStream<OfferAutoscaleProperties>(autoscaleProperties))
            {
                OfferAutoscaleProperties fromStream = MockCosmosUtil.Serializer.FromStream<OfferAutoscaleProperties>(stream);
                Assert.IsNotNull(fromStream);
                Assert.AreEqual(1000, fromStream.MaxThroughput);
            }

            OfferContentProperties content = OfferContentProperties.CreateAutoscaleOfferConent(1000);
            using (Stream stream = MockCosmosUtil.Serializer.ToStream<OfferContentProperties>(content))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string output = await reader.ReadToEndAsync();
                    Assert.IsNotNull(output);
                    Assert.AreEqual("{\"offerAutopilotSettings\":{\"maxThroughput\":1000}}", output);
                }
            }

            using (Stream stream = MockCosmosUtil.Serializer.ToStream<OfferContentProperties>(content))
            {
                OfferContentProperties fromStream = MockCosmosUtil.Serializer.FromStream<OfferContentProperties>(stream);
                Assert.IsNotNull(fromStream.OfferAutopilotSettings);
                Assert.AreEqual(1000, fromStream.OfferAutopilotSettings.MaxThroughput);
                Assert.IsNull(fromStream.OfferThroughput);
            }

            using (Stream stream = MockCosmosUtil.Serializer.ToStream<ThroughputProperties>(autoscaleThroughputProperties))
            {
                ThroughputProperties fromStream = MockCosmosUtil.Serializer.FromStream<ThroughputProperties>(stream);
                Assert.AreEqual(1000, fromStream.MaxThroughput);
                Assert.IsNull(fromStream.Throughput); ;
            }
        }
    }
}