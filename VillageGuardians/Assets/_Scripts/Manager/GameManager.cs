using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
public class GameManager : MonoBehaviour
{
    public Player player;
    /// <summary>
    /// 游戏状态，共有两种，1.村庄中；2.战斗中
    /// </summary>
    public enum GameState
    {
        Village,
        Battle
    }
    BattleManager BM;
    GameDataManager GDM;
    /// <summary>
    /// 公用的列表项，不同状态加载不同列表
    /// </summary>
    public RectTransform PanelList;
    public RectTransform PanelListContainer;
    public RectTransform PanelListViewer;
    /// <summary>
    /// 列表项的返回按钮
    /// </summary>
    public RectTransform ButtonBack;
    /// <summary>
    /// 获取用于生成列表的 Prefab
    /// </summary>
    public GameObject ListItem;
    /// <summary>
    /// 获取滚动条，当数量小于6时不显示，反之
    /// </summary>
    public GameObject PanelListScrollbar;
    //获取 Menu 面板和 List 面板，用于切换动画
    public RectTransform PanelMenu;
    //定义JSON Object变量
    public JSONObject JSONO;
    private string dataJSON;
    //当前列表
    private GameObject[] currentDataList;
    //存储位置
    public GameObject dataList;
    public GameObject showList;
    //定义存储
    private Item[] itemList;
    private GameObject[] equipmentList;
    private GameObject[] magicList;
    private GameObject[] jobList;
    private GameObject[] enemyList;
    private Vector2 leftPosition = new Vector2(-480f, -65f);
    private Vector2 centerPosition = new Vector2(0, -65f);
    private Vector2 rightPosition = new Vector2(480f, -65f);
    public GameObject Village;
    public GameObject Fight;
    public GameObject Canvas;
    Text Text_Gold;
    Text Text_Day;
    enum ItemType
    {
        Item,
        Equipment,
        Magic,
        Job
    }
    public int level;
    public int[][] enemyIDList;
    private void Awake()
    {
        level = 0;
        player = new Player();
        BM = transform.GetComponent<BattleManager>();
        GDM = transform.GetComponent<GameDataManager>();
        TextAsset dataFile = Resources.Load("dataFile") as TextAsset;
        dataJSON = dataFile.text;
        JSONO = new JSONObject(dataJSON);
        if (PlayerPrefs.HasKey("load"))
        {
            if (PlayerPrefs.GetInt("load") == 1)
            {
                Debug.Log("load");
                PlayerPrefs.DeleteKey("load");
                GDM.LoadAutoData();
            }
        }
        if (player.heroList.Count == 0)
        {
            player.heroList.Add(new Hero(Hero.Job.Hero));
        }
        createItemList();
        createEquipmentList();
        createMagicList();
        createJobList();
        Canvas = GameObject.Find("Canvas");
        Village = Canvas.transform.Find("Panel_Village").gameObject;
        Village.SetActive(true);
        Fight = Canvas.transform.Find("Panel_Fight").gameObject;
        Fight.SetActive(false);
        Text_Gold = Village.transform.Find("TopBanner/Text_Gold").GetComponent<Text>();
        Text_Gold.text = player.gold + " G";
        Text_Day = Village.transform.Find("TopBanner/Text_Day").GetComponent<Text>();
        Text_Day.text = "第 " + (level + 1).ToString() + " 天";
        generateEnemyList(level);
    }
    private void Start()
    {
        //currentDataList = getList("item");
        //addItemToList(currentDataList);
    }
    private void Update()
    {
    }
    //创建列表项
    public void CreateItem()
    {
        Transform newItem = Instantiate(ListItem).transform;
        newItem.SetParent(PanelListContainer);
    }
    //打开列表
    public void openList(string type = "")
    {
        getList(type);
        //如果列表数目小于6
        if (PanelListContainer.childCount < 6)
        {
            //设置返回按钮的位置，跟在随后一个下面
            ButtonBack.anchoredPosition = new Vector3(0, -125f - (PanelListContainer.childCount - 1) * 83);
            //根据数目设置各Panel大小
            PanelList.sizeDelta = new Vector2(464f, (PanelListContainer.childCount + 1) * 83f - 3f);
            PanelListContainer.sizeDelta = new Vector2(464f, PanelListContainer.childCount * 83f);
            PanelListViewer.sizeDelta = PanelListContainer.sizeDelta;
            //隐藏滚动条
            PanelListScrollbar.SetActive(false);
        }
        //反之
        else
        {
            //固定在第6个的位置
            ButtonBack.anchoredPosition = new Vector3(0, -125f - 4 * 83);
            //根据数目设置各Panel大小
            PanelList.sizeDelta = new Vector2(464f, 500f);
            PanelListContainer.sizeDelta = new Vector2(464f, PanelListContainer.childCount * 83f);
            PanelListViewer.sizeDelta = new Vector2(464f, 5 * 83f);
            //显示滚动条
            PanelListScrollbar.SetActive(true);
        }
        //缓动动画
        DOTween.To(() => PanelMenu.anchoredPosition, x => PanelMenu.anchoredPosition = x, leftPosition, 0.2f);
        DOTween.To(() => PanelList.anchoredPosition, x => PanelList.anchoredPosition = x, centerPosition, 0.2f);
    }
    /// <summary>
    /// 关闭列表
    /// </summary>
    public void openMenu()
    {
        removeItemToData();
        //缓动动画
        DOTween.To(() => PanelMenu.anchoredPosition, x => PanelMenu.anchoredPosition = x, centerPosition, 0.2f);
        DOTween.To(() => PanelList.anchoredPosition, x => PanelList.anchoredPosition = x, rightPosition, 0.2f);
    }
    /// <summary>
    /// 得到 item 列表
    /// </summary>
    public void createItemList()
    {
        int x = JSONO.GetField("item").Count;
        itemList = new Item[x];
        for (int i = 0; i < x; i++)
        {
            Item item = new Item();
            GameObject newItem = Instantiate(ListItem);
            newItem.transform.SetParent(dataList.transform, false);
            newItem.name = "item" + i.ToString();
            Button btn = newItem.GetComponent<Button>();
            int index = i;
            btn.onClick.AddListener(() =>
            {
                buy(ItemType.Item, index);
            });
            item.GameObject = newItem;
            JSONO.GetField("item")[i].GetField(ref item.name, "name");
            JSONO.GetField("item")[i].GetField(ref item.png, "png");
            JSONO.GetField("item")[i].GetField(ref item.effect, "effect");
            JSONO.GetField("item")[i].GetField(ref item.gold, "gold");
            JSONO.GetField("item")[i].GetField(ref item.cure, "cure");
            JSONO.GetField("item")[i].GetField(ref item.comment, "comment");
            string text_desc = item.name + "\n" + item.comment;
            string text_info = "数量 " + player.itemCount[i].ToString() + "\n" + item.gold.ToString() + " G";
            Text Text_desc = newItem.transform.Find("Text_desc").GetComponent<Text>();
            Text_desc.text = text_desc;
            Text Text_info = newItem.transform.Find("Text_info").GetComponent<Text>();
            Text_info.text = text_info;
            Image IconImage = newItem.transform.Find("Icon/Image").GetComponent<Image>();
            Sprite Icon = Resources.Load<Sprite>(item.png);
            IconImage.sprite = Icon;
            IconImage.preserveAspect = true;
            itemList[i] = item;
        }
    }
    private void buy(ItemType it, int index)
    {
        switch (it)
        {
            case ItemType.Item:
                int itemCount = 0;
                for (int i = 0; i < player.itemCount.Length - 1; i++)
                {
                    itemCount += player.itemCount[i];
                }
                if (itemCount >= player.bagSize && index != 4)
                {
                    return;
                }
                if (player.gold >= itemList[index].gold)
                {
                    player.itemCount[index] += 1;
                    player.gold -= itemList[index].gold;
                    string text_info = "数量 " + player.itemCount[index].ToString() + "\n" + itemList[index].gold.ToString() + " G";
                    Text Text_info = itemList[index].GameObject.transform.Find("Text_info").GetComponent<Text>();
                    Text_info.text = text_info;
                    Text_Gold.text = player.gold + " G";
                }
                if (index == 4)
                {
                    player.bagSize = 20 + player.itemCount[4] * 20;
                }
                break;
            case ItemType.Equipment:
                break;
            case ItemType.Magic:
                break;
            case ItemType.Job:
                if (player.heroList.Count < 4)
                {
                    int price = 0;
                    JSONO.GetField("job")[index].GetField(ref price, "gold");
                    if (player.gold >= price)
                    {
                        player.heroList.Add(new Hero((Hero.Job)index));
                        player.gold -= price;
                        Text_Gold.text = player.gold + " G";
                        GDM.SaveAutoData();
                    }
                }
                break;
            default:
                break;
        }
    }
    #region 获取 weapon 和 armor 列表
    /// <summary>
    /// 获取 weapon 和 armor 列表
    /// </summary>
    public void createEquipmentList()
    {
        int x = JSONO.GetField("weapon").Count;
        int y = JSONO.GetField("armor").Count;
        equipmentList = new GameObject[x + y];
        //weapon
        string name = "";
        string png = "";
        string effect = "";
        string comment = "";
        int gold = 0;
        int attack = 0;
        for (int i = 0; i < x; i++)
        {
            GameObject newItem = Instantiate(ListItem);
            newItem.transform.SetParent(dataList.transform);
            newItem.transform.localScale = new Vector3(1, 1, 1);
            newItem.name = "weapon" + i.ToString();
            JSONO.GetField("weapon")[i].GetField(ref name, "name");
            JSONO.GetField("weapon")[i].GetField(ref png, "png");
            JSONO.GetField("weapon")[i].GetField(ref effect, "effect");
            JSONO.GetField("weapon")[i].GetField(ref gold, "gold");
            JSONO.GetField("weapon")[i].GetField(ref attack, "attack");
            JSONO.GetField("weapon")[i].GetField(ref comment, "comment");
            string text_desc = name + "\n" + comment;
            string text_info = "\n" + gold.ToString() + " G";
            Text Text_desc = GameObject.Find(newItem.name + "/Text_desc").GetComponent<Text>();
            Text_desc.text = text_desc;
            Text Text_info = GameObject.Find(newItem.name + "/Text_info").GetComponent<Text>();
            Text_info.text = text_info;
            Image IconImage = GameObject.Find(newItem.name + "/Icon/Image").GetComponent<Image>();
            Sprite Icon = Resources.Load<Sprite>(png);
            IconImage.sprite = Icon;
            IconImage.preserveAspect = true;
            equipmentList[i] = newItem;
        }
        //armor
        name = "";
        png = "";
        effect = "";
        comment = "";
        gold = 0;
        int defense = 0;
        for (int i = 0; i < y; i++)
        {
            GameObject newItem = Instantiate(ListItem);
            newItem.transform.SetParent(dataList.transform);
            newItem.transform.localScale = new Vector3(1, 1, 1);
            newItem.name = "armor" + i.ToString();
            JSONO.GetField("armor")[i].GetField(ref name, "name");
            JSONO.GetField("armor")[i].GetField(ref png, "png");
            JSONO.GetField("armor")[i].GetField(ref effect, "effect");
            JSONO.GetField("armor")[i].GetField(ref gold, "gold");
            JSONO.GetField("armor")[i].GetField(ref defense, "defense");
            JSONO.GetField("armor")[i].GetField(ref comment, "comment");
            string text_desc = name + "\n" + comment;
            string text_info = "\n" + gold.ToString() + " G";
            Text Text_desc = GameObject.Find(newItem.name + "/Text_desc").GetComponent<Text>();
            Text_desc.text = text_desc;
            Text Text_info = GameObject.Find(newItem.name + "/Text_info").GetComponent<Text>();
            Text_info.text = text_info;
            Image IconImage = GameObject.Find(newItem.name + "/Icon/Image").GetComponent<Image>();
            Sprite Icon = Resources.Load<Sprite>(png);
            IconImage.sprite = Icon;
            IconImage.preserveAspect = true;
            equipmentList[i + x] = newItem;
        }
    }
    #endregion 获取 weapon 和 armor 列表
    #region 得到 Magic 列表
    /// <summary>
    /// 得到 Magic 列表
    /// </summary>
    public void createMagicList()
    {
        int x = JSONO.GetField("magic").Count;
        magicList = new GameObject[x];
        string name = "";
        string png = "";
        string sound = "";
        string comment = "";
        int gold = 0;
        float damage = 0;
        int costMP = 0;
        bool all = false;
        for (int i = 0; i < x; i++)
        {
            GameObject newItem = Instantiate(ListItem);
            newItem.transform.SetParent(dataList.transform);
            newItem.transform.localScale = new Vector3(1, 1, 1);
            newItem.name = "magic" + i.ToString();
            JSONO.GetField("magic")[i].GetField(ref name, "name");
            JSONO.GetField("magic")[i].GetField(ref png, "png");
            JSONO.GetField("magic")[i].GetField(ref sound, "sound");
            JSONO.GetField("magic")[i].GetField(ref gold, "gold");
            JSONO.GetField("magic")[i].GetField(ref damage, "damage");
            JSONO.GetField("magic")[i].GetField(ref costMP, "costMP");
            JSONO.GetField("magic")[i].GetField(ref all, "all");
            JSONO.GetField("magic")[i].GetField(ref comment, "comment");
            string text_desc = name + "\n" + comment;
            string text_info = "\n" + gold.ToString() + " G";
            Text Text_desc = GameObject.Find(newItem.name + "/Text_desc").GetComponent<Text>();
            Text_desc.text = text_desc;
            Text Text_info = GameObject.Find(newItem.name + "/Text_info").GetComponent<Text>();
            Text_info.text = text_info;
            Image IconImage = GameObject.Find(newItem.name + "/Icon/Image").GetComponent<Image>();
            Sprite Icon = Resources.Load<Sprite>("images/magic/book");
            IconImage.sprite = Icon;
            IconImage.preserveAspect = true;
            magicList[i] = newItem;
        }
    }
    #endregion 得到 Magic 列表
    #region 得到 Job 列表
    /// <summary>
    /// 得到 Job 列表
    /// </summary>
    public void createJobList()
    {
        int x = JSONO.GetField("job").Count;
        jobList = new GameObject[x];
        string name = "";
        string png = "";
        int gold = 0;
        float baseHP = 0;
        float baseMP = 0;
        float baseStr = 0;
        float baseMag = 0;
        float baseDef = 0;
        for (int i = 0; i < x; i++)
        {
            GameObject newItem = Instantiate(ListItem);
            newItem.transform.SetParent(dataList.transform);
            newItem.transform.localScale = new Vector3(1, 1, 1);
            newItem.name = "job" + i.ToString();
            Button btn = newItem.GetComponent<Button>();
            int index = i;
            btn.onClick.AddListener(() =>
            {
                buy(ItemType.Job, index);
            });
            JSONO.GetField("job")[i].GetField(ref name, "name");
            JSONO.GetField("job")[i].GetField(ref png, "png");
            JSONO.GetField("job")[i].GetField(ref gold, "gold");
            JSONO.GetField("job")[i].GetField(ref baseHP, "baseHP");
            JSONO.GetField("job")[i].GetField(ref baseMP, "baseMP");
            JSONO.GetField("job")[i].GetField(ref baseStr, "baseStr");
            JSONO.GetField("job")[i].GetField(ref baseMag, "baseMag");
            JSONO.GetField("job")[i].GetField(ref baseDef, "baseDef");
            string text_desc = name + "\n";
            string text_info = "\n" + gold.ToString() + " G";
            Text Text_desc = GameObject.Find(newItem.name + "/Text_desc").GetComponent<Text>();
            Text_desc.text = text_desc;
            Text Text_info = GameObject.Find(newItem.name + "/Text_info").GetComponent<Text>();
            Text_info.text = text_info;
            Image IconImage = GameObject.Find(newItem.name + "/Icon/Image").GetComponent<Image>();
            Sprite Icon = Resources.Load<Sprite>(png);
            IconImage.sprite = Icon;
            IconImage.preserveAspect = true;
            jobList[i] = newItem;
        }
    }
    #endregion 得到 Job 列表
    /// <summary>
    /// 得到 Enemy 列表
    /// </summary>
    public void createEnemyList()
    {
        int x = JSONO.GetField("weapon").Count;
        enemyList = new GameObject[x];
        string name = "";
        string png = "";
        int HP = 0;
        int str = 0;
        int def = 0;
        int exp = 0;
        int gold = 0;
        for (int i = 0; i < x; i++)
        {
            GameObject newItem = Instantiate(ListItem);
            newItem.transform.SetParent(PanelListContainer);
            newItem.transform.localScale = new Vector3(1, 1, 1);
            newItem.name = "item" + i.ToString();
            JSONO.GetField("item")[i].GetField(ref name, "name");
            JSONO.GetField("item")[i].GetField(ref png, "png");
            JSONO.GetField("item")[i].GetField(ref HP, "HP");
            JSONO.GetField("item")[i].GetField(ref str, "str");
            JSONO.GetField("item")[i].GetField(ref def, "def");
            JSONO.GetField("item")[i].GetField(ref exp, "exp");
            JSONO.GetField("item")[i].GetField(ref gold, "gold");
            enemyList[i] = newItem;
        }
    }
    public List<GameObject> enemyListGameObject;
    /// <summary>
    /// 生成怪物列表
    /// </summary>
    /// <param name="level">第 level 关卡</param>
	public void generateEnemyList(int level)
    {
        string enemyListString = "";
        JSONO.GetField("level")[level].GetField(ref enemyListString, "enemy");
        bool isBossLevel = false;
        JSONO.GetField("level")[level].GetField(ref isBossLevel, "isBoss");
        if (isBossLevel)
        {
            enemyIDList = new int[1][];
            enemyIDList[0] = new int[1];
            enemyIDList[0][0] = int.Parse(enemyListString);
        }
        else
        {
            //enemyList：存储着需要生成怪物的ID
            string[] enemyList = new string[enemyListString.Split('.').Length];
            enemyList = enemyListString.Split('.');
            //共随机多少个怪
            int enemyCount = 0;
            int enemyCountMin = 0;
            int enemyCountMax = 0;
            if (level < 10)
            {
                enemyCountMin = 4;
                enemyCountMax = 8;
            }
            else if (level >= 10 && level < 18)
            {
                enemyCountMin = 6;
                enemyCountMax = 12;
            }
            else
            {
                enemyCountMin = 8;
                enemyCountMax = 16;
            }
            enemyCount = Random.Range(enemyCountMin, enemyCountMax);
            //第一个随机70%-100%的几率，第一个怪物的数量
            int enemy1Count = Mathf.RoundToInt(Random.Range(0.7f, 1f) * enemyCount);
            //第二个取1减掉第一个的百分比几率再乘以怪物的总数，第二个怪物的数量
            int enemy2Count = enemyCount - enemy1Count;
            //剩余需要生成的怪物列数
            int columnLastEnemy = enemyCount;
            //eneyColumnList：存储的每一列的怪物个数
            List<int> enemyColumnList = new List<int>();
            do
            {
                int t = 1;
                if (level < 10)
                {
                    t = Random.Range(1, 3);
                }
                else if (level >= 10 && level < 18)
                {
                    t = Random.Range(1, 4);
                }
                else
                {
                    t = Random.Range(1, 5);
                }
                if (t > columnLastEnemy)
                {
                    t = columnLastEnemy;
                }
                enemyColumnList.Add(t);
                columnLastEnemy -= t;
            } while (columnLastEnemy > 0);
            //enemyColumnList是需要生成的怪物的所有数据，是一个列表，列表每一项存储的是当列的怪物个数
            enemyIDList = new int[enemyColumnList.Count][];
            for (int i = 0; i < enemyColumnList.Count; i++)
            {
                enemyIDList[i] = new int[enemyColumnList[i]];
            }
            for (int i = 0; i < enemyIDList.Length; i++)
            {
                for (int j = 0; j < enemyIDList[i].Length; j++)
                {
                    float r = Random.Range(0, 1f);
                    if (r < 0.7f && enemy1Count > 0)
                    {
                        enemyIDList[i][j] = int.Parse(enemyList[0]);
                        enemy1Count -= 1;
                    }
                    else if (r >= 0.7f && enemy2Count > 0)
                    {
                        if (enemyList.Length > 1)
                        {
                            enemyIDList[i][j] = int.Parse(enemyList[1]);
                            enemy2Count -= 1;
                        }
                    }
                }
            }
        }
    }
    /// <summary>
    /// 获取列表
    /// </summary>
    public void getList(string type)
    {
        switch (type)
        {
            case "item":
                if (itemList == null)
                {
                    createItemList();
                }
                foreach (var item in itemList)
                {
                    addItemToList(item.GameObject);
                }
                break;
            case "equipment":
                if (equipmentList == null)
                {
                    createEquipmentList();
                }
                addItemToList(equipmentList);
                break;
            case "magic":
                if (magicList == null)
                {
                    createMagicList();
                }
                addItemToList(magicList);
                break;
            case "job":
                if (jobList == null)
                {
                    createJobList();
                }
                addItemToList(jobList);
                break;
            case "enemy":
                if (enemyList == null)
                {
                    createEnemyList();
                }
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// 将得到的列表添加进入UI中
    /// </summary>
    public void addItemToList(GameObject[] list)
    {
        int y = list.Length;
        for (int i = 0; i < y; i++)
        {
            list[i].transform.SetParent(showList.transform, false);
        }
    }
    public void addItemToList(GameObject go)
    {
        go.transform.SetParent(showList.transform, false);
    }
    public void removeItemToData()
    {
        while (showList.transform.childCount > 0)
        {
            showList.transform.GetChild(0).SetParent(dataList.transform);
        }
    }
    public void beginFight()
    {
        generateEnemyList(level);
        BM.init();
        Village.SetActive(false);
        Fight.SetActive(true);
        GDM.SaveAutoData();
    }
    public void backToVillage()
    {
        Text_Gold.text = player.gold + " G";
        Village.SetActive(true);
        Fight.SetActive(false);
        level += 1;
        Text_Day.text = "第 " + (level + 1).ToString() + " 天";
        GDM.SaveAutoData();
    }
    public void GameOver()
    {
        SceneManager.LoadScene("Start");
    }
}