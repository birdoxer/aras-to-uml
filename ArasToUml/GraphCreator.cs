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

        public void CreateGraph(bool relsAsClasses)
        {
            Console.WriteLine("Creating graph...");

            InitialiseGraph();
            MapItemTypes();
            MapRelationshipItemTypes(relsAsClasses);
            MapPolyItemTypes();

            Console.WriteLine("Graph successfully created!");
        }

        public void ExportGraph(string filePath)
        {
            Console.WriteLine($"Saving .dot file to {filePath}...");

            var graphExporter = new GraphExporter(Graph);
            graphExporter.Export(filePath);

            Console.WriteLine(".dot file successfully saved!");
        }

        private void InitialiseGraph()
        {
            Graph = new DotGraph {IsDiGraph = true};
            Graph.AddAttribute("fontname", "Calibri");
            Graph.AddAttribute("fontsize", "10");

            var baseNode = new DotNode();
            baseNode.AddAttribute("shape", "record");
            baseNode.AddAttribute("fontname", "Calibri");
            baseNode.AddAttribute("fontsize", "10");
            Graph.GraphElements.Add(baseNode);

            var baseEdge = new DotEdge();
            baseEdge.AddAttribute("shape", "record");
            baseEdge.AddAttribute("fontname", "Calibri");
            baseEdge.AddAttribute("fontsize", "10");
            baseEdge.AddAttribute("arrowhead", "normal");
            Graph.GraphElements.Add(baseEdge);
        }

        private void MapItemTypes()
        {
            Console.WriteLine("Mapping ItemTypes to graph...");

            int itemTypeCount = ArasExport.ItemTypes.getItemCount();
            for (int i = 0; i < itemTypeCount; i++)
            {
                var currentItemType = ArasExport.ItemTypes.getItemByIndex(i);
                var itemTypeClass = new DotClass
                {
                    Name = currentItemType.getProperty("name", $"\"{currentItemType.getID()}\""),
                    Label = GenerateLabelFromProperties(currentItemType)
                };
                Graph.GraphElements.Add(itemTypeClass);
                MapPropertyRelations(currentItemType);
            }

            Console.WriteLine("ItemTypes successfully mapped!");
        }

        private void MapRelationshipItemTypes(bool relsAsClasses)
        {
            Console.WriteLine("Mapping Relationship ItemTypes to graph...");

            int relTypeCount = ArasExport.RelationshipTypes.getItemCount();


            for (int i = 0; i < relTypeCount; i++)
            {
                var currentItemType = ArasExport.RelationshipTypes.getItemByIndex(i);
                var propRels = currentItemType.getRelationships("Property");

                (string sourceClassName, string targetClassName) = DetermineSourceAndTargetName(propRels);
                if (sourceClassName == "" && targetClassName == "") continue;
                string customStyle = "";
                string currentTypeName = currentItemType.getProperty("name", "");
                if (targetClassName == "")
                {
                    if (currentTypeName == "") continue;
                    MapSpecialTypeAsClass(currentItemType);
                    targetClassName = currentTypeName;
                    customStyle = "dir = \"both\", arrowtail=\"odiamond\"";
                }
                else if (relsAsClasses)
                {
                    MapRelsAsClasses(currentItemType, sourceClassName, targetClassName);
                    continue;
                }
                else
                {
                    customStyle = $"label = \"{currentTypeName}\"";
                }

                var relationshipArrow = new DotArrow(sourceClassName, targetClassName, Graph)
                    {CustomStyle = customStyle};

                Graph.GraphElements.Add(relationshipArrow);
            }


            Console.WriteLine("Relationship ItemTypes successfully mapped!");
        }

        private void MapPolyItemTypes()
        {
            Console.WriteLine("Mapping Poly-ItemTypes to graph...");

            int polyTypeCount = ArasExport.PolyItemTypes.getItemCount();
            for (int i = 0; i < polyTypeCount; i++)
            {
                var currentPolyType = ArasExport.PolyItemTypes.getItemByIndex(i);
                var polyClass = MapSpecialTypeAsClass(currentPolyType);

                var morphaeRels = currentPolyType.getRelationships("Morphae");
                int morphaeCount = morphaeRels.getItemCount();
                for (int j = 0; j < morphaeCount; j++)
                {
                    var currentMorphae = morphaeRels.getItemByIndex(j);
                    var subClassItem = currentMorphae.getPropertyItem("related_id");
                    if (subClassItem == null) continue;
                    var subClass =
                        Graph.GetDotClassByName(subClassItem.getProperty("name", "ClassHasNoName"), false);

                    subClass = subClass ?? MapSpecialTypeAsClass(subClassItem);

                    var relationshipArrow = new DotArrow(subClass, polyClass)
                        {CustomStyle = "arrowhead = \"empty\""};

                    Graph.GraphElements.Add(relationshipArrow);
                }
            }

            Console.WriteLine("Poly-ItemTypes successfully mapped!");
        }

        private DotClass MapSpecialTypeAsClass(Item type)
        {
            string typeName = type.getProperty("name");

            var typeClass = Graph.GetDotClassByName(typeName);

            int index = Graph.GraphElements.IndexOf(typeClass);

            typeClass = new DotClass
            {
                Name = typeName,
                Label = GenerateLabelFromProperties(type)
            };

            Graph.GraphElements[index] = typeClass;
            return typeClass;
        }

        private void MapPropertyRelations(Item itemType)
        {
            var itemProps = itemType.getItemsByXPath(".//Item[@type = 'Property' and data_type = 'item']");
            int itemPropCount = itemProps.getItemCount();
            for (int i = 0; i < itemPropCount; i++)
            {
                var currentProp = itemProps.getItemByIndex(i);
                string dataSource = currentProp.getPropertyAttribute("data_source", "name", "");
                if (dataSource == "") continue;
                var relationshipArrow =
                    new DotArrow(itemType.getProperty("name", "UnknownItemType"), dataSource, Graph)
                        {CustomStyle = $"label = \"{currentProp.getProperty("name", "")}\""};
                Graph.GraphElements.Add(relationshipArrow);
            }
        }

        private void MapRelsAsClasses(Item itemType, string sourceClassName, string targetClassName)
        {
            var relClass = MapSpecialTypeAsClass(itemType);
            var sourceArrow = new DotArrow(relClass.Name, sourceClassName, Graph)
                {CustomStyle = "label = source_id"};
            Graph.GraphElements.Add(sourceArrow);
            var targetArrow = new DotArrow(relClass.Name, targetClassName, Graph)
                {CustomStyle = "label = related_id"};
            Graph.GraphElements.Add(targetArrow);
        }

        private static string GenerateLabelFromProperties(Item itemType)
        {
            var labelBuilder =
                new StringBuilder($"{itemType.getProperty("name", $"\"{itemType.getID()}\"")}|");

            var propRels = itemType.getRelationships("Property");
            int propRelCount = propRels.getItemCount();
            for (int i = 0; i < propRelCount; i++)
            {
                var currentProp = propRels.getItemByIndex(i);
                string propName = currentProp.getProperty("name", currentProp.getID());
                if (propName == "related_id" || propName == "source_id") continue;
                string propDataSource = currentProp.getPropertyAttribute("data_source", "name");
                string propDataType = currentProp.getProperty("data_type", "noDataTypeFound");
                if (propDataType == "item" && !string.IsNullOrEmpty(propDataSource)) continue;

                labelBuilder.Append($"{propName} : {propDataType}\\l");
            }

            return labelBuilder.ToString();
        }

        private static Tuple<string, string> DetermineSourceAndTargetName(Item propRels)
        {
            int propCount = propRels.getItemCount();
            string sourceClassName = "";
            string targetClassName = "";
            for (int i = 0; i < propCount; i++)
            {
                var currentProp = propRels.getItemByIndex(i);
                string propName = currentProp.getProperty("name", "");
                switch (propName)
                {
                    case "source_id":
                        sourceClassName =
                            currentProp.getPropertyAttribute("data_source", "name", "");
                        break;
                    case "related_id":
                        targetClassName =
                            currentProp.getPropertyAttribute("data_source", "name", "");
                        break;
                }
            }

            return new Tuple<string, string>(sourceClassName, targetClassName);
        }
    }
}