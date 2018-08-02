using UnityEngine;
using System.Collections;

public class GameDataManager : MonoBehaviour
{

    private int itemCount = 5;

    GameManager GM;

    void Start()
    {
        GM = transform.GetComponent<GameManager>();
    }
    //根据文档名保存存档，自动存档为auto，其他为数字编号
    public void SaveData(string dataName)
    {
        JSONObject gameData = new JSONObject(JSONObject.Type.OBJECT);
        gameData.AddField("level", GM.level);
        gameData.AddField("gold", GM.player.gold);
        for (int i = 0; i < itemCount; i++)
        {
            gameData.AddField("item" + i + "Count", GM.player.itemCount[i]);
        }
        JSONObject heroListData = new JSONObject(JSONObject.Type.OBJECT);
        for (int i = 0; i < GM.player.heroList.Count; i++)
        {
            JSONObject heroData = new JSONObject(JSONObject.Type.OBJECT);
            heroData.AddField("job", (int)GM.player.heroList[i].job);
            heroData.AddField("level", GM.player.heroList[i].level);
            heroData.AddField("exp", GM.player.heroList[i].exp);
            heroData.AddField("HPMax", GM.player.heroList[i].HPMax);
            heroData.AddField("MPMax", GM.player.heroList[i].MPMax);
            heroData.AddField("str", GM.player.heroList[i].str);
            heroData.AddField("mag", GM.player.heroList[i].mag);
            heroData.AddField("def", GM.player.heroList[i].def);
            heroListData.AddField("hero_" + i, heroData);
        }
        gameData.AddField("heroList", heroListData);
        PlayerPrefs.SetString("gameData_" + dataName, gameData.ToString());
    }
    //根据文档名读取存档，自动存档为auto，其他为数字编号
    public void LoadData(string dataName)
    {
        if (PlayerPrefs.HasKey("gameData_" + dataName))
        {
            string gameData = PlayerPrefs.GetString("gameData_" + dataName);
            JSONObject j = new JSONObject(gameData);
            j.GetField(ref GM.level, "level");
            j.GetField(ref GM.player.gold, "gold");
            for (int i = 0; i < itemCount; i++)
            {
                j.GetField(ref GM.player.itemCount[i], "item" + i + "Count");
            }
            int heroListCount = j.GetField("heroList").Count;
            for (int i = 0; i < heroListCount; i++)
            {
                JSONObject heroData = j.GetField("heroList")[i];
                int jobID = 0;
                heroData.GetField(ref jobID, "job");
                Hero h = new Hero((Hero.Job)jobID);
                //实例化类时，已自动修改job值
                //h.job = (Hero.Job)jobID;
                heroData.GetField(ref h.level, "level");
                heroData.GetField(ref h.exp, "exp");
                heroData.GetField(ref h.HPMax, "HPMax");
                heroData.GetField(ref h.MPMax, "MPMax");
                heroData.GetField(ref h.str, "str");
                heroData.GetField(ref h.mag, "mag");
                heroData.GetField(ref h.def, "def");
                h.HP = h.HPMax;
                h.MP = h.MPMax;
                GM.player.heroList.Add(h);
            }
        }
    }
    //读取自动存档
    public void LoadAutoData()
    {
        LoadData("auto");
    }
    //保存自动存档
    public void SaveAutoData()
    {
        SaveData("auto");
    }

}
