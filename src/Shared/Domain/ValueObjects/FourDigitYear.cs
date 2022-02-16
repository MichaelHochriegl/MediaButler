using MediaButler.Shared.Domain.Common;

namespace MediaButler.Shared.Domain.ValueObjects;

public class FourDigitYear : ValueObject
{
    public int Year { get; private set; }

    private FourDigitYear()
    {
        
    }

    public FourDigitYear(int year)
    {
        Year = year;
    }

    public FourDigitYear(DateOnly dateOnly)
    {
        Year = dateOnly.Year;
    }

    public FourDigitYear(DateTime dateTime)
    {
        Year = dateTime.Year;
    }
}