using System;
using LiveDescribe.Interfaces;
using LiveDescribe.Model;

namespace LiveDescribe.HistoryItems
{
    public class MoveHistoryItem : IHistoryItem
    {
        private readonly IDescribableInterval _itemInterval;

        private readonly double _originalStartInVideo;
        private readonly double _originalEndInVideo;

        private readonly double _newStartInVideo;
        private readonly double _newEndInVideo;

        public MoveHistoryItem(IDescribableInterval itemInterval, double originalStartInVideo, double originalEndInVideo, 
            double newStartInVideo, double newEndInVideo)
        {
            _itemInterval = itemInterval;

            _originalStartInVideo = originalStartInVideo;
            _originalEndInVideo = originalEndInVideo;

            _newStartInVideo = newStartInVideo;
            _newEndInVideo = newEndInVideo;
        }

        public void Execute()
        {
            _itemInterval.SetStartAndEndInVideo(_newStartInVideo, _newEndInVideo);
        }

        public void UnExecute()
        {
            _itemInterval.SetStartAndEndInVideo(_originalStartInVideo, _originalEndInVideo);
        }
    }
}
