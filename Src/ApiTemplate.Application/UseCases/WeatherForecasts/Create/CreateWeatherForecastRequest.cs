#if (UseValidation)
using ApiTemplate.Application.Validation;
using FluentValidation.Results;
#endif

namespace ApiTemplate.Application.UseCases.WeatherForecasts.Create;
#if (UseValidation)
public class CreateWeatherForecastRequest : IValidatableRequest
#else
public class CreateWeatherForecastRequest
#endif
{
    public int TemperatureC { get; set; }
    public string? Summary { get; set; }

#if (UseValidation)
    public ValidationResult Validate() =>
        new CreateWeatherForecastRequestValidator().Validate(this);
#endif
}