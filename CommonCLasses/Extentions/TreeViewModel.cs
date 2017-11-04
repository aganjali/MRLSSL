using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace MRL.SSL.CommonClasses
{
    public class TreeViewModel : INotifyPropertyChanged
    {
        #region Data

        public object Tag = new object();

        bool? _isChecked = false;
        public TreeViewModel Parent;

        #endregion // Data

        #region CreateFoos

        public static void AddNode(string name)
        {

        }

        //public static List<TreeViewModel> CreateFoos()
        //{
        //    //TreeViewModel root = new TreeViewModel("Drawing Objects")
        //    //{
        //    //    IsInitiallySelected = true,
        //    //    Children =
        //    //    {
        //    //        new TreeViewModel("Lines")
        //    //        {
        //    //            Children={
        //    //                new TreeViewModel("l1"),
        //    //                new TreeViewModel("687"),
        //    //            },
        //    //        },
        //    //        new TreeViewModel("Circles")
        //    //        {
        //    //            Children =
        //    //            {
        //    //                new TreeViewModel("A blue x:10 y:2"),
        //    //                new TreeViewModel("0000"),
        //    //                new TreeViewModel("asd"),                            
        //    //            }
        //    //        },
        //    //        new TreeViewModel("Geometries")
        //    //        {
        //    //            Children =
        //    //            {
        //    //                new TreeViewModel("ghfh"),
        //    //                new TreeViewModel("86754"),
        //    //                new TreeViewModel("234ghf"),
        //    //            }
        //    //        },
        //    //    }
        //    //};

        //    root.Initialize();
        //    return new List<TreeViewModel> { root };
        //}

        public TreeViewModel(string name)
        {
            this.Name = name;
            this.Children = new List<TreeViewModel>();
        }

        public void Initialize()
        {
            foreach (TreeViewModel child in this.Children)
            {
                child.Parent = this;
                child.Initialize();
            }
        }

        #endregion // CreateFoos

        #region Properties

        public List<TreeViewModel> Children { get; private set; }

        public bool IsInitiallySelected { get; private set; }

        public string Name { get; private set; }

        #region IsChecked

        /// <summary>
        /// Gets/sets the state of the associated UI toggle (ex. CheckBox).
        /// The return value is calculated based on the check state of all
        /// child FooViewModels.  Setting this property to true or false
        /// will set all children to the same check state, and setting it 
        /// to any value will cause the parent to verify its check state.
        /// </summary>
        public bool? IsChecked
        {
            get { return _isChecked; }
            set { this.SetIsChecked(value, true, true); }
        }

        public void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked)
                return;

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue)
                this.Children.ForEach(c => c.SetIsChecked(_isChecked, true, false));

            if (updateParent && Parent != null)
                Parent.VerifyCheckState();

            this.OnPropertyChanged("IsChecked");
        }

        void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < this.Children.Count; ++i)
            {
                bool? current = this.Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            this.SetIsChecked(state, false, true);
        }

        #endregion // IsChecked

        #endregion // Properties

        public static TreeViewModel GetItemByName(string name, TreeViewModel mainRoot)
        {
            int i = 0;
            if (mainRoot == null) return mainRoot;
            if (mainRoot.Name == name)
                return mainRoot;
            for (; i < mainRoot.Children.Count; ++i)
            {
                if (mainRoot.Children[i] == null)
                    continue;
                else
                {
                    TreeViewModel t = GetItemByName(name, mainRoot.Children[i]);
                    if (t != null)
                        return t;
                }

            }
            return null;

        }

        #region INotifyPropertyChanged Members

        void OnPropertyChanged(string prop)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
