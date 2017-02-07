﻿using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CVARC.V2;

namespace HoMM.Sensors
{
    [DataContract]
    public class HommSensorData
    {
        [DataMember]
        [FromSensor(typeof(LocationSensor))]
        Location Location { get; set; }

        [DataMember]
        [FromSensor(typeof(MapSensor))]
        IEnumerable<TileObject> Map { get; set; }
    }
}
