using System.Collections.Generic;

namespace Murtlap
{
    class WordItem : Word
    {
        public WordItem() : base("")
        {

        }

        public WordItem(string value) : base(value)
        {
            contexts = new Dictionary<int, Word>();
        }

        public List<string> alternativeTranslate;

        public List<int> GoodContext;

        public int count;

        public int bestContextId;

        public Dictionary<int,Word> contexts;
    }
}