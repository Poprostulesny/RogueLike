// See https://aka.ms/new-console-template for more information

namespace OODProject;

class Program
{
    static void Main(string[] args)
    {
        KeyboardInput input = new KeyboardInput();
        Renderer Engine = new Renderer(input);
        Engine.Play();
    }
}