using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;
using Leopotam.EcsLite;

[Flags]
public enum Team : byte
{
    none = 0,
    neutral = 1,
    player = 2,
    enemy = 4,
    all = 0b_1111_1111,
}
public static class TeamService
{
    public static Team GetTeam(int ent)
    {
        if (!EcsStatic.GetPool<ECTeam>().Has(ent))
            return Team.none;
        return EcsStatic.GetPool<ECTeam>().Get(ent).team;
    }
    public static void SetTeam(int ent, Team team)
    {
        EcsStatic.GetPool<ECTeam>().SafeAdd(ent).team = team;
    }
    public static Team MakeFilter(params Team[] args)
    {
        Team filter = Team.none;
        for (int i = 0; i < args.Length; i++)
            filter |= args[i];
        return filter;
    }
    public static bool FilterContains(Team filter, Team team)
    {
        return filter.HasFlag(team);
    }
}
public struct ECTeam
{
    public Team team;
}
