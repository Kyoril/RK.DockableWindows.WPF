using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;


namespace Docker
{
    /// <summary>
    /// A class which contains content elements of a SplitContainer.
    /// </summary>
    public class SplitContainerContentCollection : IList
    {
        private readonly SplitContainer parent;
        private readonly List<FrameworkElement> contentList;


        #region Construction
        public SplitContainerContentCollection(SplitContainer parent)
        {
            this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
            contentList = new List<FrameworkElement>();
        }
        #endregion



        #region Public methods
        public int Add(FrameworkElement element)
        {
            Insert(contentList.Count, element);
            return contentList.Count - 1;
        }

        public void Insert(int index, FrameworkElement element)
        {
            VerifyElement(element);

            // Children start to change...
            OnChildrenChanging();

            // Insert element into child list
            contentList.Insert(index, element);

            // Add element as both logical and visual child
            parent.AddLogicalChildInternal(element);
            parent.AddVisualChildInternal(element);

            // Change finished
            OnChildrenChanged();
        }

        public bool Contains(FrameworkElement element)
        {
            return contentList.Contains(element);
        }

        public int IndexOf(FrameworkElement element)
        {
            return contentList.IndexOf(element);
        }

        public void Remove(FrameworkElement element)
        {
            OnChildrenChanging();

            parent.RemoveLogicalChildInternal(element);
            parent.RemoveVisualChildInternal(element);
            contentList.Remove(element);

            OnChildrenChanged();
        }
        #endregion



        #region Private methods
        /// <summary>
        /// Verifies that an element has been given and that the given element has a supported type.
        /// </summary>
        /// <param name="element">The element instance to verify.</param>
        private void VerifyElement(FrameworkElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }
            if (!(element is SplitContainer) && !(element is WindowGroup))
            {
                throw new ArgumentException(@"Only SplitContainer and WindowGroup elements are allowed to be children of a SplitContainer element!", nameof(element));
            }
        }
        #endregion



        #region Properties
        /// <summary>
        /// Gets or sets the capacity.
        /// </summary>
        public int Capacity
        {
            get => contentList.Capacity;
            set => contentList.Capacity = value;
        }

        public FrameworkElement this[int index]
        {
            get => contentList[index];
            set => contentList[index] = value;
        }
        #endregion



        #region Private methods
        private void OnChildrenChanging()
        {
            // Although this is a one liner, behavior might change later on and the logic
            // behind this method is used quite often in this class. That's why this line
            // is encapsuled in a method.
            parent.OnChildrenChanging();
        }

        private void OnChildrenChanged()
        {
            // Se OnChildrenChanging note on why this one liner is a separate method.
            parent.OnChildrenChanged();
        }
        #endregion


        #region IList implementation
        object IList.this[int index]
        {
            get => this[index];
            set => this[index] = value as FrameworkElement;
        }

        public bool IsReadOnly => false;

        public bool IsFixedSize => false;

        public int Count => contentList.Count;

        public object SyncRoot => this;

        public bool IsSynchronized => false;

        public int Add(object value)
        {
            return Add(value as FrameworkElement);
        }

        public void Clear()
        {
            OnChildrenChanging();

            // Remove registered elements from the parent's visual and logical child list
            // before clearing the list.
            foreach (FrameworkElement element in contentList)
            {
                parent.RemoveLogicalChildInternal(element);
                parent.RemoveVisualChildInternal(element);
            }

            contentList.Clear();

            OnChildrenChanged();
        }

        public bool Contains(object value)
        {
            return Contains(value as FrameworkElement);
        }

        public void CopyTo(Array array, int index) => contentList.CopyTo((FrameworkElement[])array, index);

        public IEnumerator GetEnumerator() => contentList.GetEnumerator();

        public int IndexOf(object value)
        {
            return IndexOf(value as FrameworkElement);
        }

        public void Insert(int index, object value)
        {
            Insert(index, value as FrameworkElement);
        }

        public void Remove(object value)
        {
            Remove(value as FrameworkElement);
        }

        public void RemoveAt(int index)
        {
            Remove(contentList[index]);
        }
        #endregion
    }
}
