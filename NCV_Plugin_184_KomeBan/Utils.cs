using NicoLibrary.NicoLiveData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCV_Plugin_184_KomeBan
{
    class LiveCommentDataEqualityComparer : IEqualityComparer<LiveCommentData>
    {
        public static LiveCommentDataEqualityComparer Instance { get; } = new LiveCommentDataEqualityComparer();

        public bool Equals(LiveCommentData x, LiveCommentData y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;
            return x.UserId == y.UserId;
        }

        public int GetHashCode(LiveCommentData p)
          => p.UserId.GetHashCode();
    }
}
