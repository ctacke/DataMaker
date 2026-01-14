using System;

namespace DataMaker
{
    /// <summary>
    /// Maps a property to a generated address data value.
    /// </summary>
    internal class AddressDataMapping : IPropertyMapping
    {
        private readonly Generator _generator;
        private readonly Func<AddressData, object> _accessor;

        public AddressDataMapping(Generator generator, Func<AddressData, object> accessor)
        {
            _generator = generator;
            _accessor = accessor;
        }

        public object? GetValue(IDataRow primaryRow, IDataProvider provider, int index)
        {
            var addressData = _generator.GetAddress(index);
            return _accessor(addressData);
        }
    }
}
