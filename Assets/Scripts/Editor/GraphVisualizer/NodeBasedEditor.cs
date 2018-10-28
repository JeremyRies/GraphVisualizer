using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor.GraphVisualizer
{
    public class NodeCreator
    {
        private LogicTree _tree;
        private readonly Action<Node,Node> _connectNodes;
        private readonly Func<Vector2,ILogicNode, Node> _createNode;

        public NodeCreator(Action<Node,Node> connectNodes, Func<Vector2, ILogicNode, Node> createNode)
        {
            _connectNodes = connectNodes;
            _createNode = createNode;
        }

        public List<Node> CreateNodes()
        {
            _tree = new LogicTree();
            var graphTree = new Tree<Node>();


            var root = CreateNode(_tree.Root,0);
            graphTree.AddNode(_tree.Root.Name, root);

            foreach (var kvp in _tree.Nodes)
            {
                var logicNode = kvp.Value;
                var parent = graphTree.GetNode(logicNode.Parent.Name);
				
                var newNode = CreateNode(logicNode,parent.Rect.position.x);

                graphTree.AddNode(logicNode.Name, newNode);


                
                _connectNodes(newNode,parent);
            }

            return graphTree.Nodes;
        }

        private Node CreateNode(ILogicNode logicNode, float parentXPos)
        {
            var leftGrandChildrenCombinedCount = _tree.LeftSiblingsCombinedGradChildrenCount(logicNode);
            var leftSiblingCount = _tree.GetLeftSiblingCount(logicNode);
            int hierarchyLevel = _tree.GetHierarchyLevel(logicNode);
            
            var nodePos = GetNodePosition(hierarchyLevel,leftGrandChildrenCombinedCount,leftSiblingCount, parentXPos);
            
            var node = _createNode(nodePos,logicNode);
            node.Title = logicNode.Name;
            return node;
        }
        
        private static Vector2 GetNodePosition(int hierarchyLevel, int leftGrandChildrenCombinedCount,
            int leftSiblingCount,float parentXPos)
        {
            var xSum = parentXPos;
            var space = 120;
			
            xSum += space * (leftSiblingCount + leftGrandChildrenCombinedCount);
            
            var ySum = hierarchyLevel * space;
			
            return new Vector2(xSum, ySum);
        }
    }
    
    public class NodeBasedEditor : EditorWindow
    {
        private List<Node> _nodes;
        private readonly List<Connection> _connections = new List<Connection>();

        private GUIStyle _nodeStyle;
        private GUIStyle _selectedNodeStyle;
        private GUIStyle _inPointStyle;
        private GUIStyle _outPointStyle;

        private ConnectionPoint _selectedInPoint;
        private ConnectionPoint _selectedOutPoint;

        private Vector2 _offset;
        private Vector2 _drag;

        [MenuItem("Window/Node Based Editor")]
        private static void OpenWindow()
        {
            NodeBasedEditor window = GetWindow<NodeBasedEditor>();
            window.titleContent = new GUIContent("Node Based Editor");
        }

        private void OnEnable()
        {
            var nodeCreator = new NodeCreator(ConnectNodes, CreateNode);
            _nodes = nodeCreator.CreateNodes();
            _nodeStyle = new GUIStyle();
            _nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
            _nodeStyle.border = new RectOffset(12, 12, 12, 12);

            _selectedNodeStyle = new GUIStyle();
            _selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
            _selectedNodeStyle.border = new RectOffset(12, 12, 12, 12);

            _inPointStyle = new GUIStyle();
            _inPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
            _inPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
            _inPointStyle.border = new RectOffset(4, 4, 12, 12);

            _outPointStyle = new GUIStyle();
            _outPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
            _outPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
            _outPointStyle.border = new RectOffset(4, 4, 12, 12);
        }

        private void ConnectNodes(Node arg1, Node arg2)
        {
            _connections.Add(new Connection(arg1.InPoint, arg2.OutPoint, OnClickRemoveConnection));
        }

        private Node CreateNode(Vector2 pos, ILogicNode logicNode)
        {
            return new Node(pos, 100, 100, _nodeStyle, _selectedNodeStyle, _inPointStyle, _outPointStyle,
                OnClickInPoint, OnClickOutPoint, OnClickRemoveNode,logicNode);
        }

        private void OnGUI()
        {
            DrawGrid(20, 0.2f, Color.gray);
            DrawGrid(100, 0.4f, Color.gray);

            DrawNodes();
            DrawConnections();

            DrawConnectionLine(Event.current);

            ProcessNodeEvents(Event.current);
            ProcessEvents(Event.current);

            if (GUI.changed) Repaint();
        }

        private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
        {
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            _offset += _drag * 0.5f;
            Vector3 newOffset = new Vector3(_offset.x % gridSpacing, _offset.y % gridSpacing, 0);

            for (int i = 0; i < widthDivs; i++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
            }

            for (int j = 0; j < heightDivs; j++)
            {
                Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        private void DrawNodes()
        {
            if (_nodes != null)
            {
                for (int i = 0; i < _nodes.Count; i++)
                {
                    _nodes[i].Draw();
                }
            }
        }

        private void DrawConnections()
        {
            if (_connections != null)
            {
                for (int i = 0; i < _connections.Count; i++)
                {
                    _connections[i].Draw();
                } 
            }
        }

        private void ProcessEvents(Event e)
        {
            _drag = Vector2.zero;

            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        ClearConnectionSelection();
                    }

                    if (e.button == 1)
                    {
                        ProcessContextMenu(e.mousePosition);
                    }
                    break;

                case EventType.MouseDrag:
                    if (e.button == 0)
                    {
                        OnDrag(e.delta);
                    }
                    break;
            }
        }

        private void ProcessNodeEvents(Event e)
        {
            if (_nodes != null)
            {
                for (int i = _nodes.Count - 1; i >= 0; i--)
                {
                    bool guiChanged = _nodes[i].ProcessEvents(e);

                    if (guiChanged)
                    {
                        GUI.changed = true;
                    }
                }
            }
        }

        private void DrawConnectionLine(Event e)
        {
            if (_selectedInPoint != null && _selectedOutPoint == null)
            {
                Handles.DrawBezier(
                    _selectedInPoint.Rect.center,
                    e.mousePosition,
                    _selectedInPoint.Rect.center + Vector2.left * 50f,
                    e.mousePosition - Vector2.left * 50f,
                    Color.white,
                    null,
                    2f
                );

                GUI.changed = true;
            }

            if (_selectedOutPoint != null && _selectedInPoint == null)
            {
                Handles.DrawBezier(
                    _selectedOutPoint.Rect.center,
                    e.mousePosition,
                    _selectedOutPoint.Rect.center - Vector2.left * 50f,
                    e.mousePosition + Vector2.left * 50f,
                    Color.white,
                    null,
                    2f
                );

                GUI.changed = true;
            }
        }

        private void ProcessContextMenu(Vector2 mousePosition)
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Add node"), false, () => OnClickAddNode(mousePosition)); 
            genericMenu.ShowAsContext();
        }

        private void OnDrag(Vector2 delta)
        {
            _drag = delta;

            if (_nodes != null)
            {
                for (int i = 0; i < _nodes.Count; i++)
                {
                    _nodes[i].Drag(delta);
                }
            }

            GUI.changed = true;
        }

        private void OnClickAddNode(Vector2 mousePosition)
        {
            if (_nodes == null)
            {
                _nodes = new List<Node>();
            }

            _nodes.Add(new Node(mousePosition, 200, 50, _nodeStyle, _selectedNodeStyle, _inPointStyle, _outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode,new RootNode()));
        }

        private void OnClickInPoint(ConnectionPoint inPoint)
        {
            _selectedInPoint = inPoint;

            if (_selectedOutPoint != null)
            {
                if (_selectedOutPoint.Node != _selectedInPoint.Node)
                {
                    CreateConnection();
                    ClearConnectionSelection(); 
                }
                else
                {
                    ClearConnectionSelection();
                }
            }
        }

        private void OnClickOutPoint(ConnectionPoint outPoint)
        {
            _selectedOutPoint = outPoint;

            if (_selectedInPoint != null)
            {
                if (_selectedOutPoint.Node != _selectedInPoint.Node)
                {
                    CreateConnection();
                    ClearConnectionSelection();
                }
                else
                {
                    ClearConnectionSelection();
                }
            }
        }

        private void OnClickRemoveNode(Node node)
        {
            if (_connections != null)
            {
                List<Connection> connectionsToRemove = new List<Connection>();

                for (int i = 0; i < _connections.Count; i++)
                {
                    if (_connections[i].InPoint == node.InPoint || _connections[i].OutPoint == node.OutPoint)
                    {
                        connectionsToRemove.Add(_connections[i]);
                    }
                }

                for (int i = 0; i < connectionsToRemove.Count; i++)
                {
                    _connections.Remove(connectionsToRemove[i]);
                }
            }

            _nodes.Remove(node);
        }

        private void OnClickRemoveConnection(Connection connection)
        {
            _connections.Remove(connection);
        }

        private void CreateConnection()
        {
            _connections.Add(new Connection(_selectedInPoint, _selectedOutPoint, OnClickRemoveConnection));
        }

        private void ClearConnectionSelection()
        {
            _selectedInPoint = null;
            _selectedOutPoint = null;
        }
    }
}