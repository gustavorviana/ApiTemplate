using FluentValidation;

namespace ApiTemplate.Application.UseCases.WeatherForecasts.Create;

public class CreateWeatherForecastRequestValidator : AbstractValidator<CreateWeatherForecastRequest>
{
	public CreateWeatherForecastRequestValidator()
	{
		RuleFor(x => x.TemperatureC)
			.InclusiveBetween(-100, 100)
			.WithMessage("'{PropertyName}' must be between -100 and 100. You entered {PropertyValue}.");

		RuleFor(x => x.Summary)
			.MaximumLength(500)
			.When(x => x.Summary is not null)
			.WithMessage("'{PropertyName}' must not exceed 500 characters.");
	}
}
