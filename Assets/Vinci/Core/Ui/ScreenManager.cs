using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using Vinci.Core.Utils;

public class ScreenManager : PersistentSingleton<ScreenManager>
{
    [SerializeField]
    private Screen _startingScreen;
    [SerializeField]
    private Screen[] _screens;

    private Screen _currentScreen;

    private readonly Stack<Screen> _history = new Stack<Screen>();

    private void Start()
    {
        for (int i = 0; i < instance._screens.Length; i++)
        {
            _screens[i].Initialize();
            _screens[i].Hide();
        } 

        if(instance._startingScreen != null)
        {
            Show(_startingScreen, true);
        }
    }

    public static T GetScreen<T>() where T : Screen
    {
        for(int i = 0; 1 < instance._screens.Length; i++)
        {
            if(instance._screens[i] is T tScreen)
            {
                return tScreen;
            }
        }

        return null;
    }

    public static T Show<T>(bool remeber = true) where T : Screen
    {
        for (int i = 0; i < instance._screens.Length; i++)
        {
            if (instance._screens[i] is T)
            {
                if(instance._currentScreen != null)
                {
                    if(remeber)
                    {
                        instance._history.Push(instance._currentScreen);
                    }

                    instance._currentScreen.Hide();
                }

                instance._screens[i].Show();
                instance._currentScreen = instance._screens[i];
            }
        }

        return null;
    }

    public static void Show(Screen screen, bool remeber = true)
    {
        if(instance._currentScreen != null)
        {
            if(remeber)
            {
                instance._history.Push(instance._currentScreen);
            }
            instance._currentScreen.Hide();
        }
        screen.Show();

        instance._currentScreen = screen;
    }

    public static void ShowLast()
    {
        if(instance._history.Count != 0)
        {
            Show(instance._history.Pop());
        }
    }

}