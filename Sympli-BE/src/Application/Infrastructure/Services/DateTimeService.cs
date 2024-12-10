using Sympli.Application.Common.Interfaces;

namespace Sympli.Application.Infrastructure.Services;

public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.Now;
}
