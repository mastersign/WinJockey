using System.ComponentModel;
using System.Reflection;

namespace Mastersign.WinJockey;

sealed class PropertyObserver<TClass, TProperty> : IDisposable
    where TClass : class, INotifyPropertyChanged
    where TProperty : class
{
    private TClass target;
    private readonly PropertyInfo property;
    private readonly Action<TProperty> bindAction;
    private readonly Action<TProperty> unbindAction;
    private TProperty lastValue;

    public PropertyObserver(TClass target, string propertyName, Action<TProperty> bindAction, Action<TProperty> unbindAction)
    {
        this.target = target ?? throw new ArgumentNullException(nameof(propertyName));
        this.bindAction = bindAction;
        this.unbindAction = unbindAction;
        property = typeof(TClass).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        if (!property.PropertyType.IsAssignableTo(typeof(TProperty)))
        {
            throw new ArgumentException("The type of the property is not compatible with the second type argument.");
        }
        lastValue = CurrentValue;
        this.target.PropertyChanged += PropertyChangedHandler;
        if (lastValue != null) bindAction?.Invoke(lastValue);
    }

    TProperty CurrentValue => (TProperty)property.GetValue(target);

    private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
    {
        if (target == null) return;
        if (e.PropertyName != property.Name) return;
        var currentValue = CurrentValue;
        if (lastValue == currentValue) return;
        if (lastValue != null)
        {
            unbindAction?.Invoke(lastValue);
        }
        lastValue = currentValue;
        if (lastValue != null)
        {
            bindAction?.Invoke(lastValue);
        }
    }

    public void Dispose()
    {
        target.PropertyChanged -= PropertyChangedHandler;
        target = null;
    }
}
