namespace PantryToPlate.Core.Services;

public interface IDatabaseInitializer
{
    Task Initialized { get; }
    void Start();
}
