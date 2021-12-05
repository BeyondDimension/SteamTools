// Copyright (c) 2021 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using AndroidX.RecyclerView.Widget;
using DynamicData;
using System;
using System.Linq;
using System.Reactive.Linq;

// https://github.com/reactiveui/ReactiveUI/blob/main/src/ReactiveUI.AndroidX/ReactiveRecyclerViewAdapter.cs

// ReSharper disable once CheckNamespace
namespace ReactiveUI.AndroidX
{
    /// <summary>
    /// An adapter for the Android <see cref="RecyclerView"/>.
    /// </summary>
    /// <typeparam name="TViewModel">The type of ViewModel that this adapter holds.</typeparam>
    public abstract class ReactiveRecyclerViewAdapter2<TViewModel> : RecyclerView.Adapter
        where TViewModel : class, IReactiveObject
    {
        private readonly ISourceList<TViewModel> _list;

        private readonly IDisposable? _inner;

        protected ReactiveRecyclerViewAdapter2(ISourceList<TViewModel> list, IDisposable? inner = null)
        {
            _list = list;
            _inner = inner;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReactiveRecyclerViewAdapter2{TViewModel}"/> class.
        /// </summary>
        /// <param name="backingList">The backing list.</param>
        protected ReactiveRecyclerViewAdapter2(IObservable<IChangeSet<TViewModel>> backingList)
        {
            _list = new SourceList<TViewModel>(backingList);

            _inner = _list
                            .Connect()
                            .ForEachChange(UpdateBindings)
                            .Subscribe();
        }

        /// <inheritdoc/>
        public override int ItemCount => _list.Count;

        /// <inheritdoc/>
        public override int GetItemViewType(int position) => GetItemViewType(position, GetViewModelByPosition(position));

        /// <summary>
        /// Determine the View that will be used/re-used in lists where
        /// the list contains different cell designs.
        /// </summary>
        /// <param name="position">The position of the current view in the list.</param>
        /// <param name="viewModel">The ViewModel associated with the current View.</param>
        /// <returns>An ID to be used in OnCreateViewHolder.</returns>
        public virtual int GetItemViewType(int position, TViewModel? viewModel) => base.GetItemViewType(position);

        /// <inheritdoc/>
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is null)
            {
                throw new ArgumentNullException(nameof(holder));
            }

            if (!(holder is IViewFor viewForHolder))
            {
                throw new ArgumentException("Holder must be derived from IViewFor", nameof(holder));
            }

            viewForHolder.ViewModel = GetViewModelByPosition(position);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inner?.Dispose();
                _list.Dispose();
            }

            base.Dispose(disposing);
        }

        private TViewModel? GetViewModelByPosition(int position) => position >= _list.Count ? null : _list.Items.ElementAt(position);

        private void UpdateBindings(Change<TViewModel> change)
        {
            switch (change.Reason)
            {
                case ListChangeReason.Add:
                    NotifyItemInserted(change.Item.CurrentIndex);
                    break;
                case ListChangeReason.Remove:
                    NotifyItemRemoved(change.Item.CurrentIndex);
                    break;
                case ListChangeReason.Moved:
                    NotifyItemMoved(change.Item.PreviousIndex, change.Item.CurrentIndex);
                    break;
                case ListChangeReason.Replace:
                case ListChangeReason.Refresh:
                    NotifyItemChanged(change.Item.CurrentIndex);
                    break;
                case ListChangeReason.AddRange:
                    NotifyItemRangeInserted(GetPositionStart(change), change.Range.Count);
                    break;
                case ListChangeReason.RemoveRange:
                case ListChangeReason.Clear:
                    NotifyItemRangeRemoved(GetPositionStart(change), change.Range.Count);
                    break;
            }
        }

        private static int GetPositionStart(Change<TViewModel> change) => change.Range.Index < 0 ? 0 : change.Range.Index;
    }
}