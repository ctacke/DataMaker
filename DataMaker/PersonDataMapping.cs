using System;

namespace DataMaker
{
    /// <summary>
    /// Maps a property to a generated person data value.
    /// </summary>
    internal class PersonDataMapping : IPropertyMapping
    {
        private readonly Func<PersonData, object> _formatter;

        public PersonDataMapping(Func<PersonData, object> formatter)
        {
            _formatter = formatter;
        }

        public object? GetValue(IDataRow primaryRow, IDataProvider provider, int index)
        {
            var personData = PersonDataGenerator.Generate();
            return _formatter(personData);
        }
    }
}
