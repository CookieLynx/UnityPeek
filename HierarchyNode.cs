namespace UnityPeek
{
    using System.Collections.Generic;

	public class HierarchyNode
	{
		public HierarchyNode()
		{
			this.Name = string.Empty;
			this.Children = new List<HierarchyNode>();
			this.Id = 0;
		}

		public string Name { get; set; }

		public List<HierarchyNode> Children { get; set; } = [];

		public int Id { get; set; }
	}
}