using System.Collections;
using System.Collections.Generic;

using UnityEngine.Assertions;

namespace Orazum.SpriteAtlas
{
    enum Turn
    {
        Left,
        Right
    }

    class BinaryTreePass : IEnumerable
    {
        List<Turn> _turns;
        int _current;

        public int AreaIncrease { get; set; }

        public BinaryTreePass(int capacity = 100)
        {
            _turns = new List<Turn>(100);
        }

        public BinaryTreePass Copy()
        {
            return new BinaryTreePass(_turns);
        }
        BinaryTreePass(List<Turn> turns)
        {
            _turns = new List<Turn>(turns);
        }

        public void Push(Turn turn)
        {
            _turns.Add(turn);
            _current = _turns.Count - 1;
        }

        public void Pop()
        {
            _turns.RemoveAt(_current);
            _current = _turns.Count - 1;
            Assert.IsTrue(_current >= 0);
        }

        public IEnumerator GetEnumerator()
        {
            return new BinaryTreePassEnumerator(_turns);
        }
    }

    class BinaryTreePassEnumerator : IEnumerator
    {
        List<Turn> _turns;
        int pos = -1;

        public BinaryTreePassEnumerator(List<Turn> turns)
        {
            _turns = turns;
        }

        public object Current => _turns[pos];

        public bool MoveNext()
        {
            pos++;
            return pos < _turns.Count;
        }

        public void Reset()
        {
            pos = -1;
        }
    }
}