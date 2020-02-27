using System;
using System.Collections.Generic;
using System.Text;
using Aras.IOM;
using net.sf.dotnetcli;

namespace ArasToUml
{
    internal class ArasExport
    {
        private static Innovator _myInnovator;
        private readonly string _packageName;
        private readonly string _prefix;
        private readonly bool _doConsiderPackage;

        internal ArasExport(CommandLine cmd)
        {
            Console.WriteLine("Establishing server connection...");
            var login = new ArasLogin(
                cmd.GetOptionValue("u"),
                cmd.GetOptionValue("d"),
                cmd.GetOptionValue("l"),
                cmd.GetOptionValue("p"));
            Console.WriteLine("Server connection established.");

            _myInnovator = login.Innovator;
            _prefix = cmd.GetOptionValue("f");
            _packageName = cmd.GetOptionValue("g");
            _doConsiderPackage = !string.IsNullOrWhiteSpace(_packageName);
            bool excludeDefProps = cmd.HasOption("e");

            Console.Write($"Fetching all ItemTypes with prefix {_prefix}");
            if (_doConsiderPackage)
                Console.Write($" inside PackageDefinition {_packageName}");
            Console.WriteLine("...");
            AllItemTypes = FetchAllItemTypes(excludeDefProps);

            login.LogOut();

            int allItemCount = AllItemTypes.getItemCount();
            switch (allItemCount)
            {
                case -1:
                    throw new ItemApplyException(
                        $"Error when trying to find ItemTypes: {AllItemTypes.getErrorString()}");
                case 0:
                    Console.WriteLine($"No ItemTypes found. Please check prefix and/or package name and run again.");
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine($"{allItemCount} ItemTypes found.");
                    break;
            }
        }

        private Item AllItemTypes { get; }
        internal Item ItemTypes { get; private set; }
        internal Item RelationshipTypes { get; private set; }
        internal Item PolyItemTypes { get; private set; }

        private Item FetchAllItemTypes(bool excludeDefProps)
        {
            var allItemTypes = _myInnovator.newItem("ItemType", "get");
            allItemTypes.setAttribute("serverEvents", "0");
            allItemTypes.setAttribute("select", "is_relationship, name");
            allItemTypes.setProperty("name", $"{_prefix}*");
            allItemTypes.setPropertyCondition("name", "like");
            if (_doConsiderPackage)
            {
                var andItem = allItemTypes.newAND();
                andItem.setProperty("name", CreatePackageItemTypeCollection());
                andItem.setPropertyCondition("name", "in");
            }

            var morphaeRel = _myInnovator.newItem("Morphae", "get");
            morphaeRel.setAttribute("select", "related_id(name)");
            allItemTypes.addRelationship(morphaeRel);
            var propertyRel = _myInnovator.newItem("Property", "get");
            propertyRel.setAttribute("select", "name, data_type, data_source");
            if (excludeDefProps)
            {
                propertyRel.setPropertyCondition("name", "not in");
                propertyRel.setProperty("name", CreateDefPropsExclusionClause());
            }

            allItemTypes.addRelationship(propertyRel);
            return allItemTypes.apply();
        }

        private string CreatePackageItemTypeCollection()
        {
            var nameSet = new HashSet<string>();
            var packageDefinition = _myInnovator.newItem("PackageDefinition", "get");
            packageDefinition.setAttribute("select", "id");
            packageDefinition.setProperty("name", _packageName);
            var packageGroup = _myInnovator.newItem("PackageGroup", "get");
            packageGroup.setAttribute("select", "id");
            packageGroup.setProperty("name", "'ItemType','RelationshipType'");
            packageGroup.setPropertyCondition("name", "in");
            packageDefinition.addRelationship(packageGroup);
            packageDefinition = packageDefinition.apply();
            if (packageDefinition.getItemCount() != 1)
                throw new ArgumentException($"No PackageDefinition found with name {_packageName}");

            packageGroup = packageDefinition.getRelationships("PackageGroup");
            int groupCount = packageGroup.getItemCount();
            if (groupCount < 1) 
                throw new ArgumentException($"No ItemTypes found in PackageDefinition with name {_packageName}");

            var idSet = new HashSet<string>();
            for (int i = 0; i < groupCount; i++)
            {
                idSet.Add(packageGroup.getItemByIndex(i).getID());
            }
            
            var packageElements = _myInnovator.newItem("PackageElement", "get");
            packageElements.setAttribute("select", "name");
            packageElements.setProperty("source_id", $"'{string.Join("','", idSet)}'");
            packageElements.setPropertyCondition("source_id", "in");
            packageElements = packageElements.apply();
            int elementCount = packageElements.getItemCount();
            if (elementCount < 1)
                throw new ArgumentException(
                    $"No ItemTypes found in 'ItemTypes' PackageGroup of PackageDefinition with name {_packageName}");

            for (int i = 0; i < elementCount; i++)
            {
                string currentName = packageElements.getItemByIndex(i).getProperty("name", "");
                if (currentName == "") continue;
                nameSet.Add(currentName);
            }

            return $"'{string.Join("','", nameSet)}'";
        }

        private static string CreateDefPropsExclusionClause()
        {
            var propsToExclude = new List<string>
            {
                "allow_private_permission", "auto_search", "behavior", "classification", "close_icon", "config_id",
                "core", "created_by_id", "created_on", "css", "current_state", "default_page_size", "effective_date",
                "enforce_discovery", "generation", "help_url", "hide_where_used", "id", "implementation_type",
                "instance_data", "is_current", "is_dependent", "is_relationship", "is_released", "is_versionable",
                "keyed_name", "label", "label_plural", "large_icon", "locked_by_id ", "major_rev", "managed_by_id",
                "minor_rev", "modified_by_id", "modified_on", "new_version", "not_lockable", "open_icon",
                "owned_by_id ", "permission_id", "release_date", "revisions", "show_parameters_tab", "sort_order",
                "state", "structure_view", "superseded_date", "team_id", "unlock_on_logout", "use_src_access"
            };
            var clauseBuilder = new StringBuilder("");
            foreach (string property in propsToExclude) clauseBuilder.Append($",'{property}'");

            return clauseBuilder.ToString().Substring(1);
        }

        internal void SplitAllItemTypes()
        {
            ItemTypes = AllItemTypes.getItemsByXPath(
                "//Item[is_relationship = '0' and not(boolean(Relationships/Item[@type = 'Morphae']))]");
            RelationshipTypes = AllItemTypes.getItemsByXPath("//Item[is_relationship = '1']");
            PolyItemTypes =
                AllItemTypes.getItemsByXPath(
                    "//Item[is_relationship = '0' and boolean(Relationships/Item[@type = 'Morphae'])]");
        }
    }
}