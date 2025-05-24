using AsciiBinary;

namespace Test;

public static class Program
{
    private static void Run()
    {
        var c = "``]# !G#i$^($$ !O# !W# !_#I5, M#%%>7d>`...g#000000gshake+4fsize-4jexsize+2+4";
        var encoded = AscBin.EncodeReadable([8192]);
        Console.WriteLine(c);
        Console.WriteLine(encoded.Select(v => (char)v).ToArray());
        var d = AscBin.DecodeReadable(c.Select(v => (byte)v).ToArray());
        Console.WriteLine(d);
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