﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFTetris.ViewModels;

namespace WPFTetris.ViewModels
{
    public class BoardViewModel : ObservableCollection<BlockViewModel?>
    {
        public BlockViewModel? this[int i, int j] { get => this[(i * 10) + j]; set => this[(i * 10) + j] = value; }

        public BoardViewModel()
        {
            for (int i = 0; i < 200; i++)
                Add(new BlockViewModel(i, y));
        }
    }
}
