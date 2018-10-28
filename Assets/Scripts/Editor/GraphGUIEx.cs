using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;
using Node = UnityEditor.Graphs.Node;

namespace Editor
{
	public class GraphGUIEx : GraphGUI{
	}

	public class InAndOutSlots
	{
		public Slot InPut;
		public Slot OutPut;
	}

	public interface ILogicNode
	{
		ILogicNode Parent { get; }
		string Name { get; }
		IReadOnlyReactiveProperty<double> CompleteFactor { get; }
		ReactiveProperty<double> OwnFactor { get; }
	}

	public class LogicNode : ILogicNode
	{
		private readonly IReadOnlyReactiveProperty<double> _completeFactor;
		private readonly ReactiveProperty<double> _ownFactor = new ReactiveProperty<double>(1);
		
		public ILogicNode Parent { get; private set; }

		public string Name { get; private set; }

		public IReadOnlyReactiveProperty<double> CompleteFactor
		{
			get { return _completeFactor; }
		}

		public ReactiveProperty<double> OwnFactor
		{
			get { return _ownFactor; }
		}

		public LogicNode(ILogicNode parentNode, string name)
		{
			Parent = parentNode;
			Name = name;

			_completeFactor = _ownFactor.CombineLatest(parentNode.CompleteFactor, (own, parent) => own * parent).ToReactiveProperty();
		}
	}

	public class RootNode : ILogicNode
	{
		private readonly ReactiveProperty<double> _completeFactor = new ReactiveProperty<double>(1);
		private readonly ReactiveProperty<double> _ownFactor = new ReactiveProperty<double>(1);
		
		public ILogicNode Parent
		{
			get { return null; }
		}

		public string Name
		{
			get { return "Root"; }
		}

		public IReadOnlyReactiveProperty<double> CompleteFactor
		{
			get { return _completeFactor; }
		}

		public ReactiveProperty<double> OwnFactor
		{
			get { return _ownFactor; }
		}
	}

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

	public class Tree<T>
	{
		private readonly Dictionary<string, T> _nodes = new Dictionary<string, T>();

		public List<T> Nodes
		{
			get { return _nodes.Values.ToList(); }
		}

		public void AddNode(string name, T node)
		{
			_nodes.Add(name,node);
		}

		public T GetNode(string name)
		{
			return _nodes[name];
		}
	}
	
	public class GraphEditorWindow : EditorWindow
	{
		private static EditorWindow _editorWindow;
		
		private static Graph _graph;
		private static GraphGUIEx _graphGui;
		private static LogicTree _tree;

		[MenuItem("Window/Example")]
		public static void Do()
		{
			_editorWindow = GetWindow<GraphEditorWindow> ();
			
			_graph = CreateInstance<Graph>();
			_graph.hideFlags = HideFlags.HideAndDontSave;

			ConnectLogicToVisuals();


			_graphGui = CreateInstance<GraphGUIEx>();
			_graphGui.graph = _graph;
		}

		private static void ConnectLogicToVisuals()
		{
			_tree = new LogicTree();
			var graphTree = new Tree<InAndOutSlots>();


			var root = CreateNode(_tree.Root,0);
			graphTree.AddNode(_tree.Root.Name, root);

			foreach (var kvp in _tree.Nodes)
			{
				var logicNode = kvp.Value;
				var parent = graphTree.GetNode(logicNode.Parent.Name);
				
				var newNode = CreateNode(logicNode,parent.InPut.node.position.x);

				graphTree.AddNode(logicNode.Name, newNode);



				_graph.Connect(newNode.InPut, parent.OutPut);
			}
		}

		private static InAndOutSlots CreateNode(ILogicNode logicNode, float parentXPos)
		{
			Node node = CreateInstance<Node>();
			node.title = logicNode.Name;

			var leftGrandChildrenCombinedCount = _tree.LeftSiblingsCombinedGradChildrenCount(logicNode);
			var leftSiblingCount = _tree.GetLeftSiblingCount(logicNode);
			int hierarchyLevel = _tree.GetHierarchyLevel(logicNode);
			
			node.position = GetNodePosition(logicNode.Name, hierarchyLevel,leftGrandChildrenCombinedCount,leftSiblingCount, parentXPos);
			
			var inAndOut = new InAndOutSlots
			{
				InPut = node.AddInputSlot("input"),
				OutPut = node.AddOutputSlot("output")
			};		
			
			node.AddProperty(new Property(typeof(double), "Complete Factor"));
			node.AddProperty(new Property(typeof(double), "Own Factor"));
			
			logicNode.CompleteFactor.Subscribe(factor => {node.SetPropertyValue("Complete Factor", factor); });
			logicNode.OwnFactor.Subscribe(factor => {node.SetPropertyValue("Own Factor", factor); });
			

			_graph.AddNode(node);
			
			return inAndOut;
		}

		private static Rect GetNodePosition(string name, int hierarchyLevel, int leftGrandChildrenCombinedCount,
			int leftSiblingCount,float parentXPos)
		{
			var xSum = parentXPos;
			var space = 70;
			
//			for (var index = 0; index < bonusNodeName.Length; index++)
//			{
				xSum += space * leftGrandChildrenCombinedCount;
				xSum += space * leftSiblingCount;

				Debug.Log("Name: "  + name + " GrandChildCount " + leftGrandChildrenCombinedCount);
//			}

			var ySum = hierarchyLevel * space;
			
			return new Rect(xSum, ySum, 0, 0);
		}

		private void OnGUI ()
		{

			if (_graph && _graphGui != null) {
				_graphGui.BeginGraphGUI (_editorWindow, new Rect (0, 0, _editorWindow.position.width, _editorWindow.position.height));
				_graphGui.OnGraphGUI ();


				_graphGui.EndGraphGUI ();

			}
		}
	}
}
