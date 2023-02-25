using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulsar.Atoms;

public static class Bit
{
	public static bool IsSet(ulong number, byte bitIndex)
	{
		return ((number >> bitIndex) & 1) != 0;
	}
	public static bool IsSet<T>(this T t, int pos) where T : struct, IConvertible
	{
		var value = t.ToInt64(CultureInfo.CurrentCulture);
		return (value & (1 << pos)) != 0;
	}

	public static ulong Set(ulong number, byte bitIndex)
	{
		ulong u = (ulong)(1 << bitIndex);
		return number | u;
	}
	public static ulong Clear(ulong number, byte bitIndex)
	{
		return number & (ulong)( ~(1 << bitIndex));
	}
}
