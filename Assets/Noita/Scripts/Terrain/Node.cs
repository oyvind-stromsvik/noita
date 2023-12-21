namespace Noita {

    /// <summary>
    /// A node in the map grid.
    ///
    /// Each pixel of the source texture becomes a node in the grid.
    /// 
    /// I can't remember exactly why I called this Node. I think it was because
    /// I followed Sharp Accent's Lemmings tutorial initially, where he refered
    /// to this as a node. Ref. https://sharpaccent.com/?c=course&id=19
    /// </summary>
    public class Node {
        public int x;
        public int y;
        public bool isEmpty;
    }

}
