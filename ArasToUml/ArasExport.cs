using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Aras.IOM;
using net.sf.dotnetcli;

namespace ArasToUml
{
    internal class ArasExport
    {
        internal static Innovator MyInnovator;
        public readonly string Prefix;

        internal ArasExport(CommandLine cmd)
        {
            Console.WriteLine("Establishing server connection...");
            ArasLogin login = new ArasLogin(
                cmd.GetOptionValue("u"),
                cmd.GetOptionValue("d"),
                cmd.GetOptionValue("l"),
                cmd.GetOptionValue("p"));
            Console.WriteLine("Server connection established.");

            MyInnovator = login.Innovator;
            Prefix = cmd.GetOptionValue("f");
            bool excludeDefProps = cmd.HasOption("e");

            Console.WriteLine($"Fetching all ItemTypes with prefix {Prefix}...");
            AllItemTypes = FetchAllItemTypes(excludeDefProps);

            login.LogOut();

            int allItemCount = AllItemTypes.getItemCount();
            switch (allItemCount)
            {
                case -1:
                    throw new ItemApplyException(
                        $"Error when trying to find ItemTypes: {AllItemTypes.getErrorString()}");
                case 0:
                    Console.WriteLine($"No ItemTypes found with prefix {Prefix}. Please check prefix and run again.");
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine($"{allItemCount} ItemTypes found.");
                    break;
            }
        }

        private Item AllItemTypes { get; }
        internal Item ItemTypes { get; set; }
        internal Item RelationshipTypes { get; set; }
        internal Item PolyItemTypes { get; set; }

        private Item FetchAllItemTypes(bool excludeDefProps)
        {
            Item allItemTypes = MyInnovator.newItem("ItemType", "get");
            allItemTypes.setAttribute("serverEvents", "0");
            allItemTypes.setAttribute("select", "is_relationship, name");
            allItemTypes.setProperty("name", $"{Prefix}*");
            allItemTypes.setPropertyCondition("name", "like");
            Item morphaeRel = MyInnovator.newItem("Morphae", "get");
            morphaeRel.setAttribute("select", "related_id(name)");
            allItemTypes.addRelationship(morphaeRel);
            Item propertyRel = MyInnovator.newItem("Property", "get");
            propertyRel.setAttribute("select", "name, data_type, data_source");
            if (excludeDefProps)
            {
                propertyRel.setPropertyCondition("name", "not in");
                propertyRel.setProperty("name", CreateDefPropsExclusionClause());
            }

            allItemTypes.addRelationship(propertyRel);
            return allItemTypes.apply();
        }

        private static string CreateDefPropsExclusionClause()
        {
            List<string> propsToExclude = new List<string>
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
            StringBuilder clauseBuilder = new StringBuilder("");
            foreach (string property in propsToExclude)
            {
                clauseBuilder.Append($",'{property}'");
            }

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