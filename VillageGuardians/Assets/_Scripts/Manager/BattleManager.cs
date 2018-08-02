using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    //当前行动的角色
    int currentActorIndex;
    int nextActorIndex;
    //可行动角色列表
    private List<Hero> heroList;
    private List<Enemy> enemyList;
    int actorID;
    //当前被攻击对象
    GameObject beHurtGameObject;

    public List<ActItem> ActorList;
    //当前是否在行动
    bool inAct;
    //是否战斗结束
    bool isEnd;
    //行动计时器
    float actTimer;
    //每回合行动时间
    float everyActTime;
    int wave;
    GameManager GM;

    Text leftWave;
    GameObject backToVillageButton;
    GameObject gameOverButton;

    Hero hero1;

    void Start()
    {
        GM = this.GetComponent<GameManager>();
        Text_Item = GameObject.Find("Canvas").transform.Find("Panel_Fight/Text_Item").GetComponent<Text>();
    }

    Button btn;
    bool isAttackToDead;
    int deadID;

    void Update()
    {
        if (!GM.Fight.activeSelf)
        {
            return;
        }
        lerpEnemy();
        if (!isWaveIn)
        {
            return;
        }
        updateInfo();
        updateItem();
        fixActorIndex();
        if (!inAct && !isEnd)
        {
            actTimer += Time.deltaTime;
        }
        if (actTimer > everyActTime)
        {
            actTimer = 0;
            turnBase();
        }
        if (blinkTotal > 0)
        {
            hertBlink(beHurtGameObject, isAttackToDead, deadID);
        }
    }

    public void init()
    {
        bindInfo();
        updateInfo();
        updateItem();

        wave = 0;
        heroList = new List<Hero>();
        enemyList = new List<Enemy>();
        inAct = false;
        isEnd = false;
        actorID = 0;
        currentActorIndex = 0;
        ActorList = new List<ActItem>();

        for (int i = 0; i < GM.player.heroList.Count; i++)
        {
            ActorList.Add(GM.player.heroList[i] as ActItem);
            actorID++;
            heroList.Add(GM.player.heroList[i]);
        }

        leftWave = GameObject.Find("Canvas").transform.Find("Panel_Fight/Text_leftWave").GetComponent<Text>();
        backToVillageButton = GameObject.Find("Canvas").transform.Find("Panel_Fight/Button_Win").gameObject;
        backToVillageButton.SetActive(false);
        gameOverButton = GameObject.Find("Canvas").transform.Find("Panel_Fight/Button_GameOver").gameObject;
        gameOverButton.SetActive(false);

        EnemyListRT = GameObject.Find("Canvas").transform.Find("Panel_Fight/EnemyList").GetComponent<RectTransform>();

        actTimer = 2f;
        everyActTime = 0.5f;

        isAttackToDead = false;
        createHeros();
        createEnemys();
    }

    #region 进行一个回合
    float moveDistance = 20f;
    public void turnBase()
    {
        if (inAct || blinkTotal > 0)
        {
            return;
        }
        inAct = true;
        Transform trans = ActorList[currentActorIndex].GameObject.transform;
        int attacker = -1;
        int defencer = -1;
        if (ActorList[currentActorIndex].ActorType == 1)
        {
            if (!checkHPLose())
            {
                //int i = Random.Range(0, enemyList.Count);
                int i = 0;
                attacker = currentActorIndex;
                defencer = i;
                Debug.Log("id:::" + i);
                beHurtGameObject = enemyList[i].GameObject;
                moveDistance = Mathf.Abs(moveDistance);
                Debug.Log("执行了一次攻击！");
                int beHurtHP = heroList[attacker].str - enemyList[defencer].def;
                beHurtHP = Random.Range(beHurtHP, beHurtHP + 4);
                //十分之一的暴击几率
                float crit = Random.Range(0, 10);
                if (crit < 1)
                {
                    beHurtHP = beHurtHP * 2;
                }
                if (beHurtHP > 0)
                {
                    flyText(enemyList[defencer].GameObject.transform.position, new Color(1, 0, 0, 1), "-" + beHurtHP);
                    enemyList[defencer].HP = enemyList[defencer].HP - beHurtHP;
                    Debug.Log("被攻击者的生命：" + enemyList[defencer].HP);
                    if (enemyList[defencer].HP <= 0)
                    {
                        isAttackToDead = true;
                        deadID = defencer;
                        for (int j = 0; j < heroList.Count; j++)
                        {
                            heroList[j].exp += enemyList[defencer].exp;
                            heroList[j].checkLevelUp();
                        }
                        Debug.Log("金币：" + enemyList[defencer].gold);
                        GM.player.gold += enemyList[defencer].gold;
                        Debug.Log("角色等级：" + heroList[attacker].level);
                        Debug.Log("角色经验：" + heroList[attacker].exp);
                        Debug.Log("角色攻击：" + heroList[attacker].str);
                    }
                }
                else
                {
                    flyText(enemyList[defencer].GameObject.transform.position, new Color(1, 1, 1, 1), "闪避");
                    Debug.Log("未破防");
                }
            }
        }
        else if (ActorList[currentActorIndex].ActorType == -1)
        {
            int i = Random.Range(0, heroList.Count);
            attacker = currentActorIndex - heroList.Count;
            defencer = i;
            Debug.Log("id:::" + i);
            beHurtGameObject = heroList[i].GameObject;
            moveDistance = Mathf.Abs(moveDistance) * -1;
            Debug.Log("执行了一次攻击！");
            int beHurtHP = 0;
            //加入怪物命中率
            float hitRate = Random.Range(0f, 1f);
            if (GM.level < 5)
            {
                if (hitRate > 0.5f)
                {
                    beHurtHP = enemyList[attacker].str - heroList[defencer].def;
                }
            }
            else if (GM.level >= 5 && GM.level < 15)
            {
                if (hitRate > 0.3f)
                {
                    beHurtHP = enemyList[attacker].str - heroList[defencer].def;
                }
            }
            else
            {
                if (hitRate > 0.1f)
                {
                    beHurtHP = enemyList[attacker].str - heroList[defencer].def;
                }
            }
            if (beHurtHP > 0)
            {
                flyText(heroList[defencer].GameObject.transform.position, new Color(1, 0, 0, 1), "-" + beHurtHP);
                heroList[defencer].HP = heroList[defencer].HP - beHurtHP;
                if (heroList[defencer].HP <= 0)
                {
                    Debug.Log("游戏结束");
                    gameOverButton.SetActive(true);
                    isEnd = true;
                    return;
                }
                Debug.Log("被攻击者的生命：" + heroList[defencer].HP);
            }
            else
            {
                flyText(heroList[defencer].GameObject.transform.position, new Color(0, 1, 0, 1), "闪避");
                Debug.Log("未破防");
            }
        }
        trans.DOMoveX(trans.position.x + moveDistance, 0.1f).OnComplete(() =>
        {
            blinkTotal = 0.5f;
            trans.DOMoveX(trans.position.x - moveDistance, 0.1f).OnComplete(() =>
            {
                inAct = false;
            });
        });
        currentActorIndex++;
    }
    #endregion
    #region 切换角色
    void fixActorIndex()
    {
        if (currentActorIndex >= ActorList.Count)
        {
            currentActorIndex -= ActorList.Count;
        }
        nextActorIndex = currentActorIndex + 1;
        if (nextActorIndex >= ActorList.Count)
        {
            nextActorIndex -= ActorList.Count;
        }
    }
    #endregion
    #region AI攻击逻辑
    //选择AI攻击对象
    void selectAttackTarget()
    {

    }
    #endregion
    #region 受伤闪烁
    float blinkTotal = 0;
    float blinkTimer = 0;
    bool timerUp = false;
    void hertBlink(GameObject go, bool isDead, int did)
    {
        blinkTotal -= Time.deltaTime;
        if (go != null)
        {
            Image img = go.transform.Find("Image").GetComponent<Image>();
            if (isDead)
            {
                img.color = new Color(1, 0, 0, blinkTimer);
            }
            else
            {
                img.color = new Color(img.color.r, img.color.g, img.color.b, blinkTimer);
            }
            if (blinkTimer >= 1)
            {
                timerUp = false;
            }
            if (blinkTimer <= 0)
            {
                timerUp = true;
            }
            if (!timerUp)
            {
                blinkTimer -= Time.deltaTime * 50;
            }
            else
            {
                blinkTimer += Time.deltaTime * 50;
            }
            if (blinkTotal < 0)
            {
                if (isDead && did > -1)
                {
                    GameObject.Destroy(enemyList[did].GameObject);
                    enemyList.RemoveAt(did);
                    ActorList.RemoveAt(did + heroList.Count);
                    if (enemyList.Count == 0)
                    {
                        createEnemys();
                    }
                }
                isAttackToDead = false;
                deadID = -1;
                inAct = false;
                blinkTotal = 0;
                img.color = new Color(1, 1, 1, 1);
            }
            else
            {
                inAct = true;
            }
        }
        else
        {
            inAct = false;
            blinkTotal = 0;
        }
    }
    #endregion

    bool isWaveIn = false;
    void createEnemys()
    {
        EnemyListRT.anchoredPosition = new Vector2(75, 110);
        enemyList.Clear();
        int[][] enemys = GM.enemyIDList;
        if (wave >= enemys.Length)
        {
            backToVillageButton.SetActive(true);
            isEnd = true;
            return;
        }
        leftWave.text = "第 " + (wave + 1).ToString() + " 波 / 共 " + enemys.Length + " 波";
        Debug.Log("enemyWave:::" + enemys.Length);
        for (int i = 0; i < enemys.Length; i++)
        {
            Debug.Log("enemyWave:::" + i + ":::" + enemys[i].Length);
        }
        for (int j = 0; j < enemys[wave].Length; j++)
        {
            string png = "";
            GM.JSONO.GetField("enemy")[enemys[wave][j]].GetField(ref png, "png");
            GameObject go = Instantiate(Resources.Load<GameObject>("Enemys/Enemy"));
            go.name = "Enemy" + ActorList.Count;
            go.transform.SetParent(GM.Fight.transform.Find("EnemyList"), false);
            go.transform.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>(png);
            go.transform.Find("Image").GetComponent<Image>().SetNativeSize();
            Enemy e = new Enemy();
            Debug.Log("enemyID:::" + enemys[wave][j]);
            e.setJsonToEnemy(GM.JSONO.GetField("enemy")[enemys[wave][j]]);
            e.GameObject = go;
            ActorList.Add(e as ActItem);
            enemyList.Add(e);
        }
        wave += 1;
        isLerping = true;
        lerpTimer = 0;
        isWaveIn = false;
        currentActorIndex = 0;
        fillHP();
    }

    Transform heroListContainer;
    void createHeros()
    {
        heroListContainer = GameObject.Find("Canvas").transform.Find("Panel_Fight/HeroList");
        for (int i = 0; i < heroListContainer.childCount; i++)
        {
            GameObject.Destroy(heroListContainer.GetChild(i).gameObject);
        }
        for (int i = 0; i < GM.player.heroList.Count; i++)
        {
            Transform hero = Instantiate(Resources.Load<GameObject>("Prefabs/Hero")).transform;
            hero.SetParent(heroListContainer);
            if (i != 0)
            {
                string png = "";
                GM.JSONO.GetField("job")[i-1].GetField(ref png, "png");
                hero.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>(png);
            }
            GM.player.heroList[i].GameObject = hero.gameObject;
        }
    }
    Transform infoPanel;
    Text Text_LV;
    Slider Slider_LV;
    Text Text_HP;
    Slider Slider_HP;
    Text Text_MP;
    Slider Slider_MP;
    void bindInfo()
    {
        infoPanel = GameObject.Find("Canvas").transform.Find("Panel_Fight/HeroListInfo");
        for (int i = 0; i < infoPanel.childCount; i++)
        {
            GameObject.Destroy(infoPanel.GetChild(i).gameObject);
        }
        for (int i = 0; i < GM.player.heroList.Count; i++)
        {
            Transform heroInfo = Instantiate(Resources.Load<GameObject>("Prefabs/HeroInfo")).transform;
            heroInfo.SetParent(infoPanel, false);
        }
    }
    void updateInfo()
    {
        for (int i = 0; i < GM.player.heroList.Count; i++)
        {
            Text_LV = infoPanel.GetChild(i).Find("Text_LV").GetComponent<Text>();
            Slider_LV = infoPanel.GetChild(i).Find("Slider_LV").GetComponent<Slider>();
            Text_HP = infoPanel.GetChild(i).Find("Text_HP").GetComponent<Text>();
            Slider_HP = infoPanel.GetChild(i).Find("Slider_HP").GetComponent<Slider>();
            Text_MP = infoPanel.GetChild(i).Find("Text_MP").GetComponent<Text>();
            Slider_MP = infoPanel.GetChild(i).Find("Slider_MP").GetComponent<Slider>();

            float nextLvExp = Mathf.Round(Mathf.Pow((GM.player.heroList[i].level), 0.4f) * Mathf.Pow(GM.player.heroList[i].level, 2) * 5);
            Text_LV.text = "Lv：" + GM.player.heroList[i].level.ToString();
            if (GM.player.heroList[i].level != 0)
            {
                Slider_LV.value = GM.player.heroList[i].exp / nextLvExp;
            }
            else
            {
                Slider_LV.value = 0;
            }
            Text_HP.text = "HP：" + GM.player.heroList[i].HP + "/" + GM.player.heroList[i].HPMax;
            if (GM.player.heroList[i].HP != 0)
            {
                Slider_HP.value = (float)GM.player.heroList[i].HP / GM.player.heroList[i].HPMax;
            }
            else
            {
                Slider_HP.value = 0;
            }
            Text_MP.text = "MP：" + GM.player.heroList[i].MP + "/" + GM.player.heroList[i].MPMax;
            if (GM.player.heroList[i].MPMax != 0)
            {
                Slider_MP.value = (float)GM.player.heroList[i].MP / GM.player.heroList[i].MPMax;
            }
            else
            {
                Slider_MP.value = 0;
            }
        }
    }

    float lerpTimer = 0;
    bool isLerping = false;
    Vector2 fromPosition = new Vector2(75, 110);
    Vector2 toPosition = new Vector2(-110, 110);
    RectTransform EnemyListRT;
    void lerpEnemy()
    {
        if (isLerping)
        {
            lerpTimer += Time.deltaTime;
            EnemyListRT.anchoredPosition = Vector2.Lerp(fromPosition, toPosition, lerpTimer);
            if (lerpTimer > 1)
            {
                isLerping = false;
                isWaveIn = true;
            }
        }
    }

    Text Text_Item;
    void updateItem()
    {
        Text_Item.text = "道具：\n";
        for (int i = 0; i < GM.player.itemCount.Length; i++)
        {
            if (GM.player.itemCount[i] > 0)
            {
                string itemName = "";
                switch (i)
                {
                    case 0:
                        itemName = "面包";
                        break;
                    case 1:
                        itemName = "药草";
                        break;
                    case 2:
                        itemName = "蜂蜜酒";
                        break;
                    case 3:
                        itemName = "Elixir";
                        break;
                }
                Text_Item.text += itemName + "   " + GM.player.itemCount[i] + "\n";
            }
        }
    }

    /// <summary>
    /// 检查生命值损失是否需要用药
    /// </summary>
    /// <returns>true为掉血了，需要补充生命值；false为不需要补充，继续战斗</returns>
    bool checkHPLose()
    {
        int itemCount = 0;
        foreach (var item in GM.player.itemCount)
        {
            itemCount += item;
        }
        if (itemCount == 0)
        {
            return false;
        }
        int lostHP = heroList[currentActorIndex].HPMax - heroList[currentActorIndex].HP;
        if (lostHP >= 10 && lostHP < 30)
        {
            if (GM.player.itemCount[0] > 0)
            {
                heroList[currentActorIndex].HP += 10;
                flyText(heroList[currentActorIndex].GameObject.transform.position, new Color(0, 1, 0, 1), "+" + 10);
                GM.player.itemCount[0] -= 1;
            }
        }
        else if (lostHP >= 30 && lostHP < 100)
        {
            if (GM.player.itemCount[1] > 0)
            {
                heroList[currentActorIndex].HP += 30;
                flyText(heroList[currentActorIndex].GameObject.transform.position, new Color(0, 1, 0, 1), "+" + 30);
                GM.player.itemCount[1] -= 1;
            }
        }
        else if (lostHP >= 100 && lostHP < 500)
        {
            if (GM.player.itemCount[2] > 0)
            {
                heroList[currentActorIndex].HP += 100;
                flyText(heroList[currentActorIndex].GameObject.transform.position, new Color(0, 1, 0, 1), "+" + 100);
                GM.player.itemCount[2] -= 1;
            }
        }
        else if (lostHP >= 500)
        {
            if (GM.player.itemCount[3] > 0)
            {
                heroList[currentActorIndex].HP += 500;
                flyText(heroList[currentActorIndex].GameObject.transform.position, new Color(0, 1, 0, 1), "+" + 500);
                GM.player.itemCount[3] -= 1;
            }
        }
        else
        {
            return false;
        }
        return true;
    }

    void fillHP()
    {
        int lostHP = heroList[currentActorIndex].HPMax - heroList[currentActorIndex].HP;
        int itemCount = 0;
        foreach (var item in GM.player.itemCount)
        {
            itemCount += item;
        }
        if (itemCount == 0)
        {
            return;
        }
        while (lostHP > 5)
        {
            if (lostHP >= 500)
            {
                if (GM.player.itemCount[3] > 0)
                {
                    heroList[currentActorIndex].HP += 500;
                    GM.player.itemCount[3] -= 1;
                }
            }
            if (lostHP >= 100)
            {
                if (GM.player.itemCount[2] > 0)
                {
                    heroList[currentActorIndex].HP += 100;
                    GM.player.itemCount[2] -= 1;
                }
            }
            if (lostHP >= 30)
            {
                if (GM.player.itemCount[1] > 0)
                {
                    heroList[currentActorIndex].HP += 30;
                    GM.player.itemCount[1] -= 1;
                }
            }
            if (lostHP > 5)
            {
                if (GM.player.itemCount[0] > 0)
                {
                    heroList[currentActorIndex].HP += 10;
                    GM.player.itemCount[0] -= 1;
                }
            }
            if (lostHP == heroList[currentActorIndex].HPMax - heroList[currentActorIndex].HP)
            {
                break;
            }
            if (heroList[currentActorIndex].HP > heroList[currentActorIndex].HPMax)
            {
                heroList[currentActorIndex].HP = heroList[currentActorIndex].HPMax;
            }
            lostHP = heroList[currentActorIndex].HPMax - heroList[currentActorIndex].HP;
        }
    }

    void flyText(Vector3 pos,Color color,string content)
    {
        GameObject go = Instantiate(Resources.Load<GameObject>("Prefabs/Text_FlyText"), pos, Quaternion.identity) as GameObject;
        go.transform.SetParent(GameObject.Find("Canvas").transform);
        FlyText ft = go.GetComponent<FlyText>();
        ft.color = color;
        ft.content = content;
    }
}
