namespace KenKen;

internal class Cage
{
    public char Id { get; set; }
    public int TargetValue { get; set; }
    public int MaxCellValue { get; set; }

    public CageOperation Operation { get; set; }

    public List<Cell> Cells { get; set; }

    public List<List<int>> PossibleNumberCombinations { get; set; }

    public Cage(char id, int maxCellValue)
    {
        Id = id;
        MaxCellValue = maxCellValue;
        Cells = new List<Cell>();
        PossibleNumberCombinations = new List<List<int>>();
    }

    private bool IsDefaultCage() => Operation == CageOperation.None;
    
    public bool IsComplexCage() => Cells.Any(c => c.HasSameCageHorizontalNeighbors && c.HasSameCageVerticalNeighbors);

    public bool CanContainDuplicateNumbers() => IsComplexCage();

    public bool IsSolved() => Cells.All(c => c.IsSolved());

    public IEnumerable<int> GetAllPossibleNumbers()
    {
        return PossibleNumberCombinations.Where(numbers =>
        {
            foreach (Cell cell in Cells)
            {
                if (cell.CantBeNumbers(numbers))
                {
                    return false;
                }

                if (cell.IsSolved() && numbers.All(n => n != cell.Value))
                {
                    return false;
                }
            }

            if (numbers.Any(AllCellsCantBeNumber))
            {
                return false;
            }

            return true;
        }).SelectMany(l => l);
    }

    public bool AllCellsCantBeNumber(int number) => Cells.All(cell => cell.CantBeNumber(number));

    public void SolveIfDefaultCage()
    {
        if (!IsDefaultCage())
        {
            return;
        }

        foreach (Cell cell in Cells)
        {
            cell.Solve(TargetValue);
            cell.RemoveMyValueFromNeighborCells();
        }
    }

    public void CalculatePossibleNumberCombinations()
    {
        PossibleNumberCombinations.Clear();

        int min = 1;
        int max = MaxCellValue;
        int countOfNumbersNeeded = Cells.Count;

        bool canContainDuplicates = CanContainDuplicateNumbers();

        List<List<int>> possibleNumberPermutations = GetAllPermutationsOfSize(min, max, countOfNumbersNeeded);

        foreach (List<int> possibleNumberPermutation in possibleNumberPermutations)
        {
            bool hasDuplicates = possibleNumberPermutation.Distinct().Count() != possibleNumberPermutation.Count;

            if (!hasDuplicates || (hasDuplicates && canContainDuplicates))
            {
                int total = CalculateNumbersWithOperation(possibleNumberPermutation);

                if (total == TargetValue && !DoesAvailableNumbersContainSequence(possibleNumberPermutation))
                {
                    PossibleNumberCombinations.Add(possibleNumberPermutation);
                }
            }
        }
    }

    private static List<List<int>> GetAllPermutationsOfSize(int min, int max, int size)
    {
        List<List<int>> permutations = new();
        List<int> possibleNumbers = Enumerable.Range(min, max).ToList();
        List<int> possibleNumberIndexes = Enumerable.Repeat(0, size).ToList();

        bool searching = true;

        while (searching)
        {
            List<int> permutation = possibleNumberIndexes.Select(index => possibleNumbers[index]).ToList();

            permutation.Sort();
            permutations.Add(permutation);

            for (int i = 0; i < possibleNumberIndexes.Count; i++)
            {
                possibleNumberIndexes[i]++;

                if (possibleNumberIndexes[i] >= max)
                {
                    if (i == size - 1)
                    {
                        searching = false;
                    }

                    possibleNumberIndexes[i] = 0;
                }
                else
                {
                    break;
                }
            }
        }

        return permutations;
    }

    private int CalculateNumbersWithOperation(List<int> numbers)
    {
        return Operation switch
        {
            CageOperation.Addition => CalculateAddition(numbers),
            CageOperation.Multiplication => CalculateMultiplication(numbers),
            CageOperation.Subtraction => CalculateSubraction(numbers),
            CageOperation.Division => CalculateDivision(numbers),
            _ => -1,
        };
    }

    private static int CalculateAddition(List<int> numbers)
    {
        return numbers.Sum();
    }

    private static int CalculateSubraction(List<int> numbers)
    {
        int total = numbers.Max();
        int index = numbers.IndexOf(total);

        for (int i = 0; i < numbers.Count; i++)
        {
            if (i != index)
            {
                total -= numbers[i];
            }
        }

        return total;
    }

    private static int CalculateMultiplication(List<int> numbers)
    {
        int total = 1;

        foreach (int number in numbers)
        {
            total *= number;
        }

        return total;
    }

    private static int CalculateDivision(List<int> numbers)
    {
        int max = numbers.Max();
        int index = numbers.IndexOf(max);

        double total = max;

        for (int i = 0; i < numbers.Count; i++)
        {
            if (i != index)
            {
                total /= numbers[i];
            }
        }

        if (total == (int)total)
        {
            return (int)total;
        }

        return -1;
    }

    private bool DoesAvailableNumbersContainSequence(List<int> numbers)
    {
        foreach (List<int> availableNumbers in PossibleNumberCombinations)
        {
            bool match = true;

            for (int i = 0; i < numbers.Count; i++)
            {
                if (numbers[i] != availableNumbers[i])
                {
                    match = false;
                }
            }

            if (match)
            {
                return true;
            }
        }

        return false;
    }
}
