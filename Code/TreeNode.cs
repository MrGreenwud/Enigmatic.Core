using System;
using System.Collections.Generic;
using ENIX;

namespace Enigmatic.Core
{
    public class TreeNode<T> where T : TreeNode<T>
    {
        [SerializebleProperty] private T m_Parent;
        [SerializebleProperty] private List<T> m_Children = new List<T>();

        public uint DepthLevel { get; private set; }

        public T Parent => m_Parent;
        public int ChildCount => m_Children.Count;

        public T GetChild(int index)
        {
            if (index < 0 || index >= m_Children.Count)
                throw new ArgumentOutOfRangeException("index");

            return m_Children[index];
        }
        public T[] GetChildren() => m_Children.ToArray();

        public List<T> Children => m_Children;

        public int IndexOf(T node)
        {
            return m_Children.IndexOf(node);
        }

        public bool Insert(int index, T node)
        {
            if (OnValidAddChild(node) == false)
                return false;

            node.Reparent(this as T);
            m_Children.Insert(index, node);
            return true;
        }

        public bool AddChild(T node)
        {
            if (OnValidAddChild(node) == false)
                return false;

            node.Reparent(this as T);
            m_Children.Add(node);
            return true;
        }

        public void RemoveChild(T node)
        {
            if (m_Children.Remove(node) == false)
                return;

            node.m_Parent = null;
            node.DepthLevel = 0;
        }

        public void ForEach(Action<T> action)
        {
            foreach (T child in m_Children)
                action?.Invoke(child);
        }

        public void SetParent(T parent)
        {
            if (parent == null)
                throw new ArgumentNullException();

            if (m_Parent != null)
                throw new InvalidOperationException();

            m_Parent = parent;
            UpdateLevel();
        }

        public void Reparent(T parent)
        {
            if (parent == null)
                throw new ArgumentNullException();

            if (m_Parent != null)
            {
                m_Parent.RemoveChild(this as T);
                m_Parent = null;
            }

            m_Parent = parent;
            UpdateLevel();
        }

        public bool IsAncestor(T node)
        {
            T current = this as T;
            while (current != null)
            {
                if (current == node)
                    return true;

                current = current.m_Parent;
            }
            return false;
        }

        public bool OnValidAddChild(T node)
        {
            return !(node == this || node == null
                || m_Children.Contains(node) || IsAncestor(node));
        }

        public void UpdateLevel()
        {
            DepthLevel = m_Parent == null ? 0 : m_Parent.DepthLevel + 1;

            foreach (T child in m_Children)
                child.UpdateLevel();
        }

        public void UpdateAllLevel()
        {
            foreach(T node in m_Children)
            {
                node.UpdateLevel();
                node.UpdateAllLevel();
            }
        }
    }
}