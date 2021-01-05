using Livet;
using SteamTool.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamTool.Core.Common;

namespace SteamTools.ViewModels
{
	public class SteamAppSortWorker : ViewModel
	{
		#region static members
		public static readonly SortableColumn NoneColumn = new SortableColumn { Name = "None", KeySelector = null };
		public static readonly SortableColumn AppIdColumn = new SortableColumn { Name = "AppId", KeySelector = x => x.AppId, };
		public static readonly SortableColumn NameColumn = new SortableColumn { Name = "Name", KeySelector = x => x.Name, };
		public static readonly SortableColumn TypeColumn = new SortableColumn { Name = "Type", KeySelector = x => x.Type, };
		//public static readonly SortableColumn PlayTimeColumn = new SortableColumn { Name = "PlayTime", KeySelector = x => x.Level, DefaultIsDescending = true, };

		public static SortableColumn[] Columns { get; set; }

		static SteamAppSortWorker()
		{
			Columns = new[]
			{
				NoneColumn,
				AppIdColumn,
				NameColumn,
				TypeColumn,
			};
		}

		#endregion

		#region Selectors 変更通知

		private SortableColumnSelector[] _Selectors;

		public SortableColumnSelector[] Selectors
		{
			get { return this._Selectors; }
			set
			{
				if (this._Selectors != value)
				{
					this._Selectors = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion


		public SteamAppSortWorker()
		{
			this.UpdateSelectors();

			this.SetFirst(NameColumn);
		}

		public IEnumerable<SteamApp> Sort(IEnumerable<SteamApp> apps)
		{
			var selectors = this.Selectors.Where(x => x.Current.KeySelector != null).ToArray();
			if (selectors.Length == 0) return apps;

			var selector = selectors[0].Current.KeySelector;
			var orderedApps = selectors[0].IsAscending ? apps.OrderBy(selector) : apps.OrderByDescending(selector);

			for (var i = 1; i < selectors.Length; i++)
			{
				selector = selectors[i].Current.KeySelector;
				orderedApps = selectors[i].IsAscending ? orderedApps.ThenBy(selector) : orderedApps.ThenByDescending(selector);
			}

			return orderedApps;
		}

		public void SetFirst(SortableColumn column)
		{
			if (!this.Selectors.HasItems()) return;
			this.Selectors[0].SafeUpdate(column);
			this.Selectors[0].SafeUpdate(!column.DefaultIsDescending);
			this.UpdateSelectors();
		}

		public void Clear()
		{
			this.Selectors = null;
			this.UpdateSelectors();
		}

		private void UpdateSelectors(SortableColumnSelector target = null)
		{
			if (this.Selectors == null)
			{
				this.Selectors = Enumerable.Range(0, Columns.Length)
					.Select(_ => new SortableColumnSelector { Updated = x => this.UpdateSelectors(x), })
					.ToArray();
			}
			//选择非nonColumn的列
			var selectedItems = new HashSet<SortableColumn>();
			SortableColumnSelector previous = null;

			//是否启用是否在选择器中重新创建SelectableColumns
			//如果未指定目标，则重新创建所有内容；如果已指定，则从下一个目标重新创建
			var enabled = target == null;

			foreach (var selector in this.Selectors)
			{
				if (enabled)
				{
					var sortables = Columns.Where(x => !selectedItems.Contains(x)).ToList();
					var current = selector.Current;

					selector.SelectableColumns = sortables.ToArray();
					selector.SafeUpdate(sortables.Contains(current) ? current : sortables.FirstOrDefault());
				}
				else
				{
					enabled = selector == target;
				}

				if (selector.Current != NoneColumn)
				{
					selectedItems.Add(selector.Current);
				}

				previous = selector;
			}
		}
	}


	public class SortableColumnSelector : ViewModel
	{
		internal Action<SortableColumnSelector> Updated { get; set; }

		#region Current 変更通知

		private SortableColumn _Current;

		public SortableColumn Current
		{
			get { return this._Current; }
			set
			{
				if (this._Current != value)
				{
					this._Current = value;
					this.SafeUpdate(!value.DefaultIsDescending);
					this.Updated(this);
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region SelectableColumns 変更通知

		private SortableColumn[] _SelectableColumns;

		public SortableColumn[] SelectableColumns
		{
			get { return this._SelectableColumns; }
			set
			{
				if (this._SelectableColumns != value)
				{
					this._SelectableColumns = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region IsAscending / IsDescending 変更通知

		private bool _IsAscending = true;

		public bool IsAscending
		{
			get { return this._IsAscending; }
			set
			{
				if (this._IsAscending != value)
				{
					this._IsAscending = value;
					this.Updated(this);
					this.RaisePropertyChanged();
					this.RaisePropertyChanged(nameof(this.IsDescending));
				}
			}
		}

		public bool IsDescending => !this.IsAscending;

		#endregion

		internal void SafeUpdate(SortableColumn column)
		{
			this._Current = column;
			this.RaisePropertyChanged(nameof(this.Current));
		}

		internal void SafeUpdate(bool isAscending)
		{
			this._IsAscending = isAscending;
			this.RaisePropertyChanged(nameof(this.IsAscending));
			this.RaisePropertyChanged(nameof(this.IsDescending));
		}
	}

	public class SortableColumn
	{
		public string Name { get; set; }
		public bool DefaultIsDescending { get; set; }
		public Func<SteamApp, object> KeySelector { get; set; }
	}
}
