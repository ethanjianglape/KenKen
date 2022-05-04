using System.Text.RegularExpressions;

namespace KenKen;

internal class KenKen
{
    private int CellId;

    public int Size { get; set; }

    public List<Cage> Cages { get; set; }

    public List<Cell> Cells { get; set; }

    public KenKen()
    {
        Cages = new List<Cage>();
        Cells = new List<Cell>();
        CellId = 0;
    }

    public bool IsSolved() => Cages.All(c => c.IsSolved());

    public Cage? TryGetCage(char id) => Cages.FirstOrDefault(c => c.Id == id);

    public Cell? TryGetCell(int row, int column) => Cells.FirstOrDefault(c => c.Row == row && c.Column == column);

    public void ParseBoard(IEnumerable<string> board)
    {
        if (!board.Any())
        {
            return;
        }

        string firstLine = board.First();

        Size = firstLine.Length;

        int row = 0;

        foreach (string line in board.Take(Size))
        {
            ParseBoardLine(line, row);

            row++;
        }

        foreach (string line in board.Skip(Size))
        {
            ParseBoardRules(line);
        }

        Cell firstCell = Cells.First();

        SetCellNeighbors(firstCell);
        CalculateCagePossibleNumberCombinations();
    }

    private void ParseBoardLine(string line, int row)
    {
        int column = 0;

        foreach (char id in line)
        {
            Cage? cage = TryGetCage(id);

            if (cage == null)
            {
                cage = new Cage(id, Size);
                Cages.Add(cage);
            }

            Cell cell = new(cage, CellId, row, column);
            
            cage.Cells.Add(cell);
            Cells.Add(cell);

            column++;
            CellId++;
        }
    }

    private void ParseBoardRules(string line)
    {
        string[] rules = Regex.Split(line, @"\s");

        if (rules.Length != 3)
        {
            return;
        }

        char id = char.Parse(rules[0]);
        int value = int.Parse(rules[1]);
        CageOperation operation = StringToCageOperation(rules[2]);

        Cage? cage = TryGetCage(id);

        if (cage == null)
        {
            throw new Exception();
        }

        cage.TargetValue = value;
        cage.Operation = operation;
    }

    private static CageOperation StringToCageOperation(string operation)
    {
        if (operation == ".")
        {
            return CageOperation.None;
        }

        if (operation == "-")
        {
            return CageOperation.Subtraction;
        }

        if (operation == "+")
        {
            return CageOperation.Addition;
        }

        if (operation == "*")
        {
            return CageOperation.Multiplication;
        }

        if (operation == "/")
        {
            return CageOperation.Division;
        }

        return CageOperation.None;
    }

    private void SetCellNeighbors(Cell cell)
    {
        int row = cell.Row;
        int column = cell.Column;

        Cell? left = TryGetCell(row, column - 1);
        Cell? right = TryGetCell(row, column + 1);
        Cell? top = TryGetCell(row - 1, column);
        Cell? bottom = TryGetCell(row + 1, column);

        if (left != null && cell.LeftCell == null)
        {
            cell.LeftCell = left;
            left.RightCell = cell;

            SetCellNeighbors(left);
        }

        if (right != null && cell.RightCell == null)
        {
            cell.RightCell = right;
            right.LeftCell = cell;

            SetCellNeighbors(right);
        }

        if (top != null && cell.TopCell == null)
        {
            cell.TopCell = top;
            top.BottomCell = cell;

            SetCellNeighbors(top);
        }

        if (bottom != null && cell.BottomCell == null)
        {
            cell.BottomCell = bottom;
            bottom.TopCell = cell;

            SetCellNeighbors(bottom);
        }
    }

    private void CalculateCagePossibleNumberCombinations()
    {
        foreach (Cage cage in Cages)
        {
            cage.CalculatePossibleNumberCombinations();
        }
    }

    public void Solve()
    {
        SolveDefaultCages();
        FindDependentCells();
        SolveDependentCells();
    }

    private void SolveDefaultCages()
    {
        foreach (Cage cage in Cages)
        {
            cage.SolveIfDefaultCage();
        }
    }

    private void FindDependentCells()
    {
        foreach (Cell cell in Cells)
        {
            cell.FindMyDependentCells();
        }
    }

    private void SolveDependentCells()
    {
        IEnumerable<Cell> cellsWithDependents = Cells.Where(c => c.DependentCells.Count > 0);

        foreach (Cell cell in cellsWithDependents)
        {
            cell.TrySolving();
        }
    }

    public void Print()
    {
        for (int row = 0; row < Size; row++)
        {
            for (int col = 0; col < Size; col++)
            {
                Cell cell = Cells.First(c => c.Row == row && c.Column == col);

                Console.Write($"{cell.Value.GetValueOrDefault()}\t");
            }

            Console.WriteLine();
            Console.WriteLine();
        }
    }
}

