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

 /// <summary>
	/// Записывает значение свойтва, если значение не null
	/// </summary>
	/// <param name="key"></param>
	/// <param name="value"></param>
	/// <param name="writeNull"
	/// <returns></returns>
	public JsonComposer Prop<T>(string key, T? value, bool writeNull = false)
	{
		Enforce.Of(key?.Length);
		Enforce.Of(IsObject, "Текущей вложенностью должен быть объект!");

		string s;
		switch(value)
		{
			case null:
				{
					if(writeNull == false)
						return this;
					else
						s = "null";
				}	break;
			case bool vb:	s = vb == true ? "true" : "false"; break;
			case double vd: s = vd.ToString(System.Globalization.CultureInfo.InvariantCulture); break;
			case float vd: s = vd.ToString(System.Globalization.CultureInfo.InvariantCulture); break;
			case decimal vc: s = vc.ToString(System.Globalization.CultureInfo.InvariantCulture); break;
			case string vs: s = "\"" + HttpUtility.JavaScriptStringEncode(vs) + "\""; break;
			default: s = value.ToString()!; break;
		}

		putComma();

		put("\"");
		put(key);
		put("\":");

		put(s);

		return this;
	}

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
