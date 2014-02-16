using System;
using System.Linq;

namespace Weatherman.API.Geocoding
{
    public enum GeocodeStatus
    {
        Ok,
        ZeroResults,
        OverQueryLimit,
        RequestDenied,
        InvalidRequest,
        UnknownError,
        Unexpected, // in case the server returns an un-expected value
    }
}
