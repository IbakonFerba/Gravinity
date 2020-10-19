using UnityEngine;

namespace C
{
	/// <summary>
	/// Constants for easy access to layers and easy creation of Layer Masks. If you add layers, you have to regenerate this class by pressing CTRL+ALT+C or clicking on Tools/Generate Constats Classes
	/// </summary>
	public static class Layers
	{
		public const int DEFAULT = 0;
		public const int TRANSPARENT_FX = 1;
		public const int IGNORE_RAYCAST = 2;
		public const int WATER = 4;
		public const int UI = 5;
		public const int PLAYER = 8;
	
		/// <summary>
		/// Returns a layermask only including the provided Layers
		/// </summary>
		public static int OnlyIncluding(params int[] layers)
		{
			int mask = 0;
			foreach(int layer in layers)
				mask |= (1 << layer);
			return mask;
		}

		/// <summary>
		/// Returns a layermask including all Layers but the provided ones
		/// </summary>
		public static int EverythingBut(params int[] layers)
		{
			return ~OnlyIncluding(layers);
		}

		/// <summary>
		/// Returns true if the provided layer is in the layermask and false if it is not
		/// </summary>
		public static bool IsInLayerMask(this int layer, LayerMask mask)
		{
			return mask == (mask | (1 << layer));
		}
	}
}