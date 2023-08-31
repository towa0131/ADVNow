using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ADVNow.ViewModels
{
    interface IAddGameViewModel
    {
        public ICommand SelectGamePathCmd { get; set; }

        public ReactiveProperty<string> Path { get; set; }
    }
}
