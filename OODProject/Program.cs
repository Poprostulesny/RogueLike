// See https://aka.ms/new-console-template for more information

namespace OODProject;

internal class Program
{
    private static void Main(string[] args)
    {
        var input = new KeyboardInput();
        var Engine = new Renderer(input);
        Engine.Play();
    }
}