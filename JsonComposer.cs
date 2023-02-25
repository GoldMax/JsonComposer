using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using Pulsar.Atoms;
using System.Runtime.CompilerServices;
using System.Web;

namespace Pulsar;

public class JsonComposer
{
 public delegate void StringWriter(string? s);

	private StringBuilder? _data;
	private byte _deep = 0;
	// каждый бит - нужна ли запятая на данном уровне
	private ulong _comma = 0;
	// каждый бит - был ли уровень открыт массивом
	ulong _arrs = 0;
	StringWriter? _put = null;

	/// Глубина вложенности
	public byte Deep
	{
		get { return _deep; }
		set
		{
			Enforce.Of<ArgumentException>(value < 64, "Предельная величина вложенности = 63!");
			_deep = value;
		}
	}
	/// Возвращает true, если сейчас вложенность объекта
	public bool IsObject { get => Bit.IsSet(_arrs, _deep) == false; }
	/// Возвращает true, если сейчас вложенность массива
	public bool IsArray { get => Bit.IsSet(_arrs, _deep); }
	/// Текст json
	public string? Text { get => _data?.ToString(); }

	public JsonComposer()
	{
		_data = new StringBuilder();
	}
	public JsonComposer(StringWriter writer)
	{
		this._put = writer;
	}

 private void put(string? s) 
	{
		if(_put is null)
			Enforce.Of(_data).Append(s);
		else
			_put(s);
	}
	private void putComma()
	{
		if(Bit.IsSet(_comma,_deep))
			put(",");
		else
			_comma = Bit.Set(_comma,_deep);
	}

 public JsonComposer OpenObject()
	{
		putComma();
  put("{");
  Deep++;
		return this;
	}
	public JsonComposer OpenObject(string key)
	{
		Enforce.Of(key?.Length > 0);
		putComma();
		put("\"");
		put(key);
		put("\":");
		put("{");
		Deep++;
		return this;
	}
	public JsonComposer CloseObject()
	{
		Enforce.Of(IsObject, "Текущей вложенностью не является объект!");
		Enforce.Of(Deep > 0, "deep is zero!");
		put("}");
		_comma = Bit.Clear(_comma,_deep);
		Deep--;
		return this;
	}

	public JsonComposer OpenArray()
	{
		putComma();
		put("[");
		Deep++;
		_arrs = Bit.Set(_arrs,_deep);
		return this;
	}
	public JsonComposer OpenArray(string key)
	{
		Enforce.Of(key?.Length > 0);
		putComma();
		put("\"");
		put(key);
		put("\":");
		put("[");
		Deep++;
		_arrs = Bit.Set(_arrs, _deep);
		return this;
}
	public JsonComposer CloseArray()
	{
		Enforce.Of(IsArray, "Текущей вложенностью не является массив!");
		Enforce.Of(Deep > 0, "deep is zero!");
		put("]");
		_comma = Bit.Clear(_comma,_deep);
		_arrs = Bit.Clear(_arrs,_deep);
		Deep--;
		return this;
	}

	private void Prop(string key)
	{
		Enforce.Of(key?.Length);
		Enforce.Of(IsObject, "Текущей вложенностью должен быть объект!");

		putComma();

		put("\"");
		put(key);
		put("\":");
	}
	public JsonComposer Prop(string key, bool value)
	{
		Prop(key);
		this.Value(value, true);
		return this;
 }
	public JsonComposer Prop(string key, long value) => Prop(key, (decimal)value);
	public JsonComposer Prop(string key, double value) => Prop(key, (decimal)value);
	public JsonComposer Prop(string key, decimal value)
	{
		Prop(key);
		this.Value(value, true);
		return this;
	}
	public JsonComposer Prop(string key, string value)
	{
		Prop(key);
		this.Value(value, true);
		return this;
	}

	public JsonComposer Value(bool value, bool nocomma = false)
	{
		if(nocomma == false)
			putComma();
		
		put(value ? "true" : "false");

		return this;
	}
	public JsonComposer Value(decimal value, bool nocomma = false)
	{
		if(nocomma == false)
			putComma();

		put(value.ToString());
		return this;
	}
 public JsonComposer Value(string value, bool nocomma = false)
	{
		if(nocomma == false)
			putComma();

		put("\"");
		//escapeString(&put, to!string(value));
		put(HttpUtility.JavaScriptStringEncode(value));
		put("\"");
		
		return this;
	}
	/*/// Записывает значение свойтва, если значение не T.init
	ref JsonComposer propOpt(T)(string key, T value) return
	{
	if(value.isNull)
		return this;
	return prop(key, value);
 }*/

	/// Закрывает вложенности до указнной глубины
	public JsonComposer Close(uint deep = 0)
	{
		while(this._deep > deep)
			if(this.IsArray)
				CloseArray();
			else
				CloseObject();
		return this;
	}

 public override string ToString() { return Text ?? "null"; }
}

/*
 * module pulsar.text.json.composer;

import std.array : appender, Appender;
import pulsar.atoms;
public import pulsar.text.json.tools;

struct JsonComposer
{
private:
	Appender!string _data;
	ubyte _deep = 0;
	// каждый бит - нужна ли запятая на данном уровне
	ulong _comma = false;
	// каждый бит - был ли уровень открыт массивом
	ulong _arrs;
	void	delegate(string) _put;

public:
	/// Глубина вложенности
	@property //	deep
	{
		ubyte deep() { return _deep; }
		void deep(int value)
		{
			enforce(value < 64, "Предельная величина вложенности = 63!");
			_deep = cast(ubyte)value;
		}
	}

public:
	this(void	delegate(string) writer)
	{
		enforce(writer);
		this._put = writer;
	}

private:
	void put(string s) { _put ? _put(s) : _data.put(s); }
	void putComma()
	{
		if(_comma.isBit(deep))
			put(",");
		else
			_comma = _comma.setBit(deep);
	}

public:
	/// Возвращает true, если сейчас вложенность объекта
	@property bool isObject() { return _arrs.isBit(deep) == false; }
	/// Возвращает true, если сейчас вложенность массива
	@property bool isArray() { return _arrs.isBit(deep); }

	ref JsonComposer openObject() return
	{
		putComma();
		put("{");
		deep = deep + 1;
		return this;
	}
	ref JsonComposer openObject(string key) return
	{
		CheckNullOrEmpty!key;
		putComma();
		put("\"");
		put(key);
		put("\":");
		put("{");
		deep = deep + 1;
		return this;
	}
	ref JsonComposer closeObject() return
	{
		enforce(isObject, "Текущей вложенностью не является объект!");
		enforce(deep > 0, "deep is zero!");
		put("}");
		_comma = _comma.clearBit(deep);
		deep = deep - 1;
		return this;
	}

	ref JsonComposer openArray() return
	{
		putComma();
		put("[");
		deep = deep + 1;
		_arrs = _arrs.setBit(deep);
		return this;
	}
	ref JsonComposer openArray(string key) return
	{
		CheckNullOrEmpty!key;
		putComma();
		put("\"");
		put(key);
		put("\":");
		put("[");
		deep = deep + 1;
		_arrs = _arrs.setBit(deep);
		return this;
	}
	ref JsonComposer closeArray() return
	{
		enforce(isArray, "Текущей вложенностью не является массив!");
		enforce(deep > 0, "deep is zero!");
		put("]");
		_comma = _comma.clearBit(deep);
		_arrs = _arrs.clearBit(deep);
		deep = deep - 1;
		return this;
	}

	ref JsonComposer prop(T)(string key, T value) return
	{
		CheckNullOrEmpty!key;
		enforce(isObject, "Текущей вложенностью должен быть объект!");

		putComma();

		put("\"");
		put(key);
		put("\":");

		this.value!T(value, true);

		return this;
	}
	ref JsonComposer value(T)(T value, bool nocomma = false) return
	{
		if(nocomma == false)
			putComma();

		import std.traits;
		import pulsar.atoms.decimal;
		static if(!is(T == enum) && (isNumeric!T || isBoolean!T || IsFixed!T ))
		{
			put(to!string(value));
		}
		else
		{
			put("\"");
			escapeString(&put, to!string(value));
			put("\"");
		}
		return this;
	}
	/// Записывает значение свойтва, если значение не T.init
	ref JsonComposer propOpt(T)(string key, T value) return
	{
	 if(value.isNull)
		 return this;
		return prop(key, value);
	}

	/// Закрывает вложенности до указнной глубины
	ref JsonComposer close(uint deep = 0) return
	{
		while(this.deep > deep)
			if(isArray)
				closeArray();
			else
				closeObject();
		return this;
	}

public:
	@property string text() { return _data.data(); }
	string toString() { return text; }
}

// put("");

void test()
{
	escapeStringTest();


	JsonComposer jc;
	jc.openObject();
	jc.prop("one", 1);
	jc.prop("two", "2222");
	jc.openObject("three");
		jc.prop("aaa", "A\tA\"A\"");
		jc.prop("bbb", "BBB");
		jc.prop("ccc","CCC");
	jc.closeObject();
	jc.prop("four", 44.4);
	jc.prop("five", 555);
	jc.closeObject();

	//trace(jc);
	assert(jc.text == `{"one":1,"two":"2222","three":{"aaa":"A\tA\"A\"","bbb":"BBB","ccc":"CCC"},"four":44.4,"five":555}`);

	void print(string s) { traceChars(s); }

	jc = JsonComposer(); //&print);
jc.openObject();
jc.prop("one", 1);
jc.prop("two", "2222");
jc.openObject("three");
jc.prop("aaa", "A\tA\"A\"");
jc.prop("bbb", "BBB");
jc.openArray("items");
jc.openObject();
jc.prop("four", 44.4);
jc.prop("five", 555);
jc.closeObject();
jc.openObject();
jc.prop("six", 666.6);
jc.prop("seven", 777);
jc.close();

//trace(jc);
assert(jc.text == `{ "one":1,"two":"2222","three":{ "aaa":"A\tA\"A\"","bbb":"BBB","items":[{ "four":44.4,"five":555},{ "six":666.6,"seven":777}]} }`);

trace("all pass");
}

*/