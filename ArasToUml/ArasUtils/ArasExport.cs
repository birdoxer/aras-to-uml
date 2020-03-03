using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aras.IOM;

namespace ArasToUml.ArasUtils
{
    /// <summary>
    ///     Class that performs the data model export from Aras.
    /// </summary>
    internal class ArasExport
    {
        private static Innovator _myInnovator;
        private readonly ArasLogin _arasLogin;
        private readonly bool _doConsiderPackage;
        private readonly bool _excludeDefaultProps;
        private readonly string _packageName;
        private readonly List<string> _prefixList;
        private readonly bool _prefixWasGiven;
        private readonly bool _useWiderSearch;

        /// <summary>
        ///     Only constructor for the class.
        /// </summary>
        /// <remarks>
        ///     Logs into Innovator and sets the class fields/properties relating to Aras and the command line options.
        /// </remarks>
        /// <param name="options">The <see cref="ArgOptions" /> the program was started with.</param>
        internal ArasExport(ArgOptions options)
        {
            Console.WriteLine("Establishing server connection...");
            var login = new ArasLogin(options.Url, options.Database, options.Login, options.Password);
            Console.WriteLine("Server connection established.");

            _arasLogin = login;
            _myInnovator = login.Innovator;
            _prefixList = options.Prefixes?.ToList();
            _prefixWasGiven = _prefixList?.Any() ?? false;
            _packageName = options.PackageDefinition;
            _doConsiderPackage = !string.IsNullOrWhiteSpace(_packageName);
            _useWiderSearch = options.UseWiderSearch;
            _excludeDefaultProps = options.ExcludeDefaultProps;
        }

        private Item AllItemTypes { get; set; }
        internal Item ItemTypes { get; private set; }
        internal Item RelationshipTypes { get; private set; }
        internal Item PolyItemTypes { get; private set; }

        /// <summary>
        ///     Gets the desired data model from Aras and writes corresponding info to the console.
        /// </summary>
        /// <remarks>
        ///     Logs out of Aras Innovator after the getting the data model, even if an error occurred during that fetch.
        /// </remarks>
        /// <exception cref="ItemApplyException">Thrown if an error occured using apply() on an Aras Item object.</exception>
        internal void RunExport()
        {
            Console.Write("Fetching all ItemTypes");
            // ReSharper disable once AssignNullToNotNullAttribute
            // ReSharper does not realise that a null _prefixList is not possible here due to _prefixWasGiven
            if (_prefixWasGiven) Console.Write($" with prefixes '{string.Join(", ", _prefixList)}'");
            if (_doConsiderPackage)
            {
                if (_useWiderSearch && _prefixWasGiven) Console.Write(" or");
                Console.Write($" inside PackageDefinition '{_packageName}'");
            }

            Console.WriteLine("...");

            try
            {
                AllItemTypes = FetchAllItemTypes(_excludeDefaultProps);
            }
            finally
            {
                _arasLogin.LogOut();
            }

            int allItemCount = AllItemTypes.getItemCount();
            switch (allItemCount)
            {
                case -1:
                    throw new ItemApplyException(
                        $"Error when trying to find ItemTypes: {AllItemTypes.getErrorString()}");
                case 0:
                    Console.WriteLine("No ItemTypes found. Please check prefix(es) and/or package name and run again.");
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine($"{allItemCount} ItemTypes found.");
                    break;
            }
        }

        /// <summary>
        ///     Queries the Innovator instance for the data model defined by the command line options.
        /// </summary>
        /// <remarks>
        ///     This is the method that actually builds up the query items and sends request to the Aras instance.
        /// </remarks>
        /// <param name="excludeDefProps">
        ///     Determines whether the resulting .dot classes shall contain Aras default properties or not.
        ///     See <see cref="CreateDefPropsExclusionClause" />.
        /// </param>
        /// <returns>An Item object containing all ItemTypes in the data model.</returns>
        private Item FetchAllItemTypes(bool excludeDefProps)
        {
            var allItemTypes = _myInnovator.newItem("ItemType", "get");
            allItemTypes.setAttribute("serverEvents", "0");
            allItemTypes.setAttribute("select", "is_relationship, name");

            var logicalItem = _useWiderSearch ? allItemTypes.newOR() : allItemTypes.newAND();

            if (_prefixWasGiven)
            {
                var topLevelOrItem = logicalItem.newOR();
                foreach (string prefix in _prefixList)
                {
                    var orItem = topLevelOrItem.newOR();
                    orItem.setProperty("name", $"{prefix.TrimStart()}*");
                    orItem.setPropertyCondition("name", "like");
                }
            }

            if (_doConsiderPackage)
            {
                var andItem = logicalItem.newAND();
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

        /// <summary>
        ///     Finds the names for all ItemTypes and RelationshipTypes in the given <see cref="_packageName" />.
        /// </summary>
        /// <returns>
        ///     A string containing all ItemType and RelationshipType names out of the given package, formatted to work
        ///     with property condition "in".
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     Thrown if the given package definition does not exist or contains no ItemTypes.
        /// </exception>
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
            for (int i = 0; i < groupCount; i++) idSet.Add(packageGroup.getItemByIndex(i).getID());

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

        /// <summary>
        ///     Creates a clause to exclude Aras default ItemType properties from the exported data model.
        /// </summary>
        /// <returns>
        ///     A string containing all property names that shall be excluded, formatted to work with property
        ///     condition "not in".
        /// </returns>
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

        /// <summary>
        ///     Splits all ItemTypes found by <see cref="FetchAllItemTypes" /> into ItemTypes, RelationshipTypes and
        ///     PolyItem-types.
        /// </summary>
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