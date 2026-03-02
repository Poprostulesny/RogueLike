using System.Text;

namespace OODProject;

public interface Field : IDescribable
{

    public bool CanBeEntered
    {
        get; 
    }

   
   
   
    public List<IItem> Items{get;}
    public bool TryAddHero(ref Hero _player, int x, int y);
    public void RemoveHero();
    public  bool TryAddItem(IItem _item);
    public bool TryTakeItem(IItem _item);

    public abstract string[] Display();


}

public class EmptyField() : Field
{
    
    public bool CanBeEntered { get=>true; }
    public List<IItem> Items { get=>_items; }
    public char Glyph { get=>_glyph; }
    public string Description { get=>Message(); }
    public string Name { get=> "Empty Field";  }
    private char _glyph = ' ';
    public  IOccupant? Occupant;
    public Hero? Player;
    public bool isOccupied;
    private List<IItem> _items = new List<IItem>();
    
    public bool TryAddItem(IItem item)
    {
        _items.Add(item);
        UpdateGlyph();
        return true;
    }
    // TODO
    /// <summary>
    /// ma nie byc tu w srodku zadnych itemow ani tego co na nim jest
    /// usunac redundancje
    /// </summary>

    private void UpdateGlyph()
    {
        if (Player != null && Occupant != null)
        {
            _glyph = 'X';
            return;
        }

        if (Occupant != null)
        {
            _glyph = Occupant.Glyph;
            return;
        }

        if (Player != null)
        {
            _glyph = Player.Glyph;
            return;
        }

        if (_items.Count != 0)
        {
            _glyph = _items[0].Glyph;
            return;
        }
        _glyph = ' '; 
        
        
    }
    public bool TryAddOccupant(IOccupant occupant)
    {
        if (Occupant != null)
        {
            return false;
        }
        Occupant = occupant;
        isOccupied = true;
        UpdateGlyph();
        return true;
    }

    public bool TryTakeItem(IItem item)
    {   
       var res =  _items.Remove(item);
       UpdateGlyph();
       return res;
    }

    public string[] Display()
    {
        List<string> disp = new List<string>();
        disp.Add(new string("Player stands on an Empty Field:"));
        if (isOccupied)
        {
            disp.Add(new string($"Enemy: {Occupant.Name}: {Occupant.Description}"));
        }

        if (Items.Count != 0)
        {
            disp.Add(new string("Items:"));
            int cnt = 1;
            foreach (IItem item in Items)
            {
                disp.Add(new string($"- {cnt.ToString()}. {item.Name}: {item.Description}"));
                cnt++;
            }
        }
        return disp.ToArray();
        
    }


    public bool TryAddHero(ref Hero _player, int x, int y)
    {
        Player = _player;
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
}

public class NonEnterableField() : Field
{
  
   
   
    public bool CanBeEntered { get=>false; }
    public List<IItem> Items { get; }

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

    public string[] Display()
    {
        List<String> disp = new List<string>();
        disp.Add(new string("This field is non enterable"));
        return disp.ToArray();
    }

    public char Glyph { get=>'█';  }
    public string Description { get=>"Non Enterable Field";  }
    public string Name { get=>"Non Enterable Field";  }
    public string Message()
    {
        throw new NotImplementedException();
    }
}