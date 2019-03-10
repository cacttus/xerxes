using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proteus
{
    public class ItemNode<Tx>
    {
        public int RecursionStamp=0;
        public Tx Item = default(Tx);
        public ItemNode<Tx> Parent = null;
        public List<ItemNode<Tx>> Children = new List<ItemNode<Tx>>();
        
        public ItemNode(ItemNode<Tx> parent, Tx item)
        {
            Item = item;
        }
        public List<ItemNode<Tx>> FlattenBreadthFirst()
        {
            List<ItemNode<Tx>> ret = new List<ItemNode<Tx>>();
            
            FlattenBreadthFirst(this, ref ret);
            
            return ret;
        }
        public ItemNode<Tx> GetCopy(bool performDeepCopy = false)
        {
            if (performDeepCopy == true)
                throw new NotImplementedException();

            ItemNode<Tx> ret = new ItemNode<Tx>(Parent,Item);
            ret.RecursionStamp = RecursionStamp;
            ret.Children = Children;


            return ret;
        }

        private void FlattenBreadthFirst(ItemNode<Tx> parent, ref List<ItemNode<Tx>> ret)
        {
            foreach(ItemNode<Tx> c in parent.Children)
            {
                FlattenBreadthFirst(c, ref ret);
            }
            ret.Add(parent);
        }
    }
}
