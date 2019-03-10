using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proteus
{

    /// <summary>
    /// Generic Tree with multiple roots
    /// No duplicates may be in this tree
    /// </summary>
    /// <typeparam name="Tx"></typeparam>
    public class ItemTree<Tx>
    {
        public delegate void ItemTreeIterationDelegate(ItemNode<Tx> node);
        public delegate bool BComp(Tx item);

        public List<ItemNode<Tx>> Roots = new List<ItemNode<Tx>>();

        public ItemTree()
        {
        }

        public ItemTree(ItemTree<Tx> copyFrom, bool performItemCopy = false)
        {
            if (performItemCopy == true)
                throw new NotImplementedException();// not supported.

            Roots = CopyBranches(copyFrom.Roots);
        }
        /// <summary>
        /// </summary>
        /// <param name="fn"></param>
        public void IterateBreadthFirst(ItemTreeIterationDelegate fn)
        {
            foreach(ItemNode<Tx> r in Roots)
            {
                IterateBreadthFirst_r(r, fn);
            }
        }
        public void IterateDepthFirst(ItemTreeIterationDelegate fn)
        {
            foreach (ItemNode<Tx> r in Roots)
            {
                IterateDepthFirst_r(r, fn);
            }
        }

        /// <summary>
        /// Copies all children of Parent, and returns the copy.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="children"></param>
        /// <param name="performItemCopy"></param>
        /// <returns></returns>
        public List<ItemNode<Tx>> CopyBranches(List<ItemNode<Tx>> children, ItemNode<Tx> parent = null, bool performItemCopy = false)
        {
            List<ItemNode<Tx>> ret = new List<ItemNode<Tx>>();

            if (performItemCopy == true)
                throw new NotImplementedException();

            foreach(ItemNode<Tx> ch in children)
            {
                ItemNode<Tx> newNode = ch.GetCopy(performItemCopy);

                newNode.Parent = parent;
                newNode.Children = CopyBranches(ch.Children, ch);
                ret.Add(newNode);
            }

            return ret;
        }

        // - Gets a list of all items in breadth first
        // traversal.  This will give us a list of all dependencies first
        // and the exes / items last.
        public List<Tx> FlattenBreadthFirst()
        {
            List<Tx> ret = new List<Tx>();
            if(Roots.Count==0)
                return ret;

            foreach(ItemNode<Tx> item in Roots)
                GetBreadthFirstList_r(item, ref ret);

            return ret;
        }
        // Basic build -- constructs a tree from parent/child relationships, adding nodes if they do not exist
        //  and linking existing nodes.
        public void Build(Tx txParent, // the node that parents the given children.
                          List<Tx> txChildren, // children of the input parent
                          Tx parentOfParent = default(Tx), // optional parent to find in the tree and parent this parent.
                          bool blnLinkExistingParents = true, // if input child nodes have existing parents (and are all the same) we link the input parent as a chidl of their parents.
                          bool blnThrowIfParentAlreadyContainsChild = true // If the input parent has any of the children alrady, throw.
                        )
        {
            List<ItemNode<Tx>> childList;
            ItemNode<Tx> parentNode;
            ItemNode<Tx> sameParent;
            bool blnSameParents;
            
            childList = new List<ItemNode<Tx>>();
            sameParent = null;
            blnSameParents = true;

            parentNode = Find(txParent);
            if (parentNode == null)
                parentNode = Insert(txParent, parentOfParent);

            foreach (Tx child in txChildren)
            {
                ItemNode<Tx> foundChild = Find(child);

                // - Check if parents are uniform
                if (sameParent == null)
                    sameParent = foundChild.Parent;
                else if ((foundChild.Parent!=null) && (foundChild.Parent!= sameParent))
                    blnSameParents = false;

                ItemNode<Tx> mustBeNull = Find(txParent, foundChild);
                if (mustBeNull != null)
                    throw new Exception("[ItemTree] Tree tried to link a circular dependency.  One or more child nodes was a parent of the input node.");
                
                childList.Add(foundChild);
            }

            if (blnSameParents == false)
                throw new Exception("[ItemTree] Could not build tree. Input child nodes had varying parents which would make the tree circular.");

            foreach (ItemNode<Tx> child in childList)
            {
                if (child.Parent != null)
                {
                    if (blnLinkExistingParents == true)
                    {
                        ItemNode<Tx> tp = child.Parent;

                        if (!tp.Children.Contains(child))
                            throw new Exception("[ItemTree] Integrity constraint violated.  Parent did not contain referenced child.");

                        tp.Children.Remove(child);
                        tp.Children.Add(parentNode);

                        if (parentNode.Parent != null)
                        {
                            parentNode.Parent.Children.Remove(parentNode);
                        }
                        else if (Roots.Contains(parentNode))
                        {
                            Roots.Remove(parentNode);
                        }

                        parentNode.Parent = tp;
                    }
                    else
                    {
                        throw new Exception("[ItemTree] Child found which already had a parent.  Link constraint does not allow this.");
                    }
                }
                else if (Roots.Contains(child))
                {
                    Roots.Remove(child);
                }

                if (parentNode.Children.Contains(child))
                {
                    if (blnThrowIfParentAlreadyContainsChild)
                        throw new Exception("[ItemTree] Tried to add a group of children nodes of which some already had the specified parent.");
                }
                else
                {
                    parentNode.Children.Add(child);
                }

                child.Parent = parentNode;
            }
        }
        public ItemNode<Tx> Insert(Tx txChild, Tx txParent = default(Tx))
        {
            // add bt as a child of txParent
            ItemNode<Tx> ret;

            if (Find(txChild) != null)
                throw new Exception("[ItemTree] Tried to insert a duplicate node.");
            
            ItemNode<Tx> pNode;

            if ((txChild != null) && (txParent == null))
            {
                Roots.Add(ret = new ItemNode<Tx>(null, txChild));
                return ret;
            }
            else if (txChild == null && txParent != null)
            {
                // parentize existing nodes.
                // we could do this if we added Link() and another parameter - but no
                throw new NotImplementedException();
            }
            else if (txChild != null && txParent != null)
            {
                //child-ize
                pNode = Find(txParent);

                if (pNode == null)
                    throw new Exception("[ItemTree] Failed to find parent node while inserting child.");

                ret = new ItemNode<Tx>(pNode, txChild);

                if (Roots.Contains(ret))
                    Roots.Remove(ret);

                ret.Parent = pNode;
                pNode.Children.Add(ret);
            }
            else
                throw new Exception("[ItemTree] Invalid arguemnts to insert()");
            

            return ret;
        }
        public ItemNode<Tx> Remove(Tx item)
        {
            ItemNode<Tx> node = Find(item);
            Remove(node);
            return node;
        }
        public void Remove(ItemNode<Tx> node, bool blnLinkChildren = false)
        {
            // if blnLinkChildren is true we will link the children of the 
            // removed node to the removed node's existing parent (or Root if it has none)

            ItemNode<Tx> parent = node.Parent;

            if (parent != null)
            {
                if (!parent.Children.Contains(node))
                    throw new Exception("[ItemTree] integrity violated.. child parent reference did not contain child.");
                parent.Children.Remove(node);
                node.Parent = null;
            }
            else
            {
                if(!Roots.Contains(node))
                    throw new Exception("[ItemTree] integrity violated.. child parent reference (Roots) did not contain child.");
                Roots.Remove(node);
            }

            if (blnLinkChildren == true)
            {
                foreach(ItemNode<Tx> c in node.Children)
                {
                    parent.Children.Add(c);
                    c.Parent = parent;
                }
            }
        }
        public ItemNode<Tx> Find(Tx bt, ItemNode<Tx> parent = null)
        {
            ItemNode<Tx> found;

            if (bt == null)
                throw new Exception("[ItemTree] Inserting, Child was null");
            found = null;

            List<ItemNode<Tx>> findRoots;

            if (parent == null)
                findRoots = Roots;
            else
                findRoots = new List<ItemNode<Tx>>() { parent };

            foreach(ItemNode<Tx> item in findRoots)
                Find_r(bt, item, ref found);

            return found;
        }
        public ItemNode<Tx> FirstChildWithoutChildren(ItemNode<Tx> parent = null, BComp bc = null)
        {
            ItemNode<Tx> ret = null;
            FirstChildWc_r(ref ret, null, bc);
            return ret;
        }
        public void Prune(Tx objItem, bool blnThrowIfNodeHasChildren = true)
        {
            ItemNode<Tx> ret = Find(objItem);
            if (ret == null)
                throw new Exception("Failed to find Tree item for prune.");
            Prune_r(ret, blnThrowIfNodeHasChildren);
        }
        public int GetNodeCount(BComp predicat = null)
        {
            int ret = 0;
            GetNodeCount_r(Roots, ref ret );
            return ret;
        }

        #region PRIVATE_INTERNAL
        private void GetNodeCount_r(List<ItemNode<Tx>> nodes, ref int count, BComp predicate = null)
        {
            foreach (ItemNode<Tx> n in nodes)
            {
                if (n.Children.Count > 0)
                    GetNodeCount_r(n.Children, ref count);
                if ((predicate == null) || (predicate(n.Item) == true))
                    count++;
            }
        }
        private void Prune_r(ItemNode<Tx> objItem, bool blnThrowIfNodeHasChildren = true, int intRecursionStamp = -1)
        {
            if (objItem == null)
                throw new Exception("[ItemTree] Tree failed to find given item." + objItem.ToString());
            
            //Set stamp so we don't infinite loop
            if (intRecursionStamp == -1)
                intRecursionStamp = System.Environment.TickCount;
            
            if (objItem.RecursionStamp == intRecursionStamp)
                return;
           
            objItem.RecursionStamp = intRecursionStamp;

            if (blnThrowIfNodeHasChildren && (objItem.Children.Count > 0))
                throw new Exception("[ItemTree] Tried to prune tree item that had children: " + objItem.ToString());

            if (objItem.Parent == null)
            {
                if (Roots.Contains(objItem))
                {
                    Roots.Remove(objItem);
                    return;
                }
                else
                    throw new Exception("[ItemTree] Tree item had no parent, but was not a root node. Tree has a problem somewhre.");
            }

            foreach (ItemNode<Tx> child in objItem.Children)
            {
                Prune_r(child, blnThrowIfNodeHasChildren, intRecursionStamp);
            }
            objItem.Parent.Children.Remove(objItem);
            
        }
        private void FirstChildWc_r(ref ItemNode<Tx> objFound, ItemNode<Tx> parent = null, BComp predicate = null)
        {
            if (objFound != null)
                return;
            if (parent == null)
            {
                if (Roots.Count == 0)
                {
                    objFound = null;
                    return;
                }
                foreach (ItemNode<Tx> item in Roots)
                {
                    FirstChildWc_r(ref objFound, item, predicate);
                }
            }
            else
            {
                if ((parent.Children.Count == 0) && ((predicate == null) || (predicate(parent.Item) == true)))
                {
                    objFound = parent;
                }
                else
                {
                    foreach(ItemNode<Tx> nod in parent.Children)
                    {
                        FirstChildWc_r(ref objFound, nod, predicate);

                    }
                }
            }
        }
        private void GetBreadthFirstList_r(ItemNode<Tx> parent, ref List<Tx> outList)
        {
            foreach (ItemNode<Tx> child in parent.Children)
            {
                GetBreadthFirstList_r(child, ref outList);
            }
            outList.Add(parent.Item);
        }

        private void Find_r(Tx bt, ItemNode<Tx> parent, ref ItemNode<Tx> found)
        {
            if (found != null)
                return;

            if (parent.Item.Equals(bt))
            {
                found = parent;
                return;
            }

            foreach (ItemNode<Tx> n in parent.Children)
            {
                if (found != null)
                    break;
 
                Find_r(bt, n, ref found);
            }

        }
        private void IterateBreadthFirst_r(ItemNode<Tx> parent, ItemTreeIterationDelegate fn)
        {
            fn(parent);
            foreach (ItemNode<Tx> c in parent.Children)
            {
                IterateBreadthFirst_r(c, fn);
            }
        }
        private void IterateDepthFirst_r(ItemNode<Tx> parent, ItemTreeIterationDelegate fn)
        {
            foreach (ItemNode<Tx> c in parent.Children)
            {
                IterateDepthFirst_r(c, fn);
            }
            fn(parent);
        }
        #endregion
    }
}
