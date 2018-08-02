using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player
{
    public int gold = 10000;
    public int bagSize = 20;
    public int[] itemCount = new int[5];
    public List<Hero> heroList = new List<Hero>();
}
