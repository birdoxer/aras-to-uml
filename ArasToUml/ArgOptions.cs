using System;
using net.sf.dotnetcli;

namespace ArasToUml
{
    internal class ArgOptions
    {
        private readonly CommandLine _cmd;
        private readonly Options _options;

        /// <summary>
        ///     The constructor for the class which defines the <see cref="Options" /> for the program and stores the
        ///     <see cref="CommandLine" /> <paramref name="args" /> the application was started with.
        /// </summary>
        /// <remarks>
        ///     The only constructor for the class. The <see cref="Options" /> defined here are stored on the
        ///     instance in a field. The <see cref="CommandLine" /> <paramref name="args" /> are parsed
        ///     (i.e. compared to the available options) and stored in another field.
        /// </remarks>
        /// <param name="args">The command line arguments that the program was started with.</param>
        public ArgOptions(string[] args)
        {
            _options = new Options();
            _options.AddOption("u", "url", true, "URL to Aras instance");
            _options.AddOption("d", "db", true, "Name of database");
            _options.AddOption("l", "login", true, "Login username");
            _options.AddOption("p", "pw", true, "Login password");
            _options.AddOption("f", "prefix", true, "Prefix of ItemType names");
            _options.AddOption("g", "package", true, "PackageDefinition ItemTypes are grouped in");
            _options.AddOption("r", "relitems", false,
                "Determines whether ItemTypes with is_relationship equal to '1' shall be exported as classes as well");
            _options.AddOption("i", "filepath", true,
                @"File path to which the .dot file will be saved. Defaults to C:\temp\temp.dot");
            _options.AddOption("e", null, false, "Determines whether Aras default ItemType properties are excluded");

            _options.AddOption("h", "help", false,
                "If this option is found, the program will ignore all other options and print usage/help information.");

            ICommandLineParser parser = new BasicParser();
            var cmd = parser.Parse(_options, args);
            _cmd = cmd;
        }

        /// <summary>
        ///     A simple Getter for the <see cref="CommandLine" /> object on the instance.
        /// </summary>
        /// <returns>The <see cref="CommandLine" /> object stored in the corresponding field.</returns>
        public CommandLine GetCmd()
        {
            return _cmd;
        }

        /// <summary>
        ///     Method that prints out help/usage information and exits the program if the corresponding command line
        ///     argument was given.
        /// </summary>
        public void DealWithHelpOption()
        {
            if (!_cmd.HasOption("h")) return;
            var formatter = new HelpFormatter();
            formatter.PrintHelp("cmd", _options);
            Environment.Exit(0);
        }
    }
}