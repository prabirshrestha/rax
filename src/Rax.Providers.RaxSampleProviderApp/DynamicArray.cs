namespace Rax.Providers.RaxSampleProviderApp
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the json array.
    /// </summary>
    public class DynamicArray : List<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicArray"/> class. 
        /// </summary>
        public DynamicArray() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicArray"/> class. 
        /// </summary>
        /// <param name="capacity">The capacity of the json array.</param>
        public DynamicArray(int capacity) : base(capacity) { }

        /// <summary>
        /// The json representation of the array.
        /// </summary>
        /// <returns>The json representation of the array.</returns>
        public override string ToString()
        {
            //return SimpleJson.SerializeObject(this) ?? string.Empty;
            return base.ToString();
        }
    }
}