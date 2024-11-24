using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTeamColorLookup", menuName = "Team")]
public class TeamColorLookup : ScriptableObject
{
    [SerializeField] private Color[] _teamColors;

    public Color GetTeamColor(int teamIndex)
    {
        if (teamIndex < 0 || teamIndex >= _teamColors.Length)
        {
            return Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        }
        return _teamColors[teamIndex];
    }
}
