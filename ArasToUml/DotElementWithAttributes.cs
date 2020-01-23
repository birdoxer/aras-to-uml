using System.Collections.Generic;
// ReSharper disable UnusedMember.Global

namespace ArasToUml
{
    public abstract class DotElementWithAttributes : DotElement
    {
        private readonly Dictionary<string, string> _attributes = new Dictionary<string, string>();

        public void AddAttribute(string attributeName, string attributeValue)
        {
            _attributes.Add(attributeName, attributeValue);
        }

        private void RemoveAttribute(string attributeName)
        {
            _attributes.Remove(attributeName);
        }

        public void ChangeAttribute(string attributeName, string newValue)
        {
            RemoveAttribute(attributeName);
            AddAttribute(attributeName, newValue);
        }

        public string GetAttributeValue(string attributeName)
        {
            return _attributes[attributeName];
        }

        public Dictionary<string, string> GetAttributes()
        {
            return _attributes;
        }
    }
}