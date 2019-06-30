using System;
using System.Collections.Generic;

namespace ApplicationCore.Exceptions
{
    public class BadRequestException : Exception
    {
        public BadRequestException(IEnumerable<(string, string)> errors)
        {
            this.Errors = errors ?? new List<(string, string)>();
        }

        public IEnumerable<(string, string)> Errors { get; }
    }
}
