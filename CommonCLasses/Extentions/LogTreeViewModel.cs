using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace MRL.SSL.CommonClasses
{
    public class LogTreeViewModel : INotifyPropertyChanged
    {
        #region Data

        public object Tag = new object();

        bool? _isChecked = false;
        public LogTreeViewModel Parent;

        #endregion // Data

        #region CreateFoos

        public static void AddNode(string name)
        {

        }

        public LogTreeViewModel(string name)
        {
            this.Name = name;
            this.Children = new List<LogTreeViewModel>();
        }

        public void Initialize()
        {
            foreach (LogTreeViewModel child in this.Children)
            {
                child.Parent = this;
                child.Initialize();
            }
        }

        #endregion // CreateFoos

        #region Properties

        public List<LogTreeViewModel> Children { get; private set; }

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

        public static LogTreeViewModel GetItemByName(string name, LogTreeViewModel mainRoot)
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
                    LogTreeViewModel t = GetItemByName(name, mainRoot.Children[i]);
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
