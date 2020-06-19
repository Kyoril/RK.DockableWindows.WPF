using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docker
{
    public class DockWindowCollection : ObservableCollection<DockWindow>
    {
        private WindowGroup parent;


        #region Construction
        public DockWindowCollection(WindowGroup parent)
        {
            this.parent = parent;
        }
        #endregion



        #region Private methods
        private void OnDockWindowRemoved(DockWindow window)
        {
            this.parent.RemoveLogicalChildInternal(window);
        }
        private void OnDockWindowAdded(DockWindow window)
        {
            this.parent.AddLogicalChildInternal(window);
        }
        #endregion



        #region ObservableCollection overrides
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            this.parent.OnChildrenChanged();
        }
        protected override void ClearItems()
        {
            foreach(DockWindow window in this)
            {
                OnDockWindowRemoved(window);
            }

            base.ClearItems();
        }
        protected override void InsertItem(int index, DockWindow item)
        {
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
