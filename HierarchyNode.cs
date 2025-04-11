using System.Collections.Generic;

namespace UnityPeek
{
	public class HierarchyNode
	{
		public string Name { get; set; }
		public List<HierarchyNode> Children { get; set; } = new();

		public int Id { get; set; }
	}
}