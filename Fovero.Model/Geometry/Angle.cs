using System.Text.RegularExpressions;

namespace Fovero.Model.Geometry;

public readonly struct Angle(AngleUnitOfMeasure unitOfMeasure, float value) : IEquatable<Angle>
{
    public const float DegreesPerRadian = 180 / MathF.PI;

    private float Value { get; } = value;

    public AngleUnitOfMeasure UnitOfMeasure { get; } = unitOfMeasure;

    public bool IsMeasuredInRadians => UnitOfMeasure == AngleUnitOfMeasure.Radian;

    public bool IsMeasuredInDegrees => UnitOfMeasure == AngleUnitOfMeasure.Degree;

    public float Radians => IsMeasuredInRadians ? Value : Value / DegreesPerRadian;

    public float Degrees => IsMeasuredInDegrees ? Value : Value * DegreesPerRadian;

    public float Sin()
    {
        return MathF.Sin(Radians);
    }

    public float Cos()
    {
        return MathF.Cos(Radians);
    }

    public Angle ToRadians()
    {
        return To(AngleUnitOfMeasure.Radian);
    }

    public Angle ToDegrees()
    {
        return To(AngleUnitOfMeasure.Degree);
    }

    public Angle To(AngleUnitOfMeasure unitOfMeasure)
    {
        return new Angle(unitOfMeasure, unitOfMeasure == AngleUnitOfMeasure.Degree ? Degrees : Radians);
    }

    public bool Equals(Angle other)
    {
        return Degrees.Equals(other.Degrees);
    }

    public override bool Equals(object? obj)
    {
        return obj is Angle other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (int)UnitOfMeasure * 397 ^ Value.GetHashCode();
        }
    }

    public static Angle Zero => FromRadians(0);

    private static Regex CoordinatePattern { get; } = new(@"^\s*(?<deg>\d+)(°(?<min>\d{1,2})('(?<sec>\d{1,2})\"")?)?");

    public static Angle ParseCoordinate(string s)
    {
        if (s == null)
        {
            throw new ArgumentNullException(nameof(s));
        }

        var matchPattern = CoordinatePattern.Match(s);
        if (!matchPattern.Success)
        {
            throw new FormatException("Failed to parse angle: " + s);
        }

        var deg = float.Parse(matchPattern.Groups["deg"].Value);
        var min = matchPattern.Groups["min"];

        if (min.Success)
        {
            deg += int.Parse(min.Value) / 60;
        }

        var sec = matchPattern.Groups["sec"];

        if (sec.Success)
        {
            deg += int.Parse(sec.Value) / 3600;
        }

        return FromDegrees(deg);
    }

    public static Angle FromDegrees(float degrees, float minutes = 0, float seconds = 0)
    {
        return new Angle(AngleUnitOfMeasure.Degree, degrees + (minutes * 60 + seconds) / 3600);
    }

    public static Angle FromRadians(float value)
    {
        return new Angle(AngleUnitOfMeasure.Radian, value);
    }

    public static Angle Atan2(float y, float x)
    {
        return FromRadians(MathF.Atan2(y, x));
    }

    public static Angle Asin(float d)
    {
        return FromRadians(MathF.Asin(d));
    }

    public static Angle Acos(float d)
    {
        return FromRadians(MathF.Acos(d));
    }

    public static bool operator ==(Angle left, Angle right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Angle left, Angle right)
    {
        return !left.Equals(right);
    }

    public static Angle operator -(Angle a)
    {
        return new Angle(a.UnitOfMeasure, -a.Value);
    }

    public static Angle operator *(Angle a, float factor)
    {
        return new Angle(a.UnitOfMeasure, a.Value * factor);
    }

    public static Angle operator /(Angle a, float factor)
    {
        return new Angle(a.UnitOfMeasure, a.Value / factor);
    }

    public static Angle operator +(Angle a, Angle b)
    {
        return new Angle(a.UnitOfMeasure, a.Value + b.To(a.UnitOfMeasure));
    }

    public static Angle operator -(Angle a, Angle b)
    {
        return new Angle(a.UnitOfMeasure, a.Value - b.To(a.UnitOfMeasure).Value);
    }

    public static implicit operator float(Angle source)
    {
        return source.Value;
    }
}
