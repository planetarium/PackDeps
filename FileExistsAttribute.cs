namespace PackDeps;

using System.ComponentModel.DataAnnotations;
using System.IO;

public class FileExistsAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(
        object? value,
        ValidationContext validationContext
    )
    {
        if (value is string path)
        {
            if (File.Exists(path))
            {
                return ValidationResult.Success;
            }
            else if (Directory.Exists(path))
            {
                return new ValidationResult($"{path} is not a regular file.");
            }

            return new ValidationResult($"No such file: {path}.");
        }

        return new ValidationResult("Value is not a string.");
    }
}
