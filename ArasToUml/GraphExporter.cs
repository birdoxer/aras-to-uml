using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ArasToUml
{
    public class GraphExporter
    {
        private readonly DotGraph _dotGraph;
        private readonly StringBuilder _fileContentBuilder = new StringBuilder("");

        public GraphExporter(DotGraph dotGraph)
        {
            _dotGraph = dotGraph;
        }

        public void Export(string filePath)
        {
            InitialiseGraph();
            AppendDotElements();
            _fileContentBuilder.Append("}");
            File.WriteAllText(filePath, _fileContentBuilder.ToString());
        }

        private void InitialiseGraph()
        {
            _fileContentBuilder.Append(_dotGraph.IsDiGraph ? "digraph " : "graph");
            _fileContentBuilder.AppendLine($"{_dotGraph.Name} {{");
            AppendAttributes(_dotGraph);
        }

        private void AppendDotElements()
        {
            foreach (DotElement graphElement in _dotGraph.GraphElements)
                switch (graphElement)
                {
                    case DotArrow dotArrow:
                        _fileContentBuilder.AppendLine($"{dotArrow.Source.Name} -> {dotArrow.Target.Name}");
                        break;
                    case DotClass dotClass:
                        _fileContentBuilder.AppendLine($"{dotClass.Name} [");
                        _fileContentBuilder.AppendLine($"label = \"{{{dotClass.Label}}}\"");
                        AppendAttributes(dotClass);
                        _fileContentBuilder.AppendLine("]");
                        break;
                    case DotEdge dotEdge:
                        _fileContentBuilder.AppendLine("edge [");
                        AppendAttributes(dotEdge);
                        _fileContentBuilder.AppendLine("]");
                        break;
                    case DotNode dotNode:
                        _fileContentBuilder.AppendLine("node [");
                        AppendAttributes(dotNode);
                        _fileContentBuilder.AppendLine("]");
                        break;
                }
        }

        private void AppendAttributes(DotElementWithAttributes dotElement)
        {
            foreach (KeyValuePair<string, string> kvp in dotElement.GetAttributes())
                _fileContentBuilder.AppendLine($"{kvp.Key} = \"{kvp.Value}\"");
        }
    }
}