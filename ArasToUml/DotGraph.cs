using System.Collections.Generic;
using System.Linq;

namespace ArasToUml
{
    public class DotGraph : DotElementWithAttributes
    {
        public DotGraph(string name = "G")
        {
            Name = name;
        }

        public bool IsDiGraph { get; set; } = true;
        public string Name { get; set; }
        public List<DotElement> GraphElements { get; } = new List<DotElement>();

        public DotClass GetDotClassByName(string className)
        {
            List<DotClass> classList = GraphElements.FindAll(el => el.GetType() == typeof(DotClass)).Cast<DotClass>()
                .ToList();
            return classList.Find(dotClass => dotClass.Name == className);
        }
    }
}