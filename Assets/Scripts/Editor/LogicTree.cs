using System.Collections.Generic;
using System.Linq;

namespace Editor
{
    public class LogicTree
    {
        private readonly Dictionary<string,ILogicNode> _nodes = new Dictionary<string,ILogicNode>();
        public Dictionary<string,ILogicNode> Nodes
        {
            get { return _nodes; }
        }

        private readonly RootNode _root;
        public RootNode Root
        {
            get { return _root; }
        }

        public LogicTree()
        {
            _root = new RootNode();
			
            CreateNode(Root, "Other 1");
            CreateNode(Root, "Other 2");
            CreateNode(Root, "Other 3");
            CreateNode(Root, "Other 4");
            CreateNode(Root, "Other 5");
			
            var everything = CreateNode(Root, "Everything");
            var continent1 = CreateNode(everything, "Continent 1");
            CreateNode(everything, "Continent 2");
            CreateNode(everything, "Continent 3");

            var mine1 = CreateNode(continent1, "Mine 1");
            var mine2 = CreateNode(continent1, "Mine 2");
			
            var corridors =  CreateNode(mine1, "Corridors");
            var ground =  CreateNode(mine1, "Ground");
            var elevator =  CreateNode(mine1, "Elevator");
			
            CreateNode(corridors, "Speed");
            CreateNode(corridors, "Capacity");

        }


        private LogicNode CreateNode(ILogicNode parent, string name)
        {
            var node = new LogicNode(parent, name);
            _nodes.Add(name,node);
            return node;
        }

        private List<ILogicNode> GetChildren(ILogicNode parent)
        {
            return _nodes.Select(kvp => kvp.Value).Where(node => (node.Parent == parent)).ToList();
        }

        public int GetLeftSiblingCount(ILogicNode node)
        {
            return GetLeftSiblings(node).Count;
        }
		
        public int LeftSiblingsCombinedGradChildrenCount(ILogicNode node)
        {
            var leftSiblings = GetLeftSiblings(node);
            var sum = 0;
			
            foreach (var leftSibling in leftSiblings)
            {
                sum += CombinedGrandChildrenCount(leftSibling);
            }

            return sum;
        }

        private List<ILogicNode> GetLeftSiblings(ILogicNode node)
        {
            var allSiblings = GetAllSiblings(node);

            return allSiblings.Where(sibling => sibling.Name.Last() < node.Name.Last()).ToList();
        }

        private List<ILogicNode> GetAllSiblings(ILogicNode node)
        {
            return GetChildren(node.Parent).Where(child => child != node).ToList();
        }

        private int CombinedGrandChildrenCount(ILogicNode parent)
        {
            var children = GetChildren(parent);
            int count = 0;
            if (!children.Any())
                return 0;

            foreach (var child in children)
            {
                var amount = 0;
                count += GetChildRecursive(child,amount);
            }
			
            return count;
        }

        private int GetChildRecursive(ILogicNode child, int amount)
        {
            var children = GetChildren(child);
            if (!children.Any())
                return 1;
			
            var hasGrandChildren = children.SelectMany(GetChildren).Any();

            if (!hasGrandChildren)
                return amount + children.Count;

            foreach (var grandChild in children)
            {
                amount += GetChildRecursive(grandChild,amount);
            }

            return amount;
        }

        public int GetHierarchyLevel(ILogicNode logicNode)
        {
            var node = logicNode;
            var sum = 0;
            while (node.Parent != null)
            {
                sum++;
                node = node.Parent;
            }

            return sum;
        }
    }
}