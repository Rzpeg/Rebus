﻿using System;
#if NET45
using System.Runtime.Serialization;
#endif

namespace Rebus.Exceptions
{
    /// <summary>
    /// Special exception that signals that some kind of optimistic lock has been violated, and work must most likely be aborted &amp; retried
    /// </summary>
#if NET45
    [Serializable]
    public class ConcurrencyException : Exception
# elif NETSTANDARD1_6
    public class ConcurrencyException : Exception
#endif
    {
        /// <summary>
        /// Constructs the exception
        /// </summary>
        public ConcurrencyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructs the exception
        /// </summary>
        public ConcurrencyException(Exception innerException, string message)
            : base(message, innerException)
        {
        }

#if NET45
        /// <summary>
        /// Constructs the exception
        /// </summary>
        public ConcurrencyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}