using System;
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
			_nodes.Add(_root.Name,_root);
            
            CreateNode(Root, "Other1");
            CreateNode(Root, "Other2");
            CreateNode(Root, "Other3");
            CreateNode(Root, "Other4");
            CreateNode(Root, "Other5");
			
            var everything = CreateNode(Root, "Everything");
            var continent1 = CreateNode(everything, "Continent1");
            CreateNode(everything, "Continent2");
            CreateNode(everything, "Continent3");

            var mine1 = CreateNode(continent1, "Mine1");
            var mine2 = CreateNode(continent1, "Mine2");
            var mine3 = CreateNode(continent1, "Mine3");
			
            var corridors =  CreateNode(mine1, "Corridors");
            var ground =  CreateNode(mine1, "Ground");
            var elevator =  CreateNode(mine1, "Elevator");
			
            CreateNode(corridors, "CorSpeed");
            CreateNode(corridors, "CorCapacity");

            CreateNode(ground, "GroundSpeed");
            CreateNode(ground, "GroundCapacity");
            CreateNode(ground, "GroundLoadSpeed");

        }


        private LogicNode CreateNode(ILogicNode parent, string name)
        {
            var node = new LogicNode(parent, name);
            _nodes.Add(name,node);
            return node;
        }

        public List<ILogicNode> GetChildren(ILogicNode parent)
        {
            return _nodes.Select(kvp => kvp.Value).Where(node => (node.Parent == parent)).ToList();
        }

        public int GetLeftSiblingCount(ILogicNode node)
        {
            return GetLeftSiblings(node).Count;
        }
		
        public int LeftSiblingsLeafCount(ILogicNode node)
        {
            var leftSiblings = GetLeftSiblings(node);
            var sum = 0;
			
            foreach (var leftSibling in leftSiblings)
            {
                int amount = 0;
                sum += GetChildRecursive(leftSibling,amount);
            }

            return sum;
        }

        private List<ILogicNode> GetLeftSiblings(ILogicNode node)
        {
            var allSiblings = GetAllSiblings(node);
            return allSiblings.Where(sibling => string.Compare(sibling.Name, node.Name, StringComparison.Ordinal) < 1).ToList();
        }

        private List<ILogicNode> GetAllSiblings(ILogicNode node)
        {
            return GetChildren(node.Parent).Where(child => child != node).ToList();
        }

        private int GetChildRecursive(ILogicNode child, int amount)
        {
            var children = GetChildren(child);
            if (!children.Any())
                return 1;
			
            var hasGrandChildren = children.SelectMany(GetChildren).Any();

            if (!hasGrandChildren)
                return amount + children.Count;

            var leaves = 0;
            foreach (var grandChild in children)
            {
                leaves += GetChildRecursive(grandChild,amount);
            }

            return amount + leaves;
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