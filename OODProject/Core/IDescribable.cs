namespace OODProject.Core;

public interface IDescribable
{
    public char Glyph { get; }
    public string Description { get; }
    public string Name { get; }
}

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}