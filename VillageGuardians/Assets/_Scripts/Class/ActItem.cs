using UnityEngine;
public class ActItem
{
    private int id;
    private GameObject gameObject;
    //1为友军，-1为敌军
    private int actorType;

    public int Id
    {
        get
        {
            return id;
        }

        set
        {
            id = value;
        }
    }
    public GameObject GameObject
    {
        get
        {
            return gameObject;
        }

        set
        {
            gameObject = value;
        }
    }
    /// <summary>
    /// -1：敌人；0：中立；1：友军
    /// </summary>
    public int ActorType
    {
        get
        {
            return actorType;
        }

        set
        {
            actorType = value;
        }
    }
}