using Newtonsoft.Json;

namespace Models
{
	public class ComponentManifest
	{
		public string Name { get; set; }
		public string Destination { get; set; }

		[JsonProperty(PropertyName = "AddToCsproj")]
		public bool IsAddToCsprojRequired { get; set; }
	}
}
