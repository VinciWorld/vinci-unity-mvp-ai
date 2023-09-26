using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vinci.Core.UI;
using Vinci.Core.Utils;

namespace Vinci.Core.UI
{
    public class ViewManager : Singleton<ViewManager>
    {
        [SerializeField]
        private View _startingView;
        [SerializeField]
        private View[] _Views;

        private View _currentView;

        private readonly Stack<View> _history = new Stack<View>();

        protected override void Awake()
        {
            base.Awake();

            for (int i = 0; i < instance._Views.Length; i++)
            {
                _Views[i].Initialize();
                _Views[i].Hide();
            }

            if (instance._startingView != null)
            {
                Show(_startingView, true);
            }
        }

        public static T GetView<T>() where T : View
        {
            for (int i = 0; i < instance._Views.Length; i++)
            {
                if (instance._Views[i] is T tView)
                {
                    return tView;
                }
            }

            return null;
        }

        public static T Show<T>(bool remeber = true) where T : View
        {
            for (int i = 0; i < instance._Views.Length; i++)
            {
                if (instance._Views[i] is T)
                {
                    if (instance._currentView != null)
                    {
                        if (remeber)
                        {
                            instance._history.Push(instance._currentView);
                        }

                        instance._currentView.Hide();
                    }

                    instance._Views[i].Show();
                    instance._currentView = instance._Views[i];
                }
            }

            return null;
        }

        public static void Show(View View, bool remeber = true)
        {
            if (instance._currentView != null)
            {
                if (remeber)
                {
                    instance._history.Push(instance._currentView);
                }
                instance._currentView.Hide();
            }
            View.Show();

            instance._currentView = View;
        }

        public static void ShowLast()
        {
            if (instance._history.Count != 0)
            {
                Show(instance._history.Pop());
            }
        }

    }
}

