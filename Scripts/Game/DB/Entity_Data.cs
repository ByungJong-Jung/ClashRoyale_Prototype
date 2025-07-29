using System;
using System.Collections.Generic;

public class Entity_Data
{
    public string name;
    public float attack_damage;
    public float hp;
    public float attack_range;
    public float target_detection_range;
    public float speed;

    public Entity_Data() { }

    public Entity_Data(Dictionary<string, string> data)
    {
        name = data["name"];
        attack_damage = float.Parse(data["attack_damage"]);
        hp = float.Parse(data["hp"]);
        attack_range = float.Parse(data["attack_range"]);
        target_detection_range = float.Parse(data["target_detection_range"]);
        speed = float.Parse(data["speed"]);
    }
}
