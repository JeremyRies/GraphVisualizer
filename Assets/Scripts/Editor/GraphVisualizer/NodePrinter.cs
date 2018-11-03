using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Editor.GraphVisualizer
{
    public class NodePrinter
    {
        public void PrintNodes()
        {
            var tree = new LogicTree();

            var stringBuilder = new StringBuilder();
            stringBuilder.Append("digraph G {" + Environment.NewLine);
            
            foreach (var treeNode in tree.Nodes.Values)
            {
                var declaration = string.Format("{0}" + "[shape=record,",treeNode.Name);
                var label = " label=" + "\"";
                var header = string.Format("{{" + "{0}|", treeNode.Name);
                var ownFactor = string.Format("{{" + "Factor:|" + "{0}" + "}}", treeNode.OwnFactor.Value);
                var result = string.Format("|" + "{{" + "Result:|" + "{0}" + "}}",treeNode.CompleteFactor.Value);
                var closingBrackets = "}\"" +"];";

                stringBuilder.Append("\t" + declaration + label + header + ownFactor + result + closingBrackets + Environment.NewLine);
            }

            foreach (var treeNode in tree.Nodes.Values)
            {
                foreach (var child in tree.GetChildren(treeNode))
                {
                    var connection = treeNode.Name + " -> " + child.Name;
                    stringBuilder.Append("\t" +connection + Environment.NewLine);
                }
            }
            
            stringBuilder.Append("}");
            
            WriteText(stringBuilder.ToString());
        }

        private void WriteText(string text)
        {
            string path = "Assets/Resources/test.txt";

            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine(text);
            }

        }

    }
}