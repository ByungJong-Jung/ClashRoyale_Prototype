using System;
using System.Collections.Generic;

public class Unit_Card_Data
{
    public string name;
    public int elixir;

    public Unit_Card_Data() { }

    public Unit_Card_Data(Dictionary<string, string> data)
    {
        name = data["name"];
        elixir = int.Parse(data["elixir"]);
    }
}
