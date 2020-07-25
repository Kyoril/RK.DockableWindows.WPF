using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Docker
{
    public class DockWindowCollection : ObservableCollection<DockWindow>
    {
        private readonly WindowGroup parent;


        #region Construction
        public DockWindowCollection(WindowGroup parent)
        {
            this.parent = parent;
        }
        #endregion



        #region Private methods
        private void OnDockWindowRemoved(DockWindow window)
        {
            parent.RemoveLogicalChildInternal(window);
        }
        private void OnDockWindowAdded(DockWindow window)
        {
            parent.AddLogicalChildInternal(window);
        }
        #endregion



        #region ObservableCollection overrides
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            parent.OnChildrenChanged();
        }

        protected override void ClearItems()
        {
            foreach (DockWindow window in this)
            {
                OnDockWindowRemoved(window);
            }

            base.ClearItems();
        }

        protected override void InsertItem(int index, DockWindow item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            OnDockWindowAdded(item);

            try
            {
                base.InsertItem(index, item);
            }
            catch
            {
                OnDockWindowRemoved(item);
            }
        }

        protected override void RemoveItem(int index)
        {
            DockWindow window = base[index];
            base.RemoveItem(index);

            OnDockWindowRemoved(window);
        }
        #endregion
    }
}
