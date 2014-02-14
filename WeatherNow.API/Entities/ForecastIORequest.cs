using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using WeatherNow.API.Extensions;
using Newtonsoft.Json;
using RestSharp;
using System.IO;

namespace WeatherNow.API
{
    public class ForecastIORequest
    {
        private readonly string _apiKey;
        private readonly string _latitude;
        private readonly string _longitude;
        private readonly string _unit;
        private readonly string _exclude;
        private readonly string _extend;
        private readonly string _time;
        
        private const string CurrentForecastUrl = "https://api.forecast.io/forecast/{0}/{1},{2}?units={3}&extend={4}&exclude={5}";
        private const string PeriodForecastUrl = "https://api.forecast.io/forecast/{0}/{1},{2},{3}?units={4}&extend={5}&exclude={6}";

        public void Get(Action<ForecastIOResponse> callback)
        {
            var url = (_time == null) ? String.Format(CurrentForecastUrl, _apiKey, _latitude, _longitude, _unit, _extend, _exclude) :
                String.Format(PeriodForecastUrl, _apiKey, _latitude, _longitude, _time, _unit, _extend, _exclude);

            RestClient client = new RestClient();
            RestRequest request = new RestRequest(url, Method.GET);

            client.ExecuteAsync(request, (response) =>
            {
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(response.Content));
                StreamReader sr = new StreamReader(stream);

                JsonTextReader tr = new JsonTextReader(sr);
                ForecastIOResponse data = new JsonSerializer().Deserialize<ForecastIOResponse>(tr);

                callback(data);
            });
        }

        public ForecastIORequest(string apiKey, double lat, double lng, Unit unit, Extend[] extend = null, Exclude[] exclude = null)
        {
            _apiKey = apiKey;
            _latitude = lat.ToString(CultureInfo.InvariantCulture);
            _longitude = lng.ToString(CultureInfo.InvariantCulture);
            _unit = Enum.GetName(typeof(Unit), unit);
            _extend = (extend != null) ? RequestHelpers.FormatExtendString(extend) : "";
            _exclude = (exclude != null) ? RequestHelpers.FormatExcludeString(exclude) : "";
        }

        public ForecastIORequest(string apiKey, double lat, double lng, DateTime time, Unit unit, Extend[] extend = null, Exclude[] exclude = null)
        {
            _apiKey = apiKey;
            _latitude = lat.ToString(CultureInfo.InvariantCulture);
            _longitude = lng.ToString(CultureInfo.InvariantCulture);
            _time = time.ToUTCString();
            _unit = Enum.GetName(typeof(Unit), unit);
            _extend = (extend != null) ? RequestHelpers.FormatExtendString(extend) : "";
            _exclude = (exclude != null) ? RequestHelpers.FormatExcludeString(exclude) : "";
        }
    }
}
