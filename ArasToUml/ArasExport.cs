using System;
using Aras.IOM;
using net.sf.dotnetcli;

namespace ArasToUml
{
    internal class ArasExport
    {
        internal static Innovator MyInnovator;
        private readonly int _allItemCount;
        private readonly string _prefix;

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
            _prefix = cmd.GetOptionValue("f");

            Console.WriteLine($"Fetching all ItemTypes with prefix {_prefix}...");
            AllItemTypes = MyInnovator.newItem("ItemType", "get");
            AllItemTypes.setAttribute("serverEvents", "0");
            AllItemTypes.setProperty("name", $"{_prefix}*");
            AllItemTypes.setPropertyCondition("name", "like");
            Item morphaeRel = MyInnovator.newItem("Morphae", "get");
            morphaeRel.setAttribute("select", "related_id(name)");
            AllItemTypes.addRelationship(morphaeRel);
            AllItemTypes = AllItemTypes.apply();

            _allItemCount = AllItemTypes.getItemCount();
            switch (_allItemCount)
            {
                case -1:
                    throw new ItemApplyException(
                        $"Error when trying to find ItemTypes: {AllItemTypes.getErrorString()}");
                case 0:
                    Console.WriteLine($"No ItemTypes found with prefix {_prefix}. Please check prefix and run again.");
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine($"{_allItemCount} ItemTypes found.");
                    break;
            }
        }

        private Item AllItemTypes { get; }
        internal Item ItemTypes { get; set; }
        internal Item RelationshipTypes { get; set; }
        internal Item PolyItemTypes { get; set; }

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