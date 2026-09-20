namespace JobApplication.Application.Exceptions
{
    public class BusinessValidationException : Exception
    {
        public IReadOnlyCollection<string> Errors { get; }

        public BusinessValidationException(string error)
            : this(new[] { error })
        {
        }

        public BusinessValidationException(IEnumerable<string> errors)
            : base("One or more validation errors occurred.")
        {
            Errors = errors.ToList();
        }
    }
}
