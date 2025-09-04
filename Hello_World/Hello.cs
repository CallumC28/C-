using System;

namespace Hello_World //namespace is a way to organise code
{
    class HelloWorld
    {
        static void Main()
        {
            Console.WriteLine("Hello"); //writes the text in the console/terminal
            Console.Write("World"); //writes the text in the console/terminal without a new line
            Console.Write("!"); //writes without a new line
            Console.WriteLine("My name is Callum\nCummins"); //writes with a new line and \n creates a new line
        }
    }
}
//to run the program do: dotnet run