namespace OODProject;

public interface Field : IDescribable
{
    public bool CanBeEntered { get; }


    public IOccupant? Occupant { get; }
    public Hero? Player { get; }
    public bool isOccupied { get; }
    public List<IItem> Items { get; }
    public bool TryAddHero(ref Hero _player, int x, int y);
    public void RemoveHero();
    public bool TryAddItem(IItem _item);
    public bool TryTakeItem(IItem _item);
}

public class EmptyField : Field
{
    public bool CanBeEntered => true;
    public IOccupant? Occupant { get; private set; }

    public Hero? Player { get; private set; }

    public bool isOccupied { get; private set; }

    public List<IItem> Items { get; } = new();

    public char Glyph { get; private set; } = ' ';

    public string Description => Message();
    public string Name => "Empty Field";

    public bool TryAddItem(IItem item)
    {
        Items.Add(item);
        UpdateGlyph();
        return true;
    }

    public bool TryTakeItem(IItem item)
    {
        var res = Items.Remove(item);
        UpdateGlyph();
        return res;
    }


    public bool TryAddHero(ref Hero player, int x, int y)
    {
        Player = player;
        Player.ChangePosition(x, y);
        UpdateGlyph();
        return true;
    }

    public void RemoveHero()
    {
        Player = null;
        UpdateGlyph();
    }


    public string Message()
    {
        throw new NotImplementedException();
    }


    private void UpdateGlyph()
    {
        if (Player != null && Occupant != null)
        {
            Glyph = 'X';
            return;
        }

        if (Occupant != null)
        {
            Glyph = Occupant.Glyph;
            return;
        }

        if (Player != null)
        {
            Glyph = Player.Glyph;
            return;
        }

        if (Items.Count != 0)
        {
            Glyph = Items[0].Glyph;
            return;
        }

        Glyph = ' ';
    }

    public bool TryAddOccupant(IOccupant occupant)
    {
        if (Occupant != null) return false;
        Occupant = occupant;
        isOccupied = true;
        UpdateGlyph();
        return true;
    }
}

public class NonEnterableField : Field
{
    public bool CanBeEntered => false;
    public IOccupant? Occupant => null;
    public Hero? Player => null;
    public bool isOccupied => false;
    public List<IItem> Items => new();

    public bool TryAddHero(ref Hero _player, int x, int y)
    {
        return false;
    }

    public void RemoveHero()
    {
    }

    public bool TryAddItem(IItem _item)
    {
        return false;
    }

    public bool TryTakeItem(IItem _item)
    {
        return false;
    }

    public char Glyph => '█';
    public string Description => "Non Enterable Field";
    public string Name => "Non Enterable Field";

    public string Message()
    {
        throw new NotImplementedException();
    }

    public string[] Display()
    {
        var disp = new List<string>();
        disp.Add(new string("This field is non enterable"));
        return disp.ToArray();
    }
}