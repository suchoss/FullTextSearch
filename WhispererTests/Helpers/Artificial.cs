namespace WhispererTests;

public class Artificial : IEquatable<Artificial>
{
    public int Id { get; init; }
    public string Text { get; set; }
    public string Filter { get; set; }

    public bool Equals(Artificial? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Artificial)obj);
    }

    public override int GetHashCode()
    {
        return Id;
    }

}