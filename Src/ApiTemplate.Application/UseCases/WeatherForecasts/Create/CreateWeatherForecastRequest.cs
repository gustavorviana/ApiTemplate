using ApiTemplate.Application.UseCases;
using ApiTemplate.Application.Validation;
using FluentValidation.Results;

namespace ApiTemplate.Application.UseCases.WeatherForecasts.Create;

public class CreateWeatherForecastRequest : IValidatableRequest
{
	public int TemperatureC { get; set; }
	public string? Summary { get; set; }

	public ValidationResult Validate() =>
		new CreateWeatherForecastRequestValidator().Validate(this);
}
