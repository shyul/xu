﻿/// ***************************************************************************
/// Shared Libraries and Utilities
/// Copyright 2001-2008, 2014-2020 Xu Li - me@xuli.us
/// 
/// Docking Multi-Panel Control
/// 
/// ***************************************************************************

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Xu
{
    [DesignerCategory("Code")]
    public sealed class SideDockPane : DockPane, IResizable
    {
        #region Ctor
        public SideDockPane(DockStyle side) : base()
        {
            // Control Settings
            Dock = side;
            // Visible = false;
            // Component;
            //CreateContainer();
            ResumeLayout(true);
            PerformLayout();
        }
        #endregion

        #region Components

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (Parent != null)
            {
                Console.WriteLine("### SideDockPane Parent is: " + Parent.GetType().ToString());
                //if ((typeof(DockCanvas)).IsAssignableFrom(Parent.GetType()))
                if (Parent is DockCanvas dkc)
                {
                    Console.WriteLine("### ### Again SideDockPane Parent is: " + Parent.GetType().ToString());
                    Dkc = dkc;
                } // (DockCanvas)Parent;
                else
                    throw new Exception("SideDockPane can only be exsiting in Mosaic / Parent: " + Parent.GetType().ToString());
            }
            else Dkc = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="df"></param>
        public override void AddForm(int index, DockTab df)
        {
            if (index >= Count)
            {
                //index = Count;
                CreateContainer().AddForm(DockStyle.Fill, df);
                return;
            }
            else
            {
                if (index < 0) index = 0;
                DockContainers[index].AddForm(DockStyle.Fill, df);
            }
            m_tabOnly = (ShownContainerCount == 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public SideDockContainer CreateContainer()
        {
            return CreateContainer(Count);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public SideDockContainer CreateContainer(int index)
        {
            ObsoletedEvent.Debug("Create Side Container at: " + index.ToString());
            SideDockContainer dc_new = new SideDockContainer();
            SuspendLayout();
            lock (DockContainers)
            {
                if (index > Count) index = Count;
                if (index < 0) index = 0;
                DockContainers.Insert(index, dc_new);
                Controls.Add(dc_new);
                Reorder();
            }
            if (!Visible && Count > 0) Visible = true;
            ResumeLayout(true);
            return dc_new;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dc"></param>
        public override void RemoveContainer(DockContainer dc)
        {
            base.RemoveContainer(dc); // If it is root, we should keep at least one container here.
            CleanUp();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void CleanUp()
        {
            m_tabOnly = (ShownContainerCount == 0);
            if (Count <= 0)
            {
                ObsoletedEvent.Debug("Simple SidePane Clean Up");
                Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Close()
        {
            Visible = false;
        }
        #endregion

        #region Unpinned Container

        [DisplayName("Has Hidden Container")]
        public bool HasHiddenContainer
        {
            get
            {
                if (DockContainers != null)
                {
                    lock (DockContainers)
                        foreach (SideDockContainer dc in DockContainers)
                        {
                            if (!dc.ShowTab)
                                return true;
                        }
                }
                return false;
            }
        }

        [DisplayName("Hidden Container Count")]
        public int HiddenContainerCount
        {
            get
            {
                if (DockContainers != null)
                {
                    int i = 0;
                    lock (DockContainers)
                        foreach (SideDockContainer dc in DockContainers)
                        {
                            if (!dc.ShowTab)
                            {
                                i++;
                            }
                        }
                    return i;
                }
                else
                    return 0;
            }
        }

        [DisplayName("Shown Container Count")]
        public int ShownContainerCount
        {
            get
            {
                if (DockContainers != null)
                {
                    int i = 0;
                    lock (DockContainers)
                        foreach (SideDockContainer dc in DockContainers)
                        {
                            if (dc.ShowTab)
                            {
                                i++;
                            }
                        }
                    return i;
                }
                else
                    return 0;
            }
        }

        public void Hide(SideDockContainer dc)
        {
            SuspendLayout();
            Dkc.SuspendLayout();
            dc.HideContainer();
            switch (Dock)
            {
                case (DockStyle.Top):
                    dc.Anchor = (AnchorStyles)(AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
                    if (ShownContainerCount == 0)
                    {
                        Height = DockCanvas.SideHiddenTabHeight;
                        m_tabOnly = true;
                    }
                    break;
                case (DockStyle.Bottom):
                    dc.Anchor = (AnchorStyles)(AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
                    if (ShownContainerCount == 0)
                    {
                        Height = DockCanvas.SideHiddenTabHeight;
                        m_tabOnly = true;
                    }
                    break;
                case (DockStyle.Left):
                    dc.Anchor = (AnchorStyles)(AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Top);
                    if (ShownContainerCount == 0)
                    {
                        Width = DockCanvas.SideHiddenTabHeight;
                        m_tabOnly = true;
                    }
                    break;
                case (DockStyle.Right):
                    dc.Anchor = (AnchorStyles)(AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top);
                    if (ShownContainerCount == 0)
                    {
                        Width = DockCanvas.SideHiddenTabHeight;
                        m_tabOnly = true;
                    }
                    break;
                default:
                    if (ShownContainerCount == 0)
                    {
                        Height = DockCanvas.SideHiddenTabHeight;
                        Width = DockCanvas.SideHiddenTabHeight;
                        m_tabOnly = true;
                    }
                    break;
            }
            Dkc.Controls.Add(dc);
            Coordinate();
            Dkc.ResumeLayout(true);
            ResumeLayout(true);
            Invalidate(true);
        }

        public void Unhide(SideDockContainer dc)
        {
            SuspendLayout();
            Dkc.SuspendLayout();
            dc.Anchor = AnchorStyles.None;
            dc.UnhideContainer();
            switch (Dock)
            {
                case (DockStyle.Top):
                case (DockStyle.Bottom):
                    if (Height == DockCanvas.SideHiddenTabHeight)
                    {
                        if (!HasHiddenContainer)
                            Height = dc.Height;
                        else if (Height < DockCanvas.SideHiddenTabHeight + dc.Height)
                            Height = DockCanvas.SideHiddenTabHeight + dc.Height;
                    }
                    break;
                case (DockStyle.Left):
                case (DockStyle.Right):
                default:
                    if (Width == DockCanvas.SideHiddenTabHeight)
                    {
                        if (!HasHiddenContainer)
                            Width = dc.Width;
                        else if (Width < DockCanvas.SideHiddenTabHeight + dc.Width)
                            Width = DockCanvas.SideHiddenTabHeight + dc.Width;
                    }
                    break;
            }
            Controls.Add(dc);
            m_tabOnly = (ShownContainerCount == 0);
            Coordinate();
            Dkc.ResumeLayout(true);
            ResumeLayout(true);
            Invalidate(true);
        }

        private bool m_tabOnly = false;

        public SideDockContainer FirstHiddenContainer { get; private set; } = null;

        #endregion

        #region Layout

        /// <summary>
        /// 
        /// </summary>
        private LayoutStatus LayoutState = LayoutStatus.Idle;

        #region Resizing
        public override LayoutType LayoutType
        {
            get
            {
                switch (Dock)
                {
                    case (DockStyle.Left):
                    case (DockStyle.Right):
                        return LayoutType.Horizontal;
                    case (DockStyle.Bottom):
                    case (DockStyle.Top):
                    default:  // Side Pane's Dock is always on the side
                        return LayoutType.Vertical;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool HasSizeGrip { get { return (Unlocked & (!m_tabOnly)); } }

        /// <summary>
        /// 
        /// </summary>
        public Rectangle SizeGrip { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Control Area { get { return this; } }

        /// <summary>
        /// 
        /// </summary>
        public Control Canvas { get { return Dkc; } }

        public void OnGetSize(int size)
        {
            switch (Dock)
            {
                case (DockStyle.Left):
                    Size = new Size(Width + size, Height);
                    break;
                case (DockStyle.Right):
                    Size = new Size(Width - size, Height);
                    break;
                case (DockStyle.Top):
                    Size = new Size(Width, Height + size);
                    break;
                case (DockStyle.Bottom):
                    Size = new Size(Width, Height - size);
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Docking

        /// <summary>
        /// 
        /// </summary>
        public static void FinishDock()
        {
            DockStyle side = DockCanvas.FinishDock();
            ObsoletedEvent.Debug("Finish SidePane Docking, side: " + side);
            if (DockCanvas.NextContainer != null)
            {
                DockCanvas.NextContainer.AddForm(side, DockCanvas.ActiveDockForm);
                DockCanvas.NextContainer = null;
            }
        }

        #endregion

        #region TabDrag

        /// <summary>
        /// 
        /// </summary>
        private int ActiveFormNewOrder = 0;

        #endregion

        #endregion

        #region Coordinate

        /// <summary>
        /// 
        /// </summary>
        [NonSerialized]
        private const int c_hiddenContainerTabMinimum = 50;

        /// <summary>
        /// 
        /// </summary>
        [NonSerialized]
        private const int c_hiddenContainerTabGroupGap = 7;

        /// <summary>
        /// 
        /// </summary>
        private Rectangle TabBound = Rectangle.Empty;

        /// <summary>
        /// 
        /// </summary>
        public override void Coordinate()
        {
            //int shownPaneNum = ShownContainerCount;
            int splitMargin = MosaicForm.PaneGripMargin;

            if (HasHiddenContainer)
            {
                int hiddenTabBase = 0;
                int hiddenTabBoundBase = hiddenTabBase;
                int hiddenTabFixed = 0;
                int hiddenTabTextSize = DockCanvas.SideCaptionHeight;
                int hiddenTabTextFixed;
                int hiddenTabTextMargin = 8;

                switch (Dock)
                {
                    case (DockStyle.Top):
                        hiddenTabTextFixed = DockCanvas.SideHiddenTabHeight - hiddenTabTextSize;
                        lock (DockContainers)
                            for (int i = 0; i < Count; i++)
                            {
                                SideDockContainer dc = (SideDockContainer)DockContainers[i];
                                if (!dc.ShowTab)
                                {
                                    lock (dc.Tabs)
                                        for (int j = 0; j < dc.Count; j++)
                                        {
                                            DockTab df = (DockTab)dc.Tabs[j];
                                            int textWidth = df.TabNameWidth + hiddenTabTextMargin;
                                            if (textWidth < c_hiddenContainerTabMinimum) textWidth = c_hiddenContainerTabMinimum;
                                            df.TabNameRect = new Rectangle(hiddenTabBase, hiddenTabTextFixed, textWidth, hiddenTabTextSize);
                                            df.TabRect = new Rectangle(hiddenTabBase, hiddenTabFixed, textWidth, DockCanvas.SideHiddenTabHeight - 1);
                                            hiddenTabBase += textWidth;
                                        }

                                    hiddenTabBase += c_hiddenContainerTabGroupGap;
                                    if (dc.Visible) CoordinateHidenContainer(dc);
                                }
                            }
                        TabBound = new Rectangle(hiddenTabBoundBase, hiddenTabFixed, hiddenTabBase - hiddenTabBoundBase, DockCanvas.SideHiddenTabHeight - 1);
                        break;
                    case (DockStyle.Bottom):
                        hiddenTabTextFixed = Height - DockCanvas.SideHiddenTabHeight;
                        hiddenTabFixed = hiddenTabTextFixed;
                        lock (DockContainers)
                            for (int i = 0; i < Count; i++)
                            {
                                SideDockContainer dc = (SideDockContainer)DockContainers[i];
                                if (!dc.ShowTab)
                                {
                                    lock (dc.Tabs)
                                        for (int j = 0; j < dc.Count; j++)
                                        {
                                            DockTab df = (DockTab)dc.Tabs[j];
                                            int textWidth = df.TabNameWidth + hiddenTabTextMargin;
                                            if (textWidth < c_hiddenContainerTabMinimum) textWidth = c_hiddenContainerTabMinimum;
                                            df.TabNameRect = new Rectangle(hiddenTabBase, hiddenTabTextFixed, textWidth, hiddenTabTextSize);
                                            df.TabRect = new Rectangle(hiddenTabBase, hiddenTabFixed, textWidth, DockCanvas.SideHiddenTabHeight - 1);
                                            hiddenTabBase += textWidth;
                                        }
                                    hiddenTabBase += c_hiddenContainerTabGroupGap;
                                    if (dc.Visible) CoordinateHidenContainer(dc);
                                }
                            }
                        TabBound = new Rectangle(hiddenTabBoundBase, hiddenTabFixed, hiddenTabBase - hiddenTabBoundBase, DockCanvas.SideHiddenTabHeight - 1);
                        break;
                    case (DockStyle.Left):
                        hiddenTabTextFixed = DockCanvas.SideHiddenTabHeight - hiddenTabTextSize;
                        lock (DockContainers)
                            for (int i = 0; i < Count; i++)
                            {
                                SideDockContainer dc = (SideDockContainer)DockContainers[i];
                                if (!dc.ShowTab)
                                {
                                    lock (dc.Tabs)
                                        for (int j = 0; j < dc.Count; j++)
                                        {
                                            DockTab df = (DockTab)dc.Tabs[j];
                                            int textWidth = df.TabNameWidth + hiddenTabTextMargin;
                                            if (textWidth < c_hiddenContainerTabMinimum) textWidth = c_hiddenContainerTabMinimum;
                                            df.TabNameRect = new Rectangle(hiddenTabTextFixed, hiddenTabBase, hiddenTabTextSize, textWidth);
                                            df.TabRect = new Rectangle(hiddenTabFixed, hiddenTabBase, DockCanvas.SideHiddenTabHeight - 1, textWidth);
                                            hiddenTabBase += textWidth;
                                        }
                                    hiddenTabBase += c_hiddenContainerTabGroupGap;
                                    if (dc.Visible) CoordinateHidenContainer(dc);
                                }
                            }
                        TabBound = new Rectangle(hiddenTabFixed, hiddenTabBoundBase, DockCanvas.SideHiddenTabHeight - 1, hiddenTabBase - hiddenTabBoundBase);
                        break;
                    case (DockStyle.Right):
                    default:
                        hiddenTabTextFixed = Width - DockCanvas.SideHiddenTabHeight;
                        hiddenTabFixed = hiddenTabTextFixed;
                        lock (DockContainers)
                            for (int i = 0; i < Count; i++)
                            {
                                SideDockContainer dc = (SideDockContainer)DockContainers[i];
                                if (!dc.ShowTab)
                                {
                                    lock (dc.Tabs)
                                        for (int j = 0; j < dc.Count; j++)
                                        {
                                            DockTab df = (DockTab)dc.Tabs[j];
                                            int textWidth = df.TabNameWidth + hiddenTabTextMargin;
                                            if (textWidth < c_hiddenContainerTabMinimum) textWidth = c_hiddenContainerTabMinimum;
                                            df.TabNameRect = new Rectangle(hiddenTabTextFixed, hiddenTabBase, hiddenTabTextSize, textWidth);
                                            df.TabRect = new Rectangle(hiddenTabFixed, hiddenTabBase, DockCanvas.SideHiddenTabHeight - 1, textWidth);
                                            hiddenTabBase += textWidth;
                                        }
                                    hiddenTabBase += c_hiddenContainerTabGroupGap;
                                    if (dc.Visible) CoordinateHidenContainer(dc);
                                }
                            }
                        TabBound = new Rectangle(hiddenTabFixed, hiddenTabBoundBase, DockCanvas.SideHiddenTabHeight - 1, hiddenTabBase - hiddenTabBoundBase);
                        break;
                }
            }
            if (ShownContainerCount > 0)
            {
                double TotalRatio = 0; // Must be caclulated and total of the count of containers
                lock (DockContainers)
                    foreach (SideDockContainer dc in DockContainers)
                    {
                        if (dc.ShowTab)
                        {
                            TotalRatio += dc.Ratio;
                        }
                    }

                int containerSize;
                int containerBase = 0;
                int containerBaseFixed = 0;
                int shownContainerIndex = 0;
                double factor;

                switch (Dock)
                {
                    case (DockStyle.Top):
                        containerSize = Height;

                        if (HasSizeGrip)
                        {
                            containerSize -= splitMargin;
                            SizeGrip = new Rectangle(0, Height - splitMargin, Width, splitMargin);
                            factor = (Width - ((ShownContainerCount - 1) * splitMargin)) / TotalRatio;
                        }
                        else
                        {
                            factor = Width / TotalRatio;
                        }

                        if (HasHiddenContainer)
                        {
                            containerSize -= DockCanvas.SideHiddenTabHeight;
                            containerBaseFixed = DockCanvas.SideHiddenTabHeight;
                        }

                        lock (DockContainers)
                            for (int i = 0; i < DockContainers.Count; i++)
                            {
                                SideDockContainer dc = (SideDockContainer)DockContainers[i];
                                if (dc.ShowTab)
                                {
                                    if (shownContainerIndex == 0) FirstHiddenContainer = dc;

                                    int w = Convert.ToInt32((factor * dc.Ratio).ToInt64()); //Convert.ToInt32(Math.Round(factor * dc.Ratio));

                                    if (shownContainerIndex > 0 && HasSizeGrip)
                                    {
                                        w += splitMargin;
                                    }
                                    if (shownContainerIndex == ShownContainerCount - 1)
                                    {
                                        w = Width - containerBase;
                                    }
                                    dc.Location = new Point(containerBase, containerBaseFixed);
                                    dc.Width = w;
                                    dc.Height = containerSize;
                                    containerBase = dc.Right;
                                    shownContainerIndex++;
                                }
                            }
                        break;
                    case (DockStyle.Bottom):
                        containerSize = Height;

                        if (HasSizeGrip)
                        {
                            containerSize -= splitMargin;
                            SizeGrip = new Rectangle(0, 0, Width, splitMargin);
                            containerBaseFixed = splitMargin;
                            factor = (Width - ((ShownContainerCount - 1) * splitMargin)) / TotalRatio;
                        }
                        else
                        {
                            factor = Width / TotalRatio;
                        }

                        if (HasHiddenContainer)
                        {
                            containerSize -= DockCanvas.SideHiddenTabHeight;
                        }

                        lock (DockContainers)
                            for (int i = 0; i < DockContainers.Count; i++)
                            {
                                SideDockContainer dc = (SideDockContainer)DockContainers[i];
                                if (dc.ShowTab)
                                {
                                    if (shownContainerIndex == 0) FirstHiddenContainer = dc;

                                    int w = Convert.ToInt32((factor * dc.Ratio).ToInt64());

                                    if (shownContainerIndex > 0 && HasSizeGrip)
                                    {
                                        w += splitMargin;
                                    }
                                    if (shownContainerIndex == ShownContainerCount - 1)
                                    {
                                        w = Width - containerBase;
                                    }
                                    dc.Location = new Point(containerBase, containerBaseFixed);
                                    dc.Width = w;
                                    dc.Height = containerSize;
                                    containerBase = dc.Right;
                                    shownContainerIndex++;
                                }
                            }
                        break;
                    case (DockStyle.Left):
                        containerSize = Width;

                        if (HasSizeGrip)
                        {
                            containerSize -= splitMargin;
                            SizeGrip = new Rectangle(Width - splitMargin, 0, splitMargin, Height);
                            factor = (Height - ((ShownContainerCount - 1) * splitMargin)) / TotalRatio;
                        }
                        else
                        {
                            factor = Height / TotalRatio;
                        }

                        if (HasHiddenContainer)
                        {
                            containerSize -= DockCanvas.SideHiddenTabHeight;
                            containerBaseFixed = DockCanvas.SideHiddenTabHeight;
                        }

                        lock (DockContainers)
                            for (int i = 0; i < DockContainers.Count; i++)
                            {
                                SideDockContainer dc = (SideDockContainer)DockContainers[i];
                                if (dc.ShowTab)
                                {
                                    if (shownContainerIndex == 0) FirstHiddenContainer = dc;

                                    int h = Convert.ToInt32((factor * dc.Ratio).ToInt64());

                                    if (shownContainerIndex > 0 && HasSizeGrip)
                                    {
                                        h += splitMargin;
                                    }
                                    if (shownContainerIndex == ShownContainerCount - 1)
                                    {
                                        h = Height - containerBase;
                                    }
                                    dc.Location = new Point(containerBaseFixed, containerBase);
                                    dc.Width = containerSize;
                                    dc.Height = h;
                                    containerBase = dc.Bottom;
                                    shownContainerIndex++;
                                }
                            }
                        break;
                    case (DockStyle.Right):
                    default:
                        containerSize = Width;

                        if (HasSizeGrip)
                        {
                            containerSize -= splitMargin;
                            SizeGrip = new Rectangle(0, 0, splitMargin, Height);
                            containerBaseFixed = splitMargin;
                            factor = (Height - ((ShownContainerCount - 1) * splitMargin)) / TotalRatio;
                        }
                        else
                        {
                            factor = Height / TotalRatio;
                        }

                        if (HasHiddenContainer)
                        {
                            containerSize -= DockCanvas.SideHiddenTabHeight;
                        }

                        lock (DockContainers)
                            for (int i = 0; i < DockContainers.Count; i++)
                            {
                                SideDockContainer dc = (SideDockContainer)DockContainers[i];
                                if (dc.ShowTab)
                                {
                                    if (shownContainerIndex == 0) FirstHiddenContainer = dc;

                                    int h = Convert.ToInt32((factor * dc.Ratio).ToInt64());

                                    if (shownContainerIndex > 0 && HasSizeGrip)
                                    {
                                        h += splitMargin;
                                    }
                                    if (shownContainerIndex == ShownContainerCount - 1)
                                    {
                                        h = Height - containerBase;
                                    }
                                    dc.Location = new Point(containerBaseFixed, containerBase);
                                    dc.Width = containerSize;
                                    dc.Height = h;
                                    containerBase = dc.Bottom;
                                    shownContainerIndex++;
                                }
                            }
                        break;
                }
            }
            else
            {
                SizeGrip = Rectangle.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dc"></param>
        public void CoordinateHidenContainer(SideDockContainer dc)
        {
            if (!dc.ShowTab)
                switch (Dock)
                {
                    case (DockStyle.Top):
                        dc.Location = new Point(Left, Top + DockCanvas.SideHiddenTabHeight);
                        dc.Size = new Size(ClientRectangle.Width, dc.Height);
                        break;
                    case (DockStyle.Bottom):
                        dc.Location = new Point(Left, Bottom - dc.Height - DockCanvas.SideHiddenTabHeight);
                        dc.Size = new Size(ClientRectangle.Width, dc.Height);
                        break;
                    case (DockStyle.Left):
                        dc.Location = new Point(DockCanvas.SideHiddenTabHeight, Top);
                        dc.Size = new Size(dc.Width, ClientRectangle.Height);
                        break;
                    default:
                        dc.Location = new Point(Right - dc.Width - DockCanvas.SideHiddenTabHeight, Top);
                        dc.Size = new Size(dc.Width, ClientRectangle.Height);
                        break;
                }

        }
        #endregion

        #region Paint

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pe"></param>
        protected override void OnPaint(PaintEventArgs pe)
        {
            if (DockContainers != null)
            {
                if (HasHiddenContainer) // Render the side tab for hidden units
                {
                    Graphics g = pe.Graphics;
                    //g.SmoothingMode = SmoothingMode.HighQuality;
                    //g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    bool IsFirst = true;
                    int MarkerRectSize = DockCanvas.SideHiddenTabHeight - DockCanvas.SideCaptionHeight + 1;
                    switch (Dock)
                    {
                        case (DockStyle.Top):
                            g.DrawLine(Main.Theme.Panel.EdgePen, new Point(0, 0), new Point(Width, 0));
                            lock (DockContainers)
                                for (int i = 0; i < Count; i++)
                                {
                                    SideDockContainer dc = (SideDockContainer)DockContainers[i];
                                    if (!dc.ShowTab)
                                    {
                                        if (!IsFirst)
                                        {
                                            Rectangle FirstTab = dc.Tabs[0].TabRect;
                                            g.DrawLine(Main.Theme.Panel.EdgePen, new Point(FirstTab.Left - c_hiddenContainerTabGroupGap / 2 - 1, FirstTab.Top + 1), new Point(FirstTab.Left - c_hiddenContainerTabGroupGap / 2 - 1, FirstTab.Bottom - 2));
                                        }
                                        IsFirst = false;
                                        lock (dc.Tabs)
                                            for (int j = 0; j < dc.Count; j++)
                                            {
                                                DockTab df = (DockTab)dc.Tabs[j];
                                                Rectangle tempR = df.TabRect;
                                                tempR = new Rectangle(tempR.X + 4, tempR.Y, tempR.Width - 8, MarkerRectSize);

                                                if ((df.MouseState == MouseState.Down || df.MouseState == MouseState.Drag) && MouseState != MouseState.Out)
                                                {
                                                    g.FillRectangle(Main.Theme.Click.FillBrush, df.TabRect);
                                                    g.FillRectangle(Main.Theme.Click.EdgeBrush, tempR);
                                                    g.DrawString(df.TabName, Font, Main.Theme.DarkTextBrush, df.TabNameRect, AppTheme.TextAlignCenter);
                                                    g.DrawRectangle(Main.Theme.Click.EdgePen, df.TabRect);
                                                }
                                                else if ((dc.Visible && df.Visible) || (df.MouseState == MouseState.Hover && MouseState != MouseState.Out))
                                                {
                                                    g.FillRectangle(Main.Theme.Hover.FillBrush, df.TabRect);
                                                    g.FillRectangle(Main.Theme.Hover.EdgeBrush, tempR);
                                                    g.DrawString(df.TabName, Font, Main.Theme.DarkTextBrush, df.TabNameRect, AppTheme.TextAlignCenter);
                                                    g.DrawRectangle(Main.Theme.Hover.EdgePen, df.TabRect);
                                                }
                                                else
                                                {
                                                    g.FillRectangle(Main.Theme.InactiveCursor.EdgeBrush, tempR);
                                                    g.DrawString(df.TabName, Font, Main.Theme.InactiveCursor.EdgeBrush, df.TabNameRect, AppTheme.TextAlignCenter);
                                                }
                                            }
                                    }
                                }
                            break;
                        case (DockStyle.Bottom):
                            g.DrawLine(Main.Theme.Panel.EdgePen, new Point(0, Height - 1), new Point(Width, Height - 1));
                            lock (DockContainers)
                                for (int i = 0; i < Count; i++)
                                {
                                    SideDockContainer dc = (SideDockContainer)DockContainers[i];
                                    if (!dc.ShowTab)
                                    {
                                        if (!IsFirst)
                                        {
                                            Rectangle FirstTab = dc.Tabs[0].TabRect;
                                            g.DrawLine(Main.Theme.Panel.EdgePen, new Point(FirstTab.Left - c_hiddenContainerTabGroupGap / 2 - 1, FirstTab.Top + 3), new Point(FirstTab.Left - c_hiddenContainerTabGroupGap / 2 - 1, FirstTab.Bottom - 1));
                                        }
                                        IsFirst = false;
                                        lock (dc.Tabs)
                                            for (int j = 0; j < dc.Count; j++)
                                            {
                                                DockTab df = (DockTab)dc.Tabs[j];
                                                Rectangle tempR = df.TabRect;
                                                tempR = new Rectangle(tempR.X + 4, tempR.Bottom - MarkerRectSize + 1, tempR.Width - 8, MarkerRectSize);

                                                if ((df.MouseState == MouseState.Down || df.MouseState == MouseState.Drag) && MouseState != MouseState.Out)
                                                {
                                                    g.FillRectangle(Main.Theme.Click.FillBrush, df.TabRect);
                                                    g.FillRectangle(Main.Theme.Click.EdgeBrush, tempR);
                                                    g.DrawString(df.TabName, Font, Main.Theme.DarkTextBrush, df.TabNameRect, AppTheme.TextAlignCenter);
                                                    g.DrawRectangle(Main.Theme.Click.EdgePen, df.TabRect);
                                                }
                                                else if ((dc.Visible && df.Visible) || (df.MouseState == MouseState.Hover && MouseState != MouseState.Out))
                                                {
                                                    g.FillRectangle(Main.Theme.Hover.FillBrush, df.TabRect);
                                                    g.FillRectangle(Main.Theme.Hover.EdgeBrush, tempR);
                                                    g.DrawString(df.TabName, Font, Main.Theme.DarkTextBrush, df.TabNameRect, AppTheme.TextAlignCenter);
                                                    g.DrawRectangle(Main.Theme.Hover.EdgePen, df.TabRect);
                                                }
                                                else
                                                {
                                                    g.FillRectangle(Main.Theme.InactiveCursor.EdgeBrush, tempR);
                                                    g.DrawString(df.TabName, Font, Main.Theme.InactiveCursor.EdgeBrush, df.TabNameRect, AppTheme.TextAlignCenter);
                                                }
                                            }
                                    }
                                }
                            break;
                        case (DockStyle.Left):
                            lock (DockContainers)
                                for (int i = 0; i < Count; i++)
                                {
                                    SideDockContainer dc = (SideDockContainer)DockContainers[i];
                                    if (!dc.ShowTab)
                                    {
                                        if (!IsFirst)
                                        {
                                            Rectangle FirstTab = dc.Tabs[0].TabRect;
                                            g.DrawLine(Main.Theme.Panel.EdgePen, new Point(FirstTab.Left + 2, FirstTab.Top - c_hiddenContainerTabGroupGap / 2 - 1), new Point(FirstTab.Right - 3, FirstTab.Top - c_hiddenContainerTabGroupGap / 2 - 1));
                                        }
                                        IsFirst = false;
                                        lock (dc.Tabs)
                                            for (int j = 0; j < dc.Count; j++)
                                            {
                                                DockTab df = (DockTab)dc.Tabs[j];
                                                Rectangle tempR = df.TabRect;
                                                tempR = new Rectangle(0, tempR.Y + 4, MarkerRectSize, tempR.Height - 8);

                                                if ((df.MouseState == MouseState.Down || df.MouseState == MouseState.Drag) && MouseState != MouseState.Out)
                                                {
                                                    g.FillRectangle(Main.Theme.Click.FillBrush, df.TabRect);
                                                    g.FillRectangle(Main.Theme.Click.EdgeBrush, tempR);

                                                    tempR = df.TabNameRect;
                                                    g.TranslateTransform(tempR.X, tempR.Y);
                                                    g.RotateTransform(-90);
                                                    g.DrawString(df.TabName, Font, Main.Theme.DarkTextBrush, new Rectangle(-tempR.Height, 0, tempR.Height, tempR.Width), AppTheme.TextAlignCenter);
                                                    g.RotateTransform(90);
                                                    g.TranslateTransform(-tempR.X, -tempR.Y);

                                                    g.DrawRectangle(Main.Theme.Click.EdgePen, df.TabRect);
                                                }
                                                else if ((dc.Visible && df.Visible) || (df.MouseState == MouseState.Hover && MouseState != MouseState.Out))
                                                {
                                                    g.FillRectangle(Main.Theme.Hover.FillBrush, df.TabRect);
                                                    g.FillRectangle(Main.Theme.Hover.EdgeBrush, tempR);

                                                    tempR = df.TabNameRect;
                                                    g.TranslateTransform(tempR.X, tempR.Y);
                                                    g.RotateTransform(-90);
                                                    g.DrawString(df.TabName, Font, Main.Theme.DarkTextBrush, new Rectangle(-tempR.Height, 0, tempR.Height, tempR.Width), AppTheme.TextAlignCenter);
                                                    g.RotateTransform(90);
                                                    g.TranslateTransform(-tempR.X, -tempR.Y);

                                                    g.DrawRectangle(Main.Theme.Hover.EdgePen, df.TabRect);
                                                }
                                                else
                                                {
                                                    g.FillRectangle(Main.Theme.InactiveCursor.EdgeBrush, tempR);
                                                    tempR = df.TabNameRect;
                                                    g.TranslateTransform(tempR.X, tempR.Y);
                                                    g.RotateTransform(-90);
                                                    g.DrawString(df.TabName, Font, Main.Theme.InactiveCursor.EdgeBrush, new Rectangle(-tempR.Height, 0, tempR.Height, tempR.Width), AppTheme.TextAlignCenter);
                                                    g.RotateTransform(90);
                                                    g.TranslateTransform(-tempR.X, -tempR.Y);
                                                }
                                            }
                                    }
                                }
                            break;
                        case (DockStyle.Right):
                        default:
                            lock (DockContainers)
                                for (int i = 0; i < Count; i++)
                                {
                                    SideDockContainer dc = (SideDockContainer)DockContainers[i];
                                    if (!dc.ShowTab)
                                    {
                                        if (!IsFirst)
                                        {
                                            Rectangle FirstTab = dc.Tabs[0].TabRect;
                                            g.DrawLine(Main.Theme.Panel.EdgePen, new Point(FirstTab.Left + 2, FirstTab.Top - c_hiddenContainerTabGroupGap / 2 - 1), new Point(FirstTab.Right - 3, FirstTab.Top - c_hiddenContainerTabGroupGap / 2 - 1));
                                        }
                                        IsFirst = false;
                                        lock (dc.Tabs)
                                            for (int j = 0; j < dc.Count; j++)
                                            {
                                                DockTab df = (DockTab)dc.Tabs[j];
                                                Rectangle tempR = df.TabRect;
                                                tempR = new Rectangle(tempR.Right - MarkerRectSize + 1, tempR.Y + 4, MarkerRectSize, tempR.Height - 8);

                                                if ((df.MouseState == MouseState.Down || df.MouseState == MouseState.Drag) && MouseState != MouseState.Out)
                                                {
                                                    g.FillRectangle(Main.Theme.Click.FillBrush, df.TabRect);
                                                    g.FillRectangle(Main.Theme.Click.EdgeBrush, tempR);

                                                    tempR = df.TabNameRect;
                                                    g.TranslateTransform(tempR.X, tempR.Y);
                                                    g.RotateTransform(90);
                                                    g.DrawString(df.TabName, Font, Main.Theme.DarkTextBrush, new Rectangle(0, -tempR.Width, tempR.Height, tempR.Width), AppTheme.TextAlignCenter);
                                                    g.RotateTransform(-90);
                                                    g.TranslateTransform(-tempR.X, -tempR.Y);

                                                    g.DrawRectangle(Main.Theme.Click.EdgePen, df.TabRect);
                                                }
                                                else if ((dc.Visible && df.Visible) || (df.MouseState == MouseState.Hover && MouseState != MouseState.Out))
                                                {
                                                    g.FillRectangle(Main.Theme.Hover.FillBrush, df.TabRect);
                                                    g.FillRectangle(Main.Theme.Hover.EdgeBrush, tempR);

                                                    tempR = df.TabNameRect;
                                                    g.TranslateTransform(tempR.X, tempR.Y);
                                                    g.RotateTransform(90);
                                                    g.DrawString(df.TabName, Font, Main.Theme.DarkTextBrush, new Rectangle(0, -tempR.Width, tempR.Height, tempR.Width), AppTheme.TextAlignCenter);
                                                    g.RotateTransform(-90);
                                                    g.TranslateTransform(-tempR.X, -tempR.Y);

                                                    g.DrawRectangle(Main.Theme.Hover.EdgePen, df.TabRect);
                                                }
                                                else
                                                {
                                                    g.FillRectangle(Main.Theme.InactiveCursor.EdgeBrush, tempR);
                                                    tempR = df.TabNameRect;
                                                    g.TranslateTransform(tempR.X, tempR.Y);
                                                    g.RotateTransform(90);
                                                    g.DrawString(df.TabName, Font, Main.Theme.InactiveCursor.EdgeBrush, new Rectangle(0, -tempR.Width, tempR.Height, tempR.Width), AppTheme.TextAlignCenter);
                                                    g.RotateTransform(-90);
                                                    g.TranslateTransform(-tempR.X, -tempR.Y);
                                                }
                                            }
                                    }
                                }
                            break;
                    }
                }
            }
        }

        #endregion

        #region Mouse

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            Point pt = new Point(e.X, e.Y);
            if (Unlocked)
            {
                switch (LayoutState)
                {
                    case (LayoutStatus.Drag):
                        if (TabBound.Contains(pt))
                        {
                            lock (DockContainers)
                                foreach (SideDockContainer dc in DockContainers)
                                {
                                    if (!dc.ShowTab)
                                        lock (dc.Tabs)
                                            foreach (DockTab df in dc.Tabs)
                                            {
                                                Rectangle tabRect = df.TabRect;
                                                if (tabRect.Contains(pt))
                                                {
                                                    ActiveFormNewOrder = df.Order;
                                                    DockCanvas.NextContainer = dc;
                                                    if (df != DockCanvas.ActiveDockForm) df.MouseState = MouseState.Hover;
                                                }
                                                else
                                                {
                                                    if (df != DockCanvas.ActiveDockForm) df.MouseState = MouseState.Out;
                                                }
                                            }
                                    Invalidate();
                                }
                        }
                        else
                        {
                            ActiveFormNewOrder = 0;
                            LayoutState = LayoutStatus.Docking;
                            ObsoletedEvent.Debug("Start Docking");
                            // Start Dock here;
                            DockCanvas.StartDock();
                        }
                        return;
                    case (LayoutStatus.Docking):
                        if (TabBound.Contains(pt))
                        {
                            LayoutState = LayoutStatus.Drag;
                            ObsoletedEvent.Debug("Start Tab Drag");
                            DockCanvas.FinishDock();
                            DockCanvas.NextContainer = null;
                        }
                        else
                        {
                            if (e.Button == MouseButtons.Left)
                                // Work on Dock here;
                                DockCanvas.RefreshDock(Dkc); // pt, 
                            else
                            {
                                DockCanvas.FinishDock();
                                LayoutState = LayoutStatus.Idle;
                            }
                        }
                        Invalidate();
                        return;
                    case (LayoutStatus.Resizing):
                        if (MouseState == MouseState.Drag)
                        {
                            DockCanvas.RefreshResize();
                        }
                        else if (!SizeGrip.Contains(pt))
                        {
                            Cursor.Current = Cursors.Default;
                            LayoutState = LayoutStatus.Idle;
                            ObsoletedEvent.Debug("Stop Resizing");
                        }
                        return;
                    default:
                        if (SizeGrip.Contains(pt))
                        {
                            switch (LayoutType)
                            {
                                case (LayoutType.Horizontal):
                                    Cursor.Current = Cursors.SizeWE;
                                    break;
                                case (LayoutType.Vertical):
                                    Cursor.Current = Cursors.SizeNS;
                                    break;
                                default:
                                    break;
                            }
                        }
                        else if (HasHiddenContainer)
                        {
                            Cursor.Current = Cursors.Default;
                            MouseState = MouseState.Hover;
                            lock (DockContainers)
                                foreach (SideDockContainer dc in DockContainers)
                                {
                                    if (!dc.ShowTab)
                                        lock (dc.Tabs)
                                            foreach (DockTab df in dc.Tabs)
                                            {
                                                Rectangle tabRect = df.TabRect;
                                                if (df.MouseState != MouseState.Drag && df.MouseState != MouseState.Down)
                                                {
                                                    df.MouseState = (tabRect.Contains(pt)) ? MouseState.Hover : MouseState.Out;
                                                }
                                                else if (df.MouseState == MouseState.Down && e.Button == MouseButtons.Left)
                                                {
                                                    if (!TabBound.Contains(pt))
                                                    {
                                                        df.MouseState = MouseState.Drag;
                                                        LayoutState = LayoutStatus.Docking;
                                                        ObsoletedEvent.Debug("Start SidePane Tab Docking");
                                                        // Start Dock here;
                                                        DockCanvas.StartDock();
                                                    }
                                                    else if (!tabRect.Contains(pt))
                                                    {
                                                        df.MouseState = MouseState.Drag;
                                                        LayoutState = LayoutStatus.Drag;
                                                        DockCanvas.NextContainer = null;
                                                        ObsoletedEvent.Debug("Start SidePane Tab Drag");
                                                    }
                                                }
                                            }
                                }
                            Invalidate();
                        }
                        return;
                }
            }
            else
            {
                lock (DockContainers)
                    foreach (SideDockContainer dc in DockContainers)
                    {
                        if (!dc.ShowTab)
                            lock (dc.Tabs)
                                foreach (DockTab df in dc.Tabs)
                                {
                                    Rectangle tabRect = df.TabRect;
                                    if (df.MouseState != MouseState.Drag && df.MouseState != MouseState.Down)
                                    {
                                        df.MouseState = (tabRect.Contains(pt)) ? MouseState.Hover : MouseState.Out;
                                    }
                                }
                    }
                Invalidate();
                MouseState = MouseState.Hover;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            Point pt = new Point(e.X, e.Y);
            if (Unlocked && SizeGrip.Contains(pt))
            {
                MouseState = MouseState.Drag;
                LayoutState = LayoutStatus.Resizing;
                ObsoletedEvent.Debug("Start Resizing");
                DockCanvas.StartResize(this);
            }
            else if (LayoutState == LayoutStatus.Idle && HasHiddenContainer)
            {
                lock (DockContainers)
                    foreach (SideDockContainer dc in DockContainers)
                    {
                        if (!dc.ShowTab)
                        {
                            lock (dc.Tabs)
                                for (int i = 0; i < dc.Count; i++)
                                {
                                    DockTab df = (DockTab)dc.Tabs[i];
                                    if (df.MouseState == MouseState.Hover)
                                    {
                                        df.MouseState = MouseState.Down;
                                        if (DockCanvas.ActiveContainer != dc || DockCanvas.ActiveDockForm != df)
                                        {
                                            DockCanvas.ActiveContainer = dc;
                                            DockCanvas.ActiveDockForm = df;
                                            CoordinateHidenContainer(dc);
                                            dc.Visible = true;
                                            dc.BringToFront();
                                            dc.ActivateTab(i);
                                        }
                                        else // Click again to close any shown container
                                        {
                                            if (DockCanvas.ActiveContainer == dc) DockCanvas.ActiveContainer.Visible = false;
                                            DockCanvas.ActiveContainer = null;
                                        }
                                    }
                                }
                        }
                    }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            switch (LayoutState)
            {
                case (LayoutStatus.Drag):
                    if (DockCanvas.ActiveDockForm != null && DockCanvas.NextContainer != null && HasHiddenContainer && !DockCanvas.NextContainer.ShowTab)
                    {
                        if (DockCanvas.ActiveContainer == DockCanvas.NextContainer)
                        {
                            lock (DockCanvas.ActiveContainer.Tabs)
                            {
                                if (DockCanvas.ActiveDockForm.Order > ActiveFormNewOrder) ActiveFormNewOrder--;
                                else if (DockCanvas.ActiveDockForm.Order < ActiveFormNewOrder) ActiveFormNewOrder++;
                                ObsoletedEvent.Debug("Old: " + DockCanvas.ActiveDockForm.Order + " New: " + ActiveFormNewOrder);
                            }
                        }
                        else
                        {
                            DockCanvas.NextContainer.AddForm(DockStyle.Fill, DockCanvas.ActiveDockForm);
                        }
                        DockCanvas.ActiveDockForm.Order = ActiveFormNewOrder;
                        DockCanvas.NextContainer.Sort();
                        Coordinate();
                    }
                    DockCanvas.NextContainer = null;
                    break;
                case (LayoutStatus.Docking):
                    FinishDock();
                    break;
                case (LayoutStatus.Resizing):
                    DockCanvas.FinishResize();
                    break;
            }
            LayoutState = LayoutStatus.Idle;
            if (HasHiddenContainer)
            {
                Point pt = new Point(e.X, e.Y);
                lock (DockContainers)
                    foreach (SideDockContainer dc in DockContainers)
                    {
                        if (!dc.ShowTab)
                            lock (dc.Tabs)
                                for (int i = 0; i < dc.Count; i++)
                                {
                                    DockTab df = (DockTab)dc.Tabs[i];
                                    Rectangle tabRect = df.TabRect;
                                    df.MouseState = (tabRect.Contains(pt)) ? MouseState.Hover : MouseState.Out;
                                }
                    }
            }
            Invalidate();
        }
        #endregion
    }
}
