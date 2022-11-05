using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace WorldWar.Abstractions
{
	public interface IWorldWarIdentityUser
	{
		public Guid GuidId { get; }

		public string UserName { get; }

		[Required][DisplayName("Longitude")] 
		public double Longitude { get; set; }

		[Required][DisplayName("Latitude")]
		public double Latitude { get; set; }
	}
}
