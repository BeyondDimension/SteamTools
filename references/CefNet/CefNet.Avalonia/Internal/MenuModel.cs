using System;
using System.Collections.Generic;
using System.Text;

namespace CefNet.Internal
{
	internal sealed class MenuModel
	{
		private sealed class MenuModelItem
		{
			public int Command;
			public string Label;
			public CefMenuItemType Type;
			public int Group;
			public bool Visible;
			public bool Enabled;
			public bool Checked;
			public CefColor[] Colors;
			public Accelerator Accelerator;
			public MenuModel SubMenu;
		}

		private sealed class Accelerator
		{
			public int KeyCode;
			public bool Alt;
			public bool Shift;
			public bool Ctrl;
		}

		private MenuModelItem[] _items;

		private MenuModel(MenuModelItem[] items)
		{
			_items = items;
		}

		public static MenuModel FromCefMenu(CefMenuModel model)
		{
			var items = new MenuModelItem[model.Count];
			CopyItems(model, items);
			return new MenuModel(items);
		}

		private static void CopyItems(CefMenuModel model, MenuModelItem[] dest)
		{
			for (int i = 0; i < dest.Length; i++)
			{
				var item = new MenuModelItem
				{
					Command = model.GetCommandIdAt(i),
					Label = model.GetLabelAt(i),
					Type = model.GetTypeAt(i),
					Group = model.GetGroupIdAt(i),
					Visible = model.IsVisibleAt(i),
					Enabled = model.IsEnabledAt(i),
					Checked = model.IsCheckedAt(i),
				};

				CefColor color = default;
				item.Colors = new CefColor[(int)CefMenuColorType.Count];
				for (int j = 0; j < item.Colors.Length; j++)
				{
					item.Colors[j] = model.GetColorAt(i, (CefMenuColorType)j, ref color) ? color : default;
				}

				int keycode = 0, alt = 0, ctrl = 0, shift = 0;
				if (model.GetAcceleratorAt(i, ref keycode, ref shift, ref ctrl, ref alt))
				{
					item.Accelerator = new Accelerator { KeyCode = keycode, Alt = alt != 0, Ctrl = ctrl != 0, Shift = shift != 0 };
				}

				if (item.Type == CefMenuItemType.Submenu)
				{
					CefMenuModel submenu = model.GetSubMenuAt(i);
					var items = new MenuModelItem[submenu.Count];
					CopyItems(submenu, items);
					item.SubMenu = new MenuModel(items) { IsSubMenu = true };
				}
				dest[i] = item;
			}
		}

		/// <summary>
		/// Returns true if this menu is a submenu.
		/// </summary>
		public bool IsSubMenu { get; private set; }

		/// <summary>
		/// Returns the number of items in this menu.
		/// </summary>
		public int Count
		{
			get { return _items.Length; }
		}

		/// <summary>
		/// Returns the command id at the specified |index| or -1 if not found due to
		/// invalid range or the index being a separator.
		/// </summary>
		public int GetCommandIdAt(int index)
		{
			return _items[index].Command;
		}

		/// <summary>
		/// Returns the label at the specified |index| or NULL if not found due to
		/// invalid range or the index being a separator.
		/// The resulting string must be freed by calling cef_string_userfree_free().
		/// </summary>
		public string GetLabelAt(int index)
		{
			return _items[index].Label;
		}

		/// <summary>
		/// Returns the item type at the specified |index|.
		/// </summary>
		public CefMenuItemType GetTypeAt(int index)
		{
			return _items[index].Type;
		}

		/// <summary>
		/// Returns the group id at the specified |index| or -1 if invalid.
		/// </summary>
		public int GetGroupIdAt(int index)
		{
			return _items[index].Group;
		}

		/// <summary>
		/// Returns the submenu at the specified |index| or NULL if invalid.
		/// </summary>
		public MenuModel GetSubMenuAt(int index)
		{
			return _items[index].SubMenu;
		}

		/// <summary>
		/// Returns true if the specified |index| is visible.
		/// </summary>
		public bool IsVisibleAt(int index)
		{
			return _items[index].Visible;
		}

		/// <summary>
		/// Returns true if the specified |index| is enabled.
		/// </summary>
		public bool IsEnabledAt(int index)
		{
			return _items[index].Enabled;
		}

		/// <summary>
		/// Returns true if the specified |index| is checked. Only applies to check
		/// and radio items.
		/// </summary>
		public bool IsCheckedAt(int index)
		{
			return _items[index].Checked;
		}

		/// <summary>
		/// Returns true if the specified |index| has a keyboard accelerator
		/// assigned.
		/// </summary>
		public bool HasAcceleratorAt(int index)
		{
			return _items[index].Accelerator != null;
		}

		/// <summary>
		/// Retrieves the keyboard accelerator for the specified |index|. Returns true on success.
		/// </summary>
		public bool GetAcceleratorAt(int index, out int keyCode, out bool shift, out bool ctrl, out bool alt)
		{
			Accelerator accelerator = _items[index].Accelerator;
			if (accelerator == null)
			{
				keyCode = 0;
				shift = false;
				ctrl = false;
				alt = false;
				return false;
			}

			keyCode = accelerator.KeyCode;
			shift = accelerator.Shift;
			ctrl = accelerator.Ctrl;
			alt = accelerator.Alt;
			return true;
		}

		/// <summary>
		/// Returns in |color| the color that was explicitly set for |command_id| and
		/// |color_type|. Specify an |index| value of -1 to return the default color in
		/// |color|. If a color was not set then 0 will be returned in |color|. Returns
		/// true on success.
		/// </summary>
		public bool GetColorAt(int index, CefMenuColorType colorType, out CefColor color)
		{
			color = _items[index].Colors[(int)colorType];
			return color != 0;
		}

	}
}
