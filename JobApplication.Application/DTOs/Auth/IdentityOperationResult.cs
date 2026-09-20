namespace JobApplication.Application.DTOs.Auth
{
    public class IdentityOperationResult
    {
        public bool Succeeded { get; set; }
        public string? UserId { get; set; }
        public bool IsDuplicateEmail { get; set; }
        public IReadOnlyCollection<string> Errors { get; set; } = Array.Empty<string>();

        public static IdentityOperationResult Success(string? userId = null)
            => new() { Succeeded = true, UserId = userId };

        public static IdentityOperationResult Failure(IEnumerable<string> errors, bool isDuplicateEmail = false)
            => new() { Succeeded = false, Errors = errors.ToList(), IsDuplicateEmail = isDuplicateEmail };
    }
}
