using CgmInfo.Traversal;

namespace CgmInfo.Commands.Attributes
{
    public class MarkerSize : Command
    {
        public MarkerSize(double size)
            : base(5, 7)
        {
            Size = size;
        }

        public double Size { get; private set; }

        public override void Accept<T>(ICommandVisitor<T> visitor, T parameter)
        {
            visitor.AcceptAttributeMarkerSize(this, parameter);
        }
    }
}
