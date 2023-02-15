using System;
using System.Reflection;
using System.Windows.Input;
using UnityEngine.Events;

/// <summary>
/// ICommand implementation using a UnityEvent
/// </summary>
[Serializable]
public class NoesisEventCommand : UnityEvent<object>, ICommand
{
    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
        int count = GetPersistentEventCount();
        if (count > 0)
        {
            object target = GetPersistentTarget(0);
            string name = GetPersistentMethodName(0);

            MethodInfo canExecute = target.GetType().GetMethod("CanExecute" + name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            if (canExecute != null && canExecute.ReturnType == typeof(bool))
            {
                _canExecuteParam[0] = parameter;
                return (bool)canExecute.Invoke(target, _canExecuteParam);
            }
        }

        return true;
    }

    public void Execute(object parameter)
    {
        Invoke(parameter);
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    private object[] _canExecuteParam = { null };
}