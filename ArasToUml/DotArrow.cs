namespace ArasToUml
{
    public class DotArrow : DotElement
    {
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

        public DotClass Source { get; private set; }
        public DotClass Target { get; private set; }
        public string CustomStyle { get; set; }

        private void SetSourceWithClassName(string sourceName, DotGraph dotGraph)
        {
            Source = dotGraph.GetDotClassByName(sourceName);
        }

        private void SetTargetWithClassName(string targetName, DotGraph dotGraph)
        {
            Target = dotGraph.GetDotClassByName(targetName);
        }
    }
}