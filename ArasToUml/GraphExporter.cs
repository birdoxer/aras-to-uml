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
            AppendAttributes(_dotGraph, true);
        }

        private void AppendDotElements()
        {
            foreach (var graphElement in _dotGraph.GraphElements)
                switch (graphElement)
                {
                    case DotArrow dotArrow:
                        _fileContentBuilder.Append($"\t{dotArrow.Source.Name} -> {dotArrow.Target.Name}");
                        if (string.IsNullOrEmpty(dotArrow.CustomStyle))
                        {
                            _fileContentBuilder.AppendLine();
                            break;
                        }
                        _fileContentBuilder.AppendLine(" [");
                        _fileContentBuilder.AppendLine($"\t\t{dotArrow.CustomStyle}");
                        _fileContentBuilder.AppendLine("\t]");
                        break;
                    case DotClass dotClass:
                        _fileContentBuilder.AppendLine($"\t{dotClass.Name} [");
                        _fileContentBuilder.AppendLine($"\t\tlabel = \"{{{dotClass.Label}}}\"");
                        AppendAttributes(dotClass);
                        _fileContentBuilder.AppendLine("\t]");
                        break;
                    case DotEdge dotEdge:
                        _fileContentBuilder.AppendLine("\tedge [");
                        AppendAttributes(dotEdge);
                        _fileContentBuilder.AppendLine("\t]");
                        break;
                    case DotNode dotNode:
                        _fileContentBuilder.AppendLine("\tnode [");
                        AppendAttributes(dotNode);
                        _fileContentBuilder.AppendLine("\t]");
                        break;
                }
        }

        private void AppendAttributes(DotElementWithAttributes dotElement, bool useSingleIndent = false)
        {
            string indent = useSingleIndent ? "\t" : "\t\t";
            foreach (var kvp in dotElement.GetAttributes())
                _fileContentBuilder.AppendLine($"{indent}{kvp.Key} = \"{kvp.Value}\"");
        }
    }
}