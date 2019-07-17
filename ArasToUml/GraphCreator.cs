using System;
using Aras.IOM;

namespace ArasToUml
{
    internal class GraphCreator
    {
        private ArasExport ArasExport { get; }

        public GraphCreator(ArasExport arasExport)
        {
            ArasExport = arasExport;
        }

        public void CreateGraph()
        {
            Console.WriteLine("Creating graph...");

            AddItemTypes();
            AddRelationshipItemTypes();
            AddPolyItemTypes();

            Console.WriteLine("Graph successfully created!");
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