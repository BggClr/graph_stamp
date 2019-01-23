using System;
using System.Collections.Generic;

namespace Models
{
	public class Repository
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Url { get; set; }
		public RepositoryOwner Owner { get; set; }
		public IList<string> Topics { get; set; }
	}
}
