using UnityEngine;
using System.Collections;

public class Enemy : ActItem
{
    public string name;
    public string png;
    public int HP;
    public int str;
    public int def;
    public int exp;
    public int gold;

    public Enemy()
    {
        base.ActorType = -1;
        HP = 50;
        str = 10;
        def = 10;
        exp = 10;
        gold = 10;
    }

    public void setJsonToEnemy(JSONObject jo)
    {
        jo.GetField(ref HP, "HP");
        jo.GetField(ref str, "str");
        jo.GetField(ref def, "def");
        jo.GetField(ref exp, "exp");
        jo.GetField(ref gold, "gold");
    }
}