using System.Drawing;

namespace CgmInfo.Utilities
{
    public class MetafileColorIndexed : MetafileColor
    {
        private readonly MetafileColor _actualColor;

        public int Index { get; private set; }

        public MetafileColorIndexed(int colorIndex, MetafileColor actualColor)
        {
            _actualColor = actualColor;
            Index = colorIndex;
        }
        public override Color GetColor()
        {
            if (_actualColor == null)
                return default(Color);
            return _actualColor.GetColor();
        }
        protected override string GetStringValue()
        {
            if (_actualColor != null)
                return string.Format("Color Index {0} ({1})", Index, _actualColor);
            else
                return string.Format("Color Index {0} (not known by COLOUR TABLE)", Index);
        }
    }
}
