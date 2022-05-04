string fileName = "boards/kenken3.txt";

if (!File.Exists(fileName))
{
    throw new FileNotFoundException(fileName);
}

string[] lines = File.ReadAllLines(fileName);

if (lines.Length == 0)
{
    throw new Exception("Empty File!");
}

KenKen.KenKen kenken = new();

kenken.ParseBoard(lines);
kenken.Solve();
kenken.Print();

Console.ReadKey();