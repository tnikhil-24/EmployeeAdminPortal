namespace EmployeeAdminPortal.Exceptions
{
    public class DuplicateEmailException : Exception
    {
        public DuplicateEmailException(string email)
            : base($"An employee with the email '{email}' already exists.")
        {
        }
    }
}
