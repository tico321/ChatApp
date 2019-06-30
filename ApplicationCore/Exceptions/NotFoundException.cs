using System;

namespace ApplicationCore.Exceptions
{
    public class NotFoundException : Exception
    {
        private readonly string modelName;

        public NotFoundException(string modelName, string id)
        {
            this.modelName = modelName;
            this.Id = id;
        }

        public override string Message => $"{this.modelName} with Id {this.Id} was not found.";

        public string Id { get; }
    }
}
