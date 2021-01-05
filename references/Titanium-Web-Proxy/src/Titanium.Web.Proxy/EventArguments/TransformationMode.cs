namespace Titanium.Web.Proxy.EventArguments
{
    internal enum TransformationMode
    {
        None,

        /// <summary>
        ///     Removes the chunked encoding
        /// </summary>
        RemoveChunked,

        /// <summary>
        ///     Uncompress the body (this also removes the chunked encoding if exists)
        /// </summary>
        Uncompress
    }
}
