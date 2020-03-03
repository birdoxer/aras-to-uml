using System;
using System.Collections.Generic;
using ArasToUml.ArasUtils;
using ArasToUml.Graph;
using CommandLine;

namespace ArasToUml
{
    public static class Program
    {
        /// <summary>
        ///     Starting point of the application.
        /// </summary>
        /// <param name="args">
        ///     The command line arguments. See <see cref="ArgOptions"/> or run program with --help for
        ///     details.
        /// </param>
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<ArgOptions>(args)
                .WithParsed(RunOptionsAndPerformLogic)
                .WithNotParsed(HandleParseError);
        }

        /// <summary>
        ///     Initiates the Aras export and .dot graph creation and export.
        /// </summary>
        /// <remarks>
        ///     Only gets called if the command line arguments were parsed successfully.
        /// </remarks>
        /// <param name="options">The <see cref="ArgOptions"/> the program was started with.</param>
        private static void RunOptionsAndPerformLogic(ArgOptions options)
        {
            var arasExport = PerformArasExport(options);
            var graphCreator = new GraphCreator(arasExport);
            graphCreator.CreateGraph(options.RelsAsClasses);
            graphCreator.ExportGraph(options.FilePath ?? @"C:\temp\temp.dot");
        }

        /// <summary>
        ///     Prints out any errors that occurred during command line argument parsing.
        /// </summary>
        /// <remarks>
        ///     Only gets called if the command line arguments could not be parsed successfully.
        /// </remarks>
        /// <param name="errors">The errors that occurred during command line argument parsing.</param>
        private static void HandleParseError(IEnumerable<Error> errors)
        {
            Console.WriteLine("Error occured when parsing the command line arguments:");
            foreach (var error in errors) Console.WriteLine(error);
        }

        /// <summary>
        ///     Initiates the export of the data model from Aras.
        /// </summary>
        /// <param name="options">The <see cref="ArgOptions"/> the program was started with.</param>
        /// <returns>An <see cref="ArasExport"/> object that holds the exported data model.</returns>
        private static ArasExport PerformArasExport(ArgOptions options)
        {
            var arasExport = new ArasExport(options);
            arasExport.RunExport();
            arasExport.SplitAllItemTypes();
            return arasExport;
        }
    }
}