using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VaultConfiguration
{

    public class VaultResponse
    {
        [JsonProperty("request_id")]
        public string RequestId { get; set; }

        [JsonProperty("lease_id")]
        public string LeaseId { get; set; }

        [JsonProperty("renewable")]
        public bool Renewable { get; set; }

        [JsonProperty("lease_duration")]
        public int LeasDuration { get; set; }

        [JsonProperty("data")]
        public JObject Data { get; set; }

        [JsonProperty("wrap_info")]
        public object WrapInfo { get; set; }

        [JsonProperty("warnings")]
        public object Warnings { get; set; }

        [JsonProperty("auth")]
        public object Auth { get; set; }
    }
}