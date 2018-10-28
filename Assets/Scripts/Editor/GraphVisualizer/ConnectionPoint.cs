using System;
using UnityEngine;

namespace Editor.GraphVisualizer
{
    public class ConnectionPoint
    {
        public Rect Rect;

        private readonly ConnectionPointType _type;

        public readonly Node Node;

        private readonly GUIStyle _style;

        private readonly Action<ConnectionPoint> _onClickConnectionPoint;

        public ConnectionPoint(Node node, ConnectionPointType type, GUIStyle style, Action<ConnectionPoint> onClickConnectionPoint)
        {
            Node = node;
            _type = type;
            _style = style;
            _onClickConnectionPoint = onClickConnectionPoint;
            Rect = new Rect(0, 0, 10f, 10f);
        }

        public void Draw()
        {
            Rect.x = Node.Rect.x + (Node.Rect.width * 0.5f) - Rect.width * 0.5f;

            switch (_type)
            {
                case ConnectionPointType.In:
                    Rect.y = Node.Rect.y;
                    break;

                case ConnectionPointType.Out:
                    Rect.y = Node.Rect.y + Node.Rect.height -10 ;
                    break;
            }
            
            GUI.Box(Rect,"");

//            if (GUI.Button(Rect, "", _style))
//            {
//                if (_onClickConnectionPoint != null)
//                {
//                    _onClickConnectionPoint(this);
//                }
//            }
        }
    }
}