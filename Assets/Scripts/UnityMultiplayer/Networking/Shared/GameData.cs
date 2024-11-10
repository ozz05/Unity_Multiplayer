using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Map
{
    Default
}

public enum GameMode
{
    Default
}

public enum GameQueue
{
    Solo,
    Team
}

[Serializable]
public class GameInfo
{
    public Map map;
    public GameMode gameMode;
    public GameQueue gameQueue;

    public string ToMultiplayQueue()
    {
        return "";
    }
}

[Serializable]
public class UserData
{
    public string UserName;
    public string UserAuthId;
    public GameInfo UserGamePreferences;
}
