using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulsar.Atoms;

public class Enforce
{
	public class EnforceException : Exception
	{
		public EnforceException() : base() { }
		public EnforceException(string message) : base(message) { }
		public EnforceException(string message, Exception innerException) : base(message, innerException) { }
	}


	public static void Of<E>(bool exp, string? message = null) where E : Exception, new()
	{
		if(exp == false)
		{
			E? e = null;
			if(message?.Length > 0)
				e = (E?)Activator.CreateInstance(typeof(E), message);
			e ??= new E();
			throw e;
		}
	}
	public static void Of(bool exp) => Of<EnforceException>(exp);
	public static T Of<T>(T? obj, string? message = null)
	{
		Enforce.Of<EnforceException>(obj != null, message);
		return obj!;
	}
}