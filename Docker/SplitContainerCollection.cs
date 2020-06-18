using System;
using System.Collections;
using System.Windows.Media;

namespace Docker
{
    /// <summary>
    /// A class which collects multiple SplitContainer elements. This class is currently used in
    /// the DockCanvas class to contain all SplitContainers and allow them to be set up properly 
    /// using Xaml.
    /// </summary>
    public class SplitContainerCollection : ICollection, IList
    {
        private readonly DockCanvas parent;
        private VisualCollection collection;


        #region Construction
        public SplitContainerCollection(DockCanvas parent, Visual visualParent)
        {
            this.parent = parent;
            this.collection = new VisualCollection(visualParent);
        }
        #endregion


        #region Public Methods
        public int Add(SplitContainer container)
        {
            int result = this.collection.Add(container);
            return result;
        }
        public void Remove(SplitContainer container)
        {
            this.collection.Remove(container);
        }
        public bool Contains(SplitContainer container)
        {
            return this.collection.Contains(container);
        }
        public int IndexOf(SplitContainer container)
        {
            return this.collection.IndexOf(container);
        }
        public void Insert(int index, SplitContainer container)
        {
            this.collection.Insert(index, container);
        }
        public SplitContainer this[int index]
        {
            get => this.collection[index] as SplitContainer;
            set => throw new NotImplementedException();
        }
        #endregion


        #region IList implementation
        public bool IsReadOnly => false;
        public bool IsFixedSize => false;
        public int Add(object value)
        {
            return this.Add(value as SplitContainer);
        }
        public void Clear()
        {
            foreach(SplitContainer container in this.collection)
            {
                // TODO: Unregister container
            }

            this.collection.Clear();
        }
        public bool Contains(object value)
        {
            return this.Contains(value as SplitContainer);
        }
        public int IndexOf(object value)
        {
            return this.IndexOf(value as SplitContainer);
        }
        public void Insert(int index, object value)
        {
            this.Insert(index, value as SplitContainer);
        }
        public void Remove(object value)
        {
            this.Remove(value as SplitContainer);
        }
        public void RemoveAt(int index)
        {
            this.collection.RemoveAt(index);
        }
        object IList.this[int index]
        {
            get => this[index];
            set => throw new NotImplementedException();
        }
        #endregion


        #region ICollection implementation
        public int Count => this.collection.Count;
        public object SyncRoot => this.collection.SyncRoot;
        public bool IsSynchronized => this.collection.IsSynchronized;
        public void CopyTo(Array array, int index) => this.collection.CopyTo(array, index);
        public IEnumerator GetEnumerator() => this.collection.GetEnumerator();
        #endregion
    }
}
