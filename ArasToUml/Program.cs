using System;
using System.Collections.Generic;
using ArasToUml.ArasUtils;
using ArasToUml.Graph;
using CommandLine;

namespace ArasToUml
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<ArgOptions>(args)
                .WithParsed(RunOptionsAndPerformLogic)
                .WithNotParsed(HandleParseError);
        }

        private static void RunOptionsAndPerformLogic(ArgOptions options)
        {
            var arasExport = PerformArasExport(options);
            var graphCreator = new GraphCreator(arasExport);
            graphCreator.CreateGraph(options.RelsAsClasses);
            graphCreator.ExportGraph(options.FilePath ?? @"C:\temp\temp.dot");
        }

        private static void HandleParseError(IEnumerable<Error> errors)
        {
            Console.WriteLine("Error occured when parsing the command line arguments:");
            foreach (var error in errors) Console.WriteLine(error);
        }

        private static ArasExport PerformArasExport(ArgOptions options)
        {
            var arasExport = new ArasExport(options);
            arasExport.SplitAllItemTypes();
            return arasExport;
        }
    }
}