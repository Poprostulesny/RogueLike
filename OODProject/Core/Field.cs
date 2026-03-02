using System.Text;

namespace OODProject;

public interface Field : IDescribable
{

    public bool CanBeEntered
    {
        get; 
    }

   
    public  IOccupant? Occupant { get;  }
    public Hero? Player { get;  }
    public bool isOccupied { get; }
    public List<IItem> Items{get;}
    public bool TryAddHero(ref Hero _player, int x, int y);
    public void RemoveHero();
    public  bool TryAddItem(IItem _item);
    public bool TryTakeItem(IItem _item);

    


}

public class EmptyField() : Field
{
    
    public bool CanBeEntered { get=>true; }
     public IOccupant? Occupant { get=>_occupant; }
     public Hero? Player { get=>_player; }
    public bool isOccupied { get=>_isoccupied; }
    public List<IItem> Items { get=>_items; }
    public char Glyph { get=>_glyph; }
    public string Description { get=>Message(); }
    public string Name { get=> "Empty Field";  }
    private char _glyph = ' ';
    private  IOccupant? _occupant;
    private Hero? _player;
    private bool _isoccupied=false;
    
    private List<IItem> _items = new List<IItem>();
    
    public bool TryAddItem(IItem item)
    {
        _items.Add(item);
        UpdateGlyph();
        return true;
    }
  

    private void UpdateGlyph()
    {
        if (_player != null && _occupant != null)
        {
            _glyph = 'X';
            return;
        }

        if (_occupant != null)
        {
            _glyph = _occupant.Glyph;
            return;
        }

        if (_player != null)
        {
            _glyph = _player.Glyph;
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
        if (_occupant != null)
        {
            return false;
        }
        _occupant = occupant;
        _isoccupied = true;
        UpdateGlyph();
        return true;
    }

    public bool TryTakeItem(IItem item)
    {   
       var res =  _items.Remove(item);
       UpdateGlyph();
       return res;
    }

    


    public bool TryAddHero(ref Hero player, int x, int y)
    {
        _player = player;
        _player.ChangePosition(x, y);
        UpdateGlyph();
        return true;
        
    }

    public void RemoveHero()
    {   
        _player = null;
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
    public IOccupant? Occupant { get=>null; }
    public Hero? Player { get=>null; }
    public bool isOccupied { get=>false; }
    public List<IItem> Items { get=>new List<IItem>(); }

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