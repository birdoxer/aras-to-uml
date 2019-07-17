using System;
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


            AddItemTypes();
            AddRelationshipItemTypes();
            AddPolyItemTypes();

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
        }

        private void AddItemTypes()
        {
            Console.WriteLine("Adding ItemTypes to graph...");

            int itemTypeCount = ArasExport.ItemTypes.getItemCount();
            for (int i = 0; i < itemTypeCount; i++)
            {
                Item currentItemType = ArasExport.ItemTypes.getItemByIndex(i);
            }

            Console.WriteLine("ItemTypes successfully added!");
        }

        private void AddRelationshipItemTypes()
        {
        }

        private void AddPolyItemTypes()
        {
        }
    }
}