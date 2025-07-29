using System.Collections;
using System.Collections.Generic;
using System;
public class DBManager : Singleton<DBManager>
{
    public List<Entity_Data> Entity_Data_List { private set; get; }
    public List<Unit_Card_Data> Unit_Card_Data_List { private set; get; }

    public IEnumerator Co_Initialize()
    {
        yield return null;
        Entity_Data_List = CSVReader.Load<Entity_Data>("Entity_Data");
        Unit_Card_Data_List = CSVReader.Load<Unit_Card_Data>("Unit_Card_Data");
    }

    public Entity_Data GetEntity_Data(string inName)
    {
        for(int i = 0;i<Entity_Data_List.Count;i++)
        {
            if (Entity_Data_List[i].name.Equals(inName, StringComparison.OrdinalIgnoreCase))
                return Entity_Data_List[i];
        }

        return null;
    }

    public Unit_Card_Data GetUnit_Card_Data(string inName)
    {
        for (int i = 0; i < Unit_Card_Data_List.Count; i++)
        {
            if (Unit_Card_Data_List[i].name.Equals(inName, StringComparison.OrdinalIgnoreCase))
                return Unit_Card_Data_List[i];
        }

        return null;
    }
}
