using CgmInfo.Traversal;

namespace CgmInfo.Commands.MetafileDescriptor
{
    public class SegmentPriorityExtent : Command
    {
        public SegmentPriorityExtent(int minPriorityValue, int maxPriorityValue)
            : base(1, 18)
        {
            MinimumPriorityValue = minPriorityValue;
            MaximumPriorityValue = maxPriorityValue;
        }

        public int MinimumPriorityValue { get; private set; }
        public int MaximumPriorityValue { get; private set; }

        public override void Accept<T>(ICommandVisitor<T> visitor, T parameter)
        {
            visitor.AcceptMetafileDescriptorSegmentPriorityExtent(this, parameter);
        }
    }
}
