namespace Common.Validation;

public interface IValidator<T>
{
    ValidationResult Validate(T input);
}
