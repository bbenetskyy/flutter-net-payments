namespace MicroApp.Application.Validation;

public interface IValidator<T>
{
    ValidationResult Validate(T input);
}
