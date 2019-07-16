using System;
using net.sf.dotnetcli;

namespace ArasToUml
{
    public class Program
    {
        private readonly CommandLine _cmd;

        private Program(string[] args)
        {
            ArgOptions argOptions = new ArgOptions(args);
            argOptions.DealWithHelpOption();
            _cmd = argOptions.GetCmd();
        }

        public static void Main(string[] args)
        {
            Program program = new Program(args);
            ArasExport arasExport = program.PerformArasExport();
        }

        private ArasExport PerformArasExport()
        {
            ArasExport arasExport = new ArasExport(_cmd);
            arasExport.SplitAllItemTypes();
            return arasExport;
        }
    }
}