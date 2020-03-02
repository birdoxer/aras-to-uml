using System.Collections.Generic;
using CommandLine;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace ArasToUml
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ArgOptions
    {
        [Value(0, Required = false,
            HelpText = "File path to which the .dot file will be saved. Defaults to C:\\temp\\temp.dot")]
        public string FilePath { get; set; } = "C:\\temp\\temp.dot";

        [Option('u', "url", Required = true, HelpText = "URL to Aras instance")]
        public string Url { get; set; }

        [Option('d', "database", Required = true, HelpText = "Name of database")]
        public string Database { get; set; }

        [Option('l', "login", Required = true, HelpText = "Login username")]
        public string Login { get; set; }

        [Option('p', "password", Required = true, HelpText = "Login password")]
        public string Password { get; set; }

        [Option('f', "prefix", Required = false, Separator = ',', HelpText = "Prefixes of ItemType names, separated by commas")]
        public IEnumerable<string> Prefixes { get; set; }

        [Option('g', "package", Required = false, HelpText = "PackageDefinition ItemTypes are grouped in")]
        public string PackageDefinition { get; set; }

        [Option('r', "relitems", Required = false,
            HelpText =
                "Determines whether ItemTypes with is_relationship equal to '1' shall be exported as classes as well")]
        public bool RelsAsClasses { get; set; }

        [Option('e', "exclude", Required = false,
            HelpText = "Determines whether Aras default ItemType properties are excluded")]
        public bool ExcludeDefaultProps { get; set; }

        [Option('w', "wide", Required = false,
            HelpText = "If given, ItemTypes in the given package OR with the given prefix are considered.")]
        public bool UseWiderSearch { get; set; }
    }
}