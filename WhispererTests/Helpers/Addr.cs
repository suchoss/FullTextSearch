public class Addr : IEquatable<Addr>
{
    public int Id { get; set; }
    public string Adresa { get; set; }

    public int TypOvm { get; set; }
    public string DatovkaUradu { get; set; }
    public string NazevUradu { get; set; }
    public string AdresaUradu { get; set; }
    

    public bool Equals(Addr other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Addr)obj);
    }

    public override int GetHashCode()
    {
        return Id;
    }
}