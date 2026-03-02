namespace OODProject;

public  abstract class IInput
{
   public abstract void TakeInput(ref GameWorld gameWorld);
}

public static class KeyboardInput : IInput
{
    public override void  TakeInput(ref GameWorld gameWorld)
    {}
    
}