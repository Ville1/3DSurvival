using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Inventory : IEnumerable<Item> {
    private List<Item> items;

    public float Max_Weight { get; private set; }
    public float Max_Volyme { get; private set; }

    public bool Limited_Weight { get { return Max_Weight > 0.0f; } }
    public bool Limited_Volyme { get { return Max_Volyme > 0.0f; } }

    public Inventory()
    {
        items = new List<Item>();
        Max_Weight = -1.0f;
        Max_Volyme = -1.0f;
    }

    public Inventory(Dictionary<string, int> item_dictionary)
    {
        items = new List<Item>();
        foreach (KeyValuePair<string, int> pair in item_dictionary) {
            for (int i = 0; i < pair.Value; i++) {
                items.Add(ItemPrototypes.Instance.Get_Item(pair.Key));
            }
        }
        Max_Weight = -1.0f;
        Max_Volyme = -1.0f;
    }

    public Inventory(Dictionary<string, int> item_dictionary, float max_weight, float max_volyme)
    {
        items = new List<Item>();
        foreach(KeyValuePair<string, int> pair in item_dictionary) {
            for(int i = 0; i < pair.Value; i++) {
                items.Add(ItemPrototypes.Instance.Get_Item(pair.Key));
            }
        }
        Max_Weight = max_weight;
        Max_Volyme = max_volyme;
    }

    public Inventory Clone()
    {
        Inventory clone = new Inventory();
        foreach (Item item in items) {
            clone.items.Add(ItemPrototypes.Instance.Get_Item(item.Internal_Name));
        }
        return clone;
    }

    public float Current_Weight
    {
        get {
            float total = 0.0f;
            foreach(Item item in items) {
                total += item.Weight;
            }
            return total;
        }
    }

    public float Current_Volyme
    {
        get {
            float total = 0.0f;
            foreach (Item item in items) {
                total += item.Volyme;
            }
            return total;
        }
    }

    public bool Has_Tool(Tool.ToolType type, int level)
    {
        return items.Exists(x => x is Tool && (x as Tool).Type == type && (x as Tool).Level >= level);
    }

    public Tool Get_Tool(Tool.ToolType type, int level)
    {
        return (Tool)items.OrderByDescending(x => x is Tool ? (x as Tool).Level : 0).FirstOrDefault(x => x is Tool && (x as Tool).Type == type && (x as Tool).Level >= level);
    }

    public bool Can_Fit(Item item)
    {
        return (!Limited_Weight || item.Weight <= 0.0f || (Current_Weight + item.Weight <= Max_Weight)) || (!Limited_Volyme || item.Volyme <= 0.0f || (Current_Volyme + item.Volyme <= Max_Volyme));
    }

    public void Add(Item item)
    {
        items.Add(item);
    }

    public void Remove(Item item)
    {
        items.Remove(item);
    }

    public bool Is_Empty
    {
        get {
            return items.Count == 0;
        }
    }

    public IEnumerator<Item> GetEnumerator()
    {
        return items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public Dictionary<string, int> Count_Dictionary_Names
    {
        get {
            Dictionary<string, int> counts = new Dictionary<string, int>();
            foreach (Item item in items) {
                if (!counts.ContainsKey(item.Name)) {
                    counts.Add(item.Name, 1);
                } else {
                    counts[item.Name] = counts[item.Name] + 1;
                }
            }
            return counts;
        }
    }

    public Dictionary<string, int> Count_Dictionary_Internal_Names
    {
        get {
            Dictionary<string, int> counts = new Dictionary<string, int>();
            foreach (Item item in items) {
                if (!counts.ContainsKey(item.Internal_Name)) {
                    counts.Add(item.Internal_Name, 1);
                } else {
                    counts[item.Internal_Name] = counts[item.Internal_Name] + 1;
                }
            }
            return counts;
        }
    }

    public string Parse_Text(bool plus_signs)
    {
        if (Is_Empty) {
            return "Empty";
        }
        StringBuilder builder = new StringBuilder();
        foreach(KeyValuePair<string, int> pair in Count_Dictionary_Names) {
            builder.Append(pair.Key).Append(": ");
            if (plus_signs) {
                builder.Append("+");
            }
            builder.Append(pair.Value).Append(", ");
        }
        return builder.Remove(builder.Length - 2, 2).ToString();
    }
}
