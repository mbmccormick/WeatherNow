using System;
using System.Linq;

namespace Weatherman.API.Geocoding
{
    public enum LocationType
    {
        Rooftop,
        RangeInterpolated,
        GeometricCenter,
        Approximate,
        // in case the server returns back something unknown
        Unknown,
    }
}
