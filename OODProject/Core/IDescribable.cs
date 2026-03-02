namespace OODProject;

public interface IDescribable
{
    public char Glyph{get;}
    public string Description{get;}
    public string Name{get;}
    string Message();
}

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}