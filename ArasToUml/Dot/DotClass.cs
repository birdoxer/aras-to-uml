namespace ArasToUml.Dot
{
    public class DotClass : DotElementWithAttributes
    {
        private string _name;

        public string Name
        {
            get => _name;
            set => _name = value.Trim().Contains(" ") ? $"\"{value}\"" : value;
        }

        public string Label { get; set; }
    }
}