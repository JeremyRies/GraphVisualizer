using System.Collections.Generic;
using System.Linq;

namespace Editor
{
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
}