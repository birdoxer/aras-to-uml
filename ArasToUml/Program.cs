using net.sf.dotnetcli;

namespace ArasToUml
{
    public class Program
    {
        private readonly CommandLine _cmd;

        private Program(string[] args)
        {
            var argOptions = new ArgOptions(args);
            argOptions.DealWithHelpOption();
            _cmd = argOptions.GetCmd();
        }

        public static void Main(string[] args)
        {
            var program = new Program(args);
            var arasExport = program.PerformArasExport();
            var graphCreator = new GraphCreator(arasExport);
            bool relsAsClasses = program._cmd.HasOption("r");
            graphCreator.CreateGraph(relsAsClasses);
            string filePath = program._cmd.HasOption("i") ? program._cmd.GetOptionValue("i") : @"C:\temp\temp.dot";
            graphCreator.ExportGraph(filePath);
        }

        private ArasExport PerformArasExport()
        {
            var arasExport = new ArasExport(_cmd);
            arasExport.SplitAllItemTypes();
            return arasExport;
        }
    }
}