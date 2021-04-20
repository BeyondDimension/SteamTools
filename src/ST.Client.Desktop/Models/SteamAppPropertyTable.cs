using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace System.Application.Models
{
    public class SteamAppPropertyTable
    {
		public int Count
		{
			get
			{
				return _properties.Count;
			}
		}

		public IEnumerable<string> PropertyNames
		{
			get
			{
				return from prop in _properties
					   select prop.Name;
			}
		}

		internal IEnumerable<SteamAppProperty> Properties
		{
			get
			{
				return _properties;
			}
		}

		internal SteamAppProperty this[string name]
		{
			get
			{
				return _properties.FirstOrDefault((SteamAppProperty prop) => prop.Name == name);
			}
		}

		public bool HasProperty(params string[] PropertyPath)
		{
			SteamAppPropertyTable propertyTable = this;
			foreach (string name in PropertyPath)
			{
				if (propertyTable == null || propertyTable[name] == null)
				{
					return false;
				}
				propertyTable = propertyTable.GetPropertyValue<SteamAppPropertyTable>(name, null);
			}
			return true;
		}

		public T GetPropertyValue<T>(string name, T defValue = default)
		{
            if (!TryGetPropertyValue<T>(name, out T result))
            {
                result = defValue;
            }
            return result;
		}

		public bool TryGetPropertyValue<T>(string name, out T result)
		{
			bool result2 = false;
			result = default;
			SteamAppProperty property = this[name];
			if (property != null)
			{
				result2 = property.TryGetValue<T>(out result);
			}
			return result2;
		}

		public object GetPropertyAsObject(string name)
		{
			object result = null;
			SteamAppProperty property = this[name];
			if (property != null && property.Value != null)
			{
				result = property.Value;
			}
			return result;
		}

		public T GetPropertyValue<T>(T DefaultValue, params string[] PropertyPath)
		{
			SteamAppPropertyTable propertyTable = this;
			foreach (string name in PropertyPath.Take(PropertyPath.Length - 1))
			{
				SteamAppProperty property = propertyTable[name];
				if (property != null)
				{
					propertyTable = property.GetValue<SteamAppPropertyTable>();
				}
				if (property == null || propertyTable == null)
				{
					return DefaultValue;
				}
			}
			return propertyTable.GetPropertyValue<T>(PropertyPath.Last<string>(), DefaultValue);
		}

		public SteamAppPropertyType GetPropertyType(string name)
		{
			SteamAppPropertyType result = SteamAppPropertyType._Invalid_;
			SteamAppProperty property = this[name];
			if (property != null)
			{
				result = property.PropertyType;
			}
			return result;
		}

		public void AddPropertyValue(string name, SteamAppPropertyType type, object value)
		{
			SteamAppProperty item = new(name, type, value);
			_properties.Add(item);
		}

		public bool SetPropertyValue(string name, SteamAppPropertyType type, object value)
		{
			SteamAppProperty property = this[name];
			if (property == null)
			{
				property = new SteamAppProperty(name);
				_properties.Add(property);
			}
			bool result = property.PropertyType != type || !property.Value.Equals(value);
			property.PropertyType = type;
			property.Value = value;
			return result;
		}

		public bool SetPropertyValue(SteamAppPropertyType type, object value, params string[] propertyPath)
		{
			SteamAppPropertyTable propertyTable = this;
			foreach (string text in propertyPath.Take(propertyPath.Length - 1))
			{
				if (propertyTable.HasProperty(new string[]
				{
					text
				}))
				{
					propertyTable = propertyTable.GetPropertyValue<SteamAppPropertyTable>(text, null);
				}
				else
				{
					SteamAppPropertyTable propertyTable2 = new();
					propertyTable.SetPropertyValue(text, SteamAppPropertyType.Table, propertyTable2);
					propertyTable = propertyTable2;
				}
				if (propertyTable == null)
				{
					return false;
				}
			}
			return propertyTable.SetPropertyValue(propertyPath.Last<string>(), type, value);
		}

		public void RemoveProperty(string name)
		{
			_properties.RemoveAll((SteamAppProperty prop) => prop.Name == name);
		}

		public void RemoveProperty(params string[] propertyPath)
		{
			SteamAppPropertyTable propertyTable = this;
			foreach (string text in propertyPath.Take(propertyPath.Length - 1))
			{
				if (propertyTable.HasProperty(new string[]
				{
					text
				}))
				{
					propertyTable = propertyTable.GetPropertyValue<SteamAppPropertyTable>(text, null);
				}
				else
				{
					SteamAppPropertyTable propertyTable2 = new();
					propertyTable.SetPropertyValue(text, SteamAppPropertyType.Table, propertyTable2);
					propertyTable = propertyTable2;
				}
				if (propertyTable == null)
				{
					return;
				}
			}
			RemoveProperty(propertyPath.Last<string>());
		}

		public object ExtractProperty(string name)
		{
			SteamAppProperty property = this[name];
			SteamAppPropertyTable propertyTable = new();
			propertyTable.SetPropertyValue(property.Name, property.PropertyType, property.Value);
			MemoryStream memoryStream = new();
			new BinaryWriter(memoryStream).Write(propertyTable);
			return memoryStream;
		}

		public string AddExtractedProperty(object extracted)
		{
			SteamAppProperty property = new BinaryReader((MemoryStream)extracted).ReadPropertyTable()._properties[0];
			RemoveProperty(property.Name);
			_properties.Add(property);
			return property.Name;
		}

		public void Clear()
		{
			_properties.Clear();
		}

		public SteamAppPropertyTable()
		{
		}

		public SteamAppPropertyTable(SteamAppPropertyTable other)
		{
			_properties.AddRange(from prop in other._properties
									  select new SteamAppProperty(prop));
		}

		public bool Equals(SteamAppPropertyTable other)
		{
			return other != null && Count == other.Count && _properties.All((SteamAppProperty prop) => other._properties.Any((SteamAppProperty otherProp) => prop.Equals(otherProp)));
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as SteamAppPropertyTable);
		}

		public override int GetHashCode()
		{
			int num = base.GetType().GetHashCode();
			foreach (SteamAppProperty property in _properties)
			{
				num ^= property.GetHashCode();
			}
			return num;
		}

		private string ToStringInternal(int indent)
		{
			StringBuilder stringBuilder = new();
			string arg = new('\t', indent);
			foreach (SteamAppProperty property in _properties)
			{
				string arg2 = property.Name.Replace("\\", "\\\\").Replace("\"", "\\\"");
				stringBuilder.AppendFormat("{0}\"{1}\"", arg, arg2);
				switch (property.PropertyType)
				{
					case SteamAppPropertyType.Table:
						stringBuilder.AppendFormat("\n{0}{{\n{1}{0}}}", arg, property.GetValue<SteamAppPropertyTable>().ToStringInternal(indent + 1));
						break;
					case SteamAppPropertyType.String:
					case SteamAppPropertyType.WString:
						stringBuilder.AppendFormat("\t\t\"{0}\"", property.GetValue<string>().Replace("\\", "\\\\").Replace("\"", "\\\""));
						break;
					case SteamAppPropertyType.Int32:
					case SteamAppPropertyType.Float:
					case SteamAppPropertyType.Color:
					case SteamAppPropertyType.Uint64:
						stringBuilder.AppendFormat("\t\t\"{0}\"", property.Value);
						break;
					case (SteamAppPropertyType)4:
						goto IL_F1;
					default:
						goto IL_F1;
				}
				stringBuilder.Append("\n");
				continue;
			IL_F1:
				throw new NotImplementedException("The property type " + property.PropertyType.ToString() + " is invalid.");
			}
			return stringBuilder.ToString();
		}

		public override string ToString()
		{
			return ToStringInternal(0);
		}

		private readonly List<SteamAppProperty> _properties = new();
	}
}
