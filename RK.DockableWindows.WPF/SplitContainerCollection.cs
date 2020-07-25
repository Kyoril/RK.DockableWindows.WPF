using System;
using System.Collections;
using System.Windows.Media;

namespace RK.DockableWindows.WPF
{
    /// <summary>
    /// A class which collects multiple SplitContainer elements. This class is currently used in
    /// the DockCanvas class to contain all SplitContainers and allow them to be set up properly 
    /// using Xaml.
    /// </summary>
    public class SplitContainerCollection : IList
    {
        private readonly DockCanvas parent;
        private readonly VisualCollection collection;


        #region Construction
        public SplitContainerCollection(DockCanvas parent, Visual visualParent)
        {
            this.parent = parent;
            collection = new VisualCollection(visualParent);
        }
        #endregion


        #region Internal methods
        private void InvalidateParent()
        {
            parent.InvalidateMeasure();
            parent.OnSplitContainersChanged();
        }

        private void OnSplitContainerAdded(SplitContainer splitContainer)
        {
            parent.InternalAddLogicalChild(splitContainer);
        }

        private void OnSplitContainerRemoved(SplitContainer splitContainer)
        {
            parent.InternalRemoveLogicalChild(splitContainer);
        }
        #endregion


        #region Public Methods
        public int Add(SplitContainer container)
        {
            OnSplitContainerAdded(container);

            int result = collection.Add(container);
            InvalidateParent();

            return result;
        }
        public void Remove(SplitContainer container)
        {
            OnSplitContainerRemoved(container);
            collection.Remove(container);
            InvalidateParent();
        }

        public bool Contains(SplitContainer container)
        {
            return collection.Contains(container);
        }

        public int IndexOf(SplitContainer container)
        {
            return collection.IndexOf(container);
        }

        public void Insert(int index, SplitContainer container)
        {
            OnSplitContainerAdded(container);

            collection.Insert(index, container);
            InvalidateParent();
        }

        public SplitContainer this[int index]
        {
            get => collection[index] as SplitContainer;
            set => throw new NotImplementedException();
        }
        #endregion


        #region IList implementation
        public bool IsReadOnly => false;

        public bool IsFixedSize => false;

        public int Add(object value)
        {
            return Add(value as SplitContainer);
        }

        public void Clear()
        {
            foreach (var visual in collection)
            {
                var container = (SplitContainer)visual;
                OnSplitContainerRemoved(container);
            }

            collection.Clear();
            InvalidateParent();
        }

        public bool Contains(object value)
        {
            return Contains(value as SplitContainer);
        }

        public int IndexOf(object value)
        {
            return IndexOf(value as SplitContainer);
        }

        public void Insert(int index, object value)
        {
            Insert(index, value as SplitContainer);
        }

        public void Remove(object value)
        {
            Remove(value as SplitContainer);
        }

        public void RemoveAt(int index)
        {
            OnSplitContainerRemoved(this[index]);

            collection.RemoveAt(index);
            InvalidateParent();
        }

        object IList.this[int index]
        {
            get => this[index];
            set => throw new NotImplementedException();
        }
        #endregion


        #region ICollection implementation
        public int Count => collection.Count;

        public object SyncRoot => collection.SyncRoot;

        public bool IsSynchronized => collection.IsSynchronized;

        public void CopyTo(Array array, int index) => collection.CopyTo(array, index);

        public IEnumerator GetEnumerator() => collection.GetEnumerator();
        #endregion
    }
}
