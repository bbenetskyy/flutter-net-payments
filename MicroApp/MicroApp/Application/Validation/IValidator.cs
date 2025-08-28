namespace MicroApp.Validation;

public interface IValidator<T>
{
    ValidationResult Validate(T input);
}
