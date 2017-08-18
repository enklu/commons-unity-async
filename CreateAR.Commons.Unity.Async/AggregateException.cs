using System;
using System.Collections.Generic;
using System.Linq;

namespace CreateAR.Commons.Unity.Async
{
    /// <summary>
    /// Exception that represents a list of Exceptions.
    /// </summary>
    public class AggregateException : Exception
    {
        /// <summary>
        /// List of internal exceptions.
        /// </summary>
        public readonly List<Exception> Exceptions = new List<Exception>();
        
        /// <summary>
        /// A good ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "[AggregateException \n{0}]",
                Enumerable.Select<Exception, string>(Exceptions, exception => exception.ToString())
                    .Aggregate((a, b) => $"{a}, {b}"));
        }
    }
}