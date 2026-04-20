namespace EFlow.Common.Domain;

public abstract record TypedIdValueBase
{
    public Guid Value { get; }
    
    public TypedIdValueBase(Guid value)
    {
        if (value == Guid.Empty)
            throw new InvalidOperationException("Id value cannot be empty");

        Value = value;
    }
}