using System;
using System.Collections;
using System.Collections.Generic;

namespace UCalc.Data
{
    public class RecentlyOpenedItem
    {
        public string DisplayText { get; }
        public string Path { get; }

        public RecentlyOpenedItem(string path)
        {
            DisplayText = System.IO.Path.GetFileNameWithoutExtension(path);
            Path = path;
        }

        private bool Equals(RecentlyOpenedItem other)
        {
            return DisplayText == other.DisplayText && Path == other.Path;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((RecentlyOpenedItem) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DisplayText, Path);
        }

        public override string ToString()
        {
            return DisplayText;
        }
    }

    public class RecentlyOpenedList : ICollection<RecentlyOpenedItem>
    {
        private readonly List<RecentlyOpenedItem> _list;

        public RecentlyOpenedList()
        {
            _list = new List<RecentlyOpenedItem>();
        }

        public IEnumerator<RecentlyOpenedItem> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public void Add(RecentlyOpenedItem item)
        {
            var index = _list.IndexOf(item);
            if (index != -1)
            {
                _list.RemoveAt(index);
            }

            _list.Insert(0, item);

            if (_list.Count > 10)
            {
                _list.RemoveAt(10);
            }
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(RecentlyOpenedItem item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(RecentlyOpenedItem[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(RecentlyOpenedItem item)
        {
            return _list.Remove(item);
        }

        public int Count => _list.Count;
        public bool IsReadOnly => false;
    }
}