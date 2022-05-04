namespace KenKen;

internal class Cell
{
    public int? Value { get; private set; }

    public int Id { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }

    public HashSet<int> ImpossibleValues { get; set; }

    public Cage Cage { get; set; }

    public Cell? LeftCell { get; set; }
    public Cell? TopCell { get; set; }
    public Cell? RightCell { get; set; }
    public Cell? BottomCell { get; set; }

    public List<Cell> DependentCells { get; set; }

    public Cell(Cage cage, int id, int row, int column)
    {
        Value = null;
        Id = id;
        Cage = cage;
        Row = row;
        Column = column;
        ImpossibleValues = new HashSet<int>();
        DependentCells = new List<Cell>();

        LeftCell = null;
        TopCell = null;
        RightCell = null;
        BottomCell = null;
    }

    public char CageId => Cage.Id;

    public bool HasSameCageVerticalNeighbors => (TopCell != null && TopCell.CageId == CageId) || 
                                                (BottomCell != null && BottomCell.CageId == CageId);

    public bool HasSameCageHorizontalNeighbors => (LeftCell != null && LeftCell.CageId == CageId) ||
                                                  (RightCell != null && RightCell.CageId == CageId);

    public bool CantBeNumbers(List<int> numbers)
    {
        return numbers.All(CantBeNumber);
    }

    public bool CantBeNumber(int number)
    {
        return ImpossibleValues.Contains(number);
    }

    public bool DoesDependOnCell(Cell cell)
    {
        return DependentCells.Any(c => c.Id == cell.Id);
    }

    public bool IsSolved() => Value.HasValue;

    public void AddDependentCell(Cell cell)
    {
        if (cell.Id == Id)
        {
            return;
        }

        if (DoesDependOnCell(cell))
        {
            return;
        }

        DependentCells.Add(cell);
    }

    public void FindMyDependentCells()
    {
        if (IsSolved())
        {
            DependentCells.Clear();
            return;
        }

        Cell? left = LeftCell;
        Cell? right = RightCell;
        Cell? top = TopCell;
        Cell? bottom = BottomCell;

        HashSet<int> possibleValues = GetPossibleValues();

        while (left != null)
        {
            if (!left.IsSolved())
            {
                HashSet<int> leftPossibleValues = left.GetPossibleValues();
                IEnumerable<int> duplicates = possibleValues.Union(leftPossibleValues);

                if (duplicates.Any())
                {
                    AddDependentCell(left);
                    left.AddDependentCell(this);
                }
            }

            left = left.LeftCell;
        }

        while (right != null)
        {
            if (!right.IsSolved())
            {
                HashSet<int> rightPossibleValues = right.GetPossibleValues();
                IEnumerable<int> duplicates = possibleValues.Union(rightPossibleValues);

                if (duplicates.Any())
                {
                    AddDependentCell(right);
                    right.AddDependentCell(this);
                }
            }

            right = right.RightCell;
        }

        while (top != null)
        {
            if (!top.IsSolved())
            {
                HashSet<int> topPossibleValues = top.GetPossibleValues();
                IEnumerable<int> duplicates = possibleValues.Union(topPossibleValues);

                if (duplicates.Any())
                {
                    AddDependentCell(top);
                    top.AddDependentCell(this);
                }
            }

            top = top.TopCell;
        }

        while (bottom != null)
        {
            if (!bottom.IsSolved())
            {
                HashSet<int> bottomPossibleValues = bottom.GetPossibleValues();
                IEnumerable<int> duplicates = possibleValues.Union(bottomPossibleValues);

                if (duplicates.Any())
                {
                    AddDependentCell(bottom);
                    bottom.AddDependentCell(this);
                }
            }

            bottom = bottom.BottomCell;
        }
    }

    public bool TrySolving()
    {
        if (IsSolved())
        {
            return true;
        }

        HashSet<int> values = GetPossibleValues();

        // if I don't have any more possible values I could be while searching through
        // cell dependencies, then this is a dead end.
        if (values.Count == 0)
        {
            return false;
        }

        // all of my dependent cells have already been solved in this test branch, but I
        // have more than 1 possible value, which is an impossible case.
        if (DependentCells.All(c => c.IsSolved()) && values.Count > 1)
        {
            return false;
        }

        bool success = true;

        foreach (int testValue in values)
        {
            // we are going to try if 'testValue' is a valid value I can be.
            Solve(testValue);
            success = true;

            // by setting my value to 'testValue', I need to tell my dependents that they can
            // no longer be this value. We keep a record of any dependents that already couldn't
            // be this value because if this test failed, we need to then remove 'testValue' from 
            // each dependents list of impossible values, but not if that cell already couldn't be 'testValue'.
            var dependentCellAlreadyCantBeValue = new Dictionary<int, bool>();

            foreach (Cell dependentCell in DependentCells)
            {
                dependentCellAlreadyCantBeValue.Add(dependentCell.Id, dependentCell.CantBeNumber(testValue));
                dependentCell.ImpossibleValues.Add(testValue);
            }

            // for each of my dependets, see if any of them would end up in an impossible state 
            // caused by me setting my value to 'testValue'.
            foreach (Cell dependentCell in DependentCells)
            {
                success = success && dependentCell.TrySolving();
            }

            // all of my dependents are ok with me setting my value to 'testValue', so we are done
            // checking my possible value, I've been solved.
            if (success)
            {
                break;
            }

            // one of my dependents would not work if I changed my value to 'testValue', so unsolve
            // myself by setting my value back to null and inform my dependents that its now
            // possible for them to be 'testValue' again, unless they already couldn't be
            // 'testValue' before running these tests.
            foreach (Cell dependentCell in DependentCells)
            {
                if (!dependentCellAlreadyCantBeValue[dependentCell.Id])
                {
                    dependentCell.ImpossibleValues.Remove(testValue);
                }
            }

            UnSolve();
        }

        return success;
    }

    public HashSet<int> GetPossibleValues()
    {
        if (Value != null)
        {
            return new HashSet<int> { Value.Value };
        }

        return Cage
            .GetAllPossibleNumbers()
            .Except(ImpossibleValues)
            .ToHashSet();
    }

    public void Solve(int value)
    {
        Value = value;
    }

    public void UnSolve()
    {
        Value = null;
    }

    public void SolveIfSinglePossibleValue()
    {
        HashSet<int> possibleValues = GetPossibleValues();

        if (!IsSolved() && possibleValues.Count == 1)
        {
            Value = possibleValues.First();
            RemoveMyValueFromNeighborCells();
        }
    }

    public void RemovePossibleValue(int value)
    {
        ImpossibleValues.Add(value);
        SolveIfSinglePossibleValue();
    }

    public void RemoveMyValueFromNeighborCells()
    {
        if (!IsSolved())
        {
            return;
        }

        Cell? left = LeftCell;
        Cell? right = RightCell;
        Cell? top = TopCell;
        Cell? bottom = BottomCell;

        int value = Value.GetValueOrDefault();

        while (left != null)
        {
            left.RemovePossibleValue(value);
            left = left.LeftCell;
        }

        while (right != null)
        {
            right.RemovePossibleValue(value);
            right = right.RightCell;
        }

        while (top != null)
        {
            top.RemovePossibleValue(value);
            top = top.TopCell;
        }

        while (bottom != null)
        {
            bottom.RemovePossibleValue(value);
            bottom = bottom.BottomCell;
        }
    }
}

