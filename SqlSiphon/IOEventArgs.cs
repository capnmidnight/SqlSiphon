﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSiphon
{
    public delegate void IOEventHandler(object sender, IOEventArgs args);
    public class IOEventArgs : EventArgs
    {
        public string Text { get; private set; }
        public IOEventArgs(string text)
        {
            this.Text = text;
        }
    }
}
