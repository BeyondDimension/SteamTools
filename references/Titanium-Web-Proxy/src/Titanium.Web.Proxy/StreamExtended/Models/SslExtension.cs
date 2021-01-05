namespace Titanium.Web.Proxy.StreamExtended.Models
{
    /// <summary>
    ///     The SSL extension information.
    /// </summary>
    public class SslExtension
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public int Value { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public string Data { get; }

        /// <summary>
        /// Gets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public int Position { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SslExtension"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="data">The data.</param>
        /// <param name="position">The position.</param>
        public SslExtension(int value, string name, string data, int position)
        {
            Value = value;
            Name = name;
            Data = data;
            Position = position;
        }
    }
}