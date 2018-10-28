using System;
using UnityEditor;
using UnityEngine;

namespace Editor.SecondApproach
{
    public class Node
    {
        public Rect Rect;
        public string Title;
        public bool IsDragged;
        public bool IsSelected;

        public ConnectionPoint InPoint;
        public ConnectionPoint OutPoint;

        public GUIStyle Style;
        public GUIStyle DefaultNodeStyle;
        public GUIStyle SelectedNodeStyle;

        public Action<Node> OnRemoveNode;

        public Node(Vector2 position, float width, float height, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<ConnectionPoint> onClickInPoint, Action<ConnectionPoint> onClickOutPoint, Action<Node> onClickRemoveNode)
        {
            Rect = new Rect(position.x, position.y, width, height);
            Style = nodeStyle;
            InPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, onClickInPoint);
            OutPoint = new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, onClickOutPoint);
            DefaultNodeStyle = nodeStyle;
            SelectedNodeStyle = selectedStyle;
            OnRemoveNode = onClickRemoveNode;
        }

        public void Drag(Vector2 delta)
        {
            Rect.position += delta;
        }

        public void Draw()
        {
            InPoint.Draw();
            OutPoint.Draw();
            GUI.Box(Rect, Title, Style);
        }

        public bool ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        if (Rect.Contains(e.mousePosition))
                        {
                            IsDragged = true;
                            GUI.changed = true;
                            IsSelected = true;
                            Style = SelectedNodeStyle;
                        }
                        else
                        {
                            GUI.changed = true;
                            IsSelected = false;
                            Style = DefaultNodeStyle;
                        }
                    }

                    if (e.button == 1 && IsSelected && Rect.Contains(e.mousePosition))
                    {
                        ProcessContextMenu();
                        e.Use();
                    }
                    break;

                case EventType.MouseUp:
                    IsDragged = false;
                    break;

                case EventType.MouseDrag:
                    if (e.button == 0 && IsDragged)
                    {
                        Drag(e.delta);
                        e.Use();
                        return true;
                    }
                    break;
            }

            return false;
        }

        private void ProcessContextMenu()
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
            genericMenu.ShowAsContext();
        }

        private void OnClickRemoveNode()
        {
            if (OnRemoveNode != null)
            {
                OnRemoveNode(this);
            }
        }
    }
}