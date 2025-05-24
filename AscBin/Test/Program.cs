using AsciiBinary;

namespace Test;

public static class Program
{
    private static void Run()
    {
        var c = "一些非 ascii 字符串";
        var encoded = AscBin.EncodeReadable(c);
        Console.WriteLine(c);
        Console.WriteLine(encoded.Select(v => (char)v).ToArray());
        Console.WriteLine(AscBin.DecodeReadable(encoded));
    }

    private static void Main()
    {
        try
        {
            Run();
        }
        catch (Exception e)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e);
            Console.ForegroundColor = color;
        }
    }
}