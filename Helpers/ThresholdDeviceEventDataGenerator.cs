﻿#region Copyright
//=======================================================================================
// Microsoft Azure Customer Advisory Team 
//
// This sample is supplemental to the technical guidance published on my personal
// blog at http://blogs.msdn.com/b/paolos/. 
// 
// Author: Paolo Salvatori
//=======================================================================================
// Copyright © 2011 Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
// EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
// MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. YOU BEAR THE RISK OF USING IT.
//=======================================================================================
#endregion

#region Using Directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
#endregion

namespace Microsoft.WindowsAzure.CAT.ServiceBusExplorer
{
    [Serializable]
    public enum MessageFormat
    {
        [XmlEnum("0")]
        Json,
        [XmlEnum("1")]
        Xml
    }

    [Serializable]
    [XmlType(TypeName = "event", Namespace = "http://schemas.microsoft.com/servicebusexplorer")]
    [XmlRoot(ElementName = "event", Namespace = "http://schemas.microsoft.com/servicebusexplorer", IsNullable = false)]
    [DataContract(Name = "event", Namespace = "http://schemas.microsoft.com/servicebusexplorer")]
    public class ThresholdDeviceEvent
    {
        /// <summary>
        /// Gets or sets the device id.
        /// </summary>
        [XmlElement(ElementName = "deviceid", Namespace = "http://schemas.microsoft.com/servicebusexplorer")]
        [JsonProperty(PropertyName = "deviceid", Order = 1)]
        public int DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the device value.
        /// </summary>
        [XmlElement(ElementName = "value", Namespace = "http://schemas.microsoft.com/servicebusexplorer")]
        [JsonProperty(PropertyName = "value", Order = 2)]
        public int Value { get; set; }
    }

    public class ThresholdDeviceEventDataGenerator : IEventDataGenerator, IDisposable
    {
        #region Public Constants
        //***************************
        // Constants
        //***************************
        private const string GeneratorProperties = "Generator Properties";
        private const string CustomProperties = "Custom Properties";
        private const string MinDeviceIdDescription = "Gets or sets the minimum device id.";
        private const string MaxDeviceIdDescription = "Gets or sets the maximum device id.";
        private const string MinValueDescription = "Gets or sets the minimum value.";
        private const string MaxValueDescription = "Gets or sets the maximum value.";
        private const string MessageFormatDescription = "Gets or sets the message format: Json or Xml.";
        private const string CityDescription = "Gets or sets the city.";
        private const string CountryDescription = "Gets or sets the country.";

        //***************************
        // Formats
        //***************************
        private const string ExceptionFormat = "Exception: {0}";
        private const string InnerExceptionFormat = "InnerException: {0}";
        private const string EventDataCreatedFormat = "[ThresholdDeviceEventDataGenerator] {0} objects have been successfully created.";
        #endregion

        #region Public Constructor
        public ThresholdDeviceEventDataGenerator()
        {
            MinDeviceId = 1;
            MaxDeviceId = 100;
            MinValue = 1;
            MaxValue = 100;
            City = "Milan";
            Country = "Italy";
        } 
        #endregion

        #region IEventDataGenerator Methods
        public IEnumerable<EventData> GenerateEventDataCollection(int eventDataCount, WriteToLogDelegate writeToLog)
        {
            if (eventDataCount < 0)
            {
                return null;
            }
            var random = new Random();
            var list = new List<EventData>();
            for (var i = 0; i < eventDataCount; i++)
            {
                try
                {
                    var payload = new ThresholdDeviceEvent
                    {
                        DeviceId = random.Next(MinDeviceId, MaxDeviceId + 1),
                        Value = random.Next(MinDeviceId, MaxDeviceId + 1)
                    };
                    var eventData = new EventData((MessageFormat == MessageFormat.Json
                        ? JsonSerializerHelper.Serialize(payload)
                        : XmlSerializerHelper.Serialize(payload)).ToMemoryStream())
                    {
                        PartitionKey = payload.DeviceId.ToString(CultureInfo.InvariantCulture),

                    };
                    eventData.Properties.Add("deviceId", payload.DeviceId);
                    eventData.Properties.Add("value", payload.Value);
                    eventData.Properties.Add("time", DateTime.UtcNow.Ticks);
                    eventData.Properties.Add("city", City);
                    eventData.Properties.Add("country", Country);
                    list.Add(eventData);
                }
                catch (Exception ex)
                {
                    if (!string.IsNullOrWhiteSpace(ex.Message))
                    {
                        writeToLog(string.Format(CultureInfo.CurrentCulture, ExceptionFormat, ex.Message));
                    }
                    if (ex.InnerException != null && !string.IsNullOrWhiteSpace(ex.InnerException.Message))
                    {
                        writeToLog(string.Format(CultureInfo.CurrentCulture, InnerExceptionFormat, ex.InnerException.Message));
                    }
                }
            }
            if (writeToLog != null)
            {
                writeToLog(string.Format(EventDataCreatedFormat, list.Count));
            }
            return list;
        } 
        #endregion

        #region IDisposable Methods
        public void Dispose()
        {
        }
        #endregion

        #region Public Properties
        [Category(GeneratorProperties)]
        [Description(MinDeviceIdDescription)]
        [DefaultValue(1)]
        public int MinDeviceId { get; set; }

        [Category(GeneratorProperties)]
        [Description(MaxDeviceIdDescription)]
        [DefaultValue(100)]
        public int MaxDeviceId { get; set; }

        [Category(GeneratorProperties)]
        [Description(MinValueDescription)]
        public int MinValue { get; set; }

        [Category(GeneratorProperties)]
        [Description(MaxValueDescription)]
        public int MaxValue { get; set; }

        [Category(GeneratorProperties)]
        [Description(MessageFormatDescription)]
        public MessageFormat MessageFormat { get; set; }

        [Category(CustomProperties)]
        [Description(CityDescription)]
        public string City { get; set; }

        [Category(CustomProperties)]
        [Description(CountryDescription)]
        public string Country { get; set; }
        #endregion
    }
}
