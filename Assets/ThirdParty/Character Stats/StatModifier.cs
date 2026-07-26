using System;

namespace Kryz.CharacterStats
{
	public enum StatModType
	{
		Flat = 100,
		PercentAdd = 200,
		PercentMult = 300,
	}

    [Serializable]
	public struct StatModifier : IEquatable<StatModifier>
	{
		public float Value;
		public StatModType Type;
		public int Order;
		public object Source;

		public StatModifier(float value, StatModType type, int order, object source)
		{
			Value = value;
			Type = type;
			Order = order;
			Source = source;
		}

		public StatModifier(float value, StatModType type) : this(value, type, (int)type, null) { }

		public StatModifier(float value, StatModType type, int order) : this(value, type, order, null) { }

		public StatModifier(float value, StatModType type, object source) : this(value, type, (int)type, source) { }

		public bool Equals(StatModifier other)
		{
			return Value == other.Value && Type == other.Type && Order == other.Order && Source == other.Source;
		}

		public override bool Equals(object obj)
		{
			return obj is StatModifier other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Value, Type, Order, Source);
		}
	}
}
