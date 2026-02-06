namespace EFlow.Common.Domain;

public interface IBusinessRule
{
    public string Message { get; }

    public bool IsBroken();
}