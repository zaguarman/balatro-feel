using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BuffDecorator : ICreature {
    private readonly ICreature creature;
    private readonly Dictionary<BuffType, List<Buff>> buffs = new Dictionary<BuffType, List<Buff>>();

    public string Id => creature.Id;
    public string Name => creature.Name;
    public int Attack => creature.Attack + GetBuffValue(BuffType.Attack);
    public int Health => creature.Health + GetBuffValue(BuffType.Health);
    public List<IEffect> Effects => creature.Effects;

    public BuffDecorator(ICreature creature) {
        this.creature = creature;
        buffs[BuffType.Attack] = new List<Buff>();
        buffs[BuffType.Health] = new List<Buff>();
    }

    public void AddBuff(Buff buff) {
        if (!buffs.ContainsKey(buff.Type)) {
            buffs[buff.Type] = new List<Buff>();
        }
        buffs[buff.Type].Add(buff);
    }

    public void RemoveBuffsFromSource(string sourceId) {
        foreach (var buffList in buffs.Values) {
            buffList.RemoveAll(b => b.SourceId == sourceId);
        }
    }

    public int GetBuffValue(BuffType type) {
        return buffs.ContainsKey(type) ? buffs[type].Sum(b => b.Value) : 0;
    }

    public void TakeDamage(int damage, IGameContext context) {
        creature.TakeDamage(damage, context);
    }

    public void Play(IGameContext context, IPlayer owner) {
        creature.Play(context, owner);
    }
}