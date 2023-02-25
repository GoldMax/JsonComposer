using Pulsar;
using System.Diagnostics;

internal class Program
{
	static void Main(string[] args)
	{

		JsonComposer jc = new JsonComposer();
		jc.OpenObject();
		jc.Prop("one", 1);
		jc.Prop("two", "2222");
		jc.OpenObject("three");
		jc.Prop("aaa", "A\tA\"A\"");
		jc.Prop("bbb", "BBB");
		jc.Prop("ccc", "CCC");
		jc.CloseObject();
		jc.Prop("four", 44.4);
		jc.Prop("five", 555);
		jc.CloseObject();

	 Debug.WriteLine(jc);
		//assert(jc.text == `{ "one":1,"two":"2222","three":{ "aaa":"A\tA\"A\"","bbb":"BBB","ccc":"CCC"},"four":44.4,"five":555}`);

		void print(string? s) { Debug.Write(s); }

		jc = new JsonComposer(print);
		jc.OpenObject();
		jc.Prop("one", 1);
		jc.Prop("two", "2222");
		jc.OpenObject("three");
		jc.Prop("aaa", "A\tA\"A\"");
		jc.Prop("bbb", "BBB");
		jc.OpenArray("items");
		jc.OpenObject();
		jc.Prop("four", 44.4);
		jc.Prop("five", 555);
		jc.CloseObject();
		jc.OpenObject();
		jc.Prop("six", 666.6);
		jc.Prop("seven", 777);
		jc.Close();

		//trace(jc);
		//assert(jc.text == `{ "one":1,"two":"2222","three":{ "aaa":"A\tA\"A\"","bbb":"BBB","items":[{ "four":44.4,"five":555},{ "six":666.6,"seven":777}]} }`);


	}
}