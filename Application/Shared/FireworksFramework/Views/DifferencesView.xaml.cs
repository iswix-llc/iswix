///////////////////////////////////////////////
// Copyright (C) 2010-2019 ISWIX, LLC
// Web: http://www.iswix.com
// All Rights Reserved
///////////////////////////////////////////////
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using FireworksFramework.Managers;

namespace FireworksFramework.Views
{
    public partial class Differences : Window
    {
        public Differences()
        {
            InitializeComponent();
        }

        private void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewerToUpdate = sender == this.beforeDiff ? new TextBoxBase[] { this.afterDiff } :
                            sender == this.afterDiff ? new TextBoxBase[] { this.beforeDiff } : new TextBoxBase[0];

            scrollViewerToUpdate.ToList().ForEach(textToSync =>
            {
                textToSync.ScrollToVerticalOffset(e.VerticalOffset);
            });

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            DocumentManager.DocumentManagerInstance.UnSubscribe("differences");
        }
    }
}