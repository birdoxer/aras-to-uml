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
        public string Name { get; }
        public List<DotElement> GraphElements { get; } = new List<DotElement>();

        public DotClass GetDotClassByName(string className, bool createNewIfNotFound = true)
        {
            var classList = GraphElements.FindAll(el => el.GetType() == typeof(DotClass)).Cast<DotClass>()
                .ToList();
            var returnClass = classList.Find(dotClass => dotClass.Name == className);

            if (returnClass != null || !createNewIfNotFound) return returnClass;

            returnClass = new DotClass {Name = className, Label = $"{className}|"};
            GraphElements.Add(returnClass);

            return returnClass;
        }
    }
}