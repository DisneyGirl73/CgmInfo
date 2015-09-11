﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using CgmInfoGui.Visuals;

namespace CgmInfoGui.Controls
{
    public class VisualCanvas : FrameworkElement
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(List<VisualBase>), typeof(VisualCanvas), new PropertyMetadata(OnItemsSourceChanged));
        public static readonly DependencyProperty VdcExtentProperty =
            DependencyProperty.Register("VdcExtent", typeof(Rect), typeof(VisualCanvas), new PropertyMetadata(Rect.Empty, OnVdcExtentChanged));
        public static readonly DependencyProperty GeometryExtentProperty =
            DependencyProperty.Register("GeometryExtent", typeof(Rect), typeof(VisualCanvas), new PropertyMetadata(Rect.Empty, OnGeometryExtentChanged));

        public static readonly DependencyProperty VdcExtentBrushProperty =
            DependencyProperty.Register("VdcExtentBrush", typeof(Brush), typeof(VisualCanvas), new PropertyMetadata(null));
        public static readonly DependencyProperty VdcExtentPenProperty =
            DependencyProperty.Register("VdcExtentPen", typeof(Pen), typeof(VisualCanvas), new PropertyMetadata(new Pen(Brushes.Red, 1).GetCurrentValueAsFrozen()));
        public static readonly DependencyProperty GeometryExtentBrushProperty =
            DependencyProperty.Register("GeometryExtentBrush", typeof(Brush), typeof(VisualCanvas), new PropertyMetadata(null));
        public static readonly DependencyProperty GeometryExtentPenProperty =
            DependencyProperty.Register("GeometryExtentPen", typeof(Pen), typeof(VisualCanvas), new PropertyMetadata(new Pen(Brushes.Green, 1).GetCurrentValueAsFrozen()));

        public Brush VdcExtentBrush
        {
            get { return (Brush)GetValue(VdcExtentBrushProperty); }
            set { SetValue(VdcExtentBrushProperty, value); }
        }
        public Pen VdcExtentPen
        {
            get { return (Pen)GetValue(VdcExtentPenProperty); }
            set { SetValue(VdcExtentPenProperty, value); }
        }

        public Brush GeometryExtentBrush
        {
            get { return (Brush)GetValue(GeometryExtentBrushProperty); }
            set { SetValue(GeometryExtentBrushProperty, value); }
        }
        public Pen GeometryExtentPen
        {
            get { return (Pen)GetValue(GeometryExtentPenProperty); }
            set { SetValue(GeometryExtentPenProperty, value); }
        }

        public List<VisualBase> ItemsSource
        {
            get { return (List<VisualBase>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public Rect VdcExtent
        {
            get { return (Rect)GetValue(VdcExtentProperty); }
            set { SetValue(VdcExtentProperty, value); }
        }

        public Rect GeometryExtent
        {
            get { return (Rect)GetValue(GeometryExtentProperty); }
            set { SetValue(GeometryExtentProperty, value); }
        }

        private readonly VisualCollection _visuals;
        public VisualCanvas()
        {
            _visuals = new VisualCollection(this);
            ResetSize();

            // enable cached composition, since the visuals won't change anyways after they're loaded.
            // scale it up by 5x to make it look crisp when zooming in
            CacheMode = new BitmapCache(5.0) { EnableClearType = true };
        }

        private static void OnItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((VisualCanvas)obj).OnItemsSourceChanged(args);
        }
        protected virtual void OnItemsSourceChanged(DependencyPropertyChangedEventArgs args)
        {
            _visuals.Clear();
            ResetSize();

            var oldList = args.OldValue as List<VisualBase>;
            var newList = args.NewValue as List<VisualBase>;

            if (newList != null)
            {
                CreateVisuals(newList);
            }
        }

        private static void OnVdcExtentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((VisualCanvas)obj).OnVdcExtentChanged(args);
        }
        protected virtual void OnVdcExtentChanged(DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue is Rect)
            {
                var newExtent = (Rect)args.NewValue;
                UpdateSize(newExtent);
            }
        }

        private void ResetSize()
        {
            // avoid having NaN so the re-calculation can do its job
            Height = Width = 1;
        }
        private void UpdateSize(Rect newExtent)
        {
            if (newExtent.Height > Height)
                Height = newExtent.Height;
            if (newExtent.Width > Width)
                Width = newExtent.Width;
        }

        private static void OnGeometryExtentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((VisualCanvas)obj).OnGeometryExtentChanged(args);
        }
        protected virtual void OnGeometryExtentChanged(DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue is Rect)
            {
                var newExtent = (Rect)args.NewValue;
                UpdateSize(newExtent);
            }
        }
        private void CreateVisuals(List<VisualBase> list)
        {
            foreach (var item in list)
            {
                var drawingVisual = new DrawingVisualX(item);
                _visuals.Add(drawingVisual);
            }
        }

        protected override int VisualChildrenCount
        {
            get { return _visuals.Count; }
        }
        protected override Visual GetVisualChild(int index)
        {
            return _visuals[index];
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(VdcExtentBrush, VdcExtentPen, VdcExtent);
            drawingContext.DrawRectangle(GeometryExtentBrush, GeometryExtentPen, GeometryExtent);
            base.OnRender(drawingContext);
        }

        private sealed class DrawingVisualX : DrawingVisual
        {
            public DrawingVisualX(VisualBase visual)
            {
                Visual = visual;

                var context = RenderOpen();
                visual.DrawTo(context);
                context.Close();
            }
            public VisualBase Visual { get; }
        }
    }
}
