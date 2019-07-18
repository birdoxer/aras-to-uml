namespace ArasToUml
{
    public class DotArrow : DotElement
    {
        public DotArrow()
        {
        }

        public DotArrow(DotClass source, DotClass target)
        {
            Source = source;
            Target = target;
        }

        public DotArrow(string source, string target, DotGraph dotGraph)
        {
            SetSourceWithClassName(source, dotGraph);
            SetTargetWithClassName(target, dotGraph);
        }

        public DotClass Source { get; set; }
        public DotClass Target { get; set; }
        public string CustomStyle { get; set; }

        public void SetSourceWithClassName(string sourceName, DotGraph dotGraph)
        {
            Source = dotGraph.GetDotClassByName(sourceName);
        }

        public void SetTargetWithClassName(string targetName, DotGraph dotGraph)
        {
            Target = dotGraph.GetDotClassByName(targetName);
        }
    }
}