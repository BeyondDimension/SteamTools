using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SAM.API
{
	internal static class Helpers
	{
		public const int MemoryBufferSize = 1024 * 32;

		private static IntPtr[] MemoryPool = new IntPtr[]
		{
			Marshal.AllocHGlobal( MemoryBufferSize ),
			Marshal.AllocHGlobal( MemoryBufferSize ),
			Marshal.AllocHGlobal( MemoryBufferSize ),
			Marshal.AllocHGlobal( MemoryBufferSize )
		};
		private static int MemoryPoolIndex;

		public static unsafe IntPtr TakeMemory()
		{
			lock (MemoryPool)
			{
				MemoryPoolIndex++;

				if (MemoryPoolIndex >= MemoryPool.Length)
					MemoryPoolIndex = 0;

				var take = MemoryPool[MemoryPoolIndex];

				((byte*)take)[0] = 0;

				return take;
			}
		}


		private static byte[][] BufferPool = new byte[4][];
		private static int BufferPoolIndex;

		/// <summary>
		/// Returns a buffer. This will get returned and reused later on.
		/// We shouldn't really be using this anymore. 
		/// </summary>
		public static byte[] TakeBuffer(int minSize)
		{
			lock (BufferPool)
			{
				BufferPoolIndex++;

				if (BufferPoolIndex >= BufferPool.Length)
					BufferPoolIndex = 0;

				if (BufferPool[BufferPoolIndex] == null)
					BufferPool[BufferPoolIndex] = new byte[1024 * 256];

				if (BufferPool[BufferPoolIndex].Length < minSize)
				{
					BufferPool[BufferPoolIndex] = new byte[minSize + 1024];
				}

				return BufferPool[BufferPoolIndex];
			}
		}

		internal unsafe static string MemoryToString(IntPtr ptr)
		{
			var len = 0;

			for (len = 0; len < MemoryBufferSize; len++)
			{
				if (((byte*)ptr)[len] == 0)
					break;
			}

			if (len == 0)
				return string.Empty;

			return UTF8Encoding.UTF8.GetString((byte*)ptr, len);
		}
	}

	internal class MonoPInvokeCallbackAttribute : Attribute
	{
		public MonoPInvokeCallbackAttribute() { }
	}

	/// <summary>
	/// Prevent unity from stripping shit we depend on
	/// https://docs.unity3d.com/Manual/ManagedCodeStripping.html
	/// </summary>
	internal class PreserveAttribute : System.Attribute { }
}
