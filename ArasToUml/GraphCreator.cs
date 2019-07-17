using System;
using System.Text;
using Aras.IOM;

namespace ArasToUml
{
    internal class GraphCreator
    {
        public GraphCreator(ArasExport arasExport)
        {
            ArasExport = arasExport;
        }

        private ArasExport ArasExport { get; }
        private DotGraph Graph { get; set; }

        public void CreateGraph()
        {
            Console.WriteLine("Creating graph...");

            InitialiseGraph();
            MapItemTypes();
            MapRelationshipItemTypes();
            MapPolyItemTypes();

            Console.WriteLine("Graph successfully created!");
        }

        public void ExportGraph(string filePath)
        {
            Console.WriteLine($"Saving .dot file to {filePath}...");

            GraphExporter graphExporter = new GraphExporter(Graph);
            graphExporter.Export(filePath);

            Console.WriteLine($".dot file successfully saved!");
        }

        private void InitialiseGraph()
        {
            Graph = new DotGraph {IsDiGraph = true};
            Graph.AddAttribute("fontname", "Calibri");
            Graph.AddAttribute("fontsize", "10");

            DotNode baseNode = new DotNode();
            baseNode.AddAttribute("shape", "record");
            baseNode.AddAttribute("fontname", "Calibri");
            baseNode.AddAttribute("fontsize", "10");
            Graph.GraphElements.Add(baseNode);

            DotEdge baseEdge = new DotEdge();
            baseEdge.AddAttribute("shape", "record");
            baseEdge.AddAttribute("fontname", "Calibri");
            baseEdge.AddAttribute("fontsize", "10");
            Graph.GraphElements.Add(baseEdge);
        }

        private void MapItemTypes()
        {
            //TODO: Filter out Aras default properties
            //TODO: Deal with properties referencing other ItemTypes; they have to be modelled as relations
            Console.WriteLine("Adding ItemTypes to graph...");

            int itemTypeCount = ArasExport.ItemTypes.getItemCount();
            for (int i = 0; i < itemTypeCount; i++)
            {
                Item currentItemType = ArasExport.ItemTypes.getItemByIndex(i);
                DotClass itemTypeClass = new DotClass()
                {
                    Name = currentItemType.getProperty("name", $"\"{currentItemType.getID()}\""),
                    Label = GenerateLabelFromProperties(currentItemType)
                };
                Graph.GraphElements.Add(itemTypeClass);
            }

            Console.WriteLine("ItemTypes successfully added!");
        }

        private void MapRelationshipItemTypes()
        {
        }

        private void MapPolyItemTypes()
        {
        }

        private static string GenerateLabelFromProperties(Item itemType)
        {
            StringBuilder labelBuilder = new StringBuilder($"{itemType.getProperty("name", itemType.getID())}|");

            Item propRels = itemType.getRelationships("Property");
            int propRelCount = propRels.getItemCount();
            for (int i = 0; i < propRelCount; i++)
            {
                Item currentProp = propRels.getItemByIndex(i);
                string propName = currentProp.getProperty("name", currentProp.getID());
                string propDataSource = currentProp.getPropertyAttribute("data_source", "name");
                string propDataType = string.IsNullOrEmpty(propDataSource)
                    ? currentProp.getProperty("data_type", "noDataTypeFound")
                    : propDataSource;
                labelBuilder.Append($"{propName} : {propDataType}\\l");
            }

            return labelBuilder.ToString();
        }
    }
}